using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM484MessageInterpreter : InboundMessageInterpreter<IM484Provider>
	{
		public IM484MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM484Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("B8A756E9-767F-4DCD-A720-3C1B3979D907", "A Request Document Presentation (IM484) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails() => provider.GetMessageDetails();
	}
}
