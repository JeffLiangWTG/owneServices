using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TempStorageRegTransactionNewLayoutBuilder<CusTempStorageRegLineTransactionFormEditable>))]
sealed class TempStorageRegTransactionNewLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TempStorageRegTransactionNewLayoutBuilder<CusTempStorageRegLineTransactionFormEditable>, CusTempStorageRegLineTransactionFormEditable, TempStorageRegTransactionNewControlBag>
{
	protected override TempStorageRegTransactionNewLayoutBuilder<CusTempStorageRegLineTransactionFormEditable> GetColumnLayoutBuilderForTesting() => new ();

	protected override int ExpectedMaxColumns => 1;
}
