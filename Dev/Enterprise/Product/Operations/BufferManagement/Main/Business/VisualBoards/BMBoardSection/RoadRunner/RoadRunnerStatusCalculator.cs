using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.CalendarArithmetic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class RoadRunnerStatusCalculator
	{
		public static Dictionary<ZString, RoadRunnerDetails> GetResourceCodeAndRoadRunnerDetailsDictionary(GlbStaff[] staffArray, ZGuid componentPK, PropertyCache propertyCache, BusinessObjectFactory factory, RoadRunnerTaskCache cache = null)
		{
			Argument.NotNull(staffArray, nameof(staffArray));

			cache ??= new RoadRunnerTaskCache(componentPK, staffArray);

			var resourceRoadRunnerDetailsDtoList = new List<RoadRunnerDetailsDto>();
			var resourceRoadRunnerDetailsList = new Dictionary<ZString, RoadRunnerDetails>();
			var component = factory.Load<BMComponent>(componentPK);

			foreach (var staff in staffArray)
			{
				var dto = new RoadRunnerDetailsDto();
				ProcessTask task = null;
				ProcessTask workingTask = null;

				var workTimeContext = WorkingTimeContext.Create(component, staff, factory);
				var workTimeArithmetic = workTimeContext.GetWorkTimeArithmetic(factory, checkHomeBranchAndDepartment: true);

				var currentLocalTime = workTimeContext.GetCurrentLocalTime(factory, checkHomeBranchAndDepartment: true);
				var isWorkingRightNow = staff.IsWorking(currentLocalTime, workTimeArithmetic);

				var awayUntil = new ZDateTime();

				if (!staff.GS_IsActive)
				{
					dto.Status = RoadRunnerStatus.None;
				}
				else
				{
					if (isWorkingRightNow && (IsWorkingInAnyBuffer(staff, cache, out task) || IsWorkingInThisComponent(staff, cache, componentPK, out task)))
					{
						dto.Status = RoadRunnerStatus.FullSpeed;
					}
					else
					{
						if (!isWorkingRightNow)
						{
							awayUntil = AwayUntil(staff, workTimeArithmetic, currentLocalTime);
						}
						if (awayUntil.IsValid)
						{
							dto.Status = RoadRunnerStatus.Away;
						}
						else if (HasNoWorkingTask(staff, cache, out workingTask))
						{
							dto.Status = RoadRunnerStatus.Stopped;
						}
						else if (HasStartableTask(staff, propertyCache))
						{
							task = workingTask;
							dto.Status = RoadRunnerStatus.Alert;
						}
						else if (IsWorkingTaskAStandbyTask(workingTask))
						{
							task = workingTask;
							dto.Status = RoadRunnerStatus.WorkingOnStandbyTask;
						}
						else
						{
							task = workingTask;
							dto.Status = RoadRunnerStatus.WorkingInOtherComponent;
						}
					}
				}

				dto.AwayUntil = awayUntil;
				dto.RelevantActivityTime = GetRelevantActivityTime(staff, cache, dto.Status, out bool isOvertime);
				dto.IsOvertime = isOvertime;
				dto.RelevantTask = task;
				dto.ResourceCode = staff.GS_Code;

				resourceRoadRunnerDetailsDtoList.Add(dto);
			}

			factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, resourceRoadRunnerDetailsDtoList.Where(dto => dto.RelevantTask != null).Select(dto => dto.RelevantTask.P9_FH_ProcessHeader).Distinct()));

			foreach (var dto in resourceRoadRunnerDetailsDtoList)
			{
				if (dto.RelevantTask != null)
				{
					dto.RelevantTaskDescription = GetRoadRunnerTaskDescriptor(dto.RelevantTask, factory);
				}

				resourceRoadRunnerDetailsList.Add(dto.ResourceCode, dto.ConvertToRoadRunnerDetailsObject());
			}

			return resourceRoadRunnerDetailsList;
		}

		static ZDateTime AwayUntil(GlbStaff resource, IWorkTimeArithmetic workTimeArithmetic, ZDateTime currentLocalTime)
		{
			return GetFromServiceOrLoad(resource, "AwayUntil", () =>
			{
				var now = currentLocalTime.AddMilliseconds(-currentLocalTime.Millisecond);
				var awayUntil = resource.GetFirstAvailableDate(now, workTimeArithmetic);

				return awayUntil.IsValid && awayUntil != now ? awayUntil : new ZDateTime();
			});
		}

		static bool IsWorkingInAnyBuffer(GlbStaff resource, RoadRunnerTaskCache cache, out ProcessTask task)
		{
			task = GetFromServiceOrLoad(resource, "GetWorkingTaskInAnyBuffer", () => cache.GetWorkingTaskInAnyBuffer(resource));
			return task != null;
		}

		static bool IsWorkingInThisComponent(GlbStaff resource, RoadRunnerTaskCache cache, ZGuid componentPK, out ProcessTask task)
		{
			task = GetFromServiceOrLoad(resource, "IsWorkingInThisComponent" + componentPK.ToString(), () => cache.GetWorkingTaskInThisComponent(resource));
			return task != null;
		}

		static bool HasNoWorkingTask(GlbStaff resource, RoadRunnerTaskCache cache, out ProcessTask workingTask)
		{
			workingTask = GetFromServiceOrLoad(resource, "GetWorkingTask", () => cache.GetWorkingTask(resource));

			return workingTask == null;
		}

		static bool HasSuspendedTask(GlbStaff resource, RoadRunnerTaskCache cache, out ProcessTask suspendedTask)
		{
			suspendedTask = GetFromServiceOrLoad(resource, "GetSuspendedTask", () => cache.GetSuspendedTask(resource));

			return suspendedTask != null;
		}

		static bool HasClosedTask(GlbStaff resource, RoadRunnerTaskCache cache, out ProcessTask closedTask)
		{
			closedTask = GetFromServiceOrLoad(resource, "GetClosedTask", () => cache.GetClosedTask(resource));

			return closedTask != null;
		}

		static bool HasStartableTask(GlbStaff resource, PropertyCache propertyCache) => propertyCache.GetCachedValue<bool>(resource.PK, BMBoardSectionViewModel.CacheConstants.HasStartableTaskOnChannel);

		static bool IsWorkingTaskAStandbyTask(ProcessTask workingTask)
		{
			if (workingTask == null)
			{
				return false;
			}

			var workflow = workingTask.ProcessHeader as ProcessHeader;

			if (workflow == null)
			{
				return true;
			}

			return workflow.ApplicableIsStandby;
		}

		static TimeSpan GetRelevantActivityTime(GlbStaff resource, RoadRunnerTaskCache cache, RoadRunnerStatus status, out bool overtime)
		{
			var cachedValue = GetFromServiceOrLoad(resource, "GetRelevantActivityTime", () =>
			{
				bool isOvertime;
				var result = TimeSpan.Zero;
				ProcessTask task;

				switch (status)
				{
					case RoadRunnerStatus.FullSpeed:
					case RoadRunnerStatus.WorkingInOtherComponent:
						isOvertime = false;
						if (!HasNoWorkingTask(resource, cache, out task))
						{
							result += task.ElapsedDuration.ToTimeSpan();

							isOvertime = (task.HighEstimatedDuration.IsValid && task.HighEstimatedDuration.GetMinutesFromDateTimeSpan() < task.ElapsedDuration.GetMinutesFromDateTimeSpan())
								|| (task.P9_EstimatedHandoverTime.IsValid && task.P9_EstimatedHandoverTime.ToUtcZDateTime() < ZDateTime.UtcNow);
						}
						break;

					case RoadRunnerStatus.Stopped:
						isOvertime = false;

						if (BMSRegistry.Instance.ShowIdleTimeOnBoards.Value)
						{
							var maxTime = TimeSpan.Zero;

							void SetMaxTimeIfValid(ZDateTime time)
							{
								if (time.IsValid && TimeSpan.FromTicks(time.Ticks) > maxTime)
								{
									maxTime = TimeSpan.FromTicks(time.Ticks);
								}
							}

							if (HasSuspendedTask(resource, cache, out task))
							{
								SetMaxTimeIfValid(task.P9_SuspendedAtUtc);
							}

							if (HasClosedTask(resource, cache, out task))
							{
								SetMaxTimeIfValid(task.P9_CompletedTimeUtc);
							}

							if (maxTime != TimeSpan.Zero)
							{
								result = TimeSpan.FromTicks(ZDateTime.UtcNow.Ticks) - maxTime;
							}
						}

						break;

					default:
						isOvertime = false;
						break;
				}

				return Tuple.Create(result, isOvertime);
			});
			overtime = cachedValue.Item2;
			return cachedValue.Item1;
		}

		public static ZString GetRoadRunnerTaskDescriptor(ProcessTask task, BusinessObjectFactory factory)
		{
			var taskWorkflow = factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.PK, task.P9_FH_ProcessHeader));

			if (!taskWorkflow.IsNullOrEmpty())
			{
				var result = new StringBuilder();
				result.AppendFormat(CultureInfo.InvariantCulture, "{0}, {1}", taskWorkflow.First().CodeWithParentCode, task.P9_Description);

				var currentComponent = taskWorkflow.First().CurrentComponent;
				if (currentComponent != null)
				{
					result.AppendLine();
					result.Append(Res.GetString("4fa6dfdb-dc4b-42dd-947d-16537d3f9e3b", "In component: {0}", currentComponent.FC_Name));
				}

				return result.ToString();
			}
			else
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}: {1}", task.P9_TaskID, task.P9_Description);
			}
		}

		static T GetFromServiceOrLoad<T>(GlbStaff resource, string cacheKey, Func<T> loadFunc)
		{
			var service = resource.Factory.ServiceContainer.GetService<RoadRunnerStatusCacheService>();
			if (service == null)
			{
				return loadFunc();
			}
			return service.GetOrCacheValue(resource, cacheKey, loadFunc);
		}

		static ProcessTask GetFromServiceOrLoad(GlbStaff resource, string cacheKey, Func<ProcessTask> loadFunc)
		{
			var taskPK = GetFromServiceOrLoad(resource, cacheKey, () =>
			{
				var task = loadFunc();
				return task == null ? ZGuid.Empty : task.PK;
			});

			if (taskPK.IsValid)
			{
				return resource.Factory.Load<ProcessTask>(taskPK);
			}
			return null;
		}
	}
}
