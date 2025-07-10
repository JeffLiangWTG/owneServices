using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class LocalExportIncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllIncoTerms()
		{
			var list = new ZString[] { "EXW", "FAS", "FCA", "FOB" };
			var incoTerms = incoTermAndChargeFactory.GetAllIncoTerms();
			AssertEquals("Count", 4, incoTerms.Length);
			AssertEquals("Count", 4, incoTerms.Where(x => list.Contains(x)).Count());
		}

		public override void TestGetAllCharges()
		{
			var charges = incoTermAndChargeFactory.GetAllCharges();
			AssertEquals("There should be 8 charges", 8, charges.Length);
		}

		public override void TestGetCharge()
		{
			var charges = incoTermAndChargeFactory.GetAllCharges();
			var add = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.AdditionCharge);
			var com = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.Commission);
			var ded = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.DeductionCharge);
			var dis = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.Discount);
			var exw = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.ExWorks);
			var fif = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.ForeignInlandFreight);
			var oth = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.OtherCharges);
			var pac = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.PackingCost);

			AssertGetCharge(CustomsChargeTypeList.Codes.PackingCost, (CustomsChargeCode)pac);
			AssertGetCharge(CustomsChargeTypeList.Codes.Discount, (CustomsChargeCode)dis);
			AssertGetCharge(CustomsChargeTypeList.Codes.Commission, (CustomsChargeCode)com);
			AssertGetCharge(CustomsChargeTypeList.Codes.ExWorks, (CustomsChargeCode)exw);
			AssertGetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight, (CustomsChargeCode)fif);
			AssertGetCharge(CustomsChargeTypeList.Codes.OtherCharges, (CustomsChargeCode)oth);
			AssertGetCharge(CustomsChargeTypeList.Codes.AdditionCharge, (CustomsChargeCode)add);
			AssertGetCharge(CustomsChargeTypeList.Codes.DeductionCharge, (CustomsChargeCode)ded);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\KR\Business.Test\Business\JobComInvHeaderCharge\IncoTermAndCustomsChargeFactory\TestFile\LocalExportIncoTermAndCustomsChargeConfiguration.csv";
		protected override string GetCountryContext() => Core.Constants.CountryCodes.KoreaSouth + Common.KR.KRJobMessageTypeList.Codes.LocalExport;
	}
}
