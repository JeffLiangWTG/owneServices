using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaManifestHeaderSynchroniser))]
sealed class AsycudaManifestHeaderSynchroniserTest : TestCaseWithFactory
{
	public void TestLastForeignPort() => CombineAssertions(() =>
	{
		var sourceConsol = Factory.New<ForwardingConsol>();
		sourceConsol.JK_RL_NKLastForeignPort = "TEST1";

		var header = Factory.New<AsycudaManifestHeader>();
		header.SetParent(sourceConsol);
		header.Synchroniser.SetEnabled(true, false);
		header.Synchroniser.Synchronise();
		AssertEquals(header.AMA_CustomsOriginPort, "TEST1");

		sourceConsol.JK_RL_NKLastForeignPort = "TEST2";
		AssertEquals(header.AMA_CustomsOriginPort, "TEST2");
	});

	public void TestManifestBOL() => CombineAssertions(() =>
	{
		var sourceConsol = Factory.New<ForwardingConsol>();
		sourceConsol.JK_AgentType = "CLD";
		sourceConsol.JK_CoLoadMasterBill = "COLOADMBL";
		sourceConsol.JK_MasterBillNum = "MASTERBILLNUM";

		var header = Factory.New<AsycudaManifestHeader>();
		header.SetParent(sourceConsol);
		header.Synchroniser.SetEnabled(true, false);
		header.Synchroniser.Synchronise();
		AssertEquals("Manifest BOL should be mapped from Consol Coload Master Bill when Agent Type is CLD", header.AMA_MasterBill, "COLOADMBL");
		sourceConsol.JK_AgentType = "AGT";
		AssertEquals("Manifest BOL should be mapped from Consol BOL when Agent Type is not CLD", header.AMA_MasterBill, "MASTERBILLNUM");
	});

	public void TestManifestCarrier() => CombineAssertions(() =>
	{
		var carrier1 = Factory.New<OrgAddress>();
		var carrier2 = Factory.New<OrgAddress>();

		var sourceConsol = Factory.New<ForwardingConsol>();
		sourceConsol.JK_AgentType = "CLD";
		sourceConsol.JK_OA_CreditorAddress = carrier1.PK;

		var header = Factory.New<AsycudaManifestHeader>();
		header.SetParent(sourceConsol);
		header.Synchroniser.SetEnabled(true, false);
		header.Synchroniser.Synchronise();
		AssertEquals("Manifest Carrier should be mapped from Consol CoLoad With when Agent Type is not CLD", header.AMA_OA_Carrier, carrier1.PK);

		sourceConsol.JK_AgentType = "AGT";
		sourceConsol.JK_OA_ShippingLineAddress = carrier2.PK;
		AssertEquals("Manifest Carrier should be mapped from Consol Carrier when Agent Type is not CLD", header.AMA_OA_Carrier, carrier2.PK);
	});
}
