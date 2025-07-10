using System;
using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ResourceStringKeyCalculatorTest : TestCaseWithFactory
	{
		public void TestKeyGroup()
		{
			Form.Show();
			UserIdleWorker.Flush();
			AssertEquals("DummyBizo|Z0_Description", new ResourceStringKeyCalculator(Form.TextBox).DataString.Key);
		}

		public void TestDesignAndRunTimeStrings()
		{
			using (var decForm = new TestFormDeclaration(Factory.New<Declaration>()))
			{
				decForm.Show();
				AssertEquals("Code", new ResourceStringKeyCalculator(decForm.TestControl.TextBox).DataString.Caption);
			}
			using (var productForm = new TestFormProduct(Factory.New<Product>()))
			{
				productForm.Show();
				AssertEquals("Code", new ResourceStringKeyCalculator(productForm.TestControl.TextBox).DataString.Caption);
			}
		}

		public void TestMultipleResourceStringData()
		{
			using (var form = new ZDummyForm(Factory.New<DummyBusinessObjectSupportMultipleResourceStringData1>()))
			{
				form.Show();
				AssertEquals("[21] Description", new ResourceStringKeyCalculator(form.TextBox).DataString.Caption);
				AssertEquals("Number", new ResourceStringKeyCalculator(form.CalcEdit).DataString.Caption);
			}
		}

		class TestPrevDocsControl : ZUserControl
		{
			public TestPrevDocsControl()
			{
				InitializeComponent();
			}

			void InitializeComponent()
			{
				BindingSource.DataSourceType = typeof(Business.StmNote);
				TextBox = new ZTextBox();
				BindingSource.SetBindingMember(this.TextBox, "DeclarationEventsForTest.Z0_Code");
				Controls.Add(TextBox);
			}

			public ZTextBox TextBox;
		}

		class Product : Business.StmEvent
		{
			public Product(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public DummyBusinessObjectCollection ProductEventsForTest
			{
				get { return new DummyBusinessObjectCollection(this.Factory); }
			}
		}

		class Declaration : Business.StmData
		{
			public Declaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public DummyBusinessObjectCollection DeclarationEventsForTest
			{
				get { return new DummyBusinessObjectCollection(this.Factory); }
			}
		}

		class TestFormProduct : ZForm
		{
			public TestFormProduct()
			{
				InitializeComponent();
			}

			public TestFormProduct(Product bizo)
				: base(bizo)
			{
				InitializeComponent();
			}

			new void InitializeComponent()
			{
				BindingSource.DataSourceType = typeof(Product);
				TestControl = new TestPrevDocsControl();
				this.Controls.Add(TestControl);
				new ControlRebinder().Rebind(TestControl, "DeclarationEventsForTest.", "ProductEventsForTest.");
			}

			public TestPrevDocsControl TestControl;
		}

		class TestFormDeclaration : ZForm
		{
			public TestFormDeclaration()
			{
				InitializeComponent();
			}

			public TestFormDeclaration(Declaration bizo)
				: base(bizo)
			{
				InitializeComponent();
			}

			new void InitializeComponent()
			{
				BindingSource.DataSourceType = typeof(Declaration);
				TestControl = new TestPrevDocsControl();
				this.Controls.Add(TestControl);
			}

			public TestPrevDocsControl TestControl;
		}

		class ControlRebinder
		{
			public void Rebind(Control control, string searchString, string replaceString)
			{
				if (string.IsNullOrEmpty(searchString))
				{
					throw new ArgumentException("SearchString cannot be null or empty.", nameof(searchString));
				}
				this.searchString = searchString;
				this.replaceString = replaceString;
				RebindInternal(control);
			}
			string searchString;
			string replaceString;

			void RebindInternal(Control control)
			{
				var currentBindTo = control.GetBindingMember();
				if (currentBindTo.StartsWith(searchString))
				{
					control.SetBindingMember(replaceString + currentBindTo.Remove(0, searchString.Length));
				}
				foreach (Control innerControl in control.Controls)
				{
					RebindInternal(innerControl);
				}
			}
		}

		public void TestKeyGroup_ForNonGridControls()
		{
			Form.Show();
			UserIdleWorker.Flush();

			CheckControlPathKey(Form.TextBox, "TextBox|ResourceStringTestForm");
			CheckControlDataKey(Form.TextBox, "DummyBizo|Z0_Description");

			CheckControlPathKey(Form.FindBox, "TrickyFindBox|GroupBox|MockCountrySpecificUserControl");
			CheckControlDataKey(Form.FindBox, "DummyBizo|Z0_Guid");

			CheckControlPathKey(Form.ThroughCollectionTextBox, "ThroughCollectionTextBox|GroupBox|MockCountrySpecificUserControl");
			CheckControlDataKey(Form.ThroughCollectionTextBox, "DummyBizo|Z0_Code");
		}

		public void TestKeyGroup_ForGrid()
		{
			Form.Show();

			// CheckGridGUIKey(Form.Grid, "Z0_Number", ""); will contain guid
			CheckGridControlPathKey(Form.Grid, "Z0_Number", "Z0_Number|TestGrid|ResourceStringTestForm");
			CheckGridDataKey(Form.Grid, "Z0_Number", "DummyBizo|Z0_Number");

			CheckGridControlPathKey(Form.Grid, "Self+Z0_Code", "Self+Z0_Code|TestGrid|ResourceStringTestForm");
			CheckGridDataKey(Form.Grid, "Self+Z0_Code", "DummyBizo|Z0_Code");
		}

		public void TestIsControlPathKeyValid_ForBadlyNamedControlsInPath()
		{
			using (var groupBox = new ZGroupBox())
			using (var textBox = new ZTextBox())
			{
				groupBox.Controls.Add(textBox);
				textBox.Name = "ATextBox";
				groupBox.Name = char.ToLower(groupBox.GetType().Name[0]) + groupBox.GetType().Name.Substring(1) + "1";

				AssertEquals("zGroupBox1", groupBox.Name);
				AssertEquals("Using default control names", false, new ResourceStringKeyCalculator(textBox).IsControlPathKeyValid);

				groupBox.Name = "AGroupBox";
				AssertEquals("Using a valid control name", true, new ResourceStringKeyCalculator(textBox).IsControlPathKeyValid);
			}
		}

		public void TestIsControlPathKeyValid_ForTooManyControlsInPath()
		{
			using (var groupBox1 = new ZGroupBox())
			using (var groupBox2 = new ZGroupBox())
			using (var groupBox3 = new ZGroupBox())
			using (var groupBox4 = new ZGroupBox())
			using (var groupBox5 = new ZGroupBox())
			using (var groupBox6 = new ZGroupBox())
			using (var groupBox7 = new ZGroupBox())
			using (var groupBox8 = new ZGroupBox())
			using (var textBox = new ZTextBox())
			{
				groupBox1.Name = "GroupBox1";
				groupBox2.Name = "GroupBox2";
				groupBox3.Name = "GroupBox3";
				groupBox4.Name = "GroupBox4";
				groupBox5.Name = "GroupBox5";
				groupBox6.Name = "GroupBox6";
				groupBox7.Name = "GroupBox7";
				groupBox8.Name = "GroupBox8";
				groupBox1.Controls.Add(groupBox2);
				groupBox2.Controls.Add(groupBox3);
				groupBox3.Controls.Add(groupBox4);
				groupBox4.Controls.Add(groupBox5);
				groupBox5.Controls.Add(groupBox6);
				groupBox6.Controls.Add(groupBox7);
				groupBox7.Controls.Add(groupBox8);
				groupBox8.Controls.Add(textBox);
				textBox.Name = "ATextBox";

				AssertEquals("Too many controls in the path", false, new ResourceStringKeyCalculator(textBox).IsControlPathKeyValid);
				groupBox1.Dispose();
				AssertEquals("<= 8 controls in the path", true, new ResourceStringKeyCalculator(textBox).IsControlPathKeyValid);
			}
		}

		public void TestWithSetResourceStringIdentifyingControl()
		{
			using (var containerControl = new MockBaseUserControl())
			using (var control = new ZTextBox())
			using (var identifyingControl = new ZTextBox())
			{
				containerControl.DataSourceType = typeof(DummyBusinessObject);

				identifyingControl.BindTo = DummyBizoSchema.Z0_Code.Name;
				containerControl.Controls.Add(identifyingControl);

				control.SetResourceStringIdentifyingControl(identifyingControl);
				CheckControlDataKey(control, "DummyBizo|Z0_Code");
			}
		}

		public void TestWithResourceDataAttributeHavingIsApplicable()
		{
			var dummyBO = Factory.New<DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable>();
			dummyBO.SetDummyValue(1);

			using (var containerControl = new ZForm(dummyBO))
			using (var control = new ZTextBox())
			using (var identifyingControl = new ZTextBox())
			{
				containerControl.DataSourceType = typeof(DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable);
				identifyingControl.BindTo = "Z0_Description";
				containerControl.Controls.Add(identifyingControl);

				control.SetResourceStringIdentifyingControl(identifyingControl);
				CheckControlDataKey(control, "DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable|No_Multiple_Key|IsDummyValueEqualToOne|Z0_Description");
			}
		}

		public void TestZGridWithResourceDataAttributeHavingIsApplicable()
		{
			var dummyChild1 = Factory.New<DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable>();
			dummyChild1.SetDummyValue(1);
			var dummyCollection = new DummyCollectionForTest(Factory) { dummyChild1 };

			using (var zForm = new ZForm())
			using (var grid = new ZGrid())
			{
				zForm.Controls.Add(grid);
				var info = new ZTextBoxColumnStyleInfo();
				info.ColumnName = "Z0_Description";
				grid.ColumnStyles.Add(info);
				grid.SetDataBinding(dummyCollection, "");

				zForm.Show();
				grid.Focus();
				grid.CurrentCell = new DataGridCell(0, 0);

				using (var style = new ZTextBoxColumnStyle(info))
				{
					var control = style.EditControl;
					control.SetResourceStringIdentifyingControl(grid);
					CheckGridDataKey(grid, "Z0_Description", "DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable|No_Multiple_Key|IsDummyValueEqualToOne|Z0_Description");
				}
			}
		}

		#region TestAttributeSpecifiedDataKey

		public void TestAttributeSpecifiedDataKey()
		{
			AssertAttributeSpecifiedDataKey<DummyBusinessObjectWithAttributes>("Abra|Cadabra");
			AssertAttributeSpecifiedDataKey<DummyBusinessObjectWithAttributesOverride1>("Abra|Cadabra");
			AssertAttributeSpecifiedDataKey<DummyBusinessObjectWithAttributesOverride2>("Override|Cadabra");
		}

		void AssertAttributeSpecifiedDataKey<T>(string expectedKey)
		{
			using (var testForm = new FormWithAttributes((DummyBusinessObject)Factory.New(typeof(T))))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				AssertEquals(expectedKey, new ResourceStringKeyCalculator(testForm.SomePropertyTextBox).DataString.Key);
			}
		}

		class DummyBusinessObjectWithAttributes : DummyBusinessObject
		{
			public DummyBusinessObjectWithAttributes(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[ResourceStringData("Abra|Cadabra", Caption = "Abra Cadabra")]
			public virtual int SomeProperty { get { return 0; } }

			public ZPropertyInfo SomePropertyInfo { get { return GetZPropertyInfo(nameof(SomeProperty)); } }
		}

		class DummyBusinessObjectWithAttributesOverride1 : DummyBusinessObjectWithAttributes
		{
			public DummyBusinessObjectWithAttributesOverride1(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		class DummyBusinessObjectWithAttributesOverride2 : DummyBusinessObjectWithAttributes
		{
			public DummyBusinessObjectWithAttributesOverride2(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			[ResourceStringData("Override|Cadabra", Caption = "Override Cadabra")]
			public override int SomeProperty { get { return base.SomeProperty; } }
		}

		class FormWithAttributes : ResourceStringTestForm
		{
			public FormWithAttributes(DummyBusinessObject dummy) : base(dummy) { }

			public readonly ZTextBox SomePropertyTextBox = new ZTextBox();

			protected override void InitialiseForm()
			{
				base.InitialiseForm();

				SomePropertyTextBox.Name = "SomePropertyTextBox";
				SomePropertyTextBox.BindTo = "SomeProperty";
				UserControl.Controls.Add(SomePropertyTextBox);
			}
		}

		#endregion

		#region Test Classes

		class MockBaseUserControl : ZUserControl { }

		class MockCountrySpecificUserControl : MockBaseUserControl { }

		class ResourceStringTestForm : ZDummyForm
		{
			public ResourceStringTestForm(DummyBusinessObject dummy)
				: base(dummy)
			{
			}

			public readonly ZCodeFindBox FindBox = new ZCodeFindBox();
			public readonly ZTextBox ThroughCollectionTextBox = new ZTextBox();
			public readonly MockCountrySpecificUserControl UserControl = new MockCountrySpecificUserControl();
			public readonly ZGroupBox GroupBox = new ZGroupBox();
			public readonly ZCodeFindBoxColumnStyleInfo FindBoxColumnStyle = new ZCodeFindBoxColumnStyleInfo();

			protected override void InitialiseForm()
			{
				base.InitialiseForm();

				FindBox.Name = "TrickyFindBox";
				FindBox.BindTo = "RelatedDummy+" + DummyBusinessObject.Schema.Z0_Guid;
				FindBox.BindToList = "Collection";
				FindBox.ModuleID = ZArchitecture.Modules.Testing.DummyModuleIDs.Dummy;

				UserControl.Name = "NamedUserControl";
				GroupBox.Name = "GroupBox";
				Grid.Name = "TestGrid";
				ThroughCollectionTextBox.Name = "ThroughCollectionTextBox";
				ThroughCollectionTextBox.BindTo = "Collection." + DummyBusinessObject.Schema.Z0_Code;

				TabPage1.Controls.Add(UserControl);

				UserControl.Controls.Add(GroupBox);
				GroupBox.Controls.Add(FindBox);
				GroupBox.Controls.Add(ThroughCollectionTextBox);

				FindBoxColumnStyle.BindToList = "Lookups+DummyList";
				FindBoxColumnStyle.ColumnName = "Self+" + DummyBusinessObject.Schema.Z0_Code;
				FindBoxColumnStyle.ModuleID = ZArchitecture.Modules.Testing.DummyModuleIDs.Dummy;
				Grid.ColumnStyles.Add(FindBoxColumnStyle);
			}
		}

		sealed class DummyCollectionForTest : BusinessObjectCollection<DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable>
		{
			public DummyCollectionForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public DummyCollectionForTest(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
			{
			}
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (form != null)
			{
				form.Dispose();
			}
		}

		ResourceStringTestForm Form
		{
			get { return form ?? (form = new ResourceStringTestForm(Dummy)); }
		}
		ResourceStringTestForm form;

		DummyBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyBusinessObject>()); }
		}
		DummyBusinessObject dummy;

		void CheckControlPathKey(Control control, string expectedKey)
		{
			AssertEquals("Expected ControlPath Key", expectedKey, new ResourceStringKeyCalculator(control).ControlPathKey);
		}

		void CheckControlDataKey(Control control, string expectedKey)
		{
			AssertEquals("Expected Data Key", expectedKey, new ResourceStringKeyCalculator(control).DataString.Key);
		}

		void CheckGridControlPathKey(ZGrid grid, string mappingName, string expectedKey)
		{
			AssertEquals("Expected ControlPath Key", expectedKey, new ResourceStringKeyCalculator(grid, mappingName).ControlPathKey);
		}

		void CheckGridDataKey(ZGrid grid, string mappingName, string expectedKey)
		{
			AssertEquals("Expected Data Key", expectedKey, new ResourceStringKeyCalculator(grid, mappingName).DataString.Key);
		}

		#endregion
	}
}
