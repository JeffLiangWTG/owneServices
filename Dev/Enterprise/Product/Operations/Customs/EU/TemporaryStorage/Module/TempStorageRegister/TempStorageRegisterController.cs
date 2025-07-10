using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public class TempStorageRegisterController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.EU.TempStorageRegister;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.TempStorageRegister;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageRegHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new TempStorageRegisterForm((CusTempStorageRegHeader)businessEntity);//Just a frame,will be done in another workflow

		protected override SecurityCheckpoint CheckPointForView => Env.Security.EUTempStorageRegister;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.EUTempStorageRegister;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.EUTempStorageRegister;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EUTempStorageRegister;
	}
}
