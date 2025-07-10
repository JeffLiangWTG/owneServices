using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class SystemToSystemCertificateRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public SystemToSystemCertificateRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new SystemToSystemCertificateControl();
		}

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.All;
	}
}
