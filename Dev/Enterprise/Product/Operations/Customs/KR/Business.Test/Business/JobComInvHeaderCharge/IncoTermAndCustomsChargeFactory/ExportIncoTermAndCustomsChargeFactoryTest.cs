using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class ExportIncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
	{
		public override void TestGetAllIncoTerms()
		{
			var list = new ZString[] { "CFR", "CIF", "CIN", "CIP", "CPT", "DAF", "DAP", "DAT", "DDP", "DDU", "DEQ", "DES", "DPU", "EXW", "FAS", "FCA", "FOB" };
			var incoTerms = incoTermAndChargeFactory.GetAllIncoTerms();
			AssertEquals("Count", 17, incoTerms.Length);
			AssertEquals("Count", 17, incoTerms.Where(x => list.Contains(x)).Count());
		}

		public override void TestGetAllCharges()
		{
			SetupData();
			CombineAssertions(() =>
			{
				AssertEquals("There should be 11 charges", 11, charges.Length);

				AssertNotNull("Code 'ADD' exists", add);
				AssertNotNull("Code 'COM' exists", com);
				AssertNotNull("Code 'DED' exists", ded);
				AssertNotNull("Code 'DIS' exists", dis);
				AssertNotNull("Code 'EXW' exists", exw);
				AssertNotNull("Code 'FIF' exists", fif);
				AssertNotNull("Code 'LCH' exists", lch);
				AssertNotNull("Code 'OFT' exists", oft);
				AssertNotNull("Code 'ONS' exists", ons);
				AssertNotNull("Code 'OTH' exists", oth);
				AssertNotNull("Code 'PAC' exists", pac);
			});
		}

		public void TestIsDutiableReadonlyWhenExport()
		{
			SetupData();
			CombineAssertions(() =>
			{
				AssertEquals("EXW IsDutiable ReadOnly", true, exw.IsDutiableDeemedForThisCharge);
				AssertEquals("FIF IsDutiable ReadOnly", true, fif.IsDutiableDeemedForThisCharge);
				AssertEquals("LCH IsDutiable ReadOnly", true, lch.IsDutiableDeemedForThisCharge);
				AssertEquals("OFT IsDutiable ReadOnly", true, oft.IsDutiableDeemedForThisCharge);
				AssertEquals("ONS IsDutiable ReadOnly", true, ons.IsDutiableDeemedForThisCharge);
				AssertEquals("PAC IsDutiable ReadOnly", true, pac.IsDutiableDeemedForThisCharge);
			});
		}

		void SetupData()
		{
			charges = incoTermAndChargeFactory.GetAllCharges();
			add = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.AdditionCharge);
			com = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.Commission);
			ded = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.DeductionCharge);
			dis = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.Discount);
			exw = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.ExWorks);
			fif = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.ForeignInlandFreight);
			lch = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.LandingCharges);
			oft = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.OverseasFreight);
			ons = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.OverseasInsurance);
			oth = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.OtherCharges);
			pac = charges.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.PackingCost);
		}

		ICustomsChargeCode[] charges;
		ICustomsChargeCode add;
		ICustomsChargeCode com;
		ICustomsChargeCode ded;
		ICustomsChargeCode dis;
		ICustomsChargeCode exw;
		ICustomsChargeCode fif;
		ICustomsChargeCode lch;
		ICustomsChargeCode oft;
		ICustomsChargeCode ons;
		ICustomsChargeCode oth;
		ICustomsChargeCode pac;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\KR\Business.Test\Business\JobComInvHeaderCharge\IncoTermAndCustomsChargeFactory\TestFile\ExportIncoTermAndCustomsChargeConfiguration.csv";
		protected override string GetCountryContext() => Core.Constants.CountryCodes.KoreaSouth + Common.Shared.SharedJobMessageTypeList.Codes.Export;
	}
}
