using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.Testing;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class CustomsOfficesUserControlTest : TestCaseWithFactory
{
	public void TestCustomsOfficesGridVisibility()
	{
		var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
		var officeHelper = declaration.CustomsOfficeRequirementHelper;
		officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>());

		using (var userControl = new CustomsOfficesUserControl())
		{
			userControl.HandleDeclarationControlVisibilityChanged();
			Assert("The grid should be visible even if there are no requirements.", userControl.Controls.Find("CustomsOfficesGrid", true).First().Visible);

			officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement> { new CustomsOfficeRequirement { OfficeRole = "CAU" } });
			userControl.HandleDeclarationControlVisibilityChanged();
			Assert("The grid should be visible as long as there is one requirement.", userControl.Controls.Find("CustomsOfficesGrid", true).First().Visible);
		}
	}
}
