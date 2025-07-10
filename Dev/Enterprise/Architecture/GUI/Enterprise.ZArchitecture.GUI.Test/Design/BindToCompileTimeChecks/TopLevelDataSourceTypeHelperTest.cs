using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Design.Testing
{
	sealed class TopLevelDataSourceTypeHelperTest : TestCase
	{
		public void TestDataSourceType_AtDesignTime()
		{
			TopLevelControl.Site = new MockISite(TopLevelControl);
			TestDataSourceType();
		}

		public void TestDataSourceType_AtRunTime()
		{
			TopLevelControl.Site = null;
			TestDataSourceType();
		}

		void TestDataSourceType()
		{
			TopLevelControl.DataSourceAssemblyName = typeof(BusinessObject).Assembly.GetName().Name;
			TopLevelControl.DataSourceTypeName = typeof(BusinessObject).FullName;
			AssertEquals("Should find the correct type", typeof(BusinessObject), TopLevelControl.DataSourceType);
		}

		public void TestDataSourceType_WhenTypeNotFound()
		{
			TopLevelControl.DataSourceAssemblyName = "AssemblyThatDoesntExist";
			TopLevelControl.DataSourceTypeName = typeof(BusinessObject).FullName;
			AssertEquals("Type not found", null, TopLevelControl.DataSourceType);
		}

		public void TestDataSourceType_FromBindingSource()
		{
			TopLevelControl.BindingSource.DataSourceType = typeof(int);
			AssertEquals(typeof(int), TopLevelControl.DataSourceType);
		}

		#region Test Classes

		class TestTopLevelControl : Component, IDesignTimeDataSourceType, ICompositeControlBindingSourceProvider
		{
			#region IDesignTimeDataSourceType

			TopLevelDataSourceTypeHelper DesignTimeDataSourceTypeHelper
			{ get { return designTimeDataSourceTypeHelper ?? (designTimeDataSourceTypeHelper = new TopLevelDataSourceTypeHelper(this)); } }
			TopLevelDataSourceTypeHelper designTimeDataSourceTypeHelper;

			[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
			public string DataSourceAssemblyName
			{
				get { return DesignTimeDataSourceTypeHelper.DataSourceAssemblyName; }
				set { DesignTimeDataSourceTypeHelper.DataSourceAssemblyName = value; }
			}

			[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
			public string DataSourceTypeName
			{
				get { return DesignTimeDataSourceTypeHelper.DataSourceTypeName; }
				set { DesignTimeDataSourceTypeHelper.DataSourceTypeName = value; }
			}

			[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
			public Type DataSourceType
			{
				get { return DesignTimeDataSourceTypeHelper.DataSourceType; }
			}

			#endregion

			#region BindingSource

			public ICompositeControlBindingSource BindingSource
			{
				get { return bindingSource ?? (bindingSource = new ZBindingSource()); }
			}
			ICompositeControlBindingSource bindingSource;

			#endregion
		}

		class MockISite : ISite
		{
			public MockISite(IComponent rootComponent)
			{
				this.RootComponent = rootComponent;
			}

			public IComponent RootComponent { get; set; }

			public object GetService(Type serviceType)
			{
				object result = null;
				if (serviceType == typeof(IDesignerHost))
				{
					result = new MockIDesignerHost(RootComponent);
				}
				else if (serviceType == typeof(ITypeResolutionService))
				{
					result = new MockITypeResolutionService();
				}
				return result;
			}

			#region ISite Unsupported Members

			public IComponent Component
			{
				get
				{
					// TODO:  Add MockSite.Component getter implementation
					return null;
				}
			}

			public IContainer Container
			{
				get
				{
					// TODO:  Add MockSite.Container getter implementation
					return null;
				}
			}

			public bool DesignMode
			{
				get
				{
					// TODO:  Add MockSite.DesignMode getter implementation
					return false;
				}
			}

			public string Name
			{
				get
				{
					// TODO:  Add MockSite.Name getter implementation
					return null;
				}
				set
				{
					// TODO:  Add MockSite.Name setter implementation
				}
			}

			#endregion
		}

		class MockIDesignerHost : IDesignerHost
		{
			public MockIDesignerHost(IComponent rootComponent)
			{
				this.rootComponent = rootComponent;
			}

			public IComponent RootComponent
			{
				get { return rootComponent; }
			}
			readonly IComponent rootComponent;

			#region IDesignerHost Unsupported Members

			public IContainer Container
			{
				get
				{
					// TODO:  Add MockComponentHost.Container getter implementation
					return null;
				}
			}

			public event EventHandler TransactionOpening
			{
				add { throw new NotSupportedException(); }
				remove { throw new NotSupportedException(); }
			}

			public event EventHandler TransactionOpened
			{
				add { throw new NotSupportedException(); }
				remove { throw new NotSupportedException(); }
			}

			public event EventHandler LoadComplete
			{
				add { throw new NotSupportedException(); }
				remove { throw new NotSupportedException(); }
			}

			public IDesigner GetDesigner(IComponent component)
			{
				// TODO:  Add MockComponentHost.GetDesigner implementation
				return null;
			}

			public event EventHandler Activated
			{
				add { throw new NotSupportedException(); }
				remove { throw new NotSupportedException(); }
			}

			public event EventHandler Deactivated
			{
				add { throw new NotSupportedException(); }
				remove { throw new NotSupportedException(); }
			}

			public event DesignerTransactionCloseEventHandler TransactionClosed
			{
				add { throw new NotSupportedException(); }
				remove { throw new NotSupportedException(); }
			}

			public bool Loading
			{
				get
				{
					// TODO:  Add MockComponentHost.Loading getter implementation
					return false;
				}
			}

			public IComponent CreateComponent(Type componentClass, string name)
			{
				// TODO:  Add MockComponentHost.CreateComponent implementation
				return null;
			}

			IComponent IDesignerHost.CreateComponent(Type componentClass)
			{
				// TODO:  Add MockComponentHost.System.ComponentModel.Design.IDesignerHost.CreateComponent implementation
				return null;
			}

			public bool InTransaction
			{
				get
				{
					// TODO:  Add MockComponentHost.InTransaction getter implementation
					return false;
				}
			}

			public string TransactionDescription
			{
				get
				{
					// TODO:  Add MockComponentHost.TransactionDescription getter implementation
					return null;
				}
			}

			public void DestroyComponent(IComponent component)
			{
				// TODO:  Add MockComponentHost.DestroyComponent implementation
			}

			public void Activate()
			{
				// TODO:  Add MockComponentHost.Activate implementation
			}

			public string RootComponentClassName
			{
				get
				{
					// TODO:  Add MockComponentHost.RootComponentClassName getter implementation
					return null;
				}
			}

			public Type GetType(string typeName)
			{
				// TODO:  Add MockComponentHost.GetType implementation
				return null;
			}

			public event DesignerTransactionCloseEventHandler TransactionClosing
			{
				add { throw new NotSupportedException(); }
				remove { throw new NotSupportedException(); }
			}

			public DesignerTransaction CreateTransaction(string description)
			{
				// TODO:  Add MockComponentHost.CreateTransaction implementation
				return null;
			}

			DesignerTransaction IDesignerHost.CreateTransaction()
			{
				// TODO:  Add MockComponentHost.System.ComponentModel.Design.IDesignerHost.CreateTransaction implementation
				return null;
			}

			#endregion

			#region IServiceContainer Unsupported Members

			public void RemoveService(Type serviceType, bool promote)
			{
				// TODO:  Add MockComponentHost.RemoveService implementation
			}

			void IServiceContainer.RemoveService(Type serviceType)
			{
				// TODO:  Add MockComponentHost.System.ComponentModel.Design.IServiceContainer.RemoveService implementation
			}

			public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
			{
				// TODO:  Add MockComponentHost.AddService implementation
			}

			void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback)
			{
				// TODO:  Add MockComponentHost.System.ComponentModel.Design.IServiceContainer.AddService implementation
			}

			void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
			{
				// TODO:  Add MockComponentHost.System.ComponentModel.Design.IServiceContainer.AddService implementation
			}

			void IServiceContainer.AddService(Type serviceType, object serviceInstance)
			{
				// TODO:  Add MockComponentHost.System.ComponentModel.Design.IServiceContainer.AddService implementation
			}

			#endregion

			#region IServiceProvider Unsupported Members

			public object GetService(Type serviceType)
			{
				throw new NotSupportedException();
			}

			#endregion
		}

		class MockITypeResolutionService : ITypeResolutionService
		{
			public Type GetType(string name)
			{
				return name == typeof(BusinessObject).FullName ? typeof(BusinessObject) : Type.GetType(name);
			}

			#region ITypeResolutionService Unsupported Members

			public Assembly GetAssembly(AssemblyName name, bool throwOnError)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public Assembly GetAssembly(AssemblyName name)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public string GetPathOfAssembly(AssemblyName name)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public Type GetType(string name, bool throwOnError, bool ignoreCase)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public Type GetType(string name, bool throwOnError)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public void ReferenceAssembly(AssemblyName name)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			#endregion
		}

		#endregion

		#region Implementation

		TestTopLevelControl TopLevelControl
		{
			get
			{
				if (topLevelControl == null)
				{
					topLevelControl = new TestTopLevelControl();
				}
				return topLevelControl;
			}
		}
		TestTopLevelControl topLevelControl;

		protected override void TearDown()
		{
			base.TearDown();
			if (topLevelControl != null)
			{
				topLevelControl.Dispose();
			}
		}

		#endregion
	}
}
