using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(PreviousDocumentProvider))]
	class PreviousDocumentProviderTest : DocumentProviderAbstractTest<PreviousDocumentProvider>
	{
		public void TestComplementOfInformation()
		{
			info.CSI_ReferenceNumber2 = "PrevDocCOI";
			AssertEquals("PrevDocCOI", Provider.ComplementOfInformation);
		}

		protected override string SubType => "PRE";
	}
}
