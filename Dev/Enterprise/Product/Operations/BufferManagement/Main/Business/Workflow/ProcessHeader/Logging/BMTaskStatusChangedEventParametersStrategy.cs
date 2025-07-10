using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	class BMTaskStatusChangedEventParametersStrategy : TaskStatusChangedEventParametersStrategy
	{
		protected override IEnumerable<KeyValuePair<string, string>> GetAdditionalParameters(ProcessTask processTask)
		{
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

					foreach (var param in ProcessHeader.GetBufferLogInfo(bufferStatus))
					{
						yield return param;
					}
				}
			}
		}

		protected override void AddFetchHintsCore(ProcessTask processTask, BusinessObjectFactory factory)
		{
			if (processTask != null
					&& processTask.P9_StatusInfo.HasChanges
					&& ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled)
			{
				var parent = processTask.Parent;

				if (!(parent is ProcessTask) && !(parent is IProcessHeader))
				{
					var processHeader = processTask.ProcessHeader;

					factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, processTask.P9_FH_ProcessHeader));
					factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, processTask.P9_FH_ProcessHeader));

					factory.AddFetchHint(BMNCNShapeSchema.Instance, new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, processTask.P9_FH_ProcessHeader));

					if (processHeader != null && processHeader.FH_FH_ParentHeader.IsValid)
					{
						factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, processHeader.FH_FH_ParentHeader));
						factory.AddFetchHint(ProcessHeaderLinkSchema.Instance, new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderTo, processHeader.FH_FH_ParentHeader));

						factory.AddFetchHint(BMNCNShapeSchema.Instance, new ZQuery(BMNCNShapeSchema.BNS_RelatedEntityID, processHeader.FH_FH_ParentHeader));
					}
				}
			}
		}
	}
}
