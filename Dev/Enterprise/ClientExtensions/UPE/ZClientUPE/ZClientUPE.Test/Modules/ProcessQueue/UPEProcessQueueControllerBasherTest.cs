using System;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEProcessQueueController))]
	internal class UPEProcessQueueControllerBasherTest : ZControllerBasherTest
	{
		public void TestPluginTabPageCaption()
		{
			var controller = new UPEProcessQueueController();
			AssertEquals("Tab Page Caption should be 'Process Queue'", "Process Queue", controller.PluginTabPageCaption.Caption);
		}

		public override Type ControllerToBashType
		{
			get
			{
				return typeof(UPEProcessQueueController);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ProcessQueue;
		}
	}
}
