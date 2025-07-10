using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.IL.Business
{
	public class ILJobDeclarationSynchroniser : JobDeclarationSynchroniser
	{
		public ILJobDeclarationSynchroniser(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		public new JobDeclaration Destination => (JobDeclaration)base.Destination;

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			Synchronisers.Add(new FieldSynchroniser(Destination.JE_MasterBillIssuedDateInfo, Source.JS_HouseBillIssueDateInfo));
		}

		protected override void HookConsolToDeclarationSynchronisers()
		{
			base.HookConsolToDeclarationSynchronisers();

			if (Destination.IsImport)
			{
				HookForwardingShipmentNumbersUpdates();
				HookConsolNumbersUpdates();
				HookConsolTransportsUpdates();
				HookConsolUpdates();
				HookDeclarationUpdates();
				HookDeclarationConsolsUpdates();
			}

			SyncCustomEntryNumber(GetShipmentNumbers(), FDN);
			SyncCustomEntryNumber(GetConsolNumbers(), PDN);
			SyncManifestNumber();
		}

		protected override void UnHookConsolToDeclarationSynchronisers()
		{
			base.UnHookConsolToDeclarationSynchronisers();

			UnhookForwarderDealNumbers();
			UnhookConsolNumbers();
			UnhookConsolTransports();
			UnhookConsolUpdated();
			UnhookDeclarationUpdates();
			UnhookDeclarationConsols();

			SyncCustomEntryNumber(GetShipmentNumbers(), FDN);
			SyncCustomEntryNumber(GetConsolNumbers(), PDN);
			SyncManifestNumber();
		}

		#region Forwarding Shipment Numbers

		void HookForwardingShipmentNumbersUpdates()
		{
			Source.Numbers.CountChanged += HandleForwardingShipmentNumbersCountChanged;

			foreach (var cusEntry in Source.Numbers.Cast<CusEntryNumber>())
			{
				HookHandleForwardingShipmentEntryNumber(cusEntry);
			}
		}

		void UnhookForwarderDealNumbers()
		{
			Source.Numbers.CountChanged -= HandleForwardingShipmentNumbersCountChanged;

			foreach (var cusEntry in Source.Numbers.Cast<CusEntryNumber>())
			{
				UnhookHandleForwardingShipmentEntryNumber(cusEntry);
			}
		}

		void HandleForwardingShipmentNumbersCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			SyncCustomEntryNumber(GetShipmentNumbers(), FDN);

			if (e.ItemAdded)
			{
				HookHandleForwardingShipmentEntryNumber((CusEntryNumber)e.BizObject);
			}
			else if (e.ItemRemoved) {
				UnhookHandleForwardingShipmentEntryNumber((CusEntryNumber)e.BizObject);
			}
		}

		void HookHandleForwardingShipmentEntryNumber(CusEntryNumber cusEntryNumber)
		{
			cusEntryNumber.PropertyValueChanged += HandleForwardingShipmentEntryNumber;
		}

		void UnhookHandleForwardingShipmentEntryNumber(CusEntryNumber cusEntryNumber)
		{
			cusEntryNumber.PropertyValueChanged -= HandleForwardingShipmentEntryNumber;
		}

		void HandleForwardingShipmentEntryNumber(object sender, ZPropertyValueChangedEventArgs e)
		{
			if(e.Property.Name == CusEntryNumber.Schema.CE_EntryType && ((ZString)e.OldValue == FDN || (ZString)e.Property.Value == FDN))
			{
				SyncCustomEntryNumber(GetShipmentNumbers(), FDN);
			}

			if (e.Property.Name == CusEntryNumber.Schema.CE_EntryNum && ((CusEntryNumber)sender).CE_EntryType == FDN)
			{
				SyncCustomEntryNumber(GetShipmentNumbers(), FDN);
			}
		}

		#endregion

		#region Consol Numbers

		void HookConsolNumbersUpdates()
		{
			var relevantConsol = Destination.RelevantConsol;
			if (relevantConsol == null)
			{
				return;
			}

			relevantConsol.Numbers.CountChanged += HandleForwardingShipmentNumbersCountChanged;

			foreach (var cusEntry in relevantConsol.Numbers.Cast<CusEntryNumber>())
			{
				HookHandleForwardingShipmentEntryNumber(cusEntry);
			}
		}

		void UnhookConsolNumbers()
		{
			var relevantConsol = Destination.RelevantConsol;
			if (relevantConsol == null)
			{
				return;
			}

			relevantConsol.Numbers.CountChanged -= HandleConsolNumbersCountChanged;

			foreach (var cusEntry in relevantConsol.Numbers.Cast<CusEntryNumber>())
			{
				UnhookConsolShipmentEntryNumber(cusEntry);
			}
		}

		void HandleConsolNumbersCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			SyncCustomEntryNumber(GetConsolNumbers(), PDN);

			if (e.ItemAdded)
			{
				HookConsolShipmentEntryNumber((CusEntryNumber)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				UnhookConsolShipmentEntryNumber((CusEntryNumber)e.BizObject);
			}
		}

		void HookConsolShipmentEntryNumber(CusEntryNumber cusEntryNumber)
		{
			cusEntryNumber.PropertyValueChanged += HandleConsolEntryNumber;
		}

		void UnhookConsolShipmentEntryNumber(CusEntryNumber cusEntryNumber)
		{
			cusEntryNumber.PropertyValueChanged -= HandleConsolEntryNumber;
		}

		void HandleConsolEntryNumber(object sender, ZPropertyValueChangedEventArgs e)
		{
			if (e.Property.Name == CusEntryNumber.Schema.CE_EntryType && ((ZString)e.OldValue == PDN || (ZString)e.Property.Value == PDN))
			{
				SyncCustomEntryNumber(GetConsolNumbers(), PDN);
			}

			if (e.Property.Name == CusEntryNumber.Schema.CE_EntryNum && ((CusEntryNumber)sender).CE_EntryType == PDN)
			{
				SyncCustomEntryNumber(GetConsolNumbers(), PDN);
			}
		}

		#endregion

		#region Consol Transports

		void HookConsolTransportsUpdates()
		{
			var relevantConsol = Destination.RelevantConsol;
			if (relevantConsol == null)
			{
				return;
			}

			relevantConsol.Transports.CountChanged += HandleConsolTransportsCountChanged;
		}

		void UnhookConsolTransports()
		{
			var relevantConsol = Destination.RelevantConsol;
			if (relevantConsol == null)
			{
				return;
			}

			relevantConsol.Transports.CountChanged -= HandleConsolTransportsCountChanged;
		}

		void HandleConsolTransportsCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			SyncManifestNumber();

			if (e.ItemAdded)
			{
				HookConsolTransport((Transport)e.BizObject);
			}
			else if (e.ItemRemoved)
			{
				UnhookConsolTransport((Transport)e.BizObject);
			}
		}

		void HookConsolTransport(Transport transport)
		{
			transport.PropertyValueChanged += HandleConsolTransport;
		}

		void UnhookConsolTransport(Transport transport)
		{
			transport.PropertyValueChanged -= HandleConsolTransport;
		}

		void HandleConsolTransport(object sender, ZPropertyValueChangedEventArgs e)
		{
			if (e.Property.Name == Transport.Schema.JW_ArrivalPortRouteId || e.Property.Name == Transport.Schema.JW_RL_NKDiscPortForBinding)
			{
				SyncManifestNumber();
			}
		}

		#endregion

		#region Consol Updated

		void HookConsolUpdates()
		{
			var relevantConsol = Destination.RelevantConsol;
			if (relevantConsol == null)
			{
				return;
			}

			relevantConsol.OnUpdatedByDataRefreshForCustomsSynchronisation += HandleConsolReloaded;
		}

		void UnhookConsolUpdated()
		{
			var relevantConsol = Destination.RelevantConsol;
			if (relevantConsol == null)
			{
				return;
			}

			relevantConsol.OnUpdatedByDataRefreshForCustomsSynchronisation -= HandleConsolReloaded;
		}

		void HandleConsolReloaded(object sender, EventArgs e)
		{
			SyncManifestNumber();
			SyncCustomEntryNumber(GetConsolNumbers(), PDN, true);
		}

		#endregion Consol Updated

		#region Declaration Discharge Port

		void HookDeclarationUpdates()
		{
			Destination.JE_CustomsDischargePortInfo.ValueChanged += HandleDeclarationUpdated;
			Destination.JE_MessageTypeInfo.ValueChanged += HandleDeclarationUpdated;
		}

		void UnhookDeclarationUpdates()
		{
			Destination.JE_CustomsDischargePortInfo.ValueChanged -= HandleDeclarationUpdated;
			Destination.JE_MessageTypeInfo.ValueChanged -= HandleDeclarationUpdated;
		}

		void HandleDeclarationUpdated(object sender, EventArgs e)
		{
			SyncManifestNumber();
			SyncCustomEntryNumber(GetConsolNumbers(), PDN);
			SyncCustomEntryNumber(GetShipmentNumbers(), FDN);
		}

		#endregion Declaration Discharge Port

		#region Declaration Consols

		void HookDeclarationConsolsUpdates()
		{
			if (Destination.Shipment is not null)
			{
				Destination.Shipment.Consols.CountChanged += HandleDeclarationConsolsNumberChanged;
				SyncCustomEntryNumber(GetConsolNumbers(), PDN);
			}
		}

		void UnhookDeclarationConsols()
		{
			if (Destination.Shipment is not null)
			{
				Destination.Shipment.Consols.CountChanged -= HandleDeclarationConsolsNumberChanged;
			}
		}

		void HandleDeclarationConsolsNumberChanged(object sender, CollectionCountChangedEventArgs e)
		{
			SyncManifestNumber();
			SyncCustomEntryNumber(GetConsolNumbers(), PDN);
		}

		#endregion Declaration Consols

		void SyncCustomEntryNumber(CusEntryNumAdditionalReferenceCollection numbers, string entryType, bool reload = false) // reload argument is added due to a but when custom numbers were updated in consol but remain with old value in declaration
		{
			var sourceNumber = numbers?.Cast<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == entryType);

			if (sourceNumber is null)
			{
				DeleteDestinationNumber(entryType);
			}
			else
			{
				if (reload)
				{
					sourceNumber.Reload();
				}

				WriteDestinationNumber(sourceNumber);
			}
		}

		void WriteDestinationNumber(CusEntryNumber sourceNumber)
		{
			var destNumber = Destination.AdditionalReferenceNumbers.Cast<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == sourceNumber.CE_EntryType);
			if (destNumber is null)
			{
				destNumber = Destination.AdditionalReferenceNumbers.AddNew();
				destNumber.CE_EntryType = sourceNumber.CE_EntryType;
			}

			destNumber.CE_EntryNum = sourceNumber.CE_EntryNum;
		}

		void DeleteDestinationNumber(ZString entryType)
		{
			var destNumber = Destination.AdditionalReferenceNumbers.Cast<CusEntryNumber>().FirstOrDefault(n => n.CE_EntryType == entryType);
			if (destNumber is not null)
			{
				Destination.AdditionalReferenceNumbers.RemoveAndDelete(destNumber);
			}
		}

		void SyncManifestNumber()
		{
			var relevantConsol = Destination.RelevantConsol;
			if (relevantConsol == null)
			{
				Destination.JE_ManifestNumber = ZString.Empty;
				return;
			}

			var finalDestinationTransport = relevantConsol.Transports.Cast<Transport>().FirstOrDefault(x => x.JW_RL_NKDiscPortForBinding == relevantConsol.JK_RL_NKDischargePort);
			if (!Destination.IsImport || finalDestinationTransport is null)
			{
				Destination.JE_ManifestNumber = ZString.Empty;
				return;
			}

			Destination.JE_ManifestNumber = finalDestinationTransport.JW_ArrivalPortRouteId;
		}

		CusEntryNumAdditionalReferenceCollection GetConsolNumbers() => Destination.IsImport ? Destination.RelevantConsol?.Numbers : null;

		CusEntryNumAdditionalReferenceCollection GetShipmentNumbers() => Destination.IsImport ? Source.Numbers : null;

		const string PDN = IsraelConsolAdditionalReferenceNumberTypes.Codes.ParentDealNumber;
		const string FDN = IsraelShipmentAdditionalReferenceNumberTypes.Codes.ForwarderDealNumber;
	}
}
