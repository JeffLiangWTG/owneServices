using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public class TemporaryStorageController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.TemporaryStorage;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.TemporaryStorage;

		public override System.Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageJobHeader);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ZForm(businessEntity);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CustomsTemporaryStorage;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.CustomsTemporaryStorage;

		#endregion
	}
}
