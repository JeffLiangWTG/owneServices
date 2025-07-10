using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class ARInboundInterchangeProcessor : BranchInboundInterchangeProcessor
	{
		public ARInboundInterchangeProcessor(IEnumerable<string> applicationCodes) : base(applicationCodes) { }

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => messageCreator ?? (messageCreator = new ARInboundMessageCreator());
		IInboundMessageCreator messageCreator;
	}
}
