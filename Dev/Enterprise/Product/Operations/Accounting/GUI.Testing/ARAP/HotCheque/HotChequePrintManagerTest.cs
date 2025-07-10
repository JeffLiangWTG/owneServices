using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.HotCheque.Testing
{
	public class HotChequePrintManagerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPrintPromptErrorIfDocumentMenusIsMissing()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			StmTemplate chequeTemplate = Factory.NewWithValidTestData<StmTemplate>(); //Factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_Name, TemplateName));
			chequeTemplate.SO_Name = "Test template";
			Factory.Save();
			using (bankAccount.GetValidationSuspender())
			{
				bankAccount.AB_SO_ChequeTemplate = chequeTemplate.PK;
				AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
				chequeBook.AK_AutoPrintCheque = ZBool.True;
				chequeBook.AK_AB = bankAccount.PK;
				chequeBook.AK_StartNo = 2;
				chequeBook.AK_LastNo = 5;
				chequeBook.AK_CurrentNo = 3;
				AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
				hotCheque.AQ_AK = chequeBook.PK;

				chequeTemplate.SO_Name = "Test template renamed";
				Factory.Save();
				HotChequePrintManager printManager = new HotChequePrintManager(hotCheque, Factory);
				printManager.Print();
				Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Please make sure 'Test template renamed' Document menu with Empty menu path value exists for cheque template Test template renamed. If not, please create one through the Document Menu Customization screen", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}
	}
}
