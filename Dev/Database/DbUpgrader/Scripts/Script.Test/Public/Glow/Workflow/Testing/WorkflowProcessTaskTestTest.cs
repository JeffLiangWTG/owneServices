using System;
using System.Collections.Generic;
using Enterprise.Build.Database.Script.Public.Test;

namespace Enterprise.Build.Database.Script.Public.Glow.Workflow.Testing
{
	abstract class WorkflowProcessTaskTest : WorkflowTest<Tuple<string, bool>>
	{
		protected override Guid GenerateNewItem(Tuple<string, bool> item)
		{
			return ProcessTasksGeneratorForTest.NewProcessTask(item.Item1, item.Item2);
		}

		protected override List<Tuple<string, bool>> AllItems => new List<Tuple<string, bool>>
		{
			new Tuple<string, bool>("MIL", true),
			new Tuple<string, bool>("MIL", false),
			new Tuple<string, bool>("EXC", true),
			new Tuple<string, bool>("EXC", false),
			new Tuple<string, bool>("TRG", true),
			new Tuple<string, bool>("TRG", false),
			new Tuple<string, bool>("ARV", true),
			new Tuple<string, bool>("ARV", false)
		};

		protected override string PKName => "P9_PK";
	}
}

