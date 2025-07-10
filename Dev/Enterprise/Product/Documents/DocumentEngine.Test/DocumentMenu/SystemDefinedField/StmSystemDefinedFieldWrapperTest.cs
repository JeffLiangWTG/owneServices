using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedFieldWrapper))]
	sealed class StmSystemDefinedFieldWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSchema()
		{
			StmSystemDefinedField field = Factory.New<StmSystemDefinedField>();
			field.S1_Category = "CATEGORY";
			field.S1_Hint = "HINT";
			field.S1_Name = "NAME";
			field.S1_Order = 12;
			field.S1_Type = "TYP";
			StmSystemDefinedFieldWrapper wrapperField = new StmSystemDefinedFieldWrapper(field);
			wrapperField.S1_Value = "VALUE";
			AssertEquals("CATEGORY", wrapperField[StmSystemDefinedFieldWrapper.Schema.S1_Category]);
			AssertEquals("HINT", wrapperField[StmSystemDefinedFieldWrapper.Schema.S1_Hint]);
			AssertEquals("NAME", wrapperField[StmSystemDefinedFieldWrapper.Schema.S1_Name]);
			AssertEquals((ZShort)12, wrapperField[StmSystemDefinedFieldWrapper.Schema.S1_Order]);
			AssertEquals("TYP", wrapperField[StmSystemDefinedFieldWrapper.Schema.S1_Type]);
			AssertEquals("VALUE", wrapperField[StmSystemDefinedFieldWrapper.Schema.S1_Value]);
		}

		public void TestWrapper()
		{
			StmSystemDefinedField field1 = Factory.New<StmSystemDefinedField>();
			field1.S1_Name = "Field1";
			field1.S1_Order = 2;
			field1.S1_Category = "Shipment";

			StmSystemDefinedFieldWrapper wrapperField1 = new StmSystemDefinedFieldWrapper(field1);

			wrapperField1.S1_Value = "TestValue";
			AssertEquals("S1_Value", "TestValue", wrapperField1.S1_Value);

			AssertEquals("S1_Order", 2, (int)wrapperField1.S1_Order);
			AssertEquals("S1_NameFromDatabase", "Field1", wrapperField1.S1_NameFromDatabase);
			AssertEquals("S1_Category", "Shipment", wrapperField1.S1_Category);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory) => Factory.NewWithValidTestData<StmSystemDefinedField>();

		protected override BusinessObject GetNewBusinessObject()
		{
			var field = Factory.New<StmSystemDefinedField>();
			var wrapper = new StmSystemDefinedFieldWrapper(field);
			return wrapper;
		}
	}
}
