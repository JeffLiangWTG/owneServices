using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Startup.Tools.JetBrains
{
	public partial class ProfileMemoryForm : ZChildForm
	{
		public ProfileMemoryForm(ProfileMemoryModel model)
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

			await Task.Run(() =>
			{
				if (model.IsStarted)
				{
					model.TakeSnapshot();
				}
				else
				{
					model.StartProfiling();
				}
			});

			isBusy = false;
			UpdateUI();
		}

		async void BtnFinishClick(object sender, EventArgs e)
		{
			if (!model.IsStarted)
			{
				return;
			}

			isBusy = true;
			UpdateUI();

#if WINZOR
			Func<Action, Task> run = (Action action) => InvokeWinzorDispatcherAsync(action);
#else
			Func<Action, Task> run = (Action action) => Task.Run(action);
#endif

			await run(() => model.StopProfiling());

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
				btnStart.Enabled = false;
				btnFinish.Enabled = false;
				return;
			}

			ControlBox = true;
			btnFinish.Enabled = model.IsStarted;
			btnStart.Enabled = true;

			btnStart.Text = model.IsStarted
				? ResString.GetMultilingualString("c56b13f1-68ef-41f2-b55a-e82ebf213dd5", "Get Snapshot")
				: ResString.GetMultilingualString("f308bd76-7ff1-4fc1-a705-30c9dbeb6ad7", "Start");
		}

		bool isBusy;
		readonly ProfileMemoryModel model;
	}
}
