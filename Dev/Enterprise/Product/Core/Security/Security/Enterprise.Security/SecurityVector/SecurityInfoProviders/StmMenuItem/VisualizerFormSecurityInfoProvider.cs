using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider
{
	class VisualizerFormSecurityInfoProvider : StmMenuItemSecurityInfoProvider
	{
		public VisualizerFormSecurityInfoProvider(SecurityInfoProvider parent, INamedModule module, StmMenuItemsForSecurity menuItems, int index, StmMenuItemCheckpointHelper helper)
			: base(parent, module, menuItems, index, helper)
		{
			this.module = module;
			this.menuItems = menuItems;
			this.index = index;
		}

		readonly INamedModule module;
		readonly StmMenuItemsForSecurity menuItems;
		readonly int index;

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			var pk = menuItems.GetValue(index).PK.ToGuid();

			var checkpoints = new List<SecurityInfoProvider>();
			checkpoints.Add(GetCheckpoint("FormModify", ResString.GetMultilingualString("f16862b0-22d9-4935-b144-73e4a4ed073d", "Modify"), pk));
			checkpoints.Add(GetCheckpoint("FormDelivery", ResString.GetMultilingualString("293c60d9-3418-4a88-b79d-65aacaca3049", "Deliver Document"), pk));
			checkpoints.Add(GetCheckpoint("FormSendMessage", ResString.GetMultilingualString("ec6529a3-1391-45c6-81aa-457049488ad7", "Send Message"), pk));

			return checkpoints.Where(x => x != null);
		}

		CheckpointSecurityInfoProvider GetCheckpoint(string prefix, MultilingualString displayText, Guid guid)
		{
			var code = prefix + module.ModuleID.ToString();

			if (Security.FindCheckPoint(new CheckpointLookupKey(code, guid)) == null)
			{
				var checkpoint = new SecurityCheckpoint(code, displayText, (SecurityCheckpoint)Checkpoint, Security, guid);
				return new CheckpointSecurityInfoProvider(this, checkpoint);
			}

			return null;
		}
	}
}
