using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CDSCashPaymentsModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.GB.CDSCashPayments;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.GBCDSCashPayments;

		public override bool AllowNew => false;
		public override bool AllowEdit => false;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.GB.CDSCashPaymentsController);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CDSCashPaymentsFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CDSCashPaymentsFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusEntryPayInfoCollection(Factory);
	}
}
