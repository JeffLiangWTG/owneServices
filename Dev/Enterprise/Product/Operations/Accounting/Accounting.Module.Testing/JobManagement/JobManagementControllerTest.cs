using System;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobManagementController))]
	public class JobManagementControllerTest : JobManagementControllerBaseTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(JobManagementController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobManagement;
		}
	}
}
