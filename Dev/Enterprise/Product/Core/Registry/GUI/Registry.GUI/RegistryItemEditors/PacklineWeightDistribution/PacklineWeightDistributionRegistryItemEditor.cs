using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class PacklineWeightDistributionRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public PacklineWeightDistributionRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new PacklineWeightDistributionControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.TopLeftRight; }
		}
	}
}
