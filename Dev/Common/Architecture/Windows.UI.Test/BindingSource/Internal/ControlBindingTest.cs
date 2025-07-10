using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ControlBindingTest : TestCase
	{
		static ControlBindingTest()
		{ 
			MockMetaDataType.RegisterTypes(); 
		}

		#region SetNameFromBindingMemberIfRequired

		public void TestSetNameFromBindingMemberIfRequired()
		{
			KTextBox ctrlWithGeneratedName = new KTextBox();
			ctrlWithGeneratedName.Name = "InitialName";

			KDesignerActionList.IsSettingDesignerActionListProperty = true;
			BindingSource.SynchroniseNameWithBindingMemberInSmartTags = true;

			using (ComponentExtensions.SwitchToDesignMode())
			{
				BindingSource.SynchroniseNameWithBindingMemberInSmartTags = false;
				BindingSource.SetBindingMember(ctrlWithGeneratedName, "DecoyBindingMember");
				AssertEquals("When DesignMode=true SynchroniseNameWithBindingMemberInSmartTags=false", "InitialName", ctrlWithGeneratedName.Name);
			}

			BindingSource.SynchroniseNameWithBindingMemberInSmartTags = true;
			BindingSource.SetBindingMember(ctrlWithGeneratedName, "DecoyBindingMember2");
			AssertEquals("When DesignMode=false SynchroniseNameWithBindingMemberInSmartTags=true", "InitialName", ctrlWithGeneratedName.Name);

			using (ComponentExtensions.SwitchToDesignMode())
			{
				BindingSource.SynchroniseNameWithBindingMemberInSmartTags = true;
				BindingSource.SetBindingMember(ctrlWithGeneratedName, "ChildEntity.ParentEntity+BindingMember");
				AssertEquals("When DesignMode=true SynchroniseNameWithBindingMemberInSmartTags=true", "txtChildEntity_ParentEntity_BindingMember", ctrlWithGeneratedName.Name);
			}

			BindingSource.Site = Site;
			Component componentWithSameName = new Component();
			Container.Add(componentWithSameName, "txtBindingMember");

			ctrlWithGeneratedName.Name = "no_name";
			BindingSource.SetBindingMember(ctrlWithGeneratedName, "");
			BindingSource.SetBindingMember(ctrlWithGeneratedName, "BindingMember");
			AssertEquals(
				"When a component with that name already exists, another name should be used",
				"txtBindingMember1", ctrlWithGeneratedName.Name);
		}

		#endregion

		#region Compile Time Checking Code Serialization

		public void TestBindingMembersForCompileTimeCheck_EmptyBindTo()
		{
			using (ComponentExtensions.SwitchToDesignMode())
			{
				BindingSource.DataMember = "top";
				BindingSource.DataSourceType = typeof(TestComponent);
				KTextBox ctrl = new KTextBox();

				BindingSource.SetBindingMember(ctrl, "");
				AssertEquals("Empty bindto", 0, BindingSource.GetBindingMembersForCompileTimeCheck(ctrl).Count);
			}
		}

		public void TestBindingMembersForCompileTimeCheck_DotBindTo()
		{
			using (ComponentExtensions.SwitchToDesignMode())
			{
				BindingSource.DataMember = ".";
				BindingSource.DataSourceType = typeof(TestComponent);
				using (KUserControl ctrl = new KUserControl())
				{
					BindingSource.SetBindingMember(ctrl, ".");
					AssertEquals("Empty bindto", 0, BindingSource.GetBindingMembersForCompileTimeCheck(ctrl).Count);
				}
			}
		}

		public void TestBindingMembersForCompileTimeCheck_WithBindTo()
		{
			using (ComponentExtensions.SwitchToDesignMode())
			{
				BindingSource.DataSourceType = typeof(TestComponent);

				BindingSource.SetBindingMember(BoundControl, "x");
				AssertEquals("With bindto", 1, BindingSource.GetBindingMembersForCompileTimeCheck(BoundControl).Count);
				AssertEquals("With bindto", "x", BindingSource.GetBindingMembersForCompileTimeCheck(BoundControl)[0].BindingMember);
				AssertEquals("With bindto", typeof(string), BindingSource.GetBindingMembersForCompileTimeCheck(BoundControl)[0].ControlPropertyType);
			}
		}

		#endregion

		#region StartBinding / StopBinding

		public void TestFailedBinding()
		{
			Form.Controls.Add(BoundControl);
			Form.Controls.Add(BoundControl2);
			Form.Show();

			BindingSource.SetBindingMember(BoundControl, "BodgeyProperty1");
			BindingSource.SetBindingMember(BoundControl2, "BodgeyRelationshipProp.BodgeyProperty2");

			try
			{
				BindingSource.DataSource = new TestComponent();
				Fail("Should have thrown an exception");
			}
			catch (KDataBindingException e)
			{
				Assert(
					"The error message should specify the bodgey properties",
					e.Message.Contains("BodgeyProperty1"));
				Assert(
					"The error message should specify the bodgey properties",
					e.Message.Contains("BodgeyRelationshipProp.BodgeyProperty2"));
			}
			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestBindOnUnbindableControl()
		{
			TextBox unbindableControl = new TextBox();
			Form.Controls.Add(unbindableControl);
			Form.Show();
			BindingSource.DataSource = new object();
			try
			{
				BindingSource.SetBindingMember(unbindableControl, "xxx");
				Fail("Exception should have been thrown");
			}
			catch (KDataBindingException)
			{
			}
			ErrorReporter.Clear();
		}

		// Add controls to a form that has a binder, much the way the designer does it.
		[ExpectNoExceptions]
		public void TestBinding_WhenAddingControlsDynamically()
		{
			Container.Add(Form);
			Container.Add(BindingSource);

			BindingSource.DataSource = null;
			Form.Controls.Add(BoundControl);
			BindingSource.SetBindingMember(BoundControl, "blah");

			Form.Controls.Add(BoundControlNoMetaData);
			BindingSource.SetBindingMember(BoundControlNoMetaData, "blah");
			BindingSource.DataSource = null;
		}

		public void TestUsingIDataBoundControlImpl()
		{
			TestDataBoundControlImpl ctrl = new TestDataBoundControlImpl();
			Form.Controls.Add(ctrl);

			BindingSource.SetBindingMember(ctrl, "NestedObject.NestedProperty");
			object dataSource = new TestComponent();
			BindingSource.DataSource = dataSource;

			Form.Show();

			AssertEquals("DataSource", dataSource, ctrl.DataSource);
			AssertEquals("DataMember", "NestedObject.NestedProperty", ctrl.DataMember);

			BindingSource.DataSource = null;
			AssertNull("Unbound DataSource", ctrl.DataSource);
		}

		public void TestUsingIDataBoundControlImpl_NullReferenceException()
		{
			var ctrl = new TestDataBoundControlImpl_NullReferenceException();
			ctrl.Name = "Data Bound Control Name";
			Form.Controls.Add(ctrl);
			Form.Show();

			BindingSource.SetBindingMember(ctrl, "NestedObject.NestedProperty");

			try
			{
				BindingSource.DataSource = new TestComponent();
				Fail("Should have thrown an exception");
			}
			catch (KDataBindingException e)
			{
				Assert(
					"The error message should specify the control type",
					e.Message.Contains("TestDataBoundControlImpl_NullReferenceException"));
				Assert(
					"The error message should specify the control name",
					e.Message.Contains("Data Bound Control Name"));
				Assert(
					"The error message should specify the data source type",
					e.Message.Contains("TestComponent"));
				Assert(
					"The error message should specify the binding member name",
					e.Message.Contains("NestedObject.NestedProperty"));
			}
			ErrorReporter.Clear();
		}

		#endregion

		#region Test Classes

		class MockMetaDataType
		{
			public const string TestType = "KBindingSource.Test";

			public static void RegisterTypes()
			{
				MetaDataType.RegisterMetaDataType(
					new MetaDataType(TestType, typeof(int), 0));
			}
		}

		public class TestComponent : ComponentModel.Testing.KComponent
		{
			[MetaDataMember(MockMetaDataType.TestType, "PropertyMetaData")]
			public string Property
			{
				get { return property; }
				set
				{
					property = value;
					if (PropertyChanged != null)
					{
						PropertyChanged(this, EventArgs.Empty);
					}
				}
			}
			string property = "";

			public event EventHandler PropertyChanged;

			int propertyMetaData;
			public int PropertyMetaData
			{
				get { return propertyMetaData; }
				set
				{
					propertyMetaData = value;
					if (PropertyMetaDataChanged != null)
					{
						PropertyMetaDataChanged(this, EventArgs.Empty);
					}
				}
			}
			public event EventHandler PropertyMetaDataChanged;

			public TestNestedComponent NestedObject
			{ get { return new TestNestedComponent(); } }

			public List<TestComponent> Collection
			{
				get
				{
					List<TestComponent> result = new List<TestComponent>();
					result.Add(new TestComponent());
					return result;
				}
			}
		}

		public class TestNestedComponent : ComponentModel.Testing.KComponent
		{
			public TestNestedComponent()
			{ NestedProperty = ""; }

			[MetaDataMember(MockMetaDataType.TestType, "NestedPropertyMetaData")]
			public string NestedProperty { get; set; }

			public int NestedPropertyMetaData { get; set; }
		}

		[DefaultBindingProperty("Text")]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestControlNoMetaData : TextBox
		{
		}

		[DesignTimeControlNameGenerator("txt")]
		[DefaultBindingProperty("Text")]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestBoundControl : TextBox
		{
			[BindingMetaDataProperty(MockMetaDataType.TestType, "ControlMetaDataProperty")]
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

		[DefaultBindingProperty("ToBeIgnored")]
		class TestDataBoundControlImpl : Control, IDataBoundControl
		{
			public void SetDataBinding(object dataSource, string dataMember)
			{
				this.DataSource = dataSource;
				this.DataMember = dataMember;
			}

			public object DataSource;
			public string DataMember;

			public object ToBeIgnored
			{
				get { return null; }
				set { }
			}

			Type IDataBoundControl.DataSourceType
			{ get { return typeof(object); } }

			object IDataBoundControl.DataSource
			{ get { return null; } }

			string IDataBoundControl.DataMember
			{ get { return null; } }
		}

		[DefaultBindingProperty("ToBeIgnored")]
		class TestDataBoundControlImpl_NullReferenceException : Control, IDataBoundControl
		{
			public void SetDataBinding(object dataSource, string dataMember)
			{
				throw new NullReferenceException("Object reference not set to an instance of an object.");
			}

			public object ToBeIgnored
			{
				get { return null; }
				set { }
			}

			Type IDataBoundControl.DataSourceType
			{ get { return typeof(object); } }

			object IDataBoundControl.DataSource
			{ get { return null; } }

			string IDataBoundControl.DataMember
			{ get { return null; } }
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public class TestForm : KForm
		{
			public TestForm()
			{ BindingSource.DataSourceType = typeof(object); }
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

			public bool DesignMode
			{ get { return true; } }

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

		#region Implementation

		TestForm Form
		{
			get
			{
				if (form == null)
				{
					form = new TestForm();
				}
				return form;
			}
		}
		TestForm form;

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

		Container Container
		{
			get
			{
				if (container == null)
				{
					container = new Container();
				}
				return container;
			}
		}
		Container container;

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

		TestBoundControl BoundControl
		{
			get
			{
				if (boundControl == null)
				{
					boundControl = new TestBoundControl();
				}
				return boundControl;
			}
		}
		TestBoundControl boundControl;

		TestBoundControl BoundControl2
		{
			get
			{
				if (boundControl2 == null)
				{
					boundControl2 = new TestBoundControl();
				}
				return boundControl2;
			}
		}
		TestBoundControl boundControl2;

		TestControlNoMetaData BoundControlNoMetaData
		{
			get
			{
				if (boundControlNoMetaData == null)
				{
					boundControlNoMetaData = new TestControlNoMetaData();
				}
				return boundControlNoMetaData;
			}
		}
		TestControlNoMetaData boundControlNoMetaData;

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
			if (boundControl != null)
			{
				boundControl.Dispose();
			}
			if (boundControl2 != null)
			{
				boundControl2.Dispose();
			}
			if (boundControlNoMetaData != null)
			{
				boundControlNoMetaData.Dispose();
			}
		}

		#endregion
	}
}
