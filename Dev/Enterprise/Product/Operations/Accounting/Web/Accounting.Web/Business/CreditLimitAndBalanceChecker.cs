using System;
using CargoWise.Common;
using Enterprise.Accounting.Export.Business;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Web.Business
{
	public class CreditLimitAndBalanceChecker
	{
		public CreditLimitAndBalanceChecker(BaseDataAccess dbAccess)
		{
			this.dbAccess = dbAccess;
		}

		readonly BaseDataAccess dbAccess;

		public CreditLimitAndBalanceResponse GetCreditLimitAndBalanceDetails(CreditLimitAndBalanceRequest request)
		{
			CreditLimitAndBalanceResponse result = new CreditLimitAndBalanceResponse();

			try
			{
				string command = string.Format((NoResString)"EXECUTE OrgCreditLimitAndBalanceDetails '{0}', '{1}', '{2}', {3}, '', '{4}'",
					request.OrgCode,
					request.CompanyCode,
					request.AccLedger,
					request.OverdueAgingPeriod,
					request.BalanceOverdueAgingOption);

				using (var reader = dbAccess.GetCommand(command).ExecuteReader())
				{
					while (reader.Read())
					{
						CreditLimitAndBalanceInfo info = new CreditLimitAndBalanceInfo();
						info.OverdueAgingPeriod = request.OverdueAgingPeriod;
						info.AccLedger = request.AccLedger;

						info.OrgCode = GetStringValue(reader, "OrgCode");
						info.CompanyCode = GetStringValue(reader, "CompanyCode");
						info.CurrencyCode = GetStringValue(reader, "CurrencyCode");

						info.AccountBalanceTotal = GetValue<decimal>(reader, "AccountBalanceTotal").Value;
						info.ClaimTotal = GetValue<decimal>(reader, "ClaimTotal").Value;
						info.OnCreditHold = (GetValue<int>(reader, "OnCreditHold", 0).Value != 0);
						info.CreditLimit = GetValue<decimal>(reader, "CreditLimit").Value;
						info.IsOverCreditLimit = (GetStringValue(reader, "IsOverCreditLimit", true).ToUpper() == "Y");
						info.AccountBalanceTotalHasValue = true;
						info.ClaimTotalHasValue = true;
						info.OnCreditHoldHasValue = true;
						info.CreditLimitHasValue = true;
						info.IsOverCreditLimitHasValue = true;
						info.LegacySystemCode = GetStringValue(reader, "LegacySystemCode");
						info.ExternalDebtorCode = GetStringValue(reader, "ExternalDebtorCode");
						info.ExternalCreditorCode = GetStringValue(reader, "ExternalCreditorCode");
						info.SettlementGroupOrgCode = GetStringValue(reader, "SettlementGroupOrgCode");
						info.SettlementGroupLegacySystemCode = GetStringValue(reader, "SettlementGroupLegacySystemCode");
						info.SettlementGroupExternalDebtorCode = GetStringValue(reader, "SettlementGroupExternalDebtorCode");
						info.SettlementGroupExternalCreditorCode = GetStringValue(reader, "SettlementGroupExternalCreditorCode");

						Constants.BalanceOverdueAgingOptionEnum balanceOverdueAgingValue = getBalanceOverdueAgingOptionValue(request.BalanceOverdueAgingOption);

						info.UnpostedRevenueRecognisedTotalHasValue = false;
						info.UnpostedRevenueUnrecognisedTotalHasValue = false;
						if (balanceOverdueAgingValue >= Constants.BalanceOverdueAgingOptionEnum.RecognisedAndUnrecognisedRevenue)
						{
							info.UnpostedRevenueRecognisedTotal = GetValue<decimal>(reader, "UnpostedRevenueRecognisedTotal").Value;
							info.UnpostedRevenueRecognisedTotalHasValue = true;

							info.UnpostedRevenueUnrecognisedTotal = GetValue<decimal>(reader, "UnpostedRevenueUnrecognisedTotal").Value;
							info.UnpostedRevenueUnrecognisedTotalHasValue = true;
						}

						if (balanceOverdueAgingValue >= Constants.BalanceOverdueAgingOptionEnum.Overdue)
						{
							info.IsOverCreditTerms = (GetStringValue(reader, "IsOverCreditTerms", true).ToUpper() == "Y");
							info.AccountBalanceNotOverdue = GetValue<decimal>(reader, "AccountBalanceNotOverdue").Value;
							info.OverdueTotal = GetValue<decimal>(reader, "OverdueTotal").Value;
						}
						info.IsOverCreditTermsHasValue = balanceOverdueAgingValue >= Constants.BalanceOverdueAgingOptionEnum.Overdue;
						info.AccountBalanceNotOverdueHasValue = balanceOverdueAgingValue >= Constants.BalanceOverdueAgingOptionEnum.Overdue;
						info.OverdueTotalHasValue = balanceOverdueAgingValue >= Constants.BalanceOverdueAgingOptionEnum.Overdue;

						if (balanceOverdueAgingValue >= Constants.BalanceOverdueAgingOptionEnum.Aging)
						{
							info.AccountBalanceOverdueLessThanOnePeriod = GetValue<decimal>(reader, "AccountBalanceOverdueLessThanPeriod").Value;
							info.AccountBalanceOverdueOnePeriodAndOver = GetValue<decimal>(reader, "AccountBalanceOverduePeriodAndOver").Value;
							info.AccountBalanceOverdueTwoPeriodsAndOver = GetValue<decimal>(reader, "AccountBalanceOverdueTwoPeriodsAndOver").Value;
							info.AccountBalanceOverdueThreePeriodsAndOver = GetValue<decimal>(reader, "AccountBalanceOverdueThreePeriodsAndOver").Value;
						}
						info.AccountBalanceOverdueLessThanOnePeriodHasValue = balanceOverdueAgingValue >= Constants.BalanceOverdueAgingOptionEnum.Aging;
						info.AccountBalanceOverdueOnePeriodAndOverHasValue = balanceOverdueAgingValue >= Constants.BalanceOverdueAgingOptionEnum.Aging;
						info.AccountBalanceOverdueTwoPeriodsAndOverHasValue = balanceOverdueAgingValue >= Constants.BalanceOverdueAgingOptionEnum.Aging;
						info.AccountBalanceOverdueThreePeriodsAndOverHasValue = balanceOverdueAgingValue >= Constants.BalanceOverdueAgingOptionEnum.Aging;

						bool? boolValue = GetValue<bool>(reader, "IsGlobalCreditApproved");
						info.IsGlobalCreditApprovedHasValue = boolValue.HasValue;
						info.IsGlobalCreditApproved = boolValue.HasValue && boolValue.Value;
						if (info.IsGlobalCreditApprovedHasValue && info.IsGlobalCreditApproved)
						{
							info.OnGlobalCreditHoldHasValue = true;
							info.OnGlobalCreditHold = GetValue<bool>(reader, "OnGlobalCreditHold").Value;

							info.IsOverGlobalCreditLimitHasValue = true;
							info.IsOverGlobalCreditLimit = GetValue<bool>(reader, "IsOverGlobalCreditLimit").Value;
							info.GlobalCreditCurrencyCode = GetStringValue(reader, "GlobalCreditCurrencyCode");
							info.GlobalCreditGroupOrgCode = GetStringValue(reader, "GlobalCreditGroupOrgCode");
							info.GlobalCreditLimitHasValue = true;
							info.GlobalCreditLimit = GetValue<decimal>(reader, "GlobalCreditLimit").Value;

							decimal? decimalValue = GetValue<decimal>(reader, "GlobalOutstandingBalance");
							info.GlobalBalanceTotalHasValue = decimalValue.HasValue;
							info.GlobalBalanceTotal = decimalValue ?? decimal.Zero;

							decimalValue = GetValue<decimal>(reader, "GlobalClaim");
							info.GlobalClaimTotalHasValue = decimalValue.HasValue;
							info.GlobalClaimTotal = decimalValue ?? decimal.Zero;

							decimalValue = GetValue<decimal>(reader, "GlobalUnpostedRevenueUnrecognised");
							info.GlobalUnpostedRevenueRecognisedTotalHasValue = decimalValue.HasValue;
							info.GlobalUnpostedRevenueRecognisedTotal = decimalValue ?? decimal.Zero;

							decimalValue = GetValue<decimal>(reader, "GlobalUnpostedRevenueUnrecognised");
							info.GlobalUnpostedRevenueUnrecognisedTotalHasValue = decimalValue.HasValue;
							info.GlobalUnpostedRevenueUnrecognisedTotal = decimalValue ?? decimal.Zero;

							info.InvalidGlobalCreditCurrencyOrMissingExRateHasValue = true;
							info.InvalidGlobalCreditCurrencyOrMissingExRate = GetValue<bool>(reader, "InvalidGlobalCreditCurrencyOrMissingExRate").Value;
						}

						result.CreditLimitAndBalanceDetails.Add(info);
					}

					result.Succeeded = result.CreditLimitAndBalanceDetails.Count > 0;
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.Succeeded = false;
				result.ErrorMessage = ErrorHelper.ReportError(ex);
			}

			return result;
		}

		Constants.BalanceOverdueAgingOptionEnum getBalanceOverdueAgingOptionValue(string balanceOverdueAgingOptionValue)
		{
			if (balanceOverdueAgingOptionValue == Constants.BalanceOverdueAgingOption.Balance)
			{
				return Constants.BalanceOverdueAgingOptionEnum.Balance;
			}
			else if (balanceOverdueAgingOptionValue == Constants.BalanceOverdueAgingOption.RecognisedAndUnrecognisedRevenue)
			{
				return Constants.BalanceOverdueAgingOptionEnum.RecognisedAndUnrecognisedRevenue;
			}
			else if (balanceOverdueAgingOptionValue == Constants.BalanceOverdueAgingOption.Overdue)
			{
				return Constants.BalanceOverdueAgingOptionEnum.Overdue;
			}
			else if (balanceOverdueAgingOptionValue == Constants.BalanceOverdueAgingOption.Aging)
			{
				return Constants.BalanceOverdueAgingOptionEnum.Aging;
			}
			return Constants.BalanceOverdueAgingOptionEnum.Unknown;
		}

		public TransactionPaymentStatusResponse GetTransactionPaymentStatus(TransactionPaymentStatusRequest request)
		{
			var result = new TransactionPaymentStatusResponse();

			try
			{
				using (var reader = request.GenerateCheckTransactionPaymentStatusQuery(dbAccess).ExecuteReader())
				{
					TransactionPaymentStatusInfo info = null;

					while (reader.Read())
					{
						if (info != null)
						{
							result.Succeeded = false;
							result.ErrorMessage = (NoResString)"More than one transaction found for the given transaction details.";
							break;
						}

						info = new TransactionPaymentStatusInfo();
						info.JobTransactionNumber = request.JobTransactionNumber;

						info.OrgCode = GetStringValue(reader, "OrgCode");
						info.CompanyCode = GetStringValue(reader, "CompanyCode");
						info.AccLedger = GetStringValue(reader, (NoResString)"Ledger");
						info.TransactionType = GetStringValue(reader, "TransactionType");
						info.TransactionNumber = GetStringValue(reader, "TransactionNumber");
						info.CurrencyCode = GetStringValue(reader, "CurrencyCode");

						info.InvoiceTotal = GetValue<decimal>(reader, "InvoiceTotal").Value;
						info.PaidAmount = GetValue<decimal>(reader, "PaidAmount").Value;
						info.PaymentStatus = GetStringValue(reader, "PaymentStatus");

						var fullyPaidDate = GetValue<DateTime>(reader, "FullyPaidDate");
						if (fullyPaidDate.HasValue)
						{
							info.FullyPaidDate = fullyPaidDate.Value;
							info.FullyPaidDateHasValue = true;
						}
						else
						{
							info.FullyPaidDateHasValue = false;
						}

						result.Succeeded = true;
					}

					if (result.Succeeded)
					{
						result.TransactionPaymentStatus = info;
					}
					else if (info == null)
					{
						result.ErrorMessage = (NoResString)"Transaction not found";
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result.Succeeded = false;
				result.ErrorMessage = ErrorHelper.ReportError(ex);
			}

			return result;
		}

		T? GetValue<T>(System.Data.Common.DbDataReader reader, string fieldName, T? defaultValue = null)
			where T : struct
		{
			T? result = defaultValue;

			if (!reader[fieldName].Equals(DBNull.Value) && !string.IsNullOrEmpty(reader[fieldName].ToString().Trim()))
			{
				result = (T)reader[fieldName];
			}

			return result;
		}

		string GetStringValue(System.Data.Common.DbDataReader reader, string fieldName, bool returnEmptyStringIfNull = false)
		{
			string result = returnEmptyStringIfNull ? string.Empty : null;

			if (!reader[fieldName].Equals(DBNull.Value) && !string.IsNullOrEmpty(reader[fieldName].ToString().Trim()))
			{
				result = reader[fieldName].ToString().Trim();
			}

			return result;
		}
	}
}
