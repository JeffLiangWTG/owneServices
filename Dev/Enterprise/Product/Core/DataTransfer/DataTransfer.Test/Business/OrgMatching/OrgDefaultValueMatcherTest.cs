using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class OrgDefaultValueMatcherTest : TransactionedTestCase
	{
		public void TestOrgDefaultMatcherReturnsValue()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var unmatchedOrg = new UnmatchedOrganisation();
			unmatchedOrg.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrg);
			AssertNotNull(new OrgDefaultValueMatcher(factory).Match());
			AssertEquals(new BaseOrgDefaultValueMatcher(factory).Match(), new OrgDefaultValueMatcher(factory).Match());
		}
	}
}
