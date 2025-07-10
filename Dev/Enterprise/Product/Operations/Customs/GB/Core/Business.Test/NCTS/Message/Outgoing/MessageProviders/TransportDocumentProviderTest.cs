using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(TransportDocumentProvider))]
	class TransportDocumentProviderTest : DocumentProviderAbstractTest<TransportDocumentProvider>
	{
		public void TestSequenceNumber()
		{
			info.CSI_LineNo = 1;
			AssertEquals(1, Provider.SequenceNumber);
		}

		protected override string SubType => "TRA";
	}
}
