using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Business
{
	sealed class MoneyProvider : IMoney
	{
		public MoneyProvider(CusEntryLine entryLine, ZString code)
		{
			Argument.NotNull(entryLine, nameof(entryLine));
			if (code == nameof(entryLine.FOB))
			{
				var fob = entryLine.FOB;
				Amount = fob.Amount;
				CurrencyCode = fob.Currency?.Code ?? ZString.Empty;
			}
			else if (code == nameof(entryLine.CL_CustomsValue))
			{
				Amount = entryLine.CL_CustomsValue;
				CurrencyCode = string.Empty;
			}
		}

		public MoneyProvider(CusEntryHeader entryHeader, ZString code)
		{
			Argument.NotNull(entryHeader, nameof(entryHeader));

			if (code == CustomsChargeTypeList.Codes.OverseasFreight || code == CustomsChargeTypeList.Codes.OverseasInsurance)
			{
				CalculateFreightAndInsuranceAmountInJPY(entryHeader, code);
				CurrencyCode = Amount == null ? string.Empty : Core.Constants.CurrencyCodes.Japan;
			}
			else if (code == nameof(CusEntryHeaderMessageProvider.Invoice))
			{
				if (entryHeader.IsExport && ValueTypeList.Codes.S.Equals(entryHeader.EntryInstruction?.CEI_ValueType))
				{
					Amount = null;
					CurrencyCode = string.Empty;
				}
				else
				{
					CalculateAmount(entryHeader);
					CurrencyCode = entryHeader.InvoiceHeaders().FirstOrDefault()?.JZ_RX_NKInvoice_Currency;
				}
			}
			else if (code == nameof(CusEntryHeaderMessageProvider.FOB))
			{
				if (entryHeader.IsExport && ValueTypeList.Codes.S.Equals(entryHeader.EntryInstruction?.CEI_ValueType))
				{
					Amount = entryHeader.MergedLines.Sum(x => x.CL_CustomsValue);
					CurrencyCode = entryHeader.LocalCurrencyCode;
				}
				else if (entryHeader.EntryInstruction is CusEntryInstruction entryInstruction && entryInstruction.CEI_Style == JPExportDeclarationTypeList.Codes.G)
				{
					var fobMoneyArray = entryHeader.InvoiceLines.Select(x => x.JI_FOB).ToArray();
					Amount = fobMoneyArray.Sum(x => x.Amount);
					CurrencyCode = fobMoneyArray.FirstOrDefault().Currency.Code;
				}
				else
				{
					var noneFOBInvoiceHeaders = entryHeader.InvoiceHeaders().Where(x => x.JZ_IncoTerm != Core.Constants.IncoTerms.FreeOnBoard).ToArray();
					if (noneFOBInvoiceHeaders.Length != 0)
					{
						Amount = noneFOBInvoiceHeaders.Sum(x => x.JZ_Calc_FOBAmount);
						CurrencyCode = noneFOBInvoiceHeaders.FirstOrDefault().JZ_RX_NKInvoice_Currency;
					}
				}
			}
		}

		public decimal? Amount { get; internal set; }

		public string CurrencyCode { get; }

		void CalculateFreightAndInsuranceAmountInJPY(CusEntryHeader entryHeader, ZString code)
		{
			var hasValidCharge = false;
			var localCurrency = RefCurrency.LoadFromCurrencyCode(entryHeader.Factory, Core.Constants.CurrencyCodes.Japan);
			var amount = entryHeader.InvoiceHeaders().Sum(c => c.Charges.GetCharge(code, localCurrency, (c) =>
			{
				hasValidCharge |= c.ChargeCode != null;
				return hasValidCharge;
			}));
			Amount = hasValidCharge ? Math.Floor(amount) : null;
		}

		void CalculateAmount(CusEntryHeader entryHeader)
		{
			Amount = entryHeader.InvoiceHeaders().Sum(header => header.JZ_InvoiceAmount);
		}
	}
}
