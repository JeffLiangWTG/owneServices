using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.GUI;

public sealed class PassarSearchRequestConfigRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
{
	public PassarSearchRequestConfigRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(dataType, fallbackLevel, factory)
	{
	}

	protected override RegistryZUserControl NewBoundWinFormsEditorPane() => new PassarSearchRequestConfigRegistryItemUserControl();
}
