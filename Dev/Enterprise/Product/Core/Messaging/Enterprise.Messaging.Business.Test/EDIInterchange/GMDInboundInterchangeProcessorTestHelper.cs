using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Messaging.Business.Testing
{
	public class GMDInboundInterchangeProcessorTestHelper : GMDInboundInterchangeProcessor
	{
		public GMDInboundInterchangeProcessorTestHelper(IEnumerable<ZString> interchangeTypes)
			: base(interchangeTypes)
		{
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new InboundMessageCreatorTestHelper();
	}
}
