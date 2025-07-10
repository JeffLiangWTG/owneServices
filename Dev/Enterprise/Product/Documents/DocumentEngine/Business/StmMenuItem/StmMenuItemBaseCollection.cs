using System;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Business
{
	public class StmMenuItemBaseCollection : StmMenuItemCollection
	{
		public StmMenuItemBaseCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			IsManagedForDataRefresh = true;
		}

		public new StmMenuItemBase FindByPK(ZGuid pK)
		{
			return (StmMenuItemBase)base.FindByPK(pK);
		}

		public StmMenuItemBaseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			IsManagedForDataRefresh = true;
		}

		public StmMenuItemBaseCollection(BusinessObjectFactory factory, bool includeForms)
			: this(factory)
		{
			this.includeForms = includeForms;
		}

		protected readonly bool includeForms;

		public MenuEditingMode EditingMode
		{
			get { return fEditingMode; }
			set
			{
				fEditingMode = value;

				foreach (StmMenuItemBase item in this)
				{
					item.EditingMode = value;
				}
			}
		}

		public new StmMenuItemBase this[int index]
		{
			get { return (StmMenuItemBase)Elements[index]; }
		}

		public new StmMenuItemBase AddNew()
		{
			return (StmMenuItemBase)base.AddNew();
		}

		public new StmMenuItemBase AddNew(Type bizObjType)
		{
			return (StmMenuItemBase)base.AddNew(bizObjType);
		}

		protected static ZQuery GetApplicableMenusFilter(string businessContext, bool includePrivate, bool includeForms = false)
		{
			DocumentZQuery result = new DocumentZQuery(businessContext, includeForms);
			result.OrderBy = StmMenuItemBase.Schema.SU_MenuPath + ", " + StmMenuItemBase.Schema.SU_MenuIndex;

			if (!Env.CurrentUser.IsController)
			{
				ZQuery publishedFilter = new ZQuery(StmMenuItemSchema.SU_GS_NKStaffCode, "");

				if (Env.Security.AccessUnpublishedCustomizedDocumentsAndReports.IsAllowed)
				{
					var unpublishedFilter = new ZQuery(StmMenuItemSchema.SU_IsPublished, false);

					publishedFilter.AddToFilter(unpublishedFilter, JoinCondition.Or);
				}

				if (includePrivate)
				{
					ZQuery privateFilter = new ZQuery(StmMenuItemSchema.SU_GS_NKStaffCode, GlbStaff.CurrentUser.GS_Code);
					publishedFilter.AddToFilter(privateFilter, JoinCondition.Or);
				}

				result.AddToFilter(publishedFilter, JoinCondition.And);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		public void Load(BusinessContext[] businessContexts, ZGuid[] excludedTemplatePKs = null)
		{
			if (businessContexts != null)
			{
				ZQuery filter = new ZQuery();

				foreach (BusinessContext businessContext in businessContexts)
				{
					filter.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.Equal, businessContext.ToString());
				}

				if (AdditionalFilter != null)
				{
					filter.AddToFilter(AdditionalFilter);
				}

				Load(GetFilterFromExcludedTemplatePKs(excludedTemplatePKs, filter));
			}

			var mappedBusinessContexts = new VirtualBusinessContextsMapping();
			foreach (BusinessContext businessContext in businessContexts)
			{
				if (mappedBusinessContexts.ContainsKey(businessContext.ToString()))
				{
					CreateItemsForVirtualBusinessContexts(businessContext.ToString());
				}
			}
		}

		protected ZQuery GetFilterFromExcludedTemplatePKs(ZGuid[] excludedTemplatePKs, ZQuery originalFilter)
		{
			if (excludedTemplatePKs == null || excludedTemplatePKs.Length == 0)
			{
				return originalFilter;
			}

			var filterExcludingMenuItemsLinkedToExcludedTemplates = new ZDBOnlyQuery(typeof(StmMenuItemBase));
			filterExcludingMenuItemsLinkedToExcludedTemplates.AddToFilter(originalFilter);

			var excludedPivotsQuery = new ZDBOnlySubQuery(typeof(StmMenuTemplatePivot), StmMenuTemplatePivotSchema.SI_SU, true);
			excludedPivotsQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_SO, SQLComparisonOperator.Equal, excludedTemplatePKs);

			filterExcludingMenuItemsLinkedToExcludedTemplates.AddSubQuery(excludedPivotsQuery, JoinCondition.And);

			return filterExcludingMenuItemsLinkedToExcludedTemplates;
		}

		void CreateItemsForVirtualBusinessContexts(ZString virtualBusinessContext)
		{
			var mappedBusinessContexts = new VirtualBusinessContextsMapping();
			ZString realContext;
			mappedBusinessContexts.TryGetValue(virtualBusinessContext, out realContext);

			var stmMenuItem = this.Cast<StmMenuItemBase>()
						.Where(x => x.SU_BusinessContext == realContext)
						.ToArray();

			foreach (StmMenuItemBase item in stmMenuItem)
			{
				Add(ProxyStmMenuItemBase.New(item, virtualBusinessContext));
			}
		}

		public StmTemplateBase LoadNewTemplateFromFile(string fileFullPath)
		{
			var blob = StmTemplateBase.GetTemplateBlobFromFile(fileFullPath);

			var result = (StmTemplateBase)Factory.New(TemplateType);
			result.SO_DataContext = DataContext.ToString();
			result.SO_Name = (NoResString)"New Template";
			result.SO_Template = blob;
			return result;
		}

		#region Implementation

		MenuEditingMode fEditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;

		protected virtual Core.Constants.DataContext DataContext
		{
			get { return Core.Constants.DataContext.None; }
		}

		protected override BusinessObject CreateInitialisedBusinessObjectFromRow(System.Data.DataRow row)
		{
			StmMenuItemBase newMenu = (StmMenuItemBase)base.CreateInitialisedBusinessObjectFromRow(row);
			newMenu.EditingMode = EditingMode;
			return newMenu;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			StmMenuItemBase menuItem = (StmMenuItemBase)bizOAdded;
			menuItem.EditingMode = EditingMode;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new DocumentZQuery(includeForms);
		}

		protected void LoadMenus(ZQuery filter)
		{
			Load(filter);
		}

		protected virtual Type TemplateType
		{
			get { return typeof(StmTemplateBase); }
		}

		#endregion

		#region For Testing
#if DEBUG

		internal static ZQuery GetApplicableMenusFilterForTesting(string businessContext, bool includePrivate)
		{
			return GetApplicableMenusFilter(businessContext, includePrivate);
		}

#endif
		#endregion
	}
}
