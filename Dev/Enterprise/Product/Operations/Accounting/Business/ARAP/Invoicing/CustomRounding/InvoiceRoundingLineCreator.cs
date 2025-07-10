using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface IInvoiceRoundingLineCreator
	{
		void AddRoundingLine(InvoicingBase invoicingBase);

		bool IsRoundingLine(InvoicingLineBase invoicingLine);
	}

	class InvoiceRoundingLineCreator : IInvoiceRoundingLineCreator
	{
		public InvoiceRoundingLineCreator()
		{
			invoiceAmountRounder_constructorInitializedOnly = new InvoiceAmountRounder();
			taxHelper_constructorInitializedOnly = new TaxHelper();
		}

		IInvoiceAmountRounder InvoiceAmountRounder => invoiceAmountRounder_constructorInitializedOnly;
		IInvoiceAmountRounder invoiceAmountRounder_constructorInitializedOnly;

		ITaxHelper TaxHelper => taxHelper_constructorInitializedOnly;
		ITaxHelper taxHelper_constructorInitializedOnly;

		void IInvoiceRoundingLineCreator.AddRoundingLine(InvoicingBase invoice)
		{
			if (ShouldApplyRounding(invoice))
			{
				var roundingCollection = AccountingConfigurationRegistry.Instance.InvoiceTotalRounding.GetFallBackValueAtAllLevels(invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
				var roundingChargeCode = AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.GetFallBackValueAtAllLevels(invoice.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
				var invoiceTotalRounding = roundingCollection.Cast<InvoiceTotalRounding>().FirstOrDefault(x => x.Currency == invoice.AH_RX_NKTransactionCurrency);
				if (invoiceTotalRounding != null && roundingChargeCode != Guid.Empty)
				{
					Action<string> reportError = (string error) =>
					{
						error += FormattableString.Invariant($"\r\nLedger: {invoice.AH_Ledger}, Transaction Type: {invoice.AH_TransactionType}, Total OS Amount: {invoice.AH_OSTotalAmount}, Currency: {invoice.AH_RX_NKTransactionCurrency}");
						ErrorReporter.ReportOnce("InvoiceRoundingLineCreator_AddRoundingLine", error);
					};

					var roundingAmount = InvoiceAmountRounder.ApplyCustomRounding(invoice.AH_OSTotalAmount, invoice.AH_RX_NKTransactionCurrency, invoiceTotalRounding.RoundingOptionEnum, invoiceTotalRounding.RoundToCurrencyUnitEnum, reportError);

					if (roundingAmount != 0M)
					{
						var totalAmountBeforeRounding = invoice.AH_OSTotalAmount;

						var prevIgnoreValidationSuspended = invoice.IgnoreValidationSuspended;
						using (new DisposableAction(() => invoice.IgnoreValidationSuspended = false, () => invoice.IgnoreValidationSuspended = prevIgnoreValidationSuspended))
						using (invoice.GetValidationSuspenderForLines())
						{
							var line = (InvoicingLineBase)invoice.Lines.AddNew();
							line.SetContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation);

							line.GenericCharge = roundingChargeCode;
							if (invoice.Job != null)
							{
								line.AL_GB = invoice.Job.JH_GB;
								line.AL_GE = invoice.Job.JH_GE;
							}
							else
							{
								line.AL_GB = invoice.AH_GB;
								line.AL_GE = invoice.AH_GE;
							}

							if (TaxHelper.IsGSTMandatory(line))
							{
								var notReportTaxID = AccTaxRate.GetNOTREPORTTaxID(invoice.Factory, invoice.Company);
								if (notReportTaxID != null)
								{
									line.AL_AT = notReportTaxID.PK;
								}
							}
							else
							{
								line.AL_AT = ZGuid.Empty;
							}

							line.AL_RX_NKTransactionCurrency = invoice.AH_RX_NKTransactionCurrency;
							line.AL_ExchangeRate = invoice.AH_ExchangeRate;
							line.AL_OSExTaxAmount = roundingAmount;
						}

						SetRoundingAmountCachedValue(invoice, invoice.AH_OSTotalAmount);

						var totalAmountAfterRounding = totalAmountBeforeRounding + roundingAmount;
						if (totalAmountAfterRounding != invoice.AH_OSTotalAmount)
						{
							reportError(FormattableString.Invariant($"transaction OS total amount {invoice.AH_OSTotalAmount} is not equal to original amount {totalAmountBeforeRounding} + rounding amount {roundingAmount}."));
						}
					}
				}
			}
		}

		bool IInvoiceRoundingLineCreator.IsRoundingLine(InvoicingLineBase invoicingLine)
		{
			return (invoicingLine is ARInvoiceLine || invoicingLine is ARCreditNoteLine) &&
				invoicingLine.AL_JH.IsEmpty &&
				AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.Value != Guid.Empty &&
				invoicingLine.AL_AC == AccountingConfigurationRegistry.Instance.InvoiceTotalRoundingChargeCode.Value;
		}

		public static bool ShouldApplyRounding(AccTransactionHeader transactionHeader) => !transactionHeader.IsInDatabase &&
																						transactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable &&
																						(transactionHeader.AH_TransactionType == TransactionTypes.Invoice || transactionHeader.AH_TransactionType == TransactionTypes.CreditNote || transactionHeader.AH_TransactionType == TransactionTypes.AdjustmentNote);

		static Dictionary<ZGuid, ZDecimal> GetRoundingAmountCachedDictionary(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("InvoiceAmountRounder", () => { return new Dictionary<ZGuid, ZDecimal>(); });
		}

		static void SetRoundingAmountCachedValue(AccTransactionHeader invoice, ZDecimal roundedAmount)
		{
			GetRoundingAmountCachedDictionary(invoice.Factory)[invoice.PK] = roundedAmount;
		}

		public static ZDecimal GetRoundingAmountCachedValue(AccTransactionHeader invoice)
		{
			var cachedAmount = GetRoundingAmountCachedDictionary(invoice.Factory);
			if (cachedAmount.ContainsKey(invoice.PK))
			{
				return cachedAmount[invoice.PK];
			}
			else
			{
				return 0M;
			}
		}

#if DEBUG
		public void SubstituteInvoiceAmountRounder_ForTestOnly(IInvoiceAmountRounder replacement) => invoiceAmountRounder_constructorInitializedOnly = replacement;
		public IInvoiceAmountRounder InvoiceAmountRounder_ExposedForTestOnly => InvoiceAmountRounder;
		public void SubstituteTaxHelper_ForTestOnly(ITaxHelper replacement) => taxHelper_constructorInitializedOnly = replacement;
		public ITaxHelper TaxHelper_ExposedForTestOnly => TaxHelper;
		public void SetRoundingAmountCachedValue_ForTestOnly(InvoicingBase invoice, ZDecimal roundedAmount) => SetRoundingAmountCachedValue(invoice, roundedAmount);
#endif

	}
}
