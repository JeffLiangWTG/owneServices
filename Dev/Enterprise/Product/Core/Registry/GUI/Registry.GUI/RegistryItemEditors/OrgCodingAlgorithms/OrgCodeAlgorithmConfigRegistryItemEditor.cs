using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	class OrgCodeAlgorithmConfigRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public OrgCodeAlgorithmConfigRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected new OrgCodeAlgorithmRegistryDataType DataType
		{
			get { return (OrgCodeAlgorithmRegistryDataType)base.DataType; }
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new OrgCodeAlgorithmConfigControl(DataType.AlgorithmType);
		}
	}
}
