using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.DataProviders.Testing
{
	sealed class BusinessObjectFilterDataSourceTest : TestCaseWithFactory
	{
		public void TestWithParameterThatEndsInABackSlash()
		{
			const string statement = "\"<Z0_VarCharMax>\"!=\"\"";
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo.Z0_VarCharMax = @"End In Slash \";

			AssertEquals(true, new BusinessObjectFilterDataSource(bizo, statement).Evaluate());
		}

		public void TestWithParameterThatContainsAngularBrackets()
		{
			const string statement = "\"<Z0_VarCharMax>\"!=\"\"";
			var bizo = Factory.NewWithValidTestData<DummyBusinessObject>();
			bizo.Z0_VarCharMax = @"<Contains Angular Brackets>";

			AssertEquals(true, new BusinessObjectFilterDataSource(bizo, statement).Evaluate());
		}

		public void TestTextWithDoubleQuotes()
		{
			DummyBusinessObjectCollection boc = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bo = boc.AddNew();
			bo.Z0_VarCharMax = "Jimmy \"The Fish\" Soprano";

			AssertEquals(false, new BusinessObjectFilterDataSource(boc[0], "\"<Z0_VarCharMax>\"==\"\"").Evaluate());
			AssertEquals(true, new BusinessObjectFilterDataSource(boc[0], "\"<Z0_VarCharMax>\"==\"Jimmy \\\"The Fish\\\" Soprano\"").Evaluate());
		}

		public void TestTextWithFullStopsInQuotations()
		{
			DummyBusinessObjectCollection boc = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bo = boc.AddNew();
			bo.Z0_VarCharMax = "Blah. Foobar. Testing abbrev. acro. that should work.";

			AssertEquals(true, new BusinessObjectFilterDataSource(boc[0], "\"<Z0_VarCharMax>\"==\"Blah. Foobar. Testing abbrev. acro. that should work.\"").Evaluate());
		}

		public void TestTextWithChildBusinessObjects()
		{
			var boc = new DummyBusinessObjectCollection(Factory);
			var bo = boc.AddNew();

			bo.Self.Self.Z0_VarCharMax = "tom";
			AssertEquals(true, new BusinessObjectFilterDataSource(boc[0], "\"<Self.Self.Z0_VarCharMax>\" == \"tom\"").Evaluate());

			bo.Self.Self.Z0_VarCharMax = null;
			AssertEquals(true, new BusinessObjectFilterDataSource(boc[0], "\"<Self.Self.Z0_VarCharMax>\" == \"\"").Evaluate());
		}

		public void TestInequalities()
		{
			var boc = new DummyBusinessObjectCollection(Factory);
			var bo = boc.AddNew();
			bo.Z0_Number = 2;
			AssertEquals(true, new BusinessObjectFilterDataSource(boc[0], "\"<Z0_Number>\" > 1").Evaluate());
			AssertEquals(false, new BusinessObjectFilterDataSource(boc[0], "\"<Z0_Number>\" > 4").Evaluate());
		}

		public void TestDecimalValues()
		{
			DummyBusinessObjectCollection boc = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bo = boc.AddNew();
			bo.Z0_AnotherDecimal = 3.14m;

			AssertEquals(true, new BusinessObjectFilterDataSource(boc[0], "<Z0_AnotherDecimal>==3.14").Evaluate());
		}

		public void TestMultipleConditions()
		{
			DummyBusinessObjectCollection boc = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bo = boc.AddNew();
			bo.Z0_Bool = true;
			bo.Z0_VarCharMax = "blah";

			AssertEquals(false, new BusinessObjectFilterDataSource(boc[0], "<Z0_Bool>==true && \"<Z0_VarCharMax>\"!=\"blah\"").Evaluate());
		}

		public void TestRegexOnDataSourceFilter()
		{
			DummyBusinessObjectCollection boc = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bo = boc.AddNew();
			bo.Z0_Number = 3;
			bo.Z0_VarCharMax = "blah";
			AssertEquals(true, new BusinessObjectFilterDataSource(boc[0], "\"<Z0_VarCharMax>\"  == \"blah\" && <Z0_Number> == 3").Evaluate());
		}

		public void TestANDsAndORs()
		{
			DummyBusinessObjectCollection boc = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bo = boc.AddNew();
			bo.Z0_Number = 3;
			bo.Z0_VarCharMax = "blah";

			bo = boc.AddNew();
			bo.Z0_Number = 0;
			bo.Z0_VarCharMax = "blah";

			bo = boc.AddNew();
			bo.Z0_Number = 7;
			bo.Z0_VarCharMax = "blah";

			bo = boc.AddNew();
			bo.Z0_Number = 0;
			bo.Z0_VarCharMax = "tom";
			AssertEquals(true, new BusinessObjectFilterDataSource(boc[0], "(\"<Z0_VarCharMax>\"  == \"blah\" && <Z0_Number> == 3) || <Z0_Number>==7").Evaluate());
		}

		public void TestANDsAndORsExtended()
		{
			DummyBusinessObjectCollection boc = new DummyBusinessObjectCollection(Factory);
			DummyBusinessObject bo = boc.AddNew();
			bo.Z0_Number = 3;
			bo.Z0_VarCharMax = "blah";

			bo = boc.AddNew();
			bo.Z0_Number = 0;
			bo.Z0_VarCharMax = "blah";

			bo = boc.AddNew();
			bo.Z0_Number = 7;
			bo.Z0_VarCharMax = "blah";

			bo = boc.AddNew();
			bo.Z0_Number = 0;
			bo.Z0_VarCharMax = "tom";
			AssertEquals(true, new BusinessObjectFilterDataSource(boc[0], "(\"<Z0_VarCharMax>\" == \"tom\" || \"<Z0_VarCharMax>\"  == \"blah\") && (<Z0_Number> == 3 || <Z0_Number>==0)").Evaluate());
		}
	}
}
