using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.GUI;

public class InvoiceAmountBoundariesRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
{
	public InvoiceAmountBoundariesRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(dataType, fallbackLevel, factory)
	{
	}

	protected override RegistryZUserControl NewBoundWinFormsEditorPane()
	{
		return new InvoiceAmountBoundariesControl();
	}

	protected override EditorPaneAnchor Anchor
	{
		get { return EditorPaneAnchor.All; }
	}
}
