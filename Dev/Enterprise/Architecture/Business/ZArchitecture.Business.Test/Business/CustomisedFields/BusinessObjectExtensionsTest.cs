using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BusinessObjectExtensionsTest : TestCaseWithFactory
	{
		public void TestCustomFieldsHandling()
		{
			var dummyDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var customDummy = new DummyBusinessObjectWithCustomProperties(Factory, ((INeedRow)dummyDummy).Row);

			AssertEquals(true, customDummy.HasPossiblyCustomProperty("STR"));
			AssertEquals(true, customDummy.HasPossiblyCustomProperty("BOO"));
			AssertEquals(true, customDummy.HasPossiblyCustomProperty("DAT"));
			AssertEquals(true, customDummy.HasPossiblyCustomProperty("INT"));
			AssertEquals(false, customDummy.HasPossiblyCustomProperty("???"));

			AssertEquals(new ZString("String"), customDummy.GetPossiblyCustomProperty("STR"));
			AssertEquals(new ZBool(true), customDummy.GetPossiblyCustomProperty("BOO"));
			AssertEquals(new ZDateTime(2020, 1, 1), customDummy.GetPossiblyCustomProperty("DAT"));
			AssertEquals(new ZInt(6), customDummy.GetPossiblyCustomProperty("INT"));
			AssertEquals(null, customDummy.GetPossiblyCustomProperty("???"));

			AssertEquals(26, customDummy.GetPossiblyCustomPropertyMaxLength("STR"));
			AssertEquals(-1, customDummy.GetPossiblyCustomPropertyMaxLength("BOO"));
			AssertEquals(-1, customDummy.GetPossiblyCustomPropertyMaxLength("DAT"));
			AssertEquals(-1, customDummy.GetPossiblyCustomPropertyMaxLength("INT"));
			AssertEquals(-1, customDummy.GetPossiblyCustomPropertyMaxLength("???"));

			AssertEquals(false, customDummy.GetPossiblyCustomPropertyReadOnly("STR"));
			AssertEquals(false, customDummy.GetPossiblyCustomPropertyReadOnly("BOO"));
			AssertEquals(false, customDummy.GetPossiblyCustomPropertyReadOnly("DAT"));
			AssertEquals(false, customDummy.GetPossiblyCustomPropertyReadOnly("INT"));
			AssertEquals(true, customDummy.GetPossiblyCustomPropertyReadOnly("???"));

			customDummy.SetPossiblyCustomProperty("STR", new ZString("String!"));
			customDummy.SetPossiblyCustomProperty("BOO", new ZBool(false));
			customDummy.SetPossiblyCustomProperty("DAT", new ZDateTime(2021, 2, 2));
			customDummy.SetPossiblyCustomProperty("INT", new ZInt(8));

			AssertEquals(new ZString("String!"), customDummy.GetPossiblyCustomProperty("STR"));
			AssertEquals(new ZBool(false), customDummy.GetPossiblyCustomProperty("BOO"));
			AssertEquals(new ZDateTime(2021, 2, 2), customDummy.GetPossiblyCustomProperty("DAT"));
			AssertEquals(new ZInt(8), customDummy.GetPossiblyCustomProperty("INT"));
			AssertEquals(null, customDummy.GetPossiblyCustomProperty("???"));
		}
	}
}
