using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Validation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class InvoicingBaseCommonValidation : TransactionHeaderWithLinesValidation
	{
		protected InvoicingBaseCommonValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		protected new InvoicingBase Parent => (InvoicingBase)base.Parent;

		#region InvoiceTransaction

		InvoicingBase fInvoicingBase;

		protected InvoicingBase InvoiceTransaction
		{
			get { return fInvoicingBase ?? (fInvoicingBase = Parent); }
		}

		#endregion

		protected sealed override void CheckAH_OH()
		{
			base.CheckAH_OH();

			CheckAH_OHCore();

			if (Parent.ShouldCreateOrganisationCodeMatchingRule)
			{
				if (Parent.ImportedCreditor.IsEmpty)
				{
					Parent.AH_OHInfo.AddWarning(Res.GetString("6c8a052c-5ecd-4e97-ab4b-06c6bb6fd597", "Organization is not found for imported XML code '{0}'. New matching rule will be created for foreign code '{0}'.",
						Parent.ImportedCreditorXmlCode));
				}
				else
				{
					Parent.AH_OHInfo.AddWarning(Res.GetString("0D7B5B30-6D5E-4D15-A977-598F459499D0", "Creditor value is different to a value matched by default. Organization matching rule for foreign code '{0}' will be updated.",
						Parent.ImportedCreditorXmlCode));
				}
			}

			ValidateAH_OA_InvoiceAddressOverride();
			CheckExporterExemption();
		}

		void CheckExporterExemption()
		{
			var checkDate = Parent.AH_PostDate.IsEmpty ?
				(Parent.AH_InvoiceDate.IsEmpty ? ZDateTime.Now : Parent.AH_InvoiceDate)
				: Parent.AH_PostDate;

			var (warnings, errors) = ExporterExemptionValidationHelper.CheckExporterExemption(Parent.Header, Parent.AH_Ledger, checkDate, Parent.Lines);
			if (!warnings.IsEmpty)
			{
				Parent.AH_OHInfo.AddWarning(warnings);
			}
			if (!errors.IsEmpty)
			{
				Parent.AH_OHInfo.AddError(errors);
			}
		}

		protected virtual void CheckAH_OHCore()
		{
		}

		protected override void CheckAH_OA_InvoiceAddressOverride()
		{
			ValidateAH_OH();
			foreach (var notification in Parent.AH_OHInfo.Notifications)
			{
				if (notification.Type.Equals(CargoWise.ComponentModel.NotificationType.Error))
				{
					Parent.AH_OA_InvoiceAddressOverrideInfo.AddError(notification.Message);
				}
				if (notification.Type.Equals(CargoWise.ComponentModel.NotificationType.Warning))
				{
					Parent.AH_OA_InvoiceAddressOverrideInfo.AddWarning(notification.Message);
				}
			}
			Parent.OrganisationAddressWithContact.RefreshBinding();
		}

		protected override void CheckAH_PostedToEFT()
		{
			base.CheckAH_PostedToEFT();
			if (Parent.AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
				&& (ZBool)Parent.AH_PostedToEFTInfo.Value != AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.Value)
			{
				var securityNotAllowedPath = string.Empty;
				if (Parent is APInvoice && !Env.Security.AllowAPInvoiceChangeDefaultUseJobExchangeRate.IsAllowed)
				{
					securityNotAllowedPath = Env.Security.AllowAPInvoiceChangeDefaultUseJobExchangeRate.DisplayTextPathToSecurityRight;
				}
				else if (Parent is APCreditNote && !Env.Security.AllowAPCreditNoteChangeDefaultUseJobExchangeRate.IsAllowed)
				{
					securityNotAllowedPath = Env.Security.AllowAPCreditNoteChangeDefaultUseJobExchangeRate.DisplayTextPathToSecurityRight;
				}
				if (!string.IsNullOrEmpty(securityNotAllowedPath))
				{
					Parent.AH_PostedToEFTInfo.AddError(Res.GetString("a9a84358-b987-4f02-9f7a-2e703b295b54", @"You do not have sufficient security rights to modify the 'Use Job Exchange Rate' field.
Contact your system administrator for rights to modify this field.
The security right can be found in the following location:
{0}", securityNotAllowedPath));
				}
			}
		}

		protected override void CheckAH_ExchangeRate()
		{
			base.CheckAH_ExchangeRate();

			var invoiceTransaction = InvoiceTransaction;

			var isForeignDefault = GetInvoicePostingExchangeRateOptionAP(InvoicePostingExchangeRateCurrencyType.Code.Foreign) == ExRateOption.Default.Code;

			var skipValidationForOverridingExchangeRate = Parent.AH_OverrideExchangeRate && !Parent.UseJobExchangeRate && !Parent.IsLocalCurrencyTransaction && !isForeignDefault;

			if (!skipValidationForOverridingExchangeRate)
			{
				if (!Parent.IsReversalTransaction && (!invoiceTransaction.IsPosted || invoiceTransaction.IsIncompleteInvoice || invoiceTransaction.IsAllocatingInvoice))
				{
					var rateType = Parent.RateType;
					var notification = ExchangeRateCalculator.CheckExchangeRate(Parent.AH_RX_NKTransactionCurrency, Parent.IsLocalCurrencyTransaction, Parent.Company, rateType, Parent.GetExRateLedger(), Parent.AH_InvoiceDate, Parent.AH_PostDate, Parent.InvoiceTaxDate, Parent.AH_ExchangeRate);
					if (notification != null)
					{
						if ((Parent.IsAmendingTransaction && Parent.IsCopyExRateForAmendingAllowedByRegistry()) || Parent.UseJobExchangeRate)
						{
							Parent.AH_ExchangeRateInfo.AddWarning(notification.Message);
						}
						else
						{
							Parent.AH_ExchangeRateInfo.AddError(notification.Message);
						}
					}
				}
			}
		}

		public static string GetInvoicePostingExchangeRateOptionAP(string invoiceCurrencyType)
		{
			return AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.Value
				.OfType<InvoicePostingExRateOption>()
				.FirstOrDefault(x => x != null && x.InvoiceCurrencyType == invoiceCurrencyType)
				.ExRateOption;
		}

		internal void ValidateAH_OSTotalAmountAfterSuspenderForBatchLineChanges()
		{
			ValidateAH_OSTotalAmountAfterSuspenderForBatchLineChangesCore();
		}

		protected virtual void ValidateAH_OSTotalAmountAfterSuspenderForBatchLineChangesCore()
		{
			ValidateAH_OSTotalAmount();
		}

		protected sealed override void CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines()
		{
			if (!InvoiceTransaction.ValidateAH_OSTotalAmountSuspenderForBatchLineChanges.IsSuspended)
			{
				base.CheckAH_OSTotalAmountCore_FromTransactionHeaderWithLines();
				CheckAH_OSTotalAmountCore_FromInvoicingBaseCommon();
			}
		}

		protected virtual void CheckAH_OSTotalAmountCore_FromInvoicingBaseCommon()
		{
		}

		protected virtual void CheckAH_OSTotalAmount_Implementation()
		{
			if (Parent.AH_OSTotalAmount == 0M && ZeroBalanceValidationHelper.IsZeroOSTotalAmountInvalid(Parent, out string errorMessage))
			{
				Parent.AH_OSTotalAmountInfo.AddError(errorMessage);
			}
			else if (Parent.AH_OSTotalAmount < 0M && InvoiceTransaction.Lines.Count > 0 &&
				(InvoiceTransaction is ARInvoice || InvoiceTransaction is ARCreditNote
				|| InvoiceTransaction is APInvoice || InvoiceTransaction is APCreditNote))
			{
				Parent.AH_OSTotalAmountInfo.AddError(Res.GetString("1238aa99-b142-454f-a633-7278de0e7e9b", "The sum of the transaction lines should be greater than zero."));
			}
			else if (GetLevelAuthorizationRequired())
			{
				Parent.AH_OSTotalAmountInfo.AddWarning(Res.GetString("143424d9-63a2-4caa-9218-ab00b49ffa00", "You do not have rights to create transaction for this amount without authorization."));
			}

			if (InvoiceTransaction.IsStampDutyApplicable() && InvoiceTransaction.ShouldAddStampDuty() && AccountingConfigurationRegistry.Instance.StampDutyChargeCode.Value == Guid.Empty)
			{
				Parent.AH_OSTotalAmountInfo.AddError(Res.GetString("1fab7eca-73b5-41d7-ad3f-010fd1f3ae1d", "The invoice being posted attracts stamp duty, but there is no 'Stamp Duty Charge Code' defined in the registry. Please define an appropriate charge code in the registry under Accounting > Receivable Defaults > Default Settings > Stamp Duty Charge Code."));
			}
		}

		protected virtual bool GetLevelAuthorizationRequired()
		{
			return InvoiceTransaction.LevelAuthorizationRequired;
		}

		protected virtual void CheckIsSelfBillingInvoice()
		{
			if (InvoiceTransaction.IsSelfBillingInvoice && InvoiceTransaction.Header != null && !InvoiceTransaction.Header.CompanyData.OB_APCostsSelfBilled)
			{
				InvoiceTransaction.IsSelfBillingInvoiceInfo.AddError(Res.GetString("eec7d748-33a1-4a3d-a6e4-c068d1cee753", "You can’t mark this AP invoice as a Self Billing Invoice because the Creditor is not flagged to receive Self Billing Invoices."));
			}
		}

		public void ValidateIsSelfBillingInvoice()
		{
			ValidateCalculatedProperty(InvoiceTransaction.IsSelfBillingInvoiceInfo);
		}

		protected void ValidateComplianceSequenceNotNull()
		{
			InvoiceTransaction.RemoveRowError(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
			ValidateComplianceSequenceNotNullCore();
		}
		protected virtual void ValidateComplianceSequenceNotNullCore()
		{
		}

		public sealed override void ValidateAll()
		{
			using (ValidateAllInProgressSuspender.GetSuspender())
			{
				ValidateAllCore();
			}
		}

		protected virtual void ValidateAllCore()
		{
			Parent.InitializeDuplicateLinesSequenceLookup();
			Parent.ClearValidatedConsolCostPKList();

			base.ValidateAll();
			ValidateIsSelfBillingInvoice();
			ValidateComplianceSequenceNotNull();
		}

		protected FunctionalitySuspender ValidateAllInProgressSuspender
		{
			get { return validateAllInProgressSuspender ?? (validateAllInProgressSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender validateAllInProgressSuspender;
	}
}
