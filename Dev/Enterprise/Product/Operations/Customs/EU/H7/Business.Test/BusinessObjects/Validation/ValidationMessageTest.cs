using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	public class ValidationMessageTest : TestCaseWithFactory
	{
		public void TestGetBR3181RuleEUMemberStateMessage()
		{
			var validationMessage = new ValidationMessage();

			AssertEquals("[BR3181] The first 2 characters must correspond to a member state of the EU.", validationMessage.GetBR3181RuleEUMemberStateMessage());
		}

		public void TestFormatValidationRuleMessage()
		{
			var formattedMessage = ValidationMessage.FormatValidationMessageWithRuleCodePrefix("prefix", "message");
			AssertEquals("[prefix] message", formattedMessage);

			var formattedMessageEmptyPrefix = ValidationMessage.FormatValidationMessageWithRuleCodePrefix("", "message");
			AssertEquals("message", formattedMessageEmptyPrefix);
		}

		public void TestInvalidRuleMessage()
		{
			var validationMessage = new ValidationMessage();
			AssertEquals("The code you have selected is not in the list.", validationMessage.InvalidValueRuleMessage.ToString());
		}
	}
}
