using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTextBoxWithDetailsOnNoteTest : TestCaseWithDummy
	{
		[ExpectNoExceptions]
		public void TestConstructor() => new ZTextBoxWithDetailsOnNote().Dispose();

		[ExpectNoExceptions]
		public void TestShow()
		{
			using (var form = new ZTestForm())
			{
				form.Controls.Add(Control);
				form.Show();
			}
		}

		public void TestButtonText()
		{
			Control.ButtonText = "Blah";
			AssertEquals("New Button Text", "Blah", Control.ButtonText);
		}

		public void TestSize()
		{
			AssertEquals("Default Size", ControlDpiScalingHelper.NewScaledSize(200, 22), Control.Size);

			Control.Size = ControlDpiScalingHelper.NewScaledSize(100, 100);
			AssertEquals("New Size", ControlDpiScalingHelper.NewScaledSize(100, 22), Control.Size);
		}

		public void TestOverType_NoExistingNote()
		{
			Dummy.Z0_Description = "";

			using (var form = new ZForm(Dummy))
			{
				Control.NoteTypeDescription = "Detailed Goods Description";
				Control.SetBindingMember(DummyBizoSchema.Constants.Z0_Description);

				form.Controls.Add(Control);
				Control.SetDataBinding(Dummy, "");
				form.Show();
				UserIdleWorker.Flush();

				AssertEquals("precondition:", null, Control.LastShownPopup);

				for (var i = 0; i < Dummy.Z0_DescriptionInfo.MaxLength; i++)
				{
					KeySender.PostKeyDown(TextBox, Keys.A);
					Application.DoEvents();
				}

				AssertEquals("dialog should not be raised yet", null, Control.LastShownPopup);

				KeySender.PostKeyDown(TextBox, Keys.A);
				Application.DoEvents();

				AssertNotNull("Should have shown the popup", Control.LastShownPopup);
				AssertEquals("Detailed Goods Description", Control.LastShownPopup.BusinessEntity.ST_Description);

				AssertEquals("Note text", new string('a', Dummy.Z0_DescriptionInfo.MaxLength + 1), Control.LastShownPopup.BusinessEntity.ST_NoteText);
			}
		}

		public void TestOverType_ExistingNote()
		{
			Dummy.Z0_Description = "";
			_ = Dummy.GetNotes().AddNew(false, "Detailed Goods Description", "Blah");

			using (var form = new ZForm(Dummy))
			{
				Control.NoteTypeDescription = "Detailed Goods Description";
				Control.SetBindingMember(DummyBizoSchema.Constants.Z0_Description);

				form.Controls.Add(Control);
				Control.SetDataBinding(Dummy, "");
				form.Show();

				AssertEquals("precondition:", null, Control.LastShownPopup);

				for (var i = 0; i < Dummy.Z0_DescriptionInfo.MaxLength; i++)
				{
					KeySender.PostKeyDown(TextBox, Keys.A);
					Application.DoEvents();
				}

				AssertEquals("dialog should not be raised yet", null, Control.LastShownPopup);

				KeySender.PostKeyDown(TextBox, Keys.A);
				Application.DoEvents();

				AssertEquals("Should not have shown the popup since the note already exists.", null, Control.LastShownPopup);
			}
		}

		public void TestButtonTabStopIsFalse() => AssertEquals(false, Button.TabStop);

		#region Implementation

		protected override void TearDown()
		{
			base.TearDown();
			if (control != null)
			{
				control.Dispose();
			}
		}

		protected override Type TypeOfDummy => typeof(DummyEnterpriseBusinessObject);

		ZTextBoxWithDetailsOnNote Control => control ?? (control = new ZTextBoxWithDetailsOnNote());

		ZTextBox TextBox => (ZTextBox)typeof(ZTextBoxWithDetailsOnNote).GetField("textBox", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Control);

		ZButton Button => (ZButton)typeof(ZStmNotePopupBase).GetField("popupButton", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Control);

		ZTextBoxWithDetailsOnNote control;

		#endregion
	}
}
