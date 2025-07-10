using CargoWise.ResourceStrings.Cache;
using NUnit.Framework;

namespace CargoWise.Data.SqlClr.Testing
{
	sealed class HashCalculatorTest : TestCase
	{
		public void TestGetCustomizableDataKey()
		{
			var prefix = "TestPrefix";
			var caption = "TestCaption";

			using (var conn = Db.NewExtraConnectionWithMainDbCredentials(Db.ServerName, Db.EdwDatabaseName))
			{
				var expectedKey = CustomizableDataResourceStrings.GetCustomizableDataKey(prefix, caption);
				var actualKey = conn.ExecuteScalar($"select dbo.CLRGetCustomizableDataKey(N'{prefix}', N'{caption}')");

				AssertEquals("CLRGetCustomizableDataKey result", expectedKey, actualKey);
			}
		}
	}
}
