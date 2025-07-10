using Enterprise.CommissionManagement.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI.DataMapping;

namespace Enterprise.CommissionManagement.Module
{
	public class CommissionImportWizardFactory : ImportWizardFactory
	{
		public override ImportWizard New(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
		{
			var commissionImportCollectionInfo = (CommissionImportCollectionInfo)collectionInfo;
			return new CommissionImportWizard(commissionImportCollectionInfo, settingsStorage, fileMapper);
		}
	}
}
