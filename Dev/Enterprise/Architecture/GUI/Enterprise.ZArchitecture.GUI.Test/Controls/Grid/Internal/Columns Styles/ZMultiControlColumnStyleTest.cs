using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Macro;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Grid.Internal;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.Testing.TextTemplateFormTest;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZMultiControlColumnStyleTest : TestCaseWithDummy
	{
		public void TestMultiControlF9Bug()
		{
			InsertDummiesForFindBox();

			var parentFields = new GridTestParentBusinessObject(Factory);

			var textField = parentFields.Fields.AddNew();
			textField.FieldType = nameof(FieldType.Text);
			textField.FieldValue = "text 1";

			var textField2 = parentFields.Fields.AddNew();
			textField2.FieldType = nameof(FieldType.Text);
			textField2.FieldValue = "text 2";

			AssertEquals("Count", 2, parentFields.Fields.Count);

			using (var testForm = new ZMultiControlTestForm(parentFields))
			{
				testForm.Show();

				testForm.zGrid1.CurrentCell = new DataGridCell(0, 1);
				var multiStyle = (ZMultiControlColumnStyle)testForm.zGrid1.Columns[1].ColumnStyle;
				var comboControl = multiStyle.EditControl;
				AssertEquals("Row 0 Cell 1 ControlType", FieldType.Text, comboControl.ControlType);
				AssertEquals("text 1", comboControl.CurrentEditor.Text);

				testForm.zGrid1.CurrentCell = new DataGridCell(1, 1);
				AssertEquals("Row 1 Cell 1 ControlType", FieldType.Text, comboControl.ControlType);
				AssertEquals("text 2", comboControl.CurrentEditor.Text);

				//press F9
				PostKeyToGrid(testForm.zGrid1, Keys.F9);

				testForm.zGrid1.CurrentCell = new DataGridCell(1, 1);
				AssertEquals("Row 1 Cell 1 ControlType", FieldType.Text, comboControl.ControlType);
				AssertEquals("text 1", comboControl.CurrentEditor.Text);
				AssertEquals("text 1", textField2.FieldValue);
			}
		}

		static void PostKeyToGrid(ZGrid grid, Keys key)
		{
			KeySender.PostKeyDown(grid.LastFocusedColumn.EditControl, key);
			Application.DoEvents();
		}

		public void TestMultiControlGridShowingDifferentControlsInOneColumn()
		{
			InsertDummiesForFindBox();

			var parentFields = new GridTestParentBusinessObject(Factory);

			var textField = parentFields.Fields.AddNew();
			textField.FieldType = nameof(FieldType.Text);
			textField.FieldValue = "text 1";

			var dateField = parentFields.Fields.AddNew();
			dateField.FieldType = nameof(FieldType.Date);
			dateField.FieldValue = DateTime.Now.ToString();

			var intField = parentFields.Fields.AddNew();
			intField.FieldType = nameof(FieldType.Integer);
			intField.FieldValue = "123456";

			var textField2 = parentFields.Fields.AddNew();
			textField2.FieldType = nameof(FieldType.Text);
			textField2.FieldValue = "text 2";

			var dateField2 = parentFields.Fields.AddNew();
			dateField2.FieldType = nameof(FieldType.Date);
			dateField2.FieldValue = DateTime.Now.AddDays(1).ToString();

			var intField3 = parentFields.Fields.AddNew();
			intField3.FieldType = nameof(FieldType.Integer);
			intField3.FieldValue = "56789";

			var guidField = parentFields.Fields.AddNew();
			guidField.FieldType = nameof(FieldType.Guid);
			guidField.FieldValue = Factory.LoadTop1<DummyBusinessObject>(new ZQuery()).PK.ToString();

			var codeField = parentFields.Fields.AddNew();
			codeField.FieldType = nameof(FieldType.TextCodeFindBox);
			codeField.FieldValue = "CODE";

			var dropField = parentFields.Fields.AddNew();
			dropField.FieldType = nameof(FieldType.TextDropEdit);
			dropField.FieldValue = "ANOTHERCODE";

			AssertEquals("Count", 9, parentFields.Fields.Count);

			using (var testForm = new ZMultiControlTestForm(parentFields))
			{
				testForm.Show();

				testForm.zGrid1.CurrentCell = new DataGridCell(0, 1);
				var multiStyle = (ZMultiControlColumnStyle)testForm.zGrid1.Columns[1].ColumnStyle;
				var comboControl = multiStyle.EditControl;
				AssertEquals("Row 0 Cell 1 ControlType", FieldType.Text, comboControl.ControlType);
				AssertEquals("Character Casing", CharacterCasing.Normal, comboControl.CharacterCasing);

				testForm.zGrid1.CurrentCell = new DataGridCell(1, 1);
				AssertEquals("Row 1 Cell 1 ControlType", FieldType.Date, comboControl.ControlType);

				testForm.zGrid1.CurrentCell = new DataGridCell(2, 1);
				AssertEquals("Row 2 Cell 1 ControlType", FieldType.Integer, comboControl.ControlType);

				testForm.zGrid1.CurrentCell = new DataGridCell(3, 1);
				AssertEquals("Row 3 Cell 1 ControlType", FieldType.Text, comboControl.ControlType);

				testForm.zGrid1.CurrentCell = new DataGridCell(4, 1);
				AssertEquals("Row 4 Cell 1 ControlType", FieldType.Date, comboControl.ControlType);

				testForm.zGrid1.CurrentCell = new DataGridCell(5, 1);
				AssertEquals("Row 5 Cell 1 ControlType", FieldType.Integer, comboControl.ControlType);

				testForm.zGrid1.CurrentCell = new DataGridCell(6, 1);
				AssertEquals("Row 6 Cell 1 ControlType", FieldType.Guid, comboControl.ControlType);

				testForm.zGrid1.CurrentCell = new DataGridCell(7, 1);
				AssertEquals("Row 6 Cell 1 ControlType", FieldType.TextCodeFindBox, comboControl.ControlType);

				testForm.zGrid1.CurrentCell = new DataGridCell(8, 1);
				AssertEquals("Row 6 Cell 1 ControlType", FieldType.TextDropEdit, comboControl.ControlType);
			}
		}

		[TestDate(2022, 12, 23, 19, 0, 0)]
		public void TestDateTimeFieldTypeShouldSupportRelativeDateTriggers()
		{
			AssertDateTimeFieldTypeShouldSupportRelativeDateTriggers("T", "23-DEC-22 19:00");
		}

		[TestDate(2022, 12, 23, 19, 0, 0)]
		public void TestDateTimeFieldTypeShouldSupportRelativeDateTriggersCalculate()
		{
			AssertDateTimeFieldTypeShouldSupportRelativeDateTriggers("T-1", "22-DEC-22 19:00");
		}

		void AssertDateTimeFieldTypeShouldSupportRelativeDateTriggers(string inputText, string expected)
		{
			var parentFields = new GridTestParentBusinessObject(Factory);
			var dateTimeField = parentFields.Fields.AddNew();
			dateTimeField.FieldType = nameof(FieldType.DateTime);

			using (var testForm = new ZMultiControlTestForm(parentFields))
			{
				testForm.Show();
				testForm.zGrid1.CurrentCell = new DataGridCell(0, 1);

				var multiCombinationControl = (ZMultiCombinationControl)testForm.ActiveControl;
				multiCombinationControl.CurrentEditor.Text = inputText;
				testForm.zGrid1.EndEdit();

				AssertEquals(expected, multiCombinationControl.CurrentEditor.Text);
				AssertEquals(expected, dateTimeField.FieldValue);
			}
		}

		public void TestMultiControlTextBoxTabbing()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);
			parentFields.Fields.AddNew().FieldType = nameof(FieldType.Text);
			parentFields.Fields.AddNew().FieldType = nameof(FieldType.Text);

			using (var testForm = new ZMultiControlTestForm(parentFields))
			{
				testForm.Show();
				testForm.zGrid1.CurrentCell = new DataGridCell(0, 0);

				KeySender.PostKeyDown(testForm.zGrid1, Keys.Tab);
				Application.DoEvents();
				AssertEquals(new DataGridCell(0, 1), testForm.zGrid1.CurrentCell);

				var multiCombinationControl = (ZMultiCombinationControl)testForm.ActiveControl;
				KeySender.PostKeyDown((Control)multiCombinationControl.CurrentEditor, Keys.Tab);
				Application.DoEvents();
				AssertEquals(new DataGridCell(0, 2), testForm.zGrid1.CurrentCell);
			}
		}

		public void TestDecimalMultiControlSupportLocalization()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			using (Culture.SetTemporarily(new CultureInfo("de-DE")))
			{
				var parentFields = new GridTestParentBusinessObject(Factory);
				parentFields.Fields.AddNew().FieldType = nameof(FieldType.Decimal);
				parentFields.Fields[0].FieldValue = "1000,23";

				using (var testForm = new ZMultiControlTestForm(parentFields))
				{
					testForm.Show();

					testForm.zGrid1.CurrentCell = new DataGridCell(1, 1);
					var multiControlStyle = (ZMultiControlColumnStyle)testForm.zGrid1.Columns[1].ColumnStyle;
					var zCalcControl = (ZCalcEdit)multiControlStyle.EditControl.Controls[0];
					AssertNotNull(zCalcControl);
					AssertEquals("The decimal value should be rendered as German culture.", "1.000,23", zCalcControl.Text);

					zCalcControl.Text = "1,56";
					testForm.zGrid1.CurrentCell = new DataGridCell(1, 2);
					AssertEquals("The decimal value should be rendered as German culture.", "1,56", testForm.zGrid1[0, 1].ToString());
				}
			}
		}

		public void TestBooleanMultiControlChecking()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);
			parentFields.Fields.AddNew().FieldType = nameof(FieldType.Boolean);
			parentFields.Fields[0].FieldValue = Constants.BooleanTrueString;

			using (var testForm = new ZMultiControlTestForm(parentFields))
			{
				testForm.Show();
				var rect = testForm.zGrid1.GetCurrentCellBounds();
				var pos = ((rect.Y + rect.Height / 2) << 16) + (rect.X + rect.Width + testForm.zGrid1.Columns[1].Width / 2);
				MouseSender.SendMessage(testForm.zGrid1, testForm.zGrid1.Handle, WindowsMessage.WM_LBUTTONDOWN, 0, pos);
				Application.DoEvents();
				MouseSender.SendMessage(testForm.zGrid1, testForm.zGrid1.Handle, WindowsMessage.WM_LBUTTONUP, 0, pos);
				Application.DoEvents();

				AssertEquals(new DataGridCell(0, 1), testForm.zGrid1.CurrentCell);
			}
		}

		public void TestMultiControl_TypeBinding_DecimalPrecision_Localised()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);
			parentFields.Fields.AddNew().FieldType = nameof(FieldType.Decimal);
			parentFields.Fields[0].Decimal = 4;
			var input = "123456789,1234";

			using (var testForm = new ZMultiControlTestForm(parentFields, true))
			{
				testForm.Show();
				using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("de-DE")))
				{
					testForm.zGrid1.CurrentCell = new DataGridCell(1, 1);
					var multiControlStyle = (ZMultiControlColumnStyle)testForm.zGrid1.Columns[1].ColumnStyle;
					var zCalcControl = (ZCalcEdit)multiControlStyle.EditControl.Controls[0];
					AutomateKeyboard(zCalcControl, multiControlStyle, input);
					AssertNotNull(zCalcControl);
					AssertEquals("The decimal value should be rendered as German culture.", "123.456.789,1234", zCalcControl.Text);
					AssertContains("The German decimal should have a COMMA", ",", zCalcControl.Text);
					AssertContains("The German decimal should have a DOT also", ".", zCalcControl.Text);
					AssertNotContains("The German decimal should NOT contain spaces.", " ", zCalcControl.Text);
				}

				using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-ZA")))
				{
					testForm.zGrid1.CurrentCell = new DataGridCell(2, 1);
					var multiControlStyle = (ZMultiControlColumnStyle)testForm.zGrid1.Columns[1].ColumnStyle;
					var zCalcControl = (ZCalcEdit)multiControlStyle.EditControl.Controls[0];
					AutomateKeyboard(zCalcControl, multiControlStyle, input);
					AssertEquals("The decimal value should be rendered as South African culture.", "123 456 789,1234", zCalcControl.Text);
					AssertEquals("The decimal must have a precision of 4", 4, zCalcControl.Text.Split(new char[1] { ',' })[1].Length);
					AssertContains("The South African decimal SHOULD have a COMMA", ",", zCalcControl.Text);
					AssertNotContains("The South African decimals SHOULD NOT use DOT notation", ".", zCalcControl.Text);
					parentFields.Fields[0].Decimal = 6;
					Application.DoEvents();
					parentFields.Fields[0].FieldValueInfo.RefreshBinding();
					AssertEquals("The decimal value should be rendered as South African culture.", "123 456 789,123400", zCalcControl.Text);
					AssertEquals("The decimal must have a precision of 6", 6, zCalcControl.Text.Split(new char[1] { ',' })[1].Length);
				}
			}
		}

		void AutomateKeyboard(ZCalcEdit targetControl, ZMultiControlColumnStyle style, string input, bool doTab = true)
		{
			foreach (var k in input)
			{
				KeySender.SendKeyPress(targetControl, targetControl.Handle, k);
			}
			if (doTab)
			{
				KeySender.PostKeyDown((Control)style.EditControl.CurrentEditor, Keys.Tab);
				Application.DoEvents();
			}
		}

		public void TestDecimalTextConversion()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Guid.Empty, Guid.Empty))
			{
				var parentFields = new GridTestParentBusinessObject(Factory);
				parentFields.Fields.AddNew().FieldType = nameof(FieldType.Boolean);
				parentFields.Fields[0].FieldValue = string.Empty;

				using (Culture.SetTemporarily(new CultureInfo("en-ZA")))
				{
					using (var form = new ZMultiControlTestForm(parentFields, true))
					{
						form.Show();
						var multiControlStyle = (ZMultiControlColumnStyle)form.zGrid1.Columns[1].ColumnStyle;
						form.zGrid1.CurrentCell = new DataGridCell(1, 1);
						parentFields.Fields[0].FieldValue = "...";
						parentFields.Fields[0].FieldType = nameof(FieldType.Text);
						parentFields.Fields[0].FieldValueInfo.RefreshBinding();
						var textBox = (ZTextBox.Bare)multiControlStyle.EditControl.Controls[0];
						AssertEquals("...", textBox.Text);
						parentFields.Fields[0].FieldType = nameof(FieldType.Decimal);
						parentFields.Fields[0].Decimal = 2;
						parentFields.Fields[0].FieldValueInfo.RefreshBinding();
						var callc = (ZCalcEdit)multiControlStyle.EditControl.Controls[0];
						AssertEquals(true, callc.Text == "0,00");
						parentFields.Fields[0].Decimal = 4;
						parentFields.Fields[0].FieldValueInfo.RefreshBinding();
						var calld = (ZCalcEdit)multiControlStyle.EditControl.Controls[0];
						AssertEquals(true, calld.Text == "0,0000");
						parentFields.Fields[0].FieldValue = "1,23";
						parentFields.Fields[0].FieldType = nameof(FieldType.Text);
						parentFields.Fields[0].FieldValueInfo.RefreshBinding();
						var calle = (ZTextBox.Bare)multiControlStyle.EditControl.Controls[0];
						AssertEquals(true, calle.Text == "1,23");
					}
				}
			}
		}

		public void TestValueTypeBinding_ValueTypeIsChanged_ChangeControlType()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);

			var field = parentFields.Fields.AddNew();
			field.FieldType = nameof(FieldType.Text);
			field.FieldValue = "555";

			using (var form = new ZMultiControlTestForm(parentFields))
			{
				form.Show();

				form.zGrid1.CurrentCell = new DataGridCell(1, 1);
				var multiStyle = (ZMultiControlColumnStyle)form.zGrid1.Columns[1].ColumnStyle;
				var comboControl = multiStyle.EditControl;

				field.FieldType = nameof(FieldType.Integer);

				AssertEquals("ControlType should be changed", FieldType.Integer, comboControl.ControlType);
			}
		}

		[RequiresSTA]
		public void TestModuleIdBinding_ModuleIdIsChanged_ChangeControlModuleId()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);

			var field = parentFields.Fields.AddNew();
			field.FieldType = nameof(FieldType.TextCodeFindBox);
			field.FieldValue = "555";

			using (var form = new ZMultiControlTestForm(parentFields))
			{
				((IZColumnStyleInfoWithModuleID)form.zGrid1.ColumnStyles[1]).ModuleID = ModuleIDs.NotAssigned;
				form.Show();

				field.ModuleID = DummyModuleIDs.Dummy;
				form.zGrid1.CurrentCell = new DataGridCell(1, 1);
				var multiStyle = (ZMultiControlColumnStyle)form.zGrid1.Columns[1].ColumnStyle;
				var comboControl = multiStyle.EditControl;

				var currentEditor = (ZPopupFindBox)comboControl.CurrentEditor;
				currentEditor.SelectFromPopupForm();

				AssertEquals("The current editor module ID should change back to the default value", currentEditor.ModuleID, ModuleIDs.NotAssigned);
				AssertEquals("Module id of popup form should match expected", DummyModuleIDs.Dummy, ((EmbeddedModulePopup)currentEditor.PopupForm).Module.ID);
				((EmbeddedModulePopup)currentEditor.PopupForm).Close();

				field.ModuleID = DummyModuleIDs.DummyWithTemplates;
				currentEditor.SelectFromPopupForm();

				AssertEquals("The current editor module ID should change back to the default value", currentEditor.ModuleID, ModuleIDs.NotAssigned);
				AssertEquals("Module id of popup form should match expected", DummyModuleIDs.DummyWithTemplates, ((EmbeddedModulePopup)currentEditor.PopupForm).Module.ID);
				((EmbeddedModulePopup)currentEditor.PopupForm).Close();
			}
		}

		void InsertDummiesForFindBox()
		{
			for (var i = 0; i < 10; i++)
			{
				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Code = "ABC" + i;
			}

			Factory.Save();
		}

		public void TestCanGetValueOnFirstCall()
		{
			var organisation = Factory.New<IOrgHeader>();
			organisation.OH_Code = "MAHORG";
			organisation.OH_FullName = "Test Org";

			Factory.Save();

			var parentFields = new GridTestParentBusinessObject(Factory);

			var field = parentFields.Fields.AddNew();
			field.FieldType = nameof(FieldType.OrganisationGuid);
			field.FieldValue = organisation.PK.ToString();

			var collection = (IOrgHeaderCollection)Activator.CreateInstance(ObjectFactory.GetType<IOrgHeaderCollection>(), new object[] { Factory });
			collection.Add(organisation);

			field.TestList = collection;

			using (var form = new ZMultiControlTestForm(parentFields))
			{
				form.Show();

				var grid = form.zGrid1;
				var style = (ZMultiControlColumnStyle)grid.Columns[1].ColumnStyle;

				AssertEquals("MAHORG", style.GetValueAsString(grid.ListManager, 0));
				grid.CurrentCell = new DataGridCell(0, 0);
				grid.BeginEdit(style, 0);
				AssertEquals("We don't override behaviour when not readonly", organisation.PK.ToString(), style.TextBox.Text);

				grid.SetReadOnlyIncludingChildren();
				grid.SetReadOnlyIncludingColumnStyles(true);
				grid.CurrentCell = new DataGridCell(0, 0);
				grid.BeginEdit(style, 0);
				AssertEquals("When readonly, override and just display text", "MAHORG", style.TextBox.Text);
			}
		}

		public void TestCastingError()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);

			var guidField = parentFields.Fields.AddNew();
			guidField.FieldType = nameof(FieldType.GuidDropEdit);

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "ASS";

			guidField.FieldValue = dummy.PK.ToString();

			using (var form = new ZMultiControlTestForm(parentFields))
			{
				form.Show();

				var columnStyle = (ZMultiControlColumnStyle)form.zGrid1.Columns[1].ColumnStyle;

				AssertEquals("ASS", columnStyle.FormatValueObject(guidField, guidField.FieldValue));
			}
		}

		public void TestUpdateControlType_TextMacro_RootsIsNull()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);

			var dummy = Factory.NewWithValidTestData<DummyBusinessObjectWithIRootTypeProvider>();

			var textMacroField = parentFields.Fields.AddNew();
			textMacroField.FieldType = nameof(FieldType.TextMacro);
			textMacroField.FieldValue = dummy.PK.ToString();

			using (var form = new ZMultiControlTestForm(parentFields))
			{
				var columnStyleInfo = form.zGrid1.GetColumnStyle("FieldValue") as ZMultiControlColumnStyleInfo;
				columnStyleInfo.DataFieldsOnly = true;
				columnStyleInfo.AllowMultipleMacroses = false;

				form.Show();

				var columnStyle = (ZMultiControlColumnStyle)form.zGrid1.Columns.First(c => c.ColumnName == "FieldValue").ColumnStyle;
				var control = columnStyle.EditControl;
				var macrosFindBox = control.GetControlForControlType(FieldType.TextMacro) as ZMacrosFindBox;

				form.zGrid1.CurrentCell = new DataGridCell(1, 1); // to trigger Edit

				AssertContainsExactElementsInAnyOrder(new Type[] { typeof(DummyBusinessObjectWithIRootTypeProvider), typeof(DummyBusinessObject), typeof(GridTestParentBusinessObject) }, macrosFindBox.RootTypes);
				AssertContainsExactElementsInAnyOrder(new BusinessObject[] { parentFields }, macrosFindBox.Roots);
			}
		}

		public void TestDataFieldsOnly_And_AllowMultipleMacroses()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();

			var textMacroField = parentFields.Fields.AddNew();
			textMacroField.FieldType = nameof(FieldType.TextMacro);
			textMacroField.FieldValue = dummy.PK.ToString();

			using (var form = new ZMultiControlTestForm(parentFields))
			{
				var columnStyleInfo = form.zGrid1.GetColumnStyle("FieldValue") as ZMultiControlColumnStyleInfo;

				CombineAssertions("Initial Condition", () =>
				{
					AssertEquals(false, columnStyleInfo.DataFieldsOnly);
					AssertEquals(true, columnStyleInfo.AllowMultipleMacroses);
				});

				columnStyleInfo.DataFieldsOnly = true;
				columnStyleInfo.AllowMultipleMacroses = false;

				form.Show();

				var columnStyle = (ZMultiControlColumnStyle)form.zGrid1.Columns.First(c => c.ColumnName == "FieldValue").ColumnStyle;
				var control = columnStyle.EditControl;
				var macrosFindBox = control.GetControlForControlType(FieldType.TextMacro) as ZMacrosFindBox;

				CombineAssertions("Initial Condition", () =>
				{
					AssertEquals(false, macrosFindBox.DataFieldsOnly);
					AssertEquals(true, macrosFindBox.AllowMultipleMacroses);
				});

				form.zGrid1.CurrentCell = new DataGridCell(1, 1); // to trigger Edit

				AssertEquals("WHEN style-info's DataFieldsOnly is set, SHOULD be reflected on macrosFindBox", true, macrosFindBox.DataFieldsOnly);
				AssertEquals("WHEN style-info's AllowMultipleMacroses is set, SHOULD be reflected on macrosFindBox", false, macrosFindBox.AllowMultipleMacroses);
			}
		}

		public void TestControlTypeForBizObj_WhenColumnIsCustomColumn()
		{
			var dummyChild = Dummy.Collection.AddNew();
			dummyChild.Z0_NVarChar = nameof(FieldType.Text);
			var columnStyleInfo = new ZMultiControlColumnStyleInfo();
			columnStyleInfo.ColumnName = "__CONTRACTNUMBER__prop__ZString";
			columnStyleInfo.Caption = "ContractNumber";
			((IOverridablePropertyDescriptor)columnStyleInfo).PropertyDescriptor = new WorkflowCustomPropertyDescriptorForTesting("__CONTRACTNUMBER__prop__ZString", typeof(ZString), true);

			using (var testForm = new ZTestForm())
			{
				testForm.Grid.ColumnStyles.Add(columnStyleInfo);
				testForm.Grid.SetDataBinding(Dummy, "Collection");

				var propertyCollection = new CustomPropertyCollectionImpl(propertyName => "A", null);
				propertyCollection.Add(typeof(ZString), "__CONTRACTNUMBER__prop__ZString", DynamicMetaData.ListDataSource(null));
				var customBusinessObject = new CustomBusinessObject(Factory, null, propertyCollection);
				dummyChild.RegisterEditableChildObject(customBusinessObject);

				testForm.Show();

				Application.DoEvents();

				using (var columnStyle = (ZMultiControlColumnStyle)testForm.Grid.TableStyles[0].GridColumnStyles[columnStyleInfo.ColumnName])
				{
					AssertNotNull(columnStyle);
					AssertEquals("The FieldType should be TextDropEdit", FieldType.TextDropEdit, columnStyle.ControlTypeForBizObj(dummyChild));

					dummyChild.UnRegisterEditableChildObject(customBusinessObject);
					propertyCollection = new CustomPropertyCollectionImpl(propertyName => "A", null);
					propertyCollection.Add(typeof(ZString), "__CONTRACTNUMBER__prop__ZString");
					customBusinessObject = new CustomBusinessObject(Factory, null, propertyCollection);

					dummyChild.RegisterEditableChildObject(customBusinessObject);
					AssertEquals("The FieldType should be Text", FieldType.Text, columnStyle.ControlTypeForBizObj(dummyChild));

					dummyChild.UnRegisterEditableChildObject(customBusinessObject);
					AssertEquals("The FieldType should be Text", FieldType.Text, columnStyle.ControlTypeForBizObj(dummyChild));
				}
			}
		}

		public void TestControlTypeForBizObj_WhenFieldTypeColumnNameIsValidWithNoProperty()
		{
			var dummyChild = Factory.New<BusinessObjectWithoutPropertyInfo>();
			Dummy.Collection.Add(dummyChild);

			var columnStyleInfo = new ZMultiControlColumnStyleInfo();
			columnStyleInfo.ColumnName = "GuidForTest";
			columnStyleInfo.Caption = "GuidForTest";
			columnStyleInfo.FieldTypeColumnName = "GuidForTestFieldType";

			using (var testForm = new ZTestForm())
			{
				testForm.Grid.ColumnStyles.Add(columnStyleInfo);
				testForm.Grid.SetDataBinding(Dummy, "Collection");
				testForm.Show();

				using (var columnStyle = (ZMultiControlColumnStyle)testForm.Grid.TableStyles[0].GridColumnStyles[columnStyleInfo.ColumnName])
				{
					AssertNotNull(columnStyle);
					AssertEquals("The FieldType should be Guid", FieldType.Guid, columnStyle.ControlTypeForBizObj(dummyChild));
				}
			}
		}

		public void TestControlTypeForBizObj_WhenFieldTypeColumnNameIsEmptyWithNoProperty()
		{
			var dummyChild = Factory.New<BusinessObjectWithoutPropertyInfo>();
			Dummy.Collection.Add(dummyChild);

			var columnStyleInfo = new ZMultiControlColumnStyleInfo();
			columnStyleInfo.ColumnName = "GuidForTest";
			columnStyleInfo.Caption = "GuidForTest";
			columnStyleInfo.FieldTypeColumnName = string.Empty;

			using (var testForm = new ZTestForm())
			{
				testForm.Grid.ColumnStyles.Add(columnStyleInfo);
				testForm.Grid.SetDataBinding(Dummy, "Collection");
				testForm.Show();

				using (var columnStyle = (ZMultiControlColumnStyle)testForm.Grid.TableStyles[0].GridColumnStyles[columnStyleInfo.ColumnName])
				{
					AssertNotNull(columnStyle);
					AssertEquals("The FieldType should be Text", FieldType.Text, columnStyle.ControlTypeForBizObj(dummyChild));
				}
			}
		}

		public void TestVariableInformationExistsInMapTreePresenter()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);

			using (var form = new ZMultiControlTestForm(parentFields))
			{
				form.Show();

				var columnStyle = (ZMultiControlColumnStyle)form.zGrid1.Columns.First(c => c.ColumnName == "FieldValue").ColumnStyle;
				var control = columnStyle.EditControl;
				var macrosFindBox = control.GetControlForControlType(FieldType.AntlrMacro) as ZMacrosFindBox;
				var dummyWorkflow = Factory.New<DummyWithWorkflow>();
				var task = dummyWorkflow.WorkflowItems.Triggers.AddNew();
				macrosFindBox.Current = task;
				macrosFindBox.PropertyDescriptor = TypeDescriptor.GetProperties(task).Find("TriggerConditions+TriggerConditionValue", false);

				macrosFindBox.SelectFromPopupForm();
				Assert(macrosFindBox.mapTreePresenter.VariableParentTypes.Length > 0);
				Assert(macrosFindBox.mapTreePresenter.VariableNames.Length > 0);

				Assert(macrosFindBox.mapTreePresenter.VariableParentTypes.Any(m => m is Type && m.Name.Equals("Environment")));
			}
		}

		public void TestAntlrMacroColumnStyleForMcrDataField()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);

			using (var form = new ZMultiControlTestForm(parentFields))
			{
				form.Show();

				var columnStyle = (ZMultiControlColumnStyle)form.zGrid1.Columns.First(c => c.ColumnName == "FieldValue").ColumnStyle;
				var control = columnStyle.EditControl;
				var macrosFindBox = control.GetControlForControlType(FieldType.AntlrMacro) as ZMacrosFindBox;
				var dummyWorkflow = Factory.New<DummyWithWorkflow>();
				var task = dummyWorkflow.WorkflowItems.Triggers.AddNew();
				macrosFindBox.Current = task;
				macrosFindBox.PropertyDescriptor = TypeDescriptor.GetProperties(task).Find("TriggerConditions+TriggerConditionValue", false);

				macrosFindBox.SelectFromPopupForm();
				Assert(macrosFindBox.mapTreePresenter.UseMcrEvaluator);
				AssertEquals("", macrosFindBox.mapTreePresenter.OpeningMacroTag);
				AssertEquals("", macrosFindBox.mapTreePresenter.ClosingMacroTag);
			}
		}

		public void TestMapTreePresenterPropertyWithGivenLibrary()
		{
			var parentFields = new GridTestParentBusinessObject(Factory);

			using (var form = new ZMultiControlTestForm(parentFields))
			{
				form.Show();

				var columnStyle = (ZMultiControlColumnStyle)form.zGrid1.Columns.First(c => c.ColumnName == "FieldValue").ColumnStyle;
				var control = columnStyle.EditControl;
				var macrosFindBox = control.GetControlForControlType(FieldType.AntlrMacro) as ZMacrosFindBox;
				var dummyWorkflow = Factory.New<DummyWithWorkflow>();
				var task = dummyWorkflow.WorkflowItems.Triggers.AddNew();
				macrosFindBox.Current = task;
				macrosFindBox.PropertyDescriptor = TypeDescriptor.GetProperties(task).Find("TriggerConditions+TriggerConditionValue", false);
				macrosFindBox.Libraries = new IMacroLibrary[] { new CargoWiseOneStandardLibrary() };

				macrosFindBox.SelectFromPopupForm();
				Assert("MapTreePresenter should be initialized with given library", macrosFindBox.mapTreePresenter.Libraries.Any(m => m is CargoWiseOneStandardLibrary));
			}
		}

		public class BusinessObjectWithoutPropertyInfo : DummyChildBusinessObject
		{
			public BusinessObjectWithoutPropertyInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZGuid GuidForTest => Z0_Guid;
			public ZString GuidForTestFieldType => nameof(FieldType.Guid);
		}
	}
}
