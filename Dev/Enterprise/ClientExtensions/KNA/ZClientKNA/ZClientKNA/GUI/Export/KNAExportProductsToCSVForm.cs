using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.KNA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.KNA.GUI
{
	public interface IDisplayResultsQuery
	{
		ZQuery GetDisplayQuery();
	}

	internal class KNAExportProductsToCSVForm : ExportProductsToCSVForm
	{
		readonly IDisplayResultsQuery queryProvider;

		public KNAExportProductsToCSVForm() { }

		public KNAExportProductsToCSVForm(IDisplayResultsQuery queryProvider)
		{
			this.queryProvider = queryProvider;
		}

		public override void SaveSpecificDataType(string dataToLoad)
		{
			try
			{
				KNAOrgSupplierPartDataSaver dataSaver = new KNAOrgSupplierPartDataSaver(queryProvider.GetDisplayQuery());
				dataSaver.ProgressChanged += new SaveProgressChangedEventHandler(DataSaver_ProgressChanged);
				dataSaver.LogUpdated += new SaveLogUpdatedEventHandler(DataSaver_LogUpdated);
				dataSaver.ExportProductData(dataToLoad);
			}
			catch (System.NotSupportedException)
			{
				Globals.Message.Show(Res.GetString("1450a79d-58c7-4094-b3a6-5141faa2b130", "Product data import from KNA CSV file has not been implemented for your country."));
			}
		}
	}
}
