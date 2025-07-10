using System;
using System.Collections.Generic;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.Logistics
{
	[TestedType(typeof(ForwarderShippingLineVGMInboundMessages22_7_21_121_To_23_9_29_10))]
	class ForwarderShippingLineVGMInboundMessages22_7_21_121_To_23_9_29_10Test : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => true;
		protected override IDateTimeRange TestDateTimeRange => throw new NotImplementedException();

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions) => throw new NotImplementedException();
		protected override void PrepareTestData() => throw new NotImplementedException();
	}
}
