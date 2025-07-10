using System;
using CargoWise.Common;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Helpers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class AwbMigrationDataLoadForm : DataLoaderForm
	{
		public AwbMigrationDataLoadForm()
		{
		}

		protected override DataLoad GetNewDataLoader()
		{
			return new AwbMigrationDataLoad();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			try
			{
				((AwbMigrationDataLoad)dataLoader).ImportData(dataToLoad, "AWB");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.Show(Res.GetString("12345678-e597-447f-80c0-4d8a983082d2", "Air Waybill data import from CSV file failed."));
			}
		}
	}
}
