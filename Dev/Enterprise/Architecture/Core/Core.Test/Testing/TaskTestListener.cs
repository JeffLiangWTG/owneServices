using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class TaskTestListener : BaseTestListener
	{
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static readonly TaskTestListener Instance = new TaskTestListener();

		public TaskTestListener()
		{
			var taskType = typeof(Task);
			asyncDebuggingEnabledProperty = taskType.GetField("s_asyncDebuggingEnabled", BindingFlags.NonPublic | BindingFlags.Static);
			currentActiveTasksField = taskType.GetField("s_currentActiveTasks", BindingFlags.NonPublic | BindingFlags.Static);
			debuggerDisplayMethodDescriptionProperty = taskType.GetProperty("DebuggerDisplayMethodDescription", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public override void StartAllTests(DateTime startTime)
		{
			asyncDebuggingEnabledProperty.SetValue(null, true);
		}

		public override void AfterEachTest(DateTime endTime)
		{
			try
			{
				var activeTasksDictionary = (Dictionary<int, Task>)currentActiveTasksField.GetValue(null);
				var activeTasks = activeTasksDictionary?.Values.Where(x => x != null).ToArray() ?? Array.Empty<Task>();
				if (activeTasks.Length > 0)
				{
					activeTasks = activeTasks.Where(t => !leakedTasks.Contains(t.Id)).ToArray();
					if (activeTasks.Length > 0)
					{
						foreach (var task in activeTasks)
						{
							leakedTasks.Add(task.Id);
						}

						if (activeTasks.Length > expectedTasks)
						{
							// see https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/IO/Stream.cs
							var activeTasksToReport = activeTasks
								.Select(t => $"Id = {t.Id}, Status = {t.Status}, IsCanceled = {t.IsCanceled}, IsCompleted = {t.IsCompleted}, IsFaulted = {t.IsFaulted}, ToDebugString = {t.ToDebugString()}, Method = {GetDebuggerDisplayMethodDescription(t)}, Description = {GetTaskDescription(t.Id)}, Exception = {t.Exception}")
								.Where(x => !x.Contains("Method = Int32 <BeginReadInternal>")) // Ignore BeginReadInternal tasks that are from low level memory stream IO
								.ToArray();

							if (activeTasksToReport.Length > expectedTasks)
							{
								Assertion.Fail(
									"This test appears to have started a task without waiting for the result. Please ensure that all started tasks complete before the test finishes. Active tasks:"
									+ System.Environment.NewLine
									+ string.Join(System.Environment.NewLine, activeTasksToReport));
							}
						}
					}
				}
			}
			finally
			{
				TaskRegistryForTest.Reset();
				expectedTasks = 0;
			}
		}

		public void ExpectTask(string reason)
		{
			if (string.IsNullOrWhiteSpace(reason))
			{
				throw new ArgumentException("Please provide a valid reason for the test to leak a task", nameof(reason));
			}
			expectedTasks++;
		}

		string GetTaskDescription(int id)
		{
			return TaskRegistryForTest.TryGetTaskDescriptionById(id, out string description) ? description : $"The task with the provided ID was not registered by {nameof(TaskRegistryForTest)}. Try using {nameof(AsyncHelper)} for creating tasks.";
		}

		string GetDebuggerDisplayMethodDescription(Task task)
		{
			return (string)debuggerDisplayMethodDescriptionProperty.GetValue(task, Array.Empty<object>());
		}

		readonly FieldInfo asyncDebuggingEnabledProperty;
		readonly FieldInfo currentActiveTasksField;
		readonly PropertyInfo debuggerDisplayMethodDescriptionProperty;
		readonly HashSet<int> leakedTasks = new HashSet<int>();
		int expectedTasks;
	}
}
