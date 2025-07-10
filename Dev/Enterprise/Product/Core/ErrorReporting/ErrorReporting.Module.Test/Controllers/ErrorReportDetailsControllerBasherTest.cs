using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ErrorReporting.Module.Test
{
	[TestedType(typeof(ErrorReportDetailsController))]
	public class ErrorReportDetailsControllerBasherTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ErrorReportDetails;
		}
	}
}
