using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	class ExportIncoTermAndCustomsChargeFactoryTest : UCCIncoTermAndCustomsChargeFactoryTest
	{
		JobDeclaration testDec;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = MessageTypeList.Codes.Export;
		}

		public override void TestFactoryType()
		{
			AssertEquals(typeof(ExportIncoTermAndCustomsChargeFactory), incoTermAndChargeFactory.GetType());
		}

		public override void TestGetAllIncoTerms()
		{
			AssertEquals("Total Amount of Incoterms for Spanish Export Declaration", 17, incoTermAndChargeFactory.GetAllIncoTerms().Length);
		}

		public override void TestGetAllCharges()
		{
			AssertEquals("Total Amount of Charges for Spanish Export Declaration", 30, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public override void TestGetCharge()
		{
			CombineAssertions(() =>
			{
				AssertGetCharge(ESCustomsChargeTypeList.Codes.InternationalFreightExp, ESChargeProvider.GetNewInternationalFreightExp());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.TransportCostsUntilESBorder, ESChargeProvider.GetNewTransportCostsUntilESBorder());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.InsuranceUntilESBorder, ESChargeProvider.GetNewInsuranceUntilESBorder());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.OtherInternationalPayments, ESChargeProvider.GetNewOtherInternationalPayments());
				AssertGetCharge(ESCustomsChargeTypeList.Codes.OtherNationalPayments, ESChargeProvider.GetNewOtherNationalPayments());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.TransportCostsCharge, ESChargeProvider.GetNewTransportCostsExp());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, ESChargeProvider.GetNewInsuranceCostsExp());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.AdjustmentCharge, ESChargeProvider.GetNewAdjustment());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, ESChargeProvider.GetNewAirTransportCosts());
				AssertGetCharge(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge, ESChargeProvider.GetNewIndirectAndOtherPaymentsExp());
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + Constants.ProjectRelativePath + @"IncoTerm\TestFiles\ESIncoTermAndCustomsChargeConfigurationEXP.csv";
		protected override string GetCountryContext() => Core.Constants.CountryCodes.Spain;
	}
}
