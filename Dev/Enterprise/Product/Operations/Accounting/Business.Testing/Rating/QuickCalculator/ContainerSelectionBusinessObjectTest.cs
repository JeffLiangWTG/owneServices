using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ContainerSelectionBusinessObject))]
	public class ContainerSelectionBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsSelected()
		{
			var testObject = (ContainerSelectionBusinessObject)GetNewBusinessObject();

			testObject.IsSelected = true;
			AssertEquals(true, testObject.IsSelected);

			testObject.IsSelected = false;
			AssertEquals(false, testObject.IsSelected);
		}

		public void TestContainerNumber()
		{
			var testObject = (ContainerSelectionBusinessObject)GetNewBusinessObject();
			testObject.Number = "NUM123";
			AssertEquals("NUM123", testObject.Number);
		}

		public void TestContainerWeight()
		{
			var testObject = (ContainerSelectionBusinessObject)GetNewBusinessObject();
			testObject.Weight = 15m;
			AssertEquals(15m, testObject.Weight);
		}

		public void TestContainerVolume()
		{
			var testObject = (ContainerSelectionBusinessObject)GetNewBusinessObject();
			testObject.Volume = 1.5m;
			AssertEquals(1.5m, testObject.Volume);
		}

		public void TestContainerCommodity()
		{
			var testObject = (ContainerSelectionBusinessObject)GetNewBusinessObject();
			testObject.Commodity = "COMMODITY";
			AssertEquals("COMMODITY", testObject.Commodity);
		}

		public void TestQuantity()
		{
			var testObject = (ContainerSelectionBusinessObject)GetNewBusinessObject();
			testObject.Quantity = 3;
			AssertEquals(3m, testObject.Quantity);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ContainerSelectionBusinessObject(new ContainerSelectionBusinessObjectCollection());
		}
	}

	[TestedType(typeof(ContainerSelectionBusinessObjectCollection))]
	public class ContainerSelectionBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ContainerSelectionBusinessObjectCollection>
	{
		public void TestQuantities()
		{
			var testCollection = GetCollectionToTest();
			AddTestDataToCollection(testCollection);

			AssertEquals(14m, testCollection.TotalQuantity);
			AssertEquals(7m, testCollection.SelectedQuantity);
			AssertEquals("NUM2, , NUM3, ", testCollection.SelectedNumbers);
		}

		public void TestAllowNew()
		{
			var testCollection = GetCollectionToTest();
			AssertEquals(false, testCollection.AllowNew);
		}

		public void TestAllowRemove()
		{
			var testCollection = GetCollectionToTest();
			AssertEquals(false, testCollection.AllowRemove);
		}

		public void TestClone()
		{
			var testCollection = GetCollectionToTest();
			AddTestDataToCollection(testCollection);

			var clonedCollection = testCollection.Clone();

			var originValues = testCollection.Cast<ContainerSelectionBusinessObject>().Select(c => (c.IsSelected, c.Commodity, c.Number, c.Quantity, c.Volume, c.Weight));
			var clonedValues = clonedCollection.Cast<ContainerSelectionBusinessObject>().Select(c => (c.IsSelected, c.Commodity, c.Number, c.Quantity, c.Volume, c.Weight));

			AssertContainsExactElementsInAnyOrder(originValues, clonedValues);
		}

		static void AddTestDataToCollection(ContainerSelectionBusinessObjectCollection testCollection)
		{
			testCollection.AddNewSelection(false, "NUM1", 1, 1000, 1, "GEN");
			testCollection.AddNewSelection(true, "NUM2", 1, 1000, 1, "GEN");
			testCollection.AddNewSelection(true, "", 2, 1000, 1, "GEN");
			testCollection.AddNewSelection(true, "NUM3", 1, 1000, 1, "HAZ");
			testCollection.AddNewSelection(false, "NUM4", 1, 1000, 1, "HAZ");
			testCollection.AddNewSelection(true, "", 3, 1000, 1, "SHOE,HAT");
			testCollection.AddNewSelection(false, "", 5, 1000, 1, "SHIRT");
		}

		protected override ContainerSelectionBusinessObjectCollection GetCollectionToTest()
		{
			return new ContainerSelectionBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ContainerSelectionBusinessObject(GetCollectionToTest());
		}
	}
}
