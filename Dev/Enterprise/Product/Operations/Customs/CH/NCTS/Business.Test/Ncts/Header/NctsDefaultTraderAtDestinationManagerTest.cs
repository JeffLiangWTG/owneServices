using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsDefaultTraderAtDestinationManager))]
sealed class NctsDefaultTraderAtDestinationManagerTest : TestCaseWithFactory
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>("When nctsHeader is null", () => new NctsDefaultTraderAtDestinationManager(null));

	public void TestIsEnabled() => AssertEquals(true, DefaultTraderAtDestinationManager.IsEnabled());

	public void TestApplyDefaultingIfEnabled() => CombineAssertions(() =>
	{
		var comnpanyProxy = Factory.NewWithValidTestData<OrgHeader>();
		var branchProxy = Factory.NewWithValidTestData<OrgHeader>();

		GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
		GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

		DefaultTraderAtDestinationManager.ApplyDefaultingIfEnabled();
		AssertEquals("No proxy available and DestinationTrader is empty.", Guid.Empty, NctsHeader.DestinationTrader.OrganisationPK);

		NctsHeader.DestinationTrader.OrganisationPK = ZGuid.BrettsGuid;
		DefaultTraderAtDestinationManager.ApplyDefaultingIfEnabled();
		AssertEquals("No proxy available and DestinationTrader is already set: Keep existing DestinationTrader.", ZGuid.BrettsGuid, NctsHeader.DestinationTrader.OrganisationPK);

		GlbCompany.CurrentCompany.GC_OH_OrgProxy = comnpanyProxy.PK;
		Factory.Save();
		DefaultTraderAtDestinationManager.ApplyDefaultingIfEnabled();
		AssertEquals("Company proxy available and DestinationTrader is already set: Overwrite with Company proxy.", comnpanyProxy.PK, NctsHeader.DestinationTrader.OrganisationPK);

		GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;
		Factory.Save();
		DefaultTraderAtDestinationManager.ApplyDefaultingIfEnabled();
		AssertEquals("Company and Branch proxy available and DestinationTrader is already set: Overwrite with Branch proxy.", branchProxy.PK, NctsHeader.DestinationTrader.OrganisationPK);
	});

	NctsHeader NctsHeader => nctsHeader ??= Factory.New<NctsHeader>();
	NctsHeader nctsHeader;

	NctsDefaultTraderAtDestinationManager DefaultTraderAtDestinationManager => defaultTraderAtDestinationManager ??= new NctsDefaultTraderAtDestinationManager(NctsHeader);
	NctsDefaultTraderAtDestinationManager defaultTraderAtDestinationManager;
}
