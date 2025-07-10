using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.H7.Business
{
	public class BillAddressProvider : IAddress
	{
		public BillAddressProvider(AsycudaBill bill, AsycudaBillAddress.AddressType addressType)
		{
			this.bill = bill;
			this.addressType = addressType;
		}

		readonly AsycudaBill bill;
		readonly AsycudaBillAddress.AddressType addressType;

		public string City => CachedValueHelper.GetValue(ref cityCached, () =>
		{
			switch (addressType)
			{
				case AsycudaBillAddress.AddressType.Consignee:
					return bill.ABL_ConsigneeCity;
				case AsycudaBillAddress.AddressType.Shipper:
					return bill.ABL_ShipperCity;
				default:
					return null;
			}
		});
		CachedValue<string> cityCached;

		public string Country => CachedValueHelper.GetValue(ref countryCached, () =>
		{
			switch (addressType)
			{
				case AsycudaBillAddress.AddressType.Consignee:
					return bill.ABL_RN_NKConsigneeCountry;
				case AsycudaBillAddress.AddressType.Shipper:
					return bill.ABL_RN_NKShipperCountry;
				default:
					return null;
			}
		});
		CachedValue<string> countryCached;

		public string StreetAndNumber => CachedValueHelper.GetValue(ref streetAndNumberCached, () =>
		{
			switch (addressType)
			{
				case AsycudaBillAddress.AddressType.Consignee:
					return string.Format("{0} {1}", bill.ABL_ConsigneeStreet1, bill.ABL_ConsigneeStreet2);
				case AsycudaBillAddress.AddressType.Shipper:
					return string.Format("{0} {1}", bill.ABL_ShipperStreet1, bill.ABL_ShipperStreet2);
				default:
					return null;
			}
		});
		CachedValue<string> streetAndNumberCached;

		public string Postcode => CachedValueHelper.GetValue(ref postcodeCached, () =>
		{
			switch (addressType)
			{
				case AsycudaBillAddress.AddressType.Consignee:
					return bill.ABL_ConsigneePostcode;
				case AsycudaBillAddress.AddressType.Shipper:
					return bill.ABL_ShipperPostcode;
				default:
					return null;
			}
		});
		CachedValue<string> postcodeCached;
	}
}
