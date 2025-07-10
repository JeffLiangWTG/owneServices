#if DEBUG
using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Design.Testing
{
	public class DesignerActionExtenderProviderTests : TestCase
	{
		public void TestDesignTimeVisible()
		{
			DesignTimeVisibleAttribute attr = (DesignTimeVisibleAttribute)TypeDescriptor.GetAttributes(typeof(DesignerActionExtenderProvider))[typeof(DesignTimeVisibleAttribute)];
			AssertEquals("Don't show the extender provider in the component tray at design time", false, attr.Visible);
		}

		public void TestNoToolboxItem()
		{
			ToolboxItemAttribute attr = (ToolboxItemAttribute)TypeDescriptor.GetAttributes(typeof(DesignerActionExtenderProvider))[typeof(ToolboxItemAttribute)];
			AssertEquals("Don't show the extender provider in the tool box at design time", null, attr.ToolboxItemType);
		}

		public void TestCanExtend_AddsDDesignerActionList()
		{
			ExtenderProvider = new DesignerActionExtenderProvider();
			Component.Site = Site;
			AssertEquals(
	"Should not be able to extend, we just want CanExtend to be invoked so we can add DDesignerActionList to DesignerActionService",
	false, ExtenderProvider.CanExtend(Component));
			AssertEquals("Action list should be added to the service", true, Site.DesignerActionService.Contains(Component));
		}

		public void TestCanExtend_WhenSiteIsNull()
		{
			ExtenderProvider = new DesignerActionExtenderProvider();
			Component.Site = null;
			AssertEquals(false, ExtenderProvider.CanExtend(Component));
		}

		#region Implementation

		readonly TestSite Site = new TestSite();
		readonly TestComponent Component = new TestComponent();
		IExtenderProvider ExtenderProvider;

		protected override void TearDown()
		{
			if (ExtenderProvider != null)
			{
				((DesignerActionExtenderProvider)ExtenderProvider).Dispose();
			}
			base.TearDown();
		}

		#endregion

		#region Test Classes

		class TestComponent : IComponent
		{
			[SmartTagVisible]
			public string CommonlyUsedProperty
			{
				get { return ""; }
			}

			public ISite Site
			{
				get { return fSite; }
				set { fSite = value; }
			}
			ISite fSite;

			#region IComponent Unsupported Members

			event EventHandler IComponent.Disposed
			{
				add { throw new Exception("The method or operation is not implemented."); }
				remove { throw new Exception("The method or operation is not implemented."); }
			}

			void IDisposable.Dispose()
			{
				throw new Exception("The method or operation is not implemented.");
			}

			#endregion
		}

		class TestSite : ISite
		{
			public DesignerActionService DesignerActionService
			{
				get
				{
					if (fDesignerActionService == null)
					{
						fDesignerActionService = new DesignerActionService(null);
					}
					return fDesignerActionService;
				}
			}
			DesignerActionService fDesignerActionService;

			object IServiceProvider.GetService(Type serviceType)
			{
				if (serviceType == typeof(DesignerActionService))
				{
					return DesignerActionService;
				}
				return null;
			}

			#region ISite Members

			IComponent ISite.Component
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			IContainer ISite.Container
			{
				get { return null; }
			}

			bool ISite.DesignMode
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			string ISite.Name
			{
				get
				{
					throw new Exception("The method or operation is not implemented.");
				}
				set
				{
					throw new Exception("The method or operation is not implemented.");
				}
			}

			#endregion
		}

		#endregion
	}
}
#endif
