using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC561MessageInterpreter : InboundMessageInterpreter<CC561CProvider>
	{
		public CC561MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC561CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"DE72942A-3808-427A-A40F-8B6B7927B250",
			"An Exit Control Decision Notification message has been received from Customs for Job {0} through the IE561 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.ControlNotificationDateAndTime, provider.ControlNotificationDateAndTime.ToLongTimeString());

			var controlTypeCodes = RefCusCodeListTypes.GetCachedList(message.Factory, Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TypeOfControlsType, MessageCreatedDate);

			foreach (var controlType in provider.ControlTypes)
			{
				yield return (CommonResStrings.ControlType, MessageInterpreterHelper.GetCodeAndDescription(controlType.Type, controlTypeCodes));
				yield return (CommonResStrings.ControlText, controlType.Text);
			}
		}
	}
}
