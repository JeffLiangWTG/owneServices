using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARReceiptBatchPostingController))]
	public class ARReceiptBatchPostingControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARReceiptBatchPosting;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new ARReceiptBatchPoster(Factory);
		}
	}
}
