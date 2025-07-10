using System;
using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class MXCInboundInterchangeProcessor : BranchInboundInterchangeProcessor
	{
		public MXCInboundInterchangeProcessor(IEnumerable<string> applicationCodes) : base(applicationCodes)
		{
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new MXInboundMessageCreator());
		}
		IInboundMessageCreator messageCreator;

		protected override Type TypeOfInterchangeToCreate() => typeof(MXInterchange);
	}
}
