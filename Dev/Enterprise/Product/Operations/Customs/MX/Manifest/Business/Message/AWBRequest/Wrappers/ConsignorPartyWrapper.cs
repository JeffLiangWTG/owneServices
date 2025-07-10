using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class ConsignorPartyWrapper : IParty
	{
		internal ConsignorPartyWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IParty.PrimaryID => bill.Shipper?.Header?.OH_Code ?? ZString.Empty;

		string IParty.AdditionalID => bill.ABL_ShipperRegNo;

		string IParty.Name => bill.ABL_ShipperName;

		string IParty.AccountID => bill.Shipper?.Header?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(c => c.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Mexico && c.OK_CodeType == OrgCusCode.CodeTypes.AccountsPayableSuppliersReference)?.OK_CustomsRegNo ?? ZString.Empty;

		IPostalStructuredAddress IParty.PostalStructuredAddress => postalStructuredAddress ?? (postalStructuredAddress = new ConsignorPostalStructuredAddressWrapper(bill));
		IPostalStructuredAddress postalStructuredAddress;

		IReadOnlyCollection<ITradeContact> IParty.DefinedTradeContact => new ITradeContact[] { new ConsignorTradeContactWrapper(bill) };
	}

	internal class ConsignorPostalStructuredAddressWrapper : IPostalStructuredAddress
	{
		internal ConsignorPostalStructuredAddressWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IPostalStructuredAddress.PostcodeCode => bill.ABL_ShipperPostcode;

		string IPostalStructuredAddress.StreetName => string.Concat(bill.ABL_ShipperStreet1, AWBRequestConstants.StreetsSeparator, bill.ABL_ShipperStreet2);

		string IPostalStructuredAddress.CityName => bill.ABL_ShipperCity;

		string IPostalStructuredAddress.CountryID => bill.ABL_RN_NKShipperCountry;

		string IPostalStructuredAddress.CountryName => bill.ShipperCountry?.Description ?? ZString.Empty;

		string IPostalStructuredAddress.CityID => bill.Shipper?.Header?.OH_RL_NKClosestPort ?? ZString.Empty;

		string IPostalStructuredAddress.PostOfficeBox => ZString.Empty;
	}

	internal class ConsignorTradeContactWrapper : ITradeContact
	{
		internal ConsignorTradeContactWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string ITradeContact.PersonName => AWBRequestHelper.GetStaffAssignmentName(bill.Shipper?.Header?.StaffAssignments);

		string ITradeContact.DirectTelephoneCommunicationCompleteNumber => bill.ABL_ShipperPhone;

		string ITradeContact.FaxCommunicationCompleteNumber => bill.Shipper?.OA_Fax ?? ZString.Empty;

		string ITradeContact.EmailCommunicationID => bill.Shipper?.OA_Email ?? ZString.Empty;
	}
}
