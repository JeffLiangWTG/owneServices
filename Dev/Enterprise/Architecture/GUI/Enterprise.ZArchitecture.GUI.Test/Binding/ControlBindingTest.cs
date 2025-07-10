using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Design;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ControlBindingTest : TestCaseWithFactory
	{
		public void TestRelativeBindingMember()
		{
			Form.Controls.Add(BoundControl);
			Form.Controls.Add(BoundControlNoMetaData);
			BindingSource.DataSource = null;

			BindingSource.SetBindingMember(BoundControl, "Property");
			BindingSource.SetBindingMember(BoundControlNoMetaData, "Property");

			Form.Show();
			Application.DoEvents();

			AssertEquals("Property", BindingSource.GetBindingMember(BoundControl));
			AssertEquals("Property", BindingSource.GetBindingMember(BoundControlNoMetaData));
			AssertEquals("No bindings initially", 0, BoundControl.DataBindings.Count);
			AssertEquals("No bindings initially", 0, BoundControlNoMetaData.DataBindings.Count);

			BindingSource.SetBindingMember(BoundControl, "");
			BindingSource.SetBindingMember(BoundControlNoMetaData, "");
			AssertEquals("", BindingSource.GetBindingMember(BoundControl));
			AssertEquals("", BindingSource.GetBindingMember(BoundControlNoMetaData));
			AssertEquals("No bindings initially", 0, BoundControl.DataBindings.Count);
			AssertEquals("No bindings initially", 0, BoundControlNoMetaData.DataBindings.Count);

			BindingSource.SetBindingMember(BoundControl, "Property");
			AssertEquals("No bindings initially", 0, BoundControl.DataBindings.Count);
			AssertEquals("No bindings initially", 0, BoundControlNoMetaData.DataBindings.Count);
			BindingSource.DataSource = Factory.New<TestComponent>();
			AssertEquals("Should have 2 bindings", 2, BoundControl.DataBindings.Count);
			AssertEquals("No bindings now", 0, BoundControlNoMetaData.DataBindings.Count);

			BindingSource.SetBindingMember(BoundControlNoMetaData, "Property");
			AssertEquals("Should have 1 binding", 1, BoundControlNoMetaData.DataBindings.Count);

			BindingSource.SetBindingMember(BoundControl, "");
			BindingSource.SetBindingMember(BoundControlNoMetaData, "");
			AssertEquals("No binding should be set", 0, BoundControl.DataBindings.Count);
			AssertEquals("No binding should be set", 0, BoundControlNoMetaData.DataBindings.Count);

			BindingSource.SetBindingMember(BoundControl, "Property");
			BindingSource.SetBindingMember(BoundControlNoMetaData, "Property");
			AssertEquals("Property", BindingSource.GetBindingMember(BoundControl));
			AssertEquals("Property", BindingSource.GetBindingMember(BoundControlNoMetaData));
			AssertEquals("Should have 2 bindings", 2, BoundControl.DataBindings.Count);
			AssertEquals("Should have 1 bindings", 1, BoundControlNoMetaData.DataBindings.Count);

			BindingSource.SetBindingMember(BoundControl, "NestedObject.NestedProperty");
			BindingSource.SetBindingMember(BoundControlNoMetaData, "NestedObject.NestedProperty");
			AssertEquals("NestedObject.NestedProperty", BindingSource.GetBindingMember(BoundControl));
			AssertEquals("NestedObject.NestedProperty", BindingSource.GetBindingMember(BoundControlNoMetaData));
			AssertEquals("Should have 2 bindings", 2, BoundControl.DataBindings.Count);
			AssertEquals("Should have 1 bindings", 1, BoundControlNoMetaData.DataBindings.Count);

			BindingSource.DataSource = null;
			AssertEquals("NestedObject.NestedProperty", BindingSource.GetBindingMember(BoundControl));
			AssertEquals("NestedObject.NestedProperty", BindingSource.GetBindingMember(BoundControlNoMetaData));
			AssertEquals("No binding should be set", 0, BoundControl.DataBindings.Count);
			AssertEquals("No binding should be set", 0, BoundControlNoMetaData.DataBindings.Count);
		}

		public void TestBindingIncludingMetaData()
		{
			Form.Controls.Add(BoundControl);
			Form.Show();
			Application.DoEvents();

			BindingSource.SetBindingMember(BoundControl, "NestedObject.NestedProperty");
			var o = Factory.New<TestComponent>();
			o.NestedObject.NestedProperty = "";
			BindingSource.DataSource = o;

			AssertEquals("Value and meta data should be bound", 2, BoundControl.DataBindings.Count);
			AssertEquals(BoundControl.Text, o.NestedObject.NestedProperty);
			AssertEquals(BoundControl.ControlMetaDataProperty, o.NestedObject.NestedPropertyMetaData);

			o.NestedObject.NestedProperty = "newtext";
			AssertEquals(BoundControl.Text, o.NestedObject.NestedProperty);

			o.NestedObject.NestedPropertyMetaData = 5;
			AssertEquals(BoundControl.ControlMetaDataProperty, o.NestedObject.NestedPropertyMetaData);
		}

		#region Binding Metadata on wrapped properties

		public void TestBindingMetadataOnWrappedProperties()
		{
			var defaultDecimalPlaces = (int)MetaDataType.GetMetaDataType(MetaDataTypes.DecimalPlaces).DefaultValue;

			var dummy = new ParentObject();

			var propertyPathesWithExpectedDecimalPlaces = new Dictionary<string, int>();
			propertyPathesWithExpectedDecimalPlaces.Add("Child1+Number", defaultDecimalPlaces);
			propertyPathesWithExpectedDecimalPlaces.Add("Child2+Number", defaultDecimalPlaces);
			propertyPathesWithExpectedDecimalPlaces.Add("Child2+OtherNumber", defaultDecimalPlaces);
			AssertBindingMetadataOnWrappedProperties(dummy, propertyPathesWithExpectedDecimalPlaces);

			dummy.Child1 = new ChildObject1();
			dummy.Child2 = new ChildObject2();

			propertyPathesWithExpectedDecimalPlaces.Clear();
			propertyPathesWithExpectedDecimalPlaces.Add("Child1+Number", 4);
			propertyPathesWithExpectedDecimalPlaces.Add("Child2+Number", 5);
			propertyPathesWithExpectedDecimalPlaces.Add("Child2+OtherNumber", 6);
			AssertBindingMetadataOnWrappedProperties(dummy, propertyPathesWithExpectedDecimalPlaces);
		}

		void AssertBindingMetadataOnWrappedProperties(ParentObject parent, Dictionary<string, int> propertyPathesWithExpectedDecimalPlaces)
		{
			using (var testForm = new ZForm(parent) { Size = new Size(300, 300) })
			{
				var position = 10;
				foreach (var pair in propertyPathesWithExpectedDecimalPlaces)
				{
					AddNewControl<ZCalcEdit>(testForm, pair.Key, new Point(10, position));
					position += 24;
				}

				testForm.Show();
				Application.DoEvents();

				foreach (var calcEdit in testForm.Controls.OfType<ZCalcEdit>())
				{
					AssertEquals(calcEdit.BindTo, propertyPathesWithExpectedDecimalPlaces[calcEdit.BindTo], calcEdit.DecimalPlaces);
				}
			}
		}

		#region Test classes

		[ProvideMetaDataProperty("DecimalPlaces", MetaDataTypes.DecimalPlaces)]
		class ParentObject : NonPersistentBusinessObject
		{
			public ChildObject1 Child1 { get; set; }
			public ChildObject2 Child2 { get; set; }

			// Following metadata should be ignored by wrapped properties on child elements

			[DecimalPlaces("NumberDecimalPlaces")]
			public ZDecimal Number { get; set; }

			public ZPropertyInfo NumberInfo
			{
				get { return GetZPropertyInfo(nameof(Number)); }
			}

			public int NumberDecimalPlaces
			{
				get { return 1; }
			}

			public int GetDecimalPlaces(PropertyDescriptor property)
			{
				return 2;
			}
		}

		[ProvideMetaDataProperty("DecimalPlaces", MetaDataTypes.DecimalPlaces)]
		class ChildObject1 : NonPersistentBusinessObject
		{
			public ZDecimal Number { get; set; }

			public ZPropertyInfo NumberInfo
			{
				get { return GetZPropertyInfo(nameof(Number)); }
			}

			public int GetDecimalPlaces(PropertyDescriptor property)
			{
				return 4;
			}
		}

		class ChildObject2 : NonPersistentBusinessObject
		{
			[DecimalPlaces("NumberDecimalPlaces")]
			public ZDecimal Number { get; set; }

			public ZPropertyInfo NumberInfo
			{
				get { return GetZPropertyInfo(nameof(Number)); }
			}

			public int NumberDecimalPlaces
			{
				get { return 5; }
			}

			[DecimalPlaces(6)]
			public ZDecimal OtherNumber { get; set; }

			public ZPropertyInfo OtherNumberInfo
			{
				get { return GetZPropertyInfo(nameof(OtherNumber)); }
			}
		}

		#endregion

		#region Test controls

		T AddNewControl<T>(Control parentControl, string bindTo, Point location)
			where T : Control, IBindTo, new()
		{
			var control = new T
			{
				BindTo = bindTo,
				Location = location
			};
			parentControl.Controls.Add(control);
			return control;
		}

		#endregion

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1104:DoNotUseSystemWindowsTabControl", Justification = "Testing")]
		public void TestForceBinding_NotContainerControlType()
		{
			var notContainerControl = new TestNotContainerBoundControl();
			var ensureListManagerControl = new TestEnsureListManagerControl();

			var tabControl = new TabControl();
			var tabPage1 = new TabPage();
			tabPage1.Controls.Add(new TextBox());
			tabControl.TabPages.Add(tabPage1);

			var tabPage2 = new TabPage();
			tabPage2.Controls.Add(notContainerControl);
			tabPage2.Controls.Add(ensureListManagerControl);
			tabControl.TabPages.Add(tabPage2);

			Form.Controls.Add(tabControl);

			BindingSource.SetBindingMember(notContainerControl, $"{nameof(TestContainerComponent.Children)}.{nameof(TestChildComponent.Property)}");
			BindingSource.SetBindingMember(ensureListManagerControl, $"{nameof(TestContainerComponent.Children)}.{nameof(TestChildComponent.AnotherChildren)}");
			BindingSource.DataSource = Factory.New<TestContainerComponent>();

			notContainerControl.ForceBindingIncludingParents();
			AssertEquals("Ensure control.Created = false", false, notContainerControl.Created);
			AssertEquals("Bindings of default property and metadata should be bound after ControlBinding.ForceBinding()", 2, notContainerControl.DataBindings.Count);

			var bindingForMetaData = notContainerControl.DataBindings[0];
			AssertEquals("Ensure DataBindings[0] is a metadata binding instance", nameof(TestChildComponent.PropertyMetaData), bindingForMetaData.BindingMemberInfo.BindingField);
			AssertNull("binding's CurrencyManager is null yet", bindingForMetaData.BindingManagerBase);

			Form.Show();
			Application.DoEvents();

			AssertEquals("tabePage1 is selected", 0, tabControl.SelectedIndex);
			AssertEquals("notContainerControl.Created = false at this point", false, notContainerControl.Created);
			AssertNotNull("binding's CurrencyManager is not null due to parent control's OnBindingContextChanged() invocation", bindingForMetaData.BindingManagerBase);

			tabControl.SelectedIndex = 1;
			Application.DoEvents();

			AssertEquals("notContainerControl.Created = true now", true, notContainerControl.Created);
			AssertEquals("No reported exception during Format", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

			ErrorReporter.Clear();
		}

		#region Implementation

		#region Properties

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

		#endregion

		#region Test classes

		[EditorBrowsable(EditorBrowsableState.Never)]
		public class TestForm : KForm
		{
			public TestForm()
			{ BindingSource.DataSourceType = typeof(object); }
		}

		public static class MockMetaDataType
		{
			public const string TestType = "KBindingSource.Test";

			public static void RegisterTypes()
			{
				MetaDataType.RegisterMetaDataType(
					new MetaDataType(TestType, typeof(int), 0));
			}
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

		[DefaultBindingProperty("Text")]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestControlNoMetaData : TextBox
		{
		}

		public class TestComponent : DummyBusinessObject
		{
			public TestComponent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

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
			{ get { return Factory.New<TestNestedComponent>(); } }

			new public List<TestComponent> Collection
			{
				get
				{
					var result = new List<TestComponent>();
					result.Add(Factory.New<TestComponent>());
					return result;
				}
			}
		}

		public class TestNestedComponent : DummyBusinessObject
		{
			public TestNestedComponent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				NestedProperty = "";
			}

			[MetaDataMember(MockMetaDataType.TestType, "NestedPropertyMetaData")]
			public string NestedProperty { get; set; }

			public int NestedPropertyMetaData { get; set; }
		}

		#region For ForceBinding

		[DesignTimeControlNameGenerator("txt")]
		[DefaultBindingProperty("Text")]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestNotContainerBoundControl : TextBox
		{
			[BindingMetaDataProperty(MockMetaDataType.TestType, nameof(ControlMetaDataProperty))]
			public override string Text
			{
				get => base.Text;
				set => base.Text = value;
			}

			public int ControlMetaDataProperty { get; set; }
		}

		[DesignTimeControlNameGenerator("txt")]
		[DefaultBindingProperty("Text")]
		[DesignTimeVisible(false)]
		[ToolboxItem(false)]
		public class TestEnsureListManagerControl : TextBox, IDataBoundControl
		{
			[BindingMetaDataProperty(MockMetaDataType.TestType, nameof(ControlMetaDataProperty))]
			public override string Text
			{
				get => base.Text;
				set => base.Text = value;
			}

			public int ControlMetaDataProperty { get; set; }

			IDataBoundControl DataBoundControlImpl => DataBoundControl.GetDefaultImplementation(this);

			Type IDataBoundControl.DataSourceType => DataBoundControlImpl.DataSourceType;

			object IDataBoundControl.DataSource => DataBoundControlImpl.DataSource;

			string IDataBoundControl.DataMember => DataBoundControlImpl.DataMember;

			void IDataBoundControl.SetDataBinding(object dataSource, string dataMember)
			{
				DataBoundControlImpl.SetDataBinding(dataSource, dataMember);

				if (dataSource != null && BindingContext != null)
				{
					CombineAssertions("Bindings are created correctly", () =>
					{
						AssertEquals("One is for metadata, the other is for Text", 2, DataBindings.Count);
						var bindingForText = DataBindings[1];
						AssertEquals("Second binding is bound to Text property", nameof(Text), bindingForText.PropertyName);
						bindingForText.Format += BindingForTextCollectionToString_Format;
					});

					BindingContext.EnsureListManager(dataSource, dataMember); // mimic `context[dataSource, dataMember]` in `ZGrid.SetDataBinding(object dataSource, string dataMember)`
				}

				void BindingForTextCollectionToString_Format(object sender, ConvertEventArgs e) => e.Value = "Fixed String";
			}
		}

		public class TestContainerComponent : DummyBusinessObject
		{
			public TestContainerComponent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			ChildBusinessObjectCollection children;
			public ChildBusinessObjectCollection Children => children ?? (children = new ChildBusinessObjectCollection(new BusinessObjectFactory()));
		}

		public class TestChildComponent : DummyBusinessObject
		{
			public TestChildComponent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			[MetaDataMember(MockMetaDataType.TestType, nameof(PropertyMetaData))]
			public string Property { get; set; }

			public int PropertyMetaData { get; set; }

			[MetaDataMember(MockMetaDataType.TestType, nameof(AnotherPropertyMetaData))]
			public ChildBusinessObjectCollection AnotherChildren => anotherChildren ?? (anotherChildren = new ChildBusinessObjectCollection(new BusinessObjectFactory()));
			ChildBusinessObjectCollection anotherChildren;

			public int AnotherPropertyMetaData { get; set; }
		}

		public class ChildBusinessObjectCollection : BusinessObjectCollection<TestChildComponent>
		{
			public ChildBusinessObjectCollection(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		#endregion

		#endregion

		#region Overrides

		protected override void SetUp()
		{
			base.SetUp();
			MockMetaDataType.RegisterTypes();
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
			if (boundControl != null)
			{
				boundControl.Dispose();
			}

			if (boundControlNoMetaData != null)
			{
				boundControlNoMetaData.Dispose();
			}
		}

		#endregion

		#endregion
	}
}
