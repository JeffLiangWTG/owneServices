using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class OpportunityRegistryCaption
	{
		public static ResourceString GetPotentialLabelCaption()
		{
			var oppCaptionRegistry = OverridableNewDelegate.Value?.Invoke() ?? new OpportunityRegistryCaption();
			var caption = oppCaptionRegistry.PotentialLabel;

			return caption;
		}

		public static ResourceString GetCurrentLabelCaption()
		{
			var oppCaptionRegistry = OverridableNewDelegate.Value?.Invoke() ?? new OpportunityRegistryCaption();
			var caption = oppCaptionRegistry.CurrentLabel;

			return caption;
		}

		protected delegate OpportunityRegistryCaption NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected virtual ResourceString PotentialLabel => ResString.GetMultilingualString("D6A99F4D-7CD0-43EC-AE04-9C299C9D1427", "Potential");
		protected virtual ResourceString CurrentLabel => ResString.GetMultilingualString("d365fde3-f2e4-4611-a015-1110d766b452", "Current");
	}
}
