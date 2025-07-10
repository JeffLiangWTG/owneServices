using System.IO;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class DEIncoTermAndCustomsChargeFactoryTest : EUIncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllIncoTerms()
		{
			AssertEquals("Count", 14, incoTermAndChargeFactory.GetAllIncoTerms().Length);
		}

		public override void TestGetAllCharges()
		{
			AssertEquals("There should be 6 charges", 6, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(ChargeCodeList.Codes.AdditionCharge, ChargesProvider.AdditionCharge);
			AssertGetCharge(ChargeCodeList.Codes.EUBorderFreight, ChargesProvider.EUBorderFreight);
			AssertGetCharge(ChargeCodeList.Codes.DeductionCharge, ChargesProvider.DeductionCharge);
			AssertGetCharge(ChargeCodeList.Codes.EUBorderInsurance, ChargesProvider.EUBorderInsurance);
			AssertGetCharge(ChargeCodeList.Codes.OverseasFreight, ChargesProvider.OverseasFreight);
			AssertGetCharge(ChargeCodeList.Codes.OverseasInsurance, ChargesProvider.OverseasInsurance);
		}

		public override void TestFactoryType()
		{
			AssertEquals(typeof(DEIncoTermAndCustomsChargeFactory), incoTermAndChargeFactory.GetType());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => Path.Combine(BaseSourcePath, TestHelper.BusinessTestDirectory, @"IncoTerm\Testing\DEIncoTermAndCustomsChargeConfiguration.csv");

		protected override string FreightToEUBorderCode => ChargeCodeList.Codes.EUBorderFreight;

		protected override string FreightAfterEUBorderCode => ChargeCodeList.Codes.OverseasFreight;
	}
}
