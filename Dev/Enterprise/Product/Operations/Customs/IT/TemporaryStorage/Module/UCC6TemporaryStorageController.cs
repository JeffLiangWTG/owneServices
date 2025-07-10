using CargoWise.EntityFramework;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.Customs.IT.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

public sealed class UCC6TemporaryStorageController : EU.TemporaryStorage.Module.UCC6TemporaryStorageController
{
	protected override IZForm GetForm(IBusiness businessEntity) => new TemporaryStorageForm((TemporaryStorageHeader)businessEntity);
}
