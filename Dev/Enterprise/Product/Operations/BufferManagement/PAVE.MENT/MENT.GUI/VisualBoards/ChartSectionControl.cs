using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Business;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class ChartSectionControl : ZUserControl, IBoardSectionControl
	{
		public ChartSectionControl(BoardSectionViewModel viewModel)
		{
			InitializeComponent();

			SectionViewModel = viewModel;
			if (!ObjectFactory.Get<IBMSRegistry>().EnableMENTSections)
			{
				var label = CreateLabel(Res.GetString("E1FD7872-11E5-455C-842A-84F2FB03A267", "MENT sections are no longer supported"), Font.FontFamily);
				Controls.Add(label);
			}
			else if (viewModel.IsPreview)
			{
				var label = CreateLabel(Res.GetString("6f8218a7-a320-46bc-8e46-981d4434d83b", "Skew cannot be shown in preview"), Font.FontFamily);
				Controls.Add(label);
			}
		}

#if DEBUG
		public
#endif
		MENTChartWindowsControl chartControl;
		ZElementHost container;

		public BoardSectionViewModel SectionViewModel { get; }

		VisualisationViewModel CreateViewModel(ChartSectionConfiguration configuration)
		{
			var extraction = configuration.Extraction;
			var visualisation = configuration.RelatedVisualisation;
			var viewModelProvider = new VisualisationViewModelProvider(extraction, visualisation);

			VisualisationViewModel visualisationViewModel = null;

			if (extraction != null && visualisation != null && (extraction.RelatedQuery.MAQ_IsActive || !extraction.IsInstantaneous))
			{
				visualisationViewModel = viewModelProvider.GenerateViewModel(SectionViewModel.FactoryProvider);
				visualisationViewModel.RefreshModel();
			}

			return visualisationViewModel;
		}

		void CreateChartControl(VisualisationViewModel visualisationViewModel, ChartSectionConfiguration configuration)
		{
			RemoveAndDisposeContainer();
			var extraction = configuration.Extraction;

			if (visualisationViewModel != null)
			{
				chartControl = new MENTChartWindowsControl
				{
					Dock = DockStyle.Fill,
					BackColor = Color.Transparent,
					Size = ControlDpiScalingHelper.NewScaledSize(300, 300)
				};

#if !WINZOR
				plotView = new OxyplotView { Dock = DockStyle.Fill, Model = visualisationViewModel.PlotModel };
				chartControl.Controls.Add(plotView);
				plotView.InvalidatePlot(true);
#endif
				Controls.Add(chartControl);
			}
			else if (extraction != null && extraction.IsInstantaneous && !extraction.RelatedQuery.MAQ_IsActive)
			{
				label = CreateLabel(Res.GetString("250d15d3-1848-4704-a878-ceb2412ef08e", "Queries must be active to be Instantaneous."), Font.FontFamily);
				Controls.Add(label);
			}
			else
			{
				label = CreateLabel(Res.GetString("8e4cfad0-e609-4215-ab56-af39607ad5a1", "Skew does not exist."), Font.FontFamily);
				Controls.Add(label);
			}
		}
		ZLabel label;

		void RemoveAndDisposeContainer()
		{
			if (container != null && container.IsHandleCreated)
			{
				this.Controls.Remove(container);
				container.Dispose();
				container = null;
			}
			if (label != null && label.IsHandleCreated)
			{
				this.Controls.Remove(label);
				label.Dispose();
				label = null;
			}
		}

		static ZLabel CreateLabel(string text, FontFamily fontFamily)
		{
			var label = new ZLabel
			{
				Text = text,
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleCenter,
				Font = OFont.GetHeaderFont(),
			};
			label.Font = new Font(fontFamily, 18f);

			return label;
		}

		#region IBoardSectionControl Members

		string IBoardSectionControl.SectionType
		{
			get { return MENTConstants.ChartSectionType; }
		}

		bool IBoardSectionControl.AcceptDraggedControl(object control)
		{
			return false;
		}

		public event EventHandler<BoardRefreshEventArgs> RefreshCompleted;

		void IBoardSectionControl.Refresh(BoardRefreshEventArgs args)
		{
			if (!SectionViewModel.IsPreview)
			{
				try
				{
					AsyncStrategy.Default.DoAsync(() =>
					{
						var factory = SectionViewModel.FactoryProvider.GetNewEditFactory((NoResString)"MENT ViewModel Factory"); // This is a debug factory name for identification
						var section = factory.Load<BMBoardSection>(SectionViewModel.SectionPK);

						if (section != null)
						{
							var configuration = section.Configuration as ChartSectionConfiguration;
							var visualisationViewModel = CreateViewModel(configuration);
							factory.ThreadSentry.RelinquishThreadOwnership();

							this.BeginInvokeSafe(() =>
							{
								factory.ThreadSentry.TakeThreadOwnership();
								CreateChartControl(visualisationViewModel, configuration);
							});
						}
					});
				}
				finally
				{
					if (RefreshCompleted != null)
					{
						RefreshCompleted(this, args);
					}
				}
			}
		}

		bool IBoardSectionControl.SuppressBoardRefresh(BoardRefreshEventArgs args)
		{
			return false;
		}

		#endregion
	}
}
