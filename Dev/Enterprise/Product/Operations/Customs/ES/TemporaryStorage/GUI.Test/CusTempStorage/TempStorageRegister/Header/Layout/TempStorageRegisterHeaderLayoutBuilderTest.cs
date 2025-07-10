using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStorageRegisterHeaderLayoutBuilder<CusTempStorageRegHeader>))]
	class TempStorageRegisterHeaderLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TempStorageRegisterHeaderLayoutBuilder<CusTempStorageRegHeader>, CusTempStorageRegHeader, TempStorageRegisterHeaderControlBag>
	{
		protected override TempStorageRegisterHeaderLayoutBuilder<CusTempStorageRegHeader> GetColumnLayoutBuilderForTesting() => new TempStorageRegisterHeaderLayoutBuilder<CusTempStorageRegHeader>();

		protected override int ExpectedMaxColumns => 2;
	}
}
