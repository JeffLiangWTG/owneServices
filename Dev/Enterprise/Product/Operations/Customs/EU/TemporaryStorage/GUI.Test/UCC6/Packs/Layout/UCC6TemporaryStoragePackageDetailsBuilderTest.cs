using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStoragePackageDetailsBuilder))]
	public class UCC6TemporaryStoragePackageDetailsBuilderTest : ColumnLayoutBuilderAbstractTest<UCC6TemporaryStoragePackageDetailsBuilder, TemporaryStorageHeader, UCC6TemporaryStoragePackageDetailsControlBag>
	{
		protected override UCC6TemporaryStoragePackageDetailsBuilder GetColumnLayoutBuilderForTesting() => new UCC6TemporaryStoragePackageDetailsBuilder();

		protected override int ExpectedMaxColumns => 1;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
