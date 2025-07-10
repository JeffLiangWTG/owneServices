using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(AdditionalReferenceProvider))]
	sealed class AdditionalReferenceProviderTest : DocumentProviderAbstractTest<AdditionalReferenceProvider>
	{
		protected override string SubType => "REF";

		public void TestSequenceNumber()
		{
			info.CSI_LineNo = 1;
			AssertEquals(1, Provider.SequenceNumber);
		}
	}
}
