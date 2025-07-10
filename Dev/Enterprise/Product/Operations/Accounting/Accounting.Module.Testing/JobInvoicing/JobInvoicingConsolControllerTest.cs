using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobInvoicingConsolController))]
	public class JobInvoicingConsolControllerTest : JobInvoicingControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobInvoicingConsol;
		}
	}
}
