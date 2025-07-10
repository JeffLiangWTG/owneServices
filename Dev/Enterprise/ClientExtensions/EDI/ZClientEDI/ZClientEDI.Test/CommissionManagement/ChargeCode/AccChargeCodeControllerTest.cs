using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.GUI.Testing
{
	[TestedType(typeof(AccChargeCodeController))]
	public class AccChargeCodeControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccChargeCode;
		}
	}
}
