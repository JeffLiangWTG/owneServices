using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(ConsolImportBranchRuleRegistryControl))]
	sealed class ConsolImportBranchRuleRegistryControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		[RequiresSTA]
		public void TestCaption()
		{
			var bizO = new ImportBranchRule();

			using (ZForm testForm = new ZForm(bizO))
			{
				using (var testControl = new ConsolImportBranchRuleRegistryControlForTest())
				{
					testForm.Controls.Add(testControl);
					testForm.Show();

					AssertEquals("Load Port Label Caption", "Default to Branch Related to Load Port", testControl.DefaultToOriginLoadPortCaption);
					AssertEquals("Destination Port Label Caption", "Default to Branch Related to Discharge Port", testControl.DefaultToDestinationDischargePortCaption);
				}
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new ImportBranchRule();
		}
	}
}
