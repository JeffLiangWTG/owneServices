using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportJobDeclarationFormTestWithoutPlugins : JobDeclarationFormTest
	{
		public void TestCSAManualReleaseStrategy()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Business.JobMessageTypeList.Codes.Import;
			dec.CA_ServiceOption = ACROSSServiceOptions.Codes.CSA;
			Factory.Save();

			using (var form = new JobDeclarationFormForTest(dec))
			{
				var strategies = form.GetPreSaveDialogStrategiesForTesting();
				Assert(strategies.Any(x => x.GetType() == typeof(CSAManualReleaseStrategy)));
			}
		}

		public override ZString MessageTypeForFormBashing => Business.JobMessageTypeList.Codes.Import;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}
		IDisposable asecSetup;

		protected override void SetupDeclarationForSpecificFormBashing(BaseJobDeclarationForm form)
		{
			base.SetupDeclarationForSpecificFormBashing(form);
			form.CustomsBrokerageUserControl.InvoiceLinesTabPage.TabVisible = false;
			form.CustomsBrokerageUserControl.InvoicesTabPage.TabVisible = false;
			form.CustomsBrokerageUserControl.PackingTabPage.TabVisible = false;
			form.CustomsBrokerageUserControl.MiscOptionsTabPage.TabVisible = false;
			form.CustomsBrokerageUserControl.MessagesTabPage.TabVisible = false;
			form.CustomsBrokerageUserControl.InvoiceGroupingTabPage.TabVisible = false;
			form.CustomsBrokerageUserControl.ContainerTabPage.TabVisible = false;

			foreach (var plugin in form.PlugIns.Instances)
			{
				plugin.Enabled = false;
			}
		}
	}
}
