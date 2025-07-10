using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGridWithoutColumnStylesSerialisationTest : TestCase
	{
		public static void AssertColumnStylesNotSerializedByDesigner(Type gridType)
		{
			using (var grid = (Control)Activator.CreateInstance(gridType))
			{
				var columnStylesProperty = TypeDescriptor.GetProperties(gridType)["ColumnStyles"];
				var columnStyles = (ArrayList)columnStylesProperty.GetValue(grid);
				using (var column = new ZTextBoxColumnStyle(new ZTextBoxColumnStyleInfo()))
				{
					columnStyles.Add(column);
				}

				var designerHost = new TestDesignerHost();
				var site = new TestSite();
				grid.Site = site;
				site.DesignerHost = designerHost;

				designerHost.RootComponent = grid;
				columnStyles = (ArrayList)columnStylesProperty.GetValue(grid);
				AssertEquals("Should return the actual columns if the designed RootComponent is a sub-class of the grid", 1, columnStyles.Count);

				using (var form = new ZForm())
				{
					designerHost.RootComponent = form;
					columnStyles = (ArrayList)columnStylesProperty.GetValue(grid);
					AssertEquals("Should return zero columns always to trick the designer if the designed RootComponent is not a sub-class", 0, columnStyles.Count);
				}
			}
		}

		public void TestColumnsStylesNotSerializedByDesigner()
		{
			AssertColumnStylesNotSerializedByDesigner(typeof(ZGridWithoutColumnStylesSerialisation));
		}

		public void TestColumnsStylesNotSerializedByDesigner_IfEmbeddedInZModuleButtonGrid()
		{
			var columnStylesProperty = TypeDescriptor.GetProperties(typeof(ZGridWithoutColumnStylesSerialisation))["ColumnStyles"];

			using (var grid = new ZGridWithoutColumnStylesSerialisation())
			using (var moduleButtonGrid = new ZModuleButtonGrid())
			{
				AssertEquals("Should serialize (albeit perhaps with an empty array) when not embedded in ZModuleButtonGrid", true, columnStylesProperty.ShouldSerializeValue(grid));
				AssertEquals("Should not serialize ColumnStyles when embedded in a ZModuleButtonGrid", false, columnStylesProperty.ShouldSerializeValue(moduleButtonGrid.InnerGrid));
			}
		}

		#region Test Classes

		class TestSite : ISite
		{
			public IDesignerHost DesignerHost;

			public bool DesignMode
			{
				get { return true; }
			}

			#region ISite Members

			public IComponent Component
			{
				get
				{
					// TODO:  Add TestSite.Component getter implementation
					return null;
				}
			}

			public IContainer Container
			{
				get
				{
					// TODO:  Add TestSite.Container getter implementation
					return null;
				}
			}

			public string Name
			{
				get
				{
					// TODO:  Add TestSite.Name getter implementation
					return null;
				}
				set
				{
					// TODO:  Add TestSite.Name setter implementation
				}
			}

			#endregion

			#region IServiceProvider Members

			public object GetService(Type serviceType)
			{
				if (serviceType == typeof(IDesignerHost))
				{
					return DesignerHost;
				}
				return null;
			}

			#endregion
		}

		class TestDesignerHost : IDesignerHost
		{
			public IComponent RootComponent;

			IComponent IDesignerHost.RootComponent
			{
				get { return RootComponent; }
			}

			#region IDesignerHost Members

			public IContainer Container
			{
				get
				{
					// TODO:  Add TestDesignerHost.Container getter implementation
					return null;
				}
			}

			public event EventHandler TransactionOpening
			{
				add { }
				remove { }
			}

			public event EventHandler TransactionOpened
			{
				add { }
				remove { }
			}

			public event EventHandler LoadComplete
			{
				add { }
				remove { }
			}

			public IDesigner GetDesigner(IComponent component)
			{
				// TODO:  Add TestDesignerHost.GetDesigner implementation
				return null;
			}

			public event EventHandler Activated
			{
				add { }
				remove { }
			}

			public event EventHandler Deactivated
			{
				add { }
				remove { }
			}

			public event DesignerTransactionCloseEventHandler TransactionClosed
			{
				add { }
				remove { }
			}

			public bool Loading
			{
				get
				{
					// TODO:  Add TestDesignerHost.Loading getter implementation
					return false;
				}
			}

			public IComponent CreateComponent(Type componentClass, string name)
			{
				// TODO:  Add TestDesignerHost.CreateComponent implementation
				return null;
			}

			IComponent IDesignerHost.CreateComponent(Type componentClass)
			{
				// TODO:  Add TestDesignerHost.System.ComponentModel.Design.IDesignerHost.CreateComponent implementation
				return null;
			}

			public bool InTransaction
			{
				get
				{
					// TODO:  Add TestDesignerHost.InTransaction getter implementation
					return false;
				}
			}

			public string TransactionDescription
			{
				get
				{
					// TODO:  Add TestDesignerHost.TransactionDescription getter implementation
					return null;
				}
			}

			public void DestroyComponent(IComponent component)
			{
				// TODO:  Add TestDesignerHost.DestroyComponent implementation
			}

			public void Activate()
			{
				// TODO:  Add TestDesignerHost.Activate implementation
			}

			public string RootComponentClassName
			{
				get
				{
					// TODO:  Add TestDesignerHost.RootComponentClassName getter implementation
					return null;
				}
			}

			public Type GetType(string typeName)
			{
				// TODO:  Add TestDesignerHost.GetType implementation
				return null;
			}

			public event DesignerTransactionCloseEventHandler TransactionClosing
			{
				add { }
				remove { }
			}

			public DesignerTransaction CreateTransaction(string description)
			{
				// TODO:  Add TestDesignerHost.CreateTransaction implementation
				return null;
			}

			DesignerTransaction IDesignerHost.CreateTransaction()
			{
				// TODO:  Add TestDesignerHost.System.ComponentModel.Design.IDesignerHost.CreateTransaction implementation
				return null;
			}

			#endregion

			#region IServiceContainer Members

			public void RemoveService(Type serviceType, bool promote)
			{
				// TODO:  Add TestDesignerHost.RemoveService implementation
			}

			void IServiceContainer.RemoveService(Type serviceType)
			{
				// TODO:  Add TestDesignerHost.System.ComponentModel.Design.IServiceContainer.RemoveService implementation
			}

			public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
			{
				// TODO:  Add TestDesignerHost.AddService implementation
			}

			void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback)
			{
				// TODO:  Add TestDesignerHost.System.ComponentModel.Design.IServiceContainer.AddService implementation
			}

			void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
			{
				// TODO:  Add TestDesignerHost.System.ComponentModel.Design.IServiceContainer.AddService implementation
			}

			void IServiceContainer.AddService(Type serviceType, object serviceInstance)
			{
				// TODO:  Add TestDesignerHost.System.ComponentModel.Design.IServiceContainer.AddService implementation
			}

			#endregion

			#region IServiceProvider Members

			public object GetService(Type serviceType)
			{
				// TODO:  Add TestDesignerHost.GetService implementation
				return null;
			}

			#endregion
		}

		#endregion
	}
}
