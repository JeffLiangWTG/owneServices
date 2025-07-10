using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal partial class AssociatedPostalStructuredAddressWrapper : IPostalStructuredAddress
	{
		internal AssociatedPostalStructuredAddressWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IPostalStructuredAddress.PostcodeCode => bill.ABL_NotifyPartyPostcode;

		string IPostalStructuredAddress.StreetName => string.Concat(bill.ABL_NotifyPartyStreet1, ARAWBMessageConstants.StreetsSeparator, bill.ABL_NotifyPartyStreet2);

		string IPostalStructuredAddress.CityName => bill.ABL_NotifyPartyCity;

		string IPostalStructuredAddress.CountryID => bill.ABL_RN_NKNotifyPartyCountry;

		string IPostalStructuredAddress.CountryName => bill.NotifyPartyCountry?.Description ?? ZString.Empty;

		string IPostalStructuredAddress.CityID => ZString.Empty;

		string IPostalStructuredAddress.PostOfficeBox => bill.NotifyParty?.Header?.OH_RL_NKClosestPort ?? ZString.Empty;
	}
}
