using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.JP.Business.Testing
{
	abstract class BaseIncoTermAndCustomsChargeFactoryTest : Customs.Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllIncoTerms()
		{
			var list = new ZString[] { "C&F", "C&I", "CIP", "DAP", "DAT", "EXW", "DDP", "CPT", "CIF", "FOB", "CFR", "FAS", "FCA", "DAF", "DES", "DDU", "DPU", "DEQ" };
			var incoTerms = incoTermAndChargeFactory.GetAllIncoTerms();
			AssertContainsExactElementsInAnyOrder(list, incoTerms);
		}

		public override void TestGetAllCharges()
		{
			var list = new ZString[]
			{
				CustomsChargeTypeList.Codes.AdditionCharge,
				CustomsChargeTypeList.Codes.Commission,
				CustomsChargeTypeList.Codes.DeductionCharge,
				CustomsChargeTypeList.Codes.Discount,
				CustomsChargeTypeList.Codes.ExWorks,
				CustomsChargeTypeList.Codes.ForeignInlandFreight,
				CustomsChargeTypeList.Codes.LandingCharges,
				CustomsChargeTypeList.Codes.OverseasFreight,
				CustomsChargeTypeList.Codes.OverseasInsurance,
				CustomsChargeTypeList.Codes.OtherCharges,
				CustomsChargeTypeList.Codes.PackingCost,
			};

			var charges = incoTermAndChargeFactory.GetAllCharges().Select(c => c.Code);
			AssertContainsExactElementsInAnyOrder(list, charges);
		}

		public abstract void TestChargesCodes();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\JP\Core\Business.Test\Business\JobComInvHeaderCharge\IncoTermAndCustomsChargeFactory\TestFile\IncoTermAndCustomsChargeConfiguration.csv";
	}
}
