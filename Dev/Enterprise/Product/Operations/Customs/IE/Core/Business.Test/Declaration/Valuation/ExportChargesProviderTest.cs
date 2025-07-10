using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ExportChargesProviderTest : TestCase
	{
		public void TestAdditionCharge()
		{
			var charge = ExportChargesProvider.AdditionCharge;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "ADD", charge.Code);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", !charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", !charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", !charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				AssertEquals("IsIncludedInITOTIfDeemed", null, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("DistributeBy", ChargeDistributeByList.Codes.Weight, charge.DistributeBy);
				Assert("IsIncoTermNeutral", !charge.IsIncoTermNeutral);
			});
		}

		public void TestEUBorderFreight()
		{
			var charge = ExportChargesProvider.EUBorderFreight;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AFT", charge.Code);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", !charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", !charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", !charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				AssertEquals("IsIncludedInITOTIfDeemed", null, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("DistributeBy", ChargeDistributeByList.Codes.Weight, charge.DistributeBy);
				Assert("IsIncoTermNeutral", !charge.IsIncoTermNeutral);
			});
		}

		public void TestDeductionCharge()
		{
			var charge = ExportChargesProvider.DeductionCharge;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "DED", charge.Code);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", !charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", !charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", !charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				AssertEquals("IsIncludedInITOTIfDeemed", null, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("DistributeBy", ChargeDistributeByList.Codes.Weight, charge.DistributeBy);
				Assert("IsIncoTermNeutral", !charge.IsIncoTermNeutral);
			});
		}

		public void TestEUBorderInsurance()
		{
			var charge = ExportChargesProvider.EUBorderInsurance;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "INS", charge.Code);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", !charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", !charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", !charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				AssertEquals("IsIncludedInITOTIfDeemed", null, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("DistributeBy", ChargeDistributeByList.Codes.Value, charge.DistributeBy);
				Assert("IsIncoTermNeutral", !charge.IsIncoTermNeutral);
			});
		}

		public void TestOverseasFreight()
		{
			var charge = ExportChargesProvider.OverseasFreight;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "OFT", charge.Code);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", !charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", !charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", !charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				AssertEquals("IsIncludedInITOTIfDeemed", null, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("DistributeBy", ChargeDistributeByList.Codes.Weight, charge.DistributeBy);
				Assert("IsIncoTermNeutral", !charge.IsIncoTermNeutral);
			});
		}

		public void TestOverseasInsurance()
		{
			var charge = ExportChargesProvider.OverseasInsurance;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "ONS", charge.Code);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", !charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", !charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", !charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				AssertEquals("IsIncludedInITOTIfDeemed", null, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("DistributeBy", ChargeDistributeByList.Codes.Value, charge.DistributeBy);
				Assert("IsIncoTermNeutral", !charge.IsIncoTermNeutral);
			});
		}
	}
}
