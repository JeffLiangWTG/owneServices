using System.Collections.Generic;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.Testing;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Mail.ServiceTasks.Test
{
	abstract class EdiEmailProcessorServiceProviderTestCase<T> : EmailProcessorServiceProviderTestCase<T> where T : EmailProcessorServiceProvider, new()
	{
		protected override IRegistryItem EnableVerboseModeRegistryItem => EDIDataRegistry.Instance.EnableVerboseModeOnEmailProcessors;
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
