using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.H7.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public static class Extension
	{
		public static IOrganisation GetShipperOrg(this AsycudaBill bill)
		{
			IOrganisation result;
			var shipperId = bill.Shipper?.GetEuIdentificationNumber() ?? ZString.Empty;

			var address = AddressWrapper.New(bill.ABL_ShipperStreet1 + bill.ABL_ShipperStreet2, bill.ABL_ShipperCity, bill.ABL_RN_NKShipperCountry, bill.ABL_ShipperPostcode);
			result = OrganisationWrapper.New(bill.ABL_ShipperName, shipperId, address, false);

			return result;
		}

		public static IOrganisation GetConsigneeOrg(this AsycudaBill bill)
		{
			IOrganisation result;
			var consignorId = bill.ABL_ConsigneeRegNoType + bill.ABL_ConsigneeRegNo;

			var address = AddressWrapper.New(bill.ABL_ConsigneeStreet1 + bill.ABL_ConsigneeStreet2, bill.ABL_ConsigneeCity, bill.ABL_RN_NKConsigneeCountry, bill.ABL_ConsigneePostcode);
			result = OrganisationWrapper.New(bill.ABL_ConsigneeName, consignorId, address, false);

			return result;
		}

		public static IOrganisation GetImporterOrg(this AsycudaBill bill)
		{
			IOrganisation result;

			var outputPreference = EoriNameAndAddressOrBoth.NAMEANDADDRESS;
			if (RegNoHasValidPrefix(bill.ABL_ConsigneeRegNo))
			{
				outputPreference = EoriNameAndAddressOrBoth.EORI;
			}

			var address = AddressWrapper.New(bill.ABL_ConsigneeStreet1 + " " + bill.ABL_ConsigneeStreet2, bill.ABL_ConsigneeCity, bill.ABL_RN_NKConsigneeCountry, bill.ABL_ConsigneePostcode);
			result = OrganisationWrapper.New(bill.ABL_ConsigneeName, bill.ABL_ConsigneeRegNo, address, false, outputPreference);

			return result;
		}

		static bool RegNoHasValidPrefix(ZString regNo)
		{
			var result = false;
			if (!string.IsNullOrEmpty(regNo))
			{
				result = regNo.StartsWith(CountryCodes.UnitedKingdom)
					|| regNo.StartsWith(CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes);
			}

			return result;
		}
	}
}
