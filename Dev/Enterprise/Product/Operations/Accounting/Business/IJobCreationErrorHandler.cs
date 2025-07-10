using Enterprise.Accounting.Business.ConsolCosting;

namespace Enterprise.Accounting.Business
{
	public interface IJobCreationErrorHandler
	{
		void HandleJobCreationError(ApportionmentListing apportionments);
	}

	public class JobCreationErrorHandlerForApportionmentPluginThatDoesNothing : IJobCreationErrorHandler
	{
		void IJobCreationErrorHandler.HandleJobCreationError(ApportionmentListing apportionments)
		{
		}
	}

	class DefaultJobCreationErrorHandler : IJobCreationErrorHandler
	{
		void IJobCreationErrorHandler.HandleJobCreationError(ApportionmentListing apportionments)
		{
			apportionments.ReleaseMutexes();
		}
	}
}
