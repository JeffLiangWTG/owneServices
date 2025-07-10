using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.GUI;

public class ArrivalCustomerReferenceFormatConfigRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
{
	public ArrivalCustomerReferenceFormatConfigRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
	{
	}

	protected override RegistryZUserControl NewBoundWinFormsEditorPane()
	{
		return new ArrivalCustomerReferenceFormatConfigControl();
	}

	protected override EditorPaneAnchor Anchor
	{
		get { return EditorPaneAnchor.All; }
	}
}
