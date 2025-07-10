using System.IO;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ExportIncoTermAndCustomsChargeFactoryTest : EUIncoTermAndCustomsChargeFactoryTest
	{
		public override void TestFactoryType()
		{
			AssertEquals(typeof(ExportIncoTermAndCustomsChargeFactory), incoTermAndChargeFactory.GetType());
		}

		public override void TestGetAllIncoTerms()
		{
			AssertEquals("Count", 14, incoTermAndChargeFactory.GetAllIncoTerms().Length);
		}

		public override void TestGetAllCharges()
		{
			var expectedCharges = new string[]
			{
				"ADD",
				"AFT",
				"DED",
				"INS",
				"OFT",
				"ONS"
			};

			AssertContainsExactElementsInAnyOrder(expectedCharges, incoTermAndChargeFactory.GetAllCharges().Select(_ => _.Code));
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(IEExportCustomsChargeTypeList.Codes.AdditionCharge, new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.AdditionCharge, IEExportCustomsChargeTypeList.Descriptions.AdditionCharge));
			AssertGetCharge(IEExportCustomsChargeTypeList.Codes.EUBorderFreight, new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.EUBorderFreight, IEExportCustomsChargeTypeList.Descriptions.EUBorderFreight));
			AssertGetCharge(IEExportCustomsChargeTypeList.Codes.DeductionCharge, new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.DeductionCharge, IEExportCustomsChargeTypeList.Descriptions.DeductionCharge));
			AssertGetCharge(IEExportCustomsChargeTypeList.Codes.EUBorderInsurance, new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.EUBorderInsurance, IEExportCustomsChargeTypeList.Descriptions.EUBorderInsurance));
			AssertGetCharge(IEExportCustomsChargeTypeList.Codes.OverseasFreight, new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.OverseasFreight, IEExportCustomsChargeTypeList.Descriptions.OverseasFreight));
			AssertGetCharge(IEExportCustomsChargeTypeList.Codes.OverseasInsurance, new CustomsChargeCode(IEExportCustomsChargeTypeList.Codes.OverseasInsurance, IEExportCustomsChargeTypeList.Descriptions.OverseasInsurance));
		}

		protected override string GetCountryContext() => Core.Constants.CountryCodes.Ireland + IEJobMessageTypeList.Codes.Export;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\IE\Core\Business.Test\Declaration\Valuation\TestFiles\ExportIncoTermAndCustomsChargeFactory.csv");
	}
}
