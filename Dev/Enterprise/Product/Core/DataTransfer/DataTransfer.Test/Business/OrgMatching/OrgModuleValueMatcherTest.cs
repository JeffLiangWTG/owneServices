using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	class OrgModuleValueMatcherTest : TransactionedTestCase
	{
		public void TestOrgDefaultMatcherReturnsValue()
		{
			var factory = new BusinessObjectFactory();
			SetRegistryValue(true);
			var match = new OrgModuleValueMatcher(factory, DataContextType.OrderManagerOrder).Match();
			AssertEquals(OrgHeader.UnmatchOrg(factory), match);

			SetRegistryValue(false);
			match = new OrgModuleValueMatcher(factory, DataContextType.OrderManagerOrder).Match();
			AssertNull(match);
		}

		void SetRegistryValue(bool useUnmatchedOrg)
		{
			var registryValue = new CodeDescriptionBoolCollection
			{
				{ OrganisationsDataRegistry.JobTypeCodes.Order, (NoResString)"Order (Forwarding)", useUnmatchedOrg },
			};

			OrganisationsDataRegistry.Instance.UnmatchedOrganisationConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);
		}
	}
}
