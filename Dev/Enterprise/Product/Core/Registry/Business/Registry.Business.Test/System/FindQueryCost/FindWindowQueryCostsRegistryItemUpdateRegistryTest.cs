using System;
using System.Text;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class FindWindowQueryCostsRegistryItemUpdateRegistryTest : TransactionedTestCase
	{
		public void TestUpdateRegistry()
		{
			var date = new DateTime(2015, 7, 21);

			SystemDataRegistry.Instance.FindWindowQueryGovernorCosts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FindWindowQueryCosts { AllowedCost = 123, MaximalCost = 456 });

			using (var cmd = Db.Connection.Command("SELECT SD_BinaryValue FROM dbo.StmData WHERE SD_Name = @name"))
			{
				cmd.AddParameterBasedOnDbColumn("@name", "FindWindowQueryGovernorCosts", StmDataSchema.SD_Name);
				using (var reader = cmd.ExecuteReader())
				{
					Assert("No entry for Last Database Restore registry in database", reader.Read());
					var bytes = reader[StmDataSchema.Constants.SD_BinaryValue];

					Assert("Invalid value for Last Database Restore registry", bytes != DBNull.Value);

					var currentValue = Encoding.ASCII.GetString((byte[])bytes).Replace("\0", "");
					const string expectedValue = "<?xml version=\"1.0\" encoding=\"utf-16\"?><FindWindowQueryCosts><AllowedCost>123</AllowedCost><MaximalCost>456</MaximalCost></FindWindowQueryCosts>";
					AssertEquals(expectedValue, currentValue);
				}
			}
		}
	}
}
