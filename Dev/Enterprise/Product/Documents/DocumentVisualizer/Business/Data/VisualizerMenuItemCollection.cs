using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class VisualizerMenuItemCollection : StmMenuItemBaseCollection
	{
		public VisualizerMenuItemCollection(BusinessObjectFactory factory, string businessContext)
			: this(factory, businessContext, null)
		{
		}

		public VisualizerMenuItemCollection(BusinessObjectFactory factory, string businessContext, ZGuid[] excludedTemplatePKs)
			: base(factory)
		{
			this.businessContext = businessContext;
			this.excludedTemplatePKs = excludedTemplatePKs ?? Array.Empty<ZGuid>();
		}

		readonly ZString businessContext;
		readonly ZGuid[] excludedTemplatePKs;

		#region Overrides

		public new VisualizerMenuItem this[int index] => (VisualizerMenuItem)Elements[index];

		public new VisualizerMenuItem AddNew() => (VisualizerMenuItem)base.AddNew();

		public override void Load()
		{
			var filter = GetFilterForLoad();
			Load(filter);
		}

		ZQuery GetFilterForLoad()
		{
			var filter = new ZQuery();

			if (!businessContext.IsEmpty)
			{
				filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, businessContext);
			}

			if (AdditionalFilter != null)
			{
				filter.AddToFilter(AdditionalFilter);
			}

			return GetFilterFromExcludedTemplatePKs(excludedTemplatePKs, filter);
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(StmMenuItemSchema.SU_MenuType, Constants.StmMenuItemTypes.Forms);
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var privateFilter = new ZQuery(StmMenuItemSchema.SU_GS_NKStaffCode, GlbStaff.CurrentUser.GS_Code);

			var publishedFilter = new ZQuery(StmMenuItemSchema.SU_GS_NKStaffCode, ZString.Empty);
			publishedFilter.AddToFilter(privateFilter, JoinCondition.Or);
			publishedFilter.OrderBy = StmMenuItemBase.Schema.SU_MenuPath + ", " + StmMenuItemBase.Schema.SU_MenuIndex;

			return publishedFilter;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (child is VisualizerMenuItem menuItem)
			{
				menuItem.SU_BusinessContext = businessContext;
			}
		}

		protected override Type TemplateType => typeof(VisualizerTemplate);

		#endregion
	}
}

