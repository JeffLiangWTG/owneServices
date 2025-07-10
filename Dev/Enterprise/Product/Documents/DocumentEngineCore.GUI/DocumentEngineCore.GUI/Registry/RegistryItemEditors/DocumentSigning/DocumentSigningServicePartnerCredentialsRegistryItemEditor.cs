using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry.GUI
{
	public class DocumentSigningServicePartnerCredentialsRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public DocumentSigningServicePartnerCredentialsRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new DocumentSigningServicePartnerCredentialsControl();
		}
	}
}
