using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class AtLeastOneAgentFilterContainsOrgProxyValidatorTest : TestCase
	{
		public void TestValidation()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			LookupField emptyField = new LookupField(factory);
			emptyField.DisplayName = "EmptyFilter";
			emptyField.Value = Guid.Empty;

			LookupField randomOrgField = new LookupField(factory);
			randomOrgField.DisplayName = "BadFilter";
			randomOrgField.Value = Guid.NewGuid();

			LookupField orgProxyField = new LookupField(factory);
			orgProxyField.DisplayName = "GoodFilter";
			orgProxyField.Value = GlbCompany.CurrentCompany.OrgProxy.PK.ToGuid();

			LookupField orgProxyFromBranchField = new LookupField(factory);
			orgProxyFromBranchField.DisplayName = "AlsoGoodFilter";
			orgProxyFromBranchField.Value = GlbBranch.CurrentBranch.OrgProxy.PK.ToGuid();

			AtLeastOneAgentFilterContainsOrgProxyValidator atLeastOneAgentValidator = new AtLeastOneAgentFilterContainsOrgProxyValidator();
			atLeastOneAgentValidator.Filters.Add(emptyField);
			atLeastOneAgentValidator.Filters.Add(randomOrgField);
			AssertEquals(false, atLeastOneAgentValidator.IsValid(emptyField));
			AssertEquals(false, atLeastOneAgentValidator.IsValid(randomOrgField));

			string expectedErrorMessage = string.Format("At least one of the 'EmptyFilter' and 'BadFilter' should be set to only the current login company's organization proxy ({0}) or current login branch's organization proxy ({1}). This is required because you have not been granted the security right ({2}).",
				GlbCompany.CurrentCompany.OrgProxy.OH_Code, GlbBranch.CurrentBranch.OrgProxy.OH_Code, Env.Security.ReportsExternalAgentsFilter.DisplayTextPathToSecurityRight);
			AssertEquals(expectedErrorMessage, atLeastOneAgentValidator.GetErrorMessage(emptyField));

			atLeastOneAgentValidator = new AtLeastOneAgentFilterContainsOrgProxyValidator();
			atLeastOneAgentValidator.Filters.Add(emptyField);
			atLeastOneAgentValidator.Filters.Add(orgProxyField);
			AssertEquals(true, atLeastOneAgentValidator.IsValid(emptyField));
			AssertEquals(true, atLeastOneAgentValidator.IsValid(orgProxyField));

			atLeastOneAgentValidator = new AtLeastOneAgentFilterContainsOrgProxyValidator();
			atLeastOneAgentValidator.Filters.Add(emptyField);
			AssertEquals(false, atLeastOneAgentValidator.IsValid(emptyField));

			expectedErrorMessage = string.Format("'EmptyFilter' should be set to only the current login company's organization proxy ({0}) or current login branch's organization proxy ({1}). This is required because you have not been granted the security right ({2}).",
				GlbCompany.CurrentCompany.OrgProxy.OH_Code, GlbBranch.CurrentBranch.OrgProxy.OH_Code, Env.Security.ReportsExternalAgentsFilter.DisplayTextPathToSecurityRight);
			AssertEquals(expectedErrorMessage, atLeastOneAgentValidator.GetErrorMessage(emptyField));

			atLeastOneAgentValidator = new AtLeastOneAgentFilterContainsOrgProxyValidator();
			atLeastOneAgentValidator.Filters.Add(orgProxyField);
			AssertEquals(true, atLeastOneAgentValidator.IsValid(orgProxyField));

			atLeastOneAgentValidator = new AtLeastOneAgentFilterContainsOrgProxyValidator();
			atLeastOneAgentValidator.Filters.Add(orgProxyFromBranchField);
			AssertEquals(true, atLeastOneAgentValidator.IsValid(orgProxyFromBranchField));
		}

		public void TestValidation_NoOrgProxy()
		{
			var factory = new BusinessObjectFactory();
			var filter = new LookupField(factory);
			filter.DisplayName = "Filter";
			filter.Value = Guid.Empty;

			var filter2 = new LookupField(factory);
			filter2.DisplayName = "Filter2";
			filter2.Value = Guid.Empty;

			var atLeastOneAgentValidator = new AtLeastOneAgentFilterContainsOrgProxyValidator();
			atLeastOneAgentValidator.Filters.Add(filter);
			atLeastOneAgentValidator.Filters.Add(filter2);
			Assert(!atLeastOneAgentValidator.IsValid(filter));
			Assert(!atLeastOneAgentValidator.IsValid(filter2));

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

			var expectedErrorMessage = string.Format("At least one of the 'Filter' and 'Filter2' should be set to only the current login company's organization proxy ({0}). This is required because you have not been granted the security right ({1}).",
				GlbCompany.CurrentCompany.OrgProxy.OH_Code, Env.Security.ReportsExternalAgentsFilter.DisplayTextPathToSecurityRight);
			AssertEquals(expectedErrorMessage, atLeastOneAgentValidator.GetErrorMessage(filter));
		}
	}
}
