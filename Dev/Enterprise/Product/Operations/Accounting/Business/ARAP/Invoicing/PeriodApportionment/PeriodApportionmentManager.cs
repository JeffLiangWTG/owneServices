using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class PeriodApportionmentManager
	{
		public PeriodApportionmentManager(InvoicingLineBase line)
		{
			InvoicingLine = Argument.NotNull(line, nameof(line));
		}

		public InvoicingLineBase InvoicingLine { get; }

		public void DefaultClearingAccount()
		{
			var invoice = InvoicingLine.InvoiceBase;
			ZGuid clearingAccountPK = ZGuid.Empty;

			if (invoice != null && invoice.SupportMultiPeriodApportionment && !string.IsNullOrWhiteSpace(InvoicingLine.PeriodApportionmentMethod) && InvoicingLine.PeriodApportionmentMethod != PeriodApportionmentMethods.Codes.Default)
			{
				var chargeCode = InvoicingLine.ChargeCode;
				var glPostingAccounts = InvoicingLine.GetDefaultGLPostingAccounts();

				if (InvoicingLine.IsAP())
				{
					clearingAccountPK = glPostingAccounts.CostClearingAccount;
					clearingAccountPK = clearingAccountPK.IsEmpty ? chargeCode?.AC_AG_CostClearingAccount ?? ZGuid.Empty : clearingAccountPK;
					clearingAccountPK = clearingAccountPK.IsEmpty ? new ZGuid(AccountingConfigurationRegistry.Instance.PeriodApportionmentAPClearingAccount.Value) : clearingAccountPK;
				}
				else if (InvoicingLine.IsAR())
				{
					clearingAccountPK = glPostingAccounts.RevenueClearingAccount;
					clearingAccountPK = clearingAccountPK.IsEmpty ? chargeCode?.AC_AG_RevenueClearingAccount ?? ZGuid.Empty : clearingAccountPK;
					clearingAccountPK = clearingAccountPK.IsEmpty ? new ZGuid(AccountingConfigurationRegistry.Instance.PeriodApportionmentARClearingAccount.Value) : clearingAccountPK;
				}
			}

			if (clearingAccountPK != InvoicingLine.PeriodClearingGLAccountPK)
			{
				InvoicingLine.PeriodClearingGLAccountPK = clearingAccountPK;
				InvoicingLine.PeriodClearingGLAccountPKInfo.RefreshBinding();
			}
		}

		public void Recalculate()
		{
			if (InvoicingLine.InvoiceBase != null && InvoicingLine.InvoiceBase.SupportMultiPeriodApportionment)
			{
				RecalculateCore(InvoicingLine.PeriodApportionmentMethod);
				UpdateLastLine();
				Lines.RefreshBinding();
			}
		}

		string previousNonDefaultNonManualMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;

		void RecalculateCore(string method)
		{
			if (!method.In(PeriodApportionmentMethods.Codes.Default, PeriodApportionmentMethods.Codes.Manual, string.Empty))
			{
				previousNonDefaultNonManualMethod = method;
			}

			switch (method)
			{
				case PeriodApportionmentMethods.Codes.EquallyOverPeriods:
					Lines.RemoveAndDeleteAll();
					SplitEquallyOverMultiplePeriods();
					break;

				case PeriodApportionmentMethods.Codes.Day:
					Lines.RemoveAndDeleteAll();
					SplitBasedOnNumberOfDaysInPeriod();
					break;

				case PeriodApportionmentMethods.Codes.Manual:
					if (TryGetPeriods(out var periodsPair) && GetPeriodCount(periodsPair) != Lines.Count)
					{
						RecalculateCore(previousNonDefaultNonManualMethod);
					}
					break;

				default:
					Lines.RemoveAndDeleteAll();
					break;
			}
		}

		AccountingPeriodCalculator PeriodCalculator => InvoicingLine.InvoiceBase.PeriodCalculator;

		bool TryGetPeriods(out (int start, int end) periodsPair)
		{
			if (!InvoicingLine.PeriodStartDate.IsValid || !InvoicingLine.PeriodEndDate.IsValid)
			{
				periodsPair = default;
				return false;
			}

			periodsPair = (PeriodCalculator.GetPeriodFromDate(InvoicingLine.PeriodStartDate.ToDateTime()), PeriodCalculator.GetPeriodFromDate(InvoicingLine.PeriodEndDate.ToDateTime()));
			return true;
		}

		int GetPeriodCount((int start, int end) periodsPair) => periodsPair != default ? PeriodCalculator.GetPeriodCount(periodsPair.start, periodsPair.end) : ZInt.Zero;

		void SplitEquallyOverMultiplePeriods()
		{
			if (TryGetPeriods(out var periodsPair))
			{
				var periodCount = GetPeriodCount(periodsPair);

				if (periodCount > 0)
				{
					var osAmountPerPeriod = AccountingUtils.Round(InvoicingLine.AL_OSExTaxAmount / periodCount, InvoicingLine.AL_RX_NKTransactionCurrency);
					var osAmountNotRecoverableTaxPerPeriod = AccountingUtils.Round(InvoicingLine.AL_OSTaxAmount_NotRecoverable / periodCount, InvoicingLine.AL_RX_NKTransactionCurrency);

					var currentPeriod = periodsPair.start;
					while (currentPeriod < periodsPair.end)
					{
						var newline = new PeriodApportionmentLine(InvoicingLine, currentPeriod, osAmountPerPeriod, osAmountNotRecoverableTaxPerPeriod);
						Lines.Add(newline);
						currentPeriod = PeriodCalculator.GetNextPeriod(currentPeriod);
					}

					if (periodCount > 1)
					{
						var lastLine = new PeriodApportionmentLine(InvoicingLine, periodsPair.end);
						Lines.Add(lastLine);
						UpdateLastLine();
					}
				}
			}
		}

		void SplitBasedOnNumberOfDaysInPeriod()
		{
			if (TryGetPeriods(out var periodsPair))
			{
				var periodCount = GetPeriodCount(periodsPair);

				if (periodCount > 0)
				{
					var totalDays = (InvoicingLine.PeriodEndDate - InvoicingLine.PeriodStartDate).Days + 1;

					var osAmountPerDay = InvoicingLine.AL_OSExTaxAmount / totalDays;

					var osTaxAmountPerDay = InvoicingLine.AL_OSTaxAmount_NotRecoverable / totalDays;

					var currentPeriod = periodsPair.start;

					var firstLineNumberOfDays = (PeriodCalculator.GetLastDayForPeriod(currentPeriod) - InvoicingLine.PeriodStartDate).Days + 1;
					var firstLineOSAmount = AccountingUtils.Round(osAmountPerDay * firstLineNumberOfDays, InvoicingLine.AL_RX_NKTransactionCurrency);
					var firstLineOSTaxAmount = AccountingUtils.Round(osTaxAmountPerDay * firstLineNumberOfDays, InvoicingLine.AL_RX_NKTransactionCurrency);
					var firstLine = new PeriodApportionmentLine(InvoicingLine, currentPeriod, firstLineOSAmount, firstLineOSTaxAmount);
					Lines.Add(firstLine);

					currentPeriod = PeriodCalculator.GetNextPeriod(currentPeriod);

					while (currentPeriod != 0 && currentPeriod < periodsPair.end)
					{
						var lineNumberOfDays = (PeriodCalculator.GetLastDayForPeriod(currentPeriod) - PeriodCalculator.GetFirstDayForPeriod(currentPeriod)).Days + 1;

						var lineOSAmount = AccountingUtils.Round(osAmountPerDay * lineNumberOfDays, InvoicingLine.AL_RX_NKTransactionCurrency);
						var lineOSTaxAmount = AccountingUtils.Round(osTaxAmountPerDay * lineNumberOfDays, InvoicingLine.AL_RX_NKTransactionCurrency);

						var newline = new PeriodApportionmentLine(InvoicingLine, currentPeriod, lineOSAmount, lineOSTaxAmount);
						Lines.Add(newline);
						currentPeriod = PeriodCalculator.GetNextPeriod(currentPeriod);
					}

					if (periodCount > 1)
					{
						var lastLine = new PeriodApportionmentLine(InvoicingLine, periodsPair.end);
						Lines.Add(lastLine);
						UpdateLastLine();
					}
				}
			}
		}

		public void UpdateLastLine()
		{
			var splitSet = Lines.Cast<PeriodApportionmentLine>().Split(x => x.IsLastLine);
			var lastLine = splitSet.MatchingSet.SingleOrDefault(x => x.IsLastLine);

			if (lastLine != null)
			{
				var otherLines = splitSet.NonMatchingSet.ToArray();
				var osSum = otherLines.Sum(x => x.OSAmount);
				var locSum = otherLines.Sum(x => x.LocalAmount);

				var osTaxSum = otherLines.Sum(x => x.OSTaxNotRecoverable);
				var locTaxSum = otherLines.Sum(x => x.LocalTaxNotRecoverable);

				var osTotal = InvoicingLine.AL_OSExTaxAmount;
				var localTotal = InvoicingLine.AL_LocalExTaxAmount;

				var osTaxTotal = InvoicingLine.AL_OSTaxAmount_NotRecoverable;
				var localTaxTotal = InvoicingLine.AL_LocalTaxAmount_NotRecoverable;

				var signAdjuster = Math.Sign(osTotal) * Math.Sign(localTotal);
				localTotal *= signAdjuster;

				var signTaxAdjuster = Math.Sign(osTaxTotal) * Math.Sign(localTaxTotal);
				localTaxTotal *= signTaxAdjuster;

				lastLine.OSAmount = osTotal - osSum;
				lastLine.LocalAmount = localTotal - locSum;

				lastLine.OSTaxNotRecoverable = osTaxTotal - osTaxSum;
				lastLine.LocalTaxNotRecoverable = localTaxTotal - locTaxSum;
			}
		}

		public ZArchitecture.Environment.ExchangeRate CompanyExchangeRate => (InvoicingLine.Company ?? Env.CurrentCompany).ExchangeRate;

		public PeriodApportionmentLinesCollection Lines => lines ?? (lines = new PeriodApportionmentLinesCollection(InvoicingLine));
		PeriodApportionmentLinesCollection lines;
	}
}
