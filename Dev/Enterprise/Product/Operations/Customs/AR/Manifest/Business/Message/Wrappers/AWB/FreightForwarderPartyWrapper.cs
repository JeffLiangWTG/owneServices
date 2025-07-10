using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class FreightForwarderPartyWrapper : IParty
	{
		internal FreightForwarderPartyWrapper(GlbCompany company)
		{
			this.company = Argument.NotNull(company, "company cannot be null");
		}
		readonly GlbCompany company;

		string IParty.PrimaryID => company.GC_CustomsRegistrationNo;

		string IParty.SchemeAgencyID => company.GC_BusinessRegNo2;

		string IParty.AdditionalID => ZString.Empty;

		string IParty.Name => company.GC_Name;

		string IParty.AccountID => ZString.Empty;

		IPostalStructuredAddress IParty.PostalStructuredAddress => postalStructuredAddress ?? (postalStructuredAddress = new FreightForwarderPostalStructuredAddressWrapper(company));
		IPostalStructuredAddress postalStructuredAddress;

		ITradeContact IParty.DefinedTradeContact => definedTradeContact ?? (definedTradeContact = new FreightForwarderTradeContactWrapper(company));
		ITradeContact definedTradeContact;
	}
}
