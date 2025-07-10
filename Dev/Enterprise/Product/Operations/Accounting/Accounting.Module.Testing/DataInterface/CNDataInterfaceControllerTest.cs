using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(CNDataInterfaceController))]
	public class CNDataInterfaceControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.CNDataInterface;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new ChinaStandard2010DataInterfaceWrapper();
		}
	}
}
