using System;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Testing
{
	sealed class ZGridColumnsTest : TestCase
	{
		public void TestHasLayoutChanged()
		{
			Columns.DisposeAllColumns();

			DataGridColumnStyle columnStyle;

			using (Columns = CreateColumns())
			{
				AssertEquals("Initial HasLayoutChanged", false, Columns.HasLayoutChanged);

				Columns.Add(new ZTextBoxColumnStyleInfo("Column1", 80));
				AssertEquals("HasLayoutChanged after adding column programmatically", false, Columns.HasLayoutChanged);

				columnStyle = Columns[0].ColumnStyle;
				columnStyle.Width++;
				AssertEquals("HasLayoutChanged after changing width", true, Columns.HasLayoutChanged);

				Columns.HasLayoutChanged = false;
				Columns.Remove("Column1");
				AssertEquals("HasLayoutChanged after removing column programmatically", false, Columns.HasLayoutChanged);

				columnStyle.Width++;
				AssertEquals("HasLayoutChanged after changing width of removed column", false, Columns.HasLayoutChanged);

				Columns.Add(new ZTextBoxColumnStyleInfo("Column1", 80));
				columnStyle = Columns[0].ColumnStyle;
			}

			columnStyle.Width++;
			AssertEquals("HasLayoutChanged after changing width of cleared column", false, Columns.HasLayoutChanged);
		}

		public void TestAddColumnsAndIndexers()
		{
			AssertEquals(Columns[BoolColumnName], Columns[0]);
			AssertEquals(Columns[CalcColumnName], Columns[1]);
			AssertEquals(Columns[Date1ColumnName], Columns[2]);
			AssertEquals(Columns[Date2ColumnName], Columns[3]);
			AssertEquals(Columns[TextColumnName], Columns[4]);
		}

		public void TestCountAndClear()
		{
			Assert("Initial column count", Columns.Count > 0);
			Columns.DisposeAllColumns();
			AssertEquals("Column count after clear", 0, Columns.Count);
		}

		public void TestClone()
		{
			using (var clonedColumns = Columns.Clone())
			{
				Assert("Clone() did not clone the MappingName property correctly.", Columns[0].ColumnStyle.MappingName == clonedColumns[0].ColumnStyle.MappingName);
				Assert("Clone() did not clone the IsVisible property correctly.", Columns[0].IsVisible == clonedColumns[0].IsVisible);
				Assert("Clone() set Grid property properly", Columns.Grid == clonedColumns.Grid);
			}
		}

		public void TestTableNameProperty()
		{
			const string TestName = "MyTable";
			Columns.TableName = TestName;
			AssertEquals("Table name property was not set correctly.", TestName, Columns.TableName);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestColumnExists()
		{
			//adding columns for the second time and expect Exception be thrown
			AddColumns();
		}

		public void TestRemove()
		{
			var countBeforeRemove = Columns.Count;
			Columns.Remove(BoolColumnName);
			AssertEquals("Column Count after Remove", countBeforeRemove - 1, Columns.Count);
		}

		public void TestInsert()
		{
			var countBeforeRemove = Columns.Count;
			Columns.Insert(new ZTextBoxColumnStyleInfo(TextColumn2Name, 80), 0);
			AssertEquals("Column Count after Insert", countBeforeRemove + 1, Columns.Count);

			AssertEquals("Column at Correct Index", Columns[TextColumn2Name], Columns[0]);

			AssertEquals("Column Count before Insert", countBeforeRemove + 1, Columns.Count);
			Columns.Insert(new ZDateEditColumnStyleInfo(Date3ColumnName, 80), countBeforeRemove + 1);
			AssertEquals("Column Count after Insert", countBeforeRemove + 2, Columns.Count);

			AssertEquals("Column at Correct Index", Columns[Date3ColumnName], Columns[countBeforeRemove + 1]);
		}

		public void TestAddTextColumns()
		{
			using (var columns = CreateColumns())
			{
				columns.AddTextColumn("Column0", 123);
				AssertEquals("Columns[0].ColumnStyle.MappingName", "Column0", columns[0].ColumnStyle.MappingName);
				AssertEquals("Columns[0].Width", 123, columns[0].Width);
				AssertEquals("Columns[0].IsVisible", true, columns[0].IsVisible);
				AssertEquals("Columns[0].IsMandatory", true, columns[0].IsMandatory);
				AssertEquals("Columns[0].ColumnStyle.ReadOnly", false, ((columns[0].ColumnStyle).ReadOnly));
				AssertEquals("Columns[0].ColumnStyle.CharacterCasing", CharacterCasing.Upper, (((ZTextBoxColumnStyle)columns[0].ColumnStyle).CharacterCasing));

				columns.AddTextColumn("Column1", 124, false, true);
				AssertEquals("Columns[1].ColumnStyle.MappingName", "Column1", columns[1].ColumnStyle.MappingName);
				AssertEquals("Columns[1].Width", 124, columns[1].Width);
				AssertEquals("Columns[1].IsVisible", false, columns[1].IsVisible);
				AssertEquals("Columns[1].IsMandatory", true, columns[1].IsMandatory);
				AssertEquals("Columns[1].ColumnStyle.ReadOnly", false, ((columns[1].ColumnStyle).ReadOnly));
				AssertEquals("Columns[1].ColumnStyle.CharacterCasing", CharacterCasing.Upper, (((ZTextBoxColumnStyle)columns[1].ColumnStyle).CharacterCasing));

				columns.AddTextColumn("Column2", 125, true, false, true);
				AssertEquals("Columns[2].ColumnStyle.MappingName", "Column2", columns[2].ColumnStyle.MappingName);
				AssertEquals("Columns[2].Width", 125, columns[2].Width);
				AssertEquals("Columns[2].IsVisible", true, columns[2].IsVisible);
				AssertEquals("Columns[2].IsMandatory", false, columns[2].IsMandatory);
				AssertEquals("Columns[2].ColumnStyle.ReadOnly", true, ((columns[2].ColumnStyle).ReadOnly));
				AssertEquals("Columns[2].ColumnStyle.CharacterCasing", CharacterCasing.Upper, (((ZTextBoxColumnStyle)columns[2].ColumnStyle).CharacterCasing));

				columns.AddTextColumn("Column3", 126, false, false, true, CharacterCasing.Normal);
				AssertEquals("Columns[3].ColumnStyle.MappingName", "Column3", columns[3].ColumnStyle.MappingName);
				AssertEquals("Columns[3].Width", 126, columns[3].Width);
				AssertEquals("Columns[3].IsVisible", false, columns[3].IsVisible);
				AssertEquals("Columns[3].IsMandatory", false, columns[3].IsMandatory);
				AssertEquals("Columns[3].ColumnStyle.ReadOnly", true, ((columns[3].ColumnStyle).ReadOnly));
				AssertEquals("Columns[3].ColumnStyle.CharacterCasing", CharacterCasing.Normal, (((ZTextBoxColumnStyle)columns[3].ColumnStyle).CharacterCasing));

				columns.AddTextColumn("Column4", 127, CharacterCasing.Normal);
				AssertEquals("Columns[4].ColumnStyle.MappingName", "Column4", columns[4].ColumnStyle.MappingName);
				AssertEquals("Columns[4].Width", 127, columns[4].Width);
				AssertEquals("Columns[4].IsVisible", true, columns[4].IsVisible);
				AssertEquals("Columns[4].IsMandatory", true, columns[4].IsMandatory);
				AssertEquals("Columns[4].ColumnStyle.ReadOnly", false, ((columns[4].ColumnStyle).ReadOnly));
				AssertEquals("Columns[4].ColumnStyle.CharacterCasing", CharacterCasing.Normal, (((ZTextBoxColumnStyle)columns[4].ColumnStyle).CharacterCasing));
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGridIsMandatory()
		{
			new ZGridColumns(null);
		}

		public void TestSetColumnHeaderFontToGridHeaderFontOnAdd()
		{
			using (var columns = CreateColumns())
			{
				columns.AddTextColumn("Column0", 123);
				Assert(((ZGridColumnStyle)columns["Column0"].ColumnStyle).HeaderFont == grid.HeaderFont);
			}
		}

		#region Implementation

		ZGridColumns Columns;
		ZGrid grid;
		const string BoolColumnName = "BoolColumn";
		const string CalcColumnName = "CalcColumn";
		const string Date1ColumnName = "Date1Column";
		const string Date2ColumnName = "Date2Column";
		const string Date3ColumnName = "Date3Column";
		const string TextColumnName = "TextColumn";
		const string TextColumn2Name = "TextColumn2";

		protected override void SetUp()
		{
			base.SetUp();
			grid = new ZGrid();
			Columns = CreateColumns();
			AddColumns();
		}

		protected override void TearDown()
		{
			Columns.DisposeAllColumns();
			grid.Dispose();
			base.TearDown();
		}

		void AddColumns()
		{
			Columns.AddBoolColumn(BoolColumnName, 100, true, true);
			Columns.AddCalcEditColumn(CalcColumnName, 100, true, true, 3);
			Columns.AddDateColumn(Date1ColumnName, 100, true, true);
			Columns.AddDateColumn(Date2ColumnName, 100, true, true, ZDateTimePickerFormat.Time);
			Columns.AddTextColumn(TextColumnName, 100, true, true);
		}

		ZGridColumns CreateColumns()
		{
			return new ZGridColumns(grid);
		}

		#endregion
	}
}
