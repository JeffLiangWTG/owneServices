using System.Collections.Generic;

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider
{
	class UniversalCopySecurityInfoProvider : SecurityInfoProvider
	{
		public UniversalCopySecurityInfoProvider(SecurityInfoProvider parent, ISecurityCheckpoint checkpoint) : base(parent, checkpoint) { }

		#region Overrides of SecurityInfoProvider

		public override string Name
		{
			get { return Checkpoint.DisplayText; }
		}

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			yield return new CheckpointSecurityInfoProvider(this, Security.FindOrCreateUniversalCopyRunCheckpoint((SecurityCheckpoint)Checkpoint));
			yield return new CheckpointSecurityInfoProvider(this, Security.FindOrCreateUniversalCopyCEDPrivateCheckpoint((SecurityCheckpoint)Checkpoint));
			yield return new CheckpointSecurityInfoProvider(this, Security.FindOrCreateUniversalCopyEditPublishCheckpoint((SecurityCheckpoint)Checkpoint));
			yield return new CheckpointSecurityInfoProvider(this, Security.FindOrCreateUniversalCopyDeletePublishCheckpoint((SecurityCheckpoint)Checkpoint));
		}

		#endregion
	}
}
