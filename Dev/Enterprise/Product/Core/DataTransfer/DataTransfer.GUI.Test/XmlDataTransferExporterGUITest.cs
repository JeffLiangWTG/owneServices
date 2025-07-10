using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.GUI.Testing
{
	sealed class XmlDataTransferExporterGUITest : TestCase
	{
		public void TestShowSaveFileDialog()
		{
			ZFormModaliser.FileNameToSelectInShowCommonDialog = "Foo.xml";
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var gui = new XmlDataTransferExporterGUI();
			gui.ShowSaveFileDialog("default", "initial");
			AssertType(typeof(SaveFileDialog), ZFormModaliser.LastCommonDialogShownDialogForTest);
			AssertEquals("Foo.xml", gui.UnmappedFile);
		}

		public void TestShowProgressForm()
		{
			var mockProgressSupported = new Mock<IProgressSupporter>();
			var gui = new XmlDataTransferExporterGUI();
			using (var progressForm = (ProgressForm)gui.ShowProgressForm(mockProgressSupported.Object, 10))
			{
				AssertEquals(0, progressForm.PercentComplete);
				mockProgressSupported.Raise(p => p.Progress += null, EventArgs.Empty);
				AssertEquals(10, progressForm.PercentComplete);
				mockProgressSupported.Raise(p => p.Progress += null, EventArgs.Empty);
				AssertEquals(20, progressForm.PercentComplete);
			}
		}
	}
}
