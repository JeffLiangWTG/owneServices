using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(SupportingDocumentProvider))]
	class SupportingDocumentProviderTest : DocumentProviderAbstractTest<SupportingDocumentProvider>
	{
		public void TestComplementOfInformation()
		{
			info.CSI_ReferenceNumber2 = "SupDocRef";
			AssertEquals("SupDocRef", Provider.ComplementOfInformation);
		}

		public void TestDocumentLineItemNumber()
		{
			info.CSI_ItemNumber = 0;
			AssertEquals(null, Provider.DocumentLineItemNumber);

			info.CSI_ItemNumber = 4;
			AssertEquals(4, Provider.DocumentLineItemNumber);
		}

		protected override string SubType => "SUP";
	}
}
