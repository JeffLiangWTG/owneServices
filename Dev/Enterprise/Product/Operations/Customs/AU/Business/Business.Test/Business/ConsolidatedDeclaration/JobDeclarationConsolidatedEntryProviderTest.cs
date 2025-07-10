using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobDeclarationConsolidatedEntryProviderTest : TestCaseWithFactory
	{
		public void TestQueueForConsolidationValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var headerQuestion = entryHeader.Questions.AddNew();
			headerQuestion.ON_CPDecNum = 1;
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			Factory.Save();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var expectedMessage = "This is a required lodgement declaration (not a question), you must acknowledge this declaration by entering YES, otherwise your entry/amendment will be rejected.";
			AssertEquals("Before queue for consolidation", false, headerQuestion.ON_AnswerCodeInfo.HasNotification(expectedMessage));
			declaration.ConsolidatedEntryProvider.QueueForConsolidation(out var queueMessage);
			AssertEquals("After queue for consolidation", false, headerQuestion.ON_AnswerCodeInfo.HasNotification(expectedMessage));
			AssertEquals($"Entry status after queue", ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, declaration.JE_EntryStatus);
		}

		public void TestConsolidationStatusAfterDequeueOrRemoveFromConsolidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			declaration.ConsolidatedEntryProvider.QueueForConsolidation(out var queueMessage);
			AssertEquals($"Entry status after queue", ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, declaration.JE_EntryStatus);
			Factory.Save();
			declaration.ConsolidatedEntryProvider.DequeueOrRemoveFromConsolidation(out var dequeueMessage);
			AssertEquals($"Entry status after dequeue", ZString.Empty, declaration.JE_EntryStatus);
		}
	}
}
