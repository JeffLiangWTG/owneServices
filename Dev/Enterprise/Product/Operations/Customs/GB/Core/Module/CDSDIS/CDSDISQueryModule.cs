using CargoWise.EntityFramework;
using Enterprise.Customs.GB.CDS;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CDSDISQueryModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.GB.CDSDISQuery;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsFiles;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Broker;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.GB.CDSDISQueryController);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CDSDISQueryFilterStripBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CDSDISQueryFilterStripControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CDSDISQueryMessageCollection(Factory);
	}
}
