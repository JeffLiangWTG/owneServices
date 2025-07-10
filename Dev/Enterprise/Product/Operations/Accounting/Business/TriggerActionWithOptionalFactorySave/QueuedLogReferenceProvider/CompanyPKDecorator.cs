using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	public sealed class CompanyPKDecorator : QueuedLogReferenceProviderDecorator
	{
		public CompanyPKDecorator(IQueuedLogReferenceProvider provider) : base(provider) { }

		public override IDictionary<string, string> CreateReferenceMap(ProcessTaskNotification triggerAction, IQueuedLog queuedLog, BusinessObject parent, GlbStaff userContext)
		{
			var referenceMap = base.CreateReferenceMap(triggerAction, queuedLog, parent, userContext);
			referenceMap.Add(JobQueueReferenceParameters.TriggerCompanyPK, triggerAction.Parent.CompanyPK.ToString());

			return referenceMap;
		}

		public override QueuedLogParameters GetParametersFromReferenceMap(IDictionary<string, string> referenceMap, IQueuedLog queuedLog)
		{
			var parameters = base.GetParametersFromReferenceMap(referenceMap, queuedLog);

			referenceMap.TryGetValue(JobQueueReferenceParameters.TriggerCompanyPK, out var triggerCompanyPKString);
			var triggerCompanyPK = ZGuid.Empty;
			if (!string.IsNullOrWhiteSpace(triggerCompanyPKString))
			{
				if (!ZGuid.TryParse(triggerCompanyPKString, out triggerCompanyPK))
				{
					throw new InvalidOperationException($"{nameof(triggerCompanyPK)} is not a valid GUID.");
				}
			}

			parameters.CompanyPK = triggerCompanyPK;
			return parameters;
		}
	}
}
