using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class GetExtraTaxTypesTest : ScriptTest
	{
		public void TestExtraTaxTypes()
		{
			var accTaxRateExtraTypes = typeof(AccTaxRate.ExtraTypes);
			var extraTaxTypesInMasterFiles = accTaxRateExtraTypes.GetFields(BindingFlags.Public | BindingFlags.Static);

			var extraTypesFromFunction = new List<ZString>();
			var result = RunScript();
			foreach (DataRow row in result.Rows)
			{
				extraTypesFromFunction.Add(row["ExtraTaxType"].ToString());
			}

			var objectInMasterFiles = Factory.New<AccTaxRate>();

			foreach (var extraTaxType in extraTaxTypesInMasterFiles)
			{
				var valueInMasterFiles = extraTaxType.GetValue(objectInMasterFiles).ToString();
				if (valueInMasterFiles == AccTaxRate.ExtraTypes.ServiceTax || valueInMasterFiles == AccTaxRate.ExtraTypes.RegionalTax)
				{
					continue;
				}

				AssertCollectionContains(valueInMasterFiles, extraTypesFromFunction);
			}
		}

		DataTable RunScript()
		{
			string sql = @"select * from dbo.GetExtraTaxTypes()";

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
