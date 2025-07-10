using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentLinkLookups : AutoBMComponentLinkLookups
	{
		public BMComponentLinkLookups(AutoBMComponentLink parent)
			: base(parent)
		{
		}

		#region ComponentFroms

		public BMComponentCollection ComponentFroms
		{
			get { return ComponentFromsCore(); }
		}

		protected virtual BMComponentCollection ComponentFromsCore()
		{
			var link = Parent as BMComponentLink;

			return link?.ComponentTo?.System?.Components ?? new BMComponentCollection(Factory);
		}

		#endregion

		#region ComponentTos

		public BMComponentCollection ComponentTos
		{
			get
			{
				return ComponentTosCore();
			}
		}

		protected virtual BMComponentCollection ComponentTosCore()
		{
			var components = new BMComponentCollection(Factory);
			var link = Parent as BMComponentLink;
			if (link != null)
			{
				var system = link.ComponentToSystemPK.IsValid ?
					link.ComponentToSystemPK :
					link.ComponentFrom?.FC_FS_System;

				if (system != null)
				{
					var query = new ZQuery(BMComponentSchema.FC_FS_System, system);
					query.AddToFilter(BMComponentSchema.FC_FC_ParentComponent, null);
					components.AdditionalFilter = query;
				}

				components.ApplySort(BMComponent.Schema.FC_DisplaySequence, ListSortDirection.Ascending);
			}

			return components;
		}

		#endregion

		#region Systems

		public BMSystemCollection Systems
		{
			get { return new BMSystemCollection(Factory); }
		}

		#endregion
	}
}
