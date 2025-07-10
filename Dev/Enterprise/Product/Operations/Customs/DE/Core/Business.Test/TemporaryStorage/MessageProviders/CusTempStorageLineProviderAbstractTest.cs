using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(CusTempStorageLineProvider))]
	public abstract class CusTempStorageLineProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : CusTempStorageLineProvider
	{
		protected CusTempStorageLine TempStorageLine => tempStorageLine ?? (tempStorageLine = GetTempStorageLineToTest());
		CusTempStorageLine tempStorageLine;

		protected CusTempStorageLineProvider TempStorageLineWrapped => tempStorageLineWrapped ?? (tempStorageLineWrapped = GetTempStorageLineWrapped());
		CusTempStorageLineProvider tempStorageLineWrapped;

		protected abstract CusTempStorageLine GetTempStorageLineToTest();

		protected abstract T GetTempStorageLineWrapped();

		protected override T GetProvider() => (T)TempStorageLineWrapped;
	}
}
