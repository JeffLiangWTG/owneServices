using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseCriticalValidation : TransactionHeaderWithLinesCriticalValidation
	{
		public InvoicingBaseCriticalValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		protected new InvoicingBase Parent
		{
			get { return (InvoicingBase)base.Parent; }
		}

		protected override IEnumerable<CriticalValidationResult> OnSavingOnlyCriticalChecks()
		{
			foreach (var result in base.OnSavingOnlyCriticalChecks())
			{
				yield return result;
			}

			if (Parent.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions && Parent.AH_IsCancelled && Parent.Lines.Any()
					&& (!Parent.IsInDatabase || Parent.AH_IsCancelledInfo.HasChanges))
			{
				var developerMessage = new StringBuilder();
				developerMessage.AppendLine();
				developerMessage.AppendLine(Parent.GetTransactionHeaderWithLinesInfo());

				var charges = Parent.Factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, Parent.Lines.Select(x => x.PK).ToArray())).ToList();
				if (charges.Any())
				{
					charges.ForEach(x => developerMessage.AppendLine(x.GetJobChargeInfo()));
				}

				yield return new CriticalValidationResult(CriticalValidationErrorType.CancelledUnapprovedPayableTransactionsShouldNotHaveLines_2,
														CriticalValidationMessageTemplate.CancelledUnapprovedPayableTransactionsShouldNotHaveLinesErrorMessage,
														developerMessage.ToString());
			}

			yield return CheckTransactionHeaderReferenceATH();
		}

		CriticalValidationResult CheckTransactionHeaderReferenceATH()
		{
#if DEBUG
			if (Globals.IsTest && !InvoicingBase.ShouldCheckTransactionHeaderReferenceATH_ForTestOnly)
			{
				return null;
			}
#endif

			if (Parent.ShouldApplyCompliancePolicyForPrintingAuthorizationNumber && Parent.ComplianceBook != null && Parent.ComplianceBook.XD_PrintingAuthorizationNumber.IsEmpty)
			{
				return new CriticalValidationResult(CriticalValidationErrorType.AbortOnSavingProcess,
					CriticalValidationMessageTemplate.PostInvoiceWhenComplianceBookWithEmptyPrintingAuthorizationnumberErrorMessage);
			}
			return null;
		}
		
		#region GetMoreInfoToErrorMessageWhenLocalInvoiceAmountNotEqualToForeignAmount

		protected override string GetMoreInfoToErrorWhenLocalInvoiceAmountNotEqualToForeignAmount()
		{
			string moreInfoEror = base.GetMoreInfoToErrorWhenLocalInvoiceAmountNotEqualToForeignAmount();

			StringBuilder erroLineInfo = new StringBuilder();
			foreach (InvoicingLineBase line in Parent.Lines)
			{
				ZDecimal sumlocalAndGSTAmount = line.AL_LineAmount + line.AL_GSTVAT;
				ZDecimal osAmount = line.AL_OSAmount;

				if (line.AL_ExchangeRate == 1M && sumlocalAndGSTAmount != osAmount)
				{
					erroLineInfo.AppendLine().AppendLine(Res.GetString("AA930F55-DA6B-460e-A3A7-C23CE7412012", @"Transaction Line:
- Charge Code: {0}
- Job Number: {1}
- Branch: {2}
- Department: {3}
- Local Amount: {4}
- Foreign Currency Amount: {5}",
					GetLineChargeCode(line),
					line.Job != null ? (string)line.Job.JH_JobNum : EmptyCode,
					line.Branch != null ? (string)line.Branch.GB_Code : EmptyCode,
					line.Department != null ? (string)line.Department.GE_Code : EmptyCode,
					sumlocalAndGSTAmount,
					osAmount));

					if (line.ApportionmentChargeImportedFrom != null)
					{
						erroLineInfo.AppendLine(Res.GetString("3291E6F4-40CD-4ab8-8AFB-CECBFD466ACB",
@"- Consol #: {0}", GetConsolNumber(line)));
					}
				}
			}
			if (erroLineInfo.Length > 0)
			{
				erroLineInfo.Insert(0, System.Environment.NewLine + System.Environment.NewLine +
					Res.GetString("906CB3F6-49D3-4c6f-AD74-3256C1312787", "The items causing this issue are as follows:"));
			}

			return moreInfoEror + erroLineInfo.ToString();
		}

		string GetLineChargeCode(InvoicingLineBase line)
		{
			string chargeCode = EmptyCode;
			if (line.ChargeCode != null)
			{
				chargeCode = line.ChargeCode.AC_Code;
			}
			else if (line.GLHeader != null)
			{
				chargeCode = line.GLHeader.AG_AccountNum;
			}
			return chargeCode;
		}

		string GetConsolNumber(InvoicingLineBase line)
		{
			string consolNumber = EmptyCode;
			if (line.ApportionmentChargeImportedFrom != null &&
				line.ApportionmentChargeImportedFrom.ParentConsolCost != null &&
				line.ApportionmentChargeImportedFrom.ParentConsolCost.Consol != null)
			{
				consolNumber = line.ApportionmentChargeImportedFrom.ParentConsolCost.Consol.JK_UniqueConsignRef;
			}
			return consolNumber;
		}

		string EmptyCode
		{
			get { return Res.GetString("524ED94A-4717-4654-B37D-826F71889F56", "Empty"); }
		}

		#endregion
	}
}
