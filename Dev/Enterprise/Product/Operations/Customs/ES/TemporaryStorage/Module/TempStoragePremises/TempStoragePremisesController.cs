using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ES.TemporaryStorage.Module
{
	public class TempStoragePremisesController : EU.TemporaryStorage.Module.TempStoragePremisesController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.TempStoragePremises;

		public override Type TypeOfTopLevelBusinessObject => typeof(Business.CusTempStorageRegPremises);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsTemporaryStorage;

		protected override IZForm GetForm(IBusiness businessEntity) => new EU.TemporaryStorage.GUI.TempStoragePremisesForm((Business.CusTempStorageRegPremises)businessEntity);
	}
}
