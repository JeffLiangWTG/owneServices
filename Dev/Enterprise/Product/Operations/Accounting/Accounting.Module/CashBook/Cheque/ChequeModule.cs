using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class ChequeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Cheque;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowView => false;

		public override bool AllowDelete => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Cheque);

		protected override IBusinessObjectCollection GetNewGridCollection() => new AccReceivedChequeCollection(Factory);

		protected override IFilterControl GetNewFilterControl() => new ChequeFilterControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() =>  new ChequeFilterBusinessObject();
	}
}
