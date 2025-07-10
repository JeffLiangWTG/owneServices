using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.Business
{
	public class ImporterAddressRequirement : JobDocAddressRequirement
	{
		public ImporterAddressRequirement(DocAddressType defaultDocAddressType, ContactType defaultContactType)
			: base(defaultDocAddressType, defaultContactType)
		{
			Initialize();
		}

		void Initialize()
		{
			this.ValidateOrganisationPK += ValidateOrganization;
			this.ValidateCompanyName += ValidateE2_CompanyName;
			this.ValidateGovRegNo += ValidateE2_GovRegNum;
		}

		void ValidateOrganization(JobDocAddressValidation validation)
		{
			var parent = (JPJobDocAddress)validation.Parent;
			if (!parent.E2_AddressOverride && parent.Parent is JobDeclaration declaration)
			{
				var targetInfo = parent.OrganisationPKInfo;
				if (declaration.IsImport && declaration.IsSea)
				{
					MandatoryValidation.WarnIfNotEntered(targetInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			var parent = (JPJobDocAddress)validation.Parent;
			if (parent.E2_AddressOverride && parent.Parent is JobDeclaration declaration)
			{
				var targetInfo = parent.E2_CompanyNameInfo;
				if (declaration.IsImport && declaration.IsSea)
				{
					MandatoryValidation.WarnIfNotEntered(targetInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		void ValidateE2_GovRegNum(JobDocAddressValidation validation)
		{
		}
	}
}
