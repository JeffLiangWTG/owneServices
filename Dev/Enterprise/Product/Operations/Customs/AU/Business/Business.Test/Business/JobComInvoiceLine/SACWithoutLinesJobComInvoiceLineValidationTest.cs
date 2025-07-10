using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class SACWithoutLinesJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		public void TestSACWithoutLineShouldNotHaveAnyNotification()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			testDec.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 100m;

			testDec.MergeManager.DisablePreSaveMergeRequirementForTesting();
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			testDec.DoMerge();

			AssertEquals("Early check", 1, invoice.JobComInvoiceLines.Count);
			JobComInvoiceLine line = invoice.JobComInvoiceLines[0];
			AssertEquals(typeof(SACWithoutLinesJobComInvoiceLineValidation), line.Validation.GetType());
			line.ClearAllNotifications();
			line.RunPreSaveValidation();
			AssertEquals(line.Notifications.GetErrors().ToUniqueMessageListString() + System.Environment.NewLine +
				line.Notifications.GetMessageErrors().ToUniqueMessageListString() + System.Environment.NewLine +
				line.Notifications.GetWarnings().ToUniqueMessageListString() + System.Environment.NewLine +
				line.AddInfo.Notifications.GetErrors().ToUniqueMessageListString() + System.Environment.NewLine +
				line.AddInfo.Notifications.GetWarnings().ToUniqueMessageListString() + System.Environment.NewLine +
				line.AddInfo.Notifications.GetMessageErrors().ToUniqueMessageListString(), true, !line.Notifications.HasNotifications());
		}
	}
}
