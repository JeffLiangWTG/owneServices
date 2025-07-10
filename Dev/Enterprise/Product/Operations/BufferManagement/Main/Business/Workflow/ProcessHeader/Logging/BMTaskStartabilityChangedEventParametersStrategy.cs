using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	static class BMTaskStartabilityChangedEventParametersStrategy
	{
		public static string GetLogReference(ProcessTask processTask, bool isStartable)
		{
			var parameters = new Dictionary<string, string>();

			if (!processTask.P9_GS_NKAssignedStaffMember.IsEmpty)
			{
				parameters.Add(ProcessTaskStatusCodeList.Codes.Assigned, processTask.P9_GS_NKAssignedStaffMember);
			}

			ProcessHeader processHeader = null;

			if (BMSRegistryProvider.IsBufferManagementEnabled && (processHeader = processTask.ProcessHeader as ProcessHeader) != null)
			{
				BufferStatus bufferStatus = null;

				var currentComponent = processHeader.CurrentComponent;
				if (currentComponent != null && currentComponent.IsBuffer)
				{
					var bufferPenetration = processHeader.CalculateBufferPenetration();
					var zone = ZoneCalculator.CalculateZone(bufferPenetration.Penetration);

					bufferStatus = new BufferStatus(
						constraintStatus: ConstraintStatus.Unknown,
						bufferPenetration: bufferPenetration.Penetration,
						bufferZone: zone);

					foreach (var pair in ProcessHeader.GetBufferLogInfo(bufferStatus))
					{
						parameters[pair.Key] = pair.Value;
					}
				}
			}

			var reference = WorkflowStartabilityStrategy.GetStartabilityLogReferenceString(isStartable);

			return StmALog.GenerateEventReferenceToFitInReferenceMaxLength(reference, parameters);
		}
	}
}
