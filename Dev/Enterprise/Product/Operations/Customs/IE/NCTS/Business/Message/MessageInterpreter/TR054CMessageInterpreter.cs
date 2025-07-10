using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR054CMessageInterpreter : InboundMessageInterpreter<TR054CProvider>
	{
		public TR054CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR054CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("33CE5566-C1DF-4F67-9F7C-C0F44D916793", "A Request for Advice (TR054) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (Res.GetString("B4909519-F145-46D0-95B4-EE5D8FC34945", "Advice Requested"), provider.AdviceRequested.ToString());
			yield return (Res.GetString("23BD0F6F-2CCF-42F0-BB3C-518C38B473B2", "Advice Request Date & Time"), provider.AdviceRequestDateAndTime.ToLongTimeString());
		}
	}
}
