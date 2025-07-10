using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public class ConsolidatedBillingSettingsRegistryEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public ConsolidatedBillingSettingsRegistryEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new ConsolidatedBillingSettingsRegistryUserControl();
		}
	}
}
