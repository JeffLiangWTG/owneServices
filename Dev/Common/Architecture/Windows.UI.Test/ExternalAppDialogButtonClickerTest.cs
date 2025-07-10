using System;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	class ExternalAppDialogButtonClickerTest : TestCase
	{
		public void TestClickDialogButtons()
		{
			using (var formWithButton = new FormWithButton())
			{
				formWithButton.Text = "FormWithButton";
				using (var clicker = new TestExternalAppDialogButtonClicker("DOSTUff"))
				{
					var someButton = new Button();
					someButton.Text = "&DoStuff";
					someButton.Click += new EventHandler((sender, args) => formWithButton.Close());
					someButton.Click += new EventHandler(OnSomeButton_Click);
					formWithButton.Controls.Add(someButton);

					clicker.PressButtonOnNextDialogs();
					formWithButton.ShowDialog();
					AssertEquals("Button should be clicked.", true, fButtonClicked);
				}
			}
		}

		public void TestDontClickAlreadyShowingForm()
		{
			using (var formWithButton = new FormWithButton())
			{
				formWithButton.Text = "FormWithButton";
				using (var clicker = new TestExternalAppDialogButtonClicker("DOSTUff"))
				{
					var someButton = new Button();
					someButton.Text = "&DoStuff";
					someButton.Click += new EventHandler(OnSomeButton_Click);
					formWithButton.Controls.Add(someButton);

					formWithButton.ShowDialog();
					clicker.PressButtonOnNextDialogs();
					AssertEquals("Button on already showing form should not be clicked. Windows open initially", false, fButtonClicked);
				}
			}
		}

		public void TestDisposesInTimelyFashion()
		{
			TestExternalAppDialogButtonClicker clicker = new TestExternalAppDialogButtonClicker("DOSTUff");
			clicker.PressButtonOnNextDialogs();

			int i = 0;
			while (i++ < 100 && !clicker.IsSearchingForButtonToClick)
			{
				Thread.Sleep(1000);
			}

			DateTime before = DateTime.Now;
			clicker.Dispose();
			AssertEquals("Shouldn't take more than 5 seconds to finish a dispose", true, DateTime.Now.Subtract(before) < new TimeSpan(0, 0, 5));
		}

		#region Implementation

		bool fButtonClicked;
		void OnSomeButton_Click(object sender, EventArgs e)
		{
			fButtonClicked = true;
		}

		class FormWithButton : Form
		{
			public FormWithButton()
			{
				timer = new System.Windows.Forms.Timer();
				timer.Enabled = false;
				timer.Interval = 1000;
				timer.Tick += Timer_Tick;
			}

			System.Windows.Forms.Timer timer;

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				timer.Enabled = true;
			}

			void Timer_Tick(object sender, EventArgs e)
			{
				timer.Enabled = false;
				timer.Tick -= Timer_Tick;
				timer = null;
				if (!IsDisposed)
				{
					Dispose();
				}
			}
		}

		protected class TestExternalAppDialogButtonClicker : ExternalAppDialogButtonClicker
		{
			public TestExternalAppDialogButtonClicker(string buttonText)
				: base(buttonText)
			{
			}

			public new bool IsSearchingForButtonToClick
			{
				get { return base.IsSearchingForButtonToClick; }
			}
		}

		#endregion
	}
}
