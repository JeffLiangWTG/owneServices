using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	sealed class NctsDepartureCargoDescPhase5ValidationDeciderTest : TestCaseWithFactory
	{
		public void TestIsRuleB1805_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleB1805_1Active);
		}

		public void TestIsRuleB1875_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleB1875_1Active);
		}

		public void TestIsRuleB1922Active()
		{
			AssertEquals(false, validationDecider.IsRuleB1922Active);
		}

		public void TestIsRuleB2101Active()
		{
			AssertEquals(false, validationDecider.IsRuleB2101Active);
		}

		public void TestIsRuleB2400_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleB2400_1Active);
		}

		public void TestIsRuleC0045Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0045Active);
		}

		public void TestIsRuleC0153_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleC0153_1Active);
		}

		public void TestIsRuleC0343Active()
		{
			AssertEquals(false, validationDecider.IsRuleC0343Active);
		}

		public void TestIsRuleC0343_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleC0343_1Active);
		}

		public void TestIsRuleC0343_2Active()
		{
			AssertEquals(false, validationDecider.IsRuleC0343_2Active);
		}

		public void TestIsRuleC0821_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleC0821_1Active);
		}

		public void TestIsRuleC0837Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0837Active);
		}

		public void TestIsRuleC0837_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleC0837_1Active);
		}

		public void TestIsRuleC901Active()
		{
			AssertEquals(true, validationDecider.IsRuleC901Active);
		}

		public void TestIsRuleC0909_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleC0909_1Active);
		}

		public void TestIsRuleE1107Active()
		{
			AssertEquals(false, validationDecider.IsRuleE1107Active);
		}

		public void TestIsRuleE1107_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleE1107_1Active);
		}

		public void TestIsRuleE1109Active()
		{
			AssertEquals(true, validationDecider.IsRuleE1109Active);
		}

		public void TestIsRuleE1301Active()
		{
			AssertEquals(false, validationDecider.IsRuleE1301Active);
		}

		public void TestIsRuleE1407Active()
		{
			AssertEquals(true, validationDecider.IsRuleE1407Active);
		}

		public void TestIsRuleNR0020Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0020Active);
		}

		public void TestIsRuleNR0021Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0021Active);
		}

		public void TestIsRuleNR0024Active()
		{
			AssertEquals(true, validationDecider.IsRuleNR0024Active);
		}

		public void TestIsRuleNR0025Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0025Active);
		}

		public void TestIsRuleNR0040Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0040Active);
		}

		public void TestIsRuleNR0041Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0041Active);
		}

		public void TestIsRuleNR0042Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0042Active);
		}

		public void TestIsRuleNR0043Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0043Active);
		}

		public void TestIsRuleNR0044Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0044Active);
		}

		public void TestIsRuleNR0045Active()
		{
			AssertEquals(false, validationDecider.IsRuleNR0045Active);
		}

		public void TestIsRuleNR0058Active()
		{
			AssertEquals(true, validationDecider.IsRuleNR0058Active);
		}

		public void TestIsRuleNR0059Active()
		{
			AssertEquals(true, validationDecider.IsRuleNR0059Active);
		}

		public void TestIsRuleNR0060Active()
		{
			AssertEquals(true, validationDecider.IsRuleNR0060Active);
		}

		public void TestIsRuleR0221_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleR0221_1Active);
		}

		public void TestIsRuleR0221_3Active()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = false;
				AssertEquals("When IsVehicles = false", true, validationDecider.IsRuleR0221_3Active);

				goodsItem.IsVehicles = true;
				AssertEquals("When IsVehicles = true", false, validationDecider.IsRuleR0221_3Active);
			});
		}

		public void TestIsRuleR0507Active()
		{
			AssertEquals(true, validationDecider.IsRuleR0507Active);
		}

		public void TestIsRuleR0507_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleR0507_1Active);
		}

		public void TestIsRuleR0601_1Active()
		{
			AssertEquals(false, validationDecider.IsRuleR0601_1Active);
		}

		public void TestIsRuleR0909Active()
		{
			AssertEquals(true, validationDecider.IsRuleR0909Active);
		}

		public void TestIsRuleRP11Active()
		{
			AssertEquals(false, validationDecider.IsRuleRP11Active);
		}

		public void TestIsRuleTR0096Active()
		{
			AssertEquals(true, validationDecider.IsRuleTR0096Active);
		}

		public void TestIsRuleC0909Active()
		{
			AssertEquals(true, validationDecider.IsRuleC0909Active);
		}

		public void TestIsRuleTR0068Active()
		{
			AssertEquals(true, validationDecider.IsRuleTR0068Active);
		}

		public void TestIsRuleTR0076Active()
		{
			AssertEquals(true, validationDecider.IsRuleTR0076Active);
		}

		public void TestIsRuleTR0100Active()
		{
			AssertEquals(true, validationDecider.IsRuleTR0100Active);
		}

		protected override void SetUp()
		{
			base.SetUp();
			goodsItem = Factory.New<NctsDepartureCargoDesc>();
			validationDecider = new NctsDepartureCargoDescPhase5ValidationDecider(goodsItem);
		}

		NctsDepartureCargoDesc goodsItem;
		EU.NCTS.Business.INctsDepartureCargoDescPhase5ValidationDecider validationDecider;
	}
}
