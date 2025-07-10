using System;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using GlowIndexQueryService.Business;

namespace Enterprise.BufferManagement.GUI
{
	public class TagRuleFilterStripWrapperControl : BMFilterStripWrapperControl
	{
		public override string ControlIdentifier => "TagRule";

		protected override CodeDescriptionPairList PreviewDropDownMenuItems
		{
			get
			{
				if (DataSource != null && ((TagRule)DataSource).TGR_ActionType == TagRuleActionTypeList.Codes.AddAndRemoveTag)
				{
					return new AddRemoveTagRulePreviewOptionsList();
				}

				return null;
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);

			if (DataSource != null)
			{
				RefreshPreviewButton(ModuleName);
				((TagRule)DataSource).TGR_ActionTypeInfo.ValueChanged += (sender, args) => RefreshPreviewButton(ModuleName);
			}
		}

		protected override FilterRuleFilterStripControl GetNewFilterStripControl()
		{
			var filterObject = GetModuleFilters();
			filterObject.SetGlowFiltersIfAllowed();

			var bmFilterRuleFilterBusinessObject = filterObject as IBMFilterRuleFilterBusinessObject;
			if (bmFilterRuleFilterBusinessObject != null)
			{
				bmFilterRuleFilterBusinessObject.FilterControlIdentifier = FilterControlIdentifier;
			}

			return new TagRuleFilterRuleFilterStripControl(filterObject, NewFilterStripString, ControlIdentifier);
		}

		public void ReloadFilters(SearchType type)
		{
			if (stripControl?.FilterBusinessObject != null)
			{
				stripControl.FilterBusinessObject.SearchType = type;
				stripControl.FilterBusinessObject.LoadLayout(null);
			}
		}

		public bool IndexUsagePermitted => stripControl?.FilterBusinessObject?.IndexSearchFields?.Status == GlowIndexQueryStatus.Success;
	}
}
