using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class DataTransferFormTest : TestCase
	{
		public void TestShowAndClose()
		{
			using (var testForm = new TestDataTransferForm())
			{
				testForm.Show();
				AssertEquals(true, testForm.Visible);

				testForm.Close();
				AssertEquals(false, testForm.Visible);
			}
		}

		public void TestSetProcessProgress()
		{
			using (var testForm = new TestDataTransferForm())
			{
				testForm.Show();
				testForm.SetProcessProgress(50, 8, 2, "Entry");

				AssertEquals(50, testForm.ProgressBar.Value);
				AssertEquals("2", testForm.RowsExcludedLabel.Text);
				AssertEquals("6", testForm.RowsIncludedLabel.Text);
				AssertEquals("8", testForm.RowsProcessedLabel.Text);
				AssertEquals("Entry", testForm.LogListBox.Items[0].ToString());

				testForm.FinishProcess();

				AssertEquals(100, testForm.ProgressBar.Value);
				AssertEquals(false, testForm.ProcessButton.Enabled);
				AssertEquals(true, testForm.CloseButton.Enabled);
			}
		}

		public void TestDialogFilter()
		{
			using (var testForm = new TestDataTransferForm())
			{
				AssertEquals("Comma delimited files (*.csv)|*.csv|Text files (*.txt)|*.txt|XML files (*.xml)|*.xml|All files (*.*)|*.*", testForm.DialogFilter);
				testForm.DialogFilter = "Test";
				AssertEquals("Test", testForm.DialogFilter);
			}
		}

		public void TestFileNameBoxIsEmpty()
		{
			using (var testForm = new TestDataTransferForm())
			{
				AssertEquals("File name is not entered, it should be empty", "", testForm.FileNameTextBox.Text);
				testForm.Show();
				testForm.ProcessButton.PerformClick();
				AssertEquals("Check message on dialog box", "Please select a file to import from.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPathInFileNameBoxIsInValid()
		{
			using (var testForm = new TestDataTransferForm())
			{
				testForm.FileNameTextBox.Text = "X:\\123?.txt";
				Assert("Path entered is invalid", !CargoWise.IO.PathValidation.IsValid(testForm.FileNameTextBox.Text));
				testForm.Show();
				testForm.ProcessButton.PerformClick();
				AssertEquals("Check message on dialog box", "There are invalid characters in the file path.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestProcessFile()
		{
			using (var testForm = new TestDataTransferForm())
			{
				testForm.FileNameTextBox.Text = "Testing123";
				testForm.StartProcess += new ProcessFileEventHandler(TestForm_StartProcess);
				AssertEquals("Start process flag should be false", false, StartProcessCalled);
				testForm.Show();
				testForm.ProcessButton.PerformClick();
				AssertEquals("Start process flag should now be true", true, StartProcessCalled);
			}
		}

		public void TestCancelProcessFile()
		{
			using (var testForm = new TestDataTransferForm())
			{
				testForm.FileNameTextBox.Text = "Testing123";
				testForm.ProcessCancelled += new EventHandler(TestForm_ProcessCancelled);
				AssertEquals("Cancelled flag should be false", false, Cancelled);
				testForm.Show();
				testForm.ProcessButton.PerformClick();
				// The second click would cancel the process
				testForm.ProcessButton.PerformClick();
				AssertEquals("Cancelled flag should now be true", true, Cancelled);
			}
		}

		#region Implementation

		protected class TestDataTransferForm : DataTransferForm
		{
			public new ZButton BrowseButton
			{
				get { return base.BrowseButton; }
			}

			public new ProgressBar ProgressBar
			{
				get { return base.ProgressBar; }
			}

			public new ZButton ProcessButton
			{
				get { return base.ProcessButton; }
			}

			public new ZButton CloseButton
			{
				get { return base.CloseButton; }
			}

			public new ZLabel RowsProcessedLabel
			{
				get { return base.RowsProcessedLabel; }
			}

			public new ZLabel RowsIncludedLabel
			{
				get { return base.RowsIncludedLabel; }
			}

			public new ZLabel RowsExcludedLabel
			{
				get { return base.RowsExcludedLabel; }
			}

			public new ZTextBox FileNameTextBox
			{
				get { return base.FileNameTextBox; }
			}

			public new ListBox LogListBox
			{
				get { return base.LogListBox; }
			}
		}

		void TestForm_StartProcess(object sender, ProcessFileEventArgs e)
		{
			StartProcessCalled = true;
		}

		void TestForm_ProcessCancelled(object sender, EventArgs e)
		{
			Cancelled = true;
		}

		protected bool StartProcessCalled;
		protected bool Cancelled;

		#endregion

	}
}
