using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARCreditNoteForInterCompanyTransactionController))]
	class ARCreditNoteForInterCompanyTransactionControllerTest : ARCreditNoteControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARCreditNoteForInterCompanyTransaction;
		}
	}
}
