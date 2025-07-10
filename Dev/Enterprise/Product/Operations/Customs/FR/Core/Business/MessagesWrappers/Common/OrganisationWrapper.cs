using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class OrganisationWrapper : IOrganisation
	{
		public OrganisationWrapper(CusEntryHeader entryHeader, OrgAddress organisationAddress, OrgHeader header, string alternateCountryCode = "")
		{
			this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
			this.organisationHeader = header;
			declaration = this.entryHeader.Declaration;
			this.organisationAddress = organisationAddress;
			this.alternateCountryCode = alternateCountryCode;
		}

		public virtual ZString OrganisationNumber => organisationAddress?.GetEuIdentificationNumber().Trim().Left(17) ?? ZString.Empty;

		public virtual ZString OrganisationNumberEoriOnly => organisationAddress?.GetEORI().Trim().Left(17) ?? ZString.Empty;

		public virtual ZString FullName => organisationAddress?.CompanyName ?? ZString.Empty;

		public virtual ZString Address => organisationAddress?.Address1 ?? ZString.Empty;

		public virtual ZString CountryCode => alternateCountryCode.IsNullOrEmpty() ? organisationAddress?.OA_RN_NKCountryCode ?? ZString.Empty : alternateCountryCode;

		public virtual ZString PostCode => organisationAddress?.OA_PostCode ?? ZString.Empty;

		public virtual ZString City => organisationAddress?.OA_City ?? ZString.Empty;

		#region Export

		public ZString PartnerConsigneeID => organisationHeader?.CustomsClientID ?? ZString.Empty;

		public ZString PartnerDestIDInfo => organisationAddress?.UnrestrictedAdditionalAddressInformation ?? ZString.Empty;

		ZString IOrganisation.EoriCode => organisationHeader?.GetEORI().Trim().Left(17) ?? ZString.Empty;

		#endregion

		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;
		protected readonly OrgHeader organisationHeader;
		protected readonly OrgAddress organisationAddress;
		readonly string alternateCountryCode;
	}
}
