using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.Module
{
	public class TemporaryStorageController : EU.TemporaryStorage.Module.TemporaryStorageController
	{
		public TemporaryStorageController()
		{
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageJobHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new CusTempStorageForm((CusTempStorageJobHeader)businessEntity);

		#region New

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return CusTempStorageJobHeader.New(Factory);
		}

		#endregion
	}
}
