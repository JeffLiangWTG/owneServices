using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module;

public class SumARegisterModule : ZFilterGridModule
{
	public const string SumARegisterAppCode = "SUM";

	public override ModuleIdentifier ID => ModuleIDs.Customs.SumARegister;

	protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.SumARegister);

	protected override FilterBusinessObject GetNewFilterBusinessObject() => new SumARegisterFilterBusinessObject();

	protected override IFilterControl GetNewFilterControl() => new SumARegisterFilterStripControl(GridCollection, (SumARegisterFilterBusinessObject)FilterBusinessObject);

	protected override IBusinessObjectCollection GetNewGridCollection() => new CusTempStorageRegHeaderCollection<CusTempStorageRegHeader>(Factory, SumARegisterAppCode);

	public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.CustomsTemporaryStorage;

	protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;
}
