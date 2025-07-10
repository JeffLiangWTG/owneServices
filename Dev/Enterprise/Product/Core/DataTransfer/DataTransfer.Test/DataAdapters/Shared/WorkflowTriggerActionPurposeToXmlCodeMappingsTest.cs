using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Registry.Business.WorkflowManager;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(WorkflowTriggerActionPurposeXmlCodeMappings))]
	sealed class WorkflowTriggerActionPurposeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(ProcessTaskTriggerPurposeList.Codes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}
	}
}
