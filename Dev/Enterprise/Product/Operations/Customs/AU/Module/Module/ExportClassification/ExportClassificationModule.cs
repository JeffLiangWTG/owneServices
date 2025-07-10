using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	/// <summary>
	/// Module for ExportClassification.
	/// </summary>
	public class ExportClassificationModule : Customs.Module.ExportClassificationModule
	{
		public ExportClassificationModule()
		{
			AddImportFromCSVDataMenuItem(CreateImportFromCSVForm, false);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AUExportClassificationFilterControl(GridCollection, (AUExportClassificationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ExportClassificationCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AUExportClassificationFilterBusinessObject();
		}

		KForm CreateImportFromCSVForm()
		{
			return new ImportClassificationsFromCSVForm();
		}
	}
}
