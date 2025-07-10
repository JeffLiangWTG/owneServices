using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public class TagRuleFilterRuleFilterStripControl : FilterRuleFilterStripControl
	{
		public TagRuleFilterRuleFilterStripControl(FilterStripBusinessObject filterStrip, string newFilterStripString, string filterControlIdentifier)
			: base(filterStrip, newFilterStripString, filterControlIdentifier)
		{
		}

		protected override void OnPreviewClicked(string dropDownCode = null)
		{
			try
			{
				base.OnPreviewClicked(dropDownCode);
			}
			catch (InvalidFilterConfigurationException ex)
			{
				var message = Res.GetString("2567571e-0ced-403e-b3ad-22646cb80b42",
					@"Unable to build a database query with the provided filter strips in the context of the Branch and Department specified. Please ensure that any filter strip modules are available in the specified context.
Filter: {0}
Module: {1}", ex.FilterMultilingualDescription, ex.ModuleName);
				Globals.Message.ShowError(message, Res.GetString("ac8844ee-2364-46a3-b45f-e779c337bbae", "Invalid Filter Strips"));
			}
		}
	}
}
