using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStoragePremisesDetailsLayoutBuilder<CusTempStorageRegPremises>))]
	sealed class TempStoragePremisesDetailsLayoutBuilderTest
		: ColumnLayoutBuilderAbstractTest<TempStoragePremisesDetailsLayoutBuilder<CusTempStorageRegPremises>, CusTempStorageRegPremises, TempStoragePremisesDetailsControlBag>
	{
		protected override TempStoragePremisesDetailsLayoutBuilder<CusTempStorageRegPremises> GetColumnLayoutBuilderForTesting() => new ();

		protected override int ExpectedMaxColumns => 1;
	}
}
