using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Interop.DataObjects;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.GUI.ZRichTextBox;
using Color = System.Drawing.Color;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	class ZRichTextBoxTest : TestCaseWithFactory
	{
		internal class MyRichTextBoxForTest : MyRichTextBox
		{
			public void FireOnLinkClicked(LinkClickedEventArgs e) => OnLinkClicked(e);
		}

		public void TestUnsafeLinksInZRichTextBox()
		{
			var unsafeHyperLink = "file://somefile";

			AssertLinkClick(unsafeHyperLink, ZDialogResult.OK);
			AssertLinkClick(unsafeHyperLink, ZDialogResult.Cancel);
			AssertLinkClick(unsafeHyperLink, ZDialogResult.None);

			ErrorReporter.Clear();
		}

		void AssertLinkClick(string hyperLink, ZDialogResult userAnswer)
		{
			using (RawDataRegistry.Instance.ShowWarningPopupBeforeLaunchingURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new MyRichTextBoxForTest())
			{
				UnitTestUserNotification.Instance.AddAnswer(userAnswer);

				control.FireOnLinkClicked(new LinkClickedEventArgs(hyperLink));

				CombineAssertions(() =>
				{
					AssertEquals(string.Format(@"The following link leads to an external file and may be potentially unsafe:

{0}

Are you sure to sure to open the file?", hyperLink), UnitTestUserNotification.Instance.LastMessage.Text);

					if (userAnswer == ZDialogResult.OK)
					{
						AssertEquals("The WebUrlLauncher should launch the appropriate", hyperLink, WebUrlLauncher.LastUrlLaunched);
					}
					else
					{
						AssertEquals(string.Empty, WebUrlLauncher.LastUrlLaunched);
					}
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		public void TestClickRichTextBoxShouldNotTriggerNoteDataChanges()
		{
			var rtf = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\*\generator Riched20 6.3.9600}\viewkind4\uc1
\pard\f0\fs20 123456\par
}
";

			var rtfAnotherVersion = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\*\generator Riched21 6.3.9600}\viewkind4\uc1
\pard\f0\fs20 123456\par
}
";

			var dummyBiz = Factory.NewWithValidTestData<DummyBizOWithRelatedNotes>();
			var note = dummyBiz.Notes.AddNew();
			note.ST_ParentID = ZGuid.NewZGuid();
			note.ST_Description = "test note";
			note.ST_NoteData = ZBlob.FromAscii(rtf);
			AssertEquals("123456", note.ST_NoteDataAsText);
			Factory.Save();

			using (var form = new ZForm(dummyBiz))
			using (var richTextBox = new TestRichTextBox())
			using (var textBox = new ZTextBox())
			{
				note.ST_NoteType_DescriptiveText = StmNoteDescription.PubDescriptive;
				textBox.SetDataBinding(note, "ST_NoteType_DescriptiveText");
				richTextBox.SetDataBinding(note, AutoStmNote.Schema.ST_NoteData);
				form.Controls.Add(richTextBox);
				form.Controls.Add(textBox);
				form.Show();
				Application.DoEvents();

				AssertEquals("123456", richTextBox.RichEdit.Text);
				textBox.Focus();
				AssertEquals(StmNoteDescription.PubDescriptive, textBox.Text);
				Rectangle bounds;
				Point location;
				bounds = richTextBox.Bounds;
				location = bounds.Location + ControlDpiScalingHelper.NewScaledSize(bounds.Width / 2, bounds.Height / 2, false);

				MouseSender.PostMessage(richTextBox.RichEdit, richTextBox.RichEdit.Handle, WindowsMessage.WM_LBUTTONDOWN, IntPtr.Zero, new IntPtr(MakeLParam(location.X, location.Y)));
				Application.DoEvents();
				textBox.Focus();

				Assert(!note.ST_NoteDataInfo.HasChanges);

				richTextBox.RichEdit.Text = "66666";
				textBox.Focus();
				AssertEquals(StmNoteDescription.PubDescriptive, textBox.Text);

				bounds = richTextBox.Bounds;
				location = bounds.Location + ControlDpiScalingHelper.NewScaledSize(bounds.Width / 2, bounds.Height / 2, false);
				MouseSender.PostMessage(richTextBox.RichEdit, richTextBox.RichEdit.Handle, WindowsMessage.WM_LBUTTONDOWN, IntPtr.Zero, new IntPtr(MakeLParam(location.X, location.Y)));
				Application.DoEvents();
				textBox.Focus();
				Assert(note.ST_NoteDataInfo.HasChanges);
			}

			var noteAnotherVersion = dummyBiz.Notes.AddNew();
			noteAnotherVersion.ST_ParentID = ZGuid.NewZGuid();
			noteAnotherVersion.ST_Description = "test note 2";
			noteAnotherVersion.ST_NoteData = ZBlob.FromAscii(rtfAnotherVersion);
			AssertEquals("123456", noteAnotherVersion.ST_NoteDataAsText);
			Factory.Save();

			using (var form = new ZForm(dummyBiz))
			using (var richTextBox = new TestRichTextBox())
			using (var textBox = new ZTextBox())
			{
				textBox.SetDataBinding(noteAnotherVersion, "ST_NoteType_DescriptiveText");
				richTextBox.SetDataBinding(noteAnotherVersion, AutoStmNote.Schema.ST_NoteData);
				form.Controls.Add(richTextBox);
				form.Controls.Add(textBox);
				form.Show();
				Application.DoEvents();

				AssertEquals("123456", richTextBox.RichEdit.Text);
				textBox.Focus();
				textBox.Text = StmNoteDescription.PubDescriptive;
				Rectangle bounds;
				Point location;
				bounds = richTextBox.Bounds;
				location = bounds.Location + ControlDpiScalingHelper.NewScaledSize(bounds.Width / 2, bounds.Height / 2, false);
				MouseSender.PostMessage(richTextBox.RichEdit, richTextBox.RichEdit.Handle, WindowsMessage.WM_LBUTTONDOWN, IntPtr.Zero, new IntPtr(MakeLParam(location.X, location.Y)));
				Application.DoEvents();
				textBox.Focus();

				Assert(!noteAnotherVersion.ST_NoteDataInfo.HasChanges);
			}
		}
		int MakeLParam(int loWord, int hiWord)
		{
			return ((hiWord << 16) | (loWord & 0xffff));
		}

		public void TestBindAndUnbind()
		{
			using (var form = new ZForm())
			{
				var note = Factory.New<StmNote>();
				RichEdit.Parent = form;
				form.Show();

				var dataBoundControl = RichEdit;
				dataBoundControl.SetDataBinding(note, AutoStmNote.Schema.ST_NoteData);

				note.ST_NoteData = ZBlob.FromUTF8("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Tahoma;}}testing123testing\r\n\\viewkind4\\uc1\\pard\\f0\\fs17\\par\r\n}\r\n\0");
#if !WINZOR
				Assert("Should contain this text", RichEdit.Rtf.IndexOf("testing123testing") != -1);
#else
				Assert("Should contain this text", RichEdit.Html.IndexOf("testing123testing") != -1);
#endif

				dataBoundControl.SetDataBinding(null, "");
				note.ST_NoteData = ZBlob.FromUTF8("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Tahoma;}}somerandomstuff\r\n\\viewkind4\\uc1\\pard\\f0\\fs17\\par\r\n}\r\n\0");
#if !WINZOR
				Assert("Should still contain this text", RichEdit.Rtf.IndexOf("testing123testing") != -1);
#else
				Assert("Should still contain this text", RichEdit.Html.IndexOf("testing123testing") != -1);
#endif
			}
		}

		[ExpectNoExceptions]
		public void TestAllowSetByteArrayToNull()
		{
			using (var control = new ZRichTextBox())
			{
#if !WINZOR
				control.RtfZBlob = null;
#else
				control.HtmlZBlob = null;
#endif
			}
		}

		[DoNotAllowNotificationDuringTransaction]
		[ExpectNoExceptions]
		public void TestNoErrorMessageDuringDbTransaction()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummyBizO))
			{
				var note = Factory.New<StmNote>();
				RichEdit.SetDataBinding(note, AutoStmNote.Schema.ST_NoteData);
				note.Master = Factory.New<DummyBizOWithAutoLogs>();
				note.ST_Table = "tablename";
				note.ST_NoteData = new byte[20];
				note.ST_Description = "Container Release Note";
				note.ST_NoteText = @"{{{{{{{{\rtf1\ansi\ansicpg1252\deff0\nouicompat{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif; } }
								{\*\generator Riched20 10.0.14393}\viewkind4\uc1
\pard\f0\fs20\lang3081 text\par
};";
				RichEdit.skipForTest = true;
				RichEdit.Parent = form;

				AssertNoExceptionThrown(() => form.FireSaveButton());
			}
		}

		[ExpectNoExceptions]
		public void TestAllowSetByteArrayWhileDisposed()
		{
			var control = new ZRichTextBox();
			control.Dispose();
#if !WINZOR
			control.RtfZBlob = Array.Empty<byte>();
#else
			control.HtmlZBlob = Array.Empty<byte>();
#endif
		}

		public void TestReadOnly()
		{
			AssertEquals(false, RichEdit.ReadOnly);
			RichEdit.ReadOnly = false;
			AssertEquals(false, RichEdit.ReadOnly);
			RichEdit.ReadOnly = true;
			AssertEquals(true, RichEdit.ReadOnly);
			RichEdit.ReadOnly = false;
			AssertEquals(false, RichEdit.ReadOnly);
		}

		public void TestColors()
		{
			using (var form = new ZForm())
			{
				var richEdit2 = new TestRichTextBox();
				RichEdit.Parent = form;
				richEdit2.Parent = form;
				form.Show();
				RichEdit.Focus();
				AssertEquals("Must be colored in selected color", EnterpriseFormLookStrategy.SelectedControlColor, RichEdit.RichEdit.BackColor);
				AssertEquals("Must be colored in standard color", SystemColors.Window, richEdit2.RichEdit.BackColor);

				richEdit2.Focus();
				AssertEquals("Must be colored in standard color", SystemColors.Window, RichEdit.RichEdit.BackColor);
				AssertEquals("Must be colored in selected color", EnterpriseFormLookStrategy.SelectedControlColor, richEdit2.RichEdit.BackColor);
			}
		}

		public void TestPerformTab()
		{
			// ShiftTabOutOfRichTextBox handles this for Winzor
			using (var form = new ZForm())
			{
				var box1 = new TestRichTextBox();
				var box2 = new TestRichTextBox();
				var boxContainer1 = new UserControl();
				var boxContainer2 = new UserControl();
				boxContainer1.Controls.Add(box1);
				boxContainer2.Controls.Add(box2);
				form.Controls.Add(boxContainer1);
				form.Controls.Add(boxContainer2);
				form.Show();
				Application.DoEvents();

				box1.Focus();
				AssertEquals("Starting focus", true, box1.RichEdit.Focused);

				box1.PerformTab(true);
				Application.DoEvents();
				AssertEquals("Forward tab A", false, box1.RichEdit.Focused);
#if !WINZOR
				AssertEquals("Forward tab B", true, box2.RichEdit.Focused);
#endif

				box2.PerformTab(false);
				Application.DoEvents();
#if !WINZOR
				AssertEquals("Backward tab A", true, box1.RichEdit.Focused);
#endif
				AssertEquals("Backward tab B", false, box2.RichEdit.Focused);
			}
		}

		public void TestEDocsPlugIn()
		{
			var richEdit = new TestRichTextBox();
			richEdit.CreateControl();

			using (var form = new ZForm())
			{
				AssertEquals(0, form.PlugIns.Instances.Length);
				form.PlugIns.Add(ControllerIDs.eDocsPlugIn);

				AssertEquals(1, form.PlugIns.Instances.Length);
				var plugIn = form.PlugIns.Instances[0];

				form.Controls.Add(richEdit);
				Application.DoEvents();

				using (var tempFile = TempFile.New())
				using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { tempFile.Filename }))
				{
					richEdit.InsertObject(data);
					Application.DoEvents();
					AssertEquals(plugIn, richEdit.EDocsPlugIn);
				}
			}
		}

		public void TestInsertOleObjectWhenReadOnly()
		{
			Application.DoEvents();
#if !WINZOR
			var beforeRtf = RichEdit.Rtf;
#else
			var beforeRtf = RichEdit.Html;
#endif

			RichEdit.ReadOnly = true;
			RichEdit.InsertObject(SystemIcons.Error.ToBitmap());
			Application.DoEvents();
			Application.DoEvents();
#if !WINZOR
			Assert("Rtf should NOT change as the control is ReadOnly", RichEdit.Rtf == beforeRtf);
#else
			Assert("Rtf should NOT change as the control is ReadOnly", RichEdit.Html == beforeRtf);
#endif
		}

#if !WINZOR
		public void TestNonRTF_NoExceptionThrown()
		{
			ZRichTextBox boxForRtf = new TestRichTextBox();
			using (var form = new ZForm())
			{
				form.Controls.Add(boxForRtf);
				var binaryData = "";
				for (var i = 0; i < 255; ++i)
				{
					binaryData += new string((char)i, 1);
				}
				var initial = boxForRtf.Rtf;
				boxForRtf.Rtf = binaryData;
				form.Show();
				Application.DoEvents();
				AssertNoExceptionThrown("Make sure it s not throwing an Exception", () => boxForRtf.RichEdit.Text.Any());
				Assert("Rtf field is some non-blank value", boxForRtf.Rtf.Trim().Contains(@"'00\'01\'02\'03\'04\'05\'06 \'08\tab\par
\line\page\par
\'0e\'0f\'10\'11\'12\'13\'14\'15\'16\'17\'18\'19\'1a\'1b\'1c\'1d\'1e\'1f !""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz\{|\}~"));
			}
		}

		public void TestPasteTextOnly()
		{
			using (var boxForRtf = new TestRichTextBox())
			{
				boxForRtf.Rtf =
@"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fmodern\fcharset0 Courier New;}{\f1\fnil\fcharset0 Microsoft Sans Serif;}}
\viewkind4\uc1\pard\protect\f0\fs20 Terri\protect0\f1\fs17\par
}";

				boxForRtf.SelectAll();
				boxForRtf.Copy();

				using (var form = new ZForm())
				{
					var box = new TestRichTextBox();
					form.Controls.Add(box);
					form.Show();
					Application.DoEvents();

					box.PasteTextOnly();
					box.SelectAll();
					AssertFontEquals("Font should not be changed after paste", OFont.GetRichTextBoxFont(), box.ActiveRichTextFont);
				}
			}
		}

		public void TestPasteProtectedText_Rtf()
		{
			var sourceRtf = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033
{\colortbl ;\red0\green0\blue255;}
\viewkind4\uc1\pard\sb100\sa100\lang3081\protect\f0\fs24ProtectMessage\protect0
}";

			using (var box = new TestRichTextBox())
			{
				SafeClipboard.SetData(DataFormats.Rtf, sourceRtf);
				box.Paste();

				AssertNotContains("\"\\protect\" and \"\\protect0\" in Rtf will be removed.", @"\protect", box.Rtf);
			}
		}

		[DeveloperOnlyTest]
		public void TestPasteProtectedText_Text()
		{
			SafeClipboard.Clear();
			var sourceRtf = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033
{\colortbl ;\red0\green0\blue255;}
\viewkind4\uc1\pard\sb100\sa100\lang3081\protect\f0\fs24ProtectMessage\\protect\protect0
}";

			using (var box = new TestRichTextBox())
			{
				var action = new Action(() => SafeClipboard.SetData(DataFormats.Rtf, sourceRtf));
				action.Invoke();
				ClipboardTestHelper.RetryIfCopyOrCutFailed<IDataObject>(action, retry: 20);

				box.Paste();
				Application.DoEvents();

				AssertEquals("\"\\protect\" in text will not be removed.", @"ProtectMessage\protect", ORtfTextUtil.RtfToText(box.Rtf));
			}
			SafeClipboard.Clear();
		}

		[ExpectNoExceptions]
		public void TestPasteNull()
		{
			using (var box = new TestRichTextBox())
			{
				box.CreateControl();
				box.ReturnNullClipboard = true;
				box.RichEdit.Text = "";
				SafeClipboard.SetDataObject("splaty");
				box.Paste();
				AssertEquals("Nothing should be pasted", "", box.RichEdit.Text);
			}
		}

		public void TestPasteFromDataWithMaximumLimitSizeInMB()
		{
			using (SystemDataRegistry.Instance.eDocsMaximumFilesize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (var form = new ZForm())
			using (var richTextBox = new RichTextBoxForPasteTest())
			{
				form.DataObjectPasted += (object sender, DataObjectPastedEventArgs e) =>
				{
					if (e.DataToPaste.GetDataPresent(DataFormats.FileDrop))
					{
						var fileNames = (string[])e.DataToPaste.GetData(DataFormats.FileDrop);
						foreach (var fileName in fileNames)
						{
							e.AddPastedFile(fileName, Path.GetFileName(fileName), string.Empty, Guid.NewGuid(), Guid.NewGuid(), true);
						}
					}
				};

				form.PlugIns.Add(ControllerIDs.eDocsPlugIn);
				form.Controls.Add(richTextBox);
				form.Show();
				Application.DoEvents();

				using (var tempBigFile = TempFile.New())
				using (var tempNormalFile = TempFile.New())
				{
					CreateFileWithFileSizeMoreThanLimitSize(tempBigFile.Filename, 1 * 1024 * 1024);
					CreateFileWithFileSizeMoreThanLimitSize(tempNormalFile.Filename, 10);

					var dataObject = new DataObject(DataFormats.FileDrop, new[] { tempBigFile.Filename, tempNormalFile.Filename });
					richTextBox.ClipboardDataObject = dataObject;

					richTextBox.Paste();
					var bigFileName = Path.GetFileName(tempBigFile.Filename);
					var normalFileName = Path.GetFileName(tempNormalFile.Filename);
					var expectedMessage = $@"The following files are larger than the maximum file size (1MB) specified in the registry 'System -> DocManager -> eDocs Maximum File Size'
{bigFileName}";
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("Should have normal file", normalFileName, richTextBox.Rtf);
					AssertNotContains("Should not have big file", bigFileName, richTextBox.Rtf);
				}
			}

			void CreateFileWithFileSizeMoreThanLimitSize(string fileName, int limitSize)
			{
				using (var stream = new FileStream(fileName, FileMode.Create))
				using (var writer = new StreamWriter(stream))
				{
					while (stream.Position < limitSize)
					{
						writer.WriteLine("Create a file for test");
					}
				}
			}
		}

		public void TestDefaultFont()
		{
			using (var box = new TestRichTextBox())
			{
				AssertFontEquals("Default font should be set", OFont.GetRichTextBoxFont(), box.ActiveRichTextFont);

				box.CreateControl();
				AssertFontEquals("Default font should be set", OFont.GetRichTextBoxFont(), box.ActiveRichTextFont);

				box.Rtf = @"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl{\f0\fnil Microsoft Sans Serif;}}{\colortbl}{\f0\fs20\par}}";
				AssertFontEquals("Default font should be set", OFont.GetRichTextBoxFont(), box.ActiveRichTextFont);
			}
		}
#endif

		public void TestGarbageCollected()
		{
			var boxRef = GetWeakReferenceAfterCreatingRichTextBoxAndStuff();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			AssertEquals("ZRichTextBox should be garbage collected", false, boxRef.IsAlive);
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		WeakReference GetWeakReferenceAfterCreatingRichTextBoxAndStuff()
		{
			WeakReference boxRef;
			TestRichTextBox box;

			using (var form = new ZForm())
			using (box = new TestRichTextBox())
			{
				form.Controls.Add(box);
				boxRef = new WeakReference(box);
				box.CreateControl();
#if !WINZOR
				var beforeRtf = box.Rtf;
#else
				var beforeRtf = box.SelectedHtml;
#endif

				box.InsertObject(new DataObject("some text"));
#if !WINZOR
				Assert("Box Contents should have changed if data is inserted", box.Rtf != beforeRtf);
#else
				Assert("Box Contents should have changed if data is inserted", box.SelectedHtml != beforeRtf);
#endif
			}

			return boxRef;
		}

		public void TestPopupEventCalledOnPopup()
		{
			using (var form = new ZChildForm())
			using (var box = new ZRichTextBox())
			{
				form.Controls.Add(box);
				form.Show();

				var wasCalled = false;
				box.BeforePopup += (o, e) => wasCalled = true;
				box.ShowPopupEditor();

				Assert("The Popup event should be called when a popup occurs", wasCalled);
			}
		}

		public void TestShowPopupEditorNonModalShouldOnlyAllowIfCascadingReadOnly()
		{
			using (var form = new ZChildForm())
			using (var box = new ZRichTextBox())
			{
				form.Controls.Add(box);
				form.Show();

				ErrorReporter.Clear();
				AssertEquals("Precondition: Should not be readonly by default", false, box.ReadOnly);
				AssertEquals("Precondition: Should not cascade readonly by default", false, box.ReadOnlyCascadeToPopup);
				box.ShowPopupEditorNonModal();
				AssertEquals("Should report error since box is writable", $"The popup form should only ever be shown non-modally if the rich text box and its popup are readonly. Otherwise you should be using {nameof(ZRichTextBox.ShowPopupEditor)}", ErrorReporter.LastMessageReported);
				AssertEquals("Should not open popup form", 0, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());

				ErrorReporter.Clear();
				box.ReadOnly = true;
				AssertEquals("Precondition: Should not cascade readonly by default", false, box.ReadOnlyCascadeToPopup);
				box.ShowPopupEditorNonModal();
				AssertEquals("Should report error since box popup is not cascading box's readonly status", $"The popup form should only ever be shown non-modally if the rich text box and its popup are readonly. Otherwise you should be using {nameof(ZRichTextBox.ShowPopupEditor)}", ErrorReporter.LastMessageReported);
				AssertEquals("Should not open popup form", 0, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());

				ErrorReporter.Clear();
				box.ReadOnly = false;
				box.ReadOnlyCascadeToPopup = true;
				box.ShowPopupEditorNonModal();
				AssertEquals("Should report error since box is writable", $"The popup form should only ever be shown non-modally if the rich text box and its popup are readonly. Otherwise you should be using {nameof(ZRichTextBox.ShowPopupEditor)}", ErrorReporter.LastMessageReported);
				AssertEquals("Should not open popup form", 0, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());

				ErrorReporter.Clear();
				box.ReadOnly = true;
				box.ShowPopupEditorNonModal();
				AssertEquals("Should not report error since box is readonly and has cascaded its readonly status", true, string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
				AssertEquals("Should open popup form", 1, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());
				Application.OpenForms.OfType<ZRichTextBoxPopupForm>().First().Close();
				form.Close();
			}
		}

		public void TestShowPopupEditorNonModalShouldNotOpenAnotherFormIfCalledMoreThanOnce()
		{
			using (var form = new ZChildForm())
			using (var box = new ZRichTextBox())
			{
				form.Controls.Add(box);
				form.Show();

				box.ReadOnlyCascadeToPopup = true;
				box.ReadOnly = true;
				box.ShowPopupEditorNonModal();
				AssertEquals("Precondition: Should open popup form", 1, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());
				var openPopupForm = Application.OpenForms.OfType<ZRichTextBoxPopupForm>().First();

				box.ShowPopupEditorNonModal();
				AssertEquals("Should not open another popup form", 1, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());
				form.Close();
			}
		}

		public void TestShowPopupEditorNonModalShouldClosePopupIfFormClosed()
		{
			using (var form = new ZChildForm())
			using (var box = new ZRichTextBox())
			{
				form.Controls.Add(box);
				form.Show();

				box.ReadOnlyCascadeToPopup = true;
				box.ReadOnly = true;
				box.ShowPopupEditorNonModal();
				AssertEquals("Precondition: Should open popup form", 1, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());
				form.Close();
			}

			AssertEquals("Closing the form should close the popup form", 0, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());
		}

		public void TestShowPopupEditorNonModalShouldClosePopupIfTextChanges()
		{
			using (var form = new ZChildForm())
			using (var box = new ZRichTextBox())
			{
				form.Controls.Add(box);
				form.Show();

				box.ReadOnlyCascadeToPopup = true;
				box.ReadOnly = true;
				box.ShowPopupEditorNonModal();
				AssertEquals("Precondition: Should open popup form", 1, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());

#if !WINZOR
				box.RtfZBlob = new ZBlob(Encoding.UTF8.GetBytes("This is test data."));
#else
				box.HtmlZBlob = new ZBlob(Encoding.UTF8.GetBytes("This is test data."));
#endif
				AssertEquals("Changing the text should close the popup form", 0, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());
			}
		}

		public void TestShowPopupEditorNonModalCaption()
		{
			using (var form = new ZChildForm())
			using (var box = new ZRichTextBox())
			{
				form.Controls.Add(box);
				form.Show();

				box.ReadOnlyCascadeToPopup = true;
				box.ReadOnly = true;
				var caption = Res.GetData("93e09572-c311-4839-8f33-0558af993406", "hello");
				box.PopupFormCaption = caption;

				box.ShowPopupEditorNonModal();
				AssertEquals("Precondition: Should open popup form", 1, Application.OpenForms.OfType<ZRichTextBoxPopupForm>().Count());
				var openPopupForm = Application.OpenForms.OfType<ZRichTextBoxPopupForm>().First();
				AssertEquals("Should get caption from textbox property", caption, openPopupForm.CaptionResourceString);
				form.Close();
			}
		}

		public void TestWrapDataAndSendToParentFormUsesBindingObject()
		{
			using (var file = TempFile.New())
			using (var form = new ZForm())
			using (var box = new TestRichTextBox())
			using (var data = ZDataObject.FromData(DataFormats.FileDrop, new string[] { file.Filename }))
			{
#if !WINZOR
				var rtfBefore = box.RichEdit.Rtf;
#else
				var rtfBefore = box.RichEdit.Html;
#endif
				box.Parent = form;
				form.Controls.Add(box);
				box.CreateControl();
				form.Show();
				Application.DoEvents();

				var note = Factory.New<StmNote>();
				box.SetDataBinding(note, AutoStmNote.Schema.ST_NoteData);

				box.WrapDataAndSendToParentForm(data);
				AssertEquals("Data should have parentid added", note.PK.ToGuid(), data.ParentIDLink);
				AssertEquals("Data should have parent table prefix added", StmNoteSchema.Constants.Prefix, data.ParentTableLink);
			}
		}

#if !WINZOR
		public void TestDefaultValue()
		{
			using (var form = new ZChildForm())
			using (var box = new ZRichTextBox())
			{
				form.Controls.Add(box);
				form.Show();

				AssertEquals("Default SelectionColor should be Black", Color.Black, box.SelectionColor);
				AssertEquals("Default SelectedText should be Empty", string.Empty, box.SelectedText);
				AssertEquals("Default SelectedRtf should be Empty", string.Empty, box.SelectedRtf);
				AssertEquals("Default SelectionStart should be 0", 0, box.SelectionStart);
				AssertEquals("Default SelectionLength should be 0", 0, box.SelectionLength);
				AssertEquals("Default SelectionBullet should be false", false, box.SelectionBullet);
				AssertEquals("Default SelectionIndent should be 0", 0, box.SelectionIndent);
				AssertEquals("Default SelectionNumberedList should be false", false, box.SelectionNumberedList);
				AssertEquals("Default SelectionProtected should be false", false, box.SelectionProtected);
			}
		}

		public void TestHotkeys()
		{
			using (var form = new ZForm())
			using (var box = new TestRichTextBox())
			{
				form.Controls.Add(box);
				form.Show();

				KeySender.SendKeyDownToProcessCmdKey(box.RichEdit, (int)(Keys.Control | Keys.T));
				Application.DoEvents();
				Assert("Strikeout should be active", box.ActiveRichTextFont.Strikeout);
				KeySender.SendKeyDownToProcessCmdKey(box.RichEdit, (int)(Keys.Control | Keys.T));
				Application.DoEvents();
				Assert("Strikeout should not be active anymore", !box.ActiveRichTextFont.Strikeout);

				KeySender.SendKeyDownToProcessCmdKey(box.RichEdit, (int)(Keys.Control | Keys.I));
				Application.DoEvents();
				Assert("Italic should be active", box.ActiveRichTextFont.Italic);
				KeySender.SendKeyDownToProcessCmdKey(box.RichEdit, (int)(Keys.Control | Keys.I));
				Application.DoEvents();
				Assert("Italic should not be active anymore", !box.ActiveRichTextFont.Italic);

				KeySender.SendKeyDownToProcessCmdKey(box.RichEdit, (int)(Keys.Control | Keys.B));
				Application.DoEvents();
				Assert("Bold should be active", box.ActiveRichTextFont.Bold);
				KeySender.SendKeyDownToProcessCmdKey(box.RichEdit, (int)(Keys.Control | Keys.B));
				Application.DoEvents();
				Assert("Bold should not be active anymore", !box.ActiveRichTextFont.Bold);

				KeySender.SendKeyDownToProcessCmdKey(box.RichEdit, (int)(Keys.Control | Keys.U));
				Application.DoEvents();
				Assert("Underline should be active", box.ActiveRichTextFont.Underline);
				KeySender.SendKeyDownToProcessCmdKey(box.RichEdit, (int)(Keys.Control | Keys.U));
				Application.DoEvents();
				Assert("Underline should not be active anymore", !box.ActiveRichTextFont.Underline);
			}
		}

		public void TestPasteInvalidImageZRichTextBox()
		{
			const string rtfWithInvalidImage = @"{\rtf{\pict{\nonshppict 89504e470d0a1a0a0000000d4948445200000008000000080806000000c}}}";

			using (var testRichTextBox = new TestRichTextBox())
			{
				SafeClipboard.SetData(DataFormats.Rtf, rtfWithInvalidImage);
				RichEdit.Paste();
				Assert("Invalid image should not be added to rtf text.", !testRichTextBox.RichEdit.Rtf.Contains("added to eDocs tab."));
			}
		}

		public void TestIsAttachButtonVisible()
		{
			using (var zForm = new ZForm())
			using (var zRichTextBox = new ZRichTextBox())
			{
				zForm.Controls.Add(zRichTextBox);
				zForm.Show();
				Application.DoEvents();

				Assert(zRichTextBox.RichTextToolBar.AttachButton.Visible);

				zRichTextBox.IsAttachButtonVisible = false;
				Assert(!zRichTextBox.RichTextToolBar.AttachButton.Visible);

				zRichTextBox.IsAttachButtonVisible = true;
				Assert(zRichTextBox.RichTextToolBar.AttachButton.Visible);
			}
		}

		public void TestIsInsertImageButtonVisible()
		{
			using (var zForm = new ZForm())
			using (var zRichTextBox = new ZRichTextBox())
			{
				zForm.Controls.Add(zRichTextBox);
				zForm.Show();
				Application.DoEvents();

				Assert(zRichTextBox.RichTextToolBar.InsertImageButton.Visible);

				zRichTextBox.IsInsertImageButtonVisible = false;
				Assert(!zRichTextBox.RichTextToolBar.InsertImageButton.Visible);

				zRichTextBox.IsInsertImageButtonVisible = true;
				Assert(zRichTextBox.RichTextToolBar.InsertImageButton.Visible);
			}
		}
#else
		public void TestHtmlBinding()
		{
			using (var form = new ZForm())
			{
				var note = Factory.New<StmNote>();
				var richEdit = new ZRichTextBox() { Font = new Font("Tahoma", 20, GraphicsUnit.Point) };
				richEdit.CreateControl();
				richEdit.Parent = form;
				form.Show();

				var dataBoundControl = richEdit;
				dataBoundControl.SetDataBinding(note, AutoStmNote.Schema.ST_NoteData);

				AssertEquals(ZBlob.Empty, note.ST_NoteData);
				AssertEquals(ZBlob.Empty, note.ST_NoteData_HTML);

				note.ST_NoteData = ZBlob.FromUTF8(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicomp\uc0{\fonttbl{\f0\fnil Tahoma;}}{\colortbl}{{testing123 }{\f0\fs17 testing}\par}}");
				AssertEquals(@"<p>testing123 <span style=""font-family: Tahoma, sans-serif; font-size: 8.5pt;"">testing</span></p>", note.ST_NoteData_HTML.ToUTF8());
				AssertEquals(@"<p><span style=""font-family: Tahoma, sans-serif; font-size: 20pt;"">testing123 </span><span style=""font-family: Tahoma, sans-serif; font-size: 8.5pt;"">testing</span></p>", richEdit.Html);
			}
		}

		public void TestPlainTextHtmlBinding()
		{
			using (var form = new ZForm())
			{
				var note = Factory.New<StmNote>();
				var richEdit = new ZRichTextBox() { Font = new Font("Tahoma", 20, GraphicsUnit.Pixel) };
				richEdit.CreateControl();
				richEdit.Parent = form;
				form.Show();

				var dataBoundControl = richEdit;
				dataBoundControl.SetDataBinding(note, AutoStmNote.Schema.ST_NoteData);

				AssertEquals(ZBlob.Empty, note.ST_NoteData);
				AssertEquals(ZBlob.Empty, note.ST_NoteData_HTML);

				var datAmnestyFailure = new
				{
					E8_AssemblyName = "winzor.GUI.Test.dll",
					E2_TestClass = "vstest:System.Windows.Forms.ControlTest",
					E6_MethodName = "InvokeRenderDispatcherWithExceptionHandledAfterWinzorDispatcherDisposed",
					E6_PK = "http://crikey.wtg.zone/failures/testFailureHistory/2b2fbd80-8147-40b5-aad8-c54d3463f9cd"
				};
				var description = string.Format(System.Globalization.CultureInfo.InvariantCulture, "Assembly: {0}\r\nClass: {1}\r\nMethod: {2}\r\n", datAmnestyFailure.E8_AssemblyName, datAmnestyFailure.E2_TestClass, datAmnestyFailure.E6_MethodName)
					+ datAmnestyFailure.E6_PK
					+ "\r\n\r\n";

				note.ST_NoteData = ZBlob.FromUTF8(description);
				AssertEquals("<p>Assembly: winzor.GUI.Test.dll</p><p>Class: vstest:System.Windows.Forms.ControlTest</p><p>Method: InvokeRenderDispatcherWithExceptionHandledAfterWinzorDispatcherDisposed</p><p><a href=\"http://crikey.wtg.zone/failures/testFailureHistory/2b2fbd80-8147-40b5-aad8-c54d3463f9cd\" target=\"_blank\">http://crikey.wtg.zone/failures/testFailureHistory/2b2fbd80-8147-40b5-aad8-c54d3463f9cd</a></p><p></p><p></p>", note.ST_NoteData_HTML.ToUTF8());
				AssertEquals("<p><span style=\"font-family: Tahoma, sans-serif; font-size: 20px;\">Assembly: winzor.GUI.Test.dll</span></p><p><span style=\"font-family: Tahoma, sans-serif; font-size: 20px;\">Class: vstest:System.Windows.Forms.ControlTest</span></p><p><span style=\"font-family: Tahoma, sans-serif; font-size: 20px;\">Method: InvokeRenderDispatcherWithExceptionHandledAfterWinzorDispatcherDisposed</span></p><p><a href=\"http://crikey.wtg.zone/failures/testFailureHistory/2b2fbd80-8147-40b5-aad8-c54d3463f9cd\" target=\"_blank\" style=\"font-family: Tahoma, sans-serif; font-size: 20px;\">http://crikey.wtg.zone/failures/testFailureHistory/2b2fbd80-8147-40b5-aad8-c54d3463f9cd</a></p><p></p><p></p>", richEdit.Html);
			}
		}
#endif

		public void TestRichTextBox_ChangeBackColorWhenNotForcedColor()
		{
			using (var form = new ZChildForm())
			using (var box = new ZRichTextBox())
			using (var anotherBox = new ZRichTextBox())
			{
				form.Controls.Add(box);
				form.Controls.Add(anotherBox);
				form.Show();

				anotherBox.Focus();
				AssertEquals(SystemColors.Window, box.RichEdit.BackColor);

				box.RichEdit.BackColor = Color.Red;
				AssertNull(box.ForcedBackColor);
				AssertEquals(Color.Red, box.RichEdit.BackColor);

				box.Focus();
				AssertEquals(EnterpriseFormLookStrategy.SelectedControlColor, box.RichEdit.BackColor);

				box.ReadOnly = true;
				AssertNull(box.ForcedBackColor);
				AssertEquals(SystemColors.Control, box.RichEdit.BackColor);
				form.Close();
			}
		}

		public void TestRichTextBox_ChangeBackColorForReadOnlyChangedWhenForcedColor()
		{
			using (var form = new ZChildForm())
			using (var box = new ZRichTextBox())
			{
				form.Controls.Add(box);
				form.Show();

				box.RichEdit.BackColor = Color.Red;
				AssertNull(box.ForcedBackColor);
				AssertEquals(Color.Red, box.RichEdit.BackColor);

				box.ForcedBackColor = Color.Black;
				AssertEquals(Color.Black, box.ForcedBackColor);
				AssertEquals(Color.Black, box.RichEdit.BackColor);

				box.ReadOnly = true;
				AssertEquals(Color.Black, box.ForcedBackColor);
				AssertEquals(Color.Black, box.RichEdit.BackColor);

				form.Close();
			}
		}

		public void TestRichTextBox_ChangeBackColorForFocusedAndUnFocusedWhenForcedColor()
		{
			using (var form = new ZChildForm())
			using (var box = new ZRichTextBox())
			using (var anotherBox = new ZRichTextBox())
			{
				form.Controls.Add(box);
				form.Controls.Add(anotherBox);
				form.Show();

				box.RichEdit.BackColor = Color.Red;
				AssertNull(box.ForcedBackColor);
				AssertEquals(Color.Red, box.RichEdit.BackColor);

				box.ForcedBackColor = Color.Black;
				AssertEquals(Color.Black, box.ForcedBackColor);
				AssertEquals(Color.Black, box.RichEdit.BackColor);

				anotherBox.Focus();
				AssertEquals(Color.Black, box.ForcedBackColor);
				AssertEquals(Color.Black, box.RichEdit.BackColor);

				box.Focus();
				AssertEquals(Color.Black, box.ForcedBackColor);
				AssertEquals(Color.Black, box.RichEdit.BackColor);

				form.Close();
			}
		}

		public void TestRichTextBox_DefaultValueOfForceBackColor()
		{
			using var rtb = new ZRichTextBox();
			var forcedBackColor = rtb.ForcedBackColor;
			AssertNull(forcedBackColor);
			AssertEquals(SystemColors.Window, rtb.RichEdit.BackColor);
		}

		#region Test Classes

		class TestRichTextBox : ZRichTextBox
		{
			public bool ReturnNullClipboard;

#if !WINZOR
			protected override IDataObject Clipboard_GetDataObject()
			{
				return ReturnNullClipboard ? null : base.Clipboard_GetDataObject();
			}

#endif

			public new RichTextBox RichEdit
			{
				get { return base.RichEdit; }
			}

			public new void PerformTab(bool forward)
			{
				base.PerformTab(forward);
			}
		}

		class RichTextBoxForPasteTest : ZRichTextBox
		{
#if !WINZOR
			protected override IDataObject Clipboard_GetDataObject()
			{
				return ClipboardDataObject;
			}

#endif

			public IDataObject ClipboardDataObject { get; set; }
		}

		#endregion

		#region Implementation

		TestRichTextBox RichEdit;
#if !WINZOR
		void AssertFontEquals(string message, Font x, Font y)
		{
			AssertEquals(message + "; FontFamily", x.FontFamily.Name, y.FontFamily.Name);
			AssertEquals(message + "; Height", x.Height, y.Height);
			AssertEquals(message + "; Size", x.Size, y.Size);
			AssertEquals(message + "; Style", x.Style, y.Style);
		}
#endif
		protected override void SetUp()
		{
			base.SetUp();
			RichEdit = new TestRichTextBox();
			RichEdit.CreateControl();
			Application.DoEvents();
		}

		protected override void TearDown()
		{
			if (RichEdit != null)
			{
				RichEdit.Dispose();
			}

			base.TearDown();
		}

		#endregion
	}
}
