using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using CusTempStorageRegHeader = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader;

namespace Enterprise.Customs.ES.TemporaryStorage.Module
{
	public class TemporaryStorageRegisterModule : EU.TemporaryStorage.Module.TempStorageRegisterModule
	{
		public const string AppCode = "ADT";

		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.ES.TemporaryStorageRegister;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.ES.TemporaryStorageRegister);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new TemporaryStorageRegisterFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new TemporaryStorageRegisterFilterStripControl(GridCollection, (TemporaryStorageRegisterFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, AppCode);

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsTemporaryStorage;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowNew => false;

		public override bool AllowEdit => true;

		public override bool AllowDelete => false;

		public override bool AllowUniversalCopy => false;
	}
}
