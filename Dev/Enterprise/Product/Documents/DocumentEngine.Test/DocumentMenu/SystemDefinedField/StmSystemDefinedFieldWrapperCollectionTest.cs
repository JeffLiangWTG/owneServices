using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedFieldWrapperCollection))]
	sealed class StmSystemDefinedFieldWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StmSystemDefinedFieldWrapperCollection>
	{
		public void TestCollection()
		{
			StmSystemDefinedFieldWrapperCollection collection = new StmSystemDefinedFieldWrapperCollection();

			AssertEquals("AllowNew", false, collection.AllowNew);

			StmSystemDefinedFieldCollection shipmentSDFCollection = new StmSystemDefinedFieldCollection(Factory);

			StmSystemDefinedField shipmentField1 = shipmentSDFCollection.AddNew();
			shipmentField1.S1_Name = "ShipmentField1";
			StmSystemDefinedFieldWrapper shipmentField1Wrapper = new StmSystemDefinedFieldWrapper(shipmentField1);

			StmSystemDefinedField shipmentField2 = shipmentSDFCollection.AddNew();
			shipmentField2.S1_Name = "ShipmentField2";
			StmSystemDefinedFieldWrapper shipmentField2Wrapper = new StmSystemDefinedFieldWrapper(shipmentField2);

			StmSystemDefinedField shipmentField3 = shipmentSDFCollection.AddNew();
			shipmentField3.S1_Name = "ShipmentField3";
			StmSystemDefinedFieldWrapper shipmentField3Wrapper = new StmSystemDefinedFieldWrapper(shipmentField3);

			collection.Add(shipmentField1Wrapper);
			collection.Add(shipmentField2Wrapper);
			collection.Add(shipmentField3Wrapper);

			AssertEquals("Item 1 Name", "ShipmentField1", collection[0].S1_NameFromDatabase);
			AssertEquals("Item 2 Name", "ShipmentField2", collection[1].S1_NameFromDatabase);
			AssertEquals("Item 3 Name", "ShipmentField3", collection[2].S1_NameFromDatabase);
		}

		protected override StmSystemDefinedFieldWrapperCollection GetCollectionToTest()
		{
			return new StmSystemDefinedFieldWrapperCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			StmSystemDefinedField field = Factory.New<StmSystemDefinedField>();
			StmSystemDefinedFieldWrapper wrapper = new StmSystemDefinedFieldWrapper(field);
			return wrapper;
		}
	}
}
