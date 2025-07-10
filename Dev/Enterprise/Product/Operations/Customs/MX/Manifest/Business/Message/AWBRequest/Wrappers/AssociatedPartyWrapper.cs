using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class AssociatedPartyWrapper : IParty
	{
		internal AssociatedPartyWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IParty.PrimaryID => bill.NotifyParty?.Header?.OH_Code ?? ZString.Empty;

		string IParty.AdditionalID => bill.ABL_NotifyPartyRegNo;

		string IParty.Name => bill.ABL_NotifyPartyName;

		string IParty.AccountID => ZString.Empty;

		IPostalStructuredAddress IParty.PostalStructuredAddress => postalStructuredAddress ?? (postalStructuredAddress = new AssociatedPostalStructuredAddressWrapper(bill));
		IPostalStructuredAddress postalStructuredAddress;

		IReadOnlyCollection<ITradeContact> IParty.DefinedTradeContact => new ITradeContact[] { new AssociatedTradeContactWrapper(bill) };
	}

	internal partial class AssociatedPostalStructuredAddressWrapper : IPostalStructuredAddress
	{
		internal AssociatedPostalStructuredAddressWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IPostalStructuredAddress.PostcodeCode => bill.ABL_NotifyPartyPostcode;

		string IPostalStructuredAddress.StreetName => string.Concat(bill.ABL_NotifyPartyStreet1, AWBRequestConstants.StreetsSeparator, bill.ABL_NotifyPartyStreet2);

		string IPostalStructuredAddress.CityName => bill.ABL_NotifyPartyCity;

		string IPostalStructuredAddress.CountryID => bill.ABL_RN_NKNotifyPartyCountry;

		string IPostalStructuredAddress.CountryName => ZString.Empty;

		string IPostalStructuredAddress.CityID => ZString.Empty;

		string IPostalStructuredAddress.PostOfficeBox => bill.NotifyParty?.Header?.OH_RL_NKClosestPort ?? ZString.Empty;
	}

	internal class AssociatedTradeContactWrapper : ITradeContact
	{
		internal AssociatedTradeContactWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string ITradeContact.PersonName => AWBRequestHelper.GetStaffAssignmentName(bill.NotifyParty?.Header?.StaffAssignments);

		string ITradeContact.DirectTelephoneCommunicationCompleteNumber => bill.ABL_NotifyPartyPhone;

		string ITradeContact.FaxCommunicationCompleteNumber => bill.NotifyParty?.OA_Fax ?? ZString.Empty;

		string ITradeContact.EmailCommunicationID => bill.NotifyParty?.OA_Email ?? ZString.Empty;
	}
}
