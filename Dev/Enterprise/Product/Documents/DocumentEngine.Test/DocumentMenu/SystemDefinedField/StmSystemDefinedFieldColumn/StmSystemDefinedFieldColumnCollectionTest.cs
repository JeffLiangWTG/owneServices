using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedFieldColumnCollection))]
	sealed class StmSystemDefinedFieldColumnCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionMembers()
		{
			ParentField.S1_BusinessContext = "Y";
			ParentField.S1_Order = 3;

			StmSystemDefinedFieldColumn fieldColumn1 = Factory.New<StmSystemDefinedFieldColumn>();
			StmSystemDefinedFieldColumn fieldColumn2 = Factory.New<StmSystemDefinedFieldColumn>();
			StmSystemDefinedFieldColumn fieldColumn3 = Factory.New<StmSystemDefinedFieldColumn>();
			StmSystemDefinedFieldColumn fieldColumn4 = Factory.New<StmSystemDefinedFieldColumn>();

			fieldColumn1.S1_Order = 3;
			fieldColumn2.S1_Order = 3;
			fieldColumn3.S1_Order = 3;
			fieldColumn4.S1_Order = 2;

			fieldColumn1.S1_OrderColumn = 1;
			fieldColumn2.S1_OrderColumn = 2;
			fieldColumn3.S1_OrderColumn = 3;
			fieldColumn4.S1_OrderColumn = 4;

			fieldColumn1.S1_BusinessContext = "Y";
			fieldColumn2.S1_BusinessContext = "Y";
			fieldColumn3.S1_BusinessContext = "N";
			fieldColumn4.S1_BusinessContext = "Y";

			Collection.Load();

			AssertEquals("Count", 2, Collection.Count);
			AssertEquals("Contains(FieldColumn1)", true, Collection.Contains(fieldColumn1));
			AssertEquals("Contains(FieldColumn2)", true, Collection.Contains(fieldColumn2));
		}

		public void TestDefaultsForNewChild()
		{
			ParentField.S1_BusinessContext = "Avocado";
			ParentField.S1_Order = 123;

			StmSystemDefinedFieldColumn fieldColumn1 = Collection.AddNew();
			AssertEquals("FieldColumn1.S1_Order", (short)123, fieldColumn1.S1_Order);
			AssertEquals("FieldColumn1.S1_BusinessContext", "Avocado", fieldColumn1.S1_BusinessContext);
			AssertEquals("FieldColumn1.S1_OrderColumn", (short)1, fieldColumn1.S1_OrderColumn);

			StmSystemDefinedFieldColumn fieldColumn2 = Collection.AddNew();
			AssertEquals("FieldColumn2.S1_Order", (short)123, fieldColumn2.S1_Order);
			AssertEquals("FieldColumn2.S1_BusinessContext", "Avocado", fieldColumn2.S1_BusinessContext);
			AssertEquals("FieldColumn2.S1_OrderColumn", (short)2, fieldColumn2.S1_OrderColumn);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmSystemDefinedFieldColumnCollection(ParentField, Factory);
		}

		new StmSystemDefinedFieldColumnCollection Collection
		{
			get { return (StmSystemDefinedFieldColumnCollection)base.Collection; }
		}

		StmSystemDefinedField ParentField
		{
			get
			{
				if (fParentField == null)
				{
					fParentField = Factory.New<StmSystemDefinedField>();
				}
				return fParentField;
			}
		}

		StmSystemDefinedField fParentField;

		#endregion
	}
}
