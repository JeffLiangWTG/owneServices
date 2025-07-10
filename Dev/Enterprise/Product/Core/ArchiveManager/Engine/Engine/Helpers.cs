using System;
using System.Collections.Generic;
using System.Diagnostics;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine
{
	public static class Helpers
	{
		public static void ToLog(string commonLog, IEnumerable<string> logs, IArchiveLogger logger, IArchiveSystem system)
		{
			logger.LogInfo(system.Descriptor.Code, commonLog);
			logs.ForEach(log => logger.LogInfo(system.Descriptor.Code, log));
		}

		public static string ToYesNoString(this bool value)
			=> value ? (NoResString)"Yes" : (NoResString)"No";

		public static string GetTimeTaken(Stopwatch sw)
		{
			return sw != null ? $" Time taken: {sw.ElapsedMilliseconds}ms" : string.Empty;
		}

		public static string ToArchiveItemPKSanitised(IArchiveSet archiveSet)
			=> archiveSet.MainArchiveItem.PK.ToString().Replace("-", "");

		public static void SetWatermarkUsingQueryResults(IArchiveSchedule schedule, string stageName, DbCommand command, IArchiveConfiguration config = null)
		{
			var newWatermarkDateParamValue = command.GetParameterValue("@newWatermarkDate");
			var newWatermarkNKParamValue = command.GetParameterValue("@newWatermarkNK");
			var newWatermarkPKParamValue = command.GetParameterValue("@newWatermarkPK");
			var newWatermarkDate = (newWatermarkDateParamValue as DateTime?) ?? ZDateTime.MinSmallDateTimeValue;
			var newWatermarkNK = newWatermarkNKParamValue as string;
			var newWatermarkPK = (newWatermarkPKParamValue as Guid?) ?? Guid.Empty;

			if (newWatermarkDate > ZDateTime.MinSmallDateTimeValue)
			{
				var newWatermark = new ArchiveWatermark
				{
					WatermarkDate = newWatermarkDate,
					WatermarkNK = newWatermarkNK,
					WatermarkPK = newWatermarkPK
				};

				schedule.SetWatermark(stageName, newWatermark);
			}
			else
			{
				if (config?.UseOnOrBeforeDateWhenWatermarkReset ?? false)
				{
					schedule.SetWatermark(stageName, new ArchiveWatermark { WatermarkDate = config.ArchiveJobsOnOrBeforeThisDate, WatermarkNK = string.Empty, WatermarkPK = Guid.Empty });
				}
				else
				{
					schedule.SetWatermark(stageName, null);
				}
			}
		}
	}
}
