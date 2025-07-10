using CargoWise.Customs.EU.MessageContracts.ICS2;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
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

		public static BillAddressProvider NewOrNull(AsycudaBill bill, AsycudaBillAddress.AddressType addressType)
		{
			if (bill is null)
			{
				return null;
			}

			if(!IsAnyFieldSetForAddressType(bill, addressType))
			{
				return null;
			}

			return new BillAddressProvider(bill, addressType);
		}

		public string City
		{
			get
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						return bill.ABL_ConsigneeCity.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Shipper:
						return bill.ABL_ShipperCity.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.NotifyParty:
						return bill.ABL_NotifyPartyCity.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Buyer:
						return bill.ABL_BuyerCity.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Seller:
						return bill.ABL_SellerCity.GetNullIfEmpty();
					default:
						return null;
				}
			}
		}

		public string Country
		{
			get
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						return bill.ABL_RN_NKConsigneeCountry.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Shipper:
						return bill.ABL_RN_NKShipperCountry.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.NotifyParty:
						return bill.ABL_RN_NKNotifyPartyCountry.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Buyer:
						return bill.ABL_RN_NKBuyerCountry.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Seller:
						return bill.ABL_RN_NKSellerCountry.GetNullIfEmpty();
					default:
						return null;
				}
			}
		}

		public string SubDivision => null;

		public string Street
		{
			get
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						return bill.ABL_ConsigneeStreet1.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Shipper:
						return bill.ABL_ShipperStreet1.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.NotifyParty:
						return bill.ABL_NotifyPartyStreet1.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Buyer:
						return bill.ABL_BuyerStreet1.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Seller:
						return bill.ABL_SellerStreet1.GetNullIfEmpty();
					default:
						return null;
				}
			}
		}

		public string PostCode
		{
			get
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						return bill.ABL_ConsigneePostcode.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Shipper:
						return bill.ABL_ShipperPostcode.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.NotifyParty:
						return bill.ABL_NotifyPartyPostcode.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Buyer:
						return bill.ABL_BuyerPostcode.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Seller:
						return bill.ABL_SellerPostcode.GetNullIfEmpty();
					default:
						return null;
				}
			}
		}

		public string StreetAdditionalLine
		{
			get
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						return bill.ABL_ConsigneeStreet2.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Shipper:
						return bill.ABL_ShipperStreet2.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.NotifyParty:
						return bill.ABL_NotifyPartyStreet2.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Buyer:
						return bill.ABL_BuyerStreet2.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Seller:
						return bill.ABL_SellerStreet2.GetNullIfEmpty();
					default:
						return null;
				}
			}
		}

		public string Number => IsAnyFieldSetForAddressType(bill, addressType) ? "0" : null;

		public string PoBox => null;

		static bool IsAnyFieldSetForAddressType(AsycudaBill bill, AsycudaBillAddress.AddressType addressType) => addressType switch
		{
			AsycudaBillAddress.AddressType.NotifyParty =>
				!(bill.ABL_NotifyPartyCity.IsEmpty &&
				  bill.ABL_RN_NKNotifyPartyCountry.IsEmpty &&
				  bill.ABL_NotifyPartyStreet1.IsEmpty &&
				  bill.ABL_NotifyPartyPostcode.IsEmpty &&
				  bill.ABL_NotifyPartyStreet2.IsEmpty),

			AsycudaBillAddress.AddressType.Consignee =>
				!(bill.ABL_ConsigneeCity.IsEmpty &&
				  bill.ABL_RN_NKConsigneeCountry.IsEmpty &&
				  bill.ABL_ConsigneeStreet1.IsEmpty &&
				  bill.ABL_ConsigneePostcode.IsEmpty &&
				  bill.ABL_ConsigneeStreet2.IsEmpty),

			AsycudaBillAddress.AddressType.Shipper =>
				!(bill.ABL_ShipperCity.IsEmpty &&
				  bill.ABL_RN_NKShipperCountry.IsEmpty &&
				  bill.ABL_ShipperStreet1.IsEmpty &&
				  bill.ABL_ShipperPostcode.IsEmpty &&
				  bill.ABL_ShipperStreet2.IsEmpty),

			AsycudaBillAddress.AddressType.Buyer =>
				!(bill.ABL_BuyerCity.IsEmpty &&
				  bill.ABL_RN_NKBuyerCountry.IsEmpty &&
				  bill.ABL_BuyerStreet1.IsEmpty &&
				  bill.ABL_BuyerPostcode.IsEmpty &&
				  bill.ABL_BuyerStreet2.IsEmpty),

			AsycudaBillAddress.AddressType.Seller =>
				!(bill.ABL_SellerCity.IsEmpty &&
				  bill.ABL_RN_NKSellerCountry.IsEmpty &&
				  bill.ABL_SellerStreet1.IsEmpty &&
				  bill.ABL_SellerPostcode.IsEmpty &&
				  bill.ABL_SellerStreet2.IsEmpty),

			_ => false
		};
	}
}
