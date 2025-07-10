using System;
using Enterprise.eHubMessaging.ServiceTasks.HealthChecks.FailedEDIInterchangeHealthCheck;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.HealthCheck
{
	public class FailedEDIInterchangeHealthCheckResultTests : TestCase
	{
		public void TestGetEmpty_Result_ZeroCount()
		{
			var result = FailedEDIInterchangeHealthCheckResult.Empty;
			Assert(!result.FoundError());
			AssertEquals(0, result.FailedEDIInterchangesCountInPeriodPerCompany.Count);
		}

		public void TestSerialize_Result_KeyEqualsValueString()
		{
			var branchPk = Guid.NewGuid();
			var result = FailedEDIInterchangeHealthCheckResult.Empty;
			result.Add(branchPk, 5);

			AssertEquals($"{branchPk}=5", result.Serialize());
		}

		public void TestDeserialize_CsvKeyEqualsValueString_ResultsDictionary()
		{
			var branch1Pk = Guid.NewGuid();
			var branch2Pk = Guid.NewGuid();
			var result = new FailedEDIInterchangeHealthCheckResult($"{branch1Pk}=1,{branch2Pk}=2,invalid");

			AssertEquals(2, result.FailedEDIInterchangesCountInPeriodPerCompany.Count);
			AssertEquals(1, result.FailedEDIInterchangesCountInPeriodPerCompany[branch1Pk]);
			AssertEquals(2, result.FailedEDIInterchangesCountInPeriodPerCompany[branch2Pk]);
		}

		public void TestAddMethod_Update_ExistKeyValues()
		{
			var branch1Pk = Guid.NewGuid();
			var branch2Pk = Guid.NewGuid();
			var branch3Pk = Guid.NewGuid();
			var result = new FailedEDIInterchangeHealthCheckResult($"{branch1Pk}=1,{branch2Pk}=2,invalid");

			result.Add(branch3Pk, 1);
			result.Add(branch2Pk, 2);

			AssertEquals(3, result.FailedEDIInterchangesCountInPeriodPerCompany.Count);
			AssertEquals($"{branch1Pk}=1,{branch2Pk}=4,{branch3Pk}=1", result.Serialize());
		}
	}
}
