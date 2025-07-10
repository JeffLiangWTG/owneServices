using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ServiceManager.Shared.Interfaces;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.NudgingClient.Abstractions;
using WTG.StaticAnalysis.Annotation;

namespace ServiceManager.Shared.CW
{
	[ThreadSafe]
	class BusinessObjectToServiceTaskMapper : IBusinessObjectToServiceTaskMapper
	{
		public static BusinessObjectToServiceTaskMapper Instance { get { return lazyInstance.Value; } }
		static readonly Lazy<BusinessObjectToServiceTaskMapper> lazyInstance = new (() => new BusinessObjectToServiceTaskMapper());

		BusinessObjectToServiceTaskMapper()
		{
		}

		public IDictionary<string, ICollection<string>> MapBusinessObjectsToServiceTasks(INudgingController nudgingController, IEnumerable<BusinessObject> businessObjects)
		{
			var taskToBizOsMappingResult = new Dictionary<string, ICollection<string>>();
			var modifiedBizos = businessObjects.Where(biz => !biz.IsDeleted);
			if (!modifiedBizos.Any())
			{
				nudgingController.ReportNudgeIgnored(null, "No business objects were added or modified");
				return taskToBizOsMappingResult;
			}

			IEnumerable<IServiceTaskBinding> serviceTaskBindings = null;
			var tables = new Dictionary<string, string>();
			var tasks = new Dictionary<string, string>();
			foreach (var bizO in modifiedBizos)
			{
				var obj = bizO; // prevent closure in the next statement (prior C# 5.0)
				serviceTaskBindings = serviceTaskBindings ?? nudgingController.ServiceTaskBindings;
				var matchingTaskBindings = serviceTaskBindings.Where(attrib => attrib.TableName == obj.TableName);
				tables[obj.TableName] = obj.TableName;

				foreach (var taskBinding in matchingTaskBindings)
				{
					tasks[taskBinding.ServiceTaskCode] = taskBinding.ServiceTaskCode;
					var match = taskBinding.Predicates.All(columnPredicate => columnPredicate.Matches(bizO[columnPredicate.ColumnName]));
					if (match)
					{
						if (taskToBizOsMappingResult.ContainsKey(taskBinding.ServiceTaskCode))
						{
							var task = taskToBizOsMappingResult[taskBinding.ServiceTaskCode];
							task.Add(bizO.HumanReadableName);
						}
						else
						{
							taskToBizOsMappingResult.Add(taskBinding.ServiceTaskCode, new HashSet<string> { bizO.HumanReadableName });
						}
					}
				}
			}

			if (!taskToBizOsMappingResult.Any())
			{
				if (tasks.Any())
				{
					var tasksAsString = string.Join(",", tasks.Keys);
					nudgingController.ReportNudgeIgnored(null, string.Format(CultureInfo.InvariantCulture, "No records matched the predicates for task(s)='{0}'", tasksAsString));
				}
				else
				{
					var tablesAsString = string.Join(",", tables.Keys);
					nudgingController.ReportNudgeIgnored(null, string.Format(CultureInfo.InvariantCulture, "No mapping found for table(s)='{0}'", tablesAsString));
				}
			}

			return taskToBizOsMappingResult;
		}
	}
}
