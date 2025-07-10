using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.GUI;

public class EdecBordereauConfigRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
{
	public EdecBordereauConfigRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
	{
	}

	protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new EdecBordereauConfigRegistryItemUserControl();
}
