using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class FreightForwarderPartyWrapper : IParty
	{
		internal FreightForwarderPartyWrapper(GlbCompany company)
		{
			this.company = Argument.NotNull(company, "company cannot be null");
		}
		readonly GlbCompany company;

		string IParty.PrimaryID => company.GC_CustomsRegistrationNo;

		string IParty.AdditionalID => ZString.Empty;

		string IParty.Name => company.GC_Name;

		string IParty.AccountID => ZString.Empty;

		IPostalStructuredAddress IParty.PostalStructuredAddress => postalStructuredAddress ?? (postalStructuredAddress = new FreightForwarderPostalStructuredAddressWrapper(company));
		IPostalStructuredAddress postalStructuredAddress;

		IReadOnlyCollection<ITradeContact> IParty.DefinedTradeContact => new ITradeContact[] { new FreightForwarderTradeContactWrapper(company) };
	}

	internal class FreightForwarderPostalStructuredAddressWrapper : IPostalStructuredAddress
	{
		internal FreightForwarderPostalStructuredAddressWrapper(GlbCompany company)
		{
			this.company = Argument.NotNull(company, "company cannot be null");
		}
		readonly GlbCompany company;

		string IPostalStructuredAddress.PostcodeCode => company.GC_PostCode;

		string IPostalStructuredAddress.StreetName => string.Concat(company.Address1, AWBRequestConstants.StreetsSeparator, company.Address2);

		string IPostalStructuredAddress.CityName => company.GC_City;

		string IPostalStructuredAddress.CountryID => company.Country?.Code ?? ZString.Empty;

		string IPostalStructuredAddress.CountryName => company.Country?.Description ?? ZString.Empty;

		string IPostalStructuredAddress.CityID => ZString.Empty;

		string IPostalStructuredAddress.PostOfficeBox => ZString.Empty;
	}

	internal class FreightForwarderTradeContactWrapper : ITradeContact
	{
		internal FreightForwarderTradeContactWrapper(GlbCompany company)
		{
			this.company = Argument.NotNull(company, "company cannot be null");
		}
		readonly GlbCompany company;

		string ITradeContact.PersonName => ZString.Empty;

		string ITradeContact.DirectTelephoneCommunicationCompleteNumber => company.GC_Phone;

		string ITradeContact.FaxCommunicationCompleteNumber => company.GC_Fax;

		string ITradeContact.EmailCommunicationID => company.GC_Email;
	}
}
