using System.Collections.Generic;

namespace Enterprise.Security.Provider
{
	class CheckpointSecurityInfoProvider : SecurityInfoProvider
	{
		public CheckpointSecurityInfoProvider(SecurityInfoProvider parent, SecurityCheckpoint checkpoint)
			: base(parent, checkpoint)
		{ }

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			if (Checkpoint.ChildCheckPoints != null)
			{
				foreach (SecurityCheckpoint checkpoint in Checkpoint.ChildCheckPoints)
				{
					yield return new CheckpointSecurityInfoProvider(this, checkpoint);
				}
			}
		}

		public override string Name { get { return Checkpoint.DisplayText; } }
	}
}