using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MasterFiles;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class OrganisationDocumentaryWrapper : OrganisationWrapper
	{
		public OrganisationDocumentaryWrapper(CusEntryHeader entryHeader, FRJobDocAddress pJobDocAddress, OrgHeader importer, string alternateCountryCode = "") : base(entryHeader, null, importer)
		{
			this.jobDocAddress = Argument.NotNull(pJobDocAddress, "ImporterDocumentaryAddress cannot be null");
			this.importer = importer;
			this.alternateCountryCode = alternateCountryCode;
		}

		public override ZString OrganisationNumber => jobDocAddress.GetEuIdentificationNumber().Trim().Left(17);

		public override ZString OrganisationNumberEoriOnly => importer?.GetEORI().Trim().Left(17) ?? ZString.Empty;

		public override ZString FullName => jobDocAddress.CompanyName;

		public override ZString Address => jobDocAddress.Address1;

		public override ZString CountryCode => alternateCountryCode.IsNullOrEmpty() ? jobDocAddress.E2_RN_NKCountryCode : alternateCountryCode;

		public override ZString PostCode => jobDocAddress.Postcode;

		public override ZString City => jobDocAddress.City;

		readonly FRJobDocAddress jobDocAddress;
		readonly OrgHeader importer;
		readonly string alternateCountryCode;
	}
}
