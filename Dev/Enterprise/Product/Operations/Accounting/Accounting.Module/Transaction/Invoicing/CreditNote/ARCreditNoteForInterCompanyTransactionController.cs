using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class ARCreditNoteForInterCompanyTransactionController : ARCreditNoteController
	{
		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARCreditNoteForInterCompanyTransaction; }
		}

		protected override bool ShouldCheckLoginCompanyMatch => false;
	}
}
