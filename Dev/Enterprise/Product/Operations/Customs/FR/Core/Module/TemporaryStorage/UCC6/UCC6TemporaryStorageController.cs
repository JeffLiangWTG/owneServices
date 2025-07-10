using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.GUI.UCC6TemporaryStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	public class UCC6TemporaryStorageController : EU.TemporaryStorage.Module.UCC6TemporaryStorageController
	{
		public UCC6TemporaryStorageController()
			: this(FRConstants.TemporaryStorage.AppCodeIST)
		{
		}

		public UCC6TemporaryStorageController(ZString declarationType)
		{
			this.declarationType = declarationType;
		}

		readonly ZString declarationType;

		public override Type TypeOfTopLevelBusinessObject => typeof(TemporaryStorageHeader);

		protected override IZForm GetForm(IBusiness businessEntity) => new UCC6TemporaryStorageForm((TemporaryStorageHeader)businessEntity);

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return TemporaryStorageHeader.New(Factory, declarationType);
		}
	}
}
