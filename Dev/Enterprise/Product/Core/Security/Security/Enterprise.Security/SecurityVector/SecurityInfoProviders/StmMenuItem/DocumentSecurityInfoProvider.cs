using System.Collections.Generic;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider
{
	class DocumentSecurityInfoProvider : StmMenuItemSecurityInfoProvider
	{
		public DocumentSecurityInfoProvider(SecurityInfoProvider parent, INamedModule module, StmMenuItemsForSecurity menuItems, int index, StmMenuItemCheckpointHelper helper)
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
			var item = menuItems.GetValue(index);
			string code = "DocVis" + module.ModuleID.ToString();
			if (Security.FindCheckPoint(new CheckpointLookupKey(code, item.PK.ToGuid())) == null)
			{
				var checkpoint = new SecurityCheckpoint(code, ResString.GetMultilingualString("96d46d83-f7b0-42fc-8ddd-e59fcc311805", "Modify"), (SecurityCheckpoint)Checkpoint, Security, item.PK.ToGuid());
				yield return new CheckpointSecurityInfoProvider(this, checkpoint);
			}
		}
	}
}
