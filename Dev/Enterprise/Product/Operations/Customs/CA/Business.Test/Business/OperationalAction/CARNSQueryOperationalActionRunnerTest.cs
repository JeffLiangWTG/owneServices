using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageManagers.Testing;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.OperationalAction.Testing
{
	sealed class CARNSQueryOperationalActionRunnerTest : TestCaseWithFactory
	{
		IDisposable asecSetup;

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

		[TestDate(2014, 12, 21)]
		public void TestPerformFunctionOperationalAction()
		{
			Enterprise.Customs.CA.Registry.CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "555");
			Enterprise.Customs.CA.Registry.CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest("12345"))
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory, "12345");

				var testOperationalActionLog = new DummyOperationalActionSectionLog();
				var testUserNotification = new TestMessageInstructionUserNotification();
				var logAndNotificationWrapper = new OperationalActionLogAndUserNotificationWrapper(testUserNotification, testOperationalActionLog, false, false, false);

				var testRunner = new CARNSQueryOperationalActionRunner(logAndNotificationWrapper, new SendsMessagesToCustomsShutterUpperer());

				var testDeclaration_IMP = Factory.New<JobDeclaration>();
				var testDeclaration_IMP_NoEntry = Factory.New<JobDeclaration>();
				var testDeclaration_EXP = Factory.New<JobDeclaration>();

				testDeclaration_IMP.JE_MessageType = JobMessageTypeList.Codes.Import;
				testDeclaration_IMP_NoEntry.JE_MessageType = JobMessageTypeList.Codes.Import;
				testDeclaration_EXP.JE_MessageType = JobMessageTypeList.Codes.Export;

				testDeclaration_IMP.JE_DeclarationReference = "Job001";
				var entryHeader = testDeclaration_IMP.CustomsEntryHeaders.AddNew();
				entryHeader.CH_BGMReference = "12345000000011";
				entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;

				Factory.Save();

				var id_testDeclaration_IMP = testDeclaration_IMP.JE_DeclarationReference;
				var id_testDeclaration_IMP_NoEntry = testDeclaration_IMP_NoEntry.JE_DeclarationReference;
				var id_testDeclaration_EXP = testDeclaration_EXP.JE_DeclarationReference;

				var ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(0, ediMessageCollection.Length);

				testRunner.PerformFunctionOperationalAction(new BusinessObject[]
				{
					testDeclaration_IMP, testDeclaration_IMP_NoEntry, testDeclaration_EXP
				});

				CombineAssertions("Result Test", () =>
					{
						ediMessageCollection = Factory.Load<EDIMessage>(new ZQuery());
						AssertEquals("Message after runner", 1, ediMessageCollection.Length);
						Assert("message for IMP", ediMessageCollection.Any(msg => msg.EM_LinkUniqueID == testDeclaration_IMP.ReleaseEntryHeader.PK));

						var expectlog = string.Format(@"INFO: ----------------------------------------
INFO: Processing [HL Job001]
INFO: Sending message for [HL Job001]
INFO: [User's Answer]:Continue sending despite of rationality warnings.
INFO: Request RNS Status Query for Declaration Job001 message queued for sending.
INFO: [Successful]
INFO: ----------------------------------------
INFO: Processing [HL B00001000]
ERROR: No Entries exist for this declaration, please ensure at least one Invoice Header and Line have been entered and select 'Generate Entries' from the brokerage menu.
INFO: ----------------------------------------
INFO: Processing [HL B00001001]
ERROR: The Declaration is not eligible to send RNS Query since it's not a Import Job."
							, id_testDeclaration_IMP
							, id_testDeclaration_IMP_NoEntry
							, id_testDeclaration_EXP);
						AssertEquals(expectlog, testOperationalActionLog.MessagesString().Replace("\n", "\r\n").Replace("\r\r", "\r"));
					}
				);
			}
		}
	}
}
