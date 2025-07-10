using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.India
{
	public class EInvoicingDataValidatorForIndia : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForIndia(GlbCompany company)
			: base(company, shouldSendErrorNotificationEmail: true)
		{
		}

		protected override void RunCore(ILogger logger)
			=> ValidateBatchedTransactions(logger);

		public override IReadOnlyCollection<ZString> ValidateTransaction(InvoicingBase transaction, AccEInvoicingTransactionPivot pivot)
		{
			var errorMessages = new List<ZString>();
			var complianceNumberAppliesFrom = new ZDate(AccountingConfigurationRegistry.Instance.IndiaUseComplianceNumbersInsteadOfTransactionNumbersFrom.GetFallBackValueAtAllLevels(transaction.AH_GC.ToGuid(), transaction.AH_GB.ToGuid(), transaction.AH_GE.ToGuid()).Date);

			Check_TransactionNumberIsRequired(transaction, errorMessages);
			Check_InvoiceDateIsRequired(transaction, errorMessages);

			Check_SellerHasIndiaGSTNumber(transaction, errorMessages);
			Check_SellerHasStateCode(transaction, errorMessages);
			Check_SellerHasCity(transaction, errorMessages);
			Check_SellerHasPostCode(transaction, errorMessages);

			Check_BuyerHasStateCode(transaction, errorMessages);
			Check_BuyerHasCity(transaction, errorMessages);
			Check_BuyerHasPostCode(transaction, errorMessages);

			Check_ComplianceNumberIsRequired(transaction, complianceNumberAppliesFrom, errorMessages);
			Check_ComplianceSubTypeIsRequired(transaction, errorMessages);

			Check_TransactionLineDescriptionIsNotShort(transaction, errorMessages);

			Check_TransactionNumberStartCharacters(transaction, complianceNumberAppliesFrom, errorMessages);
			Check_ComplianceNumberStartCharacters(transaction, complianceNumberAppliesFrom, errorMessages);

			return errorMessages;
		}

		#region Validation Rules

		static void Check_TransactionNumberIsRequired(InvoicingBase transaction, List<ZString> errors)
		{
			if (transaction.AH_TransactionNum.IsEmpty)
			{
				errors.Add(Res.GetString("b33803e8-4577-4a13-b746-49fc91cfe93f", "Transaction number is missing."));
			}
		}

		static void Check_InvoiceDateIsRequired(InvoicingBase transaction, List<ZString> errors)
		{
			if (transaction.AH_InvoiceDate.IsEmpty)
			{
				errors.Add(Res.GetString("02fea412-4931-4e69-acbd-886985ae947e", "Transaction date is missing."));
			}
		}

		static void Check_SellerHasIndiaGSTNumber(InvoicingBase transaction, List<ZString> errors)
		{
			var customsCodes = transaction.Branch?.OrgProxy?.CustomsCodes?.Cast<OrgCusCode>() ?? Enumerable.Empty<OrgCusCode>();
			var hasIndiaGstRegistrationNumber = customsCodes.Any(
												x => x.OK_RN_NKCodeCountry == CountryCodes.India
												  && x.OK_CodeType == OrgCusCode.CodeTypes.GSTCode
												  && !x.OK_CustomsRegNo.IsEmpty);
			if (!hasIndiaGstRegistrationNumber)
			{
				errors.Add(Res.GetString("36bf3653-5038-44cd-b70b-cedb8a6706d2", "Seller does not have GSTIN."));
			}
		}

		static void Check_SellerHasStateCode(InvoicingBase transaction, List<ZString> errors)
		{
			if ((transaction.Branch?.OrgProxy?.MainAddress?.StateCode ?? ZString.Empty) == ZString.Empty)
			{
				errors.Add(Res.GetString("320007fa-8b73-45e1-8528-525c1a1529e3", "Could not determine seller's state code."));
			}
		}

		static void Check_SellerHasPostCode(InvoicingBase transaction, List<ZString> errors)
		{
			if ((transaction.Branch?.OrgProxy?.MainAddress?.Postcode ?? ZString.Empty) == ZString.Empty)
			{
				errors.Add(Res.GetString("58c23086-445c-4664-a9fa-63c789e09a20", "Could not determine seller's post code."));
			}
		}

		static void Check_SellerHasCity(InvoicingBase transaction, List<ZString> errors)
		{
			if ((transaction.Branch?.OrgProxy?.MainAddress?.City ?? ZString.Empty) == ZString.Empty)
			{
				errors.Add(Res.GetString("29301b0c-bd94-470d-a6b3-e62c6af8c396", "Could not determine seller's city."));
			}
		}

		static void Check_BuyerHasStateCode(InvoicingBase transaction, List<ZString> errors)
		{
			if ((transaction.Header?.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty) == CountryCodes.India
				&& (transaction.Header?.MainAddress?.StateCode ?? ZString.Empty) == ZString.Empty)
			{
				errors.Add(Res.GetString("4229b382-f33a-4cf4-83f5-a0685f24071f", "Could not determine buyer's state code."));
			}
		}

		static void Check_BuyerHasPostCode(InvoicingBase transaction, List<ZString> errors)
		{
			if ((transaction.Header?.MainAddress?.OA_RN_NKCountryCode ?? ZString.Empty) == CountryCodes.India
				&& (transaction.Header?.MainAddress?.Postcode ?? ZString.Empty) == ZString.Empty)
			{
				errors.Add(Res.GetString("f536d182-76cf-40e2-859d-1708655d89ea", "Could not determine buyer's post code."));
			}
		}

		static void Check_BuyerHasCity(InvoicingBase transaction, List<ZString> errors)
		{
			if ((transaction.Header?.MainAddress?.City ?? ZString.Empty) == ZString.Empty)
			{
				errors.Add(Res.GetString("e28295a4-6866-4cf8-b790-d4cea3241225", "Could not determine buyer's city."));
			}
		}

		static void Check_ComplianceNumberIsRequired(InvoicingBase transaction, ZDate complianceNumberAppliesFrom, List<ZString> errors)
		{
			var complianceNumberIsRequired = IndiaComplianceInfo.UseComplianceNumberForEInvoicingMapping(complianceNumberAppliesFrom, transaction.AH_InvoiceDate.Date);
			if (complianceNumberIsRequired
				&& transaction.AH_TransactionReference.IsEmpty)
			{
				errors.Add(Res.GetString("4b6d53d0-10ee-4e51-9382-2fb816a82ff6", "Compliance number is missing."));
			}
		}

		static void Check_ComplianceSubTypeIsRequired(InvoicingBase transaction, List<ZString> errors)
		{
			if (transaction.AH_ComplianceSubType.IsEmpty)
			{
				errors.Add(Res.GetString("6da92e07-472b-4fb4-bf07-05e517a16038", "Compliance sub-type is missing."));
			}
		}

		static void Check_TransactionLineDescriptionIsNotShort(InvoicingBase transaction, List<ZString> errors)
		{
			for (int i = 0; i < transaction.Lines.Count; i++)
			{
				var line = transaction.Lines[i];
				if (line.AL_Desc.TrimEnd().Length < 3)
				{
					var (tableName, code) = GetGLHeaderOrChargeCode(line);
					errors.Add(Res.GetString("e0e4b2dc-b603-4347-8aed-520f3f215101", "Transaction line {0} with {1} '{2}' has less then 3 characters.", i + 1, tableName, code));
				}
			}

			(string, string) GetGLHeaderOrChargeCode(InvoicingLineBase invoiceLine)
				=> invoiceLine.AL_AC.IsValid ? (invoiceLine.AL_ACInfo.HumanReadableName, invoiceLine.ChargeCode.AC_Code)
				 : invoiceLine.AL_AG.IsValid ? (invoiceLine.AL_AGInfo.HumanReadableName, invoiceLine.GLHeader.AG_AccountNum)
				 : ((ZString)Res.GetString("d2eb7519-1f01-494d-93f0-ec927ab7c8b6", "unknown"), ZString.Empty);
		}

		static void Check_TransactionNumberStartCharacters(InvoicingBase transaction, ZDate complianceNumberAppliesFrom, List<ZString> errors)
		{
			var shouldCheckTransactionNumberForInvalidCharacters = IndiaComplianceInfo.UseTransactionNumberForEInvoicingMapping(new ZDate(complianceNumberAppliesFrom), transaction.AH_InvoiceDate.Date);
			if (shouldCheckTransactionNumberForInvalidCharacters
				&& transaction.AH_TransactionNum.Length > 0)
			{
				foreach (var c in IndiaComplianceInfo.InvalidTransactionNumberLeadingCharacters)
				{
					if (transaction.AH_TransactionNum[0] == c)
					{
						errors.Add(IndiaComplianceInfo.CreateTransactionNumberValidationExceptionMessage(transaction.AH_TransactionNum, c).ToString());
					}
				}
			}
		}

		static void Check_ComplianceNumberStartCharacters(InvoicingBase transaction, ZDate complianceNumberAppliesFrom, List<ZString> errors)
		{
			var shouldCheckComplianceNumberForInvalidCharacters = IndiaComplianceInfo.UseComplianceNumberForEInvoicingMapping(complianceNumberAppliesFrom, transaction.AH_InvoiceDate.Date);
			if (shouldCheckComplianceNumberForInvalidCharacters
				&& transaction.AH_TransactionReference.Length > 0)
			{
				foreach (var c in IndiaComplianceInfo.InvalidTransactionNumberLeadingCharacters)
				{
					if (transaction.AH_TransactionReference[0] == c)
					{
						errors.Add(IndiaComplianceInfo.CreateComplianceNumberValidationExceptionMessage(transaction.AH_TransactionReference, c).ToString());
					}
				}
			}
		}

		#endregion
	}
}
