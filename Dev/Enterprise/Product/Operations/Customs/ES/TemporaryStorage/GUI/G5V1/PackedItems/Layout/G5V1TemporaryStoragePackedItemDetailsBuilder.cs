using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.TemporaryStorage.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class G5V1TemporaryStoragePackedItemDetailsBuilder<T> : UCC6TemporaryStoragePackedItemDetailsBuilder<T>
		where T : TemporaryStorageHeader
	{
		protected override int MaxColumns => 3;
	}
}
