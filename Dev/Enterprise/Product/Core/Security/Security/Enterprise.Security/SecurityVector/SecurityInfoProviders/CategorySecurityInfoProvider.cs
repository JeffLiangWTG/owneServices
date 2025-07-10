using System.Collections.Generic;
using Enterprise.Core.Modules;

namespace Enterprise.Security.Provider
{
	class CategorySecurityInfoProvider : SecurityInfoProvider
	{
		public CategorySecurityInfoProvider(SecurityInfoProvider parent, ModuleCategory category)
			: base(parent, GetSecurityCheckpoint(parent.Security, category.SecurityCheckpoint))
		{
			this.category = category;
		}

		public override IEnumerable<SecurityInfoProvider> GetChildren()
		{
			foreach (ModuleSection section in category.Sections.Values)
			{
				yield return new SectionSecurityInfoProvider(this, section);
			}
		}

		public override void FetchForGetChildren()
		{
			foreach (ModuleSection section in category.Sections.Values)
			{
				var provider = new SectionSecurityInfoProvider(this, section);
				provider.FetchForGetChildren();
			}
		}

		public override string Name { get { return category.DisplayTextWithoutAmpersand; } }

		readonly ModuleCategory category;
	}
}
