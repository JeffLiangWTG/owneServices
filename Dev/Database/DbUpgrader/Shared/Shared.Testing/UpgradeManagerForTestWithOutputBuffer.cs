using System.Collections.Generic;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Shared.Testing
{
	public class UpgradeManagerForTestWithOutputBuffer : DummyUpgradeManager
	{
		public readonly List<string> OutputTextCollection = new List<string>();

		#region IUpgradeManager Overrides

		public override void StartTask(string task)
		{
			OutputTextCollection.Add(task);
		}

		public override void StartNonEstimatedTask(string task)
		{
			OutputTextCollection.Add(task);
		}

		public override void StartSubtask(string subtask)
		{
			OutputTextCollection.Add(subtask);
		}

		public override void ShowInfoMessage(string infoMessage)
		{
			OutputTextCollection.Add(infoMessage);
		}

		public override void ShowTaskError(string errorMessage)
		{
			OutputTextCollection.Add(errorMessage);
		}

		public override VersionLabel TransformationVersionBeforeUpgrade => TransformationVersionBeforeUpgradeOverridenValue ?? new VersionLabel(0, 0);

		public VersionLabel TransformationVersionBeforeUpgradeOverridenValue { get; set; }

		#endregion
	}
}
