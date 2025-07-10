using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ZArchitecture.Business
{
	public interface IImportWizardProvider
	{
		ImportWizard GetImportWizard(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper);
	}
}
