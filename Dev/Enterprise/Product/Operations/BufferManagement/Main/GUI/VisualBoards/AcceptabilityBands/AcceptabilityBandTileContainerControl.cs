using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class AcceptabilityBandTileContainerControl : ZPanel
	{
		public AcceptabilityBandTileContainerControl(BMBoardSectionViewModel viewModel)
		{
			InitializeComponent();
			this.viewModel = viewModel;
			var sectionBands = viewModel.BoardSectionAcceptabilityBands.Where(x => x.ShouldShowAsTile);
			var emptyResults = sectionBands.Select(BoardSectionAcceptabilityBandResult.Empty).ToArray();
			PopulateTiles(emptyResults);
		}

		readonly BMBoardSectionViewModel viewModel;

		public void PopulateTiles(BoardSectionAcceptabilityBandResult[] results)
		{
			var tiles = GetOrCreateTileControls(results);
			var width = tiles.Any() ? tiles.Last().Right + Padding.Right : 0;
			ControlDpiScalingHelper.SetWidth(this, width, false);
		}

		List<AcceptabilityBandTileControl> GetOrCreateTileControls(BoardSectionAcceptabilityBandResult[] results)
		{
			try
			{
				this.SuspendDrawing();
				var oldTable = GetLayoutPanel();
				oldTable?.Controls.RemoveAndDisposeAll();

				var table = oldTable ?? new FlowLayoutPanel
				{
					Name = TableControlName,
					Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
					Size = ClientSize,
					FlowDirection = FlowDirection.LeftToRight,
					WrapContents = false,
					Margin = Padding.Empty,
					Padding = Padding.Empty,
				};

				var tiles = new List<AcceptabilityBandTileControl>();

				foreach (var result in results.OrderBy(x => x.SectionBand.DisplaySequence))
				{
					var tileViewModel = new AcceptabilityBandTileViewModel(result.SectionBand as BoardSectionAcceptabilityBand, viewModel, result.Result);
					var tile = new AcceptabilityBandTileControl(tileViewModel);
#if WINZOR
					ControlDpiScalingHelper.SetWidth(tile, tile.Controls[0].Width + tile.Controls[0].Left + tile.Controls[0].Left, false);
#endif

					tiles.Add(tile);
					table.Controls.Add(tile);
				}

				if (oldTable == null)
				{
					Controls.Add(table);
				}

				return tiles;
			}
			finally
			{
				this.ResumeDrawing();
			}
		}

		const string TableControlName = "ContentTable";

		public bool WrapContents
		{
			get
			{
				var panel = GetLayoutPanel();

				return panel != null && panel.WrapContents;
			}
			set
			{
				var panel = GetLayoutPanel();

				if (panel != null)
				{
					panel.WrapContents = value;
				}
			}
		}

		FlowLayoutPanel GetLayoutPanel()
		{
			return this.FindSingleOrDefault<FlowLayoutPanel>();
		}
	}
}
