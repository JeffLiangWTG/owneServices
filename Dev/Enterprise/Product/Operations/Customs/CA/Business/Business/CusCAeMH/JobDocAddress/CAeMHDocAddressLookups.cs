using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CAeMHDocAddressLookups : JobDocAddressLookups
	{
		public CAeMHDocAddressLookups(CAeMHDocAddress parent)
			: base(parent)
		{ }

		protected new CAeMHDocAddress Parent
		{
			get { return (CAeMHDocAddress)base.Parent; }
		}

		public OrganisationsFindBoxCollection ThirdParties
		{
			get
			{
				switch (Parent.E2_AddressType)
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
						var result = new OrganisationsFindBoxCollection(Factory);
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property5", ZBool.True));
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "Property9", ZBool.True));
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "OrJoinCondition", ZBool.True));
						result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation Types", "AndJoinCondition", ZBool.False));
						return result;
					default:
						return new OrganisationsFindBoxCollection(Factory);
				}
			}
		}
	}
}
