using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class HouseBillSynchroniser : DestinationDeletableBusinessObjectSynchroniser
	{
		public HouseBillSynchroniser(CusSCAHouse destination, CommonShipment source)
			: base(destination, source)
		{
		}

		public new CusSCAHouse Destination
		{
			get { return (CusSCAHouse)base.Destination; }
		}

		public new CommonShipment Source
		{
			get { return (CommonShipment)base.Source; }
		}

		#region Implementation

		protected abstract PivotSynchroniser GetPivotSynchroniser(CusSCAPivot pivot, PackLine packLine);

		protected internal List<PivotSynchroniser> PivotSynchronisers => fPivotSynchronisers ?? (fPivotSynchronisers = new List<PivotSynchroniser>());
		List<PivotSynchroniser> fPivotSynchronisers;

		protected override void HookSynchronisers()
		{
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_HouseBillInfo, Source.JS_HouseBillInfo, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_RL_NK_PortOfOriginInfo, Source.JS_RL_NKOriginInfo, true));
			Synchronisers.Add(new FieldSynchroniser(Destination.CA_IsMasterHouseInfo, GetIsMasterHouse, () => new ZPropertyInfo[] { Source.JS_ShipmentTypeInfo }, true));

			AddColoadMasterShipmentSyncroniser();

			FieldSynchroniser goodsOriginSynchroniser = new FieldSynchroniser(Destination.CA_RN_NKGoodsOriginInfo, Source.JS_RL_NKOriginInfo, true);
			goodsOriginSynchroniser.Format += GoodsOriginSynchroniser_Format;
			Synchronisers.Add(goodsOriginSynchroniser);

			Synchronisers.Add(new FieldSynchroniser(Destination.CA_RL_NK_PortOfDestinationInfo, Source.JS_RL_NKDestinationInfo, true));
			HookCollectionSynchronisers();
		}

		protected internal void ColoadMasterShipment_Changed(object sender, EventArgs e)
		{
			if (!Destination.IsRowDeletedOrNull)
			{
				RemoveColoadMasterShipmentSyncroniser();
				AddColoadMasterShipmentSyncroniser();
				if (!Destination.IsDeleted)
				{
					Destination.CA_MasterHouseBill = (ZString)GetMasterHouse();
				}
			}
		}

		void AddColoadMasterShipmentSyncroniser()
		{
			CommonShipment masterHouse = Source.CoLoadMasterShipment;
			if (masterHouse != null)
			{
				coloadMasterShipmentSyncroniser = new FieldSynchroniser(Destination.CA_MasterHouseBillInfo, GetMasterHouse, () => new ZPropertyInfo[] { masterHouse.JS_HouseBillInfo }, true);
				Synchronisers.Add(coloadMasterShipmentSyncroniser);
			}
		}
		FieldSynchroniser coloadMasterShipmentSyncroniser;

		void RemoveColoadMasterShipmentSyncroniser()
		{
			if (coloadMasterShipmentSyncroniser != null)
			{
				Synchronisers.Remove(coloadMasterShipmentSyncroniser);
				coloadMasterShipmentSyncroniser = null;
			}
		}

		protected IZType GetMasterHouse()
		{
			return GetMasterHouseBillFromShipment(Source);
		}

		internal static ZString GetMasterHouseBillFromShipment(CommonShipment shipment)
		{
			var coLoadMasterShipment = shipment.CoLoadMasterShipment;
			return coLoadMasterShipment != null && coLoadMasterShipment.JS_ShipmentType != Core.Constants.ShipmentTypes.BuyersConsolLead ?
				coLoadMasterShipment.JS_HouseBill : ZString.Empty;
		}

		IZType GetIsMasterHouse()
		{
			return (ZBool)(Source.IsCoLoadMaster || Source.IsBlindCoLoadMaster);
		}

		void HookCollectionSynchronisers()
		{
			Source.OuterPackLines.CountChanged += OuterPackLines_CountChanged;
			Destination.Pivot.CountChanged += Pivot_CountChanged;

			foreach (PackLine packLine in Source.OuterPackLines)
			{
				AddPackline(packLine, forceSynchronise: false);
			}

			Source.JS_PackingModeInfo.ValueChanged += JS_PackingModeInfo_ValueChanged;
			Source.ConsigneeDeliveryAddress.DocAddressChanged += ConsigneeDeliveryAddress_Changed;
			Source.ConsigneeDocumentaryAddress.DocAddressChanged += ConsigneeDocumentaryAddress_Changed;
			Source.ConsignorDocumentaryAddress.DocAddressChanged += ConsignorDocumentaryAddress_Changed;
			Source.NotifyPartyDocumentaryAddress.DocAddressChanged += NotifyPartyDocumentaryAddress_Changed;
			Source.JS_JS_ColoadMasterShipmentInfo.ValueChanged += ColoadMasterShipment_Changed;
		}

		void UnHookCollectionSynchronisers()
		{
			Source.JS_JS_ColoadMasterShipmentInfo.ValueChanged -= ColoadMasterShipment_Changed;
			Source.JS_PackingModeInfo.ValueChanged -= JS_PackingModeInfo_ValueChanged;
			Source.ConsigneeDeliveryAddress.DocAddressChanged -= ConsigneeDeliveryAddress_Changed;
			Source.ConsigneeDocumentaryAddress.DocAddressChanged -= ConsigneeDocumentaryAddress_Changed;
			Source.ConsignorDocumentaryAddress.DocAddressChanged -= ConsignorDocumentaryAddress_Changed;
			Source.NotifyPartyDocumentaryAddress.DocAddressChanged -= NotifyPartyDocumentaryAddress_Changed;
			Source.OuterPackLines.CountChanged -= OuterPackLines_CountChanged;

			foreach (PivotSynchroniser synchroniser in PivotSynchronisers)
			{
				synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
			}
			UnhookLoosePacklines();

			Destination.Pivot.CountChanged -= Pivot_CountChanged;
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			UnHookCollectionSynchronisers();
		}

		protected override void OnSynchronise(SynchroniseEventArgs e)
		{
			if (Destination.IsDeleted || Source.IsDeleted)
			{
				return;
			}

			base.OnSynchronise(e);
			for (int i = PivotSynchronisers.Count - 1; i >= 0; i--)
			{
				PivotSynchronisers[i].Synchronise(e);
			}
		}

		void RemovePackLine(PackLine packLineToRemove, bool synchronisePackline = true)
		{
			if (packLineToRemove != null && !packLineToRemove.IsDeleted)
			{
				PivotSynchroniser synchroniser = PivotSynchroniserFromPackLine(packLineToRemove);
				if (synchroniser != null)
				{
					if (synchroniser.Source == packLineToRemove)
					{
						if (!Destination.IsDeleted && synchronisePackline)
						{
							Destination.Pivot.RemoveAndDelete(synchroniser.Destination);
						}
						PivotSynchronisers.Remove(synchroniser);
						return;
					}
					else
					{
						synchroniser.RemovePackLineWatch(packLineToRemove, synchronisePackline);
					}
				}
				else if (packLineToRemove.IsDeleted || packLineToRemove.IsDeleting)
				{
					PivotSynchronisers.ForEach(sync => sync.RemovePackLineWatch(packLineToRemove, synchronisePackline));
				}
			}
		}

		void UnHookPivotSynchroniser(CusSCAPivot pivot)
		{
			var synchroniser = PivotSynchroniserFromPivot(pivot);
			if (synchroniser != null)
			{
				synchroniser.SetEnabled(false, synchroniser.DetectEnabled);
				PivotSynchronisers.Remove(synchroniser);
				synchroniser.Dispose();
			}
		}

		protected PivotSynchroniser PivotSynchroniserFromPivot(CusSCAPivot pivot)
		{
			PivotSynchroniser result = null;
			foreach (PivotSynchroniser synchroniser in PivotSynchronisers)
			{
				if (synchroniser.Destination == pivot)
				{
					result = synchroniser;
					break;
				}
			}
			return result;
		}

		protected PivotSynchroniser PivotSynchroniserFromPackLine(PackLine packLine)
		{
			PivotSynchroniser result = null;
			foreach (PivotSynchroniser synchroniser in PivotSynchronisers)
			{
				if (synchroniser.Source == packLine)
				{
					result = synchroniser;
					break;
				}
			}
			return result;
		}

		#region Formats

		void GoodsOriginSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.Value is ZString)
			{
				ZString uNLOCO = (ZString)e.Value;
				e.Value = uNLOCO.SubstringSafe(0, 2);
			}
		}

		void ForwarderClientIDSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.Value is ZGuid)
			{
				OrgHeader forwarder = Destination.Factory.Load<OrgHeader>(new ZGuid(e.Value));
				if (forwarder != null)
				{
					e.Value = forwarder.LocalManifestID.SubstringSafe(0, 10);
				}
				else
				{
					e.Value = ZString.Empty;
				}
			}
		}

		void CoLoadMasterShipmentSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.Value is ZGuid)
			{
				CommonShipment parentShipment = Source.CoLoadMasterShipment;
				if (parentShipment != null)
				{
					e.Value = parentShipment.Consignee.LocalManifestID;
				}
				else
				{
					e.Value = SeaCargoSynchroniser.ManifestID(Source);
				}
			}
		}

		#endregion

		#region Watched Collection Events

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI003", Justification = "CV_AssociatedContainer is restricted length for display purposes.  Underlying container number is 20 chars.")]
		public void AddPackline(PackLine newPackLine, bool synchronisePackline = true, bool forceSynchronise = true)
		{
			if (IsEnabled && newPackLine != null && !newPackLine.IsDeleted)
			{
				var container = newPackLine.GetContainer(Destination.Consol);
				var containerNumber = container?.JC_ContainerNum ?? GetContainerNumberFromContainerMode();

				if (container == null && containerNumber.IsEmpty)
				{
					StoreLoosePackline(newPackLine);
				}
				else
				{
					RemoveLoosePackline(newPackLine);

					PivotSynchroniser pivotSynchroniser = null;
					var pivot = FindPivotByContainerNumber(containerNumber);
					if (pivot != null)
					{
						pivotSynchroniser = PivotSynchroniserFromPivot(pivot);
						if (pivotSynchroniser != null)
						{
							pivotSynchroniser.AddPackLineWatch(newPackLine, synchronisePackline);
						}
					}

					if (pivotSynchroniser == null && synchronisePackline)
					{
						if (pivot == null)
						{
							pivot = Destination.Pivot.AddNew();
							pivot.CV_AssociatedContainer = containerNumber;
						}

						pivotSynchroniser = GetPivotSynchroniser(pivot, newPackLine);
						PivotSynchronisers.Add(pivotSynchroniser);
						pivotSynchroniser.DestinationDeleted += PivotSynch_DestinationDeleted;
					}

					if (synchronisePackline && forceSynchronise)
					{
						pivotSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
					}
				}
			}
		}

		void CreatePivotSynchroniserForPivot(CusSCAPivot pivot)
		{
			if (PivotSynchroniserFromPivot(pivot) == null)
			{
				var pivotContainerNum = pivot.Container.CN_ContainerNumber;
				PivotSynchroniser synchroniser = null;

				foreach (PackLine packLine in Source.OuterPackLines)
				{
					var container = packLine.GetContainer(Destination.Consol);
					var containerNumber = container?.JC_ContainerNum ?? GetContainerNumberFromContainerMode();

					if (containerNumber == pivotContainerNum)
					{
						if (synchroniser == null)
						{
							synchroniser = GetPivotSynchroniser(pivot, packLine);
							PivotSynchronisers.Add(synchroniser);
							synchroniser.DestinationDeleted += PivotSynch_DestinationDeleted;
						}
						else
						{
							synchroniser.AddPackLineWatch(packLine, true);
						}
					}
				}

				synchroniser?.SetEnabled(true, false);
			}
		}

		CusSCAPivot FindPivotByContainerNumber(ZString containerNumber)
		{
			var pivot = Destination.Pivot.FromContainerNumber(containerNumber);
			if (pivot == null)
			{
				Destination.Pivot.Reload(false); // check for a new pivot created by the Shipment Sea Cargo on saving
				pivot = Destination.Pivot.FromContainerNumber(containerNumber);
			}
			return pivot;
		}

		ZString GetContainerNumberFromContainerMode()
		{
			switch (Destination.Consol?.JK_ConsolMode ?? ZString.Empty)
			{
				case Core.Constants.ContainerModes.BreakBulk:
					return CusSCAPivot.BreakBulk;
				case Core.Constants.ContainerModes.Bulk:
				case Core.Constants.ContainerModes.Liquid:
					return CusSCAPivot.Bulk;
			}

			return ZString.Empty;
		}

		protected void OuterPackLines_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.BizObject is PackLine packLine)
			{
				var shouldSynchronise = !IsSenderRefreshingByDataRefreshBus(sender);
				if (e.ItemAdded)
				{
					AddPackline(packLine, synchronisePackline: shouldSynchronise);
				}
				else if (e.ItemRemoved)
				{
					RemovePackLine(packLine, synchronisePackline: shouldSynchronise);
				}
			}
		}

		protected void Pivot_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			var isRefreshingByDataRefreshBus = IsSenderRefreshingByDataRefreshBus(sender);
			if (isRefreshingByDataRefreshBus && e.ItemAdded)
			{
				CreatePivotSynchroniserForPivot(e.BizObject as CusSCAPivot);
			}
			else if (!isRefreshingByDataRefreshBus && e.ItemRemoved)
			{
				UnHookPivotSynchroniser(e.BizObject as CusSCAPivot);
			}
		}

		protected void PivotSynch_DestinationDeleted(object sender, EventArgs e)
		{
			if (sender is PivotSynchroniser pivotSynch)
			{
				pivotSynch.DestinationDeleted -= PivotSynch_DestinationDeleted;
				PivotSynchronisers.Remove(pivotSynch);
				pivotSynch.Dispose();
			}
		}

		#endregion

		#endregion

		protected void JS_PackingModeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (Destination.OceanBill != null)
			{
				if (Source.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.BreakBulk)
				{
					CusSCAContainer bBKContainer = Destination.OceanBill.Containers.Find(CusSCAPivot.BreakBulk);
					if (bBKContainer == null)
					{
						bBKContainer = Destination.OceanBill.Containers.AddNew();
						bBKContainer.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
						bBKContainer.CN_ContainerNumber = CusSCAPivot.BreakBulk;
						if (Destination.Pivot.Count == 0)
						{
							Destination.Pivot.AddNew();
						}

						Destination.Pivot[0].CV_CN = bBKContainer.PK;
					}
				}
				else if (Source.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.Bulk
					|| Source.JS_PackingMode == Enterprise.Core.Constants.ContainerModes.Liquid)
				{
					CusSCAContainer bLKContainer = Destination.OceanBill.Containers.Find(CusSCAPivot.Bulk);
					if (bLKContainer == null)
					{
						bLKContainer = Destination.OceanBill.Containers.AddNew();
						bLKContainer.CN_ContainerMode = Enterprise.Core.Constants.ContainerModes.Bulk;
						bLKContainer.CN_ContainerNumber = CusSCAPivot.Bulk;
						if (Destination.Pivot.Count == 0)
						{
							Destination.Pivot.AddNew();
						}

						Destination.Pivot[0].CV_CN = bLKContainer.PK;
					}
				}
			}
		}

		protected void ConsigneeDocumentaryAddress_Changed(object sender, EventArgs e)
		{
			Destination.UpdateConsigneeFromShipment(Source);
		}

		protected void ConsigneeDeliveryAddress_Changed(object sender, EventArgs e)
		{
			Destination.UpdateConsigneeFromShipment(Source);
		}

		protected void ConsignorDocumentaryAddress_Changed(object sender, EventArgs e)
		{
			Destination.UpdateConsignorFromShipment(Source);
		}

		protected void NotifyPartyDocumentaryAddress_Changed(object sender, EventArgs e)
		{
			Destination.UpdateNotifyFromShipment(Source);
		}

		#region LoosePacklines

		void StoreLoosePackline(PackLine loosePackLine)
		{
			if (!LoosePackLines.Any(x => x == loosePackLine))
			{
				loosePackLine.Containers.CountChanged += LoosePackLineContainers_CountChanged;
				loosePackLine.JL_JSInfo.ValueChanged += LoosePackLine_ShipmentChanged;
				LoosePackLines.Add(loosePackLine);
			}
		}

		void RemoveLoosePackline(PackLine loosePackLine)
		{
			if (LoosePackLines.Remove(loosePackLine))
			{
				loosePackLine.Containers.CountChanged -= LoosePackLineContainers_CountChanged;
				loosePackLine.JL_JSInfo.ValueChanged -= LoosePackLine_ShipmentChanged;
			}
		}

		void UnhookLoosePacklines()
		{
			foreach (var loosePackLine in LoosePackLines.ToArray())
			{
				RemoveLoosePackline(loosePackLine);
			}
		}

		void LoosePackLineContainers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!IsSenderRefreshingByDataRefreshBus(sender)
				&& sender is CommonContainerManyToManyCollection packlineContainers)
			{
				var packline = packlineContainers.ParentPackLine;
				RemoveLoosePackline(packline);
				AddPackline(packline);
			}
		}

		void LoosePackLine_ShipmentChanged(object sender, EventArgs e)
		{
			if (sender is PackLine packline && packline.Shipment == null) // deleting
			{
				RemoveLoosePackline(packline);
			}
		}

		protected internal List<PackLine> LoosePackLines => fLoosePackLines ?? (fLoosePackLines = new List<PackLine>());
		List<PackLine> fLoosePackLines;

		#endregion
	}
}
