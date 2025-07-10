using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(AdditionalReferenceProvider))]
	class AdditionalReferenceProviderTest : DocumentProviderAbstractTest<AdditionalReferenceProvider>
	{
		public void TestSequenceNumber()
		{
			info.CSI_LineNo = 1;
			AssertEquals(1, Provider.SequenceNumber);
		}

		protected override string SubType => "REF";
	}
}
