using System.Collections.Generic;
using Enterprise.Core.Modules;

namespace Enterprise.Security.Provider
{
	class DocumentsSecurityInfoProvider : StmMenuItemsSecurityInfoProvider
	{
		public DocumentsSecurityInfoProvider(SecurityInfoProvider parent, INamedModule module, StmMenuItemsForSecurity menuItems)
			: base(parent, module, new DocumentsCheckpointHelper())
		{
			this.module = module;
			this.menuItems = menuItems;
		}

		readonly INamedModule module;
		readonly StmMenuItemsForSecurity menuItems;

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			for (int i = 0; i < menuItems.Count; i++)
			{
				yield return new DocumentSecurityInfoProvider(this, module, menuItems, i, new DocumentsCheckpointHelper());
			}
		}
	}
}
