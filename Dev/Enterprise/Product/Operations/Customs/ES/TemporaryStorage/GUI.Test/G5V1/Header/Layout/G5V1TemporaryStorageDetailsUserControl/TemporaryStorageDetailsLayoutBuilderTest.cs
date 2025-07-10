using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TemporaryStorageDetailsLayoutBuilder<TemporaryStorageHeader>))]
	public sealed class TemporaryStorageDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<TemporaryStorageDetailsLayoutBuilder<TemporaryStorageHeader>, TemporaryStorageHeader, G5V1TemporaryStorageDetailsUserControlBag>
	{
		protected override TemporaryStorageDetailsLayoutBuilder<TemporaryStorageHeader> GetColumnLayoutBuilderForTesting() => new TemporaryStorageDetailsLayoutBuilder<TemporaryStorageHeader>();

		protected override int ExpectedMaxColumns => 2;
	}
}
