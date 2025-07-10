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
	public class TempStoragePremisesController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.EU.TempStoragePremises;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.TempStoragePremises;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageRegPremises);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.EUTempStoragePremises;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.EUTempStoragePremises;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.EUTempStoragePremises;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.EUTempStoragePremises;

		protected override IZForm GetForm(IBusiness businessEntity) => new TempStoragePremisesForm((CusTempStorageRegPremises)businessEntity);
	}
}
