
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.Business
{
	public class CustomsAndExciseReportOutboundMessageValidation : EDIMessageValidation
	{
		public CustomsAndExciseReportOutboundMessageValidation(CustomsAndExciseReportOutboundMessage parent) : base(parent)
		{
		}

		protected new CustomsAndExciseReportOutboundMessage Parent => (CustomsAndExciseReportOutboundMessage)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateEM_MessageType();
			ValidateMessageDate();
		}

		protected override void CheckEM_MessageType()
		{
			base.CheckEM_MessageType();
			if (Parent.EM_MessageType.IsEmpty)
			{
				Parent.EM_MessageTypeInfo.AddError(Res.GetString("a93a423f-826e-4db3-9a32-4d46670fcbaa", "Report Type must be provided."));
			}
		}

		public void ValidateMessageDate()
		{
			ValidateCalculatedProperty(Parent.MessageDateInfo);
		}

		protected void CheckMessageDate()
		{
			if (Parent.EM_MessageType != CustomsAndExciseReportTypeList.Codes.UDR && Parent.EM_MessageType != CustomsAndExciseReportTypeList.Codes.BAL && Parent.MessageDate.IsEmpty)
			{
				Parent.MessageDateInfo.AddError(Res.GetString("81c4a9ba-41e0-49e0-b441-0ed7fdb0cc7c", "Date must be provided when Message Type is not UDR or BAL."));
			}
		}
	}
}
