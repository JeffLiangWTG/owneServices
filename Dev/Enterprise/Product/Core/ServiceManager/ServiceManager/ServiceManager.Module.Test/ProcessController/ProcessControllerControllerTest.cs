using System;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Module.Testing
{
	[TestedType(typeof(ProcessControllerController))]
	class ProcessControllerControllerTest : ZControllerBasherTest
	{
		protected override Type GetBusinessObjectType() => typeof(StmServiceHost);

		protected override ControllerID GetControllerID() => ControllerIDs.ProcessController;

		//we do not have edit/new/delete/view so these tests are redundant
		[ExpectNoExceptions]
		public override void TestDeleteForm()
		{
		}

		[ExpectNoExceptions]
		public override void TestEditForm()
		{
		}

		[ExpectNoExceptions]
		public override void TestNewForm()
		{
		}

		[ExpectNoExceptions]
		public override void TestViewForm()
		{
		}
	}
}

