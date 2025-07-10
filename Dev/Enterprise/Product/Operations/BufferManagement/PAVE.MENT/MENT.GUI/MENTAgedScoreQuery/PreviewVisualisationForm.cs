using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class PreviewVisualisationForm : ZChildForm
	{
		public PreviewVisualisationForm(VisualisationViewModelProvider viewModelProvider,
			IVisualisationFactoryProvider provider)
		{
			InitializeComponent();
			CreateLoadingLabel();

			this.viewModelProvider = viewModelProvider;
			this.provider = provider;
		}

		readonly VisualisationViewModelProvider viewModelProvider;
#if DEBUG
		public
#endif
			ZLabel loadingLabel;

		readonly IVisualisationFactoryProvider provider;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			LoadPlotModelOnBackgroundThread();
		}

		protected override bool ShowStatusBar
		{
			get { return false; }
		}

		void LoadPlotModelOnBackgroundThread()
		{
			AsyncStrategy.Default.DoAsync(() =>
			{
				var viewModel = viewModelProvider.GenerateViewModel(provider);
				viewModel.RefreshModel();

				this.BeginInvokeSafe(new Action(() =>
				{
					CreateChartControl(viewModel);
					RemoveLoadingLabel();
				}));
			});
		}

		void RemoveLoadingLabel()
		{
			loadingLabel.Dispose();
			loadingLabel = null;
		}

		void CreateChartControl(VisualisationViewModel viewModel)
		{
#if !WINZOR
			chartControl = new MENTChartWindowsControl
			{
				Dock = DockStyle.Fill,
				BackColor = Color.Transparent,
				Size = ControlDpiScalingHelper.NewScaledSize(300, 300),
				CaptionRenderingEnabled = true
			};

			plotView = new OxyplotView { Dock = DockStyle.Fill, Model = viewModel.PlotModel };
			chartControl.Controls.Add(plotView);
			plotView.InvalidatePlot(true);
			Controls.Add(chartControl);
#endif
		}

		void CreateLoadingLabel()
		{
			loadingLabel =
				CreateLabel(Res.GetString("684f3c9d-9a66-4ca4-878d-c1ca759bd18c", "Loading, please wait..."));
			this.Controls.Add(loadingLabel);
		}

		ZLabel CreateLabel(string text)
		{
			var label = new ZLabel
			{
				Text = text,
				Dock = DockStyle.Fill,
				TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
				Font = new Font(Font.FontFamily, 18f),
			};

			return label;
		}
	}
}
