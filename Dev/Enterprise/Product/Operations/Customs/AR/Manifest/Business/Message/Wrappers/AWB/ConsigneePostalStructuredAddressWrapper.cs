using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class ConsigneePostalStructuredAddressWrapper : IPostalStructuredAddress
	{
		internal ConsigneePostalStructuredAddressWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IPostalStructuredAddress.PostcodeCode => bill.ABL_ConsigneePostcode;

		string IPostalStructuredAddress.StreetName => string.Concat(bill.ABL_ConsigneeStreet1, ARAWBMessageConstants.StreetsSeparator, bill.ABL_ConsigneeStreet2);

		string IPostalStructuredAddress.CityName => bill.ABL_ConsigneeCity;

		string IPostalStructuredAddress.CountryID => bill.ABL_RN_NKConsigneeCountry;

		string IPostalStructuredAddress.CountryName => bill.ConsigneeCountry?.Description ?? ZString.Empty;

		string IPostalStructuredAddress.CityID => bill.Consignee?.Header?.OH_RL_NKClosestPort ?? ZString.Empty;

		string IPostalStructuredAddress.PostOfficeBox => ZString.Empty;
	}
}
