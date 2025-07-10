using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	public class EInvoicingDataValidatorForChina : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForChina(GlbCompany company)
			: base(company, shouldSendErrorNotificationEmail: true)
		{
		}

		protected override void RunCore(ILogger logger) => ValidateBatchedTransactions(logger);

		public override IReadOnlyCollection<ZString> ValidateTransaction(InvoicingBase transaction, AccEInvoicingTransactionPivot pivot)
		{
			var errorMessages = new List<ZString>();

			if (pivot.AIP_ActionType != EInvoicingPivotActionType.Submit)
			{
				return errorMessages;
			}

			var debtor = transaction.Header;
			var isFDAOrFDBComplianceSubType = transaction.AH_ComplianceSubType == ChinaComplianceInfo.ComplianceSubTypeCodes.FDA || transaction.AH_ComplianceSubType == ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;
			var invoiceDebtorString = Res.GetString("064fa380-a99b-4248-a250-ad4dea728df1", "Invoice Debtor");
			var branchProxyString = Res.GetString("c90c7801-d1fe-49b8-809c-ebd9e66a7e5e", "Invoice Branch Proxy's Organization");

			CheckOrgTaxCode(transaction.Branch.OrgProxy, branchProxyString, errorMessages, false);
			CheckOrgTaxCode(debtor, invoiceDebtorString, errorMessages, true);
			CheckDebtorMainARMailingAddress(debtor, isFDAOrFDBComplianceSubType, errorMessages);
			CheckDefaultTaxBankAccount(debtor, isFDAOrFDBComplianceSubType, transaction.AH_RX_NKTransactionCurrency, errorMessages);
			CheckDiscountLineOrder(transaction.Lines, errorMessages);

			return errorMessages;
		}

		void CheckOrgTaxCode(OrgHeader header, ZString organizationType, List<ZString> errors, bool isDebtor)
		{
			if (header != null)
			{
				var orgCusCode = GetChinaTaxCode(header);
				if (orgCusCode == null)
				{
					if (header.OH_Category == OrgConstants.Category.Business && (header.CountryCode == CountryCodes.China || !isDebtor))
					{
						errors.Add(Res.GetString("20f9cc58-05ba-45fb-8886-d92860891745", "{0} must have a valid China VAT Registration Number.", organizationType));
					}
				}
				else if (CheckNeedValidation(header, isDebtor) || !orgCusCode.OK_CustomsRegNo.IsEmpty)
				{
					var invalidTaxCodeMessage = GetTaxCodeInvalidMessage(orgCusCode.OK_CustomsRegNo);
					if (!invalidTaxCodeMessage.IsEmpty)
					{
						errors.Add(invalidTaxCodeMessage);
					}
				}
			}

			ZString GetTaxCodeInvalidMessage(ZString registrationNum)
			{
				var result = ZString.Empty;

				var isValid = Regex.IsMatch(registrationNum, @"^([0-9A-Za-z]{9}|[0-9A-Za-z]{15,20})$");
				if (!isValid)
				{
					result = Res.GetString("b2758e83-e71a-456b-b890-855038bd63a6", "The China VAT Registration Number of the {0} must be 9 or 15-20 characters in length.", organizationType);
				}

				return result;
			}
		}

		OrgCusCode GetChinaTaxCode(OrgHeader header)
		{
			return header?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.China);
		}

		void CheckDebtorMainARMailingAddress(OrgHeader header, bool isFDAOrFDBComplianceSubType, List<ZString> errors)
		{
			var mainARMailingAddress = ChinaEInvoiceHelper.GetMainARMailingAddress(header);
			var invalidAddressMessage = Res.GetString("618e6b9e-c994-41ba-8cde-f2d95056211c", "Invoice Debtor must have a Receivable Mailing Address recorded in 'ZH-CN' language.");

			if (mainARMailingAddress == null)
			{
				errors.Add(invalidAddressMessage);
			}
			else
			{
				if (CheckNeedValidation(header) && !isFDAOrFDBComplianceSubType)
				{
					if (mainARMailingAddress.OA_State.IsEmpty && mainARMailingAddress.OA_City.IsEmpty && mainARMailingAddress.OA_Address1.IsEmpty && mainARMailingAddress.OA_Address2.IsEmpty)
					{
						errors.Add(invalidAddressMessage);
					}

					if (mainARMailingAddress.OA_Phone.IsEmpty)
					{
						errors.Add(Res.GetString("2f76352c-1033-4a9e-8a9e-d220dc77c4d3", "Invoice Debtor must have a Phone Number recorded against it's Receivable Mailing Address in 'ZH-CN' language."));
					}
				}

				var addressAndPhoneNumber = mainARMailingAddress.OA_State + mainARMailingAddress.OA_City + mainARMailingAddress.OA_Address1 + mainARMailingAddress.OA_Address2 + mainARMailingAddress.OA_Phone;
				if (addressAndPhoneNumber.Length > 100)
				{
					errors.Add(Res.GetString("C64B4322-DF15-4FDA-93ED-A30689201180", "Receivable Mailing Address recorded in 'ZH-CN' language exceeds the limit 100."));
				}

				if (mainARMailingAddress.OA_CompanyNameOverride.IsEmpty)
				{
					errors.Add(Res.GetString("bb34019f-36c1-4094-ab50-de80ec4f2742", "Invoice Debtor must have a Company Name recorded against it's Receivable Mailing Address in 'ZH-CN' language."));
				}
			}
		}

		void CheckDefaultTaxBankAccount(OrgHeader header, bool isFDAOrFDBComplianceSubType, ZString transactionCurrency, List<ZString> errors)
		{
			if (!isFDAOrFDBComplianceSubType)
			{
				var errorMessage = Res.GetString("dc60bbf3-a932-4a86-a885-de06701bdfc7", "Invoice Debtor must have a default CN's TAX Bank Account in Invoiced Currency or Local Currency");
				var defaultTaxBankAccount = ChinaEInvoiceHelper.GetDefaultTaxBankAccount(header, transactionCurrency);

				if (defaultTaxBankAccount == null)
				{
					if (CheckNeedValidation(header))
					{
						errors.Add(errorMessage);
					}
				}
				else if (defaultTaxBankAccount.A1_BankName.IsEmpty || defaultTaxBankAccount.A1_BankAccount.IsEmpty)
				{
					errors.Add(errorMessage);
				}
			}
		}

		void CheckDiscountLineOrder(InvoicingLineBaseCollection lines, List<ZString> errors)
		{
			if (AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.GetFallBackValueAtAllLevels(lines.Cast<AccTransactionLines>().FirstOrDefault().AL_GC.ToGuid(), lines.Cast<AccTransactionLines>().FirstOrDefault().AL_GB.ToGuid(), Guid.Empty))
			{
				var errorMessage = Res.GetString("5A06B0BC-5A8C-40AB-BE36-5E54402FFED3", "The 'discount line' must be recorded immediately after the discounted line in the same invoice group.");
				var notCommentLines = lines.OfType<InvoicingLineBase>().Where(x => !x.ChargeCode?.IsComment ?? true);
				var lineGroups = notCommentLines.GroupBy(line => line.AL_JH);
				var hasError = false;

				foreach (var lineGroup in lineGroups)
				{
					if (hasError)
					{
						break;
					}

					var invoicingLines = lineGroup.OrderBy(line => line.AL_Sequence);
					var lastLineIsNormalLine = false;

					foreach (var line in invoicingLines)
					{
						var currentLineIsDiscountLine = line.AL_LineAmount < ZDecimal.Zero;

						if (currentLineIsDiscountLine && !lastLineIsNormalLine)
						{
							errors.Add(errorMessage);
							hasError = true;
							break;
						}

						lastLineIsNormalLine = line.AL_LineAmount > ZDecimal.Zero;
					}
				}
			}
		}

		bool CheckNeedValidation(OrgHeader header, bool isDebtor = true) => header.OH_Category != OrgConstants.Category.NaturalPersonIndividual && (!isDebtor || header.CountryCode == CountryCodes.China);
	}
}
