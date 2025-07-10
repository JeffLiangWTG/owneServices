using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public class CustomsOfficesUserControlTest : TestCaseWithFactory
	{
		public void TestCustomsOfficesGridVisibility()
		{
			var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;
			officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>());

			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var customsOfficesUserControl = (userControl.Controls.Find("CustomsOfficesUserControl", true).First() as ZDynamicControlCreationUserControl).HostedControl as CustomsOfficesUserControl;
				customsOfficesUserControl.HandleDeclarationControlVisibilityChanged();
				Assert("The grid should be invisible as there is no requirements.", !customsOfficesUserControl.Controls.Find("CustomsOfficesGrid", true).First().Visible);

				officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>
				{
					new CustomsOfficeRequirement
					{
						OfficeRole = "CAU"
					}
				});

				customsOfficesUserControl.HandleDeclarationControlVisibilityChanged();
				Assert("The grid should be visible as long as there is one requirement.", customsOfficesUserControl.Controls.Find("CustomsOfficesGrid", true).First().Visible);
			}
		}

		public void TestCustomsOfficeFindBoxVisibility()
		{
			var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;
			officeHelper.SetMainOffice(null);

			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var customsOfficesUserControl = (userControl.Controls.Find("CustomsOfficesUserControl", true).First() as ZDynamicControlCreationUserControl).HostedControl as CustomsOfficesUserControl;
				Assert("The main office find box should be invisible as the main office is null.", !customsOfficesUserControl.Controls.Find("CustomsOfficeFindBox", true).First().Visible);

				officeHelper.SetMainOffice(new CustomsOfficeRequirement
				{
					OfficeRole = "CAU"
				});
				customsOfficesUserControl.HandleDeclarationControlVisibilityChanged();
				Assert("The main office find box should be visible as the main office is not null.", customsOfficesUserControl.Controls.Find("CustomsOfficeFindBox", true).First().Visible);
			}
		}

		public void TestIsOverrideLabelByFriendlyName()
		{
			var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;
			officeHelper.SetOtherRequirements(new List<CustomsOfficeRequirement>());

			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var customsOfficesUserControl = (userControl.Controls.Find("CustomsOfficesUserControl", true).First() as ZDynamicControlCreationUserControl).HostedControl as CustomsOfficesUserControl;
				AssertEquals(true, customsOfficesUserControl.IsOverrideLabelByFriendlyName);
			}
		}

		public void TestCustomsOfficeFindBoxCaption()
		{
			var declaration = Factory.New<JobDeclarationForCustomsOfficeRequirementTest>();
			var officeHelper = declaration.CustomsOfficeRequirementHelper;
			officeHelper.SetMainOffice(null);

			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var customsOfficesUserControl = (userControl.Controls.Find("CustomsOfficesUserControl", true).First() as ZDynamicControlCreationUserControl).HostedControl as CustomsOfficesUserControl;
				AssertEquals("The caption of main office find box should fallback to 'Customs Office' as the main office is null.", "Customs Office", customsOfficesUserControl.Controls.Find("CustomsOfficeFindBox", true).First().GetExtension<ILabelCaptionRenderer>().Caption);

				officeHelper.SetMainOffice(new CustomsOfficeRequirement
				{
					OfficeRole = "CAU",
					FriendlyName = "Office 123456"
				});
				customsOfficesUserControl.HandleDeclarationControlVisibilityChanged();
				AssertEquals("The caption of main office find box should be the friendly name of main office.", "Office 123456", customsOfficesUserControl.Controls.Find("CustomsOfficeFindBox", true).First().GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}
	}
}
