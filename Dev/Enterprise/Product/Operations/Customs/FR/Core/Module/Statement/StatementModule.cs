using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.FR.Module
{
	public class StatementModule : ZFilterGridModule
	{
		public StatementModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.FR.CustomsStatement;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.FRCustomsStatement;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CustomsStatement);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusStatementHeaderCollection(Factory);

		protected override IFilterControl GetNewFilterControl() => new StatementFilterControl(GridCollection, FilterBusinessObject);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new StatementFilterStripBusinessObject();

		public override bool AllowNew => true;

		public override bool AllowView => true;

		public override bool AllowEdit => true;

		public override bool AllowDelete => false;
	}
}
