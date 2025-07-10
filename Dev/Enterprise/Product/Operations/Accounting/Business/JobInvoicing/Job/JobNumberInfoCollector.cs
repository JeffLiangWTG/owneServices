using System;
using System.Text;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	internal static class JobNumberInfoCollector
	{
		internal static string GetJobNumberInfo(this Job job)
		{
			var result = string.Empty;
			if (job != null)
			{
				switch (job.JobType.Code)
				{
					case JobInvoicingConsumerTypes.LocalCartageCode:
						result = GetJobNumberDetailsForTRNJobType(job);
						break;
					default:
						break;
				}
			}

			if (!string.IsNullOrEmpty(result))
			{
				result = FormattableString.Invariant($"{System.Environment.NewLine}{nameof(job.Parent.JobNumber)} details:{System.Environment.NewLine}{result}");
			}
			return result;
		}

		static string GetJobNumberDetailsForTRNJobType(Job job)
		{
			var infoBuilder = new StringBuilder();
			if (job != null && job.Parent != null && job.Parent is CommonCartage cartage)
			{
				infoBuilder.AppendLine(FormattableString.Invariant($"{nameof(cartage.CartageParent)} is {(cartage.CartageParent == null ? string.Empty : "NOT")} null."));
				infoBuilder.AppendLine(FormattableString.Invariant($"{nameof(cartage.JJ_Status)} = {cartage.JJ_Status}."));
				if (cartage.CartageParent != null)
				{
					infoBuilder.AppendLine(FormattableString.Invariant($"{nameof(cartage.CartageParent.UniqueConsignmentID)} = {cartage.CartageParent.UniqueConsignmentID}."));
				}
				infoBuilder.AppendLine(FormattableString.Invariant($"{nameof(cartage.JJ_ConsignmentID)} = {cartage.JJ_ConsignmentID}. Original value is: {cartage.JJ_ConsignmentIDInfo.OriginalValue}."));
			}
			return infoBuilder.ToString();
		}
	}
}
