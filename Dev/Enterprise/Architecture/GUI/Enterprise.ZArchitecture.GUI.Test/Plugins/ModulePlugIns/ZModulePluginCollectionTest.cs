using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ModulePlugIn;
using Enterprise.ZArchitecture.ModulePlugIn.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Plugins.Testing
{
	internal class ZModulePluginCollectionTest : TestCaseWithFactory
	{
		public void TestExceptionGettingPlugin()
		{
			using (DummyController1.GetPlugInThrowsException())
			{
				Module.Plugins.Add(DummyControllerIDs.Dummy1);

				AssertEquals("should not have reported an exception yet.", true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));

				module.Plugins.GetPlugin(DummyControllerIDs.Dummy1);
				AssertEquals("should have reported the exception.", "ZModulePluginCollection.LoadPlugin:Dummy1", ErrorReporter.LastKeyReported);

				ErrorReporter.Clear();
				module.Plugins.GetPlugin(DummyControllerIDs.Dummy1);
				AssertEquals("the plugin should have been removed and so not report another error.", "", ErrorReporter.LastKeyReported);
			}
		}

		public void TestAdd()
		{
			AssertContainsExactElementsInAnyOrder("precondition:", Type.EmptyTypes, GetPluginTypes(Module.Plugins));

			Module.Plugins.Add(DummyControllerIDs.Dummy2);
			AssertContainsExactElementsInAnyOrder("Should have no plugins", Type.EmptyTypes, GetPluginTypes(Module.Plugins));

			Module.Plugins.Add(DummyControllerIDs.Dummy1);
			AssertContainsExactElementsInAnyOrder("Should only have added the DummyModulePlugin", new Type[] { typeof(DummyModulePlugin1) }, GetPluginTypes(Module.Plugins));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestAddNull()
		{
			Module.Plugins.Add(null);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestAddExisting()
		{
			Module.Plugins.Add(DummyControllerIDs.Dummy1);
			AssertContainsExactElementsInAnyOrder("precondition:", new Type[] { typeof(DummyModulePlugin1) }, GetPluginTypes(Module.Plugins));

			Module.Plugins.Add(DummyControllerIDs.Dummy1);
		}

		public void TestGetActionMenuItems()
		{
			AssertContainsExactElementsInAnyOrder("precondition", Array.Empty<string>(), GetMenuNames(Module.Plugins.GetActionMenuItemsToAdd()));

			Module.Plugins.Add(DummyControllerIDs.Dummy1);
			AssertContainsExactElementsInAnyOrder("Should have returned the menu items", new string[] { "Dummy" }, GetMenuNames(Module.Plugins.GetActionMenuItemsToAdd()));
		}

		public void TestGetButtonGridMenuItems()
		{
			AssertContainsExactElementsInAnyOrder("precondition", Array.Empty<string>(), GetMenuNames(Module.Plugins.GetButtonGridMenuItemsToAdd(null)));

			Module.Plugins.Add(DummyControllerIDs.Dummy1);
			AssertContainsExactElementsInAnyOrder("Should have returned the menu items", new string[] { "Button Grid Menu Item 1" }, GetMenuNames(Module.Plugins.GetButtonGridMenuItemsToAdd(null)));
		}

		public void TestGetPlugin()
		{
			AssertNull("precondition:", Module.Plugins.GetPlugin(DummyControllerIDs.Dummy1));
			Module.Plugins.Add(DummyControllerIDs.Dummy1);

			AssertEquals(typeof(DummyModulePlugin1), Module.Plugins.GetPlugin(DummyControllerIDs.Dummy1).GetType());
		}

		#region Implementation

		IEnumerable<string> GetMenuNames(IEnumerable<MenuItem> menuItems)
		{
			foreach (var item in menuItems)
			{
				yield return item.Text;
			}
		}

		IEnumerable<Type> GetPluginTypes(ModulePlugIn.ZModulePluginCollection collection)
		{
			foreach (var plugin in collection)
			{
				yield return plugin.GetType();
			}
		}

		DummyFilterGridModule Module
		{
			get { return module ?? (module = new DummyFilterGridModule()); }
		}
		DummyFilterGridModule module;

		protected override void TearDown()
		{
			using (module)
			{
				base.TearDown();
			}
		}

		#endregion
	}

	[System.Diagnostics.DebuggerTypeProxy(typeof(DebugDisplay))]
	sealed class ZModulePluginCollection
	{
		class DebugDisplay
		{
			public DebugDisplay(ModulePlugIn.ZModulePluginCollection parent)
			{
				this.parent = parent;
			}

			public ZFilterGridModule Module
			{
				get { return parent.module; }
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
			public ZModulePlugin[] plugins
			{
				get
				{
					var result = new ZModulePlugin[parent.plugins.Count];
					parent.plugins.Values.CopyTo(result, 0);
					return result;
				}
			}

			readonly ModulePlugIn.ZModulePluginCollection parent;
		}
	}
}
