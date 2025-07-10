using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUImportDiscardedMessageUserControlTest : TestCaseWithFactory
	{
		public void TestImportMessageUserControlForEdificeImportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "LEG";
			using (ZAUCustomsDeclarationForm testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MessagesTabPage;
				AssertEquals("AUImportDiscardedMessageUserControl", typeof(AUImportDiscardedMessageUserControl), testForm.CustomsBrokerageUserControl.MessageUserControl.GetType());
				AUImportDiscardedMessageUserControl control = (AUImportDiscardedMessageUserControl)testForm.CustomsBrokerageUserControl.MessageUserControl;
				AssertEquals("AUImportMessageUserControl", typeof(AUImportMessageUserControl), control.NewMessageUserControl.GetType());
			}
		}

		public void TestSupplierControlForCMRImportJob()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = "CMR";
			using (ZAUCustomsDeclarationForm testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MessagesTabPage;
				AssertEquals("AUImportDiscardedMessageUserControl", typeof(AUImportDiscardedMessageUserControl), testForm.CustomsBrokerageUserControl.MessageUserControl.GetType());
				AUImportDiscardedMessageUserControl control = (AUImportDiscardedMessageUserControl)testForm.CustomsBrokerageUserControl.MessageUserControl;
				AssertEquals("AUCMRMessageUserControl", typeof(AUCMRMessageUserControl), control.NewMessageUserControl.GetType());
			}
		}

		public void TestSupplierControlForExportJob()
		{
			var declaration = JobDeclaration.New(Factory);
			using (var testForm = new ZAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.MessagesTabPage;
				AssertEquals("AUImportDiscardedMessageUserControl", typeof(AUImportDiscardedMessageUserControl), testForm.CustomsBrokerageUserControl.MessageUserControl.GetType());
				var control = (AUImportDiscardedMessageUserControl)testForm.CustomsBrokerageUserControl.MessageUserControl;
				AssertEquals("AUCMRMessageUserControl", typeof(AUCMRMessageUserControl), control.NewMessageUserControl.GetType());
			}
		}
	}
}
