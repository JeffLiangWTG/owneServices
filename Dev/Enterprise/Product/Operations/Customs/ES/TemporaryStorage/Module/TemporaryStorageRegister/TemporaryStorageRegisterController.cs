using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ES.TemporaryStorage.Module
{
	public class TemporaryStorageRegisterController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.ES.TemporaryStorageRegister;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.ES.TemporaryStorageRegister;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageRegHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new GUI.TempStorageRegisterForm((CusTempStorageRegHeader)businessEntity);

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsTemporaryStorage;
	}
}
