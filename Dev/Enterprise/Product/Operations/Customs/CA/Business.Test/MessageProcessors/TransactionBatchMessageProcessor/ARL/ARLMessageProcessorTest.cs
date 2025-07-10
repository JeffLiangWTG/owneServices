using System;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class ARLMessageProcessorTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestBranchOnMessageIsUsedInProcessor()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CAC";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "OHC";
			company.GC_OH_OrgProxy = org.PK;
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "BBB";
			var branch3 = company.Branches.AddNew();
			branch3.GB_Code = "CCC";

			#region Message Text

			var messageText = @"
<UniversalTransactionBatch xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<TransactionBatch>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAAccountsReceivableLedger</Type>
					<Key></Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<BatchType>
			<Code>TST</Code>
			<Description>Test</Description>
		</BatchType>
		<TransactionCollection>
			<Transaction>
				<PostingJournalCollection>
					<PostingJournal>
					</PostingJournal>
				</PostingJournalCollection>
			</Transaction>
		</TransactionCollection>
	</TransactionBatch>
</UniversalTransactionBatch>";
			#endregion

			var ediMessage = ARLDailyReportDocumentWrapperTest.CreateTransactionBatchMessage(Guid.NewGuid().ToString(), Factory.BOFactory, messageText);
			ediMessage.EM_GB = branch2.PK;
			Factory.SaveForTesting();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var logger = new ServiceTaskLogForTesting();
				ProcessMessage(ediMessage, logger);
				AssertEquals("Message is processed OK", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("Branch on message is used in message processor", "BBB", logger.ToString());

				branch2.GB_IsActive = false;
				ediMessage = ARLDailyReportDocumentWrapperTest.CreateTransactionBatchMessage(Guid.NewGuid().ToString(), Factory.BOFactory, messageText);
				ediMessage.EM_GB = branch2.PK;
				Factory.SaveForTesting();

				logger = new ServiceTaskLogForTesting();
				ProcessMessage(ediMessage, logger);
				AssertEquals("Message is processed OK", EDIMessage.Status.ProcessedOK, ediMessage.EM_Status);
				AssertEquals("First active branch from message's company is used in message processor", "CCC", logger.ToString());

				branch3.GB_IsActive = false;
				ediMessage = ARLDailyReportDocumentWrapperTest.CreateTransactionBatchMessage(Guid.NewGuid().ToString(), Factory.BOFactory, messageText);
				ediMessage.EM_GB = branch2.PK;
				Factory.SaveForTesting();

				logger = new ServiceTaskLogForTesting();
				ProcessMessage(ediMessage, logger);
				AssertEquals("Message is rejected because no active branch found", EDIMessage.Status.Rejected, ediMessage.EM_Status);
				AssertEquals("Message rejected", "ERROR - Message Rejected as Company 'CAC' has no active branches.", logger.ToString());
			}
		}
	}
}
