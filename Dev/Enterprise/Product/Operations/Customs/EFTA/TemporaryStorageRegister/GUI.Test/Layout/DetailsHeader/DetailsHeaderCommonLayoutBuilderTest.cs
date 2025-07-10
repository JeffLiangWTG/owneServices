using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(DetailsHeaderCommonLayoutBuilder<CusTempStorageRegHeader>))]
sealed class DetailsHeaderCommonLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<DetailsHeaderCommonLayoutBuilder<CusTempStorageRegHeader>, CusTempStorageRegHeader, DetailsHeaderControlBag>
{
	protected override int ExpectedMaxColumns => 3;

	protected override DetailsHeaderCommonLayoutBuilder<CusTempStorageRegHeader> GetColumnLayoutBuilderForTesting()
	{
		return new DetailsHeaderCommonLayoutBuilder<CusTempStorageRegHeader>();
	}
}
