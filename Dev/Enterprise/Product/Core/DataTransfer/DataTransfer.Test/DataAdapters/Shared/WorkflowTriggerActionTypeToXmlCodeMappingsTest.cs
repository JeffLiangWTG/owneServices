using System;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(WorkflowTriggerActionTypeXmlCodeMappings))]
	sealed class WorkflowTriggerActionTypeToXmlCodeMappingsTest : EnterpriseCodeExternalCodeMappingsTest
	{
		protected override Type[] EnterpriseCodeDefinitionsClassesToCheckAgainst
		{
			get { return new[] { typeof(WorkflowTriggerActionTypeConstants.Codes) }; }
		}

		protected override bool EnterpriseAndExternalCodeShouldBeSame
		{
			get { return true; }
		}

		protected override string[] GetEnterpriseCodesToExcludeCheckingAgainst()
		{
			return
				new[]
				{
					WorkflowTriggerActionTypeConstants.Codes.SendDocument,
					WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs,
					WorkflowTriggerActionTypeConstants.Codes.SetField,
					WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange,
				};
		}
	}
}
