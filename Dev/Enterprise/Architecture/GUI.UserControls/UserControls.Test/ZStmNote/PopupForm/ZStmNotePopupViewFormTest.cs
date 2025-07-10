using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZStmNotePopupViewForm))]
	class ZStmNotePopupViewFormTest : ZFormBasherTest
	{
		public void TestSetLeadingComment()
		{
			var note = Factory.New<StmNote>();
			note.ST_Description = "Eton";

			using (var form = new ZStmNotePopupViewForm(note))
			{
				var leadingCommentLabel = GetControl<ZLabel>(form, "leadingCommentLabel");
				var noteUserControl = GetControl<ZStmNotePopupUserControl>(form, "NoteUserControl");
				form.Show();

				var commentHeight1 = leadingCommentLabel.Height;
				var noteHeight1 = noteUserControl.Height;
				AssertEquals(false, leadingCommentLabel.Visible);
				AssertEquals(string.Empty, form.LeadingComment);
				AssertEquals(string.Empty, leadingCommentLabel.Text);
				AssertEquals(0, leadingCommentLabel.Height);

				form.LeadingComment = "Leading Comment";
				var commentHeight2 = leadingCommentLabel.Height;
				var noteHeight2 = noteUserControl.Height;
				AssertEquals(true, leadingCommentLabel.Visible);
				AssertEquals("Leading Comment", form.LeadingComment);
				AssertEquals("Leading Comment", leadingCommentLabel.Text);
				Assert(leadingCommentLabel.Height > 10);
				AssertEquals("Total Height Should Not Change", commentHeight1 + noteHeight1, commentHeight2 + noteHeight2);

				form.LeadingComment = string.Empty;
				AssertEquals(false, leadingCommentLabel.Visible);
				AssertEquals(string.Empty, form.LeadingComment);
				AssertEquals(string.Empty, leadingCommentLabel.Text);
				AssertEquals(commentHeight1, leadingCommentLabel.Height);
				AssertEquals(noteHeight1, noteUserControl.Height);

				form.LeadingComment = "Some long random leading comment that should require at least 2 lines to display properly.";
				form.LeadingComment = "Leading Comment";
				AssertEquals(commentHeight2, leadingCommentLabel.Height);
				AssertEquals(noteHeight2, noteUserControl.Height);
			}
		}

		public void TestBinding()
		{
			var note = Factory.New<StmNote>();
			note.ST_Description = "Eton";

			using (var form = new ZStmNotePopupViewForm(note))
			{
				var leadingCommentLabel = GetControl<ZLabel>(form, "leadingCommentLabel");
				var noteUserControl = GetControl<ZStmNotePopupUserControl>(form, "NoteUserControl");
				form.Show();
				Application.DoEvents();

				form.GetFrontMostActiveControl().Text = "Detailed Description";
				_ = form.SelectNextControl(form.GetFrontMostActiveControl(), true, true, true, false);
				AssertEquals("Note text set", "Detailed Description", note.ST_NoteDataAsText);
			}
		}

		public void TestFormHeading()
		{
			using (var form = (ZStmNotePopupViewForm)GetFormToBash())
			{
				form.BusinessEntity.ST_Description = "Eton";
				AssertEquals("FormHeading", "Eton Note", form.FormHeading);

				form.FormHeadingPrefix = "ABC";
				AssertEquals("FormHeading", "ABC - Eton Note", form.FormHeading);
			}
		}

		public void TestDisableEdit()
		{
			using (var form = (ZStmNotePopupViewForm)GetFormToBash())
			{
				form.DisableEdit();
				Assert(form.NoteUserControl.NoteTextBox.ReadOnly);
				Assert(!form.NoteUserControl.NoteTextBox.NoteRichTextBox.Visible);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var note = Factory.New<StmNote>();
			note.ST_Table = "DummyBizo";
			note.ST_ParentID = Factory.NewWithValidTestData<DummyBusinessObject>().PK;
			Factory.Save();
			var form = new ZStmNotePopupViewForm(note)
			{
				LeadingComment = "Some long random leading comment that should require at least 2 lines to display properly."
			};
			return form;
		}

		T GetControl<T>(ZStmNotePopupViewForm form, string name)
			where T : Control => (T)typeof(ZStmNotePopupViewForm).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);

		#endregion
	}
}
