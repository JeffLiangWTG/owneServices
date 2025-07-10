using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARTransferFromRow))]
	public class ARTransferFromRowDocumentTest : ARTransferRowDocumentTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<ARTransferFromRow>();
		}
	}
}
