using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Integration;

[assembly: SchedulerAction(
		TransferRuleSchedulerAction.Code,
		TransferRuleSchedulerAction.Description,
		typeof(TransferRuleSchedulerAction))]
namespace Enterprise.BufferManagement.Business
{
	public class TransferRuleSchedulerAction : ISchedulerAction
	{
		public const string Code = "TRS";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used for service task logging")]
		public const string Description = "Transfer Rule";

		public string Execute(CancellationToken token, BusinessObjectFactory factory, ILogger logger, ZGuid targetPk, string targetCode, string parameter, ZDateTime systemCreateTimeUtc)
		{
			var schematicService = ObjectFactory.Get<ISchematicService>();

			var workflowPKs = string.IsNullOrWhiteSpace(parameter) ? new[] { targetPk.ToGuid() } : parameter.JsonDeserialize<Guid[]>();

			schematicService.ProcessTransferRules(workflowPKs, logger);

			return Constants.SchedulerActionSuccessResult;
		}
	}
}
