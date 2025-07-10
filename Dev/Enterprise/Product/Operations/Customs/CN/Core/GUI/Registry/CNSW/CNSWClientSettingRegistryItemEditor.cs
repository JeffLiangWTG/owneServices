using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.GUI
{
	public class CNSWClientSettingRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public CNSWClientSettingRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new CNSWClientSettingRegistryItemUserControl();

		protected override EditorPaneAnchor Anchor => EditorPaneAnchor.TopLeftRight;
	}
}
