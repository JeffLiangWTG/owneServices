using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class ConsigneePartyWrapper : IParty
	{
		internal ConsigneePartyWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IParty.PrimaryID => bill.Consignee?.Header?.OH_Code ?? ZString.Empty;

		string IParty.SchemeAgencyID => bill.ABL_ConsigneeRegNoType;

		string IParty.AdditionalID => bill.ABL_ConsigneeRegNo;

		string IParty.Name => bill.ABL_ConsigneeName;

		string IParty.AccountID => bill.Consignee?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Argentina && c.OK_CodeType == OrgCusCode.CodeTypes.AccountsPayableSuppliersReference)?.OK_CustomsRegNo ?? ZString.Empty;

		IPostalStructuredAddress IParty.PostalStructuredAddress => postalStructuredAddress ?? (postalStructuredAddress = new ConsigneePostalStructuredAddressWrapper(bill));
		IPostalStructuredAddress postalStructuredAddress;

		ITradeContact IParty.DefinedTradeContact => definedTradeContact ?? (definedTradeContact = new ConsigneeTradeContactWrapper(bill));
		ITradeContact definedTradeContact;
	}
}
