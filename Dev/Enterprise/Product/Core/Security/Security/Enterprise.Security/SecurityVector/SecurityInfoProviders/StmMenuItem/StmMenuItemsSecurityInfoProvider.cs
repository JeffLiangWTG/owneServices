using CargoWise.Common;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider
{
	abstract class StmMenuItemsSecurityInfoProvider : SecurityInfoProvider
	{
		internal StmMenuItemsSecurityInfoProvider(SecurityInfoProvider parent, INamedModule module, StmMenuItemCheckpointHelper helper)
			: base(parent, GetDocumentsCheckPoint(parent, module, helper))
		{
		}

		public override string Name { get { return Checkpoint.DisplayText; } }

		static SecurityCheckpoint GetDocumentsCheckPoint(SecurityInfoProvider parent, INamedModule module, StmMenuItemCheckpointHelper helper)
		{
			Argument.NotNull(helper, "helper");

			string code = helper.ModuleIDPrefix + module.ModuleID.ToString() + helper.ModuleIDSuffix;
			if (parent.Security.FindCheckPoint(new CheckpointLookupKey(code)) == null)
			{
				return new SecurityCheckpoint(code, helper.DisplayText, (SecurityCheckpoint)parent.Checkpoint, parent.Security);
			}

			return null;
		}
	}
}
