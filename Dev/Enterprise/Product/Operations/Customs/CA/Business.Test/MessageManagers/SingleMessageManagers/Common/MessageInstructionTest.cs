using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(MessageInstruction))]
	sealed class MessageInstructionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageInstruction()
		{
			var notifStr = "Notif String";
			var warningStr = "Warning String";
			var instruction = new MessageInstruction(Factory, true, notifStr, warningStr, null, false, true);
			Assert("IsWaitingForResponse", instruction.IsWaitingForResponse);
			Assert("ContainsValidationErrors", instruction.ContainsValidationErrors);
			AssertEquals("ValidationErrorsMessage", notifStr, instruction.ValidationErrorsMessage);
			Assert("ContainsAdditionalWarnings", instruction.ContainsAdditionalWarnings);
			AssertEquals("AdditionalWarningsMessage", warningStr, instruction.AdditionalWarningsMessage);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MessageInstruction(Factory, false, string.Empty, string.Empty, null, false, true);
		}
	}
}
