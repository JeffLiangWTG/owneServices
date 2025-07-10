using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ClientXPLDUploadLog))]
	public class ClientXPLDUploadLogTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeafultValues()
		{
			ClientXPLDUploadLog log = Factory.New<ClientXPLDUploadLog>();
			Assert("it should default the date", !log.U3_DateCreated.IsEmpty);
			AssertEquals("date should be today", ZDateTime.Now.ToShortDateString(), log.U3_DateCreated.ToShortDateString());
		}

		public void TestGetBISIDataWithInsertedXPLDReleaseCode()
		{
			ZString preloadedXPLD = "92323VFVG38AU2000002010-02-17ADD04XX002010-02-17                                                                                                                                                                                                                            ";
			ZString expectedXPLD = "92323VFVG38AU2000002010-02-17ADD04XX002010-02-17                                                                                        XX                                                                                                                                  ";
			ClientXPLDUploadLog log = Factory.New<ClientXPLDUploadLog>();
			log.U3_BISIData = preloadedXPLD;
			AssertEquals("It should insert the Release Code into the preloaded XPLD", expectedXPLD, log.GetBISIDataWithInsertedXPLDReleaseCode("XX"));
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
