using System;
using System.Collections.Generic;
using Enterprise.Build.Database.Script.Public.Test;

namespace Enterprise.Build.Database.Script.Public.Glow.Workflow.Testing
{
	abstract class WorkflowLogTest : WorkflowTest<string>
	{
		protected override Guid GenerateNewItem(string item)
		{
			return StmALogGeneratorForTest.NewLog(item);
		}

		protected override List<string> AllItems => new List<string> { "ADD", "XXX", "YYY", "EDT", "DEL", "ACT", "INA", "SDF", "SDU", "UST" };

		protected override string PKName => "SL_PK";
	}
}

