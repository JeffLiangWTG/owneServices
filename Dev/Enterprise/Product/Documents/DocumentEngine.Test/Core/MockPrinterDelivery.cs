using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class MockPrinterDelivery : DocDeliveryPrintDetails
	{
		public MockPrinterDelivery(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override StmPrintQueueCollection Printers
		{
			get
			{
				StmPrintQueueCollection collection = new StmPrintQueueCollection(Factory);

				StmPrintQueue printer1 = collection.AddNew();
				printer1.SQ_QueueName = "Printer1";
				Assertion.Assert("Printer 1 should be allowed", printer1.IsPrintAllowed);

				StmPrintQueue printer2 = collection.AddNew();
				printer2.SQ_QueueName = "Printer2";
				printer2.SQ_QueueDeleted = Env.Time.CurrentLocalDateTime;
				Assertion.Assert("Printer 2 should be allowed", printer2.IsPrintAllowed);

				StmPrintQueue printer3 = collection.AddNew();
				printer3.SQ_QueueName = "Printer3";
				Env.Security.GetPrintQueueCheckPoint(printer3.PK.ToGuid(), printer3.SQ_DisplayName).IsAllowed = false;
				Assertion.Assert("Printer 3 should be denied", !printer3.IsPrintAllowed);

				return collection;
			}
		}
	}
}
