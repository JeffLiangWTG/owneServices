using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class VisualBoardPreviewUserControl : ZUserControl
	{
		public VisualBoardPreviewUserControl()
		{
			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				ClearPreview();
			}
		}

#if !WINZOR
		internal
#else
		public
#endif
		void UpdatePreview(IEnumerable<BMBoardSection> boardSections)
		{
			if (boardSections == null)
			{
				ClearPreview();
			}
			else if (boardSections.Any(s => s.HasErrors()))
			{
				ShowError();
				shownSections = boardSections;
				SubscribeToLayoutChanges();
			}
			else
			{
				var viewModelPairs = boardSections.Select(section =>
					{
						var boardSectionDescriptor = SectionDescriptorProvider.Get(section.MS_SectionType);
						if (boardSectionDescriptor != null)
						{
							var boardSlideshowViewModel = new BoardSlideshowViewModel(section.Board, isPreview: true);
							var boardViewModel = boardSlideshowViewModel.CurrentBoardViewModel;
							var boardSectionViewModel = boardSectionDescriptor.GetViewModel(section, boardViewModel);

							return new BoardSectionViewModelPair(section, boardSectionViewModel);
						}
						else
						{
							return null;
						}
					}).Where(s => s != null).ToArray();

				UpdatePreviewCore(viewModelPairs);
			}
		}

		void UpdatePreviewCore(IEnumerable<BoardSectionViewModelPair> boardSections)
		{
			Control visualBoardPreview = null;
			try
			{
				foreach (var viewModelPair in boardSections)
				{
					viewModelPair.ViewModel.RefreshForPreview(viewModelPair.Section);
				}

				var tableLayoutPanel = VisualBoardSectionsRenderer.Render(boardSections);
				var size = ControlDpiScalingHelper.NewScaledSize(800, 600);
				tableLayoutPanel.Size = size;
#if !WINZOR
				var bitmap = new Bitmap(size.Width, size.Height);
				tableLayoutPanel.DrawToBitmap(bitmap, ControlDpiScalingHelper.NewScaledRectangle(Point.Empty, size));
				visualBoardPreview = new OptimisticPictureBox(shouldDisposeImageOnControlDispose: true)
				{
					Size = size,
					Image = bitmap,
					Name = nameof(VisualBoardPreviewUserControl),
					SizeMode = PictureBoxSizeMode.Zoom,
				};

				tableLayoutPanel.Controls.RemoveAndDisposeAll();
#else
				visualBoardPreview = new System.Windows.Forms.Adaptive.PreviewBox()
				{
					Size = size,
					PreviewControl = tableLayoutPanel,
					Name = nameof(VisualBoardPreviewUserControl),
					SizeMode = System.Windows.Forms.Adaptive.PreviewBoxSizeMode.Zoom,
				};

				ExtraStyleString += (ZArchitecture.Core.NoResString)"pointer-events:none;";
#endif
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (!IsDisposed && IsHandleCreated)
				{
					ShowError();
					shownSections = boardSections.Select(p => p.Section).Cast<BMBoardSection>();
					SubscribeToLayoutChanges();
				}
			}
			if (visualBoardPreview != null)
			{
				visualBoardPreview.Dock = DockStyle.Fill;
				if (!IsDisposed && IsHandleCreated)
				{
					ClearPreview();
					Controls.Add(visualBoardPreview);
					shownSections = boardSections.Select(p => p.Section).Cast<BMBoardSection>();
					SubscribeToLayoutChanges();
				}
				else
				{
					visualBoardPreview.Dispose();
				}
			}
		}

		IEnumerable<BMBoardSection> shownSections;

		void SubscribeToLayoutChanges()
		{
			foreach (var section in shownSections)
			{
				section.LayoutConfigUpdated += Section_LayoutConfigUpdated;
			}
		}

		void Section_LayoutConfigUpdated(object sender, EventArgs e)
		{
			UpdatePreview(shownSections);
		}

		void ShowError()
		{
			ClearPreview();
			var label = new ZLabel { AutoSize = true, Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, Text = Res.GetString("e8c27914-11c7-41a9-b48a-06ec7b4da02e", "Cannot preview, please check section values.") };
			Controls.Add(label);
		}

		internal void ClearPreview()
		{
			if (shownSections != null)
			{
				foreach (var section in shownSections)
				{
					section.LayoutConfigUpdated -= Section_LayoutConfigUpdated;
				}
			}

			Controls.RemoveAndDisposeAll();
		}
	}
}
