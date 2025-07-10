using System.Collections.Generic;

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider
{
	class TemplateRecordSecurityInfoProvider : SecurityInfoProvider
	{
		public TemplateRecordSecurityInfoProvider(SecurityInfoProvider parent, ISecurityCheckpoint checkpoint) : base(parent, checkpoint) { }

		#region Overrides of SecurityInfoProvider

		public override string Name => Checkpoint.DisplayText;

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			yield return new CheckpointSecurityInfoProvider(this, Security.FindOrCreateTemplateRecordAddCheckpoint((SecurityCheckpoint)Checkpoint));
			yield return new CheckpointSecurityInfoProvider(this, Security.FindOrCreateTemplateRecordEditCheckpoint((SecurityCheckpoint)Checkpoint));
			yield return new CheckpointSecurityInfoProvider(this, Security.FindOrCreateTemplateRecordDeleteCheckpoint((SecurityCheckpoint)Checkpoint));
		}

		#endregion
	}
}
