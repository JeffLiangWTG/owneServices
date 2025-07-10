using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Messaging.MessageProcessors.Testing;

namespace Enterprise.Messaging.Business.Testing
{
	sealed class BranchCustomsMessageProcessorTestHelper : BranchCustomsMessageProcessor
	{
		public BranchCustomsMessageProcessorTestHelper(ZString[] applicationCodes, ZString[] messageTypes)
			: base(applicationCodes, messageTypes)
		{ }

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessorsCore()
		{
			var result = base.GetMessageProcessorsCore();
			result.Add(new BranchCustomsApplicationTypeMessageProcessorTestHelper(Logger, "AC1"));
			result.Add(new BranchCustomsApplicationTypeMessageProcessorTestHelper(Logger, "AC2"));
			result.Add(new BranchCustomsApplicationTypeMessageProcessorTestHelper(Logger, "AC3"));
			return result;
		}

		public ZQuery GetMessageProcessorQueryExposed() => GetMessageProcessorQuery();
	}
}
