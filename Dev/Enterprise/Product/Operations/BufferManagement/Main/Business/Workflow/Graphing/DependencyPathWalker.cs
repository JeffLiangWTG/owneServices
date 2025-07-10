using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	/// <summary>
	/// Calculates the total standard estimate hours of the longest dependency path finishing with the specified header,
	/// back to the earliest non-dependent ProcessHeader.
	/// </summary>
	public class DependencyPathWalker
	{
		public DependencyPathWalker(ICollection<ScheduleNode> endpointSchedules, bool includeClosedTaskHours)
		{
			CriticalPath = WalkReturningCriticalPath(endpointSchedules, includeClosedTaskHours);

			if (CriticalPath != null)
			{
				CriticalPathStandardEstimateHours = CriticalPath.Size;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal CriticalPathStandardEstimateHours { get; private set; }
		public DependencyPath CriticalPath { get; private set; }

		public bool IsOnCriticalPath(IIdentified entity)
		{
			return CriticalPath != null && CriticalPath.Schedules.Any(w => w.Identifier == entity.Identifier);
		}

		#region Implementation

		static DependencyPath WalkReturningCriticalPath(ICollection<ScheduleNode> endpointSchedules, bool includeClosedTaskHours)
		{
			DependencyPath lastPath = null;

			foreach (var schedule in endpointSchedules)
			{
				var path = WalkReturningCriticalPath(schedule, includeClosedTaskHours);

				lastPath = PickLongestPath(lastPath, path);
			}

			return lastPath;
		}

		static DependencyPath PickLongestPath(DependencyPath lastPath, DependencyPath path)
		{
			if (lastPath == null)
			{
				return path;
			}
			else if (lastPath.Size < path.Size)
			{
				return path;
			}
			else if (lastPath.Size == path.Size && path.Schedules.Count > lastPath.Schedules.Count)
			{
				return path; // If for some reason two paths have an identical lengh, pick the one with more entities, because that's the chain that's going to have the child entities.
			}
			else
			{
				return lastPath;
			}
		}

		static DependencyPath WalkReturningCriticalPath(ScheduleNode schedule, bool includeClosedTaskHours, DependencyPath path = null)
		{
			if (path == null)
			{
				path = new DependencyPath();
			}

			var currentSchedule = schedule;

			while (currentSchedule != null)
			{
				if (currentSchedule.IsScheduleApplicableToCriticalChainDuration)
				{
					path.Size += currentSchedule.EstimatedDurationHoursIncludingChildren;
				}
				path.Schedules.Add(currentSchedule);

				var prereqSchedules = currentSchedule.GetPrerequisites(PassDirection.None);

				var count = prereqSchedules.Take(2).Count();
				if (count == 0) // We reached the start of the path
				{
					break;
				}
				else if (count == 1) // We have a single path backwards - walk it
				{
					currentSchedule = prereqSchedules.First();
					continue;
				}
				else // We have multiple paths backwards - calculate each sub-path and pick the longest one, then we're done
				{
					var childWalker = new DependencyPathWalker(prereqSchedules.ToList(), includeClosedTaskHours);
					path.Size += childWalker.CriticalPathStandardEstimateHours;

					foreach (var workflow in childWalker.CriticalPath.Schedules)
					{
						path.Schedules.Add(workflow);
					}

					break;
				}
			}

			return path;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		static decimal GetStandardEstimateHours(ProcessHeader header, bool includeClosedTaskHours)
		{
			if (!header.FH_IsActive)
			{
				// Inactive headers should still be walked, but their task hours not accumulated
				return 0.0m;
			}
			else if (includeClosedTaskHours)
			{
				return header.FH_PlannedDurationInMinutes / 60.0m;
			}
			else
			{
				var result = header.Tasks.Where(t => t.IsOpen).Sum(t => t.StandardEstimateHours);

				// Add standard estimate of child headers
				result += header.ChildLinks.Sum(l => GetStandardEstimateHours(l.HeaderTo, includeClosedTaskHours));

				return result;
			}
		}

		#endregion
	}

	public class DependencyPath
	{
		internal DependencyPath()
		{
			Schedules = new Collection<ScheduleNode>();
		}

		public decimal Size { get; set; }
		public Collection<ScheduleNode> Schedules { get; private set; }
	}
}
