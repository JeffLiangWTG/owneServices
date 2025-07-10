using System;
using NctsHeader = Enterprise.Customs.GB.Business.NctsHeader;

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5.Testing
{
	public class MessageShouldBeDiscardedTestCase
	{
		public string Description { get; set; }
		public Action<NctsHeader> Setup { get; set; }
		public bool ExpectedResult { get; set; }
		public string ExpectedReasonText { get; set; }
	}
}
