using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(ImportChargesProvider))]
	sealed class ImportChargesProviderTest : TestCaseWithFactory
	{
		public void TestCodes()
		{
			var codes = ImportChargesProvider.Codes;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 22, codes.Length);
				AssertContainsExactElementsInAnyOrder("Codes", new string[] { "AB", "AD", "AE", "AF", "AG", "AH", "AI", "AJ", "AK", "ONS", "OFT", "AL", "AN", "BA", "BB", "BC", "BD", "BE", "BF", "BG", "1X", "2X" }, codes.Select(x => x.Code));
				AssertSame("Cached", codes, ImportChargesProvider.Codes);
			});
		}

		public void TestCodesIncludedInInvoice()
		{
			var codes = ImportChargesProvider.IncludedInInvoice;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 8, codes.Length);
				AssertContainsExactElementsInAnyOrder("Codes", new string[] { "BA", "BB", "BC", "BD", "BE", "BF", "BG", "2X" }, codes.Select(x => x.Code));
				AssertSame("Cached", codes, ImportChargesProvider.IncludedInInvoice);
			});
		}

		public void TestCodesIncludedInInvoiceDeemedForThisCharge()
		{
			var codes = ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 9, codes.Length);
				AssertContainsExactElementsInAnyOrder("Codes", new string[] { "BA", "BB", "BC", "BD", "BE", "BF", "BG", "1X", "2X" }, codes.Select(x => x.Code));
				AssertSame("Cached", codes, ImportChargesProvider.IncludedInInvoiceDeemedForThisCharge);
			});
		}

		public void TestRecommendedAndMandatoryCodesEXW_FCA_FAS_FOB()
		{
			var codes = ImportChargesProvider.RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 3, codes.Length);
				AssertContainsExactElementsInAnyOrder("Codes", new string[] { "AK", "BA", "1X" }, codes.Select(x => x.Code));
				AssertSame("Cached", codes, ImportChargesProvider.RecommendedAndMandatoryCodesEXW_FCA_FAS_FOB);
			});
		}

		public void TestRecommendedAndMandatoryCodesDDP()
		{
			var codes = ImportChargesProvider.RecommendedAndMandatoryCodesDDP;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 1, codes.Length);
				AssertContainsExactElementsInAnyOrder("Codes", new string[] { "BC" }, codes.Select(x => x.Code));
				AssertSame("Cached", codes, ImportChargesProvider.RecommendedAndMandatoryCodesDDP);
			});
		}

		public void TestRecommendedCodesCFR_CPT()
		{
			var codes = ImportChargesProvider.RecommendedCodesCFR_CIF_CPT;
			CombineAssertions(() =>
			{
				AssertEquals("Count", 1, codes.Length);
				AssertContainsExactElementsInAnyOrder("Codes", new string[] { "AK" }, codes.Select(x => x.Code));
				AssertSame("Cached", codes, ImportChargesProvider.RecommendedCodesCFR_CIF_CPT);
			});
		}

		public void TestAB()
		{
			var charge = ImportChargesProvider.AB;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AB", charge.Code);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", false, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !charge.IsPercentageApplicable);
			});
		}

		public void TestAD()
		{
			var charge = ImportChargesProvider.AD;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AD", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.AD);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", false, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void TestAE()
		{
			var charge = ImportChargesProvider.AE;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AE", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.AE);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", false, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !charge.IsPercentageApplicable);
			});
		}

		public void TestAF()
		{
			var charge = ImportChargesProvider.AF;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AF", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.AF);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !charge.IsPercentageApplicable);
			});
		}

		public void TestAG()
		{
			var charge = ImportChargesProvider.AG;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AG", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.AG);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", false, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !charge.IsPercentageApplicable);
			});
		}

		public void TestAH()
		{
			var charge = ImportChargesProvider.AH;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AH", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.AH);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", false, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void TestAI()
		{
			var charge = ImportChargesProvider.AI;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AI", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.AI);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void TestAJ()
		{
			var charge = ImportChargesProvider.AJ;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AJ", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.AJ);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !charge.IsPercentageApplicable);
			});
		}

		public void TestAK()
		{
			var charge = ImportChargesProvider.AK;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AK", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.AK);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void TestONS()
		{
			var charge = ImportChargesProvider.ONS;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "ONS", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.ONS);
				AssertEquals("IsDutiable", true, charge.IsDutiable);
				AssertEquals("IsDutiableDeemedForThisCharge", false, charge.IsDutiableDeemedForThisCharge);
				AssertEquals("IsVATible", true, charge.IsVATible);
				AssertEquals("IsVATibleDeemedForThisCharge", true, charge.IsVATibleDeemedForThisCharge);
				AssertEquals("IsStatisticalValueApplicable", true, charge.IsStatisticalValueApplicable);
				AssertEquals("IsStatisticalValueApplicableDeemed", true, charge.IsStatisticalValueApplicableDeemed);
				AssertEquals("IsIncludedInITOTDeemedForThisCharge",false, charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				AssertEquals("IsIncoTermNeutral", true, charge.IsIncoTermNeutral);
				AssertEquals("IsPercentageApplicable", true, charge.IsPercentageApplicable);
			});
		}

		public void TestOFT()
		{
			var charge = ImportChargesProvider.OFT;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "OFT", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.OFT);
				AssertEquals("IsDutiable", true, charge.IsDutiable);
				AssertEquals("IsDutiableDeemedForThisCharge", false, charge.IsDutiableDeemedForThisCharge);
				AssertEquals("IsVATible", true, charge.IsVATible);
				AssertEquals("IsVATibleDeemedForThisCharge", true, charge.IsVATibleDeemedForThisCharge);
				AssertEquals("IsStatisticalValueApplicable", true, charge.IsStatisticalValueApplicable);
				AssertEquals("IsStatisticalValueApplicableDeemed", true, charge.IsStatisticalValueApplicableDeemed);
				AssertEquals("IsIncludedInITOTDeemedForThisCharge", false, charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				AssertEquals("IsIncoTermNeutral", true, charge.IsIncoTermNeutral);
				AssertEquals("IsPercentageApplicable", true, charge.IsPercentageApplicable);
			});
		}

		public void TestAL()
		{
			var charge = ImportChargesProvider.AL;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AL", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.AL);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void TestAN()
		{
			var charge = ImportChargesProvider.AN;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "AN", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.AN);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void TestBA()
		{
			var charge = ImportChargesProvider.BA;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "BA", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.BA);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void TestBB()
		{
			var charge = ImportChargesProvider.BB;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "BB", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.BB);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !charge.IsPercentageApplicable);
			});
		}

		public void TestBC()
		{
			var charge = ImportChargesProvider.BC;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "BC", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.BC);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", !charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !charge.IsPercentageApplicable);
			});
		}

		public void TestBD()
		{
			var charge = ImportChargesProvider.BD;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "BD", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.BD);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void TestBE()
		{
			var charge = ImportChargesProvider.BE;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "BE", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.BE);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void TestBF()
		{
			var charge = ImportChargesProvider.BF;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "BF", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.BF);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void TestBG()
		{
			var charge = ImportChargesProvider.BG;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "BG", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider.BG);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void Test1X()
		{
			var charge = ImportChargesProvider._1X;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "1X", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider._1X);
				Assert("IsDutiable", charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", !charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", charge.IsPercentageApplicable);
			});
		}

		public void Test2X()
		{
			var charge = ImportChargesProvider._2X;
			CombineAssertions(() =>
			{
				AssertEquals("Charge Code", "2X", charge.Code);
				AssertSame("Cached", charge, ImportChargesProvider._2X);
				Assert("IsDutiable", !charge.IsDutiable);
				Assert("IsDutiableDeemedForThisCharge", charge.IsDutiableDeemedForThisCharge);
				Assert("IsVATible", !charge.IsVATible);
				Assert("IsVATibleDeemedForThisCharge", charge.IsVATibleDeemedForThisCharge);
				Assert("IsStatisticalValueApplicable", !charge.IsStatisticalValueApplicable);
				Assert("IsStatisticalValueApplicableDeemed", charge.IsStatisticalValueApplicableDeemed);
				Assert("IsIncludedInITOTDeemedForThisCharge", charge.IsIncludedInITOTDeemedForThisCharge);
				AssertEquals("IsIncludedInITOTIfDeemed", true, charge.IsIncludedInITOTIfDeemed);
				Assert("IsIncoTermNeutral", charge.IsIncoTermNeutral);
				Assert("IsPercentageApplicable", !charge.IsPercentageApplicable);
			});
		}
	}
}
