using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(DocumentProvider))]
	class DocumentProviderTest : DocumentProviderAbstractTest<DocumentProvider>
	{
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

		public void TestReferenceNumberMaxLength_ForPhase5TransitionPeriod()
		{
			AssertEquals("Outside Phase 5 Transition Period", MessageSchema.ReferenceNumberMaxLength, Provider.ReferenceNumberMaxLength);
			AssertEquals("In Phase 5 Transition Period", MessageSchemaInTransitionPeriod.ReferenceNumberMaxLength, new DocumentProvider(info, true).ReferenceNumberMaxLength);
		}

		protected override string SubType => "REF";
	}
}
