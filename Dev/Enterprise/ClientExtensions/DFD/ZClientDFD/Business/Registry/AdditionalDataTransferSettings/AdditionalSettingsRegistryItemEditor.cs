using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.DFD.Registry
{
	internal class AdditionalSettingsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public AdditionalSettingsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new AdditionalSettingsRegistryControl();
		}
	}
}
