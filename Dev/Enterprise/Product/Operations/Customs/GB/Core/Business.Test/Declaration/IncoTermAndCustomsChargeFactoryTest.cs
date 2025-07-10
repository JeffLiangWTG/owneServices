using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Business.Testing
{
	sealed class IncoTermAndCustomsChargeFactoryTest : UCCIncoTermAndCustomsChargeFactoryTest
	{
		public void TestIsThisChargeRecommendedForThisInvoice()
		{
			var testDec = Factory.New<JobDeclaration>();
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			Assert(!incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.ExWorks, ChargesProvider.AirFreightCode));

			testDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			Assert(incoTermAndChargeFactory.IsThisChargeRecommendedForThisInvoice(invoice, Core.Constants.IncoTerms.ExWorks, ChargesProvider.AirFreightCode));
		}

		public override void TestFactoryType()
		{
			AssertEquals(typeof(IncoTermAndChargeFactory), incoTermAndChargeFactory.GetType());
		}

		public override void TestGetAllCharges()
		{
			var expectedCharges = new CustomsChargeCode[]
			{
				ChargesProvider.AirFreight,
				ChargesProvider.InternationalFreight,
				ChargesProvider.InternationalInsurance,
				ChargesProvider.AdditionCharge,
				ChargesProvider.DeductionCharge,
				ChargesProvider.VATAdjustment,
				ChargesProvider.Discount,
			};

			AssertContainsExactElementsInAnyOrder(expectedCharges, incoTermAndChargeFactory.GetAllCharges());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\GB\Core\Business.Test\Declaration\TestFiles\IncoTermAndCustomsChargeConfiguration.csv";

		public override void TestGetCharge()
		{
			AssertGetCharge(ChargesProvider.AirFreightCode, ChargesProvider.AirFreight);
			AssertGetCharge(ChargesProvider.OverseasFreightInChiefTerminologyCode_AWB, ChargesProvider.InternationalFreight);
			AssertGetCharge(ChargesProvider.VATAdjustmentCode, ChargesProvider.VATAdjustment);
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, ChargesProvider.InternationalInsurance);
			AssertGetCharge(CustomsChargeTypeList.Codes.AdditionCharge, ChargesProvider.AdditionCharge);
			AssertGetCharge(CustomsChargeTypeList.Codes.DeductionCharge, ChargesProvider.DeductionCharge);
			AssertGetCharge(CustomsChargeTypeList.Codes.Discount, ChargesProvider.Discount);
		}

		protected override string FreightToEUBorderCode => ChargesProvider.AirFreightCode;
		protected override string FreightAfterEUBorderCode => ChargesProvider.AirFreightCode;

		protected override void AssertAfterEUBorderCharge(JobComInvCharge charge)
		{
			Assert("J7_IsDutiable", !charge.J7_IsDutiable);
			Assert("J7_IsGSTApplicable", charge.J7_IsGSTApplicable);
			Assert("J7_IsStatisticalValueApplicable", !charge.J7_IsStatisticalValueApplicable);
		}

		protected override void SetUp()
		{
			base.SetUp();
			incoTermAndChargeFactory = new IncoTermAndChargeFactory();
		}
	}
}
