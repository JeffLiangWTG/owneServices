using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Enterprise.DbUpgrader.Shared
{
	public class UpgradeActionList : IUpgradeAction, IEnumerable<IUpgradeAction>
	{
		public UpgradeActionList(string name = null)
		{
			Name = name;
			list = new SortedList<int, IUpgradeAction>();
		}

		public void Run()
		{
			foreach (var action in this)
			{
				action.Run();
			}
		}

		public int Count
		{
			get { return list.Count; }
		}

		public string Name { get; set; }

		public bool RequiresApplicationLockout
		{
			get
			{
				if (requiresApplicationLockout.HasValue)
				{
					return requiresApplicationLockout.Value;
				}

				foreach (var action in this)
				{
					if (action.RequiresApplicationLockout)
					{
						return true;
					}
				}

				return false;
			}

			set
			{
				requiresApplicationLockout = value;
			}
		}
		bool? requiresApplicationLockout;

		public int EstimatedNumberOfTasks
		{
			get
			{
				if (estimatedNumberOfTasks.HasValue)
				{
					return estimatedNumberOfTasks.Value;
				}

				var taskCount = 0;
				foreach (var action in this)
				{
					taskCount += action.EstimatedNumberOfTasks;
				}

				return taskCount;
			}

			set
			{
				estimatedNumberOfTasks = value;
			}
		}
		int? estimatedNumberOfTasks;

		public IEnumerable<string> SecondaryDatabasesToUpgrade
		{
			get
			{
				if (secondaryDatabasesToUpgrade != null)
				{
					return secondaryDatabasesToUpgrade.ToArray();
				}

				var dbList = new List<string>();
				foreach (var action in this)
				{
					foreach (var dbName in action.SecondaryDatabasesToUpgrade)
					{
						if (!dbList.Contains(dbName))
						{
							dbList.Add(dbName);
						}
					}
				}

				return dbList;
			}

			set
			{
				secondaryDatabasesToUpgrade = (value != null) ? value.ToArray() : null;
			}
		}
		IEnumerable<string> secondaryDatabasesToUpgrade;

		#region Add

		public void Add(IUpgradeAction action)
		{
			AddToList(maxPriority + 1, action);
		}

		public void Add(Action action)
		{
			Add(null, action);
		}

		public void Add(string name, Action action)
		{
			AddToList(maxPriority + 1, new UpgradeAction(name, action));
		}

		/// <summary>
		/// Adds new action to the list
		/// </summary>
		/// <typeparam name="T">any Enum</typeparam>
		/// <param name="priority">priority of the action within the list</param>
		/// <param name="action">action to run</param>
		public void Add<T>(T priority, IUpgradeAction action)
			where T : IConvertible
		{
			AddToList(priority.ToInt32(CultureInfo.InvariantCulture), action);
		}

		public void Add(int priority, IUpgradeAction action)
		{
			AddToList(priority, action);
		}

		public void Add(int priority, Action action)
		{
			AddToList(priority, new UpgradeAction(action));
		}

		/// <summary>
		/// Adds new action to the list
		/// </summary>
		/// <typeparam name="T">any Enum</typeparam>
		/// <param name="priority">priority of the action within the list</param>
		/// <param name="action">action to run</param>
		public void Add<T>(T priority, string name, Action action)
			where T : IConvertible
		{
			AddToList(priority.ToInt32(CultureInfo.InvariantCulture), new UpgradeAction(name, action));
		}

		void AddToList(int priority, IUpgradeAction action)
		{
			IUpgradeAction existingAction;
			if (list.TryGetValue(priority, out existingAction))
			{
				var existingList = existingAction as UpgradeActionList;
				if (existingList == null)
				{
					existingList = new UpgradeActionList(existingAction.Name);
					existingList.Add(existingAction);
					list.Remove(priority);
					list.Add(priority, existingList);
				}

				if (string.Equals(action.Name, existingAction.Name))
				{
					action.Name = null;
				}

				existingList.Add(action);
			}
			else
			{
				list.Add(priority, action);
				UpdateMaxPriority(priority);
			}
		}

		#endregion // Add

		#region Implementation

		readonly SortedList<int, IUpgradeAction> list;
		int maxPriority;

		void UpdateMaxPriority(int priority)
		{
			if (priority > maxPriority)
			{
				maxPriority = priority;
			}
		}

		#region IEnumerable<IUpgradeAction> Interface

		public IEnumerator<IUpgradeAction> GetEnumerator()
		{
			return list.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)list).GetEnumerator();
		}

		#endregion // IEnumerable<IUpgradeAction> Interface

		#endregion // Implementation
	}
}
