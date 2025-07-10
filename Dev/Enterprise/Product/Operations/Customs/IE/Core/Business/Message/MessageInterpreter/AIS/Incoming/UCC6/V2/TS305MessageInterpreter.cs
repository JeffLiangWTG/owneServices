using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS305MessageInterpreter : InboundMessageInterpreter<TS305Provider>
	{
		public TS305MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TS305Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("FCBFECCA-56D9-4F05-9231-E01791A46184", "An Amendment Request Rejection (TS305) message has been received for TSD {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			return Enumerable.Empty<(string Key, string Value)>();
		}
	}
}
