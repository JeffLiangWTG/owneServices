using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class AccrualSummator : IAccrualSummator
	{
		public AccrualSummator()
		{
			tracer = ObjectFactory.Get<ITracer>();
		}
		readonly ITracer tracer;

		const int maxAllowedToBeFound = 1;
		internal long TimeOutInMilliSeconds = 60000; //60 seconds

#if DEBUG
		public bool EnterRecursiveCall_ForTestOnly;
		public int SimulateLongDelayInMillisec_ForTestOnly;
#endif

		IEnumerable<(string CombinationKey, IEnumerable<APReconciliationLine> Accruals)> IAccrualSummator.Sumup(IEnumerable<APReconciliationLine> reconciliationLines, decimal targetAmount, Func<APReconciliationLine, decimal> amountFieldSelector)
		{
			var sumUpResult = SumUpToTarget(reconciliationLines.OrderBy(amountFieldSelector), targetAmount, amountFieldSelector);
			return sumUpResult.Any() ? sumUpResult.Select(x => (Guid.NewGuid().ToString(), x.AsEnumerable())) : Enumerable.Empty<(string, IEnumerable<APReconciliationLine>)>();
		}

		List<List<APReconciliationLine>> SumUpToTarget(IEnumerable<APReconciliationLine> lines, decimal targetAmount, Func<APReconciliationLine, decimal> amountFieldSelector)
		{
			var result = new List<List<APReconciliationLine>>();
			tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => (NoResString)"Start sum up calculation...");
			// if the target amount is outside the possible sum up range then no need to run the recursive search
			if (lines.Where(x => amountFieldSelector(x) > 0).Sum(x => amountFieldSelector(x)) < targetAmount ||
				lines.Where(x => amountFieldSelector(x) < 0).Sum(x => amountFieldSelector(x)) > targetAmount)
			{
				return result;
			}
			var sw = Stopwatch.StartNew();
			SumUpToTargetRecursively(sw, targetAmount, lines, new List<APReconciliationLine>(), result, amountFieldSelector);
			sw.Stop();
			tracer.TraceInformation(AccountingTraceSourceCodes.APA, () => FormattableString.Invariant($"End sum up calculation, found {result.Count} matches"));
			return result;
		}

		void SumUpToTargetRecursively(Stopwatch stopWatch, decimal targetAmount, IEnumerable<APReconciliationLine> lines, List<APReconciliationLine> partialLines, List<List<APReconciliationLine>> result, Func<APReconciliationLine, decimal> amountFieldSelector)
		{
			var targetAmountSign = Math.Sign(targetAmount);
			var targetAmountAbs = Math.Abs(targetAmount);
			var sameSign = lines.All(l => Math.Sign(amountFieldSelector(l)) == targetAmountSign);

			Func<APReconciliationLine, bool> amountFilter = sameSign ? (l) => Math.Abs(amountFieldSelector(l)) <= targetAmountAbs : (l) => true;

			var indexedLines = lines.Select((item, index) => (item, index)).ToList();
#if DEBUG
			if (Globals.IsTest)
			{
				EnterRecursiveCall_ForTestOnly = true;
				if (SimulateLongDelayInMillisec_ForTestOnly > 0)
				{
					System.Threading.Thread.Sleep(SimulateLongDelayInMillisec_ForTestOnly);
				}
			}
#endif
			if (stopWatch.ElapsedMilliseconds > TimeOutInMilliSeconds)
			{
				throw new APAReconciliationTimeoutException(string.Format("Sumup time out after {0} milliseconds!", TimeOutInMilliSeconds));
			}
			foreach (var (currentLine, index) in indexedLines.Where(li => amountFilter(li.item)))
			{
				if (amountFieldSelector(currentLine) == targetAmount)
				{
					result.Add(new List<APReconciliationLine>(partialLines) { currentLine });
					if (result.Count > maxAllowedToBeFound)
					{
						// use exception to break out from the recursive calls as no need to go further
						throw new APAReconciliationTooManyMatchesFoundException(string.Format("There are more than {0} combination found!", maxAllowedToBeFound));
					}
				}
				else
				{
					SumUpToTargetRecursively(stopWatch, targetAmount - amountFieldSelector(currentLine), indexedLines.Skip(index + 1).Select(li => li.item).ToList(), new List<APReconciliationLine>(partialLines) { currentLine }, result, amountFieldSelector);
				}
			}
		}
	}
}
