using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class ImportClassificationsFromCSVForm : DataLoaderForm
	{
		public override bool ConfirmLoadData()
		{
			string loadingData = "Please Note: Only classifications with valid tariff details will be loaded.";
			DialogResult result = Globals.Message.Show(loadingData, "Confirm Classification Lookup Load", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			return (result == DialogResult.OK);
		}

		public override string FormHeading
		{
			get { return "Import Classification Lookup Data"; }
		}

		protected override DataLoad GetNewDataLoader()
		{
			return new ClassificationDataLoad();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			((ClassificationDataLoad)dataLoader).ImportClassificationData(dataToLoad);
		}
	}
}
