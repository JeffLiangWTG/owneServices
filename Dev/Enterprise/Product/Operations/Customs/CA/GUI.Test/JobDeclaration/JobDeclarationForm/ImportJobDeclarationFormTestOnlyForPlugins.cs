using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Customs.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(JobDeclarationForm))]
	sealed class ImportJobDeclarationFormTestOnlyForPlugins : JobDeclarationFormTest
	{
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

			form.CustomsBrokerageUserControl.WorkflowTabPage.TabVisible = false;
			form.CustomsBrokerageUserControl.EventTabPage.TabVisible = false;
			form.CustomsBrokerageUserControl.DeclarationTabPage.TabVisible = false;
			form.CustomsBrokerageUserControl.DeclarationUserControl.Enabled = false;

			foreach (var plugin in form.PlugIns.Instances)
			{
				plugin.Enabled = true;
			}
		}
	}
}
