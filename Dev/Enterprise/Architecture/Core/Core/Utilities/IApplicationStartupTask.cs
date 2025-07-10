using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	/// <summary>
	/// A task to run during application startup
	/// </summary>
	public interface IApplicationStartupTask : IApplicationStartupTaskWithFailureExitCode
	{
		/// <summary>
		/// Description to be displayed to the user
		/// </summary>
		string TaskDescription { get; }

		/// <summary>
		/// Check if Execute should be called. Use to avoid loading uneeded dlls if nothing needs to be done.
		/// </summary>
		bool ShouldExecute(CommandLineArguments arguments);

		/// <summary>
		/// Execute the task, return false if startup should stop and the application exit, true otherwise
		/// </summary>
		bool Execute(CommandLineArguments arguments);
	}

	/// <summary>
	/// Long running tasks that can send progress feedback to the user should implement this interface and raise the Progress event appropriately.
	/// </summary>
	public interface IApplicationStartupTaskProgress
	{
		event Progress Progress;
	}

	/// <summary>
	/// A task that should run even if a prior task fails
	/// </summary>
	public interface IApplicationStartupTaskExecuteOnFailure
	{
		void ExecuteOnFailure(CommandLineArguments arguments);
	}

	/// <summary>
	/// A special task that provides an exception handler to use on any exceptions raised in subsequent tasks.
	/// </summary>
	public interface IApplicationStartupTaskExceptionHandler
	{
		bool HandleException(Exception e, CommandLineArguments arguments);
	}

	/// <summary>
	/// A task that has specific program exit code on failure
	/// </summary>
	public interface IApplicationStartupTaskWithFailureExitCode
	{
		int FailureExitCode { get; }
	}

	public delegate void Progress(string status, int percentComplete);

	public abstract partial class AbstractApplicationStartupTask : IApplicationStartupTask
	{
		public abstract string TaskDescription { get; }

		public abstract int FailureExitCode { get; }

		#region Should Execute

		public bool ShouldExecute(CommandLineArguments arguments)
		{
#if DEBUG
			bool? result = false;
			ShouldExecute_ForTest(arguments, this, ref result);

			if (result != null)
			{
				return result.Value;
			}
#endif
			return GetShouldExecute(arguments);
		}

		protected virtual bool GetShouldExecute(CommandLineArguments arguments)
		{
			return true;
		}

		partial void ShouldExecute_ForTest(CommandLineArguments arguments, IApplicationStartupTask task, ref bool? result);

		#endregion

		#region Execute

		public bool Execute(CommandLineArguments arguments)
		{
#if DEBUG
			bool? result = false;
			DoExecute_ForTest(arguments, this, ref result);

			if (result != null)
			{
				return result.Value;
			}
#endif
			return DoExecute(arguments);
		}

		protected abstract bool DoExecute(CommandLineArguments arguments);

		partial void DoExecute_ForTest(CommandLineArguments arguments, IApplicationStartupTask task, ref bool? result);

		#endregion
	}

	public abstract class InitializingApplicationStartupTask : AbstractApplicationStartupTask
	{
	}

	public abstract class BackgroundApplicationStartupTask : AbstractApplicationStartupTask
	{
		public override string TaskDescription => Res.GetString("8c6247f3-251a-485b-8c7f-fbc22eb19945", "Starting Background Task");

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Top level exception handler")]
		protected override bool DoExecute(CommandLineArguments arguments)
		{
			Task = Task.Run(() =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						DoExecute();
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ExceptionReporter.Instance.HandleOrReport(ex);
				}
			});

			allTasks.Add(Task);

			return true;
		}

		public Task Task
		{
			get;
			private set;
		}

		public abstract void DoExecute();

		static readonly ConcurrentBag<Task> allTasks = new ConcurrentBag<Task>();

		public static void RegisterAdditionalTask(Task task)
		{
			allTasks.Add(task);
		}

		public static void WaitAll()
		{
			Task.WaitAll(allTasks.ToArray());
		}
	}

	public interface IPostLoginTask
	{
		string TaskDescription { get; }
		bool ShouldExecute();
		void Execute();
	}
}

#if DEBUG

#region Partial Class

namespace Enterprise.ZArchitecture.Core
{
	public abstract partial class AbstractApplicationStartupTask
	{
		partial void ShouldExecute_ForTest(CommandLineArguments arguments, IApplicationStartupTask task, ref bool? result)
		{
			result = shouldExecuteForTest?.Invoke(arguments, task);
		}

		partial void DoExecute_ForTest(CommandLineArguments arguments, IApplicationStartupTask task, ref bool? result)
		{
			result = executeForTest?.Invoke(arguments, task);
		}

		public static IDisposable SetupShouldExecute_ForTest(Func<CommandLineArguments, IApplicationStartupTask, bool?> doExecuteForTest)
		{
			shouldExecuteForTest = doExecuteForTest;
			return new DisposableAction(() => shouldExecuteForTest = null);
		}

		public static IDisposable SetupDoExecute_ForTest(Func<CommandLineArguments, IApplicationStartupTask, bool?> doExecuteForTest)
		{
			executeForTest = doExecuteForTest;
			return new DisposableAction(() => executeForTest = null);
		}

		[SuppressMessage("CargoWiseOne", "CW1021")]
		static Func<CommandLineArguments, IApplicationStartupTask, bool?> executeForTest;
		[SuppressMessage("CargoWiseOne", "CW1021")]
		static Func<CommandLineArguments, IApplicationStartupTask, bool?> shouldExecuteForTest;
	}
}

#endregion
#endif
