using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public class ImportClassificationsFromCSVForm : DataLoaderForm
	{
		protected override DataLoad GetNewDataLoader()
		{
			return new ClassificationDataLoad();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			((ClassificationDataLoad)dataLoader).ImportData(dataToLoad, "row");
		}
	}
}
