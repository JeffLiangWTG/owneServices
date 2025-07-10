using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business
{
	public class CopyAndSendToCustomsFormDataValidation : ZValidation
	{
		public CopyAndSendToCustomsFormDataValidation(CopyAndSendToCustomsFormData parent) : base(parent)
		{
			Parent = parent;
		}
		public CopyAndSendToCustomsFormData Parent { get; }

		public override Type AutoValidationType => typeof(CopyAndSendToCustomsFormDataValidation);

		public override void ValidateAll()
		{
			ValidateNumberOfCopies();
			ValidateMessageType();
		}

		public void ValidateNumberOfCopies()
		{
			ValidateCalculatedProperty(Parent.NumberOfCopiesInfo);
		}

		protected void CheckNumberOfCopies()
		{
			var info = Parent.NumberOfCopiesInfo;
			if ((ZInt)info.Value <= 0)
			{
				info.AddError(Res.GetString("9E1ADFA0-92C0-4A0C-B4E0-4A466E1EDCBE", "{0} should be greater than zero.", info.HumanReadableName));
			}
		}

		public void ValidateNumberOfInvoiceLinesCopies()
		{
			ValidateCalculatedProperty(Parent.NumberOfInvoiceLinesCopiesInfo);
		}

		protected void CheckNumberOfInvoiceLinesCopies()
		{
			var info = Parent.NumberOfInvoiceLinesCopiesInfo;
			MandatoryValidation.CheckNotNegative(info);
		}

		public void ValidateMessageType()
		{
			ValidateCalculatedProperty(Parent.MessageTypeInfo);
		}

		protected void CheckMessageType()
		{
			var targetInfo = Parent.MessageTypeInfo;
			if (Parent.SendToCustoms)
			{
				var messageType = Parent.MessageType;
				if (messageType.IsEmpty)
				{
					targetInfo.AddError(MandatoryValidation.MustBeEnteredMessage(targetInfo.Description));
				}
			}
		}
	}
}
