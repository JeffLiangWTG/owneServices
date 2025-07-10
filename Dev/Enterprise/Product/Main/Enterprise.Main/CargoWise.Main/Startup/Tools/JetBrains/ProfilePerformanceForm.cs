using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.Tools.JetBrains
{
	public partial class ProfilePerformanceForm : ZChildForm
	{
		public ProfilePerformanceForm(ProfilePerformanceModel model)
			: base(model)
		{
			InitializeComponent();

			this.model = model;
			this.model.IsStartedInfo.ValueChanged += IsStartedValueChanged;
			ProfileModel.IsSessionStarted = true;
		}

		void OnFormClosing(object sender, FormClosingEventArgs e)
		{
			if (model.IsStarted)
			{
				model.StopProfiling();
			}

			model.Cleanup();
			ProfileModel.IsSessionStarted = false;
		}

		async void BtnStartClick(object sender, EventArgs e)
		{
			isBusy = true;
			UpdateUI();

#if WINZOR
			Func<Action, Task> run = (Action action) => InvokeWinzorDispatcherAsync(action);
#else
			Func<Action, Task> run = (Action action) => Task.Run(action);
#endif

			await run(() =>
			{
				if (model.IsStarted)
				{
					model.StopProfiling();
				}
				else
				{
					model.StartProfiling();
				}
			});

			isBusy = false;
			UpdateUI();
		}

		void IsStartedValueChanged(object sender, EventArgs e)
		{
			if (InvokeRequired)
			{
				Invoke(new Action(UpdateUI));
			}
			else
			{
				UpdateUI();
			}
		}

		void UpdateUI()
		{
			if (isBusy)
			{
				ControlBox = false;
				btnStartStop.Enabled = false;
				btnStartStop.BackColor = Color.Gray;
				return;
			}

			ControlBox = true;
			btnStartStop.Enabled = true;

			btnStartStop.Text = model.IsStarted
				? ResString.GetMultilingualString("b67aaf98-6f6a-4c64-9901-f40409bdce7b", "Stop")
				: ResString.GetMultilingualString("f308bd76-7ff1-4fc1-a705-30c9dbeb6ad7", "Start");

			btnStartStop.BackColor = model.IsStarted
				? Color.OrangeRed
				: Color.DarkSeaGreen;
		}

		readonly ProfilePerformanceModel model;
		bool isBusy;
	}
}
