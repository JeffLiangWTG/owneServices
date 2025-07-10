using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class EInvoicingDataValidatorForVietnam : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForVietnam(GlbCompany company) : base(company)
		{
		}

		protected override void RunCore(ILogger logger)
		{
			var pivotFilter = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_Status, Core.Constants.EInvoicingPivotState.Batched);
			pivotFilter.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_GC, CurrentCompany.PK);
			var pivots = Factory.Load<AccEInvoicingTransactionPivot>(pivotFilter);
			var invoices = Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, pivots.Select(p => p.AIP_ParentID)));

			var errorMessages = Validate(invoices, pivots);
			if (errorMessages.Any())
			{
				foreach (var errorMessageWithInvoiceNumber in errorMessages)
				{
					logger.Log(LogType.Error, FormattableString.Invariant($"[{errorMessageWithInvoiceNumber.invoiceNumber}] : Failed to send this invoice due to validation error. \r\n {errorMessageWithInvoiceNumber.errorMessage}"));
				}
			}
		}

		List<(ZString invoiceNumber, ZString errorMessage)> Validate(InvoicingBase[] invoices, AccEInvoicingTransactionPivot[] pivots)
		{
			var errorDescriptions = new List<(ZString invoiceNumber, ZString errorMessage)>();
			var needToCallSave = false;
			var logCollector = new SimpleLogger();

			foreach (var invoice in invoices)
			{
				var errorDescription = Validate(invoice);
				if (!errorDescription.IsEmpty)
				{
					var pivot = pivots.FirstOrDefault(p => p.AIP_ParentID == invoice.PK);
					pivot.AIP_Status = Core.Constants.EInvoicingPivotState.BatchedWithError;
					var errorMaxLength = AccEInvoicingTransactionPivotSchema.AIP_ErrorDescription.MaxLength;
					if (errorDescription.Length > errorMaxLength)
					{
						errorDescription = errorDescription.Substring(0, errorMaxLength - 3) + "...";
					}
					pivot.AIP_ErrorDescription = errorDescription;
					needToCallSave = true;

					errorDescriptions.Add((invoice.AH_TransactionNum, errorDescription));
					SendErrorNotificationEmail(invoice, errorDescription, logCollector);
				}
			}

			if (needToCallSave)
			{
				Factory.Save();
			}
			return errorDescriptions;
		}

		void SendErrorNotificationEmail(InvoicingBase invoice, ZString errorDescription, SimpleLogger logCollector)
		{
			var emailCreator = new GEIEmailNotificationCreator(null, invoice, new List<ZString>() { errorDescription }, logCollector);
			emailCreator.SendEmail();
		}

		ZString Validate(InvoicingBase invoice)
		{
			var companyPK = invoice.Company.PK.ToGuid();
			var branchPK = invoice.Branch.PK.ToGuid();
			var validationResults = new List<string>();

			var validateDebtorTaxCodeAndType = ValidateOrgProxyTaxCodeCountryAndType(invoice);
			if (!string.IsNullOrEmpty(validateDebtorTaxCodeAndType))
			{
				validationResults.Add(validateDebtorTaxCodeAndType);
			}

			var validateFPTUsernameAndPassword = ValidateFPTUsernameAndPassword(companyPK, branchPK);
			if (!string.IsNullOrEmpty(validateFPTUsernameAndPassword))
			{
				validationResults.Add(validateFPTUsernameAndPassword);
			}

			var validateExchangeRate = ValidationExchangeRate(invoice);
			if (!string.IsNullOrEmpty(validateExchangeRate))
			{
				validationResults.Add(validateExchangeRate);
			}

			var validateEMail = ValidateEMail(invoice);
			if (!string.IsNullOrEmpty(validateEMail))
			{
				validationResults.Add(validateEMail);
			}

			return string.Join("\r\n", validationResults);
		}

		ZString ValidateOrgProxyTaxCodeCountryAndType(InvoicingBase invoice)
		{
			var branchOrgCusCodes = invoice.Branch.OrgProxy?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam);

			if (branchOrgCusCodes == null)
			{
				var companyOrgCusCodes = invoice.Company.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam);
				if (companyOrgCusCodes == null)
				{
					return Res.GetString("739b16c3-ad7b-4d8e-9ce9-595e7e5b62f8", "Branch proxy or company proxy must have VN VAT number.");
				}
				return VietnamVATCodeValidator.CheckCodeAndGetInvalidMessage(companyOrgCusCodes.OK_CustomsRegNo);
			}

			return VietnamVATCodeValidator.CheckCodeAndGetInvalidMessage(branchOrgCusCodes.OK_CustomsRegNo);
		}

		ZString ValidateFPTUsernameAndPassword(Guid companyPK, Guid branchPK)
		{
			var result = ZString.Empty;
			var userName = AccountingConfigurationRegistry.Instance.VietnamEInvoicingUserName.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
			var passWord = AccountingConfigurationRegistry.Instance.VietnamEInvoicingPassword.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
			if (userName == ZString.Empty || passWord == ZString.Empty)
			{
				result = Res.GetString("a9cd6f4d-5ada-46fe-8a7e-0994e76883b5", "Vietnam E-Invoice Service Partner Connection Username and Password should be set in registry.");
			}

			return result;
		}

		ZString ValidationExchangeRate(InvoicingBase invoice)
		{
			var result = ZString.Empty;

			if (invoice.AH_ExchangeRate.Round(2) >= 100000)
			{
				result = Res.GetString("288610F6-988A-4459-B4B5-BAEE896C79C8", "The exchange rate value '{0}' has exceeded the maximum allowable length. It should have a maximum length of 7 digits inclusive of 2 decimals.", invoice.AH_ExchangeRate.ToStringTrimZeros());
			}

			return result;
		}

		ZString ValidateEMail(InvoicingBase invoice)
		{
			var result = ZString.Empty;
			var eMails = TransactionBatchToGEIConverterForVietnam.GetOrgARContactsEmail(invoice);
			var eMail = string.Join(";", eMails) ?? ZString.Empty;

			if (eMail.Length > 250)
			{
				result = Res.GetString("c041f64b-25f1-4c26-a128-89c51f76771a", "Maximum allowed Emails exceeded, please reduce the number of Organization Contacts marked with 'Group = A/R, Delivery = EML'.");
			}

			return result;
		}
	}
}
