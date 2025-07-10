using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	public class ImportWizardFactory
	{
		static readonly string contextPrefix = "DIW:"; // Programmatic constant

		public ImportWizard New(IImportCollectionInfo collectionInfo, string contextKey)
		{
			return New(collectionInfo, new StmModuleFilterSettingsStorage(contextPrefix, contextKey), new FileMapper());
		}

		public virtual ImportWizard New(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
		{
			ImportWizard result = null;
			if (collectionInfo.Collection is IImportWizardProvider provider)
			{
				result = provider.GetImportWizard(collectionInfo, settingsStorage, fileMapper);
			}
			else
			{
				result = new ImportWizard(collectionInfo, settingsStorage, fileMapper);
			}

			return result;
		}
	}
}
