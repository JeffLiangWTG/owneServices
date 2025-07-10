using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using MethodInvoker = System.Windows.Forms.MethodInvoker;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZStmNoteRichTextBoxTest : TestCaseWithFactory
	{
		public void TestOverrideButtonStartsInvisible()
		{
			ZStmNoteRichTextBox control;
			using (var form = new ZForm())
			{
				control = new ZStmNoteRichTextBox();
				form.Controls.Add(control);
				form.Show();
				AssertEquals(false, control.OverrideValidationButton.Visible);
				AssertEquals(false, control.OverrideValidationButton.Enabled);
			}
		}

		public void TestSetCaretLocationAfterBindingHasFinished()
		{
			ZStmNoteRichTextBox control;
			using (var form = new ZForm())
			{
				control = new ZStmNoteRichTextBox();
				form.Controls.Add(control);
				form.Show();

				control.NoteTextBox.Text = "snth snth snth snth snth snth";
				control.SetCaretLocationAfterBindingHasFinished(10);
				Application.DoEvents();

				AssertEquals("SelectionStart", 10, control.NoteTextBox.SelectionStart);
				AssertEquals("SelectionLength", 0, control.NoteTextBox.SelectionLength);
			}
		}
		public void TestNotSettingSelectionLengthToZero()
		{
			ZStmNoteRichTextBox poop;
			using (var form = new ZForm())
			{
				poop = new ZStmNoteRichTextBox
				{
					IsTextOnly = true
				};

				form.Controls.Add(poop);
				form.Show();

				poop.NoteTextBox.Text = "Greetings I Am Test Text 123!";

				poop.NoteTextBox.SelectionStart = 3;
				poop.NoteTextBox.SelectionLength = 6;

				poop.UpdateControlLayoutAfterBindingHasFinished(true);
				Application.DoEvents();

				AssertEquals("SelectionStart", 3, poop.NoteTextBox.SelectionStart);
				AssertEquals("SelectionLength", 6, poop.NoteTextBox.SelectionLength);
			}
		}

		public void TestSpellCheckerWhenTextOnlyIsTrue()
		{
			const string CheckSpellingMenuKey = "checkSpelling";
			using (var form = new ZForm())
			{
				var noteBox = new ZStmNoteRichTextBox
				{
					IsTextOnly = true
				};

				form.Controls.Add(noteBox);
				form.Show();

				AssertNotNull("Spell checker should be initialized", noteBox.NoteTextBox.ContextMenuStrip?.Items[CheckSpellingMenuKey]);
			}
		}

		public void TestSpellCheckerWhenTextOnlyIsFalse()
		{
			const string CheckSpellingMenuKey = "checkSpelling";
			using (var form = new ZForm())
			{
				var noteBox = new ZStmNoteRichTextBox
				{
					IsTextOnly = false
				};

				form.Controls.Add(noteBox);
				form.Show();

				var richTextBox = noteBox.NoteRichTextBox.GetRichTextBoxForTest();

				AssertNotNull("Spell checker should be initialized", richTextBox.ContextMenuStrip?.Items[CheckSpellingMenuKey]);
			}
		}

		[ExpectNoExceptions]
		public void TestUpdateControlLayoutWhenDisposedDoesNotBlowUp()
		{
			ZStmNoteRichTextBox control;
			using (var form = new ZForm())
			{
				control = new ZStmNoteRichTextBox();
				form.Controls.Add(control);
				form.Show();
				control.IsTextOnly = true;
				_ = control.BeginInvoke(new MethodInvoker(control.Dispose));
				_ = control.GetType().GetMethod("UpdateControlLayoutAfterBindingHasFinished", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, new object[] { true });
			}

			Application.DoEvents();
		}

		[TestDate(2006, 07, 20)]
		public void TestNoteTextBoxDoesUserInfoOnF5Key()
		{
			using (var form = new ZForm())
			{
				var control = new ZStmNoteRichTextBox();
				form.Controls.Add(control);
				form.Show();
				control.IsTextOnly = true;

				var textbox = GetTextBoxControl(control);
				_ = textbox.Focus();
				AssertEquals(string.Empty, textbox.Text);

				KeySender.PostKeyDown(textbox, Keys.F5);
				Application.DoEvents();
				AssertEquals(EnvProxy.Instance.CurrentUser.InitialsAndDateTimeGmt, textbox.Text);
			}
		}

		public void TestModifyButtonVisibility()
		{
			TestPredefinedNoteTypes.Register();

			try
			{
				var master = Factory.New<DummyEnterpriseBusinessObject>();
				var note = master.GetNotes().AddNew();

				using (var form = new MockZForm(note))
				{
					form.Show();

					note.ST_Description = TestPredefinedNoteTypes.Instance.PopupLogNote.Description;
					Application.DoEvents();
					AssertEquals("IsPopupLog", true, form.RichTextBox.IsPopupLog);
					AssertEquals("ModifyButton.Visible", true, form.RichTextBox.ModifyButton.Visible);
					AssertEquals("ModifyButton.ReadOnly", false, form.RichTextBox.ModifyButton.ReadOnly);

					note.ST_Description = TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength100.Description;
					Application.DoEvents();
					AssertEquals("IsPopupLog", false, form.RichTextBox.IsPopupLog);
					AssertEquals("ModifyButton.Visible", false, form.RichTextBox.ModifyButton.Visible);

					form.RichTextBox.IsModifyButtonVisible = false;
					note.ST_Description = TestPredefinedNoteTypes.Instance.PopupLogNote.Description;
					Application.DoEvents();
					AssertEquals("IsPopupLog", true, form.RichTextBox.IsPopupLog);
					AssertEquals("ModifyButton.Visible", false, form.RichTextBox.ModifyButton.Visible);

					note.ReadOnly = true;
					note.ST_Description = "";
					form.RichTextBox.IsModifyButtonVisible = true;
					note.ST_Description = TestPredefinedNoteTypes.Instance.PopupLogNote.Description;
					Application.DoEvents();
					AssertEquals("IsPopupLog", true, form.RichTextBox.IsPopupLog);
					AssertEquals("ModifyButton.Visible", true, form.RichTextBox.ModifyButton.Visible);
					AssertEquals("ModifyButton.ReadOnly", true, form.RichTextBox.ModifyButton.ReadOnly);
				}
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestPopupButtonClick()
		{
			TestPredefinedNoteTypes.Register();

			try
			{
				var master = Factory.New<DummyEnterpriseBusinessObject>();
				var note = master.GetNotes().AddNew();

				using (var form = new MockZForm(note))
				{
					form.Show();
					AssertNull("Precondition: No forms should be shown yet.", ZFormModaliser.ActiveForm);
					note.ST_NoteText = "Boo!";

					note.ST_Description = TestPredefinedNoteTypes.Instance.PopupLogNote.Description;
					Application.DoEvents();
					AssertEquals("IsPopupLog", true, form.RichTextBox.IsPopupLog);
					form.RichTextBox.PopupButton.PerformClick();
					using (var popupForm = (ZStmNotePopupViewForm)ZFormModaliser.ActiveForm)
					{
						AssertEquals("popupForm.BusinessEntity", note, popupForm.BusinessEntity);
						AssertEquals("popupForm.BusinessEntity.ST_NoteText", "Boo!", popupForm.BusinessEntity.ST_NoteText);
						AssertEquals("popupForm.GetType()", typeof(ZStmNotePopupViewForm), popupForm.GetType());
					}

					note.ST_Description = TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength100.Description;
					Application.DoEvents();
					AssertEquals("IsPopupLog", false, form.RichTextBox.IsPopupLog);
					form.RichTextBox.PopupButton.PerformClick();
					using (var popupForm = (ZStmNotePopupForm)ZFormModaliser.ActiveForm)
					{
						Assert("popupForm.BusinessEntity should be a clone.", popupForm.BusinessEntity != note);
						AssertEquals("popupForm.BusinessEntity.ST_NoteText", "Boo!", popupForm.BusinessEntity.ST_NoteText);
						AssertEquals("popupForm.GetType()", typeof(ZStmNotePopupForm), popupForm.GetType());
					}
				}
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestModifyButtonClick()
		{
			TestPredefinedNoteTypes.Register();

			try
			{
				var master = Factory.New<DummyEnterpriseBusinessObject>();
				var note = master.GetNotes().AddNew();

				using (var form = new MockZForm(note))
				{
					form.Show();
					AssertNull("Precondition: No forms should be shown yet.", ZFormModaliser.ActiveForm);
					note.ST_NoteText = "Boo!";

					note.ST_Description = TestPredefinedNoteTypes.Instance.PopupLogNote.Description;
					Application.DoEvents();
					AssertEquals("IsPopupLog", true, form.RichTextBox.IsPopupLog);
					form.RichTextBox.ModifyButton.PerformClick();

					using (var popupForm = (ZStmNotePopupForm)ZFormModaliser.ActiveForm)
					{
						Assert("popupForm.BusinessEntity should be a clone.", popupForm.BusinessEntity != note);
						AssertEquals("popupForm.BusinessEntity.ST_NoteText", "", popupForm.BusinessEntity.ST_NoteText);
						AssertEquals("popupForm.GetType()", typeof(ZStmNotePopupForm), popupForm.GetType());
					}
				}
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		[ExpectNoExceptions]
		public void TestLayoutUpdateWithoutDrainingMessageQueue()
		{
			TestPredefinedNoteTypes.Register();

			try
			{
				var master = Factory.New<DummyEnterpriseBusinessObject>();
				var note = master.GetNotes().AddNew();

				using (var form = new MockZForm(note))
				{
					form.Show();
					form.RichTextBox.NoteTextBox.Text = "note text";

					note.ST_Description = TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength100.Description;
					Application.DoEvents();
					AssertEquals("IsTextOnly", true, form.RichTextBox.IsTextOnly);
					AssertEquals("NoteRichTextBox.Visible", true, form.RichTextBox.NoteTextBox.Visible);
					AssertEquals("NoteTextBox.Visible", false, form.RichTextBox.NoteRichTextBox.Visible);
					note.ST_Description = TestPredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes.Description;
					AssertEquals("IsTextOnly", false, form.RichTextBox.IsTextOnly);
					AssertEquals("NoteTextBox.Visible", false, form.RichTextBox.NoteTextBox.Visible);
					AssertEquals("NoteRichTextBox.Visible", true, form.RichTextBox.NoteRichTextBox.Visible);
				}
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		[ExpectNoExceptions]
		public void TestClickOnRichTextBoxImmediatelyAfterSelectingTextOnlyTypeDescriptionDoesNotBlowUp()
		{
			TestPredefinedNoteTypes.Register();
			try
			{
				var master = Factory.New<DummyEnterpriseBusinessObject>();
				using (var form = new MockZForm(master))
				{
					var note = master.GetNotes().AddNew();
					form.Show();
					form.RichTextBox.NoteRichTextBox.GetRichTextBoxForTest().GotFocus += delegate
					{
						note.ST_Description = TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength100.Description;
					};
					PerformButtonDown(form.RichTextBox.NoteRichTextBox.GetRichTextBoxForTest());
					Application.DoEvents();
					_ = form.TextBox.Focus();
				}
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		[ExpectNoExceptions]
		public void TestNoteRichTextBoxTopAndHeightBasedOnSettings()
		{
			TestPredefinedNoteTypes.Register();
			try
			{
				var master = Factory.New<DummyEnterpriseBusinessObject>();

				using (var form = new MockZForm(master))
				{
					var note = master.GetNotes().AddNew();
					form.Show();
					form.RichTextBox.NoteTextBox.Text = "note text";

					AssertEquals("IsTextOnly", false, form.RichTextBox.IsTextOnly);
					AssertEquals("NoteTextBox.Visible", false, form.RichTextBox.NoteTextBox.Visible);
					AssertEquals("NoteTextBox.Enabled", true, form.RichTextBox.NoteTextBox.Enabled);
					AssertEquals("NoteRichTextBox.Visible", true, form.RichTextBox.NoteRichTextBox.Visible);
					AssertEquals("NoteRichTextBox.Enabled", true, form.RichTextBox.NoteRichTextBox.Enabled);
					AssertEquals("UpdateControlLayout has been called", true, form.RichTextBox.updateControlLayoutCalled);
					AssertEquals("NoteTextBox.Top", 0, form.RichTextBox.NoteTextBox.Top);
					AssertEquals("NoteTextBox.Height", 236, form.RichTextBox.NoteRichTextBox.Height);
					AssertEquals("NoteTextBox.Height", 236, form.RichTextBox.NoteTextBox.Height);

					form.RichTextBox.NoteRichTextBox.Visible = false;
					form.RichTextBox.NoteRichTextBox.Enabled = false;
					form.RichTextBox.NoteTextBox.Visible = true;
					form.RichTextBox.NoteTextBox.Enabled = true;
					form.RichTextBox.IsTextOnly = true;
					form.RichTextBox.IsDescriptionVisibleInTextMode = true;

					form.RichTextBox.UpdateControlLayoutAfterBindingHasFinished(true);

					AssertEquals("NoteTextBox.Visible", true, form.RichTextBox.NoteTextBox.Visible);
					AssertEquals("NoteTextBox.Enabled", true, form.RichTextBox.NoteTextBox.Enabled);
					AssertEquals("NoteRichTextBox.Visible", false, form.RichTextBox.NoteRichTextBox.Visible);
					AssertEquals("NoteRichTextBox.Enabled", false, form.RichTextBox.NoteRichTextBox.Enabled);
					AssertEquals("NoteTextBox.Top", 32, form.RichTextBox.NoteTextBox.Top);
					AssertEquals("NoteTextBox.Height", 236, form.RichTextBox.NoteRichTextBox.Height);
					AssertEquals("NoteTextBox.Height", 204, form.RichTextBox.NoteTextBox.Height);
				}
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestOverrideButtonClick()
		{
			TestPredefinedNoteTypes.Register();

			try
			{
				var note = Factory.New<DummyStmNote>();
				var master = Factory.New<DummyEnterpriseBusinessObject>();
				note.Master = master;

				using (var form = new MockZForm(note))
				{
					form.Show();
					AssertNull("Precondition: No forms should be shown yet.", ZFormModaliser.ActiveForm);
					note.ST_NoteText = @"<RulePK>7912ed8c-1edf-4041-a3a5-62eb1765a351</RulePK>
<Reason>Australia to United States:</Reason>
<IsError>Y</IsError>
<Macro>""<ChangeCase(""<JS_GoodsDescription>"",L)>"" .Contains(""widgets"")</Macro>";

					note.ST_Description = PredefinedNoteTypes.Instance.CountryRulesValidation.Description;
					Application.DoEvents();
					form.RichTextBox.OverrideValidationButton.PerformClick();

					AssertEquals("Note IsDeleted", true, note.IsDeleted);
					var acks = Factory.Load<GenCustomAddOnRuleAck>(new ZQuery(GenCustomAddOnRuleAckSchema.XK_RuleID, new ZGuid("7912ed8c-1edf-4041-a3a5-62eb1765a351"))).Select(c => c.XK_ParentID).ToList();

					AssertCollectionContains("GenCustomAddOnRuleAck record for Master", note.Master.NotesParentPK, acks);
					AssertCollectionContains("GenCustomAddOnRuleAck record for SecondObject", note.SecondObject.NotesParentPK, acks);
				}
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		void PerformButtonDown(Control control) => MouseSender.PostMessage(control, control.Handle, CargoWise.Interop.WindowsMessage.WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);

		[ExpectNoExceptions]
		public void TestBindingOnNoteDescriptionChange()
		{
			TestPredefinedNoteTypes.Register();

			try
			{
				var master = Factory.New<DummyEnterpriseBusinessObject>();
				var note = master.GetNotes().AddNew();

				using (var form = new MockZForm(note))
				{
					form.Show();
					for (var i = 0; i < 5; i++)
					{
						note.ST_Description = TestPredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes.Description;
						Application.DoEvents();
						form.RichTextBox.NoteRichTextBox.Focus();
						for (var j = 0; j < 5; j++)
						{
							KeySender.PostKeyDown(form.RichTextBox.NoteRichTextBox, Keys.A);
						}
						_ = form.Focus();
						Application.DoEvents();

						note.ST_Description = TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength100.Description;
						form.RichTextBox.NoteRichTextBox.Focus();
						for (var j = 0; j < 5; j++)
						{
							KeySender.PostKeyDown(form.RichTextBox.NoteRichTextBox, Keys.B);
						}
						_ = form.RichTextBox.NoteTextBox.Focus();
						Application.DoEvents();
						for (var j = 0; j < 5; j++)
						{
							KeySender.PostKeyDown(form.RichTextBox.NoteTextBox, Keys.B);
						}
						_ = form.Focus();
						Application.DoEvents();
					}
				}
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestUpdateControlLayoutIsBeingCalledInIsTextOnlyForBindingSetter()
		{
			TestPredefinedNoteTypes.Register();

			try
			{
				var master = Factory.New<DummyEnterpriseBusinessObject>();
				var note1 = master.GetNotes().AddNew();

				using (var form = new MockZForm(note1))
				{
					form.Show();
					form.RichTextBox.NoteTextBox.Text = "note text";

					note1.ST_Description = TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength100.Description;
					Application.DoEvents();
					AssertEquals("IsTextOnly", true, form.RichTextBox.IsTextOnly);
					AssertEquals("NoteTextBox.Visible", true, form.RichTextBox.NoteTextBox.Visible);
					AssertEquals("NoteTextBox.Enabled", true, form.RichTextBox.NoteTextBox.Enabled);
					AssertEquals("NoteRichTextBox.Visible", false, form.RichTextBox.NoteRichTextBox.Visible);
					AssertEquals("NoteRichTextBox.Enabled", false, form.RichTextBox.NoteRichTextBox.Enabled);
					note1.ST_Description = TestPredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes.Description;
					AssertEquals("IsTextOnly", false, form.RichTextBox.IsTextOnly);
					AssertEquals("NoteTextBox.Visible", false, form.RichTextBox.NoteTextBox.Visible);
					AssertEquals("NoteTextBox.Enabled", false, form.RichTextBox.NoteTextBox.Enabled);
					AssertEquals("NoteRichTextBox.Visible", true, form.RichTextBox.NoteRichTextBox.Visible);
					AssertEquals("NoteRichTextBox.Enabled", true, form.RichTextBox.NoteRichTextBox.Enabled);
					AssertEquals("UpdateControlLayout has been called", true, form.RichTextBox.updateControlLayoutCalled);

					form.RichTextBox.updateControlLayoutCalled = false;

					var note2 = master.GetNotes().AddNew();
					note2.ST_Description = TestPredefinedNoteTypes.Instance.AccountsPayableAccountManagementNotes.Description;
					form.RichTextBox.SetDataBinding(note2, "");
					AssertEquals("UpdateControlLayout has been called", true, form.RichTextBox.updateControlLayoutCalled);
				}
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestCopyPasteTextWithTabs_UserEntersNumberOfSpaces_ShouldReplaceTabs()
		{
			var master = Factory.New<DummyEnterpriseBusinessObject>();
			var note = master.GetNotes().AddNew();
			using (var form = new MockZForm(note))
			{
				form.Show();

				var notification = new Mock<IUserNotification>() { CallBase = true };
				form.RichTextBox.NoteTextBox.UserNotificationProvider = notification.Object;
				notification.Setup(n => n.QueryUserResponse(It.IsAny<UserResponseArgument>())).Returns("4");

				form.RichTextBox.NoteTextBox.Text = "starting text";
				form.RichTextBox.NoteTextBox.ProcessCmdKey(Keys.Control | Keys.V);

				form.RichTextBox.NoteTextBox.HandleTextAdded("starting text\t\tboo\tblah");
				AssertEquals("Should have replaced each tab with 4 spaces", "starting text        boo    blah", form.RichTextBox.NoteTextBox.Text);
			}
		}

		public void TestCopyPasteTextWithTabs_ShouldReplaceTabsAndTrimText()
		{
			TestPredefinedNoteTypes.Register();

			try
			{
				var master = Factory.New<DummyEnterpriseBusinessObject>();
				var note = master.GetNotes().AddNew();
				note.ST_Description = TestPredefinedNoteTypes.Instance.TextOnlyNoteWithMaxLength100.Description;
				using (var form = new MockZForm(master))
				{
					form.Show();

					var notification = new Mock<IUserNotification>() { CallBase = true };
					form.RichTextBox.NoteTextBox.UserNotificationProvider = notification.Object;
					notification.Setup(n => n.QueryUserResponse(It.IsAny<UserResponseArgument>())).Returns("4");

					form.RichTextBox.NoteTextBox.Text = "starting text";
					form.RichTextBox.NoteTextBox.ProcessCmdKey(Keys.Control | Keys.V);

					var newText = "\t" + new string('a', 99);
					form.RichTextBox.NoteTextBox.HandleTextAdded(newText);

					AssertEquals("Should have replaced tab with 4 spaces and trim to 100 characters ", new string(' ', 4) + new string('a', 96), form.RichTextBox.NoteTextBox.Text);
				}
			}
			finally
			{
				TestPredefinedNoteTypes.Unregister();
			}
		}

		public void TestCopyPasteTextWithTabs_UserPressesCancel_ShouldUndoPaste()
		{
			var master = Factory.New<DummyEnterpriseBusinessObject>();
			var note = master.GetNotes().AddNew();
			using (var form = new MockZForm(note))
			{
				form.Show();

				var notification = new Mock<IUserNotification>() { CallBase = true };
				form.RichTextBox.NoteTextBox.UserNotificationProvider = notification.Object;
				notification.Setup(n => n.QueryUserResponse(It.IsAny<UserResponseArgument>())).Returns(value: null);

				form.RichTextBox.NoteTextBox.Text = "starting text";
				form.RichTextBox.NoteTextBox.ProcessCmdKey(Keys.Control | Keys.V);

				form.RichTextBox.NoteTextBox.HandleTextAdded("starting text\t\tboo\tblah");
				AssertEquals("Should have undone paste operation", "starting text", form.RichTextBox.NoteTextBox.Text);
			}
		}

		public void TestRichTextBoxClearedOnNullBinding()
		{
			const string rtfString = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1 
\pard\f0\fs20 this candidate is great\par
}
";
			const string actualText = "this candidate is great";

			var master = Factory.New<DummyEnterpriseBusinessObject>();
			var note = master.GetNotes().AddNew();

			note.ST_NoteData = CargoWise.Types.ZBlob.FromUTF8(rtfString);
			Factory.Save();

			using (var form = new MockZForm(note))
			{
				var rtb = new ZStmNoteRichTextBox();
				rtb.Parent = form;
				form.Show();

				CombineAssertions(() =>
				{
					rtb.SetDataBinding(note, string.Empty);
					Application.DoEvents();
#if !WINZOR
					AssertContains("RTB should have its RTF field contain the note data when bound to a note", actualText, rtb.NoteRichTextBox.Rtf);
#else
					AssertContains("RTB should have its HTML field contain the note data when bound to a note", actualText, rtb.NoteRichTextBox.Html);
#endif
					rtb.SetDataBinding(null, string.Empty);
					Application.DoEvents();
#if !WINZOR
					AssertNotContains("RTB should have its RTF field empty when bound to null", actualText, rtb.NoteRichTextBox.Rtf);
#else
					AssertNotContains("RTB should have its HTML field empty when bound to null", actualText, rtb.NoteRichTextBox.Html);
#endif
				});
			}
		}

		ZStmNoteRichTextBox.InternalZTextBox GetTextBoxControl(ZStmNoteRichTextBox box)
		{
			foreach (Control control in box.Controls)
			{
				if (control is ZStmNoteRichTextBox.InternalZTextBox)
				{
					return (ZStmNoteRichTextBox.InternalZTextBox)control;
				}
			}
			Fail("Could not find the InternalZTextBox in the ZStmNoteRichTextBoxForTest control");
			return null;
		}

		#region class MockZForm

		class MockZForm : ZForm
		{
			public MockZForm(StmNote businessEntity)
				: base(businessEntity)
			{
				RichTextBox = new ZStmNoteRichTextBox
				{
					BindToRtfNote = StmNoteSchema.Constants.ST_NoteData,
					BindToTextNote = StmNoteSchema.Constants.ST_NoteText,
					IsModifyButtonVisible = true
				};
				Controls.Add(RichTextBox);
				RichTextBox.SetDataBinding(businessEntity, "");
			}

			public MockZForm(DummyEnterpriseBusinessObject dummy)
				: base(dummy)
			{
				TextBox = new ZTextBox
				{
					TabIndex = 0
				};
				RichTextBox = new ZStmNoteRichTextBox
				{
					BindToRtfNote = StmNoteSchema.Constants.ST_NoteData,
					BindToTextNote = StmNoteSchema.Constants.ST_NoteText,
					IsModifyButtonVisible = true,
					TabIndex = 1
				};
				Controls.Add(TextBox);
				Controls.Add(RichTextBox);
				RichTextBox.SetBindingMember(ZStmNoteUserControl.BindToNotes);
			}

			public ZTextBox TextBox { get; }

			public ZStmNoteRichTextBox RichTextBox { get; }
		}

		#endregion

		#region class DummyStmNote
		class DummyStmNote : StmNote
		{
			public DummyStmNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
				SecondObject = factory.New<DummyEnterpriseBusinessObject>();
			}

			public IStmNoteParent SecondObject;

			public override ICollection<IStmNoteParent> OverrideValidationMasters
			{
				get
				{
					return new List<IStmNoteParent>()
					{
						Master,
						SecondObject
					};
				}
			}
		}

		#endregion
	}
}
