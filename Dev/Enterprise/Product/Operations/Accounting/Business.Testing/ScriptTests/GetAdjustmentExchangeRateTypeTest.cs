using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class GetAdjustmentExchangeRateTypeTest : ScriptTest
	{
		public void TestGetAdjustmentExchangeRateType()
		{
			var result = RunScript();
			AssertExchangeRateType("PER", result);

			AccountingConfigurationRegistry.Instance.ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "BUY");
			result = RunScript();
			AssertExchangeRateType("BUY", result);

			AccountingConfigurationRegistry.Instance.ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "CUS");
			result = RunScript();
			AssertExchangeRateType("CUS", result);

			((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType).DeleteValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			result = RunScript();
			AssertExchangeRateType("BUY", result);
		}

		void AssertExchangeRateType(string rateType, DataTable table)
		{
			AssertEquals(1, table.Rows.Count);
			AssertEquals(rateType, table.Rows[0]["ExchangeRateType"]);
		}

		DataTable RunScript()
		{
			var sql = string.Format(@"SELECT ExchangeRateType FROM GetAdjustmentExchangeRateType('{0}')", GlbCompany.CurrentCompany.PK);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
