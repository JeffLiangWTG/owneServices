using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	class BaseOutgoingMessageProcessorForTestWithZSaveConcurrencyException : BaseOutgoingMessageProcessor
	{
		public BaseOutgoingMessageProcessorForTestWithZSaveConcurrencyException(LoggingInformation logger)
			: base(logger)
		{
			count = 0;

			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			DummyBizo = factory1.NewWithValidTestData<DummyBusinessObject>();
			DummyBizo.Z0_Description = "initialValue";
			factory1.Save();
		}

		public DummyBusinessObject DummyBizo { get; }

		public int Increment { get; set; }

		protected override ZQuery MessageFilter => new ZQuery();

		protected override void HandleBeforeMessageProcessing(NonDependentEDIMessageCollection readyMessages, BusinessObjectFactory factory)
		{
			if (count >= maxRetryTimes)
			{
				foreach (EDIMessage message in readyMessages)
				{
					message.EM_Status = EDIMessage.Status.Sent;
				}
			}
		}

		internal override BusinessObjectFactory CreateFactory()
		{
			var factory = base.CreateFactory();
			factory.Saving += s =>
			{
				factory.Load<DummyBusinessObject>(DummyBizo.PK).Z0_Description = "changeValue";

				if (count < maxRetryTimes)
				{
					var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
					var bizoInFactory2 = factory2.Load<DummyBusinessObject>(DummyBizo.PK);
					bizoInFactory2.Z0_Description = "hello" + count;
					factory2.Save();
				}

				count += Increment;
			};
			return factory;
		}

		int count;
		const int maxRetryTimes = 6;  // ZExceptionReporting.ProcessWithSaveExceptionHandling will retry 2 times by default and BaseOutgoingMessageProcessor.Process will reTry 3 times, so in total retry time is 6.
	}
}
