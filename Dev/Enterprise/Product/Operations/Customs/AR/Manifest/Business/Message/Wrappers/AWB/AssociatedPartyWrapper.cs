using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class AssociatedPartyWrapper : IParty
	{
		internal AssociatedPartyWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IParty.PrimaryID => bill.NotifyParty?.Header?.OH_Code ?? ZString.Empty;

		string IParty.SchemeAgencyID => bill.ABL_NotifyPartyRegNoType;

		string IParty.AdditionalID => bill.ABL_NotifyPartyRegNo;

		string IParty.Name => bill.ABL_NotifyPartyName;

		string IParty.AccountID => ZString.Empty;

		IPostalStructuredAddress IParty.PostalStructuredAddress => postalStructuredAddress ?? (postalStructuredAddress = new AssociatedPostalStructuredAddressWrapper(bill));
		IPostalStructuredAddress postalStructuredAddress;

		ITradeContact IParty.DefinedTradeContact => definedTradeContact ?? (definedTradeContact = new AssociatedTradeContactWrapper(bill));
		ITradeContact definedTradeContact;
	}
}
