using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.SWT.Testing
{
	class SWTOrderModuleOverrideTest : TestCaseWithFactory
	{
		public void TestNotYetArrivedMenuItem()
		{
			using (SWTOrderModuleOverride module = (SWTOrderModuleOverride)ZModuleFactory.Instance.Create(ModuleIDs.Orders))
			{
				List<MenuItem> items = new List<MenuItem>();
				foreach (MenuItem menuItem in module.FormActionMenu.FindByText("&Actions").MenuItems)
				{
					items.Add(menuItem);
				}

				MenuAssertion.AssertHasMenu("MenuItem 'Export : Printing Out 'Not Yet Arrived' Reports'", items, "D&ata Transfer", "Export : Printing Out 'Not Yet Arrived' Reports");
			}
		}
	}
}
