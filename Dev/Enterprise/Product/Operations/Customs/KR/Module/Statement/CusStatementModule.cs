using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Module
{
	public class CusStatementModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.KR.CustomsStatement;
		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.KRCustomsStatement;
		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.ImportBroker;
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.CustomsStatement);
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusStatementFilterStripBusinessObject();
		protected override IFilterControl GetNewFilterControl() => new CusStatementFilterControl(GridCollection, FilterBusinessObject);
		protected override IBusinessObjectCollection GetNewGridCollection() => new CusStatementHeaderCollection(Factory);
		public override bool AllowNew => false;
		public override bool AllowDelete => false;
		public override bool AllowEdit => true;
	}
}
