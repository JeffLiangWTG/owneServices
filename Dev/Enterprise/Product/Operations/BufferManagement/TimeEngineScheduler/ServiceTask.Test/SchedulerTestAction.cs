using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.TimeEngineScheduler.ServiceTask.Test;
using Enterprise.ZArchitecture.Environment;

[assembly: SchedulerAction(
	SchedulerTestAction.Code,
	SchedulerTestAction.Description,
	typeof(SchedulerTestAction))]
namespace Enterprise.TimeEngineScheduler.ServiceTask.Test
{
	class SchedulerTestAction : ISchedulerAction
	{
		public const string Code = "TST";
		public const string Description = "Test action";

		public bool WasExecuted { get; private set; }
		internal List<ActionExecution> ExecutionHistory { get; }
		public string LastUsedExecutionBranchCode { get; private set; }

		readonly bool shouldFail;

		public SchedulerTestAction()
			: this(false)
		{
		}

		public SchedulerTestAction(bool fail)
		{
			ExecutionHistory = new List<ActionExecution>();
			shouldFail = fail;
		}

		public string Execute(CancellationToken token, BusinessObjectFactory factory, ILogger logger, ZGuid targetPk, string targetCode, string parameter, ZDateTime systemCreateTimeUtc)
		{
			try
			{
				if (shouldFail)
				{
					throw new Exception("Just as planned");
				}

				WasExecuted = true;
				return Constants.SchedulerActionSuccessResult;
			}
			finally
			{
				var exection = new ActionExecution();
				exection.TargetPk = targetPk;
				exection.Parameter = parameter;
				exection.ExecutionBranchCode = EnvProxy.Instance.CurrentBranch.Code;
				exection.ExecutionDepartmentCode = EnvProxy.Instance.CurrentDepartment.Code;
				ExecutionHistory.Add(exection);
			}
		}

		internal class ActionExecution
		{
			public ZGuid TargetPk;
			public string Parameter;
			public string ExecutionBranchCode;
			public string ExecutionDepartmentCode;
		}
	}
}
