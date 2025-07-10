using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.Integration;
using Enterprise.Integration.DocumentVisualizer;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Testing
{
	public class DocumentEDocsDeliveryTest : TestCaseWithFactory
	{
		public void TestSaveCopyToEDocs()
		{
			var dummyLogParent = Factory.New<DummyWithLogs>();
			var eDocsParent = Factory.New<Forwarding.IForwardingShipment>();

			var documentDelivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument(),

				PrintInstructions = new DummyPrintInstructions(),
				EDocsInstructions = new DummyEDocsInstructions
				{
					SaveCopyToEDocs = true,
					Parent = eDocsParent
				},
				LogParent = dummyLogParent
			};

			var deliverable = new DocumentDeliverable(Factory, documentDelivery);

			AssertEquals("EDocsParent", eDocsParent, deliverable.EDocsParent);
			AssertEquals("SaveCopyToEDocs", true, deliverable.SaveCopyToEDocs);

			var documentEDocsDelivery = ObjectFactory.Get<IDocumentEDocsDelivery>();

			documentEDocsDelivery.SaveCopyToEDocs(deliverable, "name", "title");

			StmPrintJob printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_ParentGuid, eDocsParent.PK));
			AssertNotNull("Should have a document created int the parent", printJob);
			AssertContains("name (title)", printJob.SP_DocumentName);
		}
	}
}
