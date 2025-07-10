using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public class RefundApplicationDocumentSendingObjectValidation : AutoRefundApplicationDocumentSendingObjectValidation
	{
		public RefundApplicationDocumentSendingObjectValidation(AutoRefundApplicationDocumentSendingObject parent) : base(parent)
		{
		}

		new RefundApplicationDocumentSendingObject Parent => (RefundApplicationDocumentSendingObject)base.Parent;

		protected override void CheckDocumentType()
		{
			if (Parent.Parent.ShouldSend)
			{
				MandatoryValidation.CheckEntered(Parent.DocumentTypeInfo);
			}
		}

		protected override void CheckDocumentDateIsValidZDateTime()
		{
			if (Parent.Parent.ShouldSend)
			{
				base.CheckDocumentDateIsValidZDateTime();
			}
		}

		protected override void CheckDocumentDateIsValidZDateTimeRange()
		{
			if (Parent.Parent.ShouldSend)
			{
				base.CheckDocumentDateIsValidZDateTimeRange();
			}
		}

		protected override void CheckDocumentIdentifierIsWesternEuropean()
		{
			if (Parent.Parent.ShouldSend)
			{
				base.CheckDocumentIdentifierIsWesternEuropean();
			}
		}

		protected override void CheckDocumentTypeIsWesternEuropean()
		{
			if (Parent.Parent.ShouldSend)
			{
				base.CheckDocumentTypeIsWesternEuropean();
			}
		}
	}
}
