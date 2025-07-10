using System;
using System.Data;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ZStmNoteGridTest : TestCaseWithFactory
	{
		public void TestColourDeciding()
		{
			var log = Factory.New<StmALog>();
			var note = log.Notes.AddNew();
			IStmNoteInternals noteInternals = note;
			var e = new ColourDecidingEventArgs(note);

			using (var form = new ZForm(log))
			{
				var tab = new ZTabControl();
				var page = new ZTabPage();
				var grid = new TestZStmNoteGrid();

				var info = new ZTextBoxColumnStyleInfo
				{
					ColumnName = StmNoteSchema.Constants.ST_Description
				};
				_ = grid.ColumnStyles.Add(info);
				grid.BindTo = "Notes.VisibleNotes";

				form.Controls.Add(tab);
				tab.TabPages.Add(page);
				page.Controls.Add(grid);

				form.Show();

				grid.ZStmNoteGrid_ColourDeciding(grid, e);
				AssertEquals("e.Colour", Color.Empty, GetColour(grid, note));

				note.ST_ParentID = ZGuid.Empty;
				AssertEquals("e.Colour", Color.FromArgb(235, 155, 155), GetColour(grid, note));

				noteInternals.IsNoteRead = true;
				AssertEquals("e.Colour", Color.Empty, GetColour(grid, note));

				noteInternals.IsNoteRead = false;
				_ = log.Logs.AddNew(ZArchitecture.Business.Events.ReadRelatedNotes);
				AssertEquals("e.Colour", SystemColors.Control, GetColour(grid, note));
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionsWhenNoDataBinding()
		{
			var log = Factory.New<StmALog>();
			using (var form = new ZForm(log))
			{
				var tab = new ZTabControl();
				var page = new ZTabPage();
				var grid = new TestZStmNoteGrid();

				form.Controls.Add(tab);
				tab.TabPages.Add(page);
				page.Controls.Add(grid);

				form.Show();

				grid.UnreadRelatedNoteTimer_Tick(this, new EventArgs());
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteNoteInTimerPeriod()
		{
			var log = Factory.New<StmALog>();
			var note = log.Notes.AddNew();
			IStmNoteInternals noteInternals = note;

			using (var form = new ZForm(log))
			{
				var tab = new ZTabControl();
				var page = new ZTabPage();
				var grid = new TestZStmNoteGrid();

				var info = new ZTextBoxColumnStyleInfo
				{
					ColumnName = StmNoteSchema.Constants.ST_Description
				};
				_ = grid.ColumnStyles.Add(info);
				grid.BindTo = "Notes.VisibleNotes";

				form.Controls.Add(tab);
				tab.TabPages.Add(page);
				page.Controls.Add(grid);

				form.Show();
				AssertEquals("Current item should be note", grid.ListManager.GetCurrent(), note);
				AssertEquals("List should have one record", 1, grid.ListManager.Count);
				Assert("Note should not be read", !noteInternals.IsNoteRead);
				note.Delete();
				grid.UnreadRelatedNoteTimer_Tick(this, new EventArgs());
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionsWhenNoParentZTabControl()
		{
			var log = Factory.New<StmALog>();
			using (var form = new ZForm(log))
			{
				var grid = new TestZStmNoteGrid();

				var info = new ZTextBoxColumnStyleInfo
				{
					ColumnName = StmNoteSchema.Constants.ST_Description
				};
				_ = grid.ColumnStyles.Add(info);

				grid.BindTo = "Notes.VisibleNotes";
				form.Controls.Add(grid);

				form.Show();
			}
		}

		public void TestCurrentDataItem()
		{
			var log = Factory.New<StmALog>();
			var note = log.Notes.AddNew();

			using (var form = new ZForm(log))
			{
				var tab = new ZTabControl();
				var page = new ZTabPage();
				var grid = new ZStmNoteGrid();

				var info = new ZTextBoxColumnStyleInfo
				{
					ColumnName = StmNoteSchema.Constants.ST_Description
				};
				_ = grid.ColumnStyles.Add(info);
				grid.BindTo = "Notes.VisibleNotes";

				page.Controls.Add(grid);
				tab.TabPages.Add(page);
				form.Controls.Add(tab);

				form.Show();

				AssertEquals("CurrentDataItemshould be note1", note, grid.CurrentDataItem);
			}
		}

		public void TestColourDecidingUsingCurrentDataItem()
		{
			var log = Factory.New<StmALog>();
			var note = log.Notes.AddNew();

			var aDummyClass = new DummyClass(Factory, null);
			aDummyClass.stmAlog = log;
			using (var form = new ZForm(aDummyClass))
			{
				var grid = new TestZStmNoteGrid();

				var info = new ZTextBoxColumnStyleInfo
				{
					ColumnName = StmNoteSchema.Constants.ST_Description
				};
				_ = grid.ColumnStyles.Add(info);
				grid.BindTo = "stmAlog.Notes.VisibleNotes";

				form.Controls.Add(grid);

				form.Show();

				note.ST_ParentID = ZGuid.Empty;
				AssertEquals("e.Colour", Color.FromArgb(235, 155, 155), GetColour(grid, note));
			}
		}

		Color GetColour(TestZStmNoteGrid grid, StmNote note)
		{
			var e = new ColourDecidingEventArgs(note);
			grid.ZStmNoteGrid_ColourDeciding(grid, e);
			return e.Colour;
		}

		sealed class TestZStmNoteGrid : ZStmNoteGrid
		{
			public new void ZStmNoteGrid_ColourDeciding(object sender, ColourDecidingEventArgs e) => base.ZStmNoteGrid_ColourDeciding(sender, e);

			public new void UnreadRelatedNoteTimer_Tick(object sender, EventArgs e) => base.UnreadRelatedNoteTimer_Tick(sender, e);
		}

		sealed class DummyClass : NonPersistentBusinessObject
		{
			public DummyClass(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public StmALog stmAlog { get; set; }

			public override SchemaGuidColumn PKSchemaColumn => stmAlog.PKSchemaColumn;
		}
	}
}
