using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class CustomsAndExciseReportErrorInterpreter : BaseInboundMessageInterpreter<CustomsAndExciseReportErrorProvider>
	{
		public CustomsAndExciseReportErrorInterpreter(Enterprise.Messaging.Business.EDIMessage message, CustomsAndExciseReportErrorProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => CommonResStrings.RosErrorMessageFriendlyName;

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			var errorCode = provider.ErrorCode;

			yield return (CommonResStrings.ErrorCode, errorCode);
			yield return (CommonResStrings.ErrorCodeDescription, RevenueErrorsList.GetDescriptionFromCode(errorCode));
		}
	}
}
