using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.JSON.Extensions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	public class ChinaEInvoicePayloadCreator : IChinaPayloadCreator
	{
		public ChinaEInvoicePayloadCreator(TransactionHeader transactionHeader)
		{
			TransactionHeader = transactionHeader;
		}

		readonly TransactionHeader TransactionHeader;

		const string ReqTypeConstant = "02";
		const string EXEMPTConstant = "EXEMPT";
		const string NOTREPORTConstant = "NOTREPORT";
		const string FREEVATConstant = "FREEVAT";

		#region IChinaPayloadCreator

		string IChinaPayloadCreator.CreatePayloadAsJson()
		{
			return GetEInvoice().ToJSON();
		}

		#endregion

		ChinaEInvoice GetEInvoice()
		{
			var data = GetData();
			var eInvoice = new ChinaEInvoice()
			{
				ReqType = ReqTypeConstant,
				TaxNo = TransactionHeader.Branch.OrgProxy?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.China)?.OK_CustomsRegNo ?? string.Empty,
				ClientNo = AccountingMasterFilesRegistry.Instance.ChinaEInvoicingCredentials.GetFallBackValueAtAllLevels(TransactionHeader.Company.PK.ToGuid(), TransactionHeader.Branch.PK.ToGuid(), Guid.Empty),
				Autoexec = AccountingMasterFilesRegistry.Instance.ChinaTransmitAndIssueFapiao.GetFallBackValueAtAllLevels(TransactionHeader.Company.PK.ToGuid(), TransactionHeader.Branch.PK.ToGuid(), Guid.Empty) ? "1" : "0",
				Data = data,
			};

			var orderDetails = GetOrderDetails(((InvoicingBase)TransactionHeader).Lines);
			eInvoice.Data.Order.OrderDetails.AddRange(orderDetails);

			return eInvoice;
		}

		Data GetData()
		{
			return new Data()
			{
				SerialNumber = ChinaEInvoiceHelper.GetSerialNumber(TransactionHeader),
				Extend = ChinaEInvoiceHelper.GetExtend(TransactionHeader),
				InvType = ChinaComplianceInfo.ComplianceSubTypeDictionary.FirstOrDefault(x => x.Value == TransactionHeader.AH_ComplianceSubType).Key,
				Version = string.Empty,
				Drawer = string.Empty,
				Payee = string.Empty,
				Reviewer = string.Empty,
				Seller = GetSeller(),
				Order = GetOrder()
			};
		}

		Seller GetSeller() => new Seller()
		{
			Identifier = string.Empty,
			Name = string.Empty,
			Address = string.Empty,
			TelephoneNo = string.Empty,
			Bank = ARBankAccount?.A1_BankName ?? string.Empty,
			BankAcc = ARBankAccount?.A1_BankAccount ?? string.Empty
		};

		AccARAccountDetails ARBankAccount => GetARBankAccount();

		AccARAccountDetails GetARBankAccount()
		{
			if (IsForeignCurrencyInvoice)
			{
				var accounts = TransactionHeader.Branch?.OrgProxy?.CompanyData?.ARAccountDetailsCollection.Where(
					x => x.A1_IsDefaultAccount
					&& x.A1_PaymentMethod == AccARAccountDetails.ARBankAccPayment
					&& x.A1_RX_NKAccountCurrency == TransactionHeader.AH_RX_NKTransactionCurrency
					&& x.A1_RN_NKCountryCode == Enterprise.Core.Constants.CountryCodes.China);

				if (accounts.Any())
				{
					return accounts.FirstOrDefault();
				}
			}

			return null;
		}

		bool IsForeignCurrencyInvoice => TransactionHeader.AH_TransactionCategory == InvoiceTypesList.Codes.ForeignCurrencyInvoice || TransactionHeader.AH_TransactionCategory == InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;

		Order GetOrder()
		{
			return new Order()
			{
				OrderNo = TransactionHeader.AH_TransactionNum,
				InvoiceList = string.Empty,
				InvoiceSplit = "0",
				InvoiceSfdy = "0",
				OrderDate = TransactionHeader.AH_PostDate.ToString("yyyy-MM-dd HH:mm:ss"),
				ChargeTaxWay = "0",
				TotalAmount = decimal.Round(TransactionHeader.AH_InvoiceAmount + TransactionHeader.AH_GSTAmount, 2).ToString(),
				TaxMark = "0",
				TotalDiscount = string.Empty,
				Remark = ChinaEInvoiceHelper.GetRemark(TransactionHeader.Factory, TransactionHeader.PK, TransactionHeader.Company.PK, TransactionHeader.Branch.PK),
				ExtractedCode = string.Empty,
				Buyer = GetBuyer()
			};
		}

		Buyer GetBuyer()
		{
			var orgHader = TransactionHeader.Header;
			var defaultBankAccount = ChinaEInvoiceHelper.GetDefaultTaxBankAccount(orgHader, TransactionHeader.AH_RX_NKTransactionCurrency);
			var mainARMailingAddress = ChinaEInvoiceHelper.GetMainARMailingAddress(orgHader);
			var email = mainARMailingAddress?.OA_Email ?? string.Empty;

			return new Buyer()
			{
				CustomerType = orgHader.OH_Category == OrgConstants.Category.Business ? "1" : "0",
				Identifier = orgHader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.China)?.OK_CustomsRegNo ?? string.Empty,
				Name = mainARMailingAddress?.CompanyName ?? string.Empty,
				Address = GetAddress(mainARMailingAddress),
				TelephoneNo = !(ExcludeBuyerPhoneNumber && IsFDAOrFDB) && mainARMailingAddress != null ? mainARMailingAddress.OA_Phone : string.Empty,
				Bank = !(ExcludeBuyerBankAccountInfo && IsFDAOrFDB) && defaultBankAccount != null ? defaultBankAccount.A1_BankName : string.Empty,
				BankAcc = !(ExcludeBuyerBankAccountInfo && IsFDAOrFDB) && defaultBankAccount != null ? defaultBankAccount.A1_BankAccount : string.Empty,
				Email = email,
				MemberId = string.Empty,
				IsSend = email.IsEmpty ? "0" : "1",
				Recipient = string.Empty,
				ReciAddress = string.Empty,
				Zip = string.Empty
			};
		}

		ZBool IsFDAOrFDB => TransactionHeader.AH_ComplianceSubType == ChinaComplianceInfo.ComplianceSubTypeCodes.FDA || TransactionHeader.AH_ComplianceSubType == ChinaComplianceInfo.ComplianceSubTypeCodes.FDB;

		ZBool ExcludeBuyerBankAccountInfo => AccountingConfigurationRegistry.Instance.DoNotSendTheseInfomationForFullyDigitalizedEInvoice.GetValueWithoutFallback(TransactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty).BuyerBankAccount;

		ZBool ExcludeBuyerPhoneNumber => AccountingConfigurationRegistry.Instance.DoNotSendTheseInfomationForFullyDigitalizedEInvoice.GetValueWithoutFallback(TransactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty).BuyerPhoneNumber;

		ZBool ExcludeBuyerAddress => AccountingConfigurationRegistry.Instance.DoNotSendTheseInfomationForFullyDigitalizedEInvoice.GetValueWithoutFallback(TransactionHeader.AH_GC.ToGuid(), Guid.Empty, Guid.Empty).BuyerAddress;

		string GetAddress(OrgAddress mainARMailingAddress)
		{
			var address = mainARMailingAddress;
			if (address != null && !(ExcludeBuyerAddress && IsFDAOrFDB))
			{
				var addressArray =  new string[]
				{
					address.RelatedState?.RW_DescriptionMultilingual?.ToString(Core.SharedConstants.Languages.ChineseSimplified) ?? string.Empty,
					address.OA_City,
					address.OA_Address1,
					address.OA_Address2
				}.Where(x => !x.IsNullOrEmpty()).ToArray();

				return string.Join(" ", addressArray);
			}

			return string.Empty;
		}

		IEnumerable<OrderDetail> GetOrderDetails(InvoicingLineBaseCollection lines)
		{
			var items = new List<OrderDetail>();
			var notCommentLines = lines.OfType<InvoicingLineBase>().Where(x => !x.ChargeCode?.IsComment ?? true);
			var lineGroups = notCommentLines.GroupBy(line => line.AL_JH);
			var alwaysTransmitNegativeChargesAsDiscount = AccountingMasterFilesRegistry.Instance.AlwaysTransmitNegativeChargesAsDiscount.GetFallBackValueAtAllLevels(TransactionHeader.Company.PK.ToGuid(), TransactionHeader.Branch.PK.ToGuid(), Guid.Empty);

			foreach (var lineGroup in lineGroups)
			{
				var currentLines = lineGroup.OrderBy(line => line.AL_Sequence);
				var currentItems = new List<OrderDetail>();

				foreach (InvoicingLineBase line in currentLines)
				{
					var amount = decimal.Round(line.AL_LineAmount, 2);
					var amountAsString = amount.ToString();

					var item = new OrderDetail()
					{
						VenderOwnCode = line.ChargeCode?.AC_Code ?? line.GLHeader?.AG_AccountNum ?? string.Empty,
						ProductCode = string.Empty,
						ProductName = line.ChargeCode?.AC_LocalLanguageDescription ?? line.AL_Desc,
						RowType = alwaysTransmitNegativeChargesAsDiscount && (amount < decimal.Zero) ? "1" : "0",
						Spec = string.Empty,
						Unit = string.Empty,
						Quantity = "1",
						UnitPrice = amountAsString,
						Amount = amountAsString,
						DeductAmount = string.Empty,
						TaxRate = decimal.Round(line.TaxRate?.GetRate(line.AL_TaxDate) / 100m ?? 0, 2).ToString(),
						TaxAmount = decimal.Round(line.AL_LocalTaxAmount, 2).ToString(),
						MxTotalAmount = decimal.Round(line.AL_LineAmount + line.AL_GSTVAT, 2).ToString(),
						TaxRateMark = GetTaxRateRemark(line.TaxRate?.AT_Code ?? string.Empty),
						PolicyMark = (line.TaxRate != null && (line.TaxRate.AT_Code == EXEMPTConstant || line.TaxRate.AT_Code == NOTREPORTConstant)) ? "1" : "",
						PolicyName = GetPolicyName(line.TaxRate?.AT_Code ?? string.Empty)
					};

					currentItems.Add(item);
				}

				if (alwaysTransmitNegativeChargesAsDiscount)
				{
					ChinaEInvoiceHelper.UpdateDiscountedLineRowType(currentItems);
				}
				items.AddRange(currentItems);
			}

			return items;
		}

		string GetPolicyName(string taxRateCode)
		{
			switch (taxRateCode)
			{
				case EXEMPTConstant:
					return (NoResString)"免税";
				case NOTREPORTConstant:
					return (NoResString)"不征税";
				default:
					return string.Empty;
			}
		}

		string GetTaxRateRemark(string taxRateCode)
		{
			switch (taxRateCode)
			{
				case EXEMPTConstant:
					return "1";
				case NOTREPORTConstant:
					return "2";
				case FREEVATConstant:
					return "3";
				default:
					return string.Empty;
			}
		}
	}
}
