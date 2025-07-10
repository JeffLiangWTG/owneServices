using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation
{
	public class OnlineTransformationProvider : IOnlineTransformationProvider
	{
		IEnumerable<IOnlineTransformation> allTasks;

		OnlineTransformationTaskStatus _status;

		public OnlineTransformationProvider()
		{
		}

		OnlineTransformationTaskStatus Status
		{
			get => _status ?? (_status = ReadTaskStatus());
			set
			{
				_status = value;
				SaveTaskStatus(_status);
			}
		}

		public IEnumerable<IOnlineTransformation> AllTasks
		{
			get
			{
				if (allTasks == null)
				{
					allTasks = ObjectFactory.Get<ITransformationMappingProvider>().GetAllMappings()
						.Where(mapping => mapping.TransformationType.GetMethod("OnlinePostUpgradeTransform", BindingFlags.Instance | BindingFlags.NonPublic).DeclaringType != typeof(DataTransformation))
						.Select(mapping => (IOnlineTransformation)Activator.CreateInstance(mapping.TransformationType))
						.ToArray();
				}

				return allTasks;
			}
		}

		public IEnumerable<IOnlineTransformation> GetRunningTasks()
		{
			ModifyStatusIfNeeded();

			return AllTasks
				.Where(task => Status.Pending.Any(taskName => AreEqual(taskName, task)));
		}

		public void OnTaskCompleted(IOnlineTransformation task)
		{
			var completed = new[] { task.FullName() };
			Status = new OnlineTransformationTaskStatus(Status.Completed.Concat(completed), Status.Pending.Except(completed));
		}

		public IEnumerable<string> DeletedPendingTasks
		{
			get
			{
				return Status.Pending
					.Where(taskName => !AllTasks.Any(task => AreEqual(taskName, task)));
			}
		}

		internal OnlineTransformationTaskStatus ReadTaskStatus()
		{
			var serializedStatus = DataRegistry.Instance.OnlineTransformationStatus;
			if (!string.IsNullOrEmpty(serializedStatus))
			{
				return JsonConvert.DeserializeObject<OnlineTransformationTaskStatus>(serializedStatus);
			}

			return new OnlineTransformationTaskStatus(
				completed: Enumerable.Empty<string>(),
				pending: Enumerable.Empty<string>());
		}

		internal void SaveTaskStatus(OnlineTransformationTaskStatus status)
		{
			var serialized = JsonConvert.SerializeObject(status, Formatting.None);
			DataRegistry.Instance.OnlineTransformationStatus = serialized;
		}

		void ModifyStatusIfNeeded()
		{
			var newTasks = AllTasks
				.Where(task => !Status.Completed.Union(Status.Pending).Any(taskName => AreEqual(taskName, task)))
				.Select(t => t.FullName())
				.ToArray();

			var unavailableTasks = Status.Completed.Union(Status.Pending)
				.Where(taskName => !AllTasks.Any(task => AreEqual(taskName, task)))
				.ToArray();

			if (newTasks.Length > 0 || unavailableTasks.Length > 0)
			{
				Status = new OnlineTransformationTaskStatus(
					completed: Status.Completed.Except(unavailableTasks),
					pending: Status.Pending.Union(newTasks).Except(unavailableTasks));
			}
		}

		bool AreEqual(string taskName, IOnlineTransformation task)
		{
			return string.Equals(taskName, task.FullName(), StringComparison.Ordinal);
		}
	}

	static class IOnlineTransformationExtensions
	{
		public static string FullName(this IOnlineTransformation task) => task.GetType().FullName;
	}
}
