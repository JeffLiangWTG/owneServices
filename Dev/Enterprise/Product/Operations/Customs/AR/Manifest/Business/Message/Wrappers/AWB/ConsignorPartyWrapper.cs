using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class ConsignorPartyWrapper : IParty
	{
		internal ConsignorPartyWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IParty.PrimaryID => bill.Shipper?.Header?.OH_Code ?? ZString.Empty;

		string IParty.SchemeAgencyID => "1";

		string IParty.AdditionalID => bill.ABL_ShipperRegNo;

		string IParty.Name => bill.ABL_ShipperName;

		string IParty.AccountID => bill.Shipper?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Argentina && c.OK_CodeType == OrgCusCode.CodeTypes.AccountsPayableSuppliersReference)?.OK_CustomsRegNo ?? ZString.Empty;

		IPostalStructuredAddress IParty.PostalStructuredAddress => postalStructuredAddress ?? (postalStructuredAddress = new ConsignorPostalStructuredAddressWrapper(bill));
		IPostalStructuredAddress postalStructuredAddress;

		ITradeContact IParty.DefinedTradeContact => definedTradeContact ?? (definedTradeContact = new ConsignorTradeContactWrapper(bill));
		ITradeContact definedTradeContact;
	}
}
