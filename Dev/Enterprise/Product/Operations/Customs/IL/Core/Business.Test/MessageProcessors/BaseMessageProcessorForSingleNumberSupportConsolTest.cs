using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	public abstract class BaseMessageProcessorForSingleNumberSupportConsolTest<TProccessor, TResponse, TMessage>
	  : BaseMessageProcessorForSingleNumberTest<TProccessor, TResponse, TMessage>
	  where TProccessor : BaseMessageProcessorForSingleNumber<TResponse>
	  where TResponse : class
	  where TMessage : ILEDIResponseMessage
	{
		public void TestProcessMessage_ProcessedOKAndCES_WhenConsolFoundOnceFromNumber()
		{
			this.cusEntryNumber.Delete();
			var (forwardingConsol, cusEntryNumber) = CreateForwardingConsolAndEntryNumber();
			var message = GetMessage(false);
			cusEntryNumber.CE_EntryNum = EntryNum;
			Factory.Save();

			var processor = CreateProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			Factory.Save();

			message.Reload();
			AssertLogsMessage(forwardingConsol, message);
		}

		public void TestProcessMessage_Discarded_WhenConsolFoundMoreThanOneRecordFromNumber()
		{
			this.cusEntryNumber.Delete();
			var (forwardingConsol, cusEntryNumber) = CreateForwardingConsolAndEntryNumber();
			var factory = Factory;
			var message = GetMessage(false);
			cusEntryNumber.CE_EntryNum = EntryNum;

			var forwardingConsol2 = factory.NewWithValidTestData<ForwardingConsol>();
			var cusEntryNumberDON2 = CusEntryNumber.LoadOrCreate<CusEntryNumber>(forwardingConsol2, NumberType, "IL");
			cusEntryNumberDON2.CE_EntryNum = EntryNum;
			factory.Save();

			var loggingInformation = new LoggingInformation();
			var processor = CreateProcessor(loggingInformation);
			var result = processor.GetLinkedBusinessObjectMetaData(message, loggingInformation);

			CombineAssertions("When More Than One Number Found", () =>
			{
				Assert("Discard reason should be provided", !result.DiscardReason.IsEmpty);
				AssertEquals("Discard reason should be as expected", MoreThanOneMessage, result.DiscardReason);
			});
		}

		protected override bool ExpectedSupportsConsol => true;

		(ForwardingConsol, CusEntryNumber) CreateForwardingConsolAndEntryNumber()
		{
			var factory = Factory;
			var forwardingConsol = factory.NewWithValidTestData<ForwardingConsol>();
			var cusEntryNumber = CusEntryNumber.LoadOrCreate<CusEntryNumber>(forwardingConsol, NumberType, "IL");
			return (forwardingConsol, cusEntryNumber);
		}
	}
}
