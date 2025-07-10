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
	public class SumARegisterController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.DE.SumARegister;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.EU.DE.SumARegister;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageRegHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new SumARegisterForm((CusTempStorageRegHeader)businessEntity);

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.DESumARegister;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.DESumARegister;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.DESumARegister;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.DESumARegister;

		#endregion
	}
}
