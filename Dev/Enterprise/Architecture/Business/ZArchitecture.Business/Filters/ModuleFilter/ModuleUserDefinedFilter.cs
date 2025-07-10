using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.Business
{
	[System.Diagnostics.DebuggerDisplay("ModuleUserDefinedFilter: '{Description}'")]
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ModuleUserDefinedFilter : ModuleGuidFilter, IIndexSearchModuleFilter
	{
		public ModuleUserDefinedFilter(StmModuleFilter layout, ModuleIdentifier id, SchemaGuidColumn pkColumn)
			: base(GetPrefixedDescription(layout), id, pkColumn, () => null)
		{
			LocalizedDescriptionSuffix = Suffix;
			this.layout = layout;
			layoutName = layout.S9_FilterName;
		}

		protected override FilterCategory DefaultCategory => FilterCategories.UserDefined;

		#region Filter Description

		public const string Suffix = "[+]";
		public const string DescriptionPrefix = "[USR]"; // non-translatable identifier

		public static ZString GetPrefixedDescription(StmModuleFilter layout)
		{
			return GetPrefixedDescription(layout.S9_FilterName);
		}

		public static ZString GetPrefixedDescription(string layoutName)
		{
			return DescriptionPrefix + layoutName;
		}

		public static ZString GetSuffixedDescription(string layoutName)
		{
			return layoutName + " " + Suffix;
		}

		#endregion

		#region Fields

		public IGlbStaff CreatorOrEarlyestUser => layout?.CreatorOrEarlyestUser;

		public bool IsPublished => layout?.S9_IsPublished ?? false;

		public bool IsLayoutDeleted => layout?.IsDeleted ?? false; // null layout? Can't be deleted!

		public ZGuid LayoutPK => layout?.PK ?? ZGuid.Empty;

		public bool IsPublishedForSpecificCompany => layout?.S9_GC != null && layout?.S9_GC != ZGuid.Empty;

		public bool IsIndexSearch => layout?.S9_IsIndexSearch ?? false;

		StmModuleFilter layout;
		ZString layoutName;

		public ZString LayoutName { get => layoutName; }

		#endregion

		#region Implementation

		public override ZString ComparisonOperator => ModuleTextFilter.ComparisonConstants.FiltersMatch;

		protected override bool IsComparisonOperatorAutomaticallySelectedCore() => true;

		protected override void ClearCore()
		{
			// Clear: it does nothing. It doesn't make sense to clear the configured filters.
		}

		protected override FilterStripBusinessObject GetNewSelectedFilters(StmModuleFilter layoutToLoad)
		{
			return base.GetNewSelectedFilters(layoutToLoad ?? layout);
		}

		public IGlowQuery GetGlowIndexQuery()
		{
			if (IsIndexSearch)
			{
				var filterStripBizo = GetNewSelectedFilters(null);
				return new BooleanQuery(BooleanOperator.And, filterStripBizo?.GetActiveFiltersQueries().ToArray());
			}
			return null;
		}

		#endregion

		#region Filter Reset

		public void Reload(StmModuleFilter newLayout = null)
		{
			if (newLayout != null)
			{
				layout = newLayout;
			}

			InvalidateCachedQuery();
			FilterBusinessObject?.Factory.ReloadAllSafe(new[] { layout });
			ReloadSelectedFilters(layout);
		}

		protected override void OnAddToActiveFiltersQueryCore()
		{
			base.OnAddToActiveFiltersQueryCore();

			if (ShouldSelectedFilterLayoutBeReloaded)
			{
				Reload();
				ShouldSelectedFilterLayoutBeReloaded = false;
			}
		}

		public bool ShouldSelectedFilterLayoutBeReloaded { get; set; }

		#endregion

		#region Validation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new ModuleUserDefinedFilterValidation(this);
		}

		#endregion

		#region XML Serialisation

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString(XmlLayoutNameElement, layoutName);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == XmlLayoutNameElement)
			{
				layoutName = reader.ReadElementString(XmlLayoutNameElement);

				if (layout == null || layout.S9_FilterName != layoutName)
				{
					layout = FilterBusinessObject.FindLayout(layoutName);
				}
			}
			else
			{
				// support legacy layouts here, because a transform on StmModuleFilter is too expensive, ever.
				ClearSelectedFilters();
				base.DeserializePropertiesFromXml(reader);
				ReloadSelectedFilters(layout);
			}

			SelectedFiltersDescriptionInfo.RefreshBinding();
		}

		const string XmlLayoutNameElement = "LayoutName";

		#endregion
	}
	public class ModuleUserDefinedFilterValidation : ModuleGuidFilterValidation
	{
		public ModuleUserDefinedFilterValidation(ModuleUserDefinedFilter parent)
			: base(parent)
		{
		}

		#region Validate All

		protected override void CheckSelectedFiltersDescription()
		{
			base.CheckSelectedFiltersDescription();

			var parent = (ModuleUserDefinedFilter)Parent;

			if (Parent.FilterBusinessObject.IsInFilterRuleMode && parent.IsPublishedForSpecificCompany)
			{
				var message = Res.GetString("d1c52a15-3005-44a9-8993-10b3e8ef0942", "{0}: User-defined filters must be published for all companies in order for them to be used for filter rules.", parent.HumanReadableShortcutName);
				parent.SelectedFiltersDescriptionInfo.AddError(message);
			}
		}

		public override Type AutoValidationType => GetType();
	}
	#endregion
}
