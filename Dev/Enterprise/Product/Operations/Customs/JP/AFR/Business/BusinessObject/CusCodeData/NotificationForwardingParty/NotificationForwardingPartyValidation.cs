namespace Enterprise.Customs.JP.AFR.Business
{
	public class NotificationForwardingPartyValidation : Customs.Business.CusCodeDataValidation
	{
		const int MaxNotificationForwardingPartyCount = 3;

		public NotificationForwardingPartyValidation(NotificationForwardingParty parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			var targetValue = Parent.CY_Data;
			var targetInfo = Parent.CY_DataInfo;

			if (targetValue.IsEmpty)
			{
				targetInfo.AddWarning(ValidationConstants.Bill.EmptyCusCodeDataWillNotBeIncluded);
			}
			else if (targetValue.Length != 5 || targetValue.KeepAlphanumericCharacters() != targetValue)
			{
				targetInfo.AddMessageError(ValidationConstants.Bill.NotificationForwardingPartyInvalid);
			}
		}

		protected override void CheckCY_Code()
		{
		}

		protected override void CheckCY_Order()
		{
			base.CheckCY_Order();
			if (Parent.CY_Order > MaxNotificationForwardingPartyCount)
			{
				Parent.CY_OrderInfo.AddWarning(ValidationConstants.Bill.CusCodeExceedingMaximumNumber(Parent.HumanReadableName, MaxNotificationForwardingPartyCount));
			}
			ValidateCY_Data();
		}

		protected new NotificationForwardingParty Parent
		{
			get { return (NotificationForwardingParty)base.Parent; }
		}
	}
}
