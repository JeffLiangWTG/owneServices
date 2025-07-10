using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class ChargeJRJExtensions
	{
		public static bool InternalFieldsPointToSameEntity(this BaseCharge charge) => charge.AutoJRJFieldsMatch() == true;			// nullable bool comparison
		public static bool InternalFieldsPointToAnotherEntity(this BaseCharge charge) => charge.AutoJRJFieldsMatch() == false;      // Create JRJ, Create Gateway Sell Apportionment, Neither

		public static bool InternalFieldsPointTo(this BaseCharge charge, JobHeader job, GlbBranch branch, GlbDepartment dept) => charge.InternalFieldsMatch(job, branch, dept) == true;		// nullable bool comparison

		public static bool? InternalFieldsMatch(this BaseCharge charge, JobHeader job, GlbBranch branch, GlbDepartment dept)
		{
			if (charge == null
				|| charge.InternalJob == null || charge.InternalBranch == null || charge.InternalDept == null
				|| job == null || branch == null || dept == null)
			{
				return null;
			}

			var result = true;

			result = result && charge.JR_JH_InternalJob == job.PK;
			result = result && charge.JR_GB_InternalBranch == branch.PK;
			result = result && charge.JR_GE_InternalDept == dept.PK;

			return result;
		}

		public static (string intJobNumber, string intBranch, string intDept) InternalFieldsStr(this BaseCharge charge)
		{
			string valueOrEmpty(string x) => string.IsNullOrWhiteSpace(x) ? Res.GetString("36855312-A82D-45F3-B007-9871DD05FD13", "empty") : x;
			string intJobNumber = null, intBranch = null, intDept = null;

			if (charge != null)
			{
				intJobNumber = charge.InternalJob?.JH_JobNum;
				intBranch = charge.InternalBranch?.GB_Code;
				intDept = charge.InternalDept?.GE_Code;
			}

			return (valueOrEmpty(intJobNumber), valueOrEmpty(intBranch), valueOrEmpty(intDept));
		}

		static bool? AutoJRJFieldsMatch(this BaseCharge charge)
		{
			return charge.InternalFieldsMatch(charge.Job, charge.Branch, charge.Department);
		}

		public static bool AreInternalFieldsEmpty(this BaseCharge charge)
			=> charge.InternalJob == null && charge.InternalBranch == null && charge.InternalDept == null;

		#region Compare Tax Registration Number

		public static bool IsNotEligableForAutoJRJ(this BaseCharge charge)
		{
			return (charge.CostAccountIsOrgProxy && charge.IsExcludedFromAutoJRJ(charge.CostAccount)) ||
				   (charge.SellAccountIsOrgProxy && charge.IsExcludedFromAutoJRJ(charge.SellAccount));
		}

		public static bool IsExcludedFromAutoJRJ(this BaseCharge charge, OrgHeader chargeOrgAccount)
		{
			return new AutoJobRevenueJournalHelper().IsExcludedFromAutoJRJ(charge.Branch, chargeOrgAccount, charge.Company.GC_RN_NKCountryCode);
		}

		#endregion
	}
}
