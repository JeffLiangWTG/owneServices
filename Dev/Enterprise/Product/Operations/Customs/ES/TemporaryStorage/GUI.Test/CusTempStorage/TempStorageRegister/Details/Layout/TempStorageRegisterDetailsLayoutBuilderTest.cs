using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStorageRegisterDetailsLayoutBuilder<CusTempStorageRegHeader>))]
	public class TempStorageRegisterDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TempStorageRegisterDetailsLayoutBuilder<CusTempStorageRegHeader>, CusTempStorageRegHeader, TempStorageRegisterDetailsControlBag>
	{
		protected override TempStorageRegisterDetailsLayoutBuilder<CusTempStorageRegHeader> GetColumnLayoutBuilderForTesting() => new ();

		protected override int ExpectedMaxColumns => 3;
	}
}
