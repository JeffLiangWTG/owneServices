using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine
{
	/// <summary>
	/// Configuration used for scheduling an archive run
	/// </summary>
	public class ArchiveConfiguration : IArchiveConfiguration
	{
		public ArchiveConfiguration(ZDateTime archiveJobsOnOrBeforeThisDate, ZInt maxRunDurationInMinutes, ZDateTime startRunTime, bool isVerboseLog, bool shouldIncludeDeclarations, bool shouldIncludeRecordsWithoutJobs = false, bool useOnOrBeforeDateWhenWatermarkReset = false)
		{
			ArchiveJobsOnOrBeforeThisDate = archiveJobsOnOrBeforeThisDate;
			MaxRunDurationInMinutes = maxRunDurationInMinutes;
			StartRunTime = startRunTime;
			IsVerboseLog = isVerboseLog;
			ShouldIncludeDeclarations = shouldIncludeDeclarations;
			ShouldIncludeRecordsWithoutJobs = shouldIncludeRecordsWithoutJobs;
			IsFilteringByJobOpenDate = false;
			UseOnOrBeforeDateWhenWatermarkReset = useOnOrBeforeDateWhenWatermarkReset;
			AMUsageReportValues = new Dictionary<string, object>();
		}

		public bool IsFilteringByJobOpenDate { get; private set; }

		public ZDateTime ArchiveJobsOnOrBeforeThisDate { get; private set; }

		public ZInt MaxRunDurationInMinutes { get; private set; }

		public ZDateTime StartRunTime { get; private set; }

		public ZDateTime StopRunTime { get { return StartRunTime.AddMinutes(MaxRunDurationInMinutes).ToDateTime(); } }

		public bool IsVerboseLog { get; private set; }

		public bool ShouldIncludeDeclarations { get; private set; }

		public bool ShouldIncludeRecordsWithoutJobs { get; private set; }

		public bool UseOnOrBeforeDateWhenWatermarkReset {  get; private set; }

		public IEnumerable<string> GetConfigurationLogs(IArchiveSystem system)
		{
			if (system.Descriptor.AllowDateParameterSelection)
			{
				var dateParameter = IsFilteringByJobOpenDate ? (NoResString)"Job Open Date" : (NoResString)"Job Close Date";
				yield return $"Date Parameter: {dateParameter}";
			}
			yield return $"{system.Descriptor.PresentTenseVerb.GetUnresolvedString()} Records on or Before: {ArchiveJobsOnOrBeforeThisDate.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture)}";
			yield return $"Max Run Duration: {MaxRunDurationInMinutes} minutes";
			yield return $"Verbose Logging: {IsVerboseLog.ToYesNoString()}";

			if (system.Descriptor.AllowShouldArchiveDeclaration)
			{
				yield return $"Incl. Customs: {ShouldIncludeDeclarations.ToYesNoString()}";
			}

			if (system.Descriptor.AllowUsingOnOrBeforeDateWhenWatermarkReset)
			{
				yield return $"Use 'On Or Before' Date When Watermark Is Reset: {UseOnOrBeforeDateWhenWatermarkReset.ToYesNoString()}";
			}
		}

		public Dictionary<string, object> AMUsageReportValues { get; private set; }

		public void SetIsFilteringByJobOpenDate(bool isFilteringByJobOpenDate)
		{
			IsFilteringByJobOpenDate = isFilteringByJobOpenDate;
		}
	}
}
