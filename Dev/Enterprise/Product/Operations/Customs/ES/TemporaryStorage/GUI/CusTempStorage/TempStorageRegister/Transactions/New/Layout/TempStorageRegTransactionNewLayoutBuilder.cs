using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI;

public class TempStorageRegTransactionNewLayoutBuilder<T> : ColumnLayoutBuilder<T, TempStorageRegTransactionNewControlBag> where T : CusTempStorageRegLineTransactionFormEditable
{
	public override TempStorageRegTransactionNewControlBag CommonBag => TempStorageRegTransactionNewControlBag.Instance;

	protected override int MaxColumns => 1;
}
