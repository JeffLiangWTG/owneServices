using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(UnipassCertificateLayoutBuilder))]
	sealed class UnipassCertificateLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<UnipassCertificateLayoutBuilder, GlbCompanyWrapper, UnipassCertificateControlBag>
	{
		protected override UnipassCertificateLayoutBuilder GetColumnLayoutBuilderForTesting() => new UnipassCertificateLayoutBuilder();
		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long;
	}
}
