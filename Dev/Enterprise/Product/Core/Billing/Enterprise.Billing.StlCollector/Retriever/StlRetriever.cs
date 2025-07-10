using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class StlRetriever : IStlRetriever
	{
		public StlRetriever(ILogger serviceLogger)
			: this(serviceLogger, new CollectionTimeProvider())
		{
		}

		public StlRetriever(ILogger serviceLogger, CollectionTimeProvider timeProvider)
			: this(serviceLogger, new BillingDataCollectorFactory(), new ScriptLoader(timeProvider, serviceLogger), timeProvider)
		{
		}

		internal StlRetriever(ILogger serviceLogger, IBillingDataCollectorFactory dataCollectorFactory, IScriptLoader scriptLoader, CollectionTimeProvider timeProvider)
		{
			this.serviceLogger = serviceLogger;
			bizoFactory = new BusinessObjectFactory();
			var stlScripts = scriptLoader.Load(bizoFactory);
			legacyWaterMark = stlScripts.FirstOrDefault()?.LegacyHighWaterMarkSettings;
			mandatoryItemsCollector = dataCollectorFactory.Create(serviceLogger, bizoFactory, stlScripts.Where(s => s.Script.IsMandatoryForMilestones));
			nonMandatoryItemsCollector = dataCollectorFactory.Create(serviceLogger, bizoFactory, stlScripts.Where(s => !s.Script.IsMandatoryForMilestones));
			this.timeProvider = timeProvider;
		}

		readonly BusinessObjectFactory bizoFactory;
		readonly IStlItemRegistrySettings legacyWaterMark;
		readonly BillingDataCollector mandatoryItemsCollector;
		readonly BillingDataCollector nonMandatoryItemsCollector;
		readonly CollectionTimeProvider timeProvider;
		protected ILogger serviceLogger;

		public IEnumerable<IStlScriptWithConfig> Scripts => (new[] { mandatoryItemsCollector, nonMandatoryItemsCollector }).SelectMany(b => b.Scripts);

#if DEBUG
		public void CollectAndSend()
		{
			CollectAndSend(CancellationToken.None);
		}
#endif

		public void CollectAndSend(CancellationToken token)
		{
			ResetWatermarksIfRequired();
			var previousEndDateTimeExclusive = LoadLastEndDateTimeExclusive();
			var endDateTimeExclusive = timeProvider.MaximumSafeEndDateTimeExclusive;

			CollectAndSend(token, mandatoryItemsCollector, isMandatoryForMilestones: true, previousEndDateTimeExclusive, endDateTimeExclusive);
			CollectAndSend(token, nonMandatoryItemsCollector, isMandatoryForMilestones: false, previousEndDateTimeExclusive, endDateTimeExclusive);

			if (legacyWaterMark != null && mandatoryItemsCollector.Scripts.All(s => s.HighWaterMarkSettings.HasHighWaterMarkBeenSet) && nonMandatoryItemsCollector.Scripts.All(s => s.HighWaterMarkSettings.HasHighWaterMarkBeenSet))
			{
				legacyWaterMark.ClearHighWaterMark();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System Service Task")]
		void CollectAndSend(CancellationToken token, BillingDataCollector billingDataCollector, bool isMandatoryForMilestones, DateTime startDateTime, DateTime endDateTimeExclusive)
		{
			if (!billingDataCollector.Scripts.Any())
			{
				return;
			}

			serviceLogger.Log(
				LogType.Information,
				string.Format(CultureInfo.InvariantCulture,
					"Date Range: >= {0}, < {1}{2}, {3} Items",
					SqlFormatInfo.ToSqlDateTimeString(startDateTime),
					SqlFormatInfo.ToSqlDateTimeString(endDateTimeExclusive),
					(startDateTime < endDateTimeExclusive) ? "" : " (no STL collection)",
					isMandatoryForMilestones ? "Mandatory" : "Non-Mandatory"));

			var rangeStartInclusive = startDateTime;
			while (rangeStartInclusive < endDateTimeExclusive)
			{
				var rangeEndExclusive = BaseDateTimeRange.GetMinDateTimeValue(rangeStartInclusive.AddDays(1), endDateTimeExclusive);
				CollectAndSendRangeTransactions(token, billingDataCollector, rangeStartInclusive, rangeEndExclusive, lastCollection: rangeEndExclusive == endDateTimeExclusive);
				rangeStartInclusive = rangeEndExclusive;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System Service Task")]
		void CollectAndSendRangeTransactions(CancellationToken token, BillingDataCollector billingDataCollector, DateTime rangeStartInclusive, DateTime rangeEndExclusive, bool lastCollection)
		{
			var logInfo = string.Format(CultureInfo.InvariantCulture,
				"Collect and send: [{0}, {1})",
				SqlFormatInfo.ToSqlDateTimeString(rangeStartInclusive),
				SqlFormatInfo.ToSqlDateTimeString(rangeEndExclusive));
			serviceLogger.Log(LogType.Information, logInfo);

			var rangeCalculator = new RecurringRange(rangeStartInclusive, rangeEndExclusive);
			billingDataCollector.CollectAndSendData(token, rangeCalculator, lastCollection);
		}

		#region StlCollectorHighWaterMark

		protected DateTime LoadLastEndDateTimeExclusive()
		{
			var lowestNextStartTime = DateTime.MaxValue;
			foreach (var item in Scripts)
			{
				if (item.NextStartTimeUtc < lowestNextStartTime)
				{
					lowestNextStartTime = item.NextStartTimeUtc;
				}
			}

			return lowestNextStartTime;
		}

		#endregion

		#region WatermarkReset

		void ResetWatermarksIfRequired()
		{
			try
			{
				var lastResetTime = SystemDataRegistry.Instance.LastWatermarkResetTime.Value;
				var query = new ZQuery(RefSysConfigSchema.ZRC_ZRT_NKConfigCode, SQLComparisonOperator.Equal, WatermarkReset.STLWatermarkResetInfoCode);
				var refSysConfig = bizoFactory.Load<RefSysConfig>(query).FirstOrDefault();
				if (refSysConfig == null)
				{
					serviceLogger.Log(
						LogType.Debug,
						string.Format(
							CultureInfo.InvariantCulture,
							"No watermark reset values found, skipping reset"));

					return;
				}

				var resetValues = refSysConfig.ZRC_StringValue.Split('|');
				if (resetValues.Length != 3 || resetValues[0] == "" || resetValues[1] == "")
				{
					LogWatermarkResetConfigFormatError(refSysConfig);
					return;
				}
				SqlFormatInfo.TryParseFromSqlDateTime(resetValues[0], out var resetTo);
				SqlFormatInfo.TryParseFromSqlDateTime(resetValues[1], out var resetRequestTime);
				if (resetRequestTime <= DateTime.MinValue || resetTo <= DateTime.MinValue)
				{
					LogWatermarkResetConfigFormatError(refSysConfig);
					return;
				}

				if (lastResetTime != resetRequestTime)
				{
					ResetWatermarks(resetTo, resetRequestTime, resetValues[2].Split(','));
				}
				else
				{
					serviceLogger.Log(
						LogType.Debug,
						string.Format(
							CultureInfo.InvariantCulture,
							"Last watermark reset up-to-date, skipping reset"));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ExceptionReporter.Instance.ReportDeveloperException(ex.Message, ex);
			}
		}

		void ResetWatermarks(DateTime resetTo, DateTime resetRequestTime, ZString[] codes)
		{
			var allReset = codes.Length == 1 && codes[0] == "";
			foreach (var script in Scripts)
			{
				if ((allReset || codes.Contains(script.Script.Code)) && script.HighWaterMarkSettings.HasHighWaterMarkBeenSet && resetTo < script.HighWaterMarkSettings.HighWaterMark)
				{
					serviceLogger.Log(
						LogType.Information,
						string.Format(CultureInfo.InvariantCulture,
						"Resetting {0} Watermark from {1} to {2}",
						script.Script.Code,
						SqlFormatInfo.ToSqlDateTimeString(script.HighWaterMarkSettings.HighWaterMark),
						SqlFormatInfo.ToSqlDateTimeString(resetTo)));

					script.HighWaterMarkSettings.HighWaterMark = resetTo;
				}
			}
			bizoFactory.Save();
			SystemDataRegistry.Instance.LastWatermarkResetTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, resetRequestTime);
		}

		void LogWatermarkResetConfigFormatError(RefSysConfig refSysConfig)
		{
			serviceLogger.Log(
			LogType.Error,
			string.Format(
				CultureInfo.InvariantCulture,
				"Invalid watermark reset config format, value was: '{0}'. Skipping Reset",
				refSysConfig.ZRC_StringValue));
		}

		#endregion
	}
}
