using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public class CusEntryHeaderMessageSendingActionValidation : ZValidation
	{
		public CusEntryHeaderMessageSendingActionValidation(CusEntryHeaderMessageSendingAction parent) : base(parent)
		{
			Parent = parent;
		}

		public CusEntryHeaderMessageSendingAction Parent { get; }

		public override Type AutoValidationType => typeof(CusEntryHeaderMessageSendingActionValidation);

		public override void ValidateAll()
		{
			ValidateMessageType();
			ValidateAnnotation();
			ValidateShouldSend();
			ValidateEntryStatus();
		}

		public void ValidateShouldSend()
		{
			ValidateCalculatedProperty(Parent.ShouldSendInfo);
		}

		protected virtual void CheckShouldSend()
		{
			if (Parent.ShouldSend && Parent.EntryInstruction == null)
			{
				Parent.ShouldSendInfo.AddError(Res.GetString("AEC044F7-37A0-4342-BAB0-72635EC7F9AD", "This entry has no linked Entry Instruction. Please check if there are any Invoice Lines linked to the corresponding Entry Instruction."));
			}
		}

		public void ValidateAnnotation()
		{
			ValidateCalculatedProperty(Parent.AnnotationInfo);
		}

		protected virtual void CheckAnnotation()
		{
		}

		public void ValidateMessageType()
		{
			ValidateCalculatedProperty(Parent.MessageTypeInfo);
		}

		protected virtual void CheckMessageType()
		{
			var targetInfo = Parent.MessageTypeInfo;
			if (Parent.ShouldSend)
			{
				var messageType = Parent.MessageType;
				if (messageType.IsEmpty)
				{
					targetInfo.AddError(MandatoryValidation.MustBeEnteredMessage(targetInfo.Description));
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(targetInfo);
				}
			}
			ValidateAnnotation();
		}

		public void ValidateEntryStatus()
		{
			ValidateCalculatedProperty(Parent.EntryStatusInfo);
		}

		protected virtual void CheckEntryStatus()
		{
		}
	}
}
