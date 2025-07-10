using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(SupportingDocumentDetailsBuilder))]
	sealed class SupportingDocumentDetailsBuilderTest : ColumnLayoutBuilderAbstractTest<SupportingDocumentDetailsBuilder, CusSupportingInfo, SupportingDocumentDetailsControlBag>
	{
		protected override SupportingDocumentDetailsBuilder GetColumnLayoutBuilderForTesting()
			 => new SupportingDocumentDetailsBuilder();

		protected override int ExpectedMaxColumns
			=> 2;

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth
			=> ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
