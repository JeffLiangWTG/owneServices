using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ZLimitedColumnsProviderTest : TestCase
	{
		public void TestWhenThereIsNotValidColumnSchemaForProvidedCodeProperty()
		{
			var limitedColumns = new ZLimitedColumnsProvider(typeof(DummyBizo));

			AssertNull(limitedColumns.CodeSchemaColumn);
			AssertNull(limitedColumns.DescriptionSchemaColumn);

			AssertEquals("Code", limitedColumns.CodeColumnName);
			AssertEquals("Description", limitedColumns.DescriptionColumnName);
		}

		public void TestWhenCodePropertyDoesNotProvide()
		{
			var limitedColumns = new ZLimitedColumnsProvider(typeof(DummyBizoWithoutCodeProperty));

			Assert("limit columns should not exist.", !limitedColumns.LimitColumnExists);
			AssertEquals(null, limitedColumns.CodeColumnName);
			AssertEquals(null, limitedColumns.DescriptionColumnName);
		}
	}
}
