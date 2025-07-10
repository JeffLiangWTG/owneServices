using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Helpers.Testing
{
	public class CCSUKProcessingInstructionHelperTests : TestCase
	{
		public void TestAll()
		{
			var helper = new CCSUKProcessingInstructionHelper(@"<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""E2680E3226A04FB5919518DDD9406C9F"" x-conversation-id=""77685C3D14D53E25E0540003BA9676AB"" ?>");
			AssertEquals("CCSUK", helper.SenderId);
			AssertEquals("WISETECHGLOBAL", helper.RecipientId);
			AssertEquals("E2680E3226A04FB5919518DDD9406C9F", helper.ExtCorrelationId);
			AssertEquals(new ZGuid("E2680E3226A04FB5919518DDD9406C9F"), helper.CorrelationId);
			AssertEquals("77685C3D14D53E25E0540003BA9676AB", helper.XConversationId);
			AssertEquals(new ZGuid("77685C3D14D53E25E0540003BA9676AB"), helper.ConversationId);
		}
	}
}
