using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.eInvoicing.China
{
	public static class ChinaEInvoicingHelper
	{
		static IEnumerable<RemarkMeta> GetRemarkElements(string source)
		{
			var regexExpr1 = @"\[([^\[\]]*)\]";
			var regexExpr2 = "<([^<>]*)>";

			var matches = Regex.Matches(source, regexExpr1);
			var target = (from Match match in matches where !string.IsNullOrEmpty(match.Groups[1].Value) select match.Groups[1].Value).Distinct().ToList();
			var result = new List<RemarkMeta>();

			foreach (var innerSource in target)
			{
				var innerMatches = Regex.Matches(innerSource, regexExpr2);
				var innerTargets = (from Match match in innerMatches where !string.IsNullOrEmpty(match.Groups[1].Value) select match.Groups[1].Value).Distinct().ToList();
				if (innerTargets.Count > 0)
				{
					result.Add(new RemarkMeta(innerSource, innerTargets[0]));
				}
				else
				{
					result.Add(new RemarkMeta(innerSource, string.Empty, true));
				}
			}

			return result;
		}

		public static string CreateRemarkString(BusinessObjectFactory factory, DataRow data, Guid companyId, Guid? branchId = null)
		{
			var remarks = AccountingConfigurationRegistry.Instance.AccountingWebServiceRemarks.GetFallBackValueAtAllLevels(companyId, branchId ?? Guid.Empty, Guid.Empty);
			var items = GetRemarkElements(remarks);

			var strItems = new Dictionary<string, string>();
			strItems.Add("ConsolMasterBill", "MasterBill");
			strItems.Add("ShipmentHouseBill", "HouseBill");
			strItems.Add("ConsolVessel", (NoResString)"Vessel");
			strItems.Add("ConsolVoyFlt", (NoResString)"Voyage");
			strItems.Add("ConsolLoadPort", "LoadPort");
			strItems.Add("ConsolDischargePort", "DischargePort");
			strItems.Add("JobInvNumber", "JobInvoiceNumber");
			strItems.Add("InvoiceAmount", "AH_OSTotal");
			strItems.Add("InvoiceCurrency", "AH_RX_NKTransactionCurrency");
			strItems.Add("TransNumber", "AH_TransactionNum");
			strItems.Add("InvoiceExchangeRate", "AH_ExchangeRate");
			strItems.Add("JobOperatorFullName", "OperatorFullName");
			strItems.Add("JobOperatorPreferredName", "OperatorFriendlyName");
			strItems.Add("JobSalesRepFullName", "RepSalesFullName");
			strItems.Add("JobSalesRepPreferredName", "RepSalesFriendlyName");
			strItems.Add("TransDescription", "AH_Desc");
			strItems.Add("ConsolETD", "ETD");
			strItems.Add("ConsolETA", "ETA");
			var currencyItems = new Dictionary<string, string>();
			currencyItems.Add("InvoiceAmount", "AH_OSTotal");
			currencyItems.Add("InvoiceCurrency", "AH_RX_NKTransactionCurrency");
			currencyItems.Add("InvoiceExchangeRate", "AH_ExchangeRate");

			var formattedResult = remarks;
			var currencyCode = data["AH_RX_NKTransactionCurrency"].ToString();
			var isCurrencyExcluded = currencyCode == Enterprise.Core.Constants.CurrencyCodes.China;
			var currency = RefCurrency.LoadFromCurrencyCode(factory, currencyCode);

			foreach (var remark in items)
			{
				var formattedCaptionAndValue = string.Empty;

				if (!isCurrencyExcluded || !currencyItems.Keys.Contains(remark.OriginalValue))
				{
					var formattedValue = string.Empty;

					if (strItems.Keys.Contains(remark.OriginalValue))
					{
						var sqlKey = strItems[remark.OriginalValue];
						var sqlValue = data[sqlKey];
						if (remark.OriginalValue == "InvoiceAmount")
						{
							formattedValue = AccountingUtils.Round(ZDecimal.Parse(sqlValue.ToString()), currency).ToString();
						}
						else if (remark.OriginalValue == "InvoiceExchangeRate")
						{
							formattedValue = ZDecimal.Parse(sqlValue.ToString()).ToString("0.000000");
						}
						else
						{
							formattedValue = sqlValue.ToString();
						}
					}
					else
					{
						continue;
					}

					formattedCaptionAndValue = remark.HasNoMacro ?
						remark.OriginalCaptionAndValue :
						(string.IsNullOrEmpty(formattedValue) ?
							string.Empty :
							remark.OriginalCaptionAndValue.Replace("<" + remark.OriginalValue + ">", formattedValue));
				}

				formattedResult = formattedResult.Replace("[" + remark.OriginalCaptionAndValue + "]", formattedCaptionAndValue);
			}

			return formattedResult;
		}
	}
}
