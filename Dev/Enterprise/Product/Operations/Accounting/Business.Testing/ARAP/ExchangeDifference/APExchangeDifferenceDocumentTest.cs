using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APExchangeDifference))]
	public class APExchangeDifferenceDocumentTest : ExchangeDifferenceDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<APExchangeDifference>();
		}
	}
}
