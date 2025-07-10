using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(CusTempStorageRegLineItemDetailsLayoutBuilder<CusTempStorageRegHeader>))]
	public class CusTempStorageRegLineItemDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CusTempStorageRegLineItemDetailsLayoutBuilder<CusTempStorageRegHeader>, CusTempStorageRegHeader, CusTempStorageRegLineItemDetailsControlBag>
	{
		protected override CusTempStorageRegLineItemDetailsLayoutBuilder<CusTempStorageRegHeader> GetColumnLayoutBuilderForTesting() => new ();

		protected override int ExpectedMaxColumns => 2;
	}
}
