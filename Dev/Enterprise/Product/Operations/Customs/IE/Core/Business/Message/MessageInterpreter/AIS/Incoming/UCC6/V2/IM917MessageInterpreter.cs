using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM917MessageInterpreter : InboundMessageInterpreter<IM917Provider>
	{
		public IM917MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM917Provider provider)
			: base(message, provider) { }

		protected override string Summary => Res.GetString("3ED9A502-0B2A-48D0-9DDC-7506BAE9539E", "A Syntax Error Notification (IM917) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			if (provider.Errors.Count > 0)
			{
				return provider.Errors.First().GetMessageDetails();
			}
			else
			{
				return Enumerable.Empty<(string, string)>();
			}
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			return provider.Errors.Skip(1).Select(x => (string.Empty, x.GetMessageDetails()));
		}
	}
}
