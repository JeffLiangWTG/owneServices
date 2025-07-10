using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Helpers;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class InvoicingBaseReversalValidation : TransactionReversalValidation
	{
		public InvoicingBaseReversalValidation(InvoicingBase parent, IDataRefreshBusUpdateActionDecider dataRefreshBusUpdateActionDecider)
			: base(parent, dataRefreshBusUpdateActionDecider)
		{
			Parent = parent;
		}

		new InvoicingBase Parent { get; }

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAH_Calc_AmendStatusCode();
			ValidateReversalStatusCode();
		}

		protected override void CheckAH_InvoiceDate()
		{
			base.CheckAH_InvoiceDate();
			if ((Parent is APInvoice || Parent is APCreditNote || Parent is APAdjustmentNote) &&
				AccountingMasterFilesRegistry.Instance.PreventInvoiceDateGreaterThanPostDate.Value && Parent.AH_InvoiceDate.Date > Parent.AH_PostDate.Date)
			{
				Parent.AH_InvoiceDateInfo.AddError(Res.GetString("A7102308-92B5-4470-8791-0E67E64D09FB", @"Invoice or Credit Note Date must be earlier or same as the Post Date. This is controlled by the registry Accounting > Payable Defaults > Default Settings > Prevent Posting Invoice Date Greater Than Post Date."));
			}

			if (Parent.ComplianceNumberAllocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				CheckComplianceNumberAllocationDate(Parent.AH_InvoiceDateInfo);
			}
		}

		protected override void CheckAH_PostDate()
		{
			base.CheckAH_PostDate();

			if (Parent.ComplianceNumberAllocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
			{
				CheckComplianceNumberAllocationDate(Parent.AH_PostDateInfo);
			}
		}

		void CheckComplianceNumberAllocationDate(ZPropertyInfo field)
		{
			var invoicingBase = Parent;
			if (invoicingBase.Company.Country.SupportComplianceSubType
				&& invoicingBase.IsComplianceNumberAllocationMandatory
				&& invoicingBase.ShouldAllocateComplianceNumberOnPosting())
			{
				var complianceError = Parent.AssignComplianceSubTypeAndCheckComplianceErrors();
				if (!complianceError.IsEmpty)
				{
					field.AddError(complianceError);
				}
			}
		}

		protected override void CheckSupportingDocumentNumber()
		{
			if (string.IsNullOrEmpty(Parent.SupportingDocumentNumber) && AccountingUtils.IsVietnamCompanyEInvoicingEnabled && Parent is ARCreditNote arCreditNote && arCreditNote.OriginalTransaction?.EInvoicingTransactionPivotSubmitted != null)
			{
				Parent.SupportingDocumentNumberInfo.AddError(AccountingConstants.AccountingSupportingDocumentNumberErrorMessage.EmptySupportingDocumentNumber);
			}
		}

		public void ValidateAH_Calc_AmendStatusCode()
		{
			ValidateCalculatedProperty(Parent.AH_Calc_AmendStatusCodeInfo);
		}

		protected virtual void CheckAH_Calc_AmendStatusCode()
		{
			var amendStatusCodeValidationInstanceProvider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.Country.Code) as IInstanceProvider<IAmendStatusCodeValidationProvider>;
			var amendStatusCodeValidationProvider = amendStatusCodeValidationInstanceProvider?.Get();
			amendStatusCodeValidationProvider?.ValidateAmendStatusCodeForInvoiceReversal(Parent);
		}

		public void ValidateReversalStatusCode()
		{
			ValidateCalculatedProperty(Parent.ReversalStatusCodeInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "CheckReversalStatusCode is used internally")]
		void CheckReversalStatusCode()
		{
			if (Parent.IsReversalStatusCodeAllowed)
			{
				MandatoryValidation.CheckEntered(Parent.ReversalStatusCodeInfo);

				if (!Parent.ReversalStatusCodeInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.ReversalStatusCodeInfo);
				}
			}
		}
	}
}
