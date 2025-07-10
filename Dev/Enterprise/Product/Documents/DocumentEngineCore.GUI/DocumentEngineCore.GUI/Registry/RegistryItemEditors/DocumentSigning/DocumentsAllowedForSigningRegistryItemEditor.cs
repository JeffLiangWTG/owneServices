using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public class DocumentsAllowedForSigningRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public DocumentsAllowedForSigningRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new DocumentsAllowedForSigningControl();
		}
	}
}
