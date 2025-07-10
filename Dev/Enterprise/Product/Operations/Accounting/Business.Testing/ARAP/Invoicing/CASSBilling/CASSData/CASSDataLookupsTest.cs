using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class CASSDataLookupsTest : TestCaseWithFactory
	{
		public void TestCASSAdjustmentReasonTypes()
		{
			var reasonTypes = new CASSAdjustmentReasonTypes();
			AssertEquals("Number of Types", 10, reasonTypes.Count);
			AssertCodeAndDescription(reasonTypes, "GEN", "General");
			AssertCodeAndDescription(reasonTypes, "WGT", "Weight");
			AssertCodeAndDescription(reasonTypes, "RAT", "Rate");
			AssertCodeAndDescription(reasonTypes, "CAR", "Due Carrier");
			AssertCodeAndDescription(reasonTypes, "AGN", "Due Agent");
			AssertCodeAndDescription(reasonTypes, "VAL", "Valuation");
			AssertCodeAndDescription(reasonTypes, "COM", "Commission");
			AssertCodeAndDescription(reasonTypes, "INC", "Incentive");
			AssertCodeAndDescription(reasonTypes, "CRG", "Charge Basis");
			AssertCodeAndDescription(reasonTypes, "DRB", "Disputed Re-bill");

			AssertEquals("All items Should be Nontranslatable", true, reasonTypes.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		public void TestGeneralAdjustmentReasons()
		{
			var list = CASSAdjustmentReasons.GetReasonCodes(CASSAdjustmentReasonTypes.General.Code);
			AssertEquals("Number of Reasons", 7, list.Count);
			AssertCodeAndDescription(list, "1", "Billed to Wrong Office");
			AssertCodeAndDescription(list, "2", "Billed to Wrong Agent");
			AssertCodeAndDescription(list, "3", "Paid Previously");
			AssertCodeAndDescription(list, "4", "Calculation Error");
			AssertCodeAndDescription(list, "5", "Other");
			AssertCodeAndDescription(list, "6", "VOID; Shipment Never Moved");
			AssertCodeAndDescription(list, "7", "Domestic Shipment; Billed as International");

			AssertEquals("All items Should be Nontranslatable", true, list.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		public void TestWeightAdjustmentReasons()
		{
			var list = CASSAdjustmentReasons.GetReasonCodes(CASSAdjustmentReasonTypes.Weight.Code);
			AssertEquals("Number of Reasons", 3, list.Count);
			AssertCodeAndDescription(list, "10", "Incorrect Gross Weight");
			AssertCodeAndDescription(list, "11", "Incorrect Chargeable Weight");
			AssertCodeAndDescription(list, "12", "Incorrect Volume Weight");

			AssertEquals("All items Should be Nontranslatable", true, list.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		public void TestRateAdjustmentReasons()
		{
			var list = CASSAdjustmentReasons.GetReasonCodes(CASSAdjustmentReasonTypes.Rate.Code);
			AssertEquals("Number of Reasons", 8, list.Count);
			AssertCodeAndDescription(list, "20", "Incorrect Contract Rate");
			AssertCodeAndDescription(list, "21", "Incorrect Spot or Ad-hoc Rate");
			AssertCodeAndDescription(list, "22", "Incorrect Service Level Rate");
			AssertCodeAndDescription(list, "23", "Incorrect Published Rate");
			AssertCodeAndDescription(list, "24", "Contract Rate not Applied");
			AssertCodeAndDescription(list, "25", "Spot/Ad-hoc Rate not Applied");
			AssertCodeAndDescription(list, "26", "Service Level Rate not Applied");
			AssertCodeAndDescription(list, "27", "Incorrect Pallet/Container Rate");

			AssertEquals("All items Should be Nontranslatable", true, list.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		public void TestDueCarrierAdjustmentReasons()
		{
			var list = CASSAdjustmentReasons.GetReasonCodes(CASSAdjustmentReasonTypes.DueCarrier.Code);
			AssertEquals("Number of Reasons", 5, list.Count);
			AssertCodeAndDescription(list, "30", "Incorrect Insurance Fee");
			AssertCodeAndDescription(list, "31", "Incorrect Security Fee");
			AssertCodeAndDescription(list, "32", "Incorrect Fuel Surcharge");
			AssertCodeAndDescription(list, "33", "Incorrect DG/RA Fee");
			AssertCodeAndDescription(list, "34", "Incorrect Other Due Carrier Fee");

			AssertEquals("All items Should be Nontranslatable", true, list.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		public void TestDueAgentAdjustmentReasons()
		{
			var list = CASSAdjustmentReasons.GetReasonCodes(CASSAdjustmentReasonTypes.DueAgent.Code);
			AssertEquals("Number of Reasons", 2, list.Count);
			AssertCodeAndDescription(list, "40", "Incorrect Due Agent Disbursement");
			AssertCodeAndDescription(list, "41", "Incorrect Due Agent Disbursement Applied");

			AssertEquals("All items Should be Nontranslatable", true, list.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		public void TestValuationAdjustmentReasons()
		{
			var list = CASSAdjustmentReasons.GetReasonCodes(CASSAdjustmentReasonTypes.Valuation.Code);
			AssertEquals("Number of Reasons", 2, list.Count);
			AssertCodeAndDescription(list, "50", "Incorrect Valuation Charge");
			AssertCodeAndDescription(list, "51", "Valuation Chg not Applied");

			AssertEquals("All items Should be Nontranslatable", true, list.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		public void TestCommissionAdjustmentReasons()
		{
			var list = CASSAdjustmentReasons.GetReasonCodes(CASSAdjustmentReasonTypes.Commission.Code);
			AssertEquals("Number of Reasons", 2, list.Count);
			AssertCodeAndDescription(list, "60", "Incorrect Commission");
			AssertCodeAndDescription(list, "61", "Commission not Applied");

			AssertEquals("All items Should be Nontranslatable", true, list.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		public void TestIncentiveAdjustmentReasons()
		{
			var list = CASSAdjustmentReasons.GetReasonCodes(CASSAdjustmentReasonTypes.Incentive.Code);
			AssertEquals("Number of Reasons", 2, list.Count);
			AssertCodeAndDescription(list, "70", "Incorrect Incentive");
			AssertCodeAndDescription(list, "71", "Incentive not Applied");

			AssertEquals("All items Should be Nontranslatable", true, list.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		public void TestChargeBasisAdjustmentReasons()
		{
			var list = CASSAdjustmentReasons.GetReasonCodes(CASSAdjustmentReasonTypes.ChargeBasis.Code);
			AssertEquals("Number of Reasons", 2, list.Count);
			AssertCodeAndDescription(list, "80", "Charges Changed to Prepaid; Billed as Collect");
			AssertCodeAndDescription(list, "81", "Charges Changed to Collect; Billed as Prepaid");

			AssertEquals("All items Should be Nontranslatable", true, list.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		public void TestDisputedRebillAdjustmentReasons()
		{
			var list = CASSAdjustmentReasons.GetReasonCodes(CASSAdjustmentReasonTypes.DisputedRebill.Code);
			AssertEquals("Number of Reasons", 3, list.Count);
			AssertCodeAndDescription(list, "90", "Re-bill Removed Per Carrier");
			AssertCodeAndDescription(list, "91", "Re-bill in Dispute Per Agent");
			AssertCodeAndDescription(list, "93", "Stale Dated Invoice");

			AssertEquals("All items Should be Nontranslatable", true, list.ToArray().Cast<CodeDescriptionPair>().All(x => typeof(NoResString) == x.MultilingualDescription.GetType()));
		}

		void AssertCodeAndDescription(CodeDescriptionPairList list, ZString code, ZString description)
		{
			Assert("Code Should Exist", list.ContainsCode(code));
			AssertEquals("Description", description, list[code].Description);
		}
	}
}
