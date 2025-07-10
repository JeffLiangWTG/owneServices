using System;
using CargoWise.EntityFramework;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Module.Testing
{
	[TestedType(typeof(ServiceTaskLogViewerController))]
	public class ServiceTaskLogViewerControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType()
		{
			return typeof(ServiceTaskLogViewer);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ServiceTaskLogViewer;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new ServiceTaskLogViewer();
		}

		public override void TestNewForm()
		{
			Assert(true);   // no new support on this controller
		}
	}
}
