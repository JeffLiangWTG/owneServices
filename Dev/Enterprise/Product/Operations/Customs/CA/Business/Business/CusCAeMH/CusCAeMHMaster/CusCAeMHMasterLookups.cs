//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusCAeMHMasterLookups
//
//    This class should be used for overriding collections in AutoCusCAeMHMasterLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHMasterLookups : AutoCusCAeMHMasterLookups
	{
		public CusCAeMHMasterLookups(AutoCusCAeMHMaster parent) : base(parent)
		{
		}

		public new CusCAeMHMaster Parent
		{
			get { return (CusCAeMHMaster)base.Parent; }
		}

		public CodeDescriptionPairList TransportModeList
		{
			get { return Factory.GetCachedValue<CusCAeMHTransportModeList>(); }
		}

		public ZZRefCarrierCombinedCollection Carriers => ZZRefCarrierCombinedCollectionExtension.GetCachedCollection(Factory, Parent.BP_ModeOfTransport);

		public ZZRefCusCodeListCombinedCollection DischargeOffices
		{
			get { return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		public CACSubLocationCollection DischargeSubLocations
		{
			get { return new CACSubLocationCollection(Factory); }
		}

		public OrganisationsFindBoxCollection ThirdParties
		{
			get
			{
				switch (CurrentAddressType)
				{
					case DocAddressTypes.Codes.ConsigneeDocumentaryAddress:
					case DocAddressTypes.Codes.ConsigneePickupDeliveryAddress:
						return new ConsigneeCollection(Factory);
					case DocAddressTypes.Codes.ConsignorDocumentaryAddress:
						return new ConsignorCollection(Factory);
					case DocAddressTypes.Codes.ImportBroker:
						return new BrokerCollection(Factory);
					case DocAddressTypes.Codes.ReceivingForwarderAddress:
						return new ForwarderCollection(Factory);
					case DocAddressTypes.Codes.Carrier:
						return new ShippingProviderCollection(Factory);
					case DocAddressTypes.Codes.Warehouse:
						return new WarehouseClientCollection(Factory);
					case DocAddressTypes.Codes.Consolidator:
					case DocAddressTypes.Codes.PlaceOfConsolidation:
						return ForwardersAndServices;
					default:
						return new OrganisationsFindBoxCollection(Factory);
				}
			}
		}
		public ZString CurrentAddressType { private get; set; }

		public RefUNLOCOCollection DischargePorts
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public OrganisationsFindBoxCollection ForwardersAndServices
		{
			get
			{
				if (fForwardersAndServices == null)
				{
					fForwardersAndServices = new OrganisationsFindBoxCollection(Factory);
					fForwardersAndServices.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
					fForwardersAndServices.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
					fForwardersAndServices.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
					fForwardersAndServices.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.False));
				}
				return fForwardersAndServices;
			}
		}
		OrganisationsFindBoxCollection fForwardersAndServices;

		public EManifestForwarderJobStatusList CustomsStatuses
		{
			get { return Factory.GetCachedValue<EManifestForwarderJobStatusList>(); }
		}

		public MessageStatusList MessageStatuses
		{
			get { return Factory.GetCachedValue<MessageStatusList>(); }
		}

		public EManifestAmendmentReasonCodes AmendmentCodes
		{
			get { return Factory.GetCachedValue<EManifestAmendmentReasonCodes>(); }
		}
	}
}
