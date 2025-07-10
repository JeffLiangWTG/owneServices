using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module
{
	[TestedType(typeof(CheckoutController))]
	public class CheckoutControllerTest : ZSingletonControllerBasherTest
	{
		public void TestCheckPointForNew()
		{
			AssertEquals(Env.Security.None, new CheckoutController().InternalCheckPointForNew);
		}

		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.Checkout;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new Checkout(new BusinessObjectFactory());
		}
	}
}
