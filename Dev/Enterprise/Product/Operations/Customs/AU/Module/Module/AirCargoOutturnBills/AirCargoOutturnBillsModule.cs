using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class AirCargoOutturnBillsModule : CMRModule
	{
		public override bool AllowNew => true;

		public override ModuleIdentifier ID => ModuleIDs.Customs.AU.AirCargoOutturnBills;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.ACAOutturnBills;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => JobInvoicingConsumerTypes.CusUnderbond.Code;

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new AirCargoOutturnBillsFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new AirCargoOutturnBillsFilterControl(GridCollection, FilterBusinessObject);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.AU.AirCargoOutturnBillsController);

		protected override IBusinessObjectCollection GetNewGridCollection() => new AirOrStandAloneCusUnderbondCollection(Factory);

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AirCargoReport;
	}
}
