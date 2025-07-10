using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public abstract class AdjustmentsDocFooter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AdjustmentsDocFooter()
		{
		}

		public AdjustmentsDocFooter(JobDeclaration declaration, IEnumerable<AdjustmentsDocPage> pages = null)
		{
			Initialise(declaration, pages);
		}

		protected virtual void Initialise(JobDeclaration declaration, IEnumerable<AdjustmentsDocPage> pages = null)
		{
			DocAttached = declaration.CA_IsDocAttached ? "X" : string.Empty;
			Interest = declaration.CA_ClaimedInterestAmount.IsEmpty ? string.Empty : declaration.CA_ClaimedInterestAmount.ToString(2);
			JustificationForRequest = declaration.CA_JustificationForRequest;
			Under = declaration.CA_Under;
			Explanation = declaration.CA_B2Explanation;
			var broker = declaration.DeclarantOnEntryDocsBrokerOnB2 ?? declaration.CusAgent;
			if (broker != null)
			{
				BrokerName = broker.GS_FullName;
				BrokerPhone = broker.GS_WorkPhone;
				if (declaration.IsPrintBrokerSignatureImage)
				{
					BrokerSignatureImage = broker.SignatureImage;
				}
				var builder = new ZStringBuilder();
				var branch = broker.HomeBranch;
				var company = branch != null ? branch.Company : null;
				builder.AppendIfNotEmpty(company != null ? company.GC_Name : ZString.Empty);
				builder.AppendIfNotEmpty(branch != null ? branch.GB_BranchName : ZString.Empty);
				BrokerAgent = builder.ToStringWithNewLineBetweenAppends();
			}
			Date = ZDateTime.Now;
			SetTotalValues(declaration, pages);
		}

		protected virtual void SetTotalValues(JobDeclaration declaration, IEnumerable<AdjustmentsDocPage> pages = null)
		{
			var totalCustomsDuties = declaration.CA_AnySightDepositAmount;
			var totalSIMAAssessment = ZDecimal.Zero;
			var totalExciseTax = ZDecimal.Zero;
			var totalGST = ZDecimal.Zero;

			var branch = GlbBranch.CurrentBranch;
			var gstRegistry = CACustomsDataRegistry.Instance.DefaultOffsetNegativeGSTLinesForTotalsInB2Form.GetFallBackValueAtAllLevels(branch.Company.PK.ToGuid(), branch.PK.ToGuid(), Guid.Empty);

			foreach (JobComInvoiceHeader claimedInvoice in declaration.B2AsClaimedForInvoices)
			{
				foreach (JobComInvoiceLine claimedLine in claimedInvoice.InvoiceLines)
				{
					if (claimedLine.CA_IsSeeded || !claimedLine.CA_OriginalLineNo.EndsWith(JobComInvoiceLine.SplitLine))
					{
						claimedLine.ResetDutyAndTaxManager();
						var dutyAndTaxManager = claimedLine.DutyAndTaxManager;
						var asAccountedLine = claimedLine.CorrespondingAsAccountedForInvoiceLine;
						totalCustomsDuties += dutyAndTaxManager.DutiesTotalAmount;
						totalSIMAAssessment += dutyAndTaxManager.SIMAPayableAmount;
						totalExciseTax += dutyAndTaxManager.ExciseTaxesTotalAmount;

						var tempGST = dutyAndTaxManager.GSTTaxesTotalAmount;
						foreach (var claimedSplitLine in
							claimedInvoice.InvoiceLines.Cast<JobComInvoiceLine>().
							Where(x => !x.CA_IsSeeded &&
							x.JI_ParentID == claimedLine.JI_ParentID &&
							x.CA_OriginalLineNo == claimedLine.CA_OriginalLineNo + JobComInvoiceLine.SplitLine))
						{
							claimedSplitLine.ResetDutyAndTaxManager();
							dutyAndTaxManager = claimedSplitLine.DutyAndTaxManager;
							totalCustomsDuties += dutyAndTaxManager.DutiesTotalAmount;
							totalSIMAAssessment += dutyAndTaxManager.SIMAPayableAmount;
							totalExciseTax += dutyAndTaxManager.ExciseTaxesTotalAmount;
							tempGST += dutyAndTaxManager.GSTTaxesTotalAmount;
						}

						if (asAccountedLine != null)
						{
							asAccountedLine.ResetDutyAndTaxManager();
							dutyAndTaxManager = asAccountedLine.DutyAndTaxManager;
							totalCustomsDuties -= dutyAndTaxManager.DutiesTotalAmount;
							totalSIMAAssessment -= dutyAndTaxManager.SIMAPayableAmount;
							totalExciseTax -= dutyAndTaxManager.ExciseTaxesTotalAmount;
							tempGST -= dutyAndTaxManager.GSTTaxesTotalAmount;
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
				}
			}

			ZDecimal subTotal = totalCustomsDuties + totalSIMAAssessment + totalExciseTax;
			ZDecimal amountDue = totalGST > 0 ? (ZDecimal)(subTotal + totalGST) : subTotal;
			SubTotal = subTotal.IsEmpty ? string.Empty : subTotal.ToString(2);
			TotalCustomDuties = totalCustomsDuties.IsEmpty ? string.Empty : totalCustomsDuties.ToString(2);
			TotalSIMAAssessment = totalSIMAAssessment.IsEmpty ? string.Empty : totalSIMAAssessment.ToString(2);
			TotalExciseTax = totalExciseTax.IsEmpty ? string.Empty : totalExciseTax.ToString(2);
			TotalGST = totalGST.IsEmpty ? string.Empty : totalGST.ToString(2);
			AmountDueReceiverGeneralCanada = amountDue > 0m ? amountDue.ToString(2) : string.Empty;
			AmountDueClaimant = amountDue < 0m ? ((ZDecimal)(-amountDue)).ToString(2) : string.Empty;
			TotalDutyAndSIMAAndTaxAndGST = totalCustomsDuties + totalSIMAAssessment + totalExciseTax + totalGST;
		}

		public ZString DocAttached { get; private set; }
		public ZString TotalCustomDuties { get; protected set; }
		public ZString TotalSIMAAssessment { get; protected set; }
		public ZString TotalExciseTax { get; protected set; }
		public ZString SubTotal { get; protected set; }
		public ZString TotalGST { get; protected set; }
		public ZDecimal TotalDutyAndSIMAAndTaxAndGST { get; protected set; }
		public ZString Interest { get; private set; }
		public ZString JustificationForRequest { get; private set; }
		public ZString Under { get; private set; }
		public ZString Explanation { get; private set; }
		public ZString BrokerName { get; private set; }
		public ZString BrokerPhone { get; private set; }
		public Image BrokerSignatureImage { get; private set; }
		public ZString BrokerAgent { get; private set; }
		public ZDateTime Date { get; private set; }
		public ZString AmountDueReceiverGeneralCanada { get; protected set; }
		public ZString AmountDueClaimant { get; protected set; }
		public ZDecimal Deposit { get; set; }
		public ZString WarehouseNumber { get; set; }
		public ZString CargoControlNumber { get; set; }
		public ZString CarrierCodeAtImportation { get; set; }
		public ZDecimal TotalAllDutyAndTaxes { get; set; }
	}
}
