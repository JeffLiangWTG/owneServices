using System.Threading;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	internal class ThreadMonitorFormTest : TestCase
	{
		public void TestThreadComboBox()
		{
			using (var form = new ThreadMonitorFormForTest(Thread.CurrentThread))
			{
				AssertEquals("Should have 2 thread items", 2, form.ThreadComboBox_Expose.Items.Count);
				AssertEquals("Main Thread", form.ThreadComboBox_Expose.Items[0]);
				AssertEquals("Print Client Thread", form.ThreadComboBox_Expose.Items[1]);
				AssertEquals("Main Thread shoule be default selected", 0, form.ThreadComboBox_Expose.SelectedIndex);
			}
		}

		public void TestIntervalTypeComboBox()
		{
			using (var form = new ThreadMonitorFormForTest(Thread.CurrentThread))
			{
				AssertEquals("Should have 3 items", 3, form.IntervalTypeComboBox_Expose.Items.Count);
				AssertEquals("Millisecond", form.IntervalTypeComboBox_Expose.Items[0]);
				AssertEquals("Second", form.IntervalTypeComboBox_Expose.Items[1]);
				AssertEquals("Minute", form.IntervalTypeComboBox_Expose.Items[2]);
				AssertEquals("Second shoule be default selected", 1, form.IntervalTypeComboBox_Expose.SelectedIndex);
			}
		}

		public void TestValidateBefreRunning()
		{
			using (var form = new ThreadMonitorFormForTest(Thread.CurrentThread))
			{
				form.Show();
				form.ThreadComboBox_Expose.SelectedIndex = -1;
				form.IntervalTypeComboBox_Expose.SelectedIndex = -1;

				form.StartButton_Expose.PerformClick();
				var expectedMessage = @"Please select a valid Thread.
Please select a valid Interval Type.
";
				AssertEquals(expectedMessage, form.StackTraceTextBox_Expose.Text);
			}
		}

		public void TestCurrentTimer()
		{
			using (var form = new ThreadMonitorFormForTest(Thread.CurrentThread))
			{
				form.Show();
				form.StartButton_Expose.PerformClick();

				AssertEquals("CurrentTimer should be enabled", true, form.CurrentTimer_Expose.Enabled);
				AssertEquals("Should be changed to Stop", "Stop", form.StartButton_Expose.Text);
				AssertEquals("IntervalNumber.Enabled should be false", false, form.IntervalNumber_Expose.Enabled);
				AssertEquals("IntervalTypeComboBox.Enabled should be false", false, form.IntervalTypeComboBox_Expose.Enabled);
			}
		}

		public void TestCollectThreadSnapshot()
		{
			using (var form = new ThreadMonitorFormForTest(Thread.CurrentThread))
			{
				form.Show();
				form.CollectThreadSnapshotButton_Expose.PerformClick();
				var timesArray = (string[])form.TimesListBox_Expose.DataSource;

				AssertEquals("TimesListBox should have 1 item", 1, timesArray.Length);
				AssertContains("Enterprise.RemotePrinting.Client.ThreadMonitorForm", form.StackTraceTextBox_Expose.Text);
			}
		}

		[DeveloperOnlyTest]
		public void TestCopyAll()
		{
			using (var form = new ThreadMonitorFormForTest(Thread.CurrentThread))
			{
				form.Show();
				form.CollectThreadSnapshotButton_Expose.PerformClick();

				form.CopyAllButton_Expose.PerformClick();
				var timesArray = (string[])form.TimesListBox_Expose.DataSource;

				var clipboardText = SafeClipboard.GetDataObject().GetData(typeof(string)).ToString();

				var expectedText = $"[{timesArray[0]}]{System.Environment.NewLine}{form.StackTraceTextBox_Expose.Text}";
				AssertEquals(expectedText, clipboardText);
			}
		}

		public void TestClear()
		{
			using (var form = new ThreadMonitorFormForTest(Thread.CurrentThread))
			{
				form.Show();

				AssertNull("TimesListBox.DataSource", form.TimesListBox_Expose.DataSource);
				AssertNullOrEmpty("StackTraceTextBox.Text", form.StackTraceTextBox_Expose.Text);

				form.CollectThreadSnapshotButton_Expose.PerformClick();

				AssertNotNull("TimesListBox", form.TimesListBox_Expose.DataSource);
				AssertNotNullOrEmpty("StackTraceTextBox", form.StackTraceTextBox_Expose.Text);

				form.ClearButton_Expose.PerformClick();

				AssertNull("TimesListBox.DataSource", form.TimesListBox_Expose.DataSource);
				AssertNullOrEmpty("StackTraceTextBox.Text", form.StackTraceTextBox_Expose.Text);
			}
		}

		class ThreadMonitorFormForTest : ThreadMonitorForm
		{
			public ThreadMonitorFormForTest(Thread mainThread) : base(mainThread)
			{
			}

			public System.Windows.Forms.Timer CurrentTimer_Expose => CurrentTimer;

			public ComboBox ThreadComboBox_Expose => ThreadComboBox;
			public NumericUpDown IntervalNumber_Expose => IntervalNumber;
			public ComboBox IntervalTypeComboBox_Expose => IntervalTypeComboBox;
			public Button StartButton_Expose => StartButton;
			public Button CollectThreadSnapshotButton_Expose => CollectThreadSnapshotButton;
			public Button CopyAllButton_Expose => CopyAllButton;
			public Button ClearButton_Expose => ClearButton;
			public TextBox StackTraceTextBox_Expose => StackTraceTextBox;
			public ListBox TimesListBox_Expose => TimesListBox;
		}
	}
}
