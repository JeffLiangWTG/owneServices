using Enterprise.Customs.EU.H7.Module.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Module.Testing
{
	[TestedType(typeof(GBH7BillController))]
	class GBH7BillControllerTest : EUH7BillControllerTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.GB.H7Bill;
	}
}
