using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Host
{
	interface ITaskInitializationRequirementsChecker
	{
		IEnumerable<ScheduleWithInfo> EnsureTaskRequirementsSatisfied(IEnumerable<ScheduleWithInfo> allTasks, IServiceTaskCollectionGovernor tasksGovernor);
	}

	class TaskInitializationRequirementsChecker : ITaskInitializationRequirementsChecker
	{
		public TaskInitializationRequirementsChecker(IHostLogger hostLogger, IGlbCompanyProvider companyProvider)
		{
			this.hostLogger = hostLogger;
			this.companyProvider = companyProvider;
		}

		public IEnumerable<ScheduleWithInfo> EnsureTaskRequirementsSatisfied(IEnumerable<ScheduleWithInfo> allTasks, IServiceTaskCollectionGovernor tasksGovernor)
		{
			var tasksByBranch = allTasks.GroupBy(info =>
				info.Task != null && info.Task.BranchPk != Guid.Empty
					? info.Task.BranchPk
					: Guid.Empty);

			var tasksThatSatisfyRequirements = new List<ScheduleWithInfo>();
			var activeCompanies = companyProvider.GetActiveCompanies().ToList();

			foreach (var tasksWithSameBranch in tasksByBranch)
			{
				tasksThatSatisfyRequirements.AddRange(
					tasksWithSameBranch.Key != Guid.Empty
						? EnsureTasksWithBranchSatisfyRequirements(tasksWithSameBranch.Key, tasksWithSameBranch, activeCompanies, tasksGovernor)
						: EnsureTasksWithoutBranchSatisfyRequirements(tasksWithSameBranch, activeCompanies, tasksGovernor));
			}

			return tasksThatSatisfyRequirements;
		}

		List<ScheduleWithInfo> EnsureTasksWithBranchSatisfyRequirements(ZGuid branch, IEnumerable<ScheduleWithInfo> tasksWithSameBranch, ICollection<IGlbCompany> activeCompanies, IServiceTaskCollectionGovernor tasksGovernor)
		{
			var tasksThatSatisfyRequirements = new List<ScheduleWithInfo>();
			var tasksToUpdateBranch = new List<Guid>();
			var tasksToDeactivate = new List<Guid>();

			using (DisposableEnvironment.ForBranch(branch.ToGuid()))
			{
				foreach (var taskInfo in tasksWithSameBranch)
				{
					var failureDescriptions = taskInfo.Info.CheckSatisfiesRequirementsForCurrentBranch(activeCompanies);
					if (failureDescriptions.Length == 0)
					{
						if (taskInfo.Info.HostedServiceAttribute.CanRunInAnyBranch)
						{
							hostLogger.Log(
								LogLevel.Information,
								Invariant($"Service task: {taskInfo.Info.Code} - '{taskInfo.Info.Description}' had non null branch assignment. Branch assignment was set to null as the task can run in any branch"));
							tasksToUpdateBranch.Add(taskInfo.Task.Pk);
						}
						tasksThatSatisfyRequirements.Add(taskInfo);
					}
					else
					{
						if (taskInfo.Info.Code.Equals("DSA", StringComparison.OrdinalIgnoreCase))
						{
							throw new InvalidOperationException("Unable to proceed with execution as the mandatory 'Database Security Admin Task' (DSA) did not satisfy the requirements");
						}

						hostLogger.Log(
							LogLevel.Warning,
							Invariant($"Task {taskInfo.Info.Code} will be deactivated. The following configuration errors need to be resolved for the task to be re-activated on the next process controller restart:\r\n\t{string.Join("\r\n\t", failureDescriptions)}"));
						tasksToDeactivate.Add(taskInfo.Task.Pk);
					}
				}

				if (tasksToUpdateBranch.Count > 0)
				{
					tasksGovernor.SetBranchPk(Guid.Empty, tasksToUpdateBranch);
				}

				if (tasksToDeactivate.Count > 0)
				{
					tasksGovernor.SetActive(false, tasksToDeactivate);
				}
			}

			return tasksThatSatisfyRequirements;
		}

		List<ScheduleWithInfo> EnsureTasksWithoutBranchSatisfyRequirements(IEnumerable<ScheduleWithInfo> tasksWithoutBranch, ICollection<IGlbCompany> activeCompanies, IServiceTaskCollectionGovernor tasksGovernor)
		{
			var tasksThatSatisfyRequirements = new List<ScheduleWithInfo>();
			var tasksStillWithoutBranch = tasksWithoutBranch.ToList();
			var tasksToUpdateBranch = new List<Guid>();
			var countNotFindActiveBranch = true;
			foreach (var company in activeCompanies)
			{
				foreach (var branch in company.GetActiveBranches())
				{
					countNotFindActiveBranch = false;

					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						foreach (var taskTuple in tasksStillWithoutBranch.ToArray())
						{
							var failureDescriptions = taskTuple.Info.CheckSatisfiesRequirementsForCurrentBranch(activeCompanies);
							if (failureDescriptions.Length == 0)
							{
								tasksThatSatisfyRequirements.Add(taskTuple);
								tasksStillWithoutBranch.Remove(taskTuple);

								if (taskTuple.Task == null)
								{
									hostLogger.Log(
										LogLevel.Debug,
										Invariant($"Service task: {taskTuple.Info.Code} - '{taskTuple.Info.Description}' has been added to the system"));
								}
								else
								{
									if (!taskTuple.Info.HostedServiceAttribute.CanRunInAnyBranch)
									{
										hostLogger.Log(
											LogLevel.Information,
											Invariant($"Service task: {taskTuple.Info.Code} - '{taskTuple.Info.Description}' had missing branch assignment and requires a branch. Branch assignment was set to {branch.GB_BranchName} as the branch satisfies task requirements"));
										tasksToUpdateBranch.Add(taskTuple.Task.Pk);
									}
								}
							}
						}

						if (tasksToUpdateBranch.Count > 0)
						{
							tasksGovernor.SetBranchPk(branch.PK.ToGuid(), tasksToUpdateBranch);
							tasksToUpdateBranch.Clear();
						}
					}
					if (tasksStillWithoutBranch.Count == 0)
					{
						break;
					}
				}
				if (tasksStillWithoutBranch.Count == 0)
				{
					break;
				}
			}

			if (countNotFindActiveBranch)
			{
				throw new ProcessControllerConfigurationException("Unable to proceed with execution as there are no active companies that contain an active branch.");
			}

			foreach (var taskTuple in tasksStillWithoutBranch)
			{
				if (taskTuple.Info.Code.Equals("DSA", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidOperationException("Unable to proceed with execution as the mandatory 'Database Security Admin Task' (DSA) did not satisfy the requirements");
				}

				hostLogger.Log(
					LogLevel.Warning,
					Invariant($"The Task {taskTuple.Info.Code} cannot be added to the system until the following configuration errors have been addressed and the process controller is restarted:\r\n\t{string.Join("\r\n\t", taskTuple.Info.BestResultsFromRequirementsCheck)}"));
			}

			return tasksThatSatisfyRequirements;
		}

		readonly IHostLogger hostLogger;
		readonly IGlbCompanyProvider companyProvider;
	}
}
