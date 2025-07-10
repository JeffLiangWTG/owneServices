using System.Collections.Generic;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public class CustomsOfficeRequirementHelperForTest : JobDeclarationCustomsOfficeRequirementHelper
{
	public CustomsOfficeRequirementHelperForTest(JobDeclarationForCustomsOfficeRequirementTest declaration) : base(declaration)
	{
		mainOffice = null;
		otherRequirements = new List<CustomsOfficeRequirement>();
	}

	protected override CustomsOfficeRequirement GetMainOffice() => mainOffice;
	CustomsOfficeRequirement mainOffice;

	protected override IEnumerable<CustomsOfficeRequirement> GetOtherRequirements() => otherRequirements;
	IEnumerable<CustomsOfficeRequirement> otherRequirements;

	public void SetMainOffice(CustomsOfficeRequirement mainOffice)
	{
		this.mainOffice = mainOffice;
	}

	public void SetOtherRequirements(IEnumerable<CustomsOfficeRequirement> otherRequirements)
	{
		this.otherRequirements = otherRequirements;
	}
}
