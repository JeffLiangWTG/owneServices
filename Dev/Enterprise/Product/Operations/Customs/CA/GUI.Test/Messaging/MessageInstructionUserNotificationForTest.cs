using System;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class MessageInstructionUserNotificationForTest : MessageInstructionUserNotification
	{
		public void ThrowExceptionIfShowFormIsTrue(MessageInstruction instruction)
		{
			if (IsShowForm(instruction))
			{
				throw new Exception();
			}
		}
	}
}
