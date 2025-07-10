using System;
using System.Collections.Generic;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(ComplianceRiskCommoditiesUsageCollector24_8_8_259_To_24_8_8_999))]
	class ComplianceRiskCommoditiesUsageCollector24_8_8_259_To_24_8_8_999Test : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => true;
		protected override IDateTimeRange TestDateTimeRange => throw new NotImplementedException();

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions) => throw new NotImplementedException();
		protected override void PrepareTestData() => throw new NotImplementedException();
	}
}
