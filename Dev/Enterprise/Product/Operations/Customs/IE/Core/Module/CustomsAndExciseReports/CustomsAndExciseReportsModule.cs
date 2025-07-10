using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.IE.Module
{
	public class CustomsAndExciseReportsModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.IE.CustomsAndExciseReports;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsFiles;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.IE.CustomsAndExciseReports);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CustomsAndExciseReportsFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CustomsAndExciseReportsFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CustomsAndExciseReportOutboundMessageCollection(Factory);
	}
}
