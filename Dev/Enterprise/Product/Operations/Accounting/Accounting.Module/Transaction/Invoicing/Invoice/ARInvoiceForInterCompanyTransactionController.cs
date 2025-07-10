using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ARInvoiceForInterCompanyTransactionController : ARInvoiceController
	{
		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARInvoiceForInterCompanyTransaction; }
		}

		protected override bool ShouldCheckLoginCompanyMatch => false;
	}
}
