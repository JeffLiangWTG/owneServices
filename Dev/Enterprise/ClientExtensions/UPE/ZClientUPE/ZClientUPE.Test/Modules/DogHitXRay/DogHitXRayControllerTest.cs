using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module
{
	[TestedType(typeof(DogHitXRayController))]
	public class DogHitXRayControllerTest : ZSingletonControllerBasherTest
	{
		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.None, new DogHitXRayController().InternalCheckPointForNew);
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.DogHitXRay;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new DogHitXRay(new BusinessObjectFactory());
		}
	}
}
