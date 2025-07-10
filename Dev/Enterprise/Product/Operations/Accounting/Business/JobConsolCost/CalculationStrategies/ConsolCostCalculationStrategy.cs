using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public partial class JobConsolCost
	{
		public class ConsolCostCalculationStrategy : ConsolCostCalculationStrategyWithCalculations
		{
			public ConsolCostCalculationStrategy(JobConsolCost cost)
				: base(cost)
			{
			}

			public override void PrepareForPosting()
			{
				base.PrepareForPosting();
				Cost.RemoveNonApplicableCharges();
				Cost.PrepareForPosting();
			}

			public override void OnE6_AK_ChequeBookSet()
			{
				base.OnE6_AK_ChequeBookSet();
				if (!Cost.IsChequeNumberAutoAllocated)
				{
					Cost.E6_ChequeOrReference = Cost.ChequeBook != null ? Cost.ChequeBook.AK_CurrentNo.ToString() : "";
				}
			}

			#region E6_ChequeOrReferenceSet

			public override void OnE6_ChequeOrReferenceSet()
			{
				base.OnE6_ChequeOrReferenceSet();
				if (!Cost.IsChequeNumberAutoAllocated && Cost.E6_AB_BankAccount.IsValid && Cost.E6_AK_ChequeBook.IsValid && !Cost.E6_ChequeOrReference.IsEmpty)
				{
					try
					{
						ZDecimal nextChequeNumber = ZDecimal.Parse(Cost.E6_ChequeOrReference.ToString()) + 1;
						if (Cost.ChequeBook != null)
						{
							AccChequeBook.UpdateCurrentNumber(Cost.ChequeBook.PK, nextChequeNumber);
						}
					}
					catch (OverflowException) { }
					catch (FormatException) { }
					catch (ArgumentNullException) { }
				}
			}

			public override ZString GetPaddedChequeNo(AccValidationHelper validationHelper, ZString rawChequeNumber)
			{
				ZString paddedChequeNo = base.GetPaddedChequeNo(validationHelper, rawChequeNumber);
				if (Cost.BankAccount != null && Cost.E6_PaymentType == ZArchitecture.Core.ReceiptTypes.Cheque)
				{
					paddedChequeNo = validationHelper.PadChequeDigitsWithLeadingZeros(Cost.BankAccount, paddedChequeNo);
				}
				return paddedChequeNo;
			}

			#endregion

			public override void HandleInvoiceNumberChanged()
			{
				base.HandleInvoiceNumberChanged();
				Cost.CalculateDueDate();
				SetupInvoiceAndPaymentDetailsFromRelatedCost();
			}

			public override void HandleCreditorChanged()
			{
				base.HandleCreditorChanged();
				Cost.CalculateDueDate();
				SetupInvoiceAndPaymentDetailsFromRelatedCost();
				Cost.E6_IsForCollectInvoice = Cost.IsCreditorOverseasAgent && AccountingConfigurationRegistry.Instance.AutoPostMasterCollectCharge.Value;
				Cost.TryToDefaultCostPlaceOfSupply();
			}

			void SetupInvoiceAndPaymentDetailsFromRelatedCost()
			{
				JobConsolCost relatedCost = GetRelatedCostWithCompleteInvoiceDetails();
				if (relatedCost != null)
				{
					Cost.E6_InvoiceDate = relatedCost.E6_InvoiceDate;
					Cost.E6_PaymentDate = relatedCost.E6_PaymentDate;
					Cost.E6_PaymentType = relatedCost.E6_PaymentType;
					Cost.E6_AB_BankAccount = relatedCost.E6_AB_BankAccount;
					Cost.E6_AK_ChequeBook = relatedCost.E6_AK_ChequeBook;
					Cost.E6_ChequeOrReference = relatedCost.E6_ChequeOrReference;
					Cost.E6_CostReference = relatedCost.E6_CostReference;
				}
			}

			JobConsolCost GetRelatedCostWithCompleteInvoiceDetails()
			{
				JobConsolCost result = null;
				var parentCollection = ((IBusinessObjectInternals)Cost).ParentCollections[0];
				var filteredParentCollection = parentCollection is JobConsolCostCollection ? (JobConsolCostCollection)parentCollection : ((JobConsolCostFilteredCollection)parentCollection).CollectionToFilter;
				foreach (JobConsolCost otherCost in filteredParentCollection)
				{
					if (otherCost.PK != Cost.PK &&
						otherCost.E6_InvoiceNum == Cost.E6_InvoiceNum &&
						otherCost.E6_OH_Creditor == Cost.E6_OH_Creditor &&
						(!otherCost.E6_InvoiceDate.IsEmpty || !otherCost.E6_PaymentDate.IsEmpty))
					{
						result = otherCost;
						break;
					}
				}
				return result;
			}

			public override void OnE6_OH_CreditorSet()
			{
				if (Cost.Creditor != null && !Cost.Creditor.CompanyData.OB_RX_NKAPDefltCurrency.IsEmpty && Cost.Creditor.CompanyData.OB_RX_NKAPDefltCurrency != Cost.E6_RX_NKCurrency)
				{
					Cost.E6_RX_NKCurrency = Cost.Creditor.CompanyData.OB_RX_NKAPDefltCurrency;
				}
			}
		}
	}
}
