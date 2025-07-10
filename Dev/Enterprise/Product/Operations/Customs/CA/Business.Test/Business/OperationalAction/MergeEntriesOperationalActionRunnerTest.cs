using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business.MessageManagers.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.CA.Business.OperationalAction.Testing
{
	sealed class MergeEntriesOperationalActionRunnerTest : TestCaseWithFactory
	{
		public void TestPerformFunctionOperationalAction()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration1.Invoices.AddNew();
			header.InvoiceLines.AddNew();

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.Invoices.AddNew();

			var declaration3 = Factory.New<JobDeclaration>();
			Factory.Save();

			var testOperationalActionLog = new DummyOperationalActionSectionLog();
			var testUserNotification = new TestMessageInstructionUserNotification();
			var logNotificationWrapper = new OperationalActionLogAndUserNotificationWrapper(testUserNotification, testOperationalActionLog, false, false);
			var testRunner = new MergeEntriesOperationalActionRunner(logNotificationWrapper);

			testRunner.PerformFunctionOperationalAction(new JobDeclaration[] { declaration1, declaration2, declaration3 });

			CombineAssertions(() =>
			{
				AssertEquals("declaration1 entry count after runner", 2, declaration1.Entries.Count);
				AssertEquals("declaration2 entry count after runner", 0, declaration2.Entries.Count);
				AssertEquals("declaration3 entry count after runner", 0, declaration2.Entries.Count);

				var expectlog = @"INFO: ----------------------------------------
INFO: Successfully merge entry for [HL B00001000]
INFO: ----------------------------------------
WARNING: Can't merge [HL B00001001]. Merge failed reason : You can't merge this entry because there is an invoice header with no invoice lines.
INFO: ----------------------------------------
WARNING: Can't merge [HL B00001002]. Merge failed reason : You can't merge this entry because there are no invoice headers.
";

				AssertMultilineASCIIEquals(expectlog, testOperationalActionLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
			});
		}
	}
}
