using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(LinesDetailsCommonLayoutBuilder<CusTempStorageRegHeader>))]
sealed class LinesDetailsCommonLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<LinesDetailsCommonLayoutBuilder<CusTempStorageRegHeader>, CusTempStorageRegHeader, LinesDetailsControlBag>
{
	protected override int ExpectedMaxColumns => 2;

	protected override LinesDetailsCommonLayoutBuilder<CusTempStorageRegHeader> GetColumnLayoutBuilderForTesting()
	{
		return new LinesDetailsCommonLayoutBuilder<CusTempStorageRegHeader>();
	}
}
