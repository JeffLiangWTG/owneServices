using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Integration.Customs.ASYCUDA;
using AsycudaPackPackedItemPivotCollection = Enterprise.Customs.ManifestBase.AsycudaPackPackedItemPivotCollection;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader,
		ILManifest.IAsycudaManifestHeader,
		IMessageAttachee
	{
		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.DeclarantList))]
		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaManifestHeader.Declarant", Caption = "Declarant")]
		public override ZGuid AMA_OA_Declarant
		{
			get => base.AMA_OA_Declarant;
			set
			{
				var oldValue = AMA_OA_Declarant;
				base.AMA_OA_Declarant = value;
				if (!IsCopying && oldValue != AMA_OA_Declarant)
				{
					AssignForwarderSubDealNumberForBills(false);
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaManifestHeader.Country", Caption = "Country")]
		public override ZString AMA_RN_NKCountry { get => base.AMA_RN_NKCountry; set => base.AMA_RN_NKCountry = value; }

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaManifestHeader.EstimateArrival", Caption = "Estimate Arrival")]
		public override ZDateTime AMA_A_ARV { get => base.AMA_A_ARV; set => base.AMA_A_ARV = value; }

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaManifestHeader.CustomsStatus", Caption = "Customs Status")]
		public override ZString RegistrationNumber { get => base.RegistrationNumber; set => base.RegistrationNumber = value; }

		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaManifestHeader.StatusDate", Caption = "Status Date")]
		public override ZDateTime RegistrationDate { get => base.RegistrationDate; set => base.RegistrationDate = value; }

		public override ZGuid AMA_OA_ShippingAgent
		{
			get => base.AMA_OA_ShippingAgent;
			set
			{
				var oldValue = AMA_OA_ShippingAgent;
				base.AMA_OA_ShippingAgent = value;
				if (!IsCopying && oldValue != AMA_OA_ShippingAgent)
				{
					AssignForwarderSubDealNumberForBills(false);
				}
			}
		}

		public override ZString AMA_MasterBill
		{
			get => base.AMA_MasterBill;
			set
			{
				var oldValue = AMA_MasterBill;
				base.AMA_MasterBill = value;

				if (!IsCopying
					&& oldValue != AMA_MasterBill
					&& !IsRoad)
				{
					foreach (var bill in Bills)
					{
						bill.TransportDocuments.EnsureTransportDocumentType(TransportDocsTypeList.Codes._704, AMA_MasterBill);
					}
				}
			}
		}

		public override ZString AMA_RL_NKPortOfDischarge
		{
			get => base.AMA_RL_NKPortOfDischarge;
			set
			{
				var oldValue = AMA_RL_NKPortOfDischarge;
				base.AMA_RL_NKPortOfDischarge = value;

				if (!IsCopying && oldValue != AMA_RL_NKPortOfDischarge)
				{
					PropagateDischargePortToChildBills();
					UpdateShippingAgentForSea();
				}
			}
		}

		public override ZString AMA_ManifestNumber
		{
			get => base.AMA_ManifestNumber;
			set
			{
				var oldValue = AMA_ManifestNumber;
				base.AMA_ManifestNumber = value;
				if (!IsCopying && oldValue != AMA_ManifestNumber)
				{
					AssignForwarderSubDealNumberForBills(true);
					if (AMA_TransportMode == Core.Constants.TransportModes.Road)
					{
						PopulateIL3TransportDocumentInfo();
					}
				}
			}
		}

		public override ZGuid AMA_OA_Carrier
		{
			get => base.AMA_OA_Carrier;
			set
			{
				var oldValue = AMA_OA_Carrier;
				base.AMA_OA_Carrier = value;

				if (!IsCopying && oldValue != AMA_OA_Carrier)
				{
					UpdateShippingAgentForSea();
				}
			}
		}

		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				var oldValue = AMA_TransportMode;
				base.AMA_TransportMode = value;
				if (!IsCopying && oldValue != AMA_TransportMode)
				{
					TransportMeans.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region AsycudaContainer

		[ChildEditable]
		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;

		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);

		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		#endregion

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;

		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Israel;
		public bool IsAwaitingResponse() => AMA_MessageStatus == MessageStatusCodeList.Codes.Awaiting;

		public AsycudaManifestMessageSendingConfiguration MessageSendingConfiguration => messageSendingConfiguration ??= GetNewMessageSendingConfiguration();
		AsycudaManifestMessageSendingConfiguration messageSendingConfiguration;
		protected virtual AsycudaManifestMessageSendingConfiguration GetNewMessageSendingConfiguration() => new AsycudaManifestMessageSendingConfiguration();

		#region ApplicationBusinessProvider

		protected override ASYCUDA.Business.ApplicationBusinessProvider GetApplicationBusinessProvider()
		{
			if (applicationBusinessProvider == null)
			{
				applicationBusinessProvider = new ApplicationBusinessProvider();
				applicationBusinessProvider.Initialise(Factory);
			}

			return applicationBusinessProvider;
		}

		ApplicationBusinessProvider applicationBusinessProvider;

		#endregion ApplicationBusinessProvider

		protected override ZString GetDataGroupingCore() => Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(AMA_RN_NKCountry);

		public override AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => AsycudaPackPackedItemPivotCollection.RelationshipType.Many;

		ZString IMessageAttachee.MessageStatus { get => AMA_MessageStatus; set => AMA_MessageStatus = value; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			AMA_AgentType = Core.Constants.AgentType.Agent;
			AMA_OA_Declarant = GlbBranch.CurrentBranch.OrgProxy?.MainAddress.PK ?? GlbCompany.CurrentCompany.OrgProxy?.MainAddress.PK ?? ZGuid.Empty;
		}

		protected override ZBool ShowTransportMeansTabCore => IsRoad;

		protected override ASYCUDA.Business.TransportMeanCollection GetNewTransportCollection() => new TransportMeanCollection(this);

		protected override TransportSupporter GetNewTransportSupporter() => new AsycudaManifestHeaderTransportSupporter(this);

		protected override BusinessObjectSynchroniser GetConsolSynchronizerCore(ForwardingConsol source) => new AsycudaManifestHeaderSynchroniser(this, source);

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AMA_ManifestType = ILManifestTypes.Codes._785;
		}
#endif

		protected override ZZDatabaseValidationHelper GetNewZZValidationHelper() => new ILDatabaseValidationHelper(this);

		protected override bool NeedPersonsTabCore => false;

		void AssignForwarderSubDealNumberForBills(bool manifestNumberHasChanged)
		{
			var bills = Bills.Cast<AsycudaBill>();
			if (manifestNumberHasChanged)
			{
				foreach (var transportDocument in bills.SelectMany(d => d.TransportDocuments).Where(td => td.CSI_Code == TransportDocsTypeList.Codes.IL1).ToList())
				{
					transportDocument.Delete();
				}

				if (AMA_ManifestNumber.IsEmpty)
				{
					return;
				}
			}

			foreach (var bill in bills)
			{
				new AsycudaBillForwarderSubDealNumberManager(bill).AssignSubDealNumberToTransportDocument();
			}
		}

		void PropagateDischargePortToChildBills()
		{
			foreach (var asycudaBill in Bills)
			{
				asycudaBill.ABL_RL_NKPortOfDischarge = AMA_RL_NKPortOfDischarge;
			}
		}

		void PopulateIL3TransportDocumentInfo()
		{
			foreach (var asycudaBill in Bills)
			{
				asycudaBill.TransportDocuments.EnsureTransportDocumentType(TransportDocsTypeList.Codes.IL3, AMA_ManifestNumber);
			}
		}

		void UpdateShippingAgentForSea()
		{
			if (!IsSea)
			{
				return;
			}

			if (Carrier == null)
			{
				AMA_OA_ShippingAgent_ZAddress.OrgPK = ZGuid.Empty;
				return;
			}

			var appointedAgentPorts = Carrier.Header.CarrierAppointedAgentPorts_Agency.Cast<OrgCarrierAppointedAgentPorts>();
			AMA_OA_ShippingAgent = GetAgentFor(appointedAgentPorts, AMA_RL_NKPortOfDischarge)
								   ?? GetAgentFor(appointedAgentPorts, Core.Constants.CountryCodes.Israel)
								   ?? ZGuid.Empty;
		}

		ZGuid? GetAgentFor(IEnumerable<OrgCarrierAppointedAgentPorts> appointedAgentPorts, ZString portOrCountry)
		{
			if (portOrCountry.IsEmpty)
			{
				return null;
			}

			var appointedAgentPort = appointedAgentPorts
				.FirstOrDefault(x => x.O5_PortOrCountry == portOrCountry);
			return appointedAgentPort?.O5_OA_AgentOfficeAddress;
		}
	}
}
