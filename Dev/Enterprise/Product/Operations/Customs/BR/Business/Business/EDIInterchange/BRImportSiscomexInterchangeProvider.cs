using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRImportSiscomexInterchangeProvider : BRInterchangeProvider
	{
		public BRImportSiscomexInterchangeProvider(NonDependentEDIMessageCollection messageCollection)
			: base(messageCollection)
		{
		}

		protected override ZString ProcessedMessageStatusCode(EDIInterchange interchange) => Constants.EDIMessageStatusCodes.Manual;

		protected override ZString QueuedInterchangeStatusCode(EDIInterchange interchange) => Constants.EDIMessageStatusCodes.Manual;

		protected override ZString GetTransportTypeCode() => EDIInterchange.TransportType.tXT;

		protected override bool ShouldAddToEDocs(EDIMessage message) => true;
	}
}

