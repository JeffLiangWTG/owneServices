using System;
using System.ComponentModel;
using System.Data;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Forms.FormDebugInfo;

namespace Enterprise.Core.Forms.Test
{
	class DebugInfoFormTest : TestCaseWithFactory
	{
		public void TestBindingInfo()
		{
			GetActiveControlInfoFromForm(null);
			var bindingInfo = GetBindingInfo(Form.OtherTextBox);
			Assert(bindingInfo, Regex.IsMatch(bindingInfo, @"DataSource type: Enterprise\.Core\.Forms\.Test\.ExampleDataSet"));
			Assert(bindingInfo, Regex.IsMatch(bindingInfo, @"BindingMember: AccTransactionHeader\.AH_Desc"));
		}

		[ExpectNoExceptions]
		public void TestGetDataSourceInfoWithNullDataSourceType()
		{
			GetDataSourceInfo(null, null);
		}

		public void TestControlInfo()
		{
			GetActiveControlInfoFromForm(null);
			var controlInfo = GetControlInfo(Form.OtherTextBox);
			Assert(controlInfo, Regex.IsMatch(controlInfo, @"Name: OtherTextBox"));
			Assert(controlInfo, Regex.IsMatch(controlInfo, @"Type: Enterprise\.ZArchitecture\.ZTextBox"));
		}

		public void TestFormInfo()
		{
			GetFormInfo(null);
			var formInfo = GetFormInfo(Form);
			Assert(formInfo, Regex.IsMatch(formInfo, @"Name: DebugInfoForm"));
			Assert(formInfo, Regex.IsMatch(formInfo, @"Type: Enterprise\.Core\.Forms\.Test\.DebugInfoForm"));
		}

		public void TestDeveloperInfo()
		{
			Form.ActiveControl = Form.OtherTextBox;
			Form.Show();
			Assert("Has an ActiveControl", Form.ActiveControl != null);
			GetActiveControlInfoFromForm(null);
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert(developerInfo, Regex.IsMatch(developerInfo, @"Form.*Control", RegexOptions.Singleline));
			Assert("Should contain table info", developerInfo.Contains("Table: AccTransactionHeader"));
		}

		public void TestDeveloperInfoWithStringProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Code = "Test";
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Code));
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(ZString).FullName}"));
			Assert("Should contain macro example", developerInfo.Contains($"Macro example: {nameof(bizO.Z0_Code)}==\"{bizO.Z0_Code}\""));
			Assert("Should contain table name", developerInfo.Contains($"Table: {bizO.TableName}"));
		}

		public void TestDeveloperInfoWithCalculatedProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.DoNotBindToMe));
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(string).FullName}"));
			Assert("Should contain *not* contain table name", developerInfo.Contains($"Table: [calculated property]"));
		}

		public void TestDeveloperInfoWithCalculatedProperty_BadPrefix()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject_WithBadPrefixProperty>();
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.L0L_WeirdProperty));
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(ZString).FullName}"));
			Assert("Should contain *not* contain table name", developerInfo.Contains($"Table: [calculated property]"));
		}

		public void TestDeveloperInfoWithCalculatedProperty_CalculatedPropertyWithTablePrefix()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_GuidWithListAttribute));
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(ZGuid).FullName}"));
			Assert("Should contain *not* contain table name", developerInfo.Contains($"Table: [calculated property]"));
		}

		public void TestDeveloperInfoWithBoolProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Bool = false;
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Bool));
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(ZBool).FullName}"));
			Assert("Should contain macro example", developerInfo.Contains($"Macro example: !{nameof(bizO.Z0_Bool)}"));
		}

		public void TestDeveloperInfoWithDecimalProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Decimal = 90.34;
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Decimal));
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(ZDecimal).FullName}"));
			Assert("Should contain macro example", developerInfo.Contains($"Macro example: {nameof(bizO.Z0_Decimal)}>={bizO.Z0_Decimal}"));
		}

		public void TestDeveloperInfoWithByteProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Byte = 11;
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Byte));
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(ZByte).FullName}"));
			Assert("Should contain macro example", developerInfo.Contains($"Macro example: {nameof(bizO.Z0_Byte)}>={bizO.Z0_Byte}"));
		}

		public void TestDeveloperInfoWithIntProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Number = 11;
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Number));
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(ZInt).FullName}"));
			Assert("Should contain macro example", developerInfo.Contains($"Macro example: {nameof(bizO.Z0_Number)}>={bizO.Z0_Number}"));
		}

		public void TestDeveloperInfoDateTimeProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Date = DateTime.Now;
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Date));
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(ZDateTime).FullName}"));
			Assert("Should contain macro example", developerInfo.Contains(string.Format("Macro example: {0}.Year>={1:yyyy} && {0}.Month>={1:MM} && {0}.Day>={1:dd}", nameof(bizO.Z0_Date), bizO.Z0_Date)));
		}

		public void TestDeveloperInfoWithGuidProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Guid = Guid.NewGuid();
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Guid));
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(ZGuid).FullName}"));
			Assert("Should contain macro example", developerInfo.Contains($"Macro example: \"<{nameof(bizO.Z0_Guid)}>\"==\"{bizO.Z0_Guid}\""));
		}

		public void TestDeveloperInfoWithCollectionAndNestedProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var nestedBizO = bizO.Collection.AddNew();
			nestedBizO.Z0_Decimal = 90.34;
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, $"{nameof(bizO.Collection)}.{nameof(nestedBizO.Z0_Decimal)}");
			Form.Show();
			var developerInfo = GetActiveControlInfoFromForm(Form);
			Assert("Should contain type", developerInfo.Contains($"Property Type: {typeof(ZDecimal).FullName}"));
			Assert("Should contain macro example", developerInfo.Contains($"Macro example: {nameof(bizO.Collection)}[0].{nameof(nestedBizO.Z0_Decimal)}>={nestedBizO.Z0_Decimal}"));
		}

		public void TestGetFormat_InvalidCastException()
		{
			const string format = "{0}.Year>={1:yyyy} && {0}.Month>={1:MM} && {0}.Day>={1:dd}";
			var nowZDateTimeOffset = (object)ZDateTimeOffset.Now;
			var nowZDateTime = (object)ZDateTime.Now;
			var todayZDate = (object)ZDate.Today;
			AssertEquals(format, GetFormat(ref nowZDateTimeOffset));
			AssertEquals(format, GetFormat(ref nowZDateTime));
			AssertEquals(format, GetFormat(ref todayZDate));
		}

		DebugInfoForm Form => form ?? (form = new DebugInfoForm());
		DebugInfoForm form;
		protected override void TearDown()
		{
			base.TearDown();
			form?.Dispose();
		}
	}

	class TestForDocEngine : TestCaseWithFactory
	{
		public void TestBoundBoCodeWithoutListAttribute()
		{
			var dummyBo = Factory.New<DummyBusinessObject>();
			using (var testForm = new TestDropEditWithoutListAttributeForm(dummyBo))
			{
				testForm.DropEdit.SetDataBinding(dummyBo, AutoDummyBizo.Schema.Z0_FK_Code);
				testForm.Show();
				Application.DoEvents();

				AssertNoExceptionThrown(() => FormDebugInfo.GetActiveControlInfoFromControlForDocEngine(testForm.DropEdit.CodeBox));
			}
		}

		public void TestMacroInfoDoesNotDisplayIfCannotBeAccessed()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			using (var zForm = new ZForm())
			using (var grid = new ZGridTest.ZTestGrid())
			{
				var activeGridCustomColumnsInitializer = new ZActiveGridCustomColumnsInitializer(grid, collection, new ResourceStringData("", "Test Properties"), true);
				activeGridCustomColumnsInitializer.HookCollection();

				zForm.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code });

				var dummyWithPropertyContainer = Factory.New<ZActiveGridCustomColumnsInitializerTest.DummyWithPropertyContainer>();
				dummyWithPropertyContainer.CustomPropertyContainer.AddCustomProperty("Product Code", "Product Code", typeof(ZString), _ => "Test Product");
				collection.Add(dummyWithPropertyContainer);

				grid.SetDataBinding(collection, "");
				zForm.Show();
				Application.DoEvents();

				grid.CurrentCell = new DataGridCell(0, 1);

				var formInfo = GetActiveControlInfoFromFormForDocEngine(zForm);
				AssertNotContains("Macro:", formInfo.ControlInfo);
			}
		}

		public void TestBindingInfoForGridControl()
		{
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();
			using (var zForm = new ZForm())
			using (var grid = new ZGridTest.ZTestGrid())
			{
				zForm.Controls.Add(grid);
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = DummyBizoSchema.Constants.Z0_Code });

				dummy.Collection.AddNew();
				dummy.Collection.AddNew();

				zForm.SetDataBinding(dummy, "");
				grid.SetDataBinding(dummy.Collection, "");

				zForm.Show();
				Application.DoEvents();

				grid.CurrentCell = new DataGridCell(1, 0);

				var formInfo = GetActiveControlInfoFromFormForDocEngine(zForm);

				AssertEquals(@"ZForm > Code 

Control: Z0_Code (System.Windows.Forms.DataGridTextBox)
   in Form: ZForm (Enterprise.ZArchitecture.GUI.ZForm)

DataSource Type: Enterprise.ZArchitecture.Business.Testing.DummyChildEnterpriseBusinessObjectCollection
Binding Member: Z0_Code (ZString)

Table/Field Name: DummyBizo.Z0_Code

Macro: <Z0_Code>
", formInfo.ControlInfo);
			}
		}

		public void TestBindingInfo()
		{
			var (bindingInfo, _) = FormDebugInfo.GetBindingInfoForDocEngine(Form.OtherTextBox);
			Assert(bindingInfo, Regex.IsMatch(bindingInfo, @"DataSource Type: Enterprise\.Core\.Forms\.Test\.ExampleDataSet"));
			Assert(bindingInfo, Regex.IsMatch(bindingInfo, @"Binding Member: AccTransactionHeader\.AH_Desc"));
			Assert(bindingInfo, !Regex.IsMatch(bindingInfo, @"Mapping Path(s):
/AccTransactionHeader/AH_Desc"));
		}

		public void TestControlInfo()
		{
			var controlInfo = FormDebugInfo.GetActiveControlInfoFromFormForDocEngine(null);
			controlInfo.ControlInfo = FormDebugInfo.GetControlInfoForDocEngine(Form.OtherTextBox);
			Assert(controlInfo.ControlInfo, Regex.IsMatch(controlInfo.ControlInfo, @"Control: OtherTextBox .*Enterprise\.ZArchitecture\.ZTextBox"));
		}

		public void TestFormInfo()
		{
			var formInfo = FormDebugInfo.GetFormInfoForDocEngine(null);
			formInfo = FormDebugInfo.GetFormInfoForDocEngine(Form);
			Assert(formInfo, Regex.IsMatch(formInfo, @"Form: DebugInfoForm .*Enterprise\.Core\.Forms\.Test\.DebugInfoForm"));
		}

		public void TestDeveloperInfo()
		{
			Form.Show();
			Assert("Has an ActiveControl", Form.ActiveControl != null);

			var developerInfo = FormDebugInfo.GetActiveControlInfoFromFormForDocEngine(null);
			developerInfo = FormDebugInfo.GetActiveControlInfoFromFormForDocEngine(Form);
			Assert(developerInfo.ControlInfo, Regex.IsMatch(developerInfo.ControlInfo, @"Form.*Control", RegexOptions.Singleline));
		}

		public void TestDeveloperInfoWithStringProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Code = "Test";

			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Code));

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain type", developerInfo.ControlInfo.Contains($"Binding Member: {nameof(bizO.Z0_Code)} ({nameof(ZString)})"));
			Assert("Should contain macro example", developerInfo.ControlInfo.Contains("Macro: <Z0_Code>"));
			Assert("Should contain table name", developerInfo.ControlInfo.Contains("Table/Field Name: DummyBizo.Z0_Code"));
		}

		public void TestDeveloperInfoWithCalculatedProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.DoNotBindToMe));

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain *not* contain table name", developerInfo.ControlInfo.Contains("Table/Field Name: [calculated property]"));
		}

		public void TestDeveloperInfoWithCalculatedProperty_BadPrefix()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject_WithBadPrefixProperty>();
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.L0L_WeirdProperty));

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain *not* contain table name", developerInfo.ControlInfo.Contains("Table/Field Name: [calculated property]"));
		}

		public void TestDeveloperInfoWithCalculatedProperty_CalculatedPropertyWithTablePrefix()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_GuidWithListAttribute));

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain *not* contain table name", developerInfo.ControlInfo.Contains("Table/Field Name: [calculated property]"));
		}

		public void TestDeveloperInfoWithBoolProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Bool = false;

			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Bool));

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain type", developerInfo.ControlInfo.Contains($"Binding Member: {nameof(bizO.Z0_Bool)} ({nameof(ZBool)})"));
			Assert("Should contain macro example", developerInfo.ControlInfo.Contains("Macro: <Z0_Bool>"));
		}

		public void TestDeveloperInfoWithDecimalProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Decimal = 90.34;

			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Decimal));

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain type", developerInfo.ControlInfo.Contains($"Binding Member: {nameof(bizO.Z0_Decimal)} ({nameof(ZDecimal)})"));
			Assert("Should contain macro example", developerInfo.ControlInfo.Contains("Macro: <Z0_Decimal>"));
		}

		public void TestDeveloperInfoWithByteProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Byte = 11;

			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Byte));

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain type", developerInfo.ControlInfo.Contains($"Binding Member: {nameof(bizO.Z0_Byte)} ({nameof(ZByte)})"));
			Assert("Should contain macro example", developerInfo.ControlInfo.Contains("Macro: <Z0_Byte>"));
		}

		public void TestDeveloperInfoWithIntProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Number = 11;

			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Number));

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain type", developerInfo.ControlInfo.Contains($"Binding Member: {nameof(bizO.Z0_Number)} ({nameof(ZInt)})"));
			Assert("Should contain macro example", developerInfo.ControlInfo.Contains("Macro: <Z0_Number>"));
		}

		public void TestDeveloperInfoDateTimeProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Date = DateTime.Now;

			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Date));

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain type", developerInfo.ControlInfo.Contains($"Binding Member: {nameof(bizO.Z0_Date)} ({nameof(ZDateTime)})"));
			Assert("Should contain macro example", developerInfo.ControlInfo.Contains("Macro: <Z0_Date>"));
		}

		public void TestDeveloperInfoWithGuidProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			bizO.Z0_Guid = Guid.NewGuid();

			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, nameof(bizO.Z0_Guid));

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain type", developerInfo.ControlInfo.Contains($"Binding Member: {nameof(bizO.Z0_Guid)} ({nameof(ZGuid)})"));
			Assert("Should contain macro example", developerInfo.ControlInfo.Contains("Macro: <Z0_Guid>"));
		}

		public void TestDeveloperInfoWithCollectionAndNestedProperty()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var nestedBizO = bizO.Collection.AddNew();
			nestedBizO.Z0_Decimal = 90.34;

			Form.BlahBlahTextBox.DataBindings.Add("Text", bizO, $"{nameof(bizO.Collection)}.{nameof(nestedBizO.Z0_Decimal)}");

			Form.Show();

			var developerInfo = GetActiveControlInfoFromFormForDocEngine(Form);
			Assert("Should contain type", developerInfo.ControlInfo.Contains($"Binding Member: {nameof(bizO.Collection)}.{nameof(nestedBizO.Z0_Decimal)} ({nameof(ZDecimal)})"));
			Assert("Should contain macro example", developerInfo.ControlInfo.Contains("Macro: <Collection[1].Z0_Decimal>"));
		}

		public void TestBindingInfo_MappingPath_SimpleControl()
		{
			using var debugForm = new DebugInfoFormWithGrid(Factory.New<DummyBusinessObject>());
			var (bindingInfo, _) = FormDebugInfo.GetBindingInfoForDocEngine(debugForm.OtherTextBox);
			AssertContains(@"Mapping Path(s):
/Z0_Description", bindingInfo);
		}

		public void TestBindingInfo_MappingPath_GridControl()
		{
			var bo = Factory.New<DummyBusinessObject>();
			bo.Collection.AddNew();

			using var debugForm = new DebugInfoFormWithGrid(Factory.New<DummyBusinessObject>());
			var itemsGrid = debugForm.ItemsGrid;
			itemsGrid.SetDataBinding(bo.Collection, "");
			debugForm.Show();
			Application.DoEvents();

			itemsGrid.CurrentCell = new DataGridCell(0, 0);
			var (info, _, _) = GetActiveControlInfoFromFormForDocEngine(debugForm);

			var expectedMappingString = @"Mapping Path(s):
/Collection[X]
/Collection[@Property=''][X] or /Collection[@Property=''][3]
/Collection[1]/Z0_ChildOnly or /Collection[@Property=''][1]/Z0_ChildOnly
/Z0_ChildOnly";

			AssertContains(expectedMappingString, info, true);
		}

		public void TestBindingInfo_ForControlHavingMultipleDataBindings()
		{
			var bizO = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();

			using var dummyForm = new ZForm(bizO);
			using var control = new DummyControlWithDoubleBinding();
			dummyForm.Controls.Add(control);
			dummyForm.Show();

			AssertNoExceptionThrown(() => FormDebugInfo.GetActiveControlInfoFromForm(dummyForm));
		}

		DebugInfoForm Form => form ?? (form = new DebugInfoForm());
		DebugInfoForm form;

		protected override void TearDown()
		{
			base.TearDown();
			form?.Dispose();
		}
	}

	#region Helper Classes

	[TestClass]
	internal class DebugInfoForm : ZForm
	{
		public ZTextBox BlahBlahTextBox;
		public ZTextBox OtherTextBox;
		readonly Container components;

		public DebugInfoForm()
		{
			InitializeComponent();
			OtherTextBox.DataBindings.Add("Text", new ExampleDataSet(), "AccTransactionHeader.AH_Desc"); // Can't reference ZArchitecture so we wont use KBinding
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

#pragma warning disable IDE0001 // Simplify Names
		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.BlahBlahTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OtherTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = ControlDpiScalingHelper.NewScaledSize(432, 22);
			//
			// BlahBlahTextBox
			//
			this.BlahBlahTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(48, 120);
			this.BlahBlahTextBox.Name = "BlahBlahTextBox";
			this.BlahBlahTextBox.TabIndex = 2;
			this.BlahBlahTextBox.Text = "ZTextBox1";
			//
			// OtherTextBox
			//
			this.OtherTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(224, 152);
			this.OtherTextBox.Name = "OtherTextBox";
			this.OtherTextBox.TabIndex = 3;
			this.OtherTextBox.Text = "ZTextBox1";
			//
			// DebugInfoForm
			//
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(432, 266);
			this.Controls.Add(this.OtherTextBox);
			this.Controls.Add(this.BlahBlahTextBox);
			this.Name = "DebugInfoForm";
			this.Text = "Form1";
			this.Controls.SetChildIndex(this.BlahBlahTextBox, 0);
			this.Controls.SetChildIndex(this.OtherTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.ResumeLayout(false);
		}
		#endregion
#pragma warning restore IDE0001 // Simplify Names
	}

	[TestClass]
	internal class DummyEnterpriseBusinessObject_WithBadPrefixProperty : DummyEnterpriseBusinessObject
	{
		public DummyEnterpriseBusinessObject_WithBadPrefixProperty(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString L0L_WeirdProperty { get; } // No one should ever make a real table with this prefix...
	}

	[DesignerCategory("code")]
	[System.Diagnostics.DebuggerStepThrough()]
	[ToolboxItem(true)]
#pragma warning disable CW1108 // Do Not Use DataSet
	class ExampleDataSet : DataSet  // This is legacy architecture that will be removed
#pragma warning restore CW1108 // Do Not Use DataSet
	{
		AccTransactionHeaderDataTable tableAccTransactionHeader;

		public ExampleDataSet()
		{
			this.InitClass();
			var schemaChangedHandler = new CollectionChangeEventHandler(this.SchemaChanged);
			this.Tables.CollectionChanged += schemaChangedHandler;
			this.Relations.CollectionChanged += schemaChangedHandler;
		}

#if NETFRAMEWORK
		protected ExampleDataSet(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			var strSchema = ((string)(info.GetValue("XmlSchema", typeof(string))));
			if ((strSchema != null))
			{
#pragma warning disable CW1108 // Do Not Use DataSet
				var ds = new DataSet(); // This is legacy architecture that will be removed
#pragma warning restore CW1108 // Do Not Use DataSet
				ds.ReadXmlSchema(new XmlTextReader(new System.IO.StringReader(strSchema)));
				if ((ds.Tables["AccTransactionHeader"] != null))
				{
					this.Tables.Add(new AccTransactionHeaderDataTable(ds.Tables["AccTransactionHeader"]));
				}
				this.DataSetName = ds.DataSetName;
				this.Prefix = ds.Prefix;
				this.Namespace = ds.Namespace;
				this.Locale = ds.Locale;
				this.CaseSensitive = ds.CaseSensitive;
				this.EnforceConstraints = ds.EnforceConstraints;
				this.Merge(ds, false, System.Data.MissingSchemaAction.Add);
				this.InitVars();
			}
			else
			{
				this.InitClass();
			}
			this.GetSerializationData(info, context);
			var schemaChangedHandler = new CollectionChangeEventHandler(this.SchemaChanged);
			this.Tables.CollectionChanged += schemaChangedHandler;
			this.Relations.CollectionChanged += schemaChangedHandler;
		}
#endif

		[Browsable(false)]
		[DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Content)]
		public AccTransactionHeaderDataTable AccTransactionHeader
		{
			get
			{
				return this.tableAccTransactionHeader;
			}
		}

#pragma warning disable CW1108 // Do Not Use DataSet
		public override DataSet Clone()  // This is legacy architecture that will be removed
#pragma warning restore CW1108 // Do Not Use DataSet
		{
			var cln = ((ExampleDataSet)(base.Clone()));
			cln.InitVars();
			return cln;
		}

		protected override bool ShouldSerializeTables()
		{
			return false;
		}

		protected override bool ShouldSerializeRelations()
		{
			return false;
		}

		protected override void ReadXmlSerializable(XmlReader reader)
		{
			this.Reset();
#pragma warning disable CW1108 // Do Not Use DataSet
			var ds = new DataSet(); // This is legacy architecture that will be removed
#pragma warning restore CW1108 // Do Not Use DataSet
			ds.ReadXml(reader);
			if ((ds.Tables["AccTransactionHeader"] != null))
			{
				this.Tables.Add(new AccTransactionHeaderDataTable(ds.Tables["AccTransactionHeader"]));
			}
			this.DataSetName = ds.DataSetName;
			this.Prefix = ds.Prefix;
			this.Namespace = ds.Namespace;
			this.Locale = ds.Locale;
			this.CaseSensitive = ds.CaseSensitive;
			this.EnforceConstraints = ds.EnforceConstraints;
			this.Merge(ds, false, System.Data.MissingSchemaAction.Add);
			this.InitVars();
		}

		protected override System.Xml.Schema.XmlSchema GetSchemaSerializable()
		{
			var stream = new System.IO.MemoryStream();
			this.WriteXmlSchema(new XmlTextWriter(stream, null));
			stream.Position = 0;
			return System.Xml.Schema.XmlSchema.Read(new XmlTextReader(stream), null);
		}

		internal void InitVars()
		{
			this.tableAccTransactionHeader = ((AccTransactionHeaderDataTable)(this.Tables["AccTransactionHeader"]));
			if ((this.tableAccTransactionHeader != null))
			{
				this.tableAccTransactionHeader.InitVars();
			}
		}

		void InitClass()
		{
			this.DataSetName = "ExampleDataSet";
			this.Prefix = "";
			this.Namespace = "http://tempuri.org/ExampleDataSet.xsd";
			this.Locale = new System.Globalization.CultureInfo("en-US");
			this.CaseSensitive = false;
			this.EnforceConstraints = true;
			this.tableAccTransactionHeader = new AccTransactionHeaderDataTable();
			this.Tables.Add(this.tableAccTransactionHeader);
		}

		bool ShouldSerializeAccTransactionHeader()
		{
			return false;
		}

		void SchemaChanged(object sender, CollectionChangeEventArgs e)
		{
			if ((e.Action == System.ComponentModel.CollectionChangeAction.Remove))
			{
				this.InitVars();
			}
		}

		public delegate void AccTransactionHeaderRowChangeEventHandler(object sender, AccTransactionHeaderRowChangeEvent e);

		[System.Diagnostics.DebuggerStepThrough()]
		public class AccTransactionHeaderDataTable : DataTable, System.Collections.IEnumerable
		{
			DataColumn columnAH_AB;

			DataColumn columnAH_Desc;

			internal AccTransactionHeaderDataTable()
				: base("AccTransactionHeader")
			{
				this.InitClass();
			}

			internal AccTransactionHeaderDataTable(DataTable table)
				: base(table.TableName)
			{
				if ((table.CaseSensitive != table.DataSet.CaseSensitive))  // This is legacy architecture that will be removed
				{
					this.CaseSensitive = table.CaseSensitive;
				}
				if ((table.Locale.ToString() != table.DataSet.Locale.ToString()))  // This is legacy architecture that will be removed
				{
					this.Locale = table.Locale;
				}
				if ((table.Namespace != table.DataSet.Namespace))  // This is legacy architecture that will be removed
				{
					this.Namespace = table.Namespace;
				}
				this.Prefix = table.Prefix;
				this.MinimumCapacity = table.MinimumCapacity;
				this.DisplayExpression = table.DisplayExpression;
			}

			[Browsable(false)]
			public int Count
			{
				get
				{
					return this.Rows.Count;
				}
			}

			internal DataColumn AH_ABColumn
			{
				get
				{
					return this.columnAH_AB;
				}
			}

			internal DataColumn AH_DescColumn
			{
				get
				{
					return this.columnAH_Desc;
				}
			}

			public AccTransactionHeaderRow this[int index]
			{
				get
				{
					return ((AccTransactionHeaderRow)(this.Rows[index]));
				}
			}

			public event AccTransactionHeaderRowChangeEventHandler AccTransactionHeaderRowChanged;

			public event AccTransactionHeaderRowChangeEventHandler AccTransactionHeaderRowChanging;

			public event AccTransactionHeaderRowChangeEventHandler AccTransactionHeaderRowDeleted;

			public event AccTransactionHeaderRowChangeEventHandler AccTransactionHeaderRowDeleting;

			public void AddAccTransactionHeaderRow(AccTransactionHeaderRow row)
			{
				this.Rows.Add(row);
			}

			public AccTransactionHeaderRow AddAccTransactionHeaderRow(Guid aH_AB, string aH_Desc)
			{
				var rowAccTransactionHeaderRow = ((AccTransactionHeaderRow)(this.NewRow()));
				rowAccTransactionHeaderRow.ItemArray = new object[] {
																			aH_AB,
																			aH_Desc };
				this.Rows.Add(rowAccTransactionHeaderRow);
				return rowAccTransactionHeaderRow;
			}

			public System.Collections.IEnumerator GetEnumerator()
			{
				return this.Rows.GetEnumerator();
			}

			public override DataTable Clone()
			{
				var cln = ((AccTransactionHeaderDataTable)(base.Clone()));
				cln.InitVars();
				return cln;
			}

			protected override DataTable CreateInstance()
			{
				return new AccTransactionHeaderDataTable();
			}

			internal void InitVars()
			{
				this.columnAH_AB = this.Columns["AH_AB"];
				this.columnAH_Desc = this.Columns["AH_Desc"];
			}

			void InitClass()
			{
				this.columnAH_AB = new DataColumn("AH_AB", typeof(Guid), null, System.Data.MappingType.Element);
				this.Columns.Add(this.columnAH_AB);
				this.columnAH_Desc = new DataColumn("AH_Desc", typeof(string), null, System.Data.MappingType.Element);
				this.Columns.Add(this.columnAH_Desc);
			}

			public AccTransactionHeaderRow NewAccTransactionHeaderRow()
			{
				return ((AccTransactionHeaderRow)(this.NewRow()));
			}

			protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
			{
				return new AccTransactionHeaderRow(builder);
			}

			protected override Type GetRowType()
			{
				return typeof(AccTransactionHeaderRow);
			}

			protected override void OnRowChanged(DataRowChangeEventArgs e)
			{
				base.OnRowChanged(e);
				if ((this.AccTransactionHeaderRowChanged != null))
				{
					this.AccTransactionHeaderRowChanged(this, new AccTransactionHeaderRowChangeEvent(((AccTransactionHeaderRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowChanging(DataRowChangeEventArgs e)
			{
				base.OnRowChanging(e);
				if ((this.AccTransactionHeaderRowChanging != null))
				{
					this.AccTransactionHeaderRowChanging(this, new AccTransactionHeaderRowChangeEvent(((AccTransactionHeaderRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowDeleted(DataRowChangeEventArgs e)
			{
				base.OnRowDeleted(e);
				if ((this.AccTransactionHeaderRowDeleted != null))
				{
					this.AccTransactionHeaderRowDeleted(this, new AccTransactionHeaderRowChangeEvent(((AccTransactionHeaderRow)(e.Row)), e.Action));
				}
			}

			protected override void OnRowDeleting(DataRowChangeEventArgs e)
			{
				base.OnRowDeleting(e);
				if ((this.AccTransactionHeaderRowDeleting != null))
				{
					this.AccTransactionHeaderRowDeleting(this, new AccTransactionHeaderRowChangeEvent(((AccTransactionHeaderRow)(e.Row)), e.Action));
				}
			}

			public void RemoveAccTransactionHeaderRow(AccTransactionHeaderRow row)
			{
				this.Rows.Remove(row);
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class AccTransactionHeaderRow : DataRow
		{
			readonly AccTransactionHeaderDataTable tableAccTransactionHeader;

			internal AccTransactionHeaderRow(DataRowBuilder rb)
				: base(rb)
			{
				this.tableAccTransactionHeader = ((AccTransactionHeaderDataTable)(this.Table));
			}

			public Guid AH_AB
			{
				get
				{
					try
					{
						return ((Guid)(this[this.tableAccTransactionHeader.AH_ABColumn]));
					}
					catch (InvalidCastException e)
					{
						throw new StrongTypingException("Cannot get value because it is DBNull.", e);
					}
				}
				set
				{
					this[this.tableAccTransactionHeader.AH_ABColumn] = value;
				}
			}

			public string AH_Desc
			{
				get
				{
					try
					{
						return ((string)(this[this.tableAccTransactionHeader.AH_DescColumn]));
					}
					catch (InvalidCastException e)
					{
						throw new StrongTypingException("Cannot get value because it is DBNull.", e);
					}
				}
				set
				{
					this[this.tableAccTransactionHeader.AH_DescColumn] = value;
				}
			}

			public bool IsAH_ABNull()
			{
				return this.IsNull(this.tableAccTransactionHeader.AH_ABColumn);
			}

			public void SetAH_ABNull()
			{
				this[this.tableAccTransactionHeader.AH_ABColumn] = System.Convert.DBNull;
			}

			public bool IsAH_DescNull()
			{
				return this.IsNull(this.tableAccTransactionHeader.AH_DescColumn);
			}

			public void SetAH_DescNull()
			{
				this[this.tableAccTransactionHeader.AH_DescColumn] = System.Convert.DBNull;
			}
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public class AccTransactionHeaderRowChangeEvent : EventArgs
		{
			readonly AccTransactionHeaderRow eventRow;

			readonly DataRowAction eventAction;

			public AccTransactionHeaderRowChangeEvent(AccTransactionHeaderRow row, DataRowAction action)
			{
				this.eventRow = row;
				this.eventAction = action;
			}

			public AccTransactionHeaderRow Row
			{
				get
				{
					return this.eventRow;
				}
			}

			public DataRowAction Action
			{
				get
				{
					return this.eventAction;
				}
			}
		}
	}

	class TestDropEditWithoutListAttributeForm : ZChildForm
	{
		public ZDropEdit DropEdit;

		public TestDropEditWithoutListAttributeForm(BusinessObject bizObj) : base(bizObj)
		{
		}

		protected override void InitializeComponent()
		{
			DropEdit = new ZDropEdit
			{
				BindTo = AutoDummyBizo.Schema.Z0_FK_Code,
				Location = ControlDpiScalingHelper.NewScaledPoint(24, 8),
				Name = "DropEdit",
				Size = ControlDpiScalingHelper.NewScaledSize(232, 20),
				TabIndex = 5,
				ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription
			};
			Controls.Add(DropEdit);
		}
	}

	class DummyControlWithDoubleBinding : ZUserControl
	{
		public DummyControlWithDoubleBinding()
		{
			InitializeComponent();
			SetDataSourceBinding("Caption", "Z0_Description");
			SetDataSourceBinding("Mode", "Z0_Code");
		}

		void InitializeComponent()
		{
			this.DummyTextBox = new ZTextBox();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DummyTextBox.SuspendLayout();
			this.SuspendLayout();
			this.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			//
			// DummyControlWithDoubleBinding
			//
			this.Controls.Add(this.DummyTextBox);
			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(432, 380);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZString Mode
		{
			get { return mode; }
			set
			{
				mode = value;
			}
		}

		ZString mode;

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ZString Caption
		{
			get { return this.DummyTextBox.Text; }
			set { this.DummyTextBox.Text = value; }
		}

		ZTextBox DummyTextBox;
	}

#endregion
}
