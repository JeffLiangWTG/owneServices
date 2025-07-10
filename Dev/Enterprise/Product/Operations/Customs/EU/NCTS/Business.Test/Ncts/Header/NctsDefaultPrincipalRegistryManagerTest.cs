using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsDefaultPrincipalRegistryManagerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new NctsDefaultPrincipalRegistryManager(nctsHeader: null));
		}

		public void TestIsRegistryEnabled()
		{
			CombineAssertions(() =>
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();

				AssertEquals("When NCTSDefaultPrincipal registry item has its default status, IsRegistryEnabled", false, defaultPrincipalRegistryManager.IsRegistryEnabled());

				var nctsDefaultPrincipal = new NctsDefaultPrincipal(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultPrincipal.LeaveBlank = false;
				nctsDefaultPrincipal.Principal = organization.PK;
				EUCustomsDataRegistry.Instance.NCTSDefaultPrincipal.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultPrincipal);
				AssertEquals("When LeaveBlank is not ticked and Principal is filled, IsRegistryEnabled", true, defaultPrincipalRegistryManager.IsRegistryEnabled());

				nctsDefaultPrincipal.LeaveBlank = true;
				nctsDefaultPrincipal.Principal = ZGuid.Empty;
				EUCustomsDataRegistry.Instance.NCTSDefaultPrincipal.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultPrincipal);
				AssertEquals("When LeaveBlank is ticked and Principal is not filled, IsRegistryEnabled", true, defaultPrincipalRegistryManager.IsRegistryEnabled());

				nctsDefaultPrincipal.LeaveBlank = true;
				nctsDefaultPrincipal.Principal = organization.PK;
				EUCustomsDataRegistry.Instance.NCTSDefaultPrincipal.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultPrincipal);
				AssertEquals("When LeaveBlank is ticked and Principal is filled, IsRegistryEnabled", true, defaultPrincipalRegistryManager.IsRegistryEnabled());
			});
		}

		public void TestApplyDefaultingIfEnabled()
		{
			CombineAssertions(() =>
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();

				nctsHeader.Principal.OrganisationPK = ZGuid.BrettsGuid;
				defaultPrincipalRegistryManager.ApplyDefaultingIfEnabled();
				AssertEquals("When NCTSDefaultPrincipal registry item has its disabled default status, Principal should not change", ZGuid.BrettsGuid, nctsHeader.Principal.OrganisationPK);

				var nctsDefaultPrincipal = new NctsDefaultPrincipal(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultPrincipal.LeaveBlank = false;
				nctsDefaultPrincipal.Principal = organization.PK;
				EUCustomsDataRegistry.Instance.NCTSDefaultPrincipal.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultPrincipal);
				nctsHeader.Principal.OrganisationPK = ZGuid.BrettsGuid;
				defaultPrincipalRegistryManager.ApplyDefaultingIfEnabled();
				AssertEquals("When registry defaulting is enabled with valid organisation override, Principal", organization.PK, nctsHeader.Principal.OrganisationPK);

				nctsDefaultPrincipal.LeaveBlank = true;
				nctsDefaultPrincipal.Principal = ZGuid.Empty;
				EUCustomsDataRegistry.Instance.NCTSDefaultPrincipal.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultPrincipal);
				nctsHeader.Principal.OrganisationPK = ZGuid.BrettsGuid;
				defaultPrincipalRegistryManager.ApplyDefaultingIfEnabled();
				AssertEquals("When registry defaulting is enabled with empty organisation override, Principal", ZGuid.Empty, nctsHeader.Principal.OrganisationPK);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			defaultPrincipalRegistryManager = new NctsDefaultPrincipalRegistryManager(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsDefaultPrincipalRegistryManager defaultPrincipalRegistryManager;
	}
}
