using System;
using System.Collections.Generic;
using CargoWise.Billing.Collectors.Warehouse;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Warehouse
{
	[TestedType(typeof(WinCEDeviceUsageCollector_21_8_25_614_To_21_8_25_999))]
	sealed class WinCEDeviceUsageCollector_21_8_25_614_To_21_8_25_999Test : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => throw new NotImplementedException();

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions) => throw new NotImplementedException();
		protected override void PrepareTestData() => throw new NotImplementedException();
	}
}
