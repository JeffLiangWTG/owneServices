using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public sealed class CountrySpecificValidationHelper
	{
		CountrySpecificValidationHelper()
		{ }

		public static (bool, bool) ShouldAddErrorOrWarningWhenIsOutsideExpectedTaxAmount(InvoicingLineBase invoiceLineBase)
		{
			var shouldAddErrorOrWarning = false;
			var addError = false;
			var supportedCountries = new string[] { Core.Constants.CountryCodes.Portugal };
			var supportedTransactionTypes = new string[] { TransactionTypes.Invoice, TransactionTypes.CreditNote };

			if (supportedCountries.Contains(GlbCompany.CurrentCompany.Country.Code.ToString())
				&& invoiceLineBase.InvoiceBase.AH_Ledger == LedgerTypes.AccountsReceivable
				&& supportedTransactionTypes.Contains(invoiceLineBase.TransactionHeader.AH_TransactionType.ToString())
				&& !invoiceLineBase.InvoiceBase.RegistryValueForCalculateTaxAtHeaderLevel
				&& invoiceLineBase.CalculateExpectedOSTaxAmount() != invoiceLineBase.AL_OSTaxAmount)
			{
				shouldAddErrorOrWarning = true;
				var notSupportedComplianceSubTypes = new string[]
				{
					PortugalComplianceInfo.ComplianceSubTypeCodes.CBC,
					PortugalComplianceInfo.ComplianceSubTypeCodes.CBD,
					PortugalComplianceInfo.ComplianceSubTypeCodes.CBI,
					PortugalComplianceInfo.ComplianceSubTypeCodes.LCD,
					PortugalComplianceInfo.ComplianceSubTypeCodes.LCR,
					PortugalComplianceInfo.ComplianceSubTypeCodes.LTX,
					PortugalComplianceInfo.ComplianceSubTypeCodes.TCM,
					PortugalComplianceInfo.ComplianceSubTypeCodes.TDM,
					PortugalComplianceInfo.ComplianceSubTypeCodes.TXM,
				};

				if (!notSupportedComplianceSubTypes.Contains(invoiceLineBase.TransactionHeader.AH_ComplianceSubType.ToString()))
				{
					addError = true;
				}
			}
			return (shouldAddErrorOrWarning, addError);
		}

		public static bool AddWarningIfDateIsInTheFuture(ZPropertyInfo datePropertyInfo)
		{
			var result = false;
			var supportedCountries = new string[] { Core.Constants.CountryCodes.Portugal };
			if (supportedCountries.Contains(GlbCompany.CurrentCompany.Country.Code.ToString()))
			{
				var supportedPropertyNames = new string[] {
					AutoAccTransactionHeader.Schema.AH_InvoiceDate,
					AutoJobCharge.Schema.JR_APInvoiceDate,
					PeriodicInvoiceBase.Schema.InvoiceDate,
					ChangeTransactionDatesBusinessObject.Schema.InvoiceDate
				};
				if (supportedPropertyNames.Contains(datePropertyInfo.Name) && datePropertyInfo.Value is ZDateTime && ((ZDateTime)datePropertyInfo.Value).Date > ZDateTime.Today)
				{
					datePropertyInfo.AddWarning(Res.GetString("6edd54c6-e57c-46de-878b-d6eabcc77715", "{0} is in the future. Please check the {0} against the current system date and time. If this invoice is posted, future invoices cannot use current or previous date.", datePropertyInfo.HumanReadableName));
					result = true;
				}
			}
			return result;
		}

		public static bool ShouldValidateAL_OSTaxAmount(InvoicingLineBase invoicingLineBase)
		{
			var result = false;
			var supportedCountries = new string[] { Core.Constants.CountryCodes.Portugal };
			var supportedTransactionTypes = new string[] { TransactionTypes.Invoice, TransactionTypes.CreditNote };

			if (supportedCountries.Contains(GlbCompany.CurrentCompany.Country.Code.ToString())
				&& invoicingLineBase.InvoiceBase.AH_Ledger == LedgerTypes.AccountsReceivable
				&& supportedTransactionTypes.Contains(invoicingLineBase.TransactionHeader.AH_TransactionType.ToString())
				&& invoicingLineBase.Validation is InvoicingLineBaseValidation)
			{
				result = true;
			}

			return result;
		}

		public static IEnumerable<InvoicingBase> GetTransactionsWithWarningAboutInvoiceDateInTheFuture(TransactionCreatorHashtable transactions, ZDateTime today)
		{
			var result = Array.Empty<InvoicingBase>();
			var supportedCountries = new string[] { Core.Constants.CountryCodes.Portugal };
			if (supportedCountries.Contains(GlbCompany.CurrentCompany.Country.Code.ToString()))
			{
				var allARAPTransactions = new List<InvoicingBase>();
				allARAPTransactions.AddRange(transactions.GetAllARInvoicesAndCreditNotes());
				allARAPTransactions.AddRange(transactions.GetAllAPInvoicesAndCreditNotes());
				result = allARAPTransactions.Where(x => x.AH_InvoiceDate.Date > today.Date).ToArray();
			}
			return result;
		}

		public static void AddErrorOrWarningIfNoTaxMessage(ZPropertyInfo taxMessagePropertyInfo, ZGuid taxId, Func<ZDecimal> getExTaxAmount, Func<ZDecimal> getTaxAmount, Func<ZDecimal> getExtraTaxAmount, bool isAP = false, bool isAR = false)
		{
			var taxMessageIsEmpty = taxMessagePropertyInfo.Value is ZGuid messagePK && messagePK.IsEmpty;

			if (taxId == ZGuid.Empty || !taxMessageIsEmpty)
			{
				return;
			}

			if (isAP == isAR)
			{
				ErrorReporter.ReportOnce("CountrySpecificValidationHelper.AddErrorOrWarningIfTaxAmountIsZeroWithoutTaxMessage", Invariant($"Line should be either AP or AR related but was {(isAP ? (NoResString)"both" : (NoResString)"neither")}"));
			}

			var registryValue = isAP
				? AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryPayables.Value
				: isAR
					? AccountingConfigurationRegistry.Instance.TaxMessageIsMandatoryReceivables.Value
					: string.Empty;

			var addError = false;

			switch (registryValue)
			{
				case Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxIsZero:
					addError = getExTaxAmount() != ZDecimal.Zero && getTaxAmount() == ZDecimal.Zero;
					break;

				case Constants.TaxMessageMandatoryOptionConstants.RequiredAlways:
					addError = true;
					break;

				case Constants.TaxMessageMandatoryOptionConstants.RequiredWhenExtraTaxIsNotZero:
					addError = getExtraTaxAmount() != ZDecimal.Zero;
					break;

				case Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax:
					addError = (getExTaxAmount() != ZDecimal.Zero && getTaxAmount() == ZDecimal.Zero) || getExtraTaxAmount() != ZDecimal.Zero;
					break;
			}

			string message() => registryValue == Constants.TaxMessageMandatoryOptionConstants.RequiredAlways
				? Res.GetString("1725DFCA-1DE6-4306-84DD-C4CA43DAFBFE", "Tax Message is Required.")
				: registryValue == Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxIsZero
					? Res.GetString("645b4e1f-72d9-4fe9-a8ae-61d73f38b688", "Tax Amount is zero, please enter a Tax Message.")
					: Res.GetString("84E5AACD-F1E2-4170-B858-F60BAD9D4B8A", "Please enter a valid Tax Message when using this Tax ID.");

			if (addError)
			{
				taxMessagePropertyInfo.AddError(message());
			}
		}

		public static bool NeedToCheckCompanyAndOrgsRegistrationNumber()
		{
			var supportedCountries = new string[] { Core.Constants.CountryCodes.Portugal };
			return supportedCountries.Contains(GlbCompany.CurrentCompany.Country.Code.ToString());
		}
	}
}
