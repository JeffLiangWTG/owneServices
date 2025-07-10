using System;
using Enterprise.StabilityChecker;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	sealed class LicenceStabilityCheckerTest : TestCase
	{
		[TestDate(2005, 12, 01)]
		public void TestSystemWillExpireIn7Days()
		{
			LicenceStabilityChecker licenceStabilityChecker = new LicenceStabilityChecker();
			StabilityResult[] results = licenceStabilityChecker.Check();
			AssertEquals("results.Length", 1, results.Length);
			AssertEquals("StabilityLevel", StabilityResultLevel.Warning, results[0].StabilityLevel);
			AssertEquals("Description", LicenceExpiryCheck.Create().StabilityErrorMessage, results[0].Description);
		}

		[TestDate(2005, 12, 04)]
		public void TestSystemWillExpireTomorrow()
		{
			LicenceStabilityChecker licenceStabilityChecker = new LicenceStabilityChecker();
			StabilityResult[] results = licenceStabilityChecker.Check();
			AssertEquals("results.Length", 1, results.Length);
			AssertEquals("StabilityLevel", StabilityResultLevel.Critical, results[0].StabilityLevel);
			AssertEquals("Description", LicenceExpiryCheck.Create().StabilityErrorMessage, results[0].Description);
		}

		[TestDate(2005, 12, 06)]
		public void TestSystemExpired()
		{
			LicenceStabilityChecker licenceStabilityChecker = new LicenceStabilityChecker();
			StabilityResult[] results = licenceStabilityChecker.Check();
			AssertEquals("results.Length", 1, results.Length);
			AssertEquals("StabilityLevel", StabilityResultLevel.Critical, results[0].StabilityLevel);
		}

		[TestDate(2013, 1, 1)]
		public void TestSystemWillExpireIn25Days()
		{
			DateTime expiryDateTime = new DateTime(2005, 12, 5, 16, 0, 0);
			TestDateAttribute.Date = expiryDateTime.AddDays(-25);

			LicenceStabilityChecker licenceStabilityChecker = new LicenceStabilityChecker();
			StabilityResult[] results = licenceStabilityChecker.Check();
			AssertEquals("results.Length", 1, results.Length);
			AssertEquals("StabilityLevel", StabilityResultLevel.Warning, results[0].StabilityLevel);
			AssertEquals("Description", LicenceExpiryCheck.Create().StabilityErrorMessage, results[0].Description);
		}

		[TestDate(2005, 11, 5)]
		public void TestHealthy()
		{
			LicenceStabilityChecker licenceStabilityChecker = new LicenceStabilityChecker();
			StabilityResult[] results = licenceStabilityChecker.Check();
			AssertEquals("results.Length", 0, results.Length);
		}
	}
}
