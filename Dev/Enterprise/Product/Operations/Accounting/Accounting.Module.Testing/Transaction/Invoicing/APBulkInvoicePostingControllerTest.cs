using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APBulkInvoicePostingController))]
	public class APBulkInvoicePostingControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APBulkInvoicePosting;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new APBulkInvoicePoster(Factory);
		}

		public void TestCorrectContext()
		{
			Assert(Controller.Factory.HasContext(BusinessContext.APBulkInvoicePoster));
		}
	}
}
