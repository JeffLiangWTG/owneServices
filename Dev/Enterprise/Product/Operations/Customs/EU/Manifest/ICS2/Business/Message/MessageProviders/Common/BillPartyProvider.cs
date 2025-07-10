using System.Collections.Generic;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class BillPartyProvider : IParty
	{
		BillPartyProvider(AsycudaBill bill, AsycudaBillAddress.AddressType addressType)
		{
			this.bill = bill;
			this.addressType = addressType;
		}

		readonly AsycudaBill bill;
		readonly AsycudaBillAddress.AddressType addressType;

		public static BillPartyProvider NewOrNull(AsycudaBill bill, AsycudaBillAddress.AddressType addressType)
		{
			if (bill is null)
			{
				return null;
			}

			if (!IsAnyFieldSetForPartyType(bill, addressType))
			{
				return null;
			}

			return new BillPartyProvider(bill, addressType);
		}

		public string Name
		{
			get
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						return bill.ABL_ConsigneeName.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Shipper:
						return bill.ABL_ShipperName.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.NotifyParty:
						return bill.ABL_NotifyPartyName.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Buyer:
						return bill.ABL_BuyerName.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Seller:
						return bill.ABL_SellerName.GetNullIfEmpty();
					default:
						return null;
				}
			}
		}

		public string IdentificationNumber
		{
			get
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						return bill.ABL_ConsigneeRegNo.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Shipper:
						return bill.ABL_ShipperRegNo.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.NotifyParty:
						return bill.ABL_NotifyPartyRegNo.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Buyer:
						return bill.ABL_BuyerRegNo.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Seller:
						return bill.ABL_SellerRegNo.GetNullIfEmpty();
					default:
						return null;
				}
			}
		}

		public string TypeOfPerson
		{
			get
			{
				switch (addressType)
				{
					case AsycudaBillAddress.AddressType.Consignee:
						return bill.ConsigneePersonType.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Shipper:
						return bill.ShipperPersonType.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.NotifyParty:
						return bill.NotifyPartyPersonType.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Buyer:
						return bill.BuyerPersonType.GetNullIfEmpty();
					case AsycudaBillAddress.AddressType.Seller:
						return bill.SellerPersonType.GetNullIfEmpty();
					default:
						return null;
				}
			}
		}
		public string Status => null;

		public IAddress Address => CachedValueHelper.GetValue(ref address, () => BillAddressProvider.NewOrNull(bill, addressType));
		CachedValue<IAddress> address;

		public IReadOnlyCollection<IIdentifierTypePair> Communications => communications ?? (communications = GetCommunicationProviderCollection());
		IReadOnlyCollection<IIdentifierTypePair> communications;

		IReadOnlyCollection<IIdentifierTypePair> GetCommunicationProviderCollection()
		{
			return new IIdentifierTypePair[]
			{
				new CommunicationProvider(GetPhoneNumber(), CommunicationType.Codes.TE),
			};
		}

		string GetPhoneNumber()
		{
			switch (addressType)
			{
				case AsycudaBillAddress.AddressType.Consignee:
					return bill.ABL_ConsigneePhone.GetNullIfEmpty();
				case AsycudaBillAddress.AddressType.Shipper:
					return bill.ABL_ShipperPhone.GetNullIfEmpty();
				case AsycudaBillAddress.AddressType.NotifyParty:
					return bill.ABL_NotifyPartyPhone.GetNullIfEmpty();
				case AsycudaBillAddress.AddressType.Buyer:
					return bill.ABL_BuyerPhone.GetNullIfEmpty();
				case AsycudaBillAddress.AddressType.Seller:
					return bill.ABL_SellerPhone.GetNullIfEmpty();
				default:
					return null;
			}
		}

		static bool IsAnyFieldSetForPartyType(AsycudaBill bill, AsycudaBillAddress.AddressType addressType) => addressType switch
		{
			AsycudaBillAddress.AddressType.NotifyParty =>
				!(bill.NotifyParty == null &&
				bill.ABL_NotifyPartyName.IsEmpty &&
				bill.ABL_NotifyPartyRegNo.IsEmpty &&
				bill.NotifyPartyPersonType.IsEmpty &&
				bill.ABL_NotifyPartyPhone.IsEmpty &&
				bill.ABL_NotifyPartyCity.IsEmpty &&
				bill.ABL_RN_NKNotifyPartyCountry.IsEmpty &&
				bill.ABL_NotifyPartyStreet1.IsEmpty &&
				bill.ABL_NotifyPartyPostcode.IsEmpty &&
				bill.ABL_NotifyPartyStreet2.IsEmpty),

			AsycudaBillAddress.AddressType.Consignee =>
				!(bill.Consignee == null &&
				bill.ABL_ConsigneeName.IsEmpty &&
				bill.ABL_ConsigneeRegNo.IsEmpty &&
				bill.ConsigneePersonType.IsEmpty &&
				bill.ABL_ConsigneePhone.IsEmpty &&
				bill.ABL_ConsigneeCity.IsEmpty &&
				bill.ABL_RN_NKConsigneeCountry.IsEmpty &&
				bill.ABL_ConsigneeStreet1.IsEmpty &&
				bill.ABL_ConsigneePostcode.IsEmpty &&
				bill.ABL_ConsigneeStreet2.IsEmpty),

			AsycudaBillAddress.AddressType.Shipper =>
				!(bill.Shipper == null &&
				bill.ABL_ShipperName.IsEmpty &&
				bill.ABL_ShipperRegNo.IsEmpty &&
				bill.ShipperPersonType.IsEmpty &&
				bill.ABL_ShipperPhone.IsEmpty &&
				bill.ABL_ShipperCity.IsEmpty &&
				bill.ABL_RN_NKShipperCountry.IsEmpty &&
				bill.ABL_ShipperStreet1.IsEmpty &&
				bill.ABL_ShipperPostcode.IsEmpty &&
				bill.ABL_ShipperStreet2.IsEmpty),

			AsycudaBillAddress.AddressType.Buyer =>
				!(bill.Buyer == null &&
				bill.ABL_BuyerName.IsEmpty &&
				bill.ABL_BuyerRegNo.IsEmpty &&
				bill.BuyerPersonType.IsEmpty &&
				bill.ABL_BuyerPhone.IsEmpty &&
				bill.ABL_BuyerCity.IsEmpty &&
				bill.ABL_RN_NKBuyerCountry.IsEmpty &&
				bill.ABL_BuyerStreet1.IsEmpty &&
				bill.ABL_BuyerPostcode.IsEmpty &&
				bill.ABL_BuyerStreet2.IsEmpty),

			AsycudaBillAddress.AddressType.Seller =>
				!(bill.Seller == null &&
				bill.ABL_SellerName.IsEmpty &&
				bill.ABL_SellerRegNo.IsEmpty &&
				bill.SellerPersonType.IsEmpty &&
				bill.ABL_SellerPhone.IsEmpty &&
				bill.ABL_SellerCity.IsEmpty &&
				bill.ABL_RN_NKSellerCountry.IsEmpty &&
				bill.ABL_SellerStreet1.IsEmpty &&
				bill.ABL_SellerPostcode.IsEmpty &&
				bill.ABL_SellerStreet2.IsEmpty),

			_ => false
		};
	}
}
