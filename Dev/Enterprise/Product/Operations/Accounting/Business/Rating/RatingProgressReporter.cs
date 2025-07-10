using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Accounting.Business
{
	public class RatingProgressReporter(
		IAutoRatingStrategy[] ratingItems,
		IAutoRatingGUIInteractor interactor,
		string currentProcessName,
		AutoRateOptions options,
		IRatingAdapterCountWrapper countWrapper = null)
	{
		readonly IRatingAdapterCountWrapper CountWrapper = countWrapper ?? new RatingAdapterCountWrapper();
		readonly long StartTicks = ZDateTime.Now.Ticks;
		int? TotalAdapterCount;
		int AutoratedAdapterCount;

		public void IncrementAndReport(IAutoRating autoRating)
		{
			AutoratedAdapterCount++;
			AutoratedAdaptersForErrorReporting.Add(autoRating?.GetType());

			// Getting the values in the constructor does not always return the correct values.
			// This is because the adapters sometimes are not ready to be included in the calculation.
			// For example, a customs declaration is waiting for a response and having an error.
			// So we calculate the values here instead, at the first increment, in the middle of autorating
			// to make sure all the adapters to use for autorating are included.
			if (!TotalAdapterCount.HasValue)
			{
				TotalAdapterCount = ratingItems.Sum(x => CountWrapper.GetRatingAdaptersCount(x, options));

				AdapterProviderInfosForErrorReporting = ratingItems
					.GroupBy(CountWrapper.GetAdaptersProviderType)
					.Where(g => g.Key != null)
					.ToDictionary(
						g => g.Key,
						g => g.Sum(x => CountWrapper.GetRatingAdaptersCount(x, options))
					);
			}

			var elapsedTicks = ZDateTime.Now.Ticks - StartTicks;
			elapsedTicks = elapsedTicks == 0 ? 1 : elapsedTicks;
			var speedPerTick = (decimal)AutoratedAdapterCount / elapsedTicks;
			var remaining = TotalAdapterCount - AutoratedAdapterCount;
			var eta = new TimeSpan(Convert.ToInt64(remaining / speedPerTick));

			// Something went wrong, maybe an unnecessary extra call to IncrementAndReport?
			if (TotalAdapterCount < AutoratedAdapterCount)
			{
				ReportError();
			}

			interactor.ReportProgress(currentProcessName, TotalAdapterCount.Value, Math.Min(TotalAdapterCount.Value, AutoratedAdapterCount), eta, speedPerTick);
		}

		#region Error Reporting

		Dictionary<Type, int> AdapterProviderInfosForErrorReporting;
		readonly List<Type> AutoratedAdaptersForErrorReporting = [];

		void ReportError()
		{
			var errorMessageBuilder = new ZStringBuilder();

			#region SuppressResourceStringsCheckRegion

			errorMessageBuilder.Append("The number of processed adapters was greater than the total number.");
			errorMessageBuilder.Append("Done: " + AutoratedAdapterCount);
			errorMessageBuilder.Append("Total: " + TotalAdapterCount);
			errorMessageBuilder.Append("Rating Adapter Providers Info list:");
			foreach (var adapterProvider in AdapterProviderInfosForErrorReporting)
			{
				errorMessageBuilder.Append($"{adapterProvider.Key.FullName ?? "null"}: {adapterProvider.Value}");
			}

			errorMessageBuilder.Append("AutoRated list:");
			foreach (var autoRatingType in AutoratedAdaptersForErrorReporting)
			{
				errorMessageBuilder.Append(autoRatingType?.FullName ?? "null");
			}

			errorMessageBuilder.Append("AutoRateCost:" + options.AutoRateCost);
			errorMessageBuilder.Append("AutoRateRevenue:" + options.AutoRateRevenue);
			errorMessageBuilder.Append("BillingType:" + options.BillingType);

			#endregion

			ErrorReporter.ReportOnce(errorMessageBuilder.ToStringWithNewLineBetweenAppends());
		}

		#endregion

#if DEBUG

		#region For Testing

		public int AutoratedAdapterCountForTest => AutoratedAdapterCount;
		public int? TotalAdapterCountForTest => TotalAdapterCount;

		#endregion

#endif
	}

	public interface IRatingAdapterCountWrapper
	{
		int GetRatingAdaptersCount(IAutoRatingStrategy strategy, AutoRateOptions options);
		Type GetAdaptersProviderType(IAutoRatingStrategy strategy);
	}

	public class RatingAdapterCountWrapper : IRatingAdapterCountWrapper
	{
		public int GetRatingAdaptersCount(IAutoRatingStrategy strategy, AutoRateOptions options) => strategy.GetRatingAdaptersCount(options);
		public Type GetAdaptersProviderType(IAutoRatingStrategy strategy) => strategy.AdaptersProvider()?.GetType();
	}
}
