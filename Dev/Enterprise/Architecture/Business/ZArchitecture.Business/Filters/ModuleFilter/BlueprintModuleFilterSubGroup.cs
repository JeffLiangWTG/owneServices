using System;
using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class BlueprintModuleFilterSubGroup
	{
		protected BlueprintModuleFilterSubGroup(BlueprintModuleFilterSubGroup parent)
			: this(parent, false) { }

		protected BlueprintModuleFilterSubGroup(BlueprintModuleFilterSubGroup parent, bool allowNullParent)
		{
			if (parent == null && !allowNullParent)
			{
				throw new ArgumentNullException(nameof(parent));
			}

			this.parent = parent;
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly BlueprintModuleFilterSubGroup parent;

		public BlueprintModuleFilterSubGroup Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return parent; }
		}

		public string CurrentlyProcessedGroup { get; set; }

		public FilterOrCategory CurrentlyProcessedFilterOrCategory { get; set; }

		public List<FilterOrCategory> CategoriesToIgnore { get; set; }

		public abstract QueryBlueprintPart GetSubQuery(QueryBlueprintPart query);
	}
}
