using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.GUI;

namespace Enterprise.DocumentEngineCore.Registry.GUI
{
	public partial class DocumentSigningServiceCredentialsConfigurationControl : RegistryBusinessObjectTemplateZUserControl
	{
		public DocumentSigningServiceCredentialsConfigurationControl()
		{
			InitializeComponent();
		}

		public DocumentSigningServiceCredentialsConfigurationControl(DocumentSigningServiceCredentialsConfigurationRegistryDataType dataType)
		{
			InitializeComponent();
			InitializeCaptions(dataType);
			ProviderCodeDropEdit.Visible = false;
		}

		void InitializeCaptions(DocumentSigningServiceCredentialsConfigurationRegistryDataType dataType)
		{
			if (!string.IsNullOrEmpty(dataType?.AccessKeyCaption))
			{
				AccessKeyTextBox.CaptionResourceString = new ResourceStringData("", dataType.AccessKeyCaption);
			}
			if (!string.IsNullOrEmpty(dataType?.KeyIDCaption))
			{
				KeyIDTextBox.CaptionResourceString = new ResourceStringData("", dataType.KeyIDCaption);
			}
		}
	}
}
