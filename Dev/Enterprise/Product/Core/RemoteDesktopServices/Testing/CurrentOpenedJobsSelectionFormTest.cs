using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Server;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class CurrentOpenedJobsSelectionFormTest : TestCase
	{
		[GuiTest]
		public void TestSelectionFormPopUpOnce()
		{
			using (var handler = new StartDropHandlerForTest())
			using (var stream = new MemoryStream())
			{
#if NETFRAMEWORK
				var messageByte1 = Encoding.UTF8.GetBytes(WindowCaptionUtils.Base64EncodeCaptionAndHandle("NotAnExistingFormText", (UIntPtr)111213));
				var messageByte2 = Encoding.UTF8.GetBytes(WindowCaptionUtils.Base64EncodeCaptionAndHandle("(NotAnExistingFormText)", (UIntPtr)131415));
#else
				var messageByte1 = Encoding.UTF8.GetBytes(WindowCaptionUtils.Base64EncodeCaptionAndHandle("NotAnExistingFormText", 111213));
				var messageByte2 = Encoding.UTF8.GetBytes(WindowCaptionUtils.Base64EncodeCaptionAndHandle("(NotAnExistingFormText)", 131415));
#endif

				stream.Write(messageByte1, 0, messageByte1.Length);
				stream.Position = 0;
				handler.Handle(null, stream);
				var dragDropHelperForTest = handler.GetDragDropHelperInstance() as DragDropHelperForTest;
				AssertNotNull(dragDropHelperForTest);
				Assert(dragDropHelperForTest.SelectionFormPopCount == 0);

				stream.Position = 0;
				stream.Write(messageByte2, 0, messageByte2.Length);
				stream.Position = 0;
				handler.Handle(null, stream);
				Assert(dragDropHelperForTest.SelectionFormPopCount == 1);

				stream.Position = 0;
				handler.Handle(null, stream);
				Assert(dragDropHelperForTest.SelectionFormPopCount == 1);
			}
		}

		public void TestStartDropHandlerInvalidMessage()
		{
			var originalMessage = InitializationMessageHandler.RemoteInitializationMessage;
			using (var handler = new StartDropHandlerForTest())
			using (var stream = new MemoryStream())
			using (new DisposableAction(() => InitializationMessageHandler.RemoteInitializationMessage = new MessageElements.InitializationMessage(Array.Empty<string>(), ClientVersion.Version.ToString()),
				() => InitializationMessageHandler.RemoteInitializationMessage = originalMessage))
			{
				var messageByte1 = Encoding.UTF8.GetBytes("|||MockFormHandle1");
				var messageByte2 = Encoding.UTF8.GetBytes("");
				var messageByte3 = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes("NotAnExistingFormText")));

				stream.Write(messageByte1, 0, messageByte1.Length);
				stream.Position = 0;
				AssertNoExceptionThrown(() => handler.Handle(null, stream));
				Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));

				stream.Position = 0;
				stream.Write(messageByte2, 0, messageByte2.Length);
				stream.Position = 0;
				AssertNoExceptionThrown(() => handler.Handle(null, stream));

				stream.Position = 0;
				stream.Write(messageByte3, 0, messageByte3.Length);
				stream.Position = 0;
				AssertNoExceptionThrown(() => handler.Handle(null, stream));

				ErrorReporter.Instance.Clear();
			}
		}

		[GuiTest]
		public void TestComboBoxDropDownWidth_ShortTitles()
		{
			TestComboBoxDropDownWidthCore(
				comboBox => comboBox.Width,
				"FormTitleForTest1",
				"FormTitleForTest2",
				"FormTitleForTest3");
		}

		const string LongTitle = "alongLongLONGLONGLONGLONG" +
			"LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG" +
			"LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG" +
			"LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONG" +
			"LONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGLONGTitle";
		const string LongTitle1 = LongTitle + "1";
		const string LongTitle2 = LongTitle + "2";
		const string LongTitle3 = LongTitle + "3";

		[GuiTest]
		public void TestComboBoxDropDownWidth_LongTitles()
		{
			TestComboBoxDropDownWidthCore(
				comboBox =>
				{
					using (var g = comboBox.CreateGraphics())
					{
						return (int)g.MeasureString(LongTitle1, comboBox.Font).Width;
					}
				},
				"FormShortTitleForTest1",
				"FormShortTitleForTest2",
				LongTitle1);
		}

		[GuiTest]
		public void TestComboBoxDropDownWidth_LongTitlesWithScrollBar()
		{
			TestComboBoxDropDownWidthCore(
				comboBox =>
				{
					comboBox.MaxDropDownItems = 3;

					using (var g = comboBox.CreateGraphics())
					{
						return (int)g.MeasureString(LongTitle1, comboBox.Font).Width + SystemInformation.VerticalScrollBarWidth;
					}
				},
				"FormShortTitleForTest1",
				"FormShortTitleForTest2",
				LongTitle1,
				LongTitle2,
				LongTitle3);
		}

		[GuiTest]
		public void TestComboBoxDropDownWidth_NoTitle()
		{
			TestComboBoxDropDownWidthCore(comboBox => comboBox.Width);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Baseline")]
		void TestComboBoxDropDownWidthCore(Func<ComboBox, int> expectedDropDownWidthGetter, params string[] titles)
		{
			var forms = titles.Select(x => new Form { Text = x }).ToArray();

			try
			{
				using (var selectionForm = new CurrentOpenedJobsSelectionForm(forms))
				{
					var expectedWidth = expectedDropDownWidthGetter(selectionForm.ComboBox);
					selectionForm.Show();

					AssertEquals(expectedWidth, selectionForm.ComboBox.DropDownWidth);

					selectionForm.Close();
				}
			}
			finally
			{
				forms.ForEach(x => x.Dispose());
			}
		}
	}
}
