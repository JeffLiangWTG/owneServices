using System.Data;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Core.Forms.Testing
{
	sealed class CodeDescriptionListEditControlTest : TransactionedTestCase
	{
		public void TestIsMultilineDescriptionColumn()
		{
			AssertEquals("Description ColumnStyle", typeof(ZTextBoxColumnStyle), Control.CodeDescriptionGrid.Columns["Description"].ColumnStyle.GetType());

			Control.IsMultilineDescriptionColumn = true;
			AssertEquals("Description ColumnStyle", typeof(ZMultiLineTextBoxColumnStyle), Control.CodeDescriptionGrid.Columns["Description"].ColumnStyle.GetType());

			Control.IsMultilineDescriptionColumn = false;
			AssertEquals("Description ColumnStyle", typeof(ZTextBoxColumnStyle), Control.CodeDescriptionGrid.Columns["Description"].ColumnStyle.GetType());
		}

		public void TestReadOnly()
		{
			Control.ReadOnly = true;
			AssertEquals(true, Control.ReadOnly);
			Control.ReadOnly = false;
			AssertEquals(false, Control.ReadOnly);
		}

		public void TestCodeColumnCaption()
		{
			using (var editControl = new CodeDescriptionListEditControlForTest())
			{
				AssertEquals("Code column caption", "Code", editControl.CodeDescriptionGrid.Columns["Code"].ColumnStyle.HeaderText);
				AssertEquals("Code column caption", "Code", ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).Caption);
			}

			using (var editControl = new CodeDescriptionListEditControlForTest(true, true, CharacterCasing.Normal, CharacterCasing.Normal, "Gah"))
			{
				AssertEquals("Code column caption", "Gah", editControl.CodeDescriptionGrid.Columns["Code"].ColumnStyle.HeaderText);
				AssertEquals("Code column caption", "Gah", ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).Caption);
			}
		}

		public void TestCodeAndDescriptionFieldCasing()
		{
			using (var editControl = new CodeDescriptionListEditControlForTest())
			{
				AssertEquals("Precondition: Column 0 should be the Code column", "Code", editControl.CodeDescriptionGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals("Precondition: Column 1 should be the Description column", "Description", editControl.CodeDescriptionGrid.Columns[1].ColumnStyle.MappingName);

				AssertEquals("Code column CharacterCasing", CharacterCasing.Upper, ((ZTextBoxColumnStyle)editControl.CodeDescriptionGrid.Columns[0].ColumnStyle).CharacterCasing);
				AssertEquals("Description column CharacterCasing", CharacterCasing.Upper, ((ZTextBoxColumnStyle)editControl.CodeDescriptionGrid.Columns[1].ColumnStyle).CharacterCasing);
				AssertEquals("Code column CharacterCasing", CharacterCasing.Upper, ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).CharacterCasing);
				AssertEquals("Description column CharacterCasing", CharacterCasing.Upper, ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[1]).CharacterCasing);
			}

			using (var editControl = new CodeDescriptionListEditControlForTest(true, true, CharacterCasing.Lower, CharacterCasing.Normal))
			{
				AssertEquals("Precondition: Column 0 should be the Code column", "Code", editControl.CodeDescriptionGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals("Precondition: Column 1 should be the Description column", "Description", editControl.CodeDescriptionGrid.Columns[1].ColumnStyle.MappingName);

				AssertEquals("Code column CharacterCasing", CharacterCasing.Lower, ((ZTextBoxColumnStyle)editControl.CodeDescriptionGrid.Columns[0].ColumnStyle).CharacterCasing);
				AssertEquals("Description column CharacterCasing", CharacterCasing.Normal, ((ZTextBoxColumnStyle)editControl.CodeDescriptionGrid.Columns[1].ColumnStyle).CharacterCasing);
				AssertEquals("Code column CharacterCasing", CharacterCasing.Lower, ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).CharacterCasing);
				AssertEquals("Description column CharacterCasing", CharacterCasing.Normal, ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[1]).CharacterCasing);
			}
		}

		public void TestCodeAndDescriptionFieldWidth()
		{
			using (var editControl = new CodeDescriptionListEditControlForTest())
			{
				AssertEquals("Precondition: Column 0 should be the Code column", "Code", editControl.CodeDescriptionGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals("Precondition: Column 1 should be the Description column", "Description", editControl.CodeDescriptionGrid.Columns[1].ColumnStyle.MappingName);

				AssertEquals("Code column width", 75, ((ZTextBoxColumnStyle)editControl.CodeDescriptionGrid.Columns[0].ColumnStyle).Width);
				AssertEquals("Description column width", 280, ((ZTextBoxColumnStyle)editControl.CodeDescriptionGrid.Columns[1].ColumnStyle).Width);
				AssertEquals("Code column width", 75, ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).Width);
				AssertEquals("Description column width", 280, ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[1]).Width);
			}

			using (var editControl = new CodeDescriptionListEditControlForTest(true, true, CharacterCasing.Upper, CharacterCasing.Upper))
			{
				AssertEquals("Precondition: Column 0 should be the Code column", "Code", editControl.CodeDescriptionGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals("Precondition: Column 1 should be the Description column", "Description", editControl.CodeDescriptionGrid.Columns[1].ColumnStyle.MappingName);

				AssertEquals("Code column width", 75, ((ZTextBoxColumnStyle)editControl.CodeDescriptionGrid.Columns[0].ColumnStyle).Width);
				AssertEquals("Description column width", 280, ((ZTextBoxColumnStyle)editControl.CodeDescriptionGrid.Columns[1].ColumnStyle).Width);
				AssertEquals("Code column width", 75, ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).Width);
				AssertEquals("Description column width", 280, ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[1]).Width);
			}

			using (var editControl = new CodeDescriptionListEditControlForTest(true, true, CharacterCasing.Upper, CharacterCasing.Upper, "Code", 100, 300))
			{
				AssertEquals("Precondition: Column 0 should be the Code column", "Code", editControl.CodeDescriptionGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals("Precondition: Column 1 should be the Description column", "Description", editControl.CodeDescriptionGrid.Columns[1].ColumnStyle.MappingName);

				AssertEquals("Code column width", 100, ((ZTextBoxColumnStyle)editControl.CodeDescriptionGrid.Columns[0].ColumnStyle).Width);
				AssertEquals("Description column width", 300, ((ZTextBoxColumnStyle)editControl.CodeDescriptionGrid.Columns[1].ColumnStyle).Width);
				AssertEquals("Code column width", 100, ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).Width);
				AssertEquals("Description column width", 300, ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[1]).Width);
			}
		}

		public void TestShowCodeAndDescriptionColumns()
		{
			using (var editControl = new CodeDescriptionListEditControlForTest())
			{
				AssertEquals("CodeDescriptionGrid should have 2 columns", 2, editControl.CodeDescriptionGrid.Columns.Count);
				AssertEquals("CodeDescriptionGrid should contain the Code column", "Code", editControl.CodeDescriptionGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals("CodeDescriptionGrid should contain the Description column", "Description", editControl.CodeDescriptionGrid.Columns[1].ColumnStyle.MappingName);
				AssertEquals("CodeDescriptionGrid should contain the Code column", "Code", ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).ColumnName);
				AssertEquals("CodeDescriptionGrid should contain the Description column", "Description", ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[1]).ColumnName);
			}

			using (var editControl = new CodeDescriptionListEditControlForTest(true, true, CharacterCasing.Upper, CharacterCasing.Normal))
			{
				AssertEquals("CodeDescriptionGrid should have 2 columns", 2, editControl.CodeDescriptionGrid.Columns.Count);
				AssertEquals("CodeDescriptionGrid should contain the Code column", "Code", editControl.CodeDescriptionGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals("CodeDescriptionGrid should contain the Description column", "Description", editControl.CodeDescriptionGrid.Columns[1].ColumnStyle.MappingName);
				AssertEquals("CodeDescriptionGrid should contain the Code column", "Code", ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).ColumnName);
				AssertEquals("CodeDescriptionGrid should contain the Description column", "Description", ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[1]).ColumnName);
			}

			using (var editControl = new CodeDescriptionListEditControlForTest(true, false, CharacterCasing.Upper, CharacterCasing.Normal))
			{
				AssertEquals("CodeDescriptionGrid should have 1 column", 1, editControl.CodeDescriptionGrid.Columns.Count);
				AssertEquals("CodeDescriptionGrid should contain the Code column", "Code", editControl.CodeDescriptionGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals("CodeDescriptionGrid should contain the Code column", "Code", ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).ColumnName);
			}

			using (var editControl = new CodeDescriptionListEditControlForTest(false, true, CharacterCasing.Upper, CharacterCasing.Normal))
			{
				AssertEquals("CodeDescriptionGrid should have 1 column", 1, editControl.CodeDescriptionGrid.Columns.Count);
				AssertEquals("CodeDescriptionGrid should contain the Description column", "Description", editControl.CodeDescriptionGrid.Columns[0].ColumnStyle.MappingName);
				AssertEquals("CodeDescriptionGrid should contain the Description column", "Description", ((ZTextBoxColumnStyleInfo)editControl.CodeDescriptionGrid.ColumnStyles[0]).ColumnName);
			}
		}

		public void TestFieldValue()
		{
			var setList = new CodeDescriptionPairList();
			setList.AddPair("ABC", "This is ABC");
			setList.AddPair("XYZ", "This is XYZ");

			Control.FieldValue = setList.ToXMLByteArray();

			var getList = new CodeDescriptionPairList(Control.FieldValue);
			AssertEquals("GetList.Count", 2, getList.Count);
			AssertEquals("GetList.GetDescriptionFromCode(\"ABC\")", "This is ABC", getList.GetDescriptionFromCode("ABC"));
			AssertEquals("GetList.GetDescriptionFromCode(\"XYZ\")", "This is XYZ", getList.GetDescriptionFromCode("XYZ"));
		}

		public void TestCodeMaxLength()
		{
			Assert("MaxLength not set", ((ZTextBoxColumnStyle)Control.CodeDescriptionGrid.Columns["Code"].ColumnStyle).TextBox.MaxLength > 100);

			Control.CodeMaxLength = 4;
			AssertEquals("MaxLength set", 4, ((ZTextBoxColumnStyle)Control.CodeDescriptionGrid.Columns["Code"].ColumnStyle).TextBox.MaxLength);
		}

		#region CodeDescriptionListEditControlForTest

		class CodeDescriptionListEditControlForTest : CodeDescriptionListEditControl
		{
			public CodeDescriptionListEditControlForTest()
			{
			}

			public CodeDescriptionListEditControlForTest(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing)
				: base(showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing)
			{
			}

			public CodeDescriptionListEditControlForTest(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing, string codeColumnCaption)
				: base(showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, codeColumnCaption)
			{
			}

			public CodeDescriptionListEditControlForTest(bool showCodeColumn, bool showDescriptionColumn, CharacterCasing codeFieldCasing, CharacterCasing descriptionFieldCasing, string codeColumnCaption,
				int codeColumnWidth, int descriptionColumnWidth)
				: base(showCodeColumn, showDescriptionColumn, codeFieldCasing, descriptionFieldCasing, codeColumnCaption, "Description", codeColumnWidth, descriptionColumnWidth)
			{
			}

			public new ZGrid CodeDescriptionGrid
			{
				get { return base.CodeDescriptionGrid; }
			}

			public new DataSet Data // for testing only
			{
				get { return base.Data; }
			}
		}

		#endregion

		#region Implementation

		CodeDescriptionListEditControlForTest Control
		{
			get { return control ?? (control = new CodeDescriptionListEditControlForTest()); }
		}
		CodeDescriptionListEditControlForTest control;

		protected override void TearDown()
		{
			base.TearDown();
			if (control != null)
			{
				control.Dispose();
			}
		}

		#endregion
	}
}
