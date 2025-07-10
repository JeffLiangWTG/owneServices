using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	public class QueuedLogReferenceProvider : IQueuedLogReferenceProvider
	{
		public QueuedLogReferenceProvider(ILogger logger = null)
		{
			this.logger = logger;
		}

		public IDictionary<string, string> CreateReferenceMap(ProcessTaskNotification triggerAction, IQueuedLog queuedLog, BusinessObject parent, GlbStaff userContext)
		{
			var parentType = parent.GetType();
			var parentTypeName = Assembly.CreateQualifiedName(parentType.Assembly.GetName().Name, parentType.FullName);
			var referenceMap = new Dictionary<string, string> {
				{ JobQueueReferenceParameters.TriggerAction, triggerAction.PQ_TriggerType },
				{ JobQueueReferenceParameters.ParentPK, parent.PK.ToString() },
				{ JobQueueReferenceParameters.ParentType, parentTypeName }
			};

			return referenceMap;
		}

		public QueuedLogParameters GetParametersFromReferenceMap(IDictionary<string, string> referenceMap, IQueuedLog queuedLog)
		{
			var parameters = new QueuedLogParameters();

			referenceMap.TryGetValue(JobQueueReferenceParameters.ParentPK, out var parentPKString);
			referenceMap.TryGetValue(JobQueueReferenceParameters.ParentType, out var typeName);

			ZGuid parentPK;
			if (string.IsNullOrEmpty(parentPKString) || !ZGuid.TryParse(parentPKString, out parentPK))
			{
				throw new InvalidOperationException($"{nameof(parentPKString)} is invalid.");
			}
			Type parentBizoType;
			if (string.IsNullOrEmpty(typeName) || (parentBizoType = Type.GetType(typeName)) == null)
			{
				throw new InvalidOperationException($"{nameof(typeName)} is invalid.");
			}

			var parent = queuedLog.Factory.Load(parentBizoType, parentPK);
			if (parent == null)
			{
				referenceMap.TryGetValue(JobQueueReferenceParameters.TriggerAction, out var triggerAction);
				logger?.Log(LogType.Debug, $"{nameof(parent)} is invalid. It might be deleted. ParentPK : {parentPK}, parentBizoType : {parentBizoType}, triggerAction : {triggerAction}");
			}
			else
			{
				var workflowProvider = parent as IWorkflowProvider;
				if (workflowProvider == null)
				{
					throw new InvalidOperationException($"{nameof(workflowProvider)} is invalid.");
				}

				parameters.WorkflowProvider = workflowProvider;
			}
			return parameters;
		}

		ILogger logger { get; }

#if DEBUG
		public ILogger Logger_ForTestOnly => logger;
#endif
	}
}
