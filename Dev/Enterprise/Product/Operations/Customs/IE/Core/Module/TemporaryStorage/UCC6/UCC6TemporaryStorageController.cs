using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.Module
{
	public sealed class UCC6TemporaryStorageController : EU.TemporaryStorage.Module.UCC6TemporaryStorageController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => new TemporaryStorageForm((TemporaryStorageHeader)businessEntity);
	}
}
