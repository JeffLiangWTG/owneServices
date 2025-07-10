using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(MoveInNoticeMessageItemWrapper))]
	sealed class MoveInNoticeMessageItemWrapperTest : InboundMessageDocumentItemWrapperTest<MoveInNoticeMessageItemWrapper, MoveInNoticeMessageWrapper>
	{
		public void TestItemProperties()
		{
			var wrapper = DocItemWrapper;

			CombineAssertions(() =>
			{
				AssertEquals("I_6", "AAAAAAAAA1AAAAAAAAA2", wrapper.I_6);
				AssertEquals("I_7", "999,999", wrapper.I_7);
				AssertEquals("I_8", "999,999", wrapper.I_8);
				AssertEquals("I_9", "10,250.26", wrapper.I_9);
				AssertEquals("I_10", "AAA", wrapper.I_10);
				AssertEquals("I_11", "AAA", wrapper.I_11);
				AssertEquals("I_12", "A", wrapper.I_12);
				AssertEquals("I_13", "AAA", wrapper.I_13);
				AssertEquals("I_14", "AAA", wrapper.I_14);
				AssertEquals("I_15", "AAAAAAAAA1AAAAAAAAA2A", wrapper.I_15);
				AssertEquals("I_16", "AA", wrapper.I_16);
			});
		}

		protected override string GetDefaultMessageTestFile() => "MoveInNoticeMessage.txt";
	}
}
