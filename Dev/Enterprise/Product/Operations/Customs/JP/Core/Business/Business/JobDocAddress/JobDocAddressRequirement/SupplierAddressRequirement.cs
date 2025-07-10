using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.Business
{
	public class SupplierAddressRequirement : JobDocAddressRequirement
	{
		public SupplierAddressRequirement(DocAddressType defaultDocAddressType, ContactType defaultContactType)
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
				if (declaration.IsExport && declaration.IsSea && !declaration.IsECR)
				{
					MandatoryValidation.WarnIfNotEntered(targetInfo);
				}
				else
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}

				CheckIsCusCodeRequired(declaration, parent, targetInfo);
			}
		}

		void ValidateE2_CompanyName(JobDocAddressValidation validation)
		{
			var parent = (JPJobDocAddress)validation.Parent;
			if (parent.E2_AddressOverride && parent.Parent is JobDeclaration declaration)
			{
				var targetInfo = parent.E2_CompanyNameInfo;
				if (declaration.IsExport && declaration.IsSea && !declaration.IsECR)
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
			var parent = (JPJobDocAddress)validation.Parent;
			if (parent.E2_AddressOverride && parent.Parent is JobDeclaration declaration)
			{
				CheckIsCusCodeRequired(declaration, parent, parent.E2_GovRegNumInfo);
			}
		}

		void CheckIsCusCodeRequired(JobDeclaration declaration, JPJobDocAddress parent, ZPropertyInfo targetInfo)
		{
			var requiredCusCodeTypes = parent.RequiredCusCodeTypes;
			var isCusCodeRequired = declaration.IsExport && declaration.CustomsEntryInstructions.Cast<CusEntryInstruction>().Any(c => c.CEI_Style == JPExportDeclarationTypeList.Codes.M) &&
					(parent.E2_AddressOverride
						? requiredCusCodeTypes.All(c => c != parent.E2_GovRegNumType) || parent.E2_GovRegNum.IsEmpty
						: parent.Organisation is OrgHeader orgHeader && orgHeader.CustomsCodes.GetOrgCusCodeObjectMatchingCountryAndCodes(Core.Constants.CountryCodes.Japan, requiredCusCodeTypes) == null);

			if (isCusCodeRequired)
			{
				targetInfo.AddMessageError(Res.GetString("A2307200-AF58-4C1E-9625-41F8D7F8C52D", "The selected Exporter must have either an LPC Legal Person Code, a CIE Importer/Exporter Code, or a JAS JASTPRO Code."));
			}
		}
	}
}
