using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CN2004DataInterfaceController))]
	public class CN2004DataInterfaceControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CN2004DataInterface;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new ChinaStandard2004DataInterfaceWrapper();
		}
	}
}
