using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(WorkQueuesModule))]
	class WorkQueuesModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WorkQueues;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);

			collection.Add(collection.Factory.New<WorkQueue>());
		}

		public void TestHasDeleteMenuItem()
		{
			using (var form = new ZForm())
			using (var module = new WorkQueuesModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				Application.DoEvents();
				AssertEquals("&Delete", module.DeleteMenuItem.Text);
				AssertNull("Should be no overridden DeleteButtonText, since that's used to signify it's a 'Deactivate' button rather than Delete.", module.DeleteButtonText);
			}
		}
	}
}
