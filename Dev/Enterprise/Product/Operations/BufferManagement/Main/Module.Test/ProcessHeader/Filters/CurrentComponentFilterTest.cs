using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(CurrentComponentFilter))]
	class CurrentComponentFilterTest : ComponentFilterTestCase<CurrentComponentFilter>
	{
		protected override string componentFilterDescription => ProcessHeader.ModuleFilterConstants.CurrentComponent;

		protected override void SetComponent(ProcessHeader workflow, ZGuid value)
		{
			workflow.FH_FC_CurrentComponent = value;
		}
	}
}
