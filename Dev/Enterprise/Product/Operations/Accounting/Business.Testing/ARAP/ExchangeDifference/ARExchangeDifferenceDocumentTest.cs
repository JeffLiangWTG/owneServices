using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARExchangeDifference))]
	public class ARExchangeDifferenceDocumentTest : ExchangeDifferenceDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<ARExchangeDifference>();
		}
	}
}
