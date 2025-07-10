using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(TransportDocumentProvider))]
	sealed class TransportDocumentProviderTest : DocumentProviderAbstractTest<TransportDocumentProvider>
	{
		protected override string SubType => "TRA";

		public void TestSequenceNumber()
		{
			info.CSI_LineNo = 1;
			AssertEquals(1, Provider.SequenceNumber);
		}
	}
}
