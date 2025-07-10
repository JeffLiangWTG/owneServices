using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Container
{
	[TestedType(typeof(DocCFSShipmentCollection))]
	internal class DocCFSShipmentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCFSShipmentCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			NonPersistentCFSShipment shipment = new NonPersistentCFSShipment();
			return DocCFSShipment.New(shipment, Factory);
		}

		protected override DocCFSShipmentCollection GetCollectionToTest()
		{
			return new DocCFSShipmentCollection(Factory);
		}
	}
}
