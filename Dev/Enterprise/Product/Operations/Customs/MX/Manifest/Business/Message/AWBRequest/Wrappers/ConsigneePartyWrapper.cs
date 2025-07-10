using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class ConsigneePartyWrapper : IParty
	{
		internal ConsigneePartyWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IParty.PrimaryID => bill.Consignee?.Header?.OH_Code ?? ZString.Empty;

		string IParty.AdditionalID => bill.ABL_ConsigneeRegNo;

		string IParty.Name => bill.ABL_ConsigneeName;

		string IParty.AccountID => bill.Consignee?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Mexico && c.OK_CodeType == OrgCusCode.CodeTypes.AccountsPayableSuppliersReference)?.OK_CustomsRegNo ?? ZString.Empty;

		IPostalStructuredAddress IParty.PostalStructuredAddress => postalStructuredAddress ?? (postalStructuredAddress = new ConsigneePostalStructuredAddressWrapper(bill));
		IPostalStructuredAddress postalStructuredAddress;

		IReadOnlyCollection<ITradeContact> IParty.DefinedTradeContact => new ITradeContact[] { new ConsigneeTradeContactWrapper(bill) };
	}

	internal class ConsigneePostalStructuredAddressWrapper : IPostalStructuredAddress
	{
		internal ConsigneePostalStructuredAddressWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IPostalStructuredAddress.PostcodeCode => bill.ABL_ConsigneePostcode;

		string IPostalStructuredAddress.StreetName => string.Concat(bill.ABL_ConsigneeStreet1, AWBRequestConstants.StreetsSeparator, bill.ABL_ConsigneeStreet2);

		string IPostalStructuredAddress.CityName => bill.ABL_ConsigneeCity;

		string IPostalStructuredAddress.CountryID => bill.ABL_RN_NKConsigneeCountry;

		string IPostalStructuredAddress.CountryName => bill.ConsigneeCountry?.Description ?? ZString.Empty;

		string IPostalStructuredAddress.CityID => bill.Consignee?.Header?.OH_RL_NKClosestPort ?? ZString.Empty;

		string IPostalStructuredAddress.PostOfficeBox => ZString.Empty;
	}

	internal class ConsigneeTradeContactWrapper : ITradeContact
	{
		internal ConsigneeTradeContactWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string ITradeContact.PersonName => AWBRequestHelper.GetStaffAssignmentName(bill.Consignee?.Header?.StaffAssignments);

		string ITradeContact.DirectTelephoneCommunicationCompleteNumber => bill.ABL_ConsigneePhone;

		string ITradeContact.FaxCommunicationCompleteNumber => bill.Consignee?.OA_Fax ?? ZString.Empty;

		string ITradeContact.EmailCommunicationID => bill.Consignee?.OA_Email ?? ZString.Empty;
	}
}
