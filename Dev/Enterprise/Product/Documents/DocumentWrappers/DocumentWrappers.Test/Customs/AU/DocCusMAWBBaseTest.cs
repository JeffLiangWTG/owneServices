using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCusMAWBBase))]
	sealed class DocCusMAWBBaseTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			var result = DocCusMAWBBase.New(cusMAWB, Factory);
			return new DocumentWrapper[] { result };
		}
	}
}
