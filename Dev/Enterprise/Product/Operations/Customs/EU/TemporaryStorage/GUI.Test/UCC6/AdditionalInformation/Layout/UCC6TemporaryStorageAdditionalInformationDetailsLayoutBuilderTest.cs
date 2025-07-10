using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageAdditionalInformationDetailsLayoutBuilder))]

	public class UCC6TemporaryStorageAdditionalInformationDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<UCC6TemporaryStorageAdditionalInformationDetailsLayoutBuilder, TemporaryStorageBill, UCC6TemporaryStorageAdditionalInformationDetailsControlBag>
	{
		protected override UCC6TemporaryStorageAdditionalInformationDetailsLayoutBuilder GetColumnLayoutBuilderForTesting() => new UCC6TemporaryStorageAdditionalInformationDetailsLayoutBuilder();

		protected override int ExpectedMaxColumns => 1;
		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
