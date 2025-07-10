using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	public static class QueuedLogReferenceProviderFactory
	{
		public static IQueuedLogReferenceProvider GetReferenceProvider(string triggerAction, ILogger logger = null)
		{
			switch (triggerAction)
			{
				case WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies:
					return new CompanyPKDecorator(new QueuedLogReferenceProvider(logger));

				// In other more complex cases:
				// Implement new decorators and combine decorators, like:
				// return new ParameterDecorator1(new ParameterDecorator2(new CompanyPKDecorator(new QueueLogReferenceProvider())))
				//
				// Also can use decorator to modify exist parameter, like:
				// return new NumberAddOneDecorator(new NumberDecorator(...))

				default:
					return new QueuedLogReferenceProvider(logger);
			}
		}
	}
}
