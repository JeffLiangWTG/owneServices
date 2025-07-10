using CargoWise.Customs.JP.MessageDefinitions;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing
{
	[TestedType(typeof(MoveInNoticeMessageWrapper))]
	sealed class MoveInNoticeMessageWrapperTest : InboundMessageDocumentWrapperTest<MoveInNoticeMessageWrapper, ICarryInDocument>
	{
		public void TestHeaderProperties()
		{
			var wrapper = MessageDocumentWrapper;
			CombineAssertions(() =>
			{
				AssertEquals("H_2", "AAAAAAA", wrapper.H_2);
				AssertEquals("H_3", "AAAAA", wrapper.H_3);
				AssertEquals("H_4", "2025/05/26", wrapper.H_4);
				AssertEquals("H_5", "AAAAA", wrapper.H_5);
				AssertEquals("Item count", 50, wrapper.Items.Count);
			});
		}

		protected override string GetDefaultMessageTestFile() => "MoveInNoticeMessage.txt";
	}
}
