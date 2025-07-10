using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BE.GUI.Registry;

public class CustomsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
{
	public CustomsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(dataType, fallbackLevel, factory)
	{
	}

	protected override EditorPaneAnchor Anchor => EditorPaneAnchor.TopLeftRight;

	protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new CustomsRegistryItemControl();
}
