using Enterprise.Customs.ES.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public class ArrivalAtExitMessagePrettyFormatter : EdiFactV921ESMessagePrettyFormatter
	{
		public ArrivalAtExitMessagePrettyFormatter(ICUSRESV921ESMessageProvider messageHelperProvider) : base(messageHelperProvider)
		{
		}
	}
}
