using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM933MessageInterpreter : InboundMessageInterpreter<IIM933Provider>
	{
		public IM933MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM933Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM933Provider messageProvider;

		protected override string Summary => Res.GetString("54DA5B22-6FDB-4415-89DD-DDCE87528F41", "A Presentation Notification Rejection (IM933) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (Res.GetString("FADEB370-D4A8-4CC8-A425-186C8E693421", "Rejection Date"), messageProvider.NotificationRejectionDate.ToShortDateString());
			yield return (Res.GetString("A0ABC4A1-800E-4426-9B91-3583081154BA", "Rejection Reason"), messageProvider.NotificationRejectionReason);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			return messageProvider.FunctionalErrors.Select(x => (string.Empty, x.GetMessageDetails(GetCodeAndDescription(x.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180))));
		}
	}
}
