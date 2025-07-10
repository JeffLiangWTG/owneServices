using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class ConsignorPostalStructuredAddressWrapper : IPostalStructuredAddress
	{
		internal ConsignorPostalStructuredAddressWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IPostalStructuredAddress.PostcodeCode => bill.ABL_ShipperPostcode;

		string IPostalStructuredAddress.StreetName => string.Concat(bill.ABL_ShipperStreet1, ARAWBMessageConstants.StreetsSeparator, bill.ABL_ShipperStreet2);

		string IPostalStructuredAddress.CityName => bill.ABL_ShipperCity;

		string IPostalStructuredAddress.CountryID => bill.ABL_RN_NKShipperCountry;

		string IPostalStructuredAddress.CountryName => bill.ShipperCountry?.Description ?? ZString.Empty;

		string IPostalStructuredAddress.CityID => bill.Shipper?.Header?.OH_RL_NKClosestPort ?? ZString.Empty;

		string IPostalStructuredAddress.PostOfficeBox => ZString.Empty;
	}
}
