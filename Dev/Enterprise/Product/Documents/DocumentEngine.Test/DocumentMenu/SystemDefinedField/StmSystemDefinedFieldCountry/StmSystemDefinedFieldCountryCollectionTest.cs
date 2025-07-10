using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedFieldCountryCollection))]
	sealed class StmSystemDefinedFieldCountryCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionMembers()
		{
			StmSystemDefinedFieldCountry fieldCountry1 = Factory.New<StmSystemDefinedFieldCountry>();
			StmSystemDefinedFieldCountry fieldCountry2 = Factory.New<StmSystemDefinedFieldCountry>();
			StmSystemDefinedFieldCountry fieldCountry3 = Factory.New<StmSystemDefinedFieldCountry>();
			StmSystemDefinedField emptyField = Factory.New<StmSystemDefinedField>();

			Parent.S1_Name = "Name1";
			fieldCountry1.S1_Name = "Name1";
			fieldCountry2.S1_Name = "Name1";
			fieldCountry3.S1_Name = "Name2";

			Parent.S1_BusinessContext = "Context1";
			fieldCountry1.S1_BusinessContext = "Context1";
			fieldCountry2.S1_BusinessContext = "Context2";
			fieldCountry3.S1_BusinessContext = "Context3";

			Collection.Load();
			AssertEquals("Count", 1, Collection.Count);
			AssertEquals("Contains(FieldCountry1)", true, Collection.Contains(fieldCountry1));

			Parent.S1_Name = "";
			Parent.S1_BusinessContext = "";
			Collection.Load();
			AssertEquals("Count", 0, Collection.Count);
		}

		public void TestDefaultsForNewChild()
		{
			Parent.S1_Name = "Quirky";
			Parent.S1_BusinessContext = "Context";

			StmSystemDefinedFieldCountry fieldCountry = Collection.AddNew();
			AssertEquals("Collection.AddNew().S1_Name", "Quirky", fieldCountry.S1_Name);
			AssertEquals("Collection.AddNew().S1_BusinessContext", "Context", fieldCountry.S1_BusinessContext);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmSystemDefinedFieldCountryCollection(Parent, Factory);
		}

		new StmSystemDefinedFieldCountryCollection Collection
		{
			get { return (StmSystemDefinedFieldCountryCollection)base.Collection; }
		}

		StmSystemDefinedField Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = Factory.New<StmSystemDefinedField>();
				}
				return fParent;
			}
		}

		StmSystemDefinedField fParent;

		#endregion
	}
}
