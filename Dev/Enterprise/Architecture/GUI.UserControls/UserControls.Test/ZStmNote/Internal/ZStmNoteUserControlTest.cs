using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZStmNoteUserControlTest : TransactionedTestCase
	{
		public void TestFocusOnTabPage()
		{
			using (var tabControl = new ZTabControl())
			{
				tabControl.TabPages.Add(new ZTabPage());
				tabControl.TabPages.Add(new ZTabPage());

				var noteTabPage = new ZTabPage();
				tabControl.TabPages.Add(noteTabPage);
				var noteControl = new ZStmNoteUserControl();
				noteTabPage.Controls.Add(noteControl);

				Application.DoEvents();
				noteControl.FocusOnTabPage();
				Assert(tabControl.SelectedTab == noteTabPage);
			}
		}

		public void TestSelectedNote()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyEnterpriseBusinessObject>();
			_ = dummy.GetNotes().AddNew();
			var note2 = dummy.GetNotes().AddNew();
			_ = dummy.GetNotes().AddNew();

			using (var form = new ZForm(dummy))
			{
				var tabControl = new ZTemplateTabControl();
				var noteTab = new ZStmNoteTabPageExposed();
				tabControl.TabPages.Insert(noteTab, 0);
				form.Controls.Add(tabControl);

				form.Show();
				Application.DoEvents();

				noteTab.ZStmNoteUserControlExposed.SelectedNote = note2;
				Assert("SelectedNote", noteTab.ZStmNoteUserControlExposed.SelectedNote == note2);
			}
		}

		public void Test_GoingFromTextToRtf_TheFirstTime_ShowsRtfCorrectly()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyEnterpriseBusinessObject>();
			var note1 = dummy.GetNotes().AddNew();
			note1.ST_Description = PredefinedNoteTypes.Instance.CartageHistoryNotes.Description; //text
			var note2 = dummy.GetNotes().AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.CartageHistoryNotes.Description; //text
			var note3 = dummy.GetNotes().AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description; //text

			using (var form = new ZForm(dummy))
			{
				var tabControl = new ZTemplateTabControl();
				var noteTab = new ZStmNoteTabPageExposed();
				tabControl.TabPages.Insert(noteTab, 0);
				form.Controls.Add(tabControl);

				form.Show();
				Application.DoEvents();

				//It looks really silly to have to write it this way, but because the sort is undefined when we open the form,
				//and we need the first note to be text and the second note to be rtf,
				//we have to reconfigure what the notes are after the grid loads and the arbitrary order has been decided.
				note1 = (StmNote)noteTab.ZStmNoteUserControlExposed.NoteGrid.List[0];
				note2 = (StmNote)noteTab.ZStmNoteUserControlExposed.NoteGrid.List[1];
				note3 = (StmNote)noteTab.ZStmNoteUserControlExposed.NoteGrid.List[2];

				note1.ST_Description = PredefinedNoteTypes.Instance.CartageHistoryNotes.Description; //text
				note1.ST_NoteText = "asdf";
				note2.ST_Description = PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description; //rtf
				note2.ST_NoteDataAsText = "qwer";
				note3.ST_Description = PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description; //text
				note3.ST_NoteText = "zxcv";

				Application.DoEvents();
				AssertEquals("asdf", noteTab.ZStmNoteUserControlExposed.NoteRichTextBox.NoteTextBox.Text);

				noteTab.ZStmNoteUserControlExposed.SelectedNote = note2;
				Application.DoEvents();
#if !WINZOR
				Assert("qwer", noteTab.ZStmNoteUserControlExposed.NoteRichTextBox.NoteRichTextBox.Rtf.Contains("qwer"));
#else
				Assert("qwer", noteTab.ZStmNoteUserControlExposed.NoteRichTextBox.NoteRichTextBox.Html.Contains("qwer"));
#endif
				noteTab.ZStmNoteUserControlExposed.SelectedNote = note1;
				Application.DoEvents();
				AssertEquals("asdf", noteTab.ZStmNoteUserControlExposed.NoteRichTextBox.NoteTextBox.Text);

				noteTab.ZStmNoteUserControlExposed.SelectedNote = note2;
				Application.DoEvents();
#if !WINZOR
				Assert("qwer", noteTab.ZStmNoteUserControlExposed.NoteRichTextBox.NoteRichTextBox.Rtf.Contains("qwer"));
#else
				Assert("qwer", noteTab.ZStmNoteUserControlExposed.NoteRichTextBox.NoteRichTextBox.Html.Contains("qwer"));
#endif
			}
		}

		public void TestRemoveAction()
		{
			var oldValue = Env.Security.NotesDelete.IsAllowed;

			try
			{
				Env.Security.NotesDelete.IsAllowed = true;
				AssertNoteUserControl(new NoteUserControlAssertion(delegate(ZStmNoteUserControl testNoteControl)
				{
					AssertEquals(RemoveAction.RemoveAndDelete, testNoteControl.NoteGrid.RemoveAction);
				}
				));

				Env.Security.NotesDelete.IsAllowed = false;
				AssertNoteUserControl(new NoteUserControlAssertion(delegate(ZStmNoteUserControl testNoteControl)
				{
					AssertEquals(RemoveAction.NoRemovePossible, testNoteControl.NoteGrid.RemoveAction);
				}
				));
			}
			finally
			{
				Env.Security.NotesDelete.IsAllowed = oldValue;
			}
		}

		public void TestSecurityPanelVisible()
		{
			var oldValue = Env.Security.Notes.IsAllowed;

			try
			{
				Env.Security.Notes.IsAllowed = true;
				AssertNoteUserControl(new NoteUserControlAssertion(delegate(ZStmNoteUserControl testNoteControl)
				{
					Assert(!testNoteControl.SecurityPanel.Visible);
				}
				));

				Env.Security.Notes.IsAllowed = false;
				AssertNoteUserControl(new NoteUserControlAssertion(delegate(ZStmNoteUserControl testNoteControl)
				{
					Assert(testNoteControl.SecurityPanel.Visible);
				}
				));
			}
			finally
			{
				Env.Security.Notes.IsAllowed = oldValue;
			}
		}

		public void TestDefaultAuditNotesColumns()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyEnterpriseBusinessObject>();

			using (var form = new ZForm(dummy))
			{
				var notesControl = new ZStmNoteUserControl();

				form.Controls.Add(notesControl);

				CombineAssertions(() =>
				{
					Assert("By User Full Name column is visible by default", notesControl.NoteGrid.GetColumnStyle("ST_CreatedByUserName").IsVisible);
					Assert("Last modified column is visible by default", notesControl.NoteGrid.GetColumnStyle("ST_LastModifiedDate").IsVisible);
					Assert("Last modified column is visible by default", notesControl.NoteGrid.GetColumnStyle("ST_LastModifiedDateLocal").IsVisible);
					Assert("Last modified column is visible by default", notesControl.NoteGrid.GetColumnStyle("ST_LastModifiedByUserName").IsVisible);
					Assert("Created date column is hidden by default", !notesControl.NoteGrid.GetColumnStyle("ST_CreatedDateUtc").IsVisible);
					Assert("Created date column is hidden by default", !notesControl.NoteGrid.GetColumnStyle("ST_CreatedDateLocal").IsVisible);
					Assert("Created date column is hidden by default", !notesControl.NoteGrid.GetColumnStyle("ST_CreatedByUserInitials").IsVisible);
				});
			}
		}

		delegate void NoteUserControlAssertion(ZStmNoteUserControl userControl);

		void AssertNoteUserControl(NoteUserControlAssertion noteControlAssertion)
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyEnterpriseBusinessObject>();
			_ = dummy.GetNotes().AddNew();

			using (var form = new ZForm(dummy))
			{
				var tabControl = new ZTemplateTabControl();
				var noteTab = new ZStmNoteTabPageExposed();
				tabControl.TabPages.Insert(noteTab, 0);
				form.Controls.Add(tabControl);

				form.Show();
				Application.DoEvents();

				noteControlAssertion(noteTab.ZStmNoteUserControlExposed);
			}
		}

		#region Test Classes

		internal class ZStmNoteTabPageExposed : ZStmNoteTabPage
		{
			public ZStmNoteUserControl ZStmNoteUserControlExposed
			{
				get
				{
					var field = typeof(ZStmNoteTabPage).GetField("ZStmNoteUserControl", BindingFlags.Instance | BindingFlags.NonPublic);
					return (ZStmNoteUserControl)field.GetValue(this);
				}
			}
		}

		#endregion
	}
}
