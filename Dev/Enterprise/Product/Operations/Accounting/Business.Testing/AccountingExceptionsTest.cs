using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Testing
{
	public class JobCreationExceptionTest : ExceptionHandledWithPopupTest
	{
		protected override ExceptionHandledWithPopup GetExceptionInstance()
		{
			return new JobCreationException("JobCreationException error message");
		}
	}
}
