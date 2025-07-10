using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class VerboseLoggingRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public VerboseLoggingRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType,
				fallbackLevel,
				factory)
		{
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new VerboseLoggingControl();
		}
	}
}
