using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	internal class SendingRuleCodeDescriptionPairList : CodeDescriptionPairList
	{
		public SendingRuleCodeDescriptionPairList()
		{
			AddPair(EmailNotificationSendingRules.ALL, ResString.GetMultilingualString("d3ca56f7-f40c-4a31-ae58-7bf687b1d6c9", "Send to staff roles AND notification group"));
			AddPair(EmailNotificationSendingRules.GRP, ResString.GetMultilingualString("82fe9f0c-8231-40ec-a7cf-0707f0163802", "Send to notification group, send to staff roles if there is no notification group"));
			AddPair(EmailNotificationSendingRules.ROL, ResString.GetMultilingualString("fe64b3ef-961e-46ff-8d88-d2b9501aeca7", "Send to staff roles, send to notification group if there are no staff roles"));
			AddPair(EmailNotificationSendingRules.NON, ResString.GetMultilingualString("92c8aaa5-5f67-4bea-92f5-d41b7c079243", "Do not send email notifications"));
		}
	}

	public static class EmailNotificationSendingRules
	{
		public const string ALL = "ALL", GRP = "GRP", ROL = "ROL", NON = "NON";
	}
}
