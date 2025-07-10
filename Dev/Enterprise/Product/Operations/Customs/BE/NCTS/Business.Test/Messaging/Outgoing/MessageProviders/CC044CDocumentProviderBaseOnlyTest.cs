using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC044CDocumentProvider))]
	sealed class CC044CDocumentProviderBaseOnlyTest : DocumentProviderAbstractTest<CC044CDocumentProvider>
	{
		protected override string SubType => "REF";

		public void TestSequenceNumber()
		{
			info.CSI_Status = "XXX";
			info.CSI_LineNo = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestType()
		{
			info.CSI_Status = "XXX";
			AssertNull(Provider.Type);
		}

		public void TestReferenceNumber()
		{
			info.CSI_Status = "XXX";
			AssertNull(Provider.ReferenceNumber);
		}

		public void TestSequenceNumber_NEW()
		{
			info.CSI_Status = "NEW";
			info.CSI_LineNo = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestType_NEW()
		{
			info.CSI_Status = "NEW";
			AssertEquals("TYP", Provider.Type);
		}

		public void TestReferenceNumber_NEW()
		{
			info.CSI_Status = "NEW";
			AssertEquals("RefNum", Provider.ReferenceNumber);
		}
	}
}
