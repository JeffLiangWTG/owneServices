using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public class CusClassificationModule : Customs.Module.SingleTariffClassificationModule
	{
		public CusClassificationModule()
		{
			AddImportFromCSVDataMenuItem(new CreateFormHandler(CreateImportFromCSVForm));
		}

		protected KForm CreateImportFromCSVForm()
		{
			return new GUI.ImportClassificationsFromCSVForm();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new CusClassificationFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusClassificationFilterControl(GridCollection, (CusClassificationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new Customs.Business.BaseClassificationCollection<CusClassification>(Factory);
		}
	}
}
