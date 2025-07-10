using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZStmNotePopupBaseTest : TestCaseWithFactory
	{
		public void TestButtonTextDefault() => AssertEquals("", Control.ButtonText);

		public void TestButtonTabStopIsFalse() => AssertEquals(false, Button.TabStop);

		public void TestPopupButton()
		{
			using (var form = new ZForm(Dummy))
			{
				Control.NoteTypeDescription = "Detailed Goods Description";
				form.Controls.Add(Control);
				Control.SetDataBinding(Dummy, "");
				form.Show();

				AssertEquals("precondition:", null, Control.LastShownPopup);

				Control.PerformButtonClick();
				AssertNotNull("Should have shown the popup", Control.LastShownPopup);
				AssertEquals("Detailed Goods Description", Control.LastShownPopup.BusinessEntity.ST_Description);
			}
		}

		public void TestF3()
		{
			using (var form = new ZForm(Dummy))
			{
				Control.NoteTypeDescription = "Detailed Goods Description";
				form.Controls.Add(Control);
				Control.SetDataBinding(Dummy, "");
				form.Show();

				AssertEquals("precondition:", null, Control.LastShownPopup);

				KeySender.PostKeyDown(Control, Keys.F3);
				Application.DoEvents();

				AssertNotNull("Should have shown the popup", Control.LastShownPopup);
				AssertEquals("Detailed Goods Description", Control.LastShownPopup.BusinessEntity.ST_Description);
			}
		}

		public void TestMaximumNoteLength_ShouldSetMaxLengthOnNote()
		{
			using (var form = new ZForm(Dummy))
			using (var control = new ZStmNotePopupBaseWithExposedNote())
			{
				control.NoteTypeDescription = "Detailed Goods Description";
				form.Controls.Add(control);
				control.SetDataBinding(Dummy, "");
				form.Show();

				AssertNull(control.Note_Exposed);
				control.CreateNewNoteIfNotExists_Exposed();
				AssertEquals(-1, control.Note_Exposed.NoteTextMaxLength);

				control.MaximumNoteLength = 100;
				AssertEquals(100, control.Note_Exposed.NoteTextMaxLength);
			}
		}

		public void TestMaximumNoteLength_ShouldSetMaxLengthOnNewNotes()
		{
			using (var form = new ZForm(Dummy))
			using (var control = new ZStmNotePopupBaseWithExposedNote())
			{
				control.NoteTypeDescription = "Detailed Goods Description";
				form.Controls.Add(control);
				control.SetDataBinding(Dummy, "");
				form.Show();

				AssertNull(control.Note_Exposed);
				control.MaximumNoteLength = 100;
				control.CreateNewNoteIfNotExists_Exposed();
				AssertEquals(100, control.Note_Exposed.NoteTextMaxLength);
			}
		}

		public void TestMaximumNoteLength_ShouldSetMaxLengthAfterBinding()
		{
			using (var form = new ZForm(Dummy))
			using (var control = new ZStmNotePopupBaseWithExposedNote())
			{
				var existingNote = Dummy.Notes.AddNew();
				existingNote.ST_Description = "Detailed Goods Description";
				existingNote.ST_NoteText = "MD WAS HERE".PadRight(111, '!');

				AssertEquals("StmNote.NoteTextMaxLength should be default", PredefinedNoteTypes.Instance.DetailedGoodsDescription.TextOnlyMaxLength, existingNote.NoteTextMaxLength);

				control.NoteTypeDescription = "Detailed Goods Description";
				control.MaximumNoteLength = 100;

				form.Controls.Add(control);
				AssertNull(control.Note_Exposed);
				control.SetDataBinding(Dummy, "");
				AssertEquals("StmNote.NoteTextMaxLength should be reset", 100, control.Note_Exposed.NoteTextMaxLength);
			}
		}

		#region Implementation

		class ZStmNotePopupBaseWithExposedNote : ZStmNotePopupBase
		{
			internal StmNote Note_Exposed => base.Note;

			internal void CreateNewNoteIfNotExists_Exposed() => base.CreateNewNoteIfNotExists();
		}

		DummyEnterpriseBusinessObject Dummy => dummy ?? (dummy = Factory.New<DummyEnterpriseBusinessObject>());
		DummyEnterpriseBusinessObject dummy;

		protected override void TearDown()
		{
			base.TearDown();
			if (control != null)
			{
				control.Dispose();
			}
		}

		ZStmNotePopupBase Control => control ?? (control = new ZStmNotePopupEdit());

		ZButton Button => (ZButton)typeof(ZStmNotePopupBase).GetField("popupButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Control);

		ZStmNotePopupEdit control;

		#endregion
	}
}
