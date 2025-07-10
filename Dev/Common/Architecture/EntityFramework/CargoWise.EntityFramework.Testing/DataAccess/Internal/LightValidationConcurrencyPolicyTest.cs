using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class LightValidationConcurrencyPolicyTest : TestCase
	{
		public void TestShouldCheck()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };

			var dummy1 = factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy1Light = (ILightValidationInternals)dummy1;
			var dummy1Row = dummy1.Row;
			var lightValidationColumn = dummy1Row.Table.Columns[dummy1Light.IsValidSchemaColumn.Name];

			dummy1Light.IsValid = true;
			dummy1.Row.AcceptChanges();
			dummy1Light.IsValid = false;

			AssertEquals("No need to check concurrency when setting IsValid to false", false, LightValidationConcurrencyPolicy.Instance.ShouldCheck(dummy1Row, lightValidationColumn));

			dummy1.Row.AcceptChanges();
			dummy1Light.IsValid = true;

			AssertEquals("Should check concurrency when setting IsValid to true", true, LightValidationConcurrencyPolicy.Instance.ShouldCheck(dummy1Row, lightValidationColumn));

			dummy1.Row.AcceptChanges();

			AssertEquals("No need to check concurrency on unmodified row", false, LightValidationConcurrencyPolicy.Instance.ShouldCheck(dummy1Row, lightValidationColumn));
		}

		public void TestAllowMergeForConcurrencyCheck()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };

			var dummy1 = factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy1Light = (ILightValidationInternals)dummy1;
			var dummy1Row = dummy1.Row;
			var lightValidationColumn = dummy1Row.Table.Columns[dummy1Light.IsValidSchemaColumn.Name];

			dummy1Light.IsValid = false;
			dummy1.Row.AcceptChanges();
			dummy1Light.IsValid = true;

			AssertEquals("Allow automatic merge of IsValid column in WHERE clause when only light validation is changed",
				true, LightValidationConcurrencyPolicy.Instance.AllowAutomaticMergeIfDatabaseValuesAreEqual(dummy1Row, lightValidationColumn));

			dummy1.Z0_Number += 1;

			AssertEquals("Do not allow automatic merge of IsValid column in WHERE clause when there are other changes",
				false, LightValidationConcurrencyPolicy.Instance.AllowAutomaticMergeIfDatabaseValuesAreEqual(dummy1Row, lightValidationColumn));
		}
	}
}
