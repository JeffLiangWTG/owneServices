using System;
using CargoWise.Common;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class CapabilityTaskAutoAssignmentServiceTaskWithTimeAdvancingLoggerDataAccessor : WorkflowCapabilityAssignerDataAccessor
	{
		public CapabilityTaskAutoAssignmentServiceTaskWithTimeAdvancingLoggerDataAccessor(ILogger logger)
			: base(logger)
		{
		}

		protected override BatchLogger GetWorkflowBatchLogger(string workflowBatchNameForLogging) => new TimeAdvancingLogger(Logger, workflowBatchNameForLogging);
	}

	class TimeAdvancingLogger : BatchLogger
	{
		public TimeAdvancingLogger(ILogger logger, string batchName)
			: base(logger, batchName)
		{
		}

		public override IDisposable BatchLoadStarting()
		{
			var disposable = base.BatchLoadStarting();

			return new DisposableAction(() =>
			{
				TestDateAttribute.AddMinutes(1);

				disposable.Dispose();
			});
		}
	}
}
