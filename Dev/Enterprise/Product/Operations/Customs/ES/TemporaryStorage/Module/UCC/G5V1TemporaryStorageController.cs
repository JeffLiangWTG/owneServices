using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.TemporaryStorage.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.Module
{
	public class G5V1TemporaryStorageController : EU.TemporaryStorage.Module.UCC6TemporaryStorageController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => new G5V1TemporaryStorageForm((TemporaryStorageHeader)businessEntity);
	}
}
