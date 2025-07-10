using CargoWise.EntityFramework;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public class TempStorageRegisterModule : EU.TemporaryStorage.Module.TempStorageRegisterModule
{
	public override bool AllowNew => false;

	public override bool AllowEdit => false;

	public override bool AllowDelete => false;

	protected override IBusinessObjectCollection GetNewGridCollection()
	{
		return new CusTempStorageRegHeaderCollection(Factory);
	}

	protected override FilterBusinessObject GetNewFilterBusinessObject() => new TempStorageRegisterFilterBusinessObject();

	protected override IFilterControl GetNewFilterControl() => new TempStorageRegisterFilterStripControl(GridCollection, FilterBusinessObject);
}
