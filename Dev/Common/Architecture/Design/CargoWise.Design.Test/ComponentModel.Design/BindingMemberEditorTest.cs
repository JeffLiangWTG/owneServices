using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Design.Testing
{
	class BindingMemberEditorTest : TestCase
	{
		[GuiTest]
		public void TestEditor_OnBindingSource_BindingMember()
		{
			EditorAttribute attr = (EditorAttribute)BindingMemberProperty.Attributes[typeof(EditorAttribute)];
			UITypeEditor editor = (UITypeEditor)BindingMemberProperty.GetEditor(typeof(UITypeEditor));
			AssertEquals("Editor", typeof(BindingMemberEditor), editor.GetType());
		}

		#region Test Classes
		class TestSite : ISite
		{
			readonly KBindingSource bindingSource;
			readonly Control control;
			public TestSite(KBindingSource bindingSource, Control control)
			{
				this.bindingSource = bindingSource;
				this.control = control;
			}

			object IServiceProvider.GetService(Type serviceType)
			{
				if (serviceType == typeof(IExtenderListService))
				{
					return new TestExtenderListService(bindingSource);
				}

				return null;
			}

			#region ISite Members
			IComponent ISite.Component
			{
				get
				{
					return control;
				}
			}

			IContainer ISite.Container
			{
				get
				{
					return new Container();
				}
			}

			bool ISite.DesignMode
			{
				get
				{
					return false;
				}
			}

			string ISite.Name
			{
				get
				{
					return control.Name;
				}

				set
				{
					control.Name = value;
				}
			}
			#endregion
		}

		class TestExtenderListService : IExtenderListService
		{
			public TestExtenderListService(KBindingSource bindingSource)
			{
				this.bindingSource = bindingSource;
			}

			public IExtenderProvider[] GetExtenderProviders()
			{
				return new IExtenderProvider[] { bindingSource };
			}

			readonly KBindingSource bindingSource;
		}

		public class TestBoundForm : KForm
		{
			public TestBoundForm()
			{
				BindingSource.DataSourceType = typeof(List<TestElementType1>);
			}

			public new KBindingSource BindingSource
			{
				get
				{
					return base.BindingSource;
				}
			}
		}

		class TestElementType1
		{
			public List<TestElementType2> List
			{
				get
				{
					return null;
				}
			}
		}

		class TestElementType2
		{
			public string Property
			{
				get
				{
					return "";
				}
			}
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
		}

		protected override void TearDown()
		{
			base.TearDown();
			AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			if (boundForm != null)
			{
				boundForm.Dispose();
			}

			if (propertyGrid != null)
			{
				propertyGrid.Dispose();
			}

			if (boundControl != null)
			{
				boundControl.Dispose();
			}
		}

		Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (!inCurrentDomain_AssemblyResolve)
			{
				inCurrentDomain_AssemblyResolve = true;
				try
				{
					return Assembly.Load(args.Name);
				}
				finally
				{
					inCurrentDomain_AssemblyResolve = false;
				}
			}

			return null;
		}

		bool inCurrentDomain_AssemblyResolve;
		TestBoundForm BoundForm
		{
			get
			{
				if (boundForm == null)
				{
					boundForm = new TestBoundForm();
				}

				return boundForm;
			}
		}

		TestBoundForm boundForm;
		readonly PropertyGrid propertyGrid;
		TextBox BoundControl
		{
			get
			{
				if (boundControl == null)
				{
					boundControl = new KTextBox();
					boundControl.Site = new TestSite(BoundForm.BindingSource, boundControl);
				}

				return boundControl;
			}
		}

		TextBox boundControl;
		PropertyDescriptor BindingMemberProperty
		{
			get
			{
				if (bindingMemberProperty == null)
				{
					bindingMemberProperty = TypeDescriptor.GetProperties(BoundControl)["BindingMember"];
				}

				return bindingMemberProperty;
			}
		}

		PropertyDescriptor bindingMemberProperty;
		#endregion
	}
}
