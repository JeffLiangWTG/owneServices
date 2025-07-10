using System;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(BulkJobCloseController))]
	public class BulkJobCloseControllerTest : ZSingletonControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(BulkJobCloseController); }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BulkJobClose;
		}

		public override void TestNewForm()
		{
			AssertControllerNotNull();
			try
			{
				Controller.ShowFormForNewEntity(new BulkJobCloseProcessor(null));
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}
	}
}
