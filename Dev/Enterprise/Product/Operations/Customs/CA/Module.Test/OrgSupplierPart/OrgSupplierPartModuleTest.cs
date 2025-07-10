using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	sealed class OrgSupplierPartModuleTest : Customs.Module.Testing.OrgSupplierPartModuleTest
	{
		public void TestCopyOGDToPGADataMenuItem()
		{
			using (var moduleHostForm = new ZForm())
			using (var module = new OrgSupplierPartModule())
			{
				moduleHostForm.Controls.Add(module.EmbeddedControl);
				moduleHostForm.Show();
				Application.DoEvents();

				var menuItem = module.FormActionMenu.FindByText("Copy OGD to PGA Data", true);
				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("There is no product to be transformed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
