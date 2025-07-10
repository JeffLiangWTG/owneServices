using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class CLInboundInterchangeProcessor : BranchInboundInterchangeProcessor
	{
		public CLInboundInterchangeProcessor(IEnumerable<string> applicationCodes) : base(applicationCodes)
		{
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new CLInboundMessageCreator());
		}
		IInboundMessageCreator messageCreator;
	}
}
