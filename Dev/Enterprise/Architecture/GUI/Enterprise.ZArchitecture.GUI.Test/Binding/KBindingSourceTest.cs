using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.ComponentModel.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class KBindingSourceTest : TestCaseWithFactory
	{
		public void TestGetSetBindingMember()
		{
			var control1 = new TestControl();
			var control2 = new TestControlNoMetaData();
			Form.Controls.Add(control1);
			Form.Controls.Add(control2);

			BindingSource.DataSource = null;

			BindingSource.SetBindingMember(control1, "SomeProperty");
			BindingSource.SetBindingMember(control2, "SomeProperty");

			Form.Show();
			Application.DoEvents();

			AssertEquals("SomeProperty", BindingSource.GetBindingMember(control1));
			AssertEquals("SomeProperty", BindingSource.GetBindingMember(control2));
			AssertEquals("No bindings initially", 0, control1.DataBindings.Count);
			AssertEquals("No bindings initially", 0, control2.DataBindings.Count);

			BindingSource.SetBindingMember(control1, "");
			BindingSource.SetBindingMember(control2, "");
			AssertEquals("", BindingSource.GetBindingMember(control1));
			AssertEquals("", BindingSource.GetBindingMember(control2));
			AssertEquals("No bindings initially", 0, control1.DataBindings.Count);
			AssertEquals("No bindings initially", 0, control2.DataBindings.Count);

			BindingSource.SetBindingMember(control1, "SomeProperty");
			AssertEquals("No bindings initially", 0, control1.DataBindings.Count);
			AssertEquals("No bindings initially", 0, control2.DataBindings.Count);
			BindingSource.DataSource = Factory.New<TestObj>();
			AssertEquals("Should have 2 bindings", 2, control1.DataBindings.Count);
			AssertEquals("No bindings now", 0, control2.DataBindings.Count);

			BindingSource.SetBindingMember(control2, "SomeProperty");
			AssertEquals("Should have 1 binding", 1, control2.DataBindings.Count);

			BindingSource.SetBindingMember(control1, "");
			BindingSource.SetBindingMember(control2, "");
			AssertEquals("No binding should be set", 0, control1.DataBindings.Count);
			AssertEquals("No binding should be set", 0, control2.DataBindings.Count);

			BindingSource.SetBindingMember(control1, "SomeProperty");
			BindingSource.SetBindingMember(control2, "SomeProperty");
			AssertEquals("SomeProperty", BindingSource.GetBindingMember(control1));
			AssertEquals("SomeProperty", BindingSource.GetBindingMember(control2));
			AssertEquals("Should have 2 bindings", 2, control1.DataBindings.Count);
			AssertEquals("Should have 1 bindings", 1, control2.DataBindings.Count);

			BindingSource.SetBindingMember(control1, "NestedObject.NestedProperty");
			BindingSource.SetBindingMember(control2, "NestedObject.NestedProperty");
			AssertEquals("NestedObject.NestedProperty", BindingSource.GetBindingMember(control1));
			AssertEquals("NestedObject.NestedProperty", BindingSource.GetBindingMember(control2));
			AssertEquals("Should have 2 bindings", 2, control1.DataBindings.Count);
			AssertEquals("Should have 1 bindings", 1, control2.DataBindings.Count);

			BindingSource.DataSource = null;
			AssertEquals("NestedObject.NestedProperty", BindingSource.GetBindingMember(control1));
			AssertEquals("NestedObject.NestedProperty", BindingSource.GetBindingMember(control2));
			AssertEquals("No binding should be set", 0, control1.DataBindings.Count);
			AssertEquals("No binding should be set", 0, control2.DataBindings.Count);
		}

		public void TestBindingIncludingMetaData()
		{
			var control = new TestControl();
			Form.Controls.Add(control);
			Form.Show();
			Application.DoEvents();

			BindingSource.SetBindingMember(control, "NestedObject.NestedProperty");
			var o = Factory.New<TestObj>();
			o.NestedObject.NestedProperty = "";
			BindingSource.DataSource = o;

			AssertEquals("Value and meta data should be bound", 2, control.DataBindings.Count);
			AssertEquals(control.Text, o.NestedObject.NestedProperty);
			AssertEquals(control.ControlMetaDataProperty, o.NestedObject.NestedPropertyMetaData);

			o.NestedObject.NestedProperty = "newtext";
			AssertEquals(control.Text, o.NestedObject.NestedProperty);

			o.NestedObject.NestedPropertyMetaData = 5;
			AssertEquals(control.ControlMetaDataProperty, o.NestedObject.NestedPropertyMetaData);
		}

		public void TestExceptionThrownIfRequiredMetaDataNotDefinedOnBusinessProperty()
		{
			var ctrl = new KListBox();
			Form.Controls.Add(ctrl);

			var o = Factory.New<TestObj>();
			BindingSource.DataSource = o;

			try
			{
				BindingSource.SetBindingMember(ctrl, "NestedObject.NestedProperty");
				Form.Show();
				Application.DoEvents();
			}
			catch (KDataBindingException)
			{
			}

			AssertNotNull("Expected an exception as ListDataSource not specified on the property", ErrorReporter.LastExceptionReported);
			AssertEquals(
				"Exception raised with a descriptive exception message",
				true, ErrorReporter.LastExceptionReported.Message.Contains("Could not find required meta-data ListDataSource"));
			ErrorReporter.Clear();
			BindingSource.SetDataBinding(null, "");
		}

		#region Implementation

		#region Properties

		TestBindingSource BindingSource
		{
			get
			{
				if (bindingSource == null)
				{
					bindingSource = new TestBindingSource(Form);
				}
				return bindingSource;
			}
		}
		TestBindingSource bindingSource;

		TestForm Form
		{
			get
			{
				if (form == null)
				{
					form = new TestForm();
					form.Site = Site;
				}
				return form;
			}
		}
		TestForm form;

		readonly Container Container = new Container();

		MockSite Site
		{
			get
			{
				if (site == null)
				{
					site = new MockSite(Container);
				}
				return site;
			}
		}
		MockSite site;

		#endregion

		#region Test classes

		[EditorBrowsable(EditorBrowsableState.Never)]
		public class TestObj : DummyBusinessObject
		{
			public TestObj(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			[MetaDataMember(Enterprise.ZArchitecture.GUI.ControlBindingTest.MockMetaDataType.TestType, "SomePropertyMetaData")]
			public string SomeProperty
			{
				get
				{ return this.someProperty; }
				set
				{
					this.someProperty = value;
					if (SomePropertyChanged != null)
					{
						SomePropertyChanged(this, EventArgs.Empty);
					}
				}
			}
			public event EventHandler SomePropertyChanged;
			string someProperty = "";

			public int SomePropertyMetaData
			{
				get { return somePropertyMetaData; }
				set
				{
					somePropertyMetaData = value;
					if (SomePropertyMetaDataChanged != null)
					{
						SomePropertyMetaDataChanged(this, EventArgs.Empty);
					}
				}
			}
			public event EventHandler SomePropertyMetaDataChanged;
			int somePropertyMetaData;

			public TestNestedObject NestedObject
			{ get { return Factory.New<TestNestedObject>(); } }

			public KBindingList<TestNestedObject> ChildCollection
			{
				get { return childCollection ?? (childCollection = new KBindingList<TestNestedObject>()); }
			}
			KBindingList<TestNestedObject> childCollection;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public class TestNestedObject : DummyBusinessObject
		{
			public TestNestedObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				NestedProperty = "";
			}

			[MetaDataMember(Enterprise.ZArchitecture.GUI.ControlBindingTest.MockMetaDataType.TestType, "NestedPropertyMetaData")]
			public string NestedProperty { get; set; }
			public int NestedPropertyMetaData { get; set; }
		}

		class TestBindingSource : KBindingSource
		{
			public TestBindingSource()
			{ DataSourceType = typeof(object); }

			public TestBindingSource(IContainer container)
				: base(container)
			{ DataSourceType = typeof(object); }

			public TestBindingSource(ContainerControl containerControl)
				: base(containerControl)
			{ DataSourceType = typeof(object); }
		}

		[DefaultBindingProperty("Text")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestControl : TextBox
		{
			[BindingMetaDataProperty(Enterprise.ZArchitecture.GUI.ControlBindingTest.MockMetaDataType.TestType, "ControlMetaDataProperty")]
			public override string Text
			{
				get { return base.Text; }
				set { base.Text = value; }
			}

			public int ControlMetaDataProperty
			{
				get { return controlMetaDataProperty; }
				set
				{
					controlMetaDataProperty = value;
					if (ControlMetaDataPropertyChanged != null)
					{
						ControlMetaDataPropertyChanged(this, EventArgs.Empty);
					}
				}
			}
			int controlMetaDataProperty;
			public event EventHandler ControlMetaDataPropertyChanged;
		}

		[DefaultBindingProperty("Text")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestControlNoMetaData : TextBox
		{
		}

		class MockSite : ISite
		{
			readonly IContainer container;

			public MockSite(IContainer container)
			{ this.container = container; }

			public IComponent Component
			{ get { throw new Exception("The method or operation is not implemented."); } }

			public IContainer Container
			{ get { return container; } }

			public bool DesignMode { get; set; }

			public string Name
			{
				get { return this.name; }
				set { this.name = value; }
			}
			string name;

			public object GetService(Type serviceType)
			{ return null; }
		}

		#endregion

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();
			Enterprise.ZArchitecture.GUI.ControlBindingTest.MockMetaDataType.RegisterTypes();
		}

		protected override void TearDown()
		{
			base.TearDown();
			KDesignerActionList.IsSettingDesignerActionListProperty = false;
			if (form != null)
			{
				form.Dispose();
			}
			if (bindingSource != null)
			{
				bindingSource.Dispose();
			}
		}

		#endregion

		#endregion
	}
}
