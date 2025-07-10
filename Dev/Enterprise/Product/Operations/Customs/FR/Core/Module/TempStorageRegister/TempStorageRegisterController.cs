using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.GUI.CusTempStorage;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	public class TempStorageRegisterController : EU.TemporaryStorage.Module.TempStorageRegisterController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageRegHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new TempStorageRegisterForm((CusTempStorageRegHeader)businessEntity);

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.FRTempStorageRegister;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.FRTempStorageRegister;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.FRTempStorageRegister;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.FRTempStorageRegister;
	}
}
