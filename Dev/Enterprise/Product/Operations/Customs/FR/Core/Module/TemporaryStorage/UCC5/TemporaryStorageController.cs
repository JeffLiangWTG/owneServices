using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.GUI.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	public class TemporaryStorageController : EU.TemporaryStorage.Module.TemporaryStorageController
	{
		public TemporaryStorageController()
			: this(FRConstants.TemporaryStorage.AppCodeIST)
		{
		}

		public TemporaryStorageController(ZString applicationCode)
		{
			this.applicationCode = applicationCode;
		}

		readonly ZString applicationCode;

		public override Type TypeOfTopLevelBusinessObject => typeof(CusTempStorageJobHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new CusTempStorageForm((CusTempStorageJobHeader)businessEntity);

		#region New

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return CusTempStorageJobHeader.New(Factory, applicationCode);
		}

		#endregion
	}
}
