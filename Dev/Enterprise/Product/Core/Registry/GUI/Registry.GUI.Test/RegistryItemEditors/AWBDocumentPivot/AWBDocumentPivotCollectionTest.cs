using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(AWBDocumentPivotCollection))]
	sealed class AWBDocumentPivotCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AWBDocumentPivotCollection>
	{
		protected override AWBDocumentPivotCollection GetCollectionToTest()
		{
			return new AWBDocumentPivotCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AWBDocumentPivot(Factory);
		}

		public void TestCollectionAddNewWorksForNonPersistentBusinessObject()
		{
			var factory = new BusinessObjectFactory();
			var collection = new AWBDocumentPivotCollection(factory);
			var pivot = collection.AddNew();
			AssertEquals("Pivot", "", pivot.Title);

			var newPivot = (AWBDocumentPivot)((System.ComponentModel.IBindingList)collection).AddNew();
			AssertEquals("Pivot", "", newPivot.Title);
		}

		public void TestFromAndToXmlArray()
		{
			var factory = new BusinessObjectFactory();
			var collection = new AWBDocumentPivotCollection(factory);

			var pivot1 = collection.AddNew();
			pivot1.Title = "Title1";
			pivot1.Name = "Name1";
			pivot1.Printed = true;

			var pivot2 = collection.AddNew();
			pivot2.Title = "Title2";
			pivot2.Name = "Name2";
			pivot2.Printed = false;

			var pivot3 = collection.AddNew();
			pivot3.Title = "Title3";
			pivot3.Name = "Name3";
			pivot3.Printed = true;

			AssertEquals("Count", 3, collection.Count);

			var xmlArray = collection.ToXmlArray();

			var collection2 = new AWBDocumentPivotCollection(new BusinessObjectFactory());
			collection2.LoadFromXmlArray(xmlArray);
			AssertEquals("Count", 3, collection2.Count);

			AssertEquals("Collection2[0].Title", "Title1", collection2[0].Title);
			AssertEquals("Collection2[0].Name", "Name1", collection2[0].Name);
			AssertEquals("Collection2[0].Printed", ZBool.True, collection2[0].Printed);

			AssertEquals("Collection2[1].Title", "Title2", collection2[1].Title);
			AssertEquals("Collection2[1].Name", "Name2", collection2[1].Name);
			AssertEquals("Collection2[1].Printed", ZBool.False, collection2[1].Printed);

			AssertEquals("Collection2[2].Title", "Title3", collection2[2].Title);
			AssertEquals("Collection2[2].Name", "Name3", collection2[2].Name);
			AssertEquals("Collection2[2].Printed", ZBool.True, collection2[2].Printed);
		}
	}
}
