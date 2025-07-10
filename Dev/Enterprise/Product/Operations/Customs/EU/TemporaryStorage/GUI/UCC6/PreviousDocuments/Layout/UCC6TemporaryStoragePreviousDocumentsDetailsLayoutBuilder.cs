using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStoragePreviousDocumentsDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, UCC6TemporaryStoragePreviousDocumentsDetailsControlBag>
		where T : TemporaryStoragePreviousDocument
	{
		public override UCC6TemporaryStoragePreviousDocumentsDetailsControlBag CommonBag => UCC6TemporaryStoragePreviousDocumentsDetailsControlBag.Instance;
	}
}
