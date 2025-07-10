using System;
using System.Collections.Generic;
using System.Globalization;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	class BMTaskPenetrationResetEventParametersStrategy : TaskPenetrationResetEventParametersStrategy
	{
		readonly decimal maxBufferPenetration = 99.99m;

		public override IEnumerable<KeyValuePair<string, string>> GetLogReferenceParameters(ProcessTask processTask)
		{
			ProcessHeader processHeader;

			if (BMSRegistryProvider.IsBufferManagementEnabled && (processHeader = processTask.ProcessHeader as ProcessHeader) != null)
			{
				var currentComponent = processHeader.CurrentComponent;
				if (currentComponent != null && currentComponent.IsBuffer)
				{
					// Maximum buffer penetration is 9999% to avoid overflow on SL_Reference.  
					var workflowPenetration = Math.Min(processHeader.GetPenetrationPercentage(), maxBufferPenetration);
					yield return new KeyValuePair<string, string>(BMConstants.BufferParameters.WorkflowBufferPenetration, Utilities.Round(workflowPenetration, 2).ToString("0.00", CultureInfo.InvariantCulture));

					var taskPenetration = Math.Min(processHeader.GetTaskPenetrationPercentage(processTask), maxBufferPenetration);
					yield return new KeyValuePair<string, string>(BMConstants.BufferParameters.TaskBufferPenetration, Utilities.Round(taskPenetration, 2).ToString("0.00", CultureInfo.InvariantCulture));
				}
			}
		}
	}
}
