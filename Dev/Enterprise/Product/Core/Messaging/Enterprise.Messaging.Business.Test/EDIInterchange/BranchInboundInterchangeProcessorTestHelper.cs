using System.Collections.Generic;

namespace Enterprise.Messaging.Business.Testing
{
	public class BranchInboundInterchangeProcessorTestHelper : BranchInboundInterchangeProcessor
	{
		public BranchInboundInterchangeProcessorTestHelper(IEnumerable<string> applicationCodes)
			: base(applicationCodes)
		{
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new InboundMessageCreatorTestHelper();
	}
}
