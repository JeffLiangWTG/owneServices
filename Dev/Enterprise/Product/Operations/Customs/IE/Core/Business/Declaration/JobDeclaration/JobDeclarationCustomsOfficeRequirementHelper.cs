using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class JobDeclarationCustomsOfficeRequirementHelper : EU.Business.JobDeclarationCustomsOfficeRequirementHelper
	{
		public JobDeclarationCustomsOfficeRequirementHelper(JobDeclaration declaration) : base(declaration)
		{
		}

		protected override CustomsOfficeRequirement GetMainOffice() => Factory.GetCachedValue("IE.JobDeclarationCustomsOfficeRequirementHelper.MainOffice|" + Declaration.JE_MessageType, () =>
		{
			var result = Declaration.IsImport
				? new CustomsOfficeRequirement(officeRole: ZString.Empty, isMandatory: true, isLocalCountryOnly: true)
				: Declaration.IsExport
					? new CustomsOfficeRequirement(officeRole: EuOfficeCodesTypes.Codes.OfficeOfExport, isMandatory: false, isLocalCountryOnly: true)
					: base.GetMainOffice();
			result.FriendlyName = Declaration.JE_CustomsOfficeInfo.HumanReadableName;
			return result;
		});

		protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements() => Factory.GetCachedValue(
			"IE.JobDeclarationCustomsOfficeRequirementHelper.OtherOffices|" + Declaration.JE_MessageType,
			CreateOtherRequirements
		);

		IEnumerable<CustomsOfficeRequirement> CreateOtherRequirements()
		{
			yield return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance }
			};

			yield return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfExit, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false)
			{
				OfficeRolesForLookup = new ZString[] {
					EuOfficeCodesTypes.Codes.OfficeOfExitInland,
					EuOfficeCodesTypes.Codes.OfficeOfLodgementExit,
					EuOfficeCodesTypes.Codes.OfficeOfExit
				}
			};

			yield return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.SupervisingOffice, isMandatory: false, isLocalCountryOnly: true, isForeignCountryOnly: false)
			{
				OfficeRolesForLookup = Array.Empty<ZString>()
			};

			if (Declaration.IsImport)
			{
				yield return new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfDischarge, isMandatory: false, isLocalCountryOnly: true, isForeignCountryOnly: false)
				{
					OfficeRolesForLookup = Array.Empty<ZString>()
				};
			}
		}
	}
}
