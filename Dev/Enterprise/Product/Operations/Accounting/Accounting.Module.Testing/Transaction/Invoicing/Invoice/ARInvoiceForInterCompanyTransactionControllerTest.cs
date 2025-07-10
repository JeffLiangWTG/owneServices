using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARInvoiceForInterCompanyTransactionController))]
	class ARInvoiceForInterCompanyTransactionControllerTest : ARInvoiceControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARInvoiceForInterCompanyTransaction;
		}
	}
}
