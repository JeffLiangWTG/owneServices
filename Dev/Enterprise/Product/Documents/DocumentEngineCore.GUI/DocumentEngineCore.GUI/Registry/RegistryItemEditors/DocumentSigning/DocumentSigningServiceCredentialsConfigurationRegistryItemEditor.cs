using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry.GUI
{
	public class DocumentSigningServiceCredentialsConfigurationRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public DocumentSigningServiceCredentialsConfigurationRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
			if (!typeof(DocumentSigningServiceCredentialsWithProviderRegistryDataType).IsAssignableFrom(dataType.GetType()))
			{
				this.dataType = (DocumentSigningServiceCredentialsConfigurationRegistryDataType)dataType;
			}
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			if (dataType != null)
			{
				return new DocumentSigningServiceCredentialsConfigurationControl(dataType);
			}

			return new DocumentSigningServiceCredentialsConfigurationControl();
		}

		readonly DocumentSigningServiceCredentialsConfigurationRegistryDataType dataType;
	}
}
