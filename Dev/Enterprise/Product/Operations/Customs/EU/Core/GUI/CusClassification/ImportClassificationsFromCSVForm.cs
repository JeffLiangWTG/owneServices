using Enterprise.Customs.EU.Business.Classification;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.GUI
{
	public class ImportClassificationsFromCSVForm : DataLoaderForm
	{
		protected override DataLoad GetNewDataLoader()
		{
			return new ClassificationDataLoad();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			((ClassificationDataLoad)dataLoader).ImportData(dataToLoad, (NoResString)"row");
		}
	}
}
