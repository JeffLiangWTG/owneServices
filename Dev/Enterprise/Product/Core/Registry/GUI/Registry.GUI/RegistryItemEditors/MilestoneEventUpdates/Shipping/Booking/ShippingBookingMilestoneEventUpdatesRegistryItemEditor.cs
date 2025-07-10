using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	class ShippingBookingMilestoneEventUpdatesRegistryItemEditor : MilestoneEventUpdatesRegistryItemEditor
	{
		public ShippingBookingMilestoneEventUpdatesRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ShippingBookingMilestoneEventUpdatesRegistryControl();
		}
	}
}
