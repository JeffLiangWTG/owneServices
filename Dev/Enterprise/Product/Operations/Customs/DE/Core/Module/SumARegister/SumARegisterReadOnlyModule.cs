using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Module
{
	public class SumARegisterReadOnlyModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.DE.SumARegisterReadOnly;

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsTemporaryStorage;

		public override bool AllowNew => false;

		public override bool AllowEdit => false;

		public override bool AllowDelete => false;

		public override bool AllowView => true;

		public override bool AllowUniversalCopy => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.DE.SumARegisterReadOnly);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new SumARegisterFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new SumARegisterFilterStripControl(GridCollection, (SumARegisterFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new CusTempStorageRegHeaderCollection<Business.CusTempStorage.CusTempStorageRegHeader>(Factory, SumARegisterAppCode);

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		const string SumARegisterAppCode = "SUM";
	}
}
