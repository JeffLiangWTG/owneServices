using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Delivery;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(DocumentDeliverableCollection))]
	sealed class DocumentDeliverableCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentDeliverableCollection>
	{
		protected override DocumentDeliverableCollection GetCollectionToTest()
		{
			return new DocumentDeliverableCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var delivery = new DummyDocumentDelivery
			{
				Document = new DummyDocument(),
				PrintInstructions = new DummyPrintInstructions(),
				EDocsInstructions = new DummyEDocsInstructions(),
				LogParent = Factory.New<DummyWithLogs>()
			};

			return new DocumentDeliverable(Factory, delivery);
		}
	}
}
