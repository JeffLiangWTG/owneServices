using CargoWise.Common;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class FreightForwarderPostalStructuredAddressWrapper : IPostalStructuredAddress
	{
		internal FreightForwarderPostalStructuredAddressWrapper(GlbCompany company)
		{
			this.company = Argument.NotNull(company, "company cannot be null");
		}
		readonly GlbCompany company;

		string IPostalStructuredAddress.PostcodeCode => company.GC_PostCode;

		string IPostalStructuredAddress.StreetName => string.Concat(company.Address1, ARAWBMessageConstants.StreetsSeparator, company.Address2);

		string IPostalStructuredAddress.CityName => company.GC_City;

		string IPostalStructuredAddress.CountryID => company.Country?.Code ?? ZString.Empty;

		string IPostalStructuredAddress.CountryName => company.Country?.Description ?? ZString.Empty;

		string IPostalStructuredAddress.CityID => ZString.Empty;

		string IPostalStructuredAddress.PostOfficeBox => ZString.Empty;
	}
}
