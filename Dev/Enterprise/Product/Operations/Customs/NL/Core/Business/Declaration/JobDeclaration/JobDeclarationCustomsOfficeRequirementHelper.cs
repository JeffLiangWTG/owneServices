using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class JobDeclarationCustomsOfficeRequirementHelper : EU.Business.JobDeclarationCustomsOfficeRequirementHelper
{
	public JobDeclarationCustomsOfficeRequirementHelper(JobDeclaration declaration) : base(declaration)
	{
	}

	protected override CustomsOfficeRequirement GetMainOffice() => Factory.GetCachedValue("NL.JobDeclarationCustomsOfficeRequirementHelper.MainOffice" + Declaration.JE_MessageType,
		() => Declaration.IsImport ? new CustomsOfficeRequirement(ZString.Empty, true, true) : base.GetMainOffice());

	protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements()
	{
		return Factory.GetCachedValue("NL.JobDeclarationCustomsOfficeRequirementHelper.OtherRequirements" + Declaration.JE_MessageType, () =>
		{
			var customsOfficeRequirementList = new List<CustomsOfficeRequirement>();

			if (!Declaration.IsImport)
			{
				customsOfficeRequirementList = base.GetOtherRequirements().ToList();
			}
			else
			{
				customsOfficeRequirementList.Add(new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.OfficeOfGuarantee, false, true));
			}

			if (Declaration.IsExport || Declaration.IsImport)
			{
				customsOfficeRequirementList.Add(new CustomsOfficeRequirement(EuOfficeCodesTypes.Codes.AuthorityControlCode, false, true, Enterprise.Customs.NL.Business.Res.GetData("CAE8DA78-53E8-4162-AB6E-7EA04C7E2BB6", "Supervising Office").Caption));
			}

			return customsOfficeRequirementList;
		});
	}
}
