using CargoWise.Types;
using Enterprise.Customs.Business.MessageManagers.Testing;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	public class TestMessageInstructionUserNotification : TestUserNotification, IMessageInstructionUserNotification
	{
		public bool ShowMessageInstructionForm(MessageInstruction instruction)
		{
			HasShowMessageInstructionFormBeenCalled = true;
			IsWaitingForResponse = instruction.IsWaitingForResponse;
			if (instruction.ContainsValidationErrors)
			{
				ContainsValidationErrors = instruction.ContainsValidationErrors;
				ValidationErrorsMessage = instruction.ValidationErrorsMessage;
			}
			if (instruction.ContainsAdditionalWarnings)
			{
				ContainsAdditionalWarnings = instruction.ContainsAdditionalWarnings;
				AdditionalWarningsMessage = instruction.AdditionalWarningsMessage;
			}
			return NextAnswer ?? true;
		}

		public ZBool IsWaitingForResponse;
		public ZBool ContainsValidationErrors;
		public ZBool ContainsAdditionalWarnings;
		public ZString ValidationErrorsMessage;
		public ZString AdditionalWarningsMessage;

		public bool HasShowMessageInstructionFormBeenCalled;

		public void Reset()
		{
			HasShowMessageInstructionFormBeenCalled = false;
			IsWaitingForResponse = false;
			ContainsAdditionalWarnings = false;
			ContainsAdditionalWarnings = false;
			ValidationErrorsMessage = string.Empty;
			AdditionalWarningsMessage = string.Empty;
			NextAnswer = null;
			LastMessage = string.Empty;
		}
	}
}
