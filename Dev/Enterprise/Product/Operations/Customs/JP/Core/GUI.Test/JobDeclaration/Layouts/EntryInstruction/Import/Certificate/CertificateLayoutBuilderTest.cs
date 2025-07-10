using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(CertificateLayoutBuilder))]
	sealed class CertificateLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<CertificateLayoutBuilder, CusEntryInstruction, CertificateControlBag>
	{
		protected override CertificateLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new CertificateLayoutBuilder();
		}

		protected override ColumnLayoutBuilderCaptionWidthSize ExpectedCaptionWidth => ColumnLayoutBuilderCaptionWidthSize.Long + 80;
	}
}
