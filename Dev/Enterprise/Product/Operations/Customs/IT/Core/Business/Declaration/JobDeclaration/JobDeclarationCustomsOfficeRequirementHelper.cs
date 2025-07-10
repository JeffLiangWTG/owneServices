using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationCustomsOfficeRequirementHelper : EU.Business.JobDeclarationCustomsOfficeRequirementHelper
{
	public JobDeclarationCustomsOfficeRequirementHelper(JobDeclaration declaration) : base(declaration)
	{
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override CustomsOfficeRequirement GetMainOffice()
	{
		return Factory.GetCachedValue("IT.JobDeclarationCustomsOfficeRequirementHelper.MainOffice" + Declaration.JE_MessageType, () => GetMainOfficeCustomsRequirement());
	}

	protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
	{
		var declaration = Declaration;
		if (declaration.IsImport)
		{
			return GetImportCustomsOfficeOtherRequirements();
		}
		if (declaration.IsUCC6AndIsExport)
		{
			return GetUcc6ExportCustomsOfficeOtherRequirements(base.GetOtherRequirements());
		}
		return base.GetOtherRequirements();
	}

	protected override string GetCacheKeyCombination() => string.Join(",", base.GetCacheKeyCombination(), Declaration.IsUCC6);

	CustomsOfficeRequirement GetMainOfficeCustomsRequirement()
	{
		var friendlyName = Res.GetString("fd2e4e42-ba0f-4c25-bc77-75d3206fff85", "Office of Presentation");
		var declaration = Declaration;
		if (declaration.IsImport)
		{
			return new CustomsOfficeRequirement(ZString.Empty, isMandatory: true, isLocalCountryOnly: true, friendlyName);
		}
		if (declaration.IsExport)
		{
			return new CustomsOfficeRequirement(ZString.Empty, isMandatory: true, isLocalCountryOnly: true, friendlyName);
		}
		return base.GetMainOffice();
	}

	IEnumerable<CustomsOfficeRequirement> GetImportCustomsOfficeOtherRequirements()
	{
		return Factory.GetCachedValue("IT.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements.IMP", () => new[]
		{
			new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.AuthorityControlCode, isMandatory: false, isLocalCountryOnly: false, UniversalReferenceConstants.CommonResStrings.SupervisingCustomsOffice),
			new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: false, UniversalReferenceConstants.CommonResStrings.OfficeOfPresentationForCentralizedClearance)
		});
	}

	IEnumerable<CustomsOfficeRequirement> GetUcc6ExportCustomsOfficeOtherRequirements(IEnumerable<CustomsOfficeRequirement> customsOfficeOtherRequirementsToBeIncluded)
	{
		return Factory.GetCachedValue("IT.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements.EXP.UCC6", () =>
		{
			var result = new List<CustomsOfficeRequirement>();
			var officeOfPresentation = new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfPresentation, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false, friendlyName: UniversalReferenceConstants.CommonResStrings.OfficeOfPresentationForCentralizedClearance)
			{
				OfficeRolesForLookup = new ZString[] { EuOfficeCodesTypes.Codes.OfficeForCentralizedClearance }
			};
			var supervisingOffice = new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.SupervisingOffice, isMandatory: false, isLocalCountryOnly: false, isForeignCountryOnly: false)
			{
				OfficeRolesForLookup = Array.Empty<ZString>()
			};

			result.AddRange(customsOfficeOtherRequirementsToBeIncluded);
			result.Add(officeOfPresentation);
			result.Add(supervisingOffice);
			return result;
		});
	}
}
