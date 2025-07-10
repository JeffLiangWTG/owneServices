using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Module
{
	public class SumARegisterReadOnlyController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.DE.SumARegisterReadOnly;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.DE.SumARegisterReadOnly;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageRegHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new SumARegisterForm((CusTempStorageRegHeader)businessEntity);

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsTemporaryStorage;

		#endregion
	}
}
