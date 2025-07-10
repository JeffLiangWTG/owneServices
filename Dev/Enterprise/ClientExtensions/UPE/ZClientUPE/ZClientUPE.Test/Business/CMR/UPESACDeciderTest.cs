using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;

namespace Enterprise.Client.UPE.Business.CMR.Testing
{
	public class UPESACDeciderTest : TestCaseWithFactory
	{
		public void TestIsValidForSAC()
		{
			CusHAWB.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Australia;
			CusHAWB.CS_GoodsValue = 200;
			CusHAWB.CS_GoodsDescription = "FREDOS";
			AssertEquals("Should be true in base", true, BaseSACDecider.IsValidForSAC);
			CusHAWB.CS_GoodsValue = 200;
			CusHAWB.CS_GoodsDescription = "FREDOS";
			AssertEquals("FREDOS is one of the UPS Stop Words", false, UPESACDecider.IsValidForSAC);
			CusHAWB.CS_GoodsValue = 200;
			CusHAWB.CS_GoodsDescription = "Valid Description";
			AssertEquals("No UPS words in the goods description", true, UPESACDecider.IsValidForSAC);
			CusHAWB.CS_GoodsValue = 200;
			CusHAWB.CS_GoodsDescription = "BUG";
			AssertEquals("BUG is a quarantine stop word", false, UPESACDecider.IsValidForSAC);
			CusHAWB.CS_GoodsValue = 150;
			CusHAWB.CS_GoodsDescription = "Valid Description";
			AssertEquals("Goods value below the threshold value required for SAC", true, UPESACDecider.IsValidForSAC);
			CusHAWB.CS_GoodsValue = 201;
			AssertEquals("Goods value above thesehold value required for SAC", false, UPESACDecider.IsValidForSAC);
		}

		#region Implementation
		SACDecider BaseSACDecider
		{
			get
			{
				return new SACDecider(CusHAWB.Factory, CusHAWB.GoodsValueInAUD, CusHAWB.CS_GoodsDescription);
			}
		}

		UPESACDecider UPESACDecider
		{
			get
			{
				return new UPESACDecider(CusHAWB);
			}
		}

		UPECusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.New<UPECusHAWB>();
				}

				return fCusHAWB;
			}
		}

		UPECusHAWB fCusHAWB;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			TaxOrFeeTestHelper.SetUp();
			TaxOrFeeTestHelper.SetDeminimus(Factory, 200m);
			UPEDataRegistry.Instance.StopPhrasesForGoodsDescription = new string[] { "MMs", "FREDOs", "ORANGE BICYCLE", "BLAH" };
			UPEDataRegistry.Instance.QuarantineStopPhrasesForGoodsDescription = new string[] { "BUG" };
		}
		#endregion
	}
}
