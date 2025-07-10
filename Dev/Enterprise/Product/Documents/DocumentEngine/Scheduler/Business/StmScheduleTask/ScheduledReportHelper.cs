using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class ScheduledReportHelper : IScheduledReportHelper
	{
		public List<string> GetScheduleReportsDescriptionAssignedToUser(string userCode, bool mustBeActive, int maxCount = 0)
		{
			var query = new ZDBOnlyQuery(typeof(StmScheduleTask));
			if (mustBeActive)
			{
				query.AddToFilter(StmScheduleTaskSchema.S5_IsActive, true);
			}
			query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmMenuItemSchema.Constants.Prefix);
			query.AddToFilter(StmScheduleTaskSchema.S5_ScheduleType, ReportScheduleTask.ScheduleType);
			var optionScheduleTask = new ZDBOnlyQuery(typeof(StmScheduleTask));
			optionScheduleTask.AddToFilter(JoinCondition.Or, StmScheduleTaskSchema.S5_GS_NKPrintUser, userCode);
			var optionScheduleTaskRecipient = new ZDBOnlySubQuery(typeof(StmScheduleTaskRecipient), StmScheduleTaskRecipientSchema.S6_S5);
			optionScheduleTaskRecipient.AddToFilter(StmScheduleTaskRecipientSchema.S6_GS_NKRecipient, userCode);
			optionScheduleTask.AddSubQuery(optionScheduleTaskRecipient, JoinCondition.Or);
			query.AddToFilter(optionScheduleTask);
			var scheduleTasks = Factory.Load<StmScheduleTask>(query);

			return maxCount > 0 ? scheduleTasks.Take(maxCount).Select(s => s.S5_ScheduleDescription.ToString()).ToList() : scheduleTasks.Select(s => s.S5_ScheduleDescription.ToString()).ToList();
		}

		internal static ReportSerializationInfo DeserializeStreamToReportSerializationInfo_JsonFormat(ZBlob stream)
		{
			ReportSerializationInfo result = null;
			if (stream.Length > 0)
			{
				using (var memoryStream = new MemoryStream(stream))
				using (var reader = new StreamReader(memoryStream))
				{
					var json = reader.ReadToEnd();
					result = JsonConverterHelper.Deserialize<ReportSerializationInfo>(json);
				}
			}
			return result;
		}

		internal static ReportSerializationInfo DeserializeStreamToReportSerializationInfo(ZBlob stream)
		{
			return DeserializeStreamToReportSerializationInfo_JsonFormat(stream);
		}

		public string GetPrintUserSafe(ZBlob stream)
		{
			try
			{
				return DeserializeStreamToReportSerializationInfo(stream)?.User;
			}
			catch (JsonException)
			{
				return null;
			}
		}

		public byte[] SerializeOrgCodeInScheduledReportsByPK(ZBlob scheduleReport, ZGuid parentID, List<string> filterDisplayNameList)
		{
			Argument.NotNull(scheduleReport, nameof(scheduleReport));
			Argument.NotNull(filterDisplayNameList, nameof(filterDisplayNameList));

			var needModifications = false;

			var reportCommand = Factory.Load<ReportCommand>(parentID);
			if (reportCommand == null)
			{
				return null;
			}

			using (var pack = new DocumentPack(reportCommand))
			using (var report = (Report)pack[0])
			{
				ReportSerializationInfo reportSerializationInfo = null;
				
				reportSerializationInfo = DeserializeStreamToReportSerializationInfo(scheduleReport);

				if (reportSerializationInfo == null || reportSerializationInfo.Report == null)
				{
					return null;
				}

				report.ColumnHeadingManager.UpdateFromDeserialisedValue(reportSerializationInfo.Report.ColumnHeadingManager, report);

				foreach (var filterDisplayName in filterDisplayNameList)
				{
					var filter = reportSerializationInfo.Report.FilterCollection[filterDisplayName];
					if (!(filter is MultipleSelectionLookup orgFilter) || orgFilter.SerialisedByPK)
					{
						continue;
					}

					needModifications = true;
					orgFilter.SerialisedByPK = true;
				}

				if (!needModifications)
				{
					return null;
				}

				var json = JsonConverterHelper.Serialize(reportSerializationInfo);
				return Encoding.UTF8.GetBytes(json);
			}
		}

		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory();
				}
				return factory;
			}
		}
		BusinessObjectFactory factory;
	}
}
