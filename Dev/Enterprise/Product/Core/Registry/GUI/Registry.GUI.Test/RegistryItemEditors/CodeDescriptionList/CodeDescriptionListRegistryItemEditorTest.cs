using System;
using System.Data;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CodeDescriptionListRegistryItemEditor))]
	class CodeDescriptionListRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestGetCharacterCasingThrowsException()
		{
			CodeDescriptionPairListRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 3, RegistryStorageFlags.System);

			CodeDescriptionListRegistryItemEditor editor = new CodeDescriptionListRegistryItemEditor(registryItem, new CodeDescriptionPairListRegistryDataType(10),
				new CodeDescriptionPairListEditorInfo(false, true, (CodeDescriptionPairListEditorInfo.CharacterCasing)(-1)));

			using (Control editorPane = editor.NewWinFormsEditorPane())
			{
			}
		}

		public void TestNewWinFormsEditorPane()
		{
			CodeDescriptionPairListRegistryItem registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM_1", null, null, null, 3, RegistryStorageFlags.System);

			CodeDescriptionPairListEditorInfo editorInfo = new CodeDescriptionPairListEditorInfo(false, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal);
			editorInfo.IsMultilineDescriptionColumn = false;

			CodeDescriptionListRegistryItemEditor editor = new CodeDescriptionListRegistryItemEditor(registryItem, new CodeDescriptionPairListRegistryDataType(10),
				editorInfo);

			using (CodeDescriptionListEditControlForRegistry codeDescriptionListControl = (CodeDescriptionListEditControlForRegistry)editor.NewWinFormsEditorPane())
			{
				AssertEquals("should be single line", false, codeDescriptionListControl.CodeDescriptionGridForTest.Columns["Description"].ColumnStyle is ZMultiLineTextBoxColumnStyle);
				AssertEquals("EditorPane.RegistryItem", registryItem, codeDescriptionListControl.RegistryItemForTest);
				ZGrid codeDescriptionGrid1 = codeDescriptionListControl.CodeDescriptionGridForTest;
				AssertEquals("EditorPane's grid should only have 1 column", 1, codeDescriptionGrid1.Columns.Count);
				AssertEquals("EditorPane's grid should contain the Description column", "Description", codeDescriptionGrid1.Columns[0].ColumnStyle.MappingName);
				AssertEquals("Description column CharacterCasing", CharacterCasing.Normal, ((ZTextBoxColumnStyle)codeDescriptionGrid1.Columns[0].ColumnStyle).CharacterCasing);
			}

			registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM_2", (NoResString)"notification/e-mail/freight", null, null, 3, RegistryStorageFlags.System);

			editorInfo = new CodeDescriptionPairListEditorInfo(true, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Lower, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, (NoResString)"Index");
			editorInfo.IsMultilineDescriptionColumn = true;

			editor = new CodeDescriptionListRegistryItemEditor(registryItem, new CodeDescriptionPairListRegistryDataType(10),
				editorInfo);

			using (CodeDescriptionListEditControlForRegistry codeDescriptionListControl = (CodeDescriptionListEditControlForRegistry)editor.NewWinFormsEditorPane())
			{
				ZGrid codeDescriptionGrid = codeDescriptionListControl.CodeDescriptionGridForTest;
				ZTextBoxColumnStyle codeColumn = (ZTextBoxColumnStyle)codeDescriptionGrid.Columns[0].ColumnStyle;

				AssertEquals("should be multi line", true, codeDescriptionListControl.CodeDescriptionGridForTest.Columns["Description"].ColumnStyle is ZMultiLineTextBoxColumnStyle);
				AssertEquals("EditorPane.RegistryItem", registryItem, codeDescriptionListControl.RegistryItemForTest);
				AssertEquals("EditorPane's grid should have 2 columns", 2, codeDescriptionGrid.Columns.Count);
				AssertEquals("EditorPane's grid should contain the Code column", "Code", codeDescriptionGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals("EditorPane's grid should contain the Description column", "Description", codeDescriptionGrid.Columns[1].ColumnStyle.MappingName);
				AssertEquals("Code column max length", 10, codeColumn.TextBox.MaxLength);
				AssertEquals("Code column CharacterCasing", CharacterCasing.Lower, codeColumn.CharacterCasing);
				AssertEquals("Description column CharacterCasing", CharacterCasing.Normal, ((ZTextBoxColumnStyle)codeDescriptionGrid.Columns[1].ColumnStyle).CharacterCasing);
				AssertEquals("Code column caption", "Index", codeDescriptionGrid.Columns[0].ColumnStyle.HeaderText);
			}
		}

		public void TestSetNullValue()
		{
			using (Control editorPane = Editor.NewWinFormsEditorPane())
			{
				Editor.SetValueFromEditorPane(editorPane, null);
				ReadOnlyCodeDescriptionPairList list = (ReadOnlyCodeDescriptionPairList)Editor.GetValueFromEditorPane(editorPane);
				AssertEquals("List.Count", 0, list.Count);
			}
		}

		public void TestGetCustomValidation()
		{
			var registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM_2", (NoResString)"notification/e-mail/freight", null, null, 3, RegistryStorageFlags.System);

			var editorInfo = new CodeDescriptionPairListEditorInfo(true, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Lower, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, (NoResString)"Index");
			editorInfo.IsMultilineDescriptionColumn = true;

			AllowEmptyCodeAndDescription = false;
			var editor = new CodeDescriptionListRegistryItemEditor(registryItem, new CodeDescriptionPairListRegistryDataType(10), editorInfo);

			using (var codeDescriptionListControl = (CodeDescriptionListEditControlForRegistry)editor.NewWinFormsEditorPane())
			{
				var codeDescriptionGrid = codeDescriptionListControl.CodeDescriptionGridForTest;
				var table = new DataTable();
				table.Columns.Add(new DataColumn("Code", typeof(string)));
				table.Columns.Add(new DataColumn("Description", typeof(string)));
				table.Rows.Add("Code1", "Description1");
				table.Rows.Add("", "");
				codeDescriptionGrid.DataSource = table;

				AssertEquals("ErrorMessage", "Please enter non empty value.", Editor.GetCustomValidation(codeDescriptionListControl));
			}
		}

		public virtual void TestGetCustomValidation_AllowEmptyCodeAndDescription()
		{
			var registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM_2", (NoResString)"notification/e-mail/freight", null, null, 3, RegistryStorageFlags.System);

			var editorInfo = new CodeDescriptionPairListEditorInfo(true, true, CodeDescriptionPairListEditorInfo.CharacterCasing.Lower, CodeDescriptionPairListEditorInfo.CharacterCasing.Normal, (NoResString)"Index");
			editorInfo.IsMultilineDescriptionColumn = true;

			var editor = new CodeDescriptionListRegistryItemEditor(registryItem, new CodeDescriptionPairListRegistryDataType(10), editorInfo);

			using (var codeDescriptionListControl = (CodeDescriptionListEditControlForRegistry)editor.NewWinFormsEditorPane())
			{
				var codeDescriptionGrid = codeDescriptionListControl.CodeDescriptionGridForTest;
				var table = new DataTable();
				table.Columns.Add(new DataColumn("Code", typeof(string)));
				table.Columns.Add(new DataColumn("Description", typeof(string)));
				table.Rows.Add("Code1", "Description1");
				table.Rows.Add("Code2", "");
				table.Rows.Add("", "Desciption3");
				table.Rows.Add("", "");
				codeDescriptionGrid.DataSource = table;

				AssertEquals("ErrorMessage should be empty", "", Editor.GetCustomValidation(codeDescriptionListControl));
			}
		}

		public new void TestEnableEditorPane()
		{
			using (CodeDescriptionListEditControlForRegistry codeDescriptionListControl = (CodeDescriptionListEditControlForRegistry)Editor.NewWinFormsEditorPane())
			{
				Editor.EnableEditorPane(codeDescriptionListControl, true);
				AssertEquals(false, codeDescriptionListControl.ReadOnly);

				Editor.EnableEditorPane(codeDescriptionListControl, false);
				AssertEquals(true, codeDescriptionListControl.ReadOnly);
			}
		}

		public void TestAlwaysEditInEnglish()
		{
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockChs = Res.UseMockData())
			{
				mockChs.Put("d", new ResourceStringData("d", "描述"));
				var list = new CodeDescriptionPairList();
				list.AddPair("d", ResString.GetMultilingualString("d", "Description"));
				var registryItem = new CodeDescriptionPairListRegistryItem("Test", null, null, null, 3, RegistryStorageFlags.System, list);
				var editor = new CodeDescriptionListRegistryItemEditor(registryItem, registryItem.DataType, registryItem.EditorInfo);
				using (var control = (CodeDescriptionListEditControlForRegistry)editor.NewWinFormsEditorPane())
				{
					editor.SetValueFromEditorPane(control, registryItem.Value);
					AssertEquals("Precondition", "描述", registryItem.Value[0].Description);
					var editorList = (CodeDescriptionPairList)editor.GetValueFromEditorPane(control);
					AssertEquals("Description", editorList[0].Description);
				}
			}
		}

		#region Implementation

		bool AllowEmptyCodeAndDescription { get; set; } = true;

		protected override RegistryItemEditor GetEditor()
		{
			var registryItem = new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 3, RegistryStorageFlags.System);
			var dataType = new CodeDescriptionPairListRegistryDataType(10);
			dataType.AllowEmptyCodes = AllowEmptyCodeAndDescription;
			dataType.AllowEmptyDescriptions = AllowEmptyCodeAndDescription;
			return new CodeDescriptionListRegistryItemEditor(registryItem, dataType, new CodeDescriptionPairListEditorInfo());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CodeDescriptionListEditControlForRegistry)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CodeDescriptionListEditControlForRegistry);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CodeDescriptionPairListRegistryItem("TEST_REGISTRY_ITEM", null, null, null, 10, RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("Code", "Description");
			return new CodeDescriptionPairList[] { list };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			ReadOnlyCodeDescriptionPairList list1 = (ReadOnlyCodeDescriptionPairList)setValue;
			ReadOnlyCodeDescriptionPairList list2 = (ReadOnlyCodeDescriptionPairList)getValue;

			AssertEquals("GetValueFromEditorPaneCore().Count", list1.Count, list2.Count);

			for (int x = 0; x < list1.Count; ++x)
			{
				AssertEquals("GetValueFromEditorPaneCore().Code", list1[x].Code, list2[x].Code);
				AssertEquals("GetValueFromEditorPaneCore().Description", list1[x].Description, list2[x].Description);
			}
		}

		protected override void AssertGetValueTypeIsRegistryValueType(object getValue)
		{
			Assert("Value from EditorPane should be a ReadOnlyCodeDescriptionPairList.", getValue is ReadOnlyCodeDescriptionPairList);
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}
}
