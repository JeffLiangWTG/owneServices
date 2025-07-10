using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDefaultTraderAtDestinationManager))]
	sealed class NctsDefaultTraderAtDestinationManagerTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new NctsDefaultTraderAtDestinationManager(nctsHeader: null));
		}

		public void TestIsEnabled()
		{
			CombineAssertions(() =>
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();

				AssertEquals("When NCTSDefaultTraderAtDestination registry item has its default status, IsEnabled", false, defaultTraderAtDestinationManager.IsEnabled());

				var nctsDefaultTraderAtDestination = new NctsDefaultTraderAtDestination(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultTraderAtDestination.LeaveBlank = false;
				nctsDefaultTraderAtDestination.TraderAtDestination = organization.PK;
				EUCustomsDataRegistry.Instance.NCTSDefaultTraderAtDestination.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultTraderAtDestination);
				AssertEquals("When LeaveBlank is not ticked and TraderAtDestination is filled, IsEnabled", true, defaultTraderAtDestinationManager.IsEnabled());

				nctsDefaultTraderAtDestination.LeaveBlank = true;
				nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.Empty;
				EUCustomsDataRegistry.Instance.NCTSDefaultTraderAtDestination.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultTraderAtDestination);
				AssertEquals("When LeaveBlank is ticked and TraderAtDestination is not filled, IsEnabled", true, defaultTraderAtDestinationManager.IsEnabled());

				nctsDefaultTraderAtDestination.LeaveBlank = true;
				nctsDefaultTraderAtDestination.TraderAtDestination = organization.PK;
				EUCustomsDataRegistry.Instance.NCTSDefaultTraderAtDestination.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultTraderAtDestination);
				AssertEquals("When LeaveBlank is ticked and TraderAtDestination is filled, IsEnabled", true, defaultTraderAtDestinationManager.IsEnabled());
			});
		}

		public void TestApplyDefaultingIfEnabled()
		{
			CombineAssertions(() =>
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();

				nctsHeader.DestinationTrader.OrganisationPK = ZGuid.BrettsGuid;
				defaultTraderAtDestinationManager.ApplyDefaultingIfEnabled();
				AssertEquals("When NCTSDefaultTraderAtDestination registry item has its disabled default status, TraderAtDestination should not change", ZGuid.BrettsGuid, nctsHeader.DestinationTrader.OrganisationPK);

				var nctsDefaultTraderAtDestination = new NctsDefaultTraderAtDestination(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
				nctsDefaultTraderAtDestination.LeaveBlank = false;
				nctsDefaultTraderAtDestination.TraderAtDestination = organization.PK;
				EUCustomsDataRegistry.Instance.NCTSDefaultTraderAtDestination.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultTraderAtDestination);
				nctsHeader.DestinationTrader.OrganisationPK = ZGuid.BrettsGuid;
				defaultTraderAtDestinationManager.ApplyDefaultingIfEnabled();
				AssertEquals("When registry defaulting is enabled with valid organisation override, TraderAtDestination", organization.PK, nctsHeader.DestinationTrader.OrganisationPK);

				nctsDefaultTraderAtDestination.LeaveBlank = true;
				nctsDefaultTraderAtDestination.TraderAtDestination = ZGuid.Empty;
				EUCustomsDataRegistry.Instance.NCTSDefaultTraderAtDestination.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, value: nctsDefaultTraderAtDestination);
				nctsHeader.DestinationTrader.OrganisationPK = ZGuid.BrettsGuid;
				defaultTraderAtDestinationManager.ApplyDefaultingIfEnabled();
				AssertEquals("When registry defaulting is enabled with empty organisation override, TraderAtDestination", ZGuid.Empty, nctsHeader.DestinationTrader.OrganisationPK);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			defaultTraderAtDestinationManager = new NctsDefaultTraderAtDestinationManager(nctsHeader);
		}

		NctsHeader nctsHeader;
		NctsDefaultTraderAtDestinationManager defaultTraderAtDestinationManager;
	}
}
