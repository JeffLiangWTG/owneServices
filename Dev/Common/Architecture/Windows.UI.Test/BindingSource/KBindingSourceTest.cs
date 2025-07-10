using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Design;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.ComponentModel.Testing;
using CargoWise.Windows.UI.Design;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	sealed class KBindingSourceTest : TestCase
	{
		static KBindingSourceTest()
		{
			MockMetaDataType.RegisterTypes();
		}

		public void TestBindingContext_WithNullContainerControl_LikeTheDesigner()
		{
			BindingSource.ContainerControl = null;
			BindingSource.DataSource = new object();
			AssertNotNull(BindingSource.BindingContext);
		}

		public void TestBrowsableProperties()
		{
			BrowsableChecker.CheckBrowsableProperties(
				typeof(KBindingSource),
				"DataSourceType",
				"SynchroniseNameWithBindingMemberInSmartTags",
				"SupportMultipleTwoWayBoundPropertiesOnOneControl");
		}

		#region GetBindingSource

		public void TestGetBindingSource()
		{
			using (KForm form = new KForm())
			using (KUserControl userControl = new KUserControl())
			using (KGroupBox groupBox = new KGroupBox())
			using (KTextBox textBox = new KTextBox())
			{
				form.Controls.Add(userControl);
				userControl.Controls.Add(groupBox);
				groupBox.Controls.Add(textBox);
				AssertEquals("IBindingSource is implemented on textBox.Parent.Parent", ((ICompositeControlBindingSourceProvider)userControl).BindingSource, KBindingSource.GetBindingSource(textBox));
			}
		}

		public void TestGetBindingSource_FromUserControl()
		{
			using (KForm form = new KForm())
			using (KUserControl userControl = new KUserControl())
			{
				form.Controls.Add(userControl);
				AssertEquals("IBindingSource is implemented on userControl.Parent", ((ICompositeControlBindingSourceProvider)form).BindingSource, KBindingSource.GetBindingSource(userControl));
			}
		}

		#endregion

		#region Get/SetBindingMember

		// Add controls to a form that has a binder, much the way the designer does it.
		[ExpectNoExceptions]
		public void TestAddControlsDynamically()
		{
			Container.Add(BindingSource);

			KTextBox ctrl1 = new KTextBox();
			TestControlNoMetaData ctrl2 = new TestControlNoMetaData();

			BindingSource.DataSource = null;
			Form.Controls.Add(ctrl1);
			BindingSource.SetBindingMember(ctrl1, "blah");
			Form.Controls.Add(ctrl2);
			BindingSource.SetBindingMember(ctrl2, "blah");
			BindingSource.DataSource = null;
		}

		public void TestGetBindingMember_DefaultValueNotSet()
		{
			AssertEquals("No DefaultValueAttribute on GetBindingMember as it causes verbose SetBindingMember serialization in subclasses", 0, typeof(KBindingSource).GetMethod("GetBindingMember").GetCustomAttributes(typeof(DefaultValueAttribute), true).Length);
		}

		#endregion

		#region DataSourceType

#if !WINZOR // Don't need to test WinForms designer in Winzor
		public void TestDataSourceType_DesignerEditable()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			try
			{
				DesignerSerializerAttribute designer_attr = (DesignerSerializerAttribute)TypeDescriptor.GetAttributes(typeof(KBindingSource))[typeof(DesignerSerializerAttribute)];
				PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(KBindingSource))["DataSourceType"];

				AssertEquals(typeof(TypeFixCodeDomSerializer), Type.GetType(designer_attr.SerializerTypeName).BaseType);
				AssertEquals(property.Converter.GetType().FullName, typeof(TypeTypeConverter).FullName);
				AssertEquals(property.GetEditor(typeof(UITypeEditor)).GetType().FullName, Type.GetType(DesignerTypes.TypeValueIntellisenseEditor).FullName);
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			}
		}
#endif

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{ return Assembly.Load(args.Name); }

		public void TestDataSourceType_DoesntSetControlDataSourceAtDesignTimeIfAlreadySet()
		{
			using (KUserControl control = new KUserControl())
			using (ComponentExtensions.SwitchToDesignMode())
			{
				TestBindingSource dataSource = new TestBindingSource();
				dataSource.DataSource = typeof(MockMasterObject);
				control.SetDataBinding(dataSource, "DetailObjects");

				BindingSource.DataSourceType = typeof(MockMasterObject);
				BindingSource.SetBindingMember(control, ".");
				AssertEquals("If the control has a DataSource set on it at design time, dont touch it", dataSource, ((IDataBoundControl)control).DataSource);
				AssertEquals("If the control has a DataSource set on it at design time, dont touch it", "DetailObjects", ((IDataBoundControl)control).DataMember);

				BindingSource.DataSourceType = null;
				AssertEquals("If the control has a DataSource set on it at design time, dont touch it", dataSource, ((IDataBoundControl)control).DataSource);
				AssertEquals("If the control has a DataSource set on it at design time, dont touch it", "DetailObjects", ((IDataBoundControl)control).DataMember);
			}
		}

#endregion

		#region DataSource / DataMember

		public void TestDataMember()
		{
			TestDataBoundControlImpl ctrl = new TestDataBoundControlImpl();
			Form.Controls.Add(ctrl);

			BindingSource.SetBindingMember(ctrl, "NestedProperty");
			object dataSource = new TestObj();
			BindingSource.DataMember = "NestedObject";
			BindingSource.DataSource = dataSource;

			Form.Show();

			AssertEquals("DataSource", dataSource, ctrl.DataSource);
			AssertEquals("DataMember", "NestedObject.NestedProperty", ctrl.DataMember);

			BindingSource.DataSource = null;
			AssertNull("Unbound DataSource", ctrl.DataSource);
		}

		#endregion

		#region Current

		public void TestCurrent()
		{
			TestObj obj = new TestObj();
			BindingSource.SetDataBinding(obj, "ChildCollection");
			AssertEquals("Current when DataSourceType is not set", obj.ChildCollection, BindingSource.Current);

			TestNestedObject element = obj.ChildCollection.AddNew();
			BindingSource.DataSourceType = typeof(TestNestedObject);
			AssertEquals("Current when DataSourceType is the element", element, BindingSource.Current);

			BindingSource.DataSourceType = typeof(KBindingList<TestNestedObject>);
			AssertEquals("Current when DataSourceType is the collection", obj.ChildCollection, BindingSource.Current);
		}

		#endregion

		#region SetDataBinding

		public void TestFailedBinding()
		{
			TestControl control1 = new TestControl();
			TestControl control2 = new TestControl();
			Form.Controls.Add(control1);
			Form.Controls.Add(control2);
			Form.Show();

			BindingSource.SetBindingMember(control1, "BodgeyProperty1");
			BindingSource.SetBindingMember(control2, "BodgeyRelationshipProp.BodgeyProperty2");

			try
			{
				BindingSource.DataSource = new TestObj();
				Fail("Should have thrwon an exception");
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
			Control textbox = new Control();
			Form.Controls.Add(textbox);
			Form.Show();
			BindingSource.DataSource = new object();
			try
			{
				BindingSource.SetBindingMember(textbox, "xxx");
				Fail("Exception should have been thrown");
			}
			catch (KDataBindingException)
			{
			}
			ErrorReporter.Clear();
		}

		public void TestIDataBoundControlImpl()
		{
			TestDataBoundControlImpl ctrl = new TestDataBoundControlImpl();
			Form.Controls.Add(ctrl);

			BindingSource.SetBindingMember(ctrl, "NestedObject.NestedProperty");
			object data_source = new TestObj();
			BindingSource.DataSource = data_source;

			Form.Show();

			AssertEquals("DataSource", data_source, ctrl.DataSource);
			AssertEquals("DataMember", "NestedObject.NestedProperty", ctrl.DataMember);

			BindingSource.DataSource = null;
			AssertNull("Unbound DataSource", ctrl.DataSource);
		}

		#endregion

		#region Compile Time Checking Code

		public void TestGetBindingMembersForCompileTimeCheck_EmptyBindTo()
		{
			using (ComponentExtensions.SwitchToDesignMode())
			{
				BindingSource.DataMember = "top";
				BindingSource.DataSourceType = typeof(TestObj);
				KTextBox ctrl = new KTextBox();

				BindingSource.SetBindingMember(ctrl, "");
				AssertEquals("Empty bindto", 0, BindingSource.GetBindingMembersForCompileTimeCheck(ctrl).Count);

				BindingSource.SetDataBinding(null, "");
			}
		}

		public void TestGetBindingMembersForCompileTimeCheck_WithBindTo()
		{
			using (ComponentExtensions.SwitchToDesignMode())
			{
				BindingSource.DataSourceType = typeof(TestObj);
				KTextBox ctrl = new KTextBox();

				BindingSource.SetBindingMember(ctrl, "x");
				AssertEquals("With BindingMember", 1, BindingSource.GetBindingMembersForCompileTimeCheck(ctrl).Count);
				AssertEquals("With BindingMember", "x", BindingSource.GetBindingMembersForCompileTimeCheck(ctrl)[0].BindingMember);
				AssertEquals("With BindingMember", typeof(string), BindingSource.GetBindingMembersForCompileTimeCheck(ctrl)[0].ControlPropertyType);

				BindingSource.SetDataBinding(null, "");
			}
		}

		#endregion

		#region ForceBinding

		public void TestForceBinding()
		{
			KTextBox textBox = new KTextBox();
			BindingSource.SetBindingMember(textBox, "StringProperty");
			BindingSource.SetDataBinding(new MockMasterObject(), "");

			AssertEquals("TextBox not yet visible", false, textBox.Created);
			AssertEquals("TextBox not bound before ForceBinding()", false, textBox.DataBindings.Count > 0);
			bindingSource.ForceBinding(textBox);
			AssertEquals("TextBox bound after ForceBinding()", true, textBox.DataBindings.Count > 0);
		}

		#endregion

		#region Design-time Code Navigation Verbs

		public void TestNavigateToBoundProperty()
		{
			try
			{
				InvokeDesignerActionMethod(ControlBoundToNonExistantDataSourceType, "Go to Bound Property");
			}
			catch (ArgumentException ex)
			{
				AssertEquals("DTE object not available", ex.Message);
			}
		}

		public void TestOverrideBoundProperty()
		{
			try
			{
				InvokeDesignerActionMethod(ControlBoundToNonExistantDataSourceType, "Override Bound Property");
			}
			catch (ArgumentException ex)
			{
				AssertEquals("DTE object not available", ex.Message);
			}
		}

		void InvokeDesignerActionMethod(IComponent control, string displayName)
		{
			bool invoked = false;
			IDesignerActionItemSource itemSource = BindingSource;
			foreach (DesignerActionItem item in itemSource.GetSortedActionItems(new KDesignerActionList(control)))
			{
				KDesignerActionMethodItem method = item as KDesignerActionMethodItem;
				if (method != null)
				{
					if (method.DisplayName == displayName)
					{
						try
						{
							method.Invoke();
						}
						catch (TargetInvocationException ex)
						{
							throw ex.InnerException;
						}
						invoked = true;
					}
				}
			}
			if (!invoked)
			{
				Fail("Could not find designer action method with display name '" + displayName + "'");
			}
		}

		KTextBox ControlBoundToNonExistantDataSourceType
		{
			get
			{
				if (controlBoundToNonExistantDataSourceType == null)
				{
					controlBoundToNonExistantDataSourceType = new KTextBox();
					controlBoundToNonExistantDataSourceType.Site = Site;

					BindingSource.SetBindingMember(controlBoundToNonExistantDataSourceType, "Property");
					BindingSource.DataSourceType = new TypeNameHolder("NonExistantType");
				}
				return controlBoundToNonExistantDataSourceType;
			}
		}
		KTextBox controlBoundToNonExistantDataSourceType;

		#endregion

		#region Inner BindingSource

		public void TestUsingSystemWindowsFormsBinding_AccessingBindingSourceBeforeCallingSetDataBinding()
		{
			int count = ((IList)BindingSource).Count;
			TestUsingSystemWindowsFormsBinding();
		}

		public void TestUsingSystemWindowsFormsBinding()
		{
			MockMasterObject entity = new MockMasterObject();
			BindingSource.DataSourceType = typeof(MockDetailObject);
			BindingSource.SetDataBinding(entity, MockMasterObject.Properties.DetailObjects.Name);

			TextBox textBox = new TextBox();
			textBox.DataBindings.Add("Text", BindingSource, MockDetailObject.Properties.StringProperty.Name);
			Form.Controls.Add(textBox);

			Form.Show();

			MockDetailObject detail = entity.DetailObjects.AddNew();
			detail.StringProperty = "NewValue";
			AssertEquals("TextBox bound correctly using a System.Windows.Forms.Binding", "NewValue", textBox.Text);
		}

		#endregion

		#region IExtenderProvider

		public void TestCanExtend()
		{
			IExtenderProvider binder = BindingSource;
			BindingSource.ContainerControl = Form;

			AssertEquals(
				"Can't extend a control that is the root designed component",
				false, binder.CanExtend(Form));
			AssertEquals(
				"Can't extend a control with no data bound property",
				false, binder.CanExtend(new Control()));
			AssertEquals(
				"Can't extend a control with no data bound property",
				false, binder.CanExtend(new Control()));
			AssertEquals(
				"IDataBoundControl is extensible",
				true, binder.CanExtend(new TestIDataBoundControl()));
			AssertEquals(
				"Control with meta-data applied with no arguments extensible",
				true, binder.CanExtend(new TestPropertyBoundControl()));
			AssertEquals(
				"Can't extend a control without a zero argument " +
				"DBoundControlPropertyAttribute applied",
				false, binder.CanExtend(new TestNoValuePropertyBoundControl()));
		}

		public void TestExtenderProviderImpl()
		{
			TestControl control = new TestControl();
			KBindingSource binder = new TestBindingSource(Container);
			Container.Add(control);

			PropertyDescriptor property = TypeDescriptor.GetProperties(control)["BindingMember"];
			AssertNotNull("Extender provider property not found", property);
		}

		#endregion

		#region Test Classes

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

		class MockMetaDataType
		{
			public const string TestType = "KBindingSource.Test";

			public static void RegisterTypes()
			{
				MetaDataType.RegisterMetaDataType(
						new MetaDataType(TestType, typeof(int), 0));
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public class TestObj : KComponent
		{
			[MetaDataMember(MockMetaDataType.TestType, "SomePropertyMetaData")]
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
			{ get { return new TestNestedObject(); } }

			public KBindingList<TestNestedObject> ChildCollection
			{
				get { return childCollection ?? (childCollection = new KBindingList<TestNestedObject>()); }
			}
			KBindingList<TestNestedObject> childCollection;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public class TestNestedObject : KComponent
		{
			public TestNestedObject()
			{
				NestedProperty = "";
			}

			[MetaDataMember(MockMetaDataType.TestType, "NestedPropertyMetaData")]
			public string NestedProperty { get; set; }
			public int NestedPropertyMetaData { get; set; }
		}

		[DefaultBindingProperty("Text")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestControlNoMetaData : TextBox
		{
		}

		[DefaultBindingProperty("Text")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestControl : TextBox
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
				get { return ""; }
				set { }
			}

			Type IDataBoundControl.DataSourceType
			{ get { return typeof(object); } }

			object IDataBoundControl.DataSource
			{ get { return null; } }

			string IDataBoundControl.DataMember
			{ get { return null; } }
		}

		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		class TestIDataBoundControl : DataGrid, IDataBoundControl, IEndCurrentEdit
		{
			Type IDataBoundControl.DataSourceType
			{ get { return typeof(object); } }

			void IEndCurrentEdit.EndCurrentEdit()
			{
				if (ListManager != null)
				{
					ListManager.EndCurrentEdit();
				}
			}
		}

		[DefaultBindingProperty("SomeProperty")]
		class TestPropertyBoundControl : Control
		{
			public string SomeProperty
			{ get { return ""; } }
		}

		class TestNoValuePropertyBoundControl : Control
		{
			[BindingMetaDataProperty(MetaDataTypes.Null, "SomeProperty")]
			public string SomeProperty
			{ get { return ""; } }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		class TestForm : KForm
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

		#region Implementation

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

		protected override void TearDown()
		{
			base.TearDown();
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
	}
}
