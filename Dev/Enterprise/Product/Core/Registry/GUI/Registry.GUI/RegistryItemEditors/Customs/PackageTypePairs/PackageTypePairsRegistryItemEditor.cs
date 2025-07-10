using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public abstract class PackageTypePairsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public PackageTypePairsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected sealed override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			var packageTypesControl = new PackageTypePairsControl();
			SetDataSourceType(packageTypesControl);
			return packageTypesControl;
		}

		protected abstract void SetDataSourceType(PackageTypePairsControl packageTypesControl);

		protected sealed override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
