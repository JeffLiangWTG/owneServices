using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM482MessageInterpreter : InboundMessageInterpreter<IM482Provider>
	{
		public IM482MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM482Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("6E5B1D8E-A8AE-4B18-A15B-8DD1112B1F20", "A Document Request (IM482) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails() => provider.GetMessageDetails();
	}
}
