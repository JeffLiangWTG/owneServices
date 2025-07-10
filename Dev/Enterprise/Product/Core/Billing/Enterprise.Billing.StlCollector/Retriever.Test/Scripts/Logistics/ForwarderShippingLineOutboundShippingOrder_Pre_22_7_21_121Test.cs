using System;
using System.Collections.Generic;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderShippingLineOutboundShippingOrder_Pre_22_7_21_121))]
	sealed class ForwarderShippingLineOutboundShippingOrder_Pre_22_7_21_121Test : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;
		protected override IDateTimeRange TestDateTimeRange => throw new NotImplementedException();

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions) => throw new NotImplementedException();
		protected override void PrepareTestData() => throw new NotImplementedException();
	}
}
