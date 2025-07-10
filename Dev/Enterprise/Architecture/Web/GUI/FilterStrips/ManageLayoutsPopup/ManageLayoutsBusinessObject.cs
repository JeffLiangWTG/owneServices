using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	public class ManageLayoutsBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ManageLayoutsBusinessObject(FilterStripBusinessObject filterStripBizO, WebUser user)
			: base(new BusinessObjectFactory())
		{
			FilterStripBizO = filterStripBizO;
			User = user;
		}

		protected readonly FilterStripBusinessObject FilterStripBizO;
		protected readonly WebUser User;

		#region SelectedFilterLayoutName

		[MaxLength(StmModuleFilter.Schema.S9_FilterNameMaxLength + 2)]
		public ZString SelectedFilterLayoutName
		{
			get { return fSelectedFilterLayoutName; }
			set
			{
				if (fSelectedFilterLayoutName != value)
				{
					CheckMaximumLength(SelectedFilterLayoutNameInfo, value);
					fSelectedFilterLayoutName = value;
					SelectedFilterLayoutNameInfo.RefreshBinding();
				}
			}
		}

		public ZString SelectedFilterLayoutNameEscaped
		{
			get
			{
				return SelectedFilterLayoutName.Replace(
				FilterStripLayoutsHelperForWeb.SingleQuote,
				FilterStripLayoutsHelperForWeb.SingleQuoteEscaped);
			}
		}

		public ZPropertyInfo SelectedFilterLayoutNameInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedFilterLayoutName)); }
		}

		ZString fSelectedFilterLayoutName;

		#endregion

		#region FilterLayoutExistsWithName

		public bool FilterLayoutExistsWithName(ZString layoutName, ZBool isPublished)
		{
			bool result = true;

			if (GetSelectedFilterLayoutFromRenamedFilters(layoutName) == null)
			{
				StmModuleFilter layout = FilterStripBizO.FindLayout(layoutName, isPublished);
				result = (layout != null && !IsFilterLayoutMarkedForRename(layout) && !IsFilterLayoutMarkedForDelete(layout));
			}
			return result;
		}

		#endregion

		#region GetSelectedFilterLayout

		public StmModuleFilter GetSelectedFilterLayout()
		{
			return GetSelectedFilterLayout(SelectedFilterLayoutName);
		}

		StmModuleFilter GetSelectedFilterLayout(ZString layoutName)
		{
			StmModuleFilter result = GetSelectedFilterLayoutFromRenamedFilters(layoutName);

			if (result == null)
			{
				var match = FilterLayouts.ToArray().FirstOrDefault(item => item.Description.Equals(layoutName, StringComparison.OrdinalIgnoreCase));
				if (match != null)
				{
					result = (StmModuleFilter)match.PK;
					if (IsFilterLayoutMarkedForRename(result) || IsFilterLayoutMarkedForDelete(result))
					{
						result = null;
					}
				}
			}

			return result;
		}

		StmModuleFilter GetSelectedFilterLayoutFromRenamedFilters(ZString layoutName)
		{
			foreach (ZGuid layoutPk in RenamedFilterLayoutPks.Keys)
			{
				if (RenamedFilterLayoutPks[layoutPk].EqualsIgnoringCase(layoutName))
				{
					return (StmModuleFilter)FilterStripBizO.Layouts.FindByPK(layoutPk);
				}
			}

			return null;
		}

		#endregion

		#region FilterLayouts

		public CodeDescriptionPairList FilterLayouts
		{
			get
			{
				if (fFilterLayouts == null)
				{
					fFilterLayouts = new CodeDescriptionPairList();
					if (FilterStripBizO != null)
					{
						fFilterLayouts.AddRange(GetFilterLayouts(FilterStripBizO.Layouts_PublishedOnly, false));
						fFilterLayouts.AddRange(GetFilterLayouts(FilterStripBizO.Layouts_UnpublishedOnly, false));
						if (((OrgContactWebUser)User).CanPublishCompanyLayouts)
						{
							fFilterLayouts.AddRangeOverwriteIfExists(GetFilterLayouts(FilterStripBizO.Layouts_WebCompanyOnly(EnvProxy.Instance.CurrentCompany.PK), true));
						}
					}
				}

				return fFilterLayouts;
			}
		}

		CodeDescriptionPairList GetFilterLayouts(ReadOnlyCollection<StmModuleFilter> layouts, bool includeCompany)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();

			foreach (StmModuleFilter layout in layouts)
			{
				if (!IsFilterLayoutMarkedForDelete(layout) && !layout.S9_IsSystem && !layout.IsUserDefinedFilter)
				{
					if (layout.IsPublishedForCompanyInWeb && !includeCompany)
					{
						continue;
					}

					ZString layoutName = StmModuleFilter.GetDisplayName(IsFilterLayoutMarkedForRename(layout) ? RenamedFilterLayoutPks[layout.PK] : new ZString(layout.S9_FilterNameMultilingual.GetUnresolvedString()), layout.IsUserDefinedFilter);
					result.AddPair(layout, layoutName, layoutName);
				}
			}

			return result;
		}

		CodeDescriptionPairList fFilterLayouts;

		#endregion

		#region Deleted Filter Layouts

		public void MarkFilterLayoutForDelete(StmModuleFilter layout)
		{
			DeletedFilterLayoutPks.Add(layout.PK);
			if (RenamedFilterLayoutPks.ContainsKey(layout.PK)) // delete takes precedence over a rename
			{
				RenamedFilterLayoutPks.Remove(layout.PK);
			}
			fFilterLayouts = null;
		}

		bool IsFilterLayoutMarkedForDelete(StmModuleFilter layout)
		{
			return layout != null && DeletedFilterLayoutPks.Contains(layout.PK);
		}

		List<ZGuid> DeletedFilterLayoutPks
		{
			get { return fDeletedFilterLayoutPks ?? (fDeletedFilterLayoutPks = new List<ZGuid>()); }
		}

		List<ZGuid> fDeletedFilterLayoutPks;

		#endregion

		#region Renamed Filter Layouts

		public void MarkFilterLayoutForRename(StmModuleFilter layout, ZString newName)
		{
			RenamedFilterLayoutPks[layout.PK] = newName;
			fFilterLayouts = null;
		}

		bool IsFilterLayoutMarkedForRename(StmModuleFilter layout)
		{
			return layout != null && RenamedFilterLayoutPks.ContainsKey(layout.PK);
		}

		Dictionary<ZGuid, ZString> RenamedFilterLayoutPks
		{
			get { return fRenamedFilterLayoutPks ?? (fRenamedFilterLayoutPks = new Dictionary<ZGuid, ZString>()); }
		}

		Dictionary<ZGuid, ZString> fRenamedFilterLayoutPks;

		#endregion

		#region Saving the changes to the FilterStripBusinessObject

		public void SaveChangesToFilterStripBusinessObject()
		{
			if (DeletedFilterLayoutPks.Count > 0 || RenamedFilterLayoutPks.Count > 0)
			{
				StmModuleFilter currentLayout = LayoutsHelper.CurrentLayout;
				bool currentLayoutIsDeleted = IsFilterLayoutMarkedForDelete(currentLayout);

				DeleteFilterLayoutsMarkedForDelete(); // must delete first
				RenameFilterLayoutsMarkedForRename();
				FilterStripBizO.Layouts.Factory.Save();

				if (currentLayoutIsDeleted)
				{
					LayoutsHelper.DeleteCurrentLayout();
				}
			}
		}

#if DEBUG
		public
#endif
		void DeleteFilterLayoutsMarkedForDelete()
		{
			foreach (ZGuid layoutPk in DeletedFilterLayoutPks)
			{
				StmModuleFilter layout = (StmModuleFilter)FilterStripBizO.Layouts.FindByPK(layoutPk);
				if (layout != null && !layout.S9_IsSystem)
				{
					var registryItem = LayoutsHelper.CurrentLayoutRegistryItem;
					var query = new ZQuery();
					query.AddToFilter(StmDataSchema.SD_Name, registryItem.Name);
					var registriesUsingLayout = FilterStripBizO.Factory.Load<StmData>(query).Where(x => x.SD_BinaryValue == new ZBlob(registryItem.DataType.Serialise(layout.S9_FilterName))).ToList();
					if (registriesUsingLayout.Any())
					{
						registriesUsingLayout.DeleteAll();
					}

					FilterStripBizO.Layouts.Delete(layout);
				}
			}

			FilterStripBizO.Factory.Save();

			DeletedFilterLayoutPks.Clear();
		}

		void RenameFilterLayoutsMarkedForRename()
		{
			foreach (ZGuid layoutPk in RenamedFilterLayoutPks.Keys)
			{
				StmModuleFilter layout = (StmModuleFilter)FilterStripBizO.Layouts.FindByPK(layoutPk);
				if (layout != null && !layout.IsDeleted)
				{
					layout.S9_FilterName = RenamedFilterLayoutPks[layoutPk];
				}
			}
			RenamedFilterLayoutPks.Clear();
		}

		FilterStripLayoutsHelperForWeb LayoutsHelper
		{
			get { return (FilterStripLayoutsHelperForWeb)FilterStripBizO.LayoutsHelper; }
		}

		#endregion

		#region ResetChanges

		public void ResetChanges()
		{
			fDeletedFilterLayoutPks = null;
			fRenamedFilterLayoutPks = null;
			fFilterLayouts = null;
			SelectedFilterLayoutName = "";
		}

		#endregion
	}
}
