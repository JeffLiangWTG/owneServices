using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(PreviousDocumentProvider))]
	class PreviousDocumentProviderTest : DocumentProviderAbstractTest<PreviousDocumentProvider>
	{
		protected override string SubType => "PRE";

		public void TestComplementOfInformation()
		{
			info.CSI_ReferenceNumber2 = "PrevDocCOI";
			AssertEquals("PrevDocCOI", Provider.ComplementOfInformation);
		}
	}
}
