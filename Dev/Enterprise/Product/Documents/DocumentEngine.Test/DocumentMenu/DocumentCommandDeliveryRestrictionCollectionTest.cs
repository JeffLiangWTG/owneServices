using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	[TestedType(typeof(DocumentCommandDeliveryRestrictionCollection))]
	public class DocumentCommandDeliveryRestrictionCollectionTest : ActiveBusinessObjectCollectionTestCase<DocumentCommandDeliveryRestrictionCollection>
	{
		protected override DocumentCommandDeliveryRestrictionCollection GetCollectionToTest()
		{
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			return new DocumentCommandDeliveryRestrictionCollection(documentCommand);
		}

		public void TestRelationship()
		{
			var masterBizo = Factory.NewWithValidTestData<DocumentCommand>();
			var collection = new DocumentCommandDeliveryRestrictionCollection(masterBizo);
			AssertEquals("Precondition: collection1.Count", 0, collection.Count);

			var bizo = collection.AddNew();
			AssertEquals("testBizo1.X0_ParentID", masterBizo.PK, bizo.SDR_SU);
		}
	}
}
