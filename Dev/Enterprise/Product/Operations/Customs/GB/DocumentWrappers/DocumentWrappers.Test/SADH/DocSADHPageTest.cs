using Enterprise.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.GB.DocumentWrappers.Testing
{
	[TestedType(typeof(DocSADHPage))]
	class DocSADHPageTest : Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHPageTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper() => DocSADHPage.New(Factory, null);
	}
}
