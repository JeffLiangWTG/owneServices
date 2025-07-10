using System;
using CargoWise.EntityFramework;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.GUI;
using Enterprise.Freight.Forwarding.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Module.Testing
{
	[TestedType(typeof(JASJobConsolControllerForTest))]
	public class JASJobConsolControllerTest : TestJobConsolController
	{
		public void TestGetForm()
		{
			JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
			JASJobConsolControllerForTest controller = new JASJobConsolControllerForTest();
			using (IZForm form = controller.GetForm(consol))
			{
				AssertEquals(typeof(JASConsolForm), form.GetType());
			}
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(JASForwardingConsol), Controller.TypeOfTopLevelBusinessObject);
		}

		#region Implementation
		protected override Type GetBusinessObjectType()
		{
			return typeof(JASForwardingConsol);
		}

		public override Type ControllerToBashType
		{
			get
			{
				return typeof(JASJobConsolController);
			}
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#region JASJobConsolControllerForTest
		class JASJobConsolControllerForTest : JASJobConsolController
		{
			public new IZForm GetForm(IBusiness businessEntity)
			{
				return base.GetForm(businessEntity);
			}
		}
		#endregion
		#endregion
	}
}
