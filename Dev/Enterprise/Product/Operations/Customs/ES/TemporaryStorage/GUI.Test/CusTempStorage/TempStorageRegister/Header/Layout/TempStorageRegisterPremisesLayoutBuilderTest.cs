using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStorageRegisterPremisesLayoutBuilder<CusTempStorageRegPremises>))]
	class TempStorageRegisterPremisesLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TempStorageRegisterPremisesLayoutBuilder<CusTempStorageRegPremises>, CusTempStorageRegPremises, TempStorageRegisterPremisesControlBag>
	{
		protected override TempStorageRegisterPremisesLayoutBuilder<CusTempStorageRegPremises> GetColumnLayoutBuilderForTesting() => new TempStorageRegisterPremisesLayoutBuilder<CusTempStorageRegPremises>();

		protected override int ExpectedMaxColumns => 1;
	}
}
