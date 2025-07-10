using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class FindBoxListProviderTest : TestCaseWithDummy
	{
		public void TestCodeDescriptionFromCustomProperties()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			var findBoxProvider = new FindBoxListProviderWithCustomAndDescriptionCodeProperty(collection);
			findBoxProvider.CodePropertyName = DummyBizoSchema.Constants.Z0_VarCharMax;
			findBoxProvider.DescriptionPropertyName = DummyBizoSchema.Constants.Z0_NVarCharMax;

			var bizo = collection.AddNew();
			bizo.Z0_VarCharMax = "Some code";
			bizo.Z0_NVarCharMax = "Some description";

			var codeDescription = findBoxProvider.GetCustomCodeDescription(bizo);
			AssertEquals("The CodeDescription should use the given CodePropertyName value", bizo.Z0_VarCharMax, codeDescription.Code);
			AssertEquals("The CodeDescription should use the given DescriptionPropertyName value", bizo.Z0_NVarCharMax, codeDescription.Description);
		}

		public void TestCodeDescriptionFromCustomProperties_NullValueUsesBizoValues()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			var findBoxProvider = new FindBoxListProviderWithCustomAndDescriptionCodeProperty(collection);
			findBoxProvider.CodePropertyName = null;
			findBoxProvider.DescriptionPropertyName = null;

			var bizo = collection.AddNew();
			bizo.Z0_Code = "DACOD";
			bizo.Z0_Description = "Some description";

			var codeDescription = findBoxProvider.GetCustomCodeDescription(bizo);
			AssertEquals("The CodeDescription should fallback to the default CodePropertyName value when the custom property is null", bizo.Z0_Code, codeDescription.Code);
			AssertEquals("The CodeDescription should fallback to the default DescriptionPropertyName value when the custom property is null", bizo.Z0_Description, codeDescription.Description);
		}

		public void TestSearchOnNonPersistentCodeAttribute()
		{
			FindBoxListProviderForTest prov = new FindBoxListProviderForTest(new DummyBusinessObjectWithCalculatedCodePropertyCollection(Factory));
			prov.NearestMatch("AAA", true, -1);
			AssertEquals(0, Factory.DatabaseLoadCount);
		}

		public void TestSearchOnNonPersistentDescriptionAttribute()
		{
			FindBoxListProviderForTest prov = new FindBoxListProviderForTest(new DummyBusinessObjectWithCalculatedCodePropertyCollection(Factory));
			prov.NearestDescriptionMatch("AAA", true);
			AssertEquals(0, Factory.DatabaseLoadCount);
		}

		public void TestNoNullsOnlyEmptyEnumerables()
		{
			FindBoxListProviderForTest prov = new FindBoxListProviderForTest(new DummyBusinessObjectCollection(Factory));
			AssertEquals(0, prov.GetBusinessObjectsFromCode("").Count());
			AssertEquals(0, prov.GetBusinessObjectsFromCodeWithoutFilter("").Count());
		}

		public void TestActiveCancelledFilter()
		{
			FindBoxListProviderForTest prov = new FindBoxListProviderForTest(new DummyBusinessObjectWithActivePropertyCollection(Factory));
			prov.NearestMatch("AAA", true, -1);
			Assert("query should contain clause for IsActive = true", prov.LastIsActiveQuery.LiteralTextSql.Contains("Z0_Bool = 'Y'"));

			prov = new FindBoxListProviderForTest(new DummyBusinessObjectWithCancelledPropertyCollection(Factory));
			prov.NearestMatch("AAA", true, -1);
			Assert("query should contain clause for IsCancelled = false", prov.LastIsActiveQuery.LiteralTextSql.Contains("Z0_Bool = 'N'"));
		}

		public void TestAutoCompleteOnCommit()
		{
			FindBoxListProvider provider = new FindBoxListProvider(new DummyBusinessObjectCollection(Factory));
			AssertEquals("By default, autocomplete on commit should be off", false, provider.AutoCompleteOnCommit);
		}

		public void TestGetBusinessObjectsFromCode()
		{
			FindBoxListProvider provider = new FindBoxListProvider(new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "ZAYDEN")));

			DummyBusinessObject dummyZub = Factory.New<DummyBusinessObject>();
			dummyZub.Z0_Code = "ZUB";
			dummyZub.Z0_Description = "ZUBIN";

			DummyBusinessObject dummyZay1 = Factory.New<DummyBusinessObject>();
			dummyZay1.Z0_Code = "ZAY";
			dummyZay1.Z0_Description = "ZAYDEN2";

			DummyBusinessObject dummyZay2 = Factory.New<DummyBusinessObject>();
			dummyZay2.Z0_Code = "ZAY";
			dummyZay2.Z0_Description = "ZAYDEN";

			AssertEquals("Matches Filter", dummyZay2.PK, provider.GetBusinessObjectFromCode("ZAY").PK);
			AssertEquals("Fallback to outside filter", dummyZub.PK, provider.GetBusinessObjectFromCode("ZUB").PK);
			AssertNull("Non-existent item", provider.GetBusinessObjectFromCode("CAR"));
			AssertEquals("Only one row matched", 1, provider.GetBusinessObjectsFromCode("ZAY").Count());

			DummyBusinessObject dummyZay3 = Factory.New<DummyBusinessObject>();
			dummyZay3.Z0_Code = "ZAY";
			dummyZay3.Z0_Description = "ZAYDEN";

			AssertEquals("Multiple rows match", 2, provider.GetBusinessObjectsFromCode("ZAY").Count());
		}

		public void TestGetBusinessObjectsFromCodeWithRelationship()
		{
			FindBoxListProvider provider = new FindBoxListProvider(new DummyRelationshipBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "VLAD")));

			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "IG";
			dummy1.Z0_Description = "THERE ARE TOO MANY ZUBS IN TESTS";
			dummy1.Z0_Number = 1;

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "VL";
			dummy2.Z0_Description = "VLAD";
			dummy2.Z0_Number = 1;

			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Code = "VLA";
			dummy3.Z0_Description = "VLAD";

			AssertEquals("Matches Filter", dummy2.PK, provider.GetBusinessObjectFromCode("VL").PK);
			AssertEquals("Fallback to relationship filter", dummy1.PK, provider.GetBusinessObjectFromCode("IG").PK);
			AssertNull("Not found by relationship filter", provider.GetBusinessObjectFromCode("VLA"));
			AssertNull("Non-existent item", provider.GetBusinessObjectFromCode("ZUB"));
		}

		public void TestGetBusinessObjectsFromCodeWithoutFilter()
		{
			FindBoxListProvider provider = new FindBoxListProvider(new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "ZAYDEN")));

			DummyBusinessObject dummyZub = Factory.New<DummyBusinessObject>();
			dummyZub.Z0_Code = "ZUB";
			dummyZub.Z0_Description = "ZUBIN";

			AssertEquals("Matches Filter", dummyZub.PK, provider.GetBusinessObjectFromCodeWithoutFilter("ZUB").PK);
			AssertNull("Non-existent item", provider.GetBusinessObjectFromCodeWithoutFilter("CAR"));
		}

		public void TestDescriptionFromPrimaryKey()
		{
			FindBoxListProvider provider = new FindBoxListProvider(new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "BBBBBBB")));

			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "AAA";
			dummy1.Z0_Description = "BBBBBBB";

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "CCC";
			dummy2.Z0_Description = "DDDDDD";

			AssertEquals("Matches PK", dummy2.Z0_Description, provider.DescriptionFromPrimaryKey(dummy2.PK));
			AssertNull("Non-existent item", provider.DescriptionFromPrimaryKey(ZGuid.NewZGuid()));
			AssertNull("Invalid PK", provider.DescriptionFromPrimaryKey(ZGuid.Invalid));
		}

		public void TestCompositeCollections()
		{
			FindBoxListProvider provider = new FindBoxListProvider(new CompositeCollectionTest(Factory));

			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "TEA";
			dummy1.Z0_Description = "TEA POTS AND PANS";

			DummyChildBusinessObject dummy2 = Factory.New<DummyChildBusinessObject>();
			dummy2.Z0_Code = "SNO";
			dummy2.Z0_Description = "SNOW WHITE";

			AssertEquals("Correct description from code", dummy1.Z0_Description, provider.DescriptionFromCode("TEA"));
			AssertEquals("Correct description from code", dummy2.Z0_Description, provider.DescriptionFromCode("SNO"));

			AssertEquals("Correct description from PK", dummy1.Z0_Description, provider.DescriptionFromPrimaryKey(dummy1.PK));
			AssertEquals("Correct description from PK", dummy2.Z0_Description, provider.DescriptionFromPrimaryKey(dummy2.PK));
		}

		[ExpectNoExceptions]
		public void TestNearestMatchNotBlowUpOnCalculatedProperties()
		{
			FindBoxListProvider provider = new FindBoxListProvider(new DummyBusinessObjectWithCalculatedCodePropertyCollection(Factory));

			DummyBusinessObjectWithCalculatedCodeProperty dummy1 = Factory.New<DummyBusinessObjectWithCalculatedCodeProperty>();
			dummy1.TestCodeProperty = "TST";
			dummy1.Z0_Description = "New description";

			string returnValue = provider.NearestMatch("TST", true, -1).Item1;

			AssertNotNull("Can't retrieve a code from a calculated property - but at least it didn't blow up", returnValue);
		}

		public void TestAlternateKeyFromPrimaryKeyWithCalculatedProperty()
		{
			FindBoxListProvider provider = new FindBoxListProvider(new DummyBusinessObjectWithCalculatedCodePropertyCollection(Factory));

			DummyBusinessObjectWithCalculatedCodeProperty dummy1 = Factory.New<DummyBusinessObjectWithCalculatedCodeProperty>();
			dummy1.TestCodeProperty = "TST";
			dummy1.Z0_Description = "New description";

			AssertEquals("TST", provider.AlternateKeyFromPrimaryKey("TestCodeProperty", dummy1.PK));
			Assert(String.IsNullOrEmpty(ErrorReporter.LastKeyReported));
		}

		public void TestPrimaryKeyFromAlternateKeyWithCalculatedProperty()
		{
			var provider = new FindBoxListProvider(new DummyBusinessObjectWithCalculatedDescriptionPropertyCollection(Factory));
			var descriptionPropertyName = "TestDescriptionProperty";
			var dummy = Factory.NewWithValidTestData<DummyBusinessObjectWithCalculatedDescriptionProperty>();
			dummy.TestDescriptionProperty = "CAT";
			dummy.Z0_Description = "Calico Cat";

			CombineAssertions("Precondition: An AlternateKey should exist.", () =>
			{
				AssertEquals(1, provider.AlternateKeys.Count);
				AssertEquals("Description", provider.AlternateKeys[0].ColumnDescription);
				AssertEquals(descriptionPropertyName, provider.AlternateKeys[0].ColumnName);
				AssertEquals(SchemaColumnType.String, provider.AlternateKeys[0].ColumnType);
			});

			AssertEquals("Guid should be empty.", Guid.Empty, provider.PrimaryKeyFromAlternateKey(descriptionPropertyName, new ZString("Calico Cat")));
			AssertEquals("Developer error should have been reported.", $"IFindBoxListProviderEx.PrimaryKeyFromAlternateKey: columnName='{descriptionPropertyName}' from tableName='{dummy.TableName}' was not found.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestPrimaryKeyFromAlternateKeyWithMultilingualStringProperty_WithValidValue()
		{
			var provider = new FindBoxListProvider(new DummyBusinessObjectWithMultilingualStringPropertyCollection(Factory));
			var descriptionPropertyName = "Z0_DescriptionMultilingual";
			var dummy = Factory.NewWithValidTestData<DummyBusinessObjectWithMultilingualStringProperty>();
			dummy.Z0_Description = "Cloudy";

			CombineAssertions("Precondition: An AlternateKey should exist.", () =>
			{
				AssertEquals(1, provider.AlternateKeys.Count);
				AssertEquals("Description", provider.AlternateKeys[0].ColumnDescription);
				AssertEquals(descriptionPropertyName, provider.AlternateKeys[0].ColumnName);
				AssertEquals(SchemaColumnType.String, provider.AlternateKeys[0].ColumnType);
			});

			AssertEquals("Guid should be valid value.", dummy.PK, provider.PrimaryKeyFromAlternateKey(descriptionPropertyName, new ZString("Cloudy")));
		}

		[ExpectNoExceptions]
		public void TestPrimaryKeyFromAlternateKeyWithMultilingualStringProperty_WithInvalidValue()
		{
			var provider = new FindBoxListProvider(new DummyBusinessObjectWithMultilingualStringPropertyCollection(Factory));
			var descriptionPropertyName = "Z0_DescriptionMultilingual";
			var dummy = Factory.NewWithValidTestData<DummyBusinessObjectWithMultilingualStringProperty>();
			dummy.Z0_Description = "Cloudy";

			CombineAssertions("Precondition: An AlternateKey should exist.", () =>
			{
				AssertEquals(1, provider.AlternateKeys.Count);
				AssertEquals("Description", provider.AlternateKeys[0].ColumnDescription);
				AssertEquals(descriptionPropertyName, provider.AlternateKeys[0].ColumnName);
				AssertEquals(SchemaColumnType.String, provider.AlternateKeys[0].ColumnType);
			});

			AssertEquals("Guid should be empty.", ZGuid.Invalid, provider.PrimaryKeyFromAlternateKey(descriptionPropertyName, new ZString("Sunny")));
		}

		public void TestIFindBoxListProviderEx()
		{
			IFindBoxListProviderEx findBoxListProviderEx = new FindBoxListProvider(new DummyBusinessObjectCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotEqual, "HEN")));
			AssertEquals(1, findBoxListProviderEx.AlternateKeys.Count);
			AssertEquals(DummyBizoSchema.Z0_Description.Name, findBoxListProviderEx.AlternateKeys[0].ColumnName);
			AssertEquals(SchemaColumnType.String, findBoxListProviderEx.AlternateKeys[0].ColumnType);
			AssertEquals("Description", findBoxListProviderEx.AlternateKeys[0].ColumnDescription);

			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "CLI";
			dummy1.Z0_Description = "CLINTON";

			DummyChildBusinessObject dummy2 = Factory.New<DummyChildBusinessObject>();
			dummy2.Z0_Code = "ZUB";
			dummy2.Z0_Description = "ZUBIN";

			DummyChildBusinessObject dummy3 = Factory.New<DummyChildBusinessObject>();
			dummy3.Z0_Code = "HEN";
			dummy3.Z0_Description = "HENRY";

			DummyChildBusinessObject dummy4 = Factory.New<DummyChildBusinessObject>();
			dummy4.Z0_Code = "ZU2";
			dummy4.Z0_Description = "ZUBIN";

			AssertEquals(dummy1.PK, findBoxListProviderEx.PrimaryKeyFromAlternateKey(DummyBizoSchema.Z0_Description.Name, (ZString)"CLINTON"));
			AssertEquals(ZGuid.Invalid, findBoxListProviderEx.PrimaryKeyFromAlternateKey(DummyBizoSchema.Z0_Description.Name, (ZString)"ZUBIN"));
			AssertEquals(ZGuid.Missing, findBoxListProviderEx.PrimaryKeyFromAlternateKey(DummyBizoSchema.Z0_Description.Name, (ZString)"HENRY"));

			AssertEquals("CLINTON", findBoxListProviderEx.AlternateKeyFromPrimaryKey(DummyBizoSchema.Z0_Description.Name, dummy1.PK));
			AssertEquals("ZUBIN", findBoxListProviderEx.AlternateKeyFromPrimaryKey(DummyBizoSchema.Z0_Description.Name, dummy2.PK));
			AssertEquals("HENRY", findBoxListProviderEx.AlternateKeyFromPrimaryKey(DummyBizoSchema.Z0_Description.Name, dummy3.PK));
			AssertEquals("ZUBIN", findBoxListProviderEx.AlternateKeyFromPrimaryKey(DummyBizoSchema.Z0_Description.Name, dummy4.PK));
			AssertNull(findBoxListProviderEx.AlternateKeyFromPrimaryKey(DummyBizoSchema.Z0_Description.Name, ZGuid.NewZGuid()));
			AssertNull(findBoxListProviderEx.AlternateKeyFromPrimaryKey(DummyBizoSchema.Z0_Description.Name, ZGuid.Empty));
			AssertNull(findBoxListProviderEx.AlternateKeyFromPrimaryKey(DummyBizoSchema.Z0_Description.Name, ZGuid.Invalid));

			AssertNull(findBoxListProviderEx.AlternateKeyFromPrimaryKey("!@#", dummy1.PK));
			AssertEquals("IFindBoxListProviderEx.PrimaryKeyFromAlternateKey: !@# was not found.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestNearestDescriptionMatch()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "CLI";
			dummy1.Z0_Description = "CLINTON";

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "ZU1";
			dummy2.Z0_Description = "ZUBIN";

			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Code = "ZU2";
			dummy3.Z0_Description = "ZUBIN";

			FindBoxListProvider findBoxListProvider = new FindBoxListProvider(new DummyBusinessObjectCollection(Factory));

			AssertEquals("CLINTON", findBoxListProvider.NearestDescriptionMatch("C", true));
			AssertEquals("CLINTON", findBoxListProvider.NearestDescriptionMatch("C", false));
			AssertEquals("CLINTON", findBoxListProvider.NearestDescriptionMatch("CLINTON", true));
			AssertEquals("CLINTON", findBoxListProvider.NearestDescriptionMatch("clinton", true));
			AssertEquals("ZUBIN", findBoxListProvider.NearestDescriptionMatch("Z", true));
			AssertEquals("ZUBIN", findBoxListProvider.NearestDescriptionMatch("zub", true));
			AssertEquals("X", findBoxListProvider.NearestDescriptionMatch("X", true));
			AssertEquals("CLINTON1", findBoxListProvider.NearestDescriptionMatch("CLINTON1", true));
		}

		public void TestCodeFromDescription()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "CLI";
			dummy1.Z0_Description = "CLINTON";

			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "ZU1";
			dummy2.Z0_Description = "ZUBIN";

			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Code = "ZU2";
			dummy3.Z0_Description = "ZUBIN";

			FindBoxListProvider findBoxListProvider = new FindBoxListProvider(new DummyBusinessObjectCollection(Factory));

			AssertEquals("CLI", findBoxListProvider.CodeFromDescription("CLINTON"));
			AssertEquals("CLI", findBoxListProvider.CodeFromDescription("clinton"));
			AssertNull(findBoxListProvider.CodeFromDescription("CLINT"));
			AssertEquals("ZU1", findBoxListProvider.CodeFromDescription("ZUBIN"));
		}

		public void TestCodeFromPrimaryKey()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			var findBoxListProvider = new FindBoxListProviderWithCustomAndDescriptionCodeProperty(new DummyBusinessObjectCollection(Factory)) { CodePropertyName = null };
			AssertNoExceptionThrown(() => findBoxListProvider.CodeFromPrimaryKey(dummy1.PK));
		}

		protected override void SetUp()
		{
			// We want to ensure the ProcessFieldChangeRules are pre-loaded in the UberCache so that HitCounts are accurate
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();
		}
	}
}
