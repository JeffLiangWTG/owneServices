using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class IM2AdjustmentsDocFooter : AdjustmentsDocFooter
	{
		public IM2AdjustmentsDocFooter() : base()
		{
		}

		public IM2AdjustmentsDocFooter(JobDeclaration declaration, IEnumerable<AdjustmentsDocPage> pages = null) : base(declaration, pages)
		{
		}

		public static IM2AdjustmentsDocFooter GetEmptyFooter()
		{
			return new IM2AdjustmentsDocFooter();
		}

		protected override void SetTotalValues(JobDeclaration declaration, IEnumerable<AdjustmentsDocPage> pages)
		{
			var splitLineStr = JobComInvoiceLine.SplitLine;

			var totalCustomsDuties = declaration.CA_AnySightDepositAmount;
			var totalSimaAssessment = ZDecimal.Zero;
			var totalExciseTax = ZDecimal.Zero;
			var totalGST = ZDecimal.Zero;
			var totalAmount = ZDecimal.Zero;

			var asAccountedLines = new List<AdjustmentsDocLine>();
			var asClaimedLines = new List<AdjustmentsDocLine>();
			var asClaimedSplitLines = new List<AdjustmentsDocLine>();

			var branch = GlbBranch.CurrentBranch;
			var gstRegistry = CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.GetFallBackValueAtAllLevels(branch.Company.PK.ToGuid(), branch.PK.ToGuid(), Guid.Empty);

			foreach (var docPage in pages)
			{
				var asAccountedLine1 = docPage.AsAccountForDocLine1;
				var asAccountedLine2 = docPage.AsAccountForDocLine2;
				if (!asAccountedLine1.OriginalLineNo.IsEmpty)
				{
					asAccountedLines.Add(asAccountedLine1);
				}
				if (!asAccountedLine2.OriginalLineNo.IsEmpty)
				{
					asAccountedLines.Add(asAccountedLine2);
				}

				var asClaimedLine1 = docPage.AsClaimForDocLine1;
				var asClaimedLine2 = docPage.AsClaimForDocLine2;
				var asClaimedOriginalLineNo1 = asClaimedLine1.OriginalLineNo;
				var asClaimedOriginalLineNo2 = asClaimedLine2.OriginalLineNo;
				if (!asClaimedOriginalLineNo1.IsEmpty)
				{
					if (asClaimedOriginalLineNo1.EndsWith(splitLineStr))
					{
						asClaimedSplitLines.Add(asClaimedLine1);
					}
					else
					{
						asClaimedLines.Add(asClaimedLine1);
					}
				}
				if (!asClaimedOriginalLineNo2.IsEmpty)
				{
					if (asClaimedOriginalLineNo2.EndsWith(splitLineStr))
					{
						asClaimedSplitLines.Add(asClaimedLine2);
					}
					else
					{
						asClaimedLines.Add(asClaimedLine2);
					}
				}
			}

			foreach (var asClaimedLine in asClaimedLines)
			{
				totalCustomsDuties += asClaimedLine.CustomsDuties;
				totalSimaAssessment += IDutyAndTaxDataExtensions.IsSimaAmountPayable(asClaimedLine.SIMACode) ? asClaimedLine.SIMAAssessment : ZDecimal.Zero;
				totalExciseTax += asClaimedLine.ExciseTax;
				var tempGST = asClaimedLine.GST;
				foreach (var splitLine in asClaimedSplitLines.Where(x => x.OriginalLineNo == asClaimedLine.OriginalLineNo + splitLineStr))
				{
					totalCustomsDuties += splitLine.CustomsDuties;
					totalSimaAssessment += IDutyAndTaxDataExtensions.IsSimaAmountPayable(splitLine.SIMACode) ? splitLine.SIMAAssessment : ZDecimal.Zero;
					totalExciseTax += splitLine.ExciseTax;
					tempGST += splitLine.GST;
				}

				var asAccountLine = asAccountedLines.FirstOrDefault(x => x.OriginalLineNo == asClaimedLine.OriginalLineNo);
				if (asAccountLine != null)
				{
					totalCustomsDuties -= asAccountLine.CustomsDuties;
					totalSimaAssessment -= IDutyAndTaxDataExtensions.IsSimaAmountPayable(asAccountLine.SIMACode) ? asAccountLine.SIMAAssessment : ZDecimal.Zero;
					totalExciseTax -= asAccountLine.ExciseTax;
					tempGST -= asAccountLine.GST;
				}

				if (gstRegistry)
				{
					totalGST += tempGST;
				}
				else
				{
					totalGST += tempGST > 0 ? tempGST : ZDecimal.Zero;
				}
			}

			ZDecimal subTotal = totalCustomsDuties + totalSimaAssessment + totalExciseTax;
			totalAmount = totalGST > 0 ? (ZDecimal)(subTotal + totalGST) : subTotal;
			SubTotal = subTotal.IsEmpty ? string.Empty : subTotal.ToString(2);
			TotalCustomDuties = totalCustomsDuties.IsEmpty ? string.Empty : totalCustomsDuties.ToString(2);
			TotalSIMAAssessment = totalSimaAssessment.IsEmpty ? string.Empty : totalSimaAssessment.ToString(2);
			TotalExciseTax = totalExciseTax.IsEmpty ? string.Empty : totalExciseTax.ToString(2);
			TotalGST = totalGST.IsEmpty ? string.Empty : totalGST.ToString(2);
			AmountDueReceiverGeneralCanada = totalAmount > 0m ? totalAmount.ToString(2) : string.Empty;
			AmountDueClaimant = totalAmount < 0m ? ((ZDecimal)(-totalAmount)).ToString(2) : string.Empty;
		}
	}
}
