using System.Collections.Generic;

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider
{
	class ISACHKSecurityInfoProvider : SecurityInfoProvider
	{
		public ISACHKSecurityInfoProvider(SecurityInfoProvider parent, ISecurityCheckpoint checkpoint) : base(parent, checkpoint) { }

		#region Overrides of SecurityInfoProvider

		public override string Name => Checkpoint.DisplayText;

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			yield return new CheckpointSecurityInfoProvider(this, Security.FindOrCreateISACHKSendWithMessageErrorsCheckpoint((SecurityCheckpoint)Checkpoint));
		}

		#endregion
	}
}
