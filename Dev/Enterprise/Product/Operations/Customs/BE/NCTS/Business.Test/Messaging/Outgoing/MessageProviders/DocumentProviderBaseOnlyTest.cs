using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(DocumentProvider))]
	class DocumentProviderBaseOnlyTest : DocumentProviderAbstractTest<DocumentProvider>
	{
		protected override string SubType => "REF";

		public void TestSequenceNumber()
		{
			info.CSI_LineNo = 1;
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestType()
		{
			AssertEquals("TYP", Provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("RefNum", Provider.ReferenceNumber);
		}
	}
}
