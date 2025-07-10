using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider
{
	class OperationalActionsSecurityInfoProvider : SecurityInfoProvider
	{
		public OperationalActionsSecurityInfoProvider(SecurityInfoProvider parent, ISecurityCheckpoint checkpoint, IZFilterGridModule gridModule) : base(parent, checkpoint)
		{
			GridModule = gridModule;
		}
		readonly IZFilterGridModule GridModule;

		#region Overrides of SecurityInfoProvider

		public override string Name
		{
			get { return Checkpoint.DisplayText; }
		}

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			yield return new CheckpointSecurityInfoProvider(this, Security.FindOrCreateOperationalActionsCustomiseCheckpoint((SecurityCheckpoint)Checkpoint.Parent));

			yield return new CheckpointSecurityInfoProvider(this, Security.FindOrCreateOperationalActionsAllowRunOnAllMatchingRecordsCheckpoint((SecurityCheckpoint)Checkpoint.Parent));
			//and then the run checkpoint, with everything inside
			var result = new CheckpointSecurityInfoProvider(this, Security.FindOrCreateOperationalActionsRunCheckpoint((SecurityCheckpoint)Checkpoint.Parent));
			foreach (MultilingualString operationalAction in ObjectFactory.New<IModuleOperationalActionsHelper>().OperationalActionNames(GridModule, Factory).OfType<MultilingualString>())
			{
				Security.FindOrCreateOperationalActionsRunSpecificCheckpoint(operationalAction, (SecurityCheckpoint)result.Checkpoint);
			}
			yield return result;
		}

		#endregion
	}
}
