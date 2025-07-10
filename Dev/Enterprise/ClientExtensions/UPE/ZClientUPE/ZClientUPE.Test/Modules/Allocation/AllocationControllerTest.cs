using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module
{
	[TestedType(typeof(AllocationController))]
	public class AllocationControllerTest : ZSingletonControllerBasherTest
	{
		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.None, new AllocationController().InternalCheckPointForNew);
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.Allocation;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new Allocation(new BusinessObjectFactory());
		}
	}
}
