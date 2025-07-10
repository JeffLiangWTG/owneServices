using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.GUI.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	public class NewFromOthersTemporaryStorageController : EU.TemporaryStorage.Module.TemporaryStorageController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(TemporaryStorageWrapperFromParentHelper);

		protected override IZForm GetForm(IBusiness businessEntity) => new NewFromOtherForm(businessEntity as TemporaryStorageWrapperFromParentHelper);

		protected override IBusiness GetNewBusinessEntityInLocalFactory() => new TemporaryStorageWrapperFromParentHelper(Factory);
	}
}
