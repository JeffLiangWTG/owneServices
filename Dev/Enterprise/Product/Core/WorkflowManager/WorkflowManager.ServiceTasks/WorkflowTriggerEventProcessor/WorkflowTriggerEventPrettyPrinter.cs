using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.Shared;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	class WorkflowTriggerEventPrettyPrinter : LogSubscriber.PrettyPrinter
	{
		public WorkflowTriggerEventPrettyPrinter(string subscriberName) : base(subscriberName)
		{
		}

		public override string PrettyPrint(IQueuedLog log)
		{
			var trigger = log.LoadTrigger();
			if (trigger != null)
			{
				return string.Format(CultureInfo.InvariantCulture, (NoResString)"WTE Log (PK={0}, Evt={1}, Ref={2}) Parent ({3})", log.PK, trigger.TriggerEventCode, log.SJ_Reference, trigger.GetParentJobDetails());
			}
			else
			{
				return base.PrettyPrint(log);
			}
		}

		public override string PrettyPrintSubscriberErrorInfo(IQueuedLog log)
		{
			var trigger = log.LoadTrigger();
			if (trigger != null)
			{
				var messageBuilder = new ZStringBuilder();
				messageBuilder.AppendLine(
$@"Subscriber Name: {subsciberName}
Parent: {trigger.ParentID} Type: {trigger.Parent.GetType()}
Job Number: {(trigger as ProcessTask)?.JobNumber}
Trigger: {log.SJ_ParentID} Code: {trigger.TriggerEventCode}
Trigger Actions: {string.Join(",", trigger.CompletionTriggerActionsCollection().Select(action => action.PQ_TriggerType))}");

				var diagnosticLogInfo = new WorkflowTriggerEventData(log).GetDiagnosticLogInfo();
				if (!string.IsNullOrEmpty(diagnosticLogInfo))
				{
					messageBuilder.AppendLine(diagnosticLogInfo);
				}

				if (trigger.Parent is IForwardingConsol consol)
				{
					messageBuilder.AppendLine();
					messageBuilder.AppendLine((NoResString)"Forwarding Consol Details:");
					messageBuilder.AppendLine($"JK_MasterBillNum: {consol.JK_MasterBillNum}");
					messageBuilder.AppendLine($"JK_TransportMode: {consol.JK_TransportMode}");
					messageBuilder.AppendLine($"JK_RL_NKLoadPort: {consol.JK_RL_NKLoadPort}");
					messageBuilder.AppendLine($"JK_RL_NKDischargePort: {consol.JK_RL_NKDischargePort}");
					messageBuilder.AppendLine();

					try
					{
						var cusMAWBs = new BusinessObjectFactory().Load<ICusMAWB>(new ZQuery(CusMAWBSchema.CM_JK, trigger.ParentID))
							.OrderBy(mawb => mawb.CM_ApplicationCode)
							.OrderBy(mawb => mawb.CM_MAWB);
						messageBuilder.AppendLine((NoResString)"MAWB Details:");
						foreach (var cusMAWB in cusMAWBs)
						{
							messageBuilder.AppendLine($"CM_PK: {cusMAWB.PK}");
							messageBuilder.AppendLine($"CM_JK: {cusMAWB.CM_JK}");
							messageBuilder.AppendLine($"CM_MAWB: {cusMAWB.CM_MAWB}");
							messageBuilder.AppendLine($"CM_ApplicationCode: {cusMAWB.CM_ApplicationCode}");
							messageBuilder.AppendLine($"CM_FlightNo: {cusMAWB.CM_FlightNo}");
							messageBuilder.AppendLine($"CM_ArrivalDate: {cusMAWB.CM_ArrivalDate}");
							messageBuilder.AppendLine($"Type: {cusMAWB.GetType()}");
							messageBuilder.AppendLine($"RowHashCode: {((INeedRow)cusMAWB).Row.GetHashCode()}");
							if (cusMAWB != cusMAWBs.Last())
							{
								messageBuilder.AppendLine();
							}
						}
					}
					catch (Exception ex)
					{
						messageBuilder.AppendLine($"Failed to load CusMAWB: {ex.Message}");
					}
				}
				return messageBuilder.ToString();
			}
			else
			{
				return base.PrettyPrint(log);
			}
		}
	}
}
