using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class NctsEdiMessagePrettierTest : TestCaseWithFactory
	{
		public void TestFillSharedFields()
		{
			var sharedFields = new NCTSPrettierSharedFields();
			messagePrettier.FillSharedFieldsExposed(sharedFields);
			AssertContainsExactElementsInAnyOrder(
				expected: new [] { ("Message Recipient", "NTA.XX") },
				actual: sharedFields);
		}

		public void TestOutboundPretty()
		{
			var expected = @"<tr><td>Message Recipient</td><td>NTA.XX</td></tr>";
			AssertContains(expected, new NctsEdiMessagePrettier(message).MakeOutboundPrettyForInterpretation(header));
		}

		protected override void SetUp()
		{
			base.SetUp();
			message = Factory.New<EDIMessage>();
			message.EM_MessageText = messageText;
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			messagePrettier = new NctsEdiMessagePrettierForTest(message);
		}

		EDIMessage message;
		NctsHeader header;
		NctsEdiMessagePrettierForTest messagePrettier;
		readonly string messageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<q1:CC015C xmlns:q1=""http://ncts.dgtaxud.ec"">
  <messageRecipient>NTA.XX</messageRecipient>
</q1:CC015C>";

		class NctsEdiMessagePrettierForTest : NctsEdiMessagePrettier
		{
			public NctsEdiMessagePrettierForTest(EDIMessage message) : base(message)
			{ }

			public void FillSharedFieldsExposed(NCTSPrettierSharedFields sharedFields) => FillSharedFields(sharedFields);
		}
	}
}
