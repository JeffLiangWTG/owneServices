using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ChequeTransaction;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ChequeTransactionModule : ZFilterGridModule
	{
		public ChequeTransactionModule()
		{
		}

		public override ModuleIdentifier ID => ModuleIDs.ChequeTransaction;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowView => false;

		public override bool AllowDelete => false;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		protected override IBusinessObjectCollection GetNewGridCollection() => new ChequeTransactionHeaderCollection(Factory);

		protected override IFilterControl GetNewFilterControl() => new ChequeTransactionFilterStripControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ChequeTransactionFilterStripBusinessObject();

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => new ChequeTransactionController();
	}
}
