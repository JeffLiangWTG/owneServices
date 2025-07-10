using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(AdditionalDocumentLayoutBuilder<NctsAdditionalInfo>))]
	class AdditionalDocumentLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<AdditionalDocumentLayoutBuilder<NctsAdditionalInfo>, NctsAdditionalInfo, AdditionalDocumentControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override AdditionalDocumentLayoutBuilder<NctsAdditionalInfo> GetColumnLayoutBuilderForTesting() => new AdditionalDocumentLayoutBuilder<NctsAdditionalInfo>();
	}
}
