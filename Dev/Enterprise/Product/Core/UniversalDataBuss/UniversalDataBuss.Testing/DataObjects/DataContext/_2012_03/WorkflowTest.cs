using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11.Testing
{
	[TestedType(typeof(Workflow))]
	class WorkflowTest : DataObjectTestCase<Workflow>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(Workflow.TriggerReference), ProcessTask.MacroTriggerConditionValueMaxLength },
				{ nameof(Workflow.EventReference), StmALogSchema.SL_Reference.MaxLength },
				{ nameof(Workflow.TriggerDescription), ProcessTasksSchema.P9_Description.MaxLength },
			};
		}
	}
}

