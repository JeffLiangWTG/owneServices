using System;
using System.Collections.Generic;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderBookableContainerMaximumTEU22_5_18_358_To_22_5_18_999))]
	sealed class ForwarderBookableContainerMaximumTEU22_5_18_358_To_22_5_18_999Test : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => throw new NotImplementedException();

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions) => throw new NotImplementedException();
		protected override void PrepareTestData() => throw new NotImplementedException();
	}
}
