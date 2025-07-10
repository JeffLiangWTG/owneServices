using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class HTSClassificationModule : ImportClassificationModule
	{
		public HTSClassificationModule()
		{
			AddImportFromCSVDataMenuItem(new CreateFormHandler(CreateImportFromCSVForm));
		}

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.HTSClassification;

		internal KForm CreateImportFromCSVForm() => new GUI.ImportClassificationsFromCSVForm();

		public override ModuleIdentifier ID => ModuleIDs.Customs.CA.HTSClassification;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CA.CACusClassification);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusClassificationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CusClassificationFilterControl(GridCollection, (CusClassificationFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new HTSClassificationCollection(Factory);

		protected override bool ShowRecentItemsCore() => true;
	}
}
