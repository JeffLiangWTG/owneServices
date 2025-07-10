using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class SchematicTransferLoopMonitor : BMServiceTaskProcessor
	{
		public SchematicTransferLoopMonitor(ILogger logger)
			: base(logger, new ServiceTaskFactoryProviderWrapper(logger, SchematicTransferLoopMonitorServiceTask.Code))
		{
		}

		#region ProcessHeaderProcessor Overrides

		protected override bool IsSufficientWorkflowManagementModeEnabled => BMSRegistryProvider.IsEnhancedWorkflowManagementOrBetterEnabled;

		public override void ProcessCore(CancellationToken token)
		{
			var depth = BMSRegistry.Instance.WorkflowLoopingDetectionDepth.Value;
			var loopThreshold = BMSRegistry.Instance.WorkflowLoopingDetectionLimit.Value;

			var endTime = ZDateTime.UtcNow;
			var startTime = endTime.AddMinutes(-depth);

			var factory = FactoryProvider.Current;

			var loopedWorkflows = GetLoopedWorkflows(startTime, endTime, loopThreshold);
			var workflowsToDeactivate = loopedWorkflows.Select(pk => factory.Load<ProcessHeader>(pk)).Where(w => w != null && w.FH_IsActive).ToArray();

			if (!workflowsToDeactivate.Any())
			{
				Logger.Log(LogType.Debug, "No transfer loops found."); // Error messages for logging and reporting should be in English
				return;
			}

			var emailBuilder = new ZStringBuilder();
			var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} transfer loop(s) found:", workflowsToDeactivate.Length); // Error messages for logging and reporting should be in English
			Logger.Log(LogType.Information, message);
			emailBuilder.Append(message);

			using (new BMSServiceTaskHelper().GetTemporaryEnvironmentForServiceTaskBranch())
			{
				foreach (var workflow in workflowsToDeactivate)
				{
					token.ThrowIfCancellationRequested();
					workflow.FH_IsActive = false;
					message = string.Format(CultureInfo.InvariantCulture,
(NoResString)@"The following workflow has been deactivated since it has completed a loop. Please address the issue and reactivate the workflow:
Code = {0}, Description = {1}, Job = {2}", workflow.Code, workflow.Description, workflow.ParentJobDescription); // Error messages for logging and reporting should be in English
					Logger.Log(LogType.Warning, message);
					emailBuilder.Append(message);
				}

				var description = Res.GetString("A8A7AFE5-89F5-4F9B-9EF3-3781E559FDC7", "Workflow transfer loop detected");
				var emailDef = new BMSEmailDef(description, emailBuilder.ToStringWithNewLineBetweenAppends());

				factory.Save();
				emailDef.Send();
			}
		}

		#endregion
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]

#if DEBUG
		public static
#endif
		IEnumerable<ZGuid> GetLoopedWorkflows(ZDateTime startTime, ZDateTime endTime, int loopThreshold)
		{
			var loopedWorkflows = new List<ZGuid>();
			var sql = string.Format(CultureInfo.InvariantCulture, "SELECT WorkflowPK FROM dbo.GetLoopedWorkflows('{0}', '{1}', {2})", // SQL command
								startTime.SqlFormat, endTime.SqlFormat, loopThreshold);
			var command = Db.Connection.Command(sql); // Direct SQL is required to increase performance
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var workflowPK = new ZGuid(reader[0]);
					loopedWorkflows.Add(workflowPK);
				}
			}
			return loopedWorkflows;
		}
	}
}
