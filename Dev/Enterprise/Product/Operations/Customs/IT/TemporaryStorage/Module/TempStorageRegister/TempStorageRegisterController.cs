using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.Customs.IT.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public class TempStorageRegisterController : EU.TemporaryStorage.Module.TempStorageRegisterController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageRegHeader);

	protected override IZForm GetForm(IBusiness businessEntity) => new TempStorageRegisterForm((CusTempStorageRegHeader)businessEntity);
}
