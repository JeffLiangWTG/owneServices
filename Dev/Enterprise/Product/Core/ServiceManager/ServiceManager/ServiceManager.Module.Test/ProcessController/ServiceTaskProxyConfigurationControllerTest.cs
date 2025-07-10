using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Module.Testing
{
	[TestedType(typeof(ServiceTaskProxyConfigurationController))]
	class ServiceTaskProxyConfigurationControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ServiceTaskProxyConfiguration;
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(StmServiceHost);
		}

		public override void TestNewForm()
		{
			Assert(true);   // no new support on this controller
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new ServiceTaskProxyConfigurationControllerForTest();
			AssertEquals(Env.Security.ServiceTaskView, controller.CheckPointForView);
			AssertEquals(Env.Security.ServiceTaskNew, controller.CheckPointForNew);
			AssertEquals(Env.Security.ServiceTaskEdit, controller.CheckPointForEdit);
			AssertEquals(Env.Security.ServiceTaskDelete, controller.CheckPointForDelete);
		}

		public void TestGetForm()
		{
			var host = Factory.New<StmServiceHost>();

			var controller = new ServiceTaskProxyConfigurationControllerForTest();

			using (var form = controller.GetForm(host))
			{
				AssertEquals(typeof(ServiceTaskHostConfigurationForm), form.GetType());

				Assert(host.SH_ProxyAutoDetect);
				Assert(!host.HasChanges);
			}
		}

		class ServiceTaskProxyConfigurationControllerForTest : ServiceTaskProxyConfigurationController
		{
			public new SecurityCheckpoint CheckPointForView
			{
				get { return base.CheckPointForView; }
			}

			public new SecurityCheckpoint CheckPointForNew
			{
				get { return base.CheckPointForNew; }
			}

			public new SecurityCheckpoint CheckPointForEdit
			{
				get { return base.CheckPointForEdit; }
			}

			public new SecurityCheckpoint CheckPointForDelete
			{
				get { return base.CheckPointForDelete; }
			}

			public new IZForm GetForm(IBusiness businessEntity)
			{
				return base.GetForm(businessEntity);
			}
		}
	}
}
