using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ClaimChargesController))]
	class ClaimChargesControllerTest : UACreditNoteControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ClaimCharges;
		}
	}
}
