using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class IncompleteInvoicingBaseValidation : InvoicingBaseCommonValidation
	{
		public IncompleteInvoicingBaseValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		protected new InvoicingBase Parent => base.Parent;

		protected override void CheckAH_OHCore()
		{
			base.CheckAH_OHCore();
			MandatoryValidation.CheckEntered(Parent.AH_OHInfo);
			ListValidation.ErrorIfInvalidPK(Parent.AH_OHInfo);
		}

		protected override void CheckAH_TransactionNum()
		{
			base.CheckAH_TransactionNum();

			if (!Parent.IsSelfBillingInvoice)
			{
				MandatoryValidation.CheckEntered(Parent.AH_TransactionNumInfo);

				if (!Parent.AH_OH.IsEmpty)
				{
					if ((AccountingUtils.INTransactionNumberExists(Parent.AH_TransactionType, Parent.AH_TransactionNum, Parent.AH_OH, Parent.PK, Parent.AH_InvoiceDate)).HasNotification)
					{
						Parent.AH_TransactionNumInfo.AddError(Res.GetString("01c5294e-2ba6-41af-bfcc-edf6724aa59e", "The transaction number is already in use by Incomplete Transaction. Please select another one."));
					}
				}
			}
		}

		protected override void CheckAH_InvoiceDate()
		{
			base.CheckAH_InvoiceDate();
			MandatoryValidation.CheckEntered(Parent.AH_InvoiceDateInfo);
		}

		protected override void CheckAH_DueDate()
		{
			base.CheckAH_DueDate();
			CheckAH_DueDateMustAfterInvoiceDate();
		}

		protected override void CheckAH_RX_NKTransactionCurrency()
		{
			base.CheckAH_RX_NKTransactionCurrency();
			MandatoryValidation.CheckEntered(Parent.AH_RX_NKTransactionCurrencyInfo);
		}

		protected override void CheckAH_ExchangeRate()
		{
			base.CheckAH_ExchangeRate();
			MandatoryValidation.CheckNotNegative(Parent.AH_ExchangeRateInfo);
			MandatoryValidation.CheckEntered(Parent.AH_ExchangeRateInfo);
		}

		protected override void CheckAH_Desc()
		{
			base.CheckAH_Desc();
			MandatoryValidation.CheckEntered(Parent.AH_DescInfo);
		}

		protected override void CheckAH_PostDate()
		{
		}

		protected void CheckExpectedInvoiceTotal()
		{
		}

		protected void CheckExpectedInvoiceTaxTotal()
		{
		}

		protected void CheckExpectedInvoiceExclTaxTotal()
		{
		}

		protected override void CheckIsSelfBillingInvoice()
		{
			base.CheckIsSelfBillingInvoice();

			if (!Parent.IsSelfBillingInvoiceInfo.HasErrors() && Parent.AH_TransactionCategory == Constants.TransactionCategory.Codes.SelfBilling)
			{
				if (Parent.GetNextIncompleteSelfBillingInvoiceTransactionCount() > byte.MaxValue)
				{
					Parent.IsSelfBillingInvoiceInfo.AddError(Res.GetString("2e593c9c-0dd1-44ad-bc6a-0a522df10133", @"The maximum supported number of Incomplete Self Billing Invoices ({0}) have already been entered into the system.
You cannot create any more Incomplete Self Billing Invoices at this time.", byte.MaxValue));
				}
			}
		}

		public void ValidateExpectedInvoiceTotal()
		{
			ValidateCalculatedProperty((Parent).ExpectedInvoiceTotalInfo);
			ValidateCalculatedProperty((Parent).ExpectedInvoiceTaxTotalInfo);
			ValidateCalculatedProperty((Parent).ExpectedInvoiceExclTaxTotalInfo);
		}

		protected override void ValidateAllCore()
		{
			base.ValidateAllCore();
			ValidateExpectedInvoiceTotal();
		}

		protected override void CheckAH_OSTotalAmountCore_FromInvoicingBaseCommon()
		{
			base.CheckAH_OSTotalAmountCore_FromInvoicingBaseCommon();

			CheckAH_OSTotalAmount_Implementation();
		}

		protected override bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get
			{
				return true;
			}
		}

		protected override void CheckAH_GovernmentAllocatedID()
		{
			base.CheckAH_GovernmentAllocatedID();

			base.CheckAH_GovernmentAllocatedID_BasedOnRegistry(Parent);
		}

		protected override void CheckAH_OA_InvoiceAddressOverride()
		{
			base.CheckAH_OA_InvoiceAddressOverride();

			ValidateAH_GovernmentAllocatedID();
		}
	}
}
