using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APContraRow))]
	public class APContraRowDocumentTest : ContraRowDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<APContraRow>();
		}
	}
}
