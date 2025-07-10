using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public class TempStoragePremisesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.TempStoragePremises;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.EUTempStoragePremises;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.EU.TempStoragePremises);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TempStoragePremisesFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new TempStoragePremisesFilterStripControl(GridCollection, (TempStoragePremisesFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusTempStorageRegPremisesCollection(Factory);

		public override bool AllowDelete => true;

		public override bool AllowUniversalCopy => false;

		public override bool AllowView => false;
	}
}
