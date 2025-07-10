using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
#if DEBUG
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
#endif
	static class CashBasisVATManager
	{
		public static AccCashBasisVAT[] CreateRecords(TransactionMatchLink matchLink)
		{
			var transaction = matchLink.MatchingTransaction;
			if (transaction.AH_InvoiceAmount + transaction.AH_GSTAmount == 0)
			{
				return CreateRecordsForTransactionFullyPaidWithoutMatching(matchLink);
			}
			if (matchLink.AP_Amount == 0)
			{
				return Array.Empty<AccCashBasisVAT>();
			}

			var result = new List<AccCashBasisVAT>();
			var lines = LoadLines(matchLink);
			var localDecimals = Env.CurrentCompany.LocalCurrency.Decimals;
			decimal minimumCurrencyValue = (decimal)Math.Pow(10, -localDecimals);

			if (lines.Any())
			{
				var cashVATs = LoadCashBasisVATs(lines, false);

				var cashVATsByLines = (from line in lines
															 join cashVAT in cashVATs on line.PK equals cashVAT.YC_AL_TransactionLine into cashVATsForLine
															 select new
															 {
																 Line = line,
																 cashVATs = cashVATsForLine
															 }).ToArray();

				var amountsPendingByLines = new List<Tuple<ZGuid, decimal, decimal>>();
				decimal taxBasePendingTotal = 0;
				decimal taxPendingTotal = 0;
				foreach (var line in cashVATsByLines)
				{
					decimal lineTaxBasePending = line.Line.AL_LineAmount;
					decimal lineTaxPending = line.Line.AL_GSTVAT;
					foreach (var cashVAT in line.cashVATs)
					{
						lineTaxBasePending -= cashVAT.YC_TaxBaseAmount;
						lineTaxPending -= cashVAT.YC_TaxAmount;
					}
					if (lineTaxBasePending + lineTaxPending != 0)
					{
						amountsPendingByLines.Add(Tuple.Create(line.Line.PK, lineTaxBasePending, lineTaxPending));
						taxBasePendingTotal += lineTaxBasePending;
						taxPendingTotal += lineTaxPending;
					}
				}

				decimal totalPending = taxBasePendingTotal + taxPendingTotal;
				if (totalPending != 0)
				{
					var linesMatchLinksFilter = new ZQuery(AccTransLinePaySchema.A7_AP, matchLink.PK);
					linesMatchLinksFilter.FetchOnlyFromLocalCache = !matchLink.IsInDatabase;
					var matchedAmountsByLines = matchLink.Factory.Load<AccTransLinePay>(linesMatchLinksFilter).ToDictionary(item => item.A7_AL, item => item.A7_Amount);
					if (matchedAmountsByLines.Any())
					{
						foreach (var amountsPending in amountsPendingByLines)
						{
							ZGuid linePK = amountsPending.Item1;
							if (!matchedAmountsByLines.ContainsKey(linePK))
							{
								continue;
							}
							var cashBasisVAT = CreateRecordForMatchedAmountsByLines(matchLink, matchedAmountsByLines[linePK], amountsPending, localDecimals, minimumCurrencyValue);
							result.Add(cashBasisVAT);
						}
					}
					else
					{
						if (IsTaxFirst)
						{
							totalPending = taxPendingTotal;
						}
						var matchedAmount = matchLink.AP_Amount;
						var isFullyPaid = matchLink.MatchingTransaction.AH_OutstandingAmount == 0 && !matchLink.MatchingTransaction.AH_FullyPaidDate.IsEmpty;
						decimal matchedToPendingRate = totalPending == 0 || isFullyPaid ? 1 : Math.Min(Math.Abs(matchedAmount / totalPending), 1);
						decimal totalToCreate = 0;

						Func<List<Tuple<ZGuid, decimal, decimal>>, bool, List<Tuple<ZGuid, decimal, decimal>>> createCashVATRecords = (amountsPendingByLinesToProcess, processZeroValues) =>
						{
							var linesWithZeroValuesToCreate = new List<Tuple<ZGuid, decimal, decimal>>();
							if (Math.Abs(totalToCreate) < Math.Abs(matchedAmount))
							{
								foreach (var amountsPending in amountsPendingByLinesToProcess)
								{
									ZGuid linePK = amountsPending.Item1;
									decimal lineTaxBasePending = amountsPending.Item2;
									decimal lineTaxPending = amountsPending.Item3;
									decimal taxBaseToCreate = Utilities.Round(lineTaxBasePending * matchedToPendingRate, localDecimals);
									decimal taxToCreate = Utilities.Round(lineTaxPending * matchedToPendingRate, localDecimals);

									if (processZeroValues)
									{
										if (Math.Abs(totalToCreate) < Math.Abs(matchedAmount))
										{
											if (IsTaxBaseToCreateZero(taxBaseToCreate))
											{
												taxBaseToCreate = minimumCurrencyValue * Math.Sign(lineTaxBasePending);
											}
											if (IsTaxToCreateZero(taxToCreate, lineTaxPending))
											{
												taxToCreate = minimumCurrencyValue * Math.Sign(lineTaxPending);
											}
										}
									}

									if (Math.Abs(taxBaseToCreate) >= Math.Abs(lineTaxBasePending))
									{
										taxToCreate = lineTaxPending;
									}
									else if (Math.Abs(taxToCreate) >= Math.Abs(lineTaxPending) && (lineTaxPending != 0 || IsTaxFirst))
									{
										taxBaseToCreate = lineTaxBasePending;
									}

									if (!IsTaxBaseToCreateZero(taxBaseToCreate) && !IsTaxToCreateZero(taxToCreate, lineTaxPending))
									{
										totalToCreate += taxToCreate;
										if (!IsTaxFirst)
										{
											totalToCreate += taxBaseToCreate;
										}

										var cashBasisVAT = CreateCashBasisVAT(matchLink, linePK, taxBaseToCreate, taxToCreate);
										result.Add(cashBasisVAT);
									}
									else
									{
										linesWithZeroValuesToCreate.Add(amountsPending);
									}
								}
							}
							return linesWithZeroValuesToCreate;
						};

						var amountsPendingByLinesWithZeroValuesToCreate = createCashVATRecords(amountsPendingByLines, false);

						createCashVATRecords(amountsPendingByLinesWithZeroValuesToCreate, true);
					}
				}
			}

			return result.ToArray();
		}

		static AccCashBasisVAT CreateRecordForMatchedAmountsByLines(TransactionMatchLink matchLink, decimal matchedAmount, Tuple<ZGuid, decimal, decimal> amountsPending, int localDecimals, decimal minimumCurrencyValue)
		{
			ZGuid linePK = amountsPending.Item1;
			decimal lineTaxBasePending = amountsPending.Item2;
			decimal lineTaxPending = amountsPending.Item3;
			decimal lineTotalPending = lineTaxBasePending + lineTaxPending;
			if (IsTaxFirst)
			{
				lineTotalPending = lineTaxPending;
			}
			decimal matchedToPendingRateForLine = lineTotalPending == 0 ? 1 : Math.Min(matchedAmount / lineTotalPending, 1);
			decimal taxBaseToCreate = Utilities.Round(lineTaxBasePending * matchedToPendingRateForLine, localDecimals);
			decimal taxToCreate = Utilities.Round(lineTaxPending * matchedToPendingRateForLine, localDecimals);

			if (IsTaxBaseToCreateZero(taxBaseToCreate))
			{
				taxBaseToCreate = minimumCurrencyValue * Math.Sign(lineTaxBasePending);
			}
			if (IsTaxToCreateZero(taxToCreate, lineTaxPending))
			{
				taxToCreate = minimumCurrencyValue * Math.Sign(lineTaxPending);
			}

			if (Math.Abs(taxBaseToCreate) >= Math.Abs(lineTaxBasePending))
			{
				taxToCreate = lineTaxPending;
			}
			else if (Math.Abs(taxToCreate) >= Math.Abs(lineTaxPending) && (lineTaxPending != 0 || IsTaxFirst))
			{
				taxBaseToCreate = lineTaxBasePending;
			}

			return CreateCashBasisVAT(matchLink, linePK, taxBaseToCreate, taxToCreate);
		}

		static bool IsTaxBaseToCreateZero(decimal taxBaseToCreate)
		{
			return taxBaseToCreate == 0;
		}

		static bool IsTaxToCreateZero(decimal taxToCreate, decimal lineTaxPending)
		{
			return taxToCreate == 0 && lineTaxPending != 0;
		}

		static bool IsTaxFirst
		{
			get { return AccountingConfigurationRegistry.Instance.PartPaymentTaxRealizationRule.Value == AccountingConstants.PartPaymentTaxRealizationRuleTypes.TaxFirst.Code; }
		}

		public static AccCashBasisVAT[] CreateRecordsForTransactionFullyPaidWithoutMatching(InvoicingBase transaction)
		{
			var result = new List<AccCashBasisVAT>();
			var cashVATLines = transaction.Lines.Cast<InvoicingLineBase>().Where(item => item.AL_GSTVATBasis == AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code).ToArray();
			foreach (var line in cashVATLines)
			{
				var cashBasisVAT = transaction.Factory.New<AccCashBasisVAT>();
				cashBasisVAT.YC_AL_TransactionLine = line.PK;
				cashBasisVAT.YC_MatchGroupNum = "";
				cashBasisVAT.YC_PostDate = transaction.AH_FullyPaidDate;
				cashBasisVAT.YC_TaxBaseAmount = line.AL_LineAmount;
				cashBasisVAT.YC_TaxAmount = line.AL_GSTVAT;
				result.Add(cashBasisVAT);
			}

			return result.ToArray();
		}

		public static AccCashBasisVAT[] ReverseRecords(TransactionMatchLinkCollection matchLinks, ZDateTime reverseDate)
		{
			var result = new List<AccCashBasisVAT>();
			var factory = matchLinks.Factory;
			var uniqueMatchGroupNumbers = from TransactionMatchLink matchLink in matchLinks
										  group matchLink by matchLink.AP_MatchGroupNum into matchGroupNumbers
										  select matchGroupNumbers.Key;
			var filter = new ZQuery(AccCashBasisVATSchema.YC_MatchGroupNum, uniqueMatchGroupNumbers);
			filter.AddToFilter(AccCashBasisVATSchema.YC_GC, GlbCompany.CurrentCompany.PK);
			var cashVATRecords = factory.Load<AccCashBasisVAT>(filter);
			foreach (var cashVAT in cashVATRecords)
			{
				result.Add(ReverseCashBasisVAT(cashVAT, reverseDate));
			}

			return result.ToArray();
		}

		static AccCashBasisVAT[] CreateRecordsForTransactionFullyPaidWithoutMatching(TransactionMatchLink matchLink)
		{
			var result = new List<AccCashBasisVAT>();

			var lines = LoadLines(matchLink);
			var cashVATsForZeroValueTransactions = LoadCashBasisVATs(lines, true);
			var cashVATsForZeroValueTransactions_GetOnlySingle = (from cashVAT in cashVATsForZeroValueTransactions
																  group cashVAT by cashVAT.YC_AL_TransactionLine into cashVATGroups
																  where cashVATGroups.Count() == 1
																  select cashVATGroups.First()).ToList();
			cashVATsForZeroValueTransactions_GetOnlySingle.ForEach(item => result.Add(ReverseCashBasisVAT(item, matchLink.AP_MatchDate)));

			var factory = matchLink.Factory;
			foreach (var line in lines)
			{
				var cashBasisVAT = factory.New<AccCashBasisVAT>();
				cashBasisVAT.YC_AL_TransactionLine = line.PK;
				cashBasisVAT.YC_MatchGroupNum = matchLink.AP_MatchGroupNum;
				cashBasisVAT.YC_PostDate = matchLink.AP_MatchDate;
				cashBasisVAT.YC_TaxBaseAmount = line.AL_LineAmount;
				cashBasisVAT.YC_TaxAmount = line.AL_GSTVAT;
				result.Add(cashBasisVAT);
			}

			return result.ToArray();
		}

		static InvoicingLineBase[] LoadLines(TransactionMatchLink matchLink)
		{
			var lines = Array.Empty<InvoicingLineBase>();
			var factory = matchLink.Factory;
			var transaction = matchLink.MatchingTransaction as TransactionHeaderWithLines;
			if (transaction != null)
			{
				var fullyRecognizedLinePKToExcludeSQL = @"
SELECT AL_PK
FROM 
	dbo.AccCashBasisVAT
	JOIN dbo.AccTransactionLines ON YC_AL_TransactionLine = AL_PK
WHERE AL_AH = @AH_PK AND YC_MatchGroupNum != ''
GROUP BY AL_PK, AL_LineAmount, AL_GSTVAT
HAVING SUM(YC_TaxBaseAmount + YC_TaxAmount) = AL_LineAmount + AL_GSTVAT
";
				var fullyRecognizedLinePKToExclude = new DynamicBusinessObjectCollection(factory);
				fullyRecognizedLinePKToExclude.Load(fullyRecognizedLinePKToExcludeSQL, new[] { ZSqlParameter.New("@AH_PK", transaction.PK, AccTransactionLinesSchema.AL_AH) });

				var lineFilter = new ZQuery(AccTransactionLinesSchema.AL_AH, transaction.PK);
				lineFilter.AddToFilter(AccTransactionLinesSchema.AL_GSTVATBasis, AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code);
				lineFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, transaction.AH_GC);
				if (fullyRecognizedLinePKToExclude.Count > 0)
				{
					lineFilter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.NotEqual, fullyRecognizedLinePKToExclude.Select(item => item[AccTransactionLinesSchema.Constants.PK]));
				}
				lines = (InvoicingLineBase[])factory.Load(transaction.DependentTransactionLineType, lineFilter);
			}

			return lines;
		}

		static List<AccCashBasisVAT> LoadCashBasisVATs(InvoicingLineBase[] lines, bool loadRecordsWithEmptyMatchNumber)
		{
			int linesProcessed = 0;
			List<AccCashBasisVAT> cashVATRecords = new List<AccCashBasisVAT>();
			while (linesProcessed < lines.Length)
			{
				int batchCount = Math.Min(lines.Length - linesProcessed, GetBatchSize());
				var linePKs = lines.Skip(linesProcessed).Take(batchCount).Select(item => item.PK);
				var cashVATRecordsFilter = new ZQuery(AccCashBasisVATSchema.YC_AL_TransactionLine, linePKs);
				cashVATRecordsFilter.AddToFilter(AccCashBasisVATSchema.YC_MatchGroupNum, loadRecordsWithEmptyMatchNumber ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, ZString.Empty);
				cashVATRecordsFilter.AddToFilter(AccCashBasisVATSchema.YC_GC, GlbCompany.CurrentCompany.PK);
				cashVATRecords.AddRange(lines[0].Factory.Load<AccCashBasisVAT>(cashVATRecordsFilter));

				linesProcessed += batchCount;
			}

			return cashVATRecords;
		}

		static AccCashBasisVAT CreateCashBasisVAT(TransactionMatchLink matchLink, ZGuid linePK, decimal taxBaseAmount, decimal taxAmount)
		{
			var factory = matchLink.Factory;
			var cashBasisVAT = factory.New<AccCashBasisVAT>();
			cashBasisVAT.YC_AL_TransactionLine = linePK;
			cashBasisVAT.YC_MatchGroupNum = matchLink.AP_MatchGroupNum;
			cashBasisVAT.YC_PostDate = matchLink.AP_MatchDate;
			cashBasisVAT.YC_TaxBaseAmount = taxBaseAmount;
			cashBasisVAT.YC_TaxAmount = taxAmount;

			return cashBasisVAT;
		}

		static AccCashBasisVAT ReverseCashBasisVAT(AccCashBasisVAT originalRecord, ZDateTime reverseDate)
		{
			var cashBasisVAT = originalRecord.Factory.New<AccCashBasisVAT>();
			cashBasisVAT.YC_AL_TransactionLine = originalRecord.YC_AL_TransactionLine;
			cashBasisVAT.YC_MatchGroupNum = originalRecord.YC_MatchGroupNum;
			cashBasisVAT.YC_PostDate = reverseDate;
			cashBasisVAT.YC_TaxBaseAmount = -originalRecord.YC_TaxBaseAmount;
			cashBasisVAT.YC_TaxAmount = -originalRecord.YC_TaxAmount;

			return cashBasisVAT;
		}

		static int GetBatchSize()
		{
			var result = BatchSize;
			#if DEBUG
			if (Globals.IsTest)
			{
				result = BatchSizeForTests;
			}
			#endif

			return result;
		}

		internal const int BatchSize = 1000;

#if DEBUG
		public static IDisposable SetBatchSizeForTests(int batchSize)
		{
			batchSizeForTests = batchSize;
			return new BatchSizeResetter();
		}

		static int BatchSizeForTests
		{
			get { return (batchSizeForTests ?? (batchSizeForTests = DefaultSetBatchSizeForTests)).Value; }
		}

		[ThreadStatic]
		static int? batchSizeForTests;

		const int DefaultSetBatchSizeForTests = 3;

		class BatchSizeResetter : IDisposable
		{
			void IDisposable.Dispose()
			{
				batchSizeForTests = DefaultSetBatchSizeForTests;
			}
		}
#endif
	}
}
