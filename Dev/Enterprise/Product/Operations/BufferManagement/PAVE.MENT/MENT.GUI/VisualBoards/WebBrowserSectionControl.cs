using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class WebBrowserSectionControl : ZUserControl, IBoardSectionControl
	{
		public WebBrowserSectionControl(IBMBoardSection section, BoardSectionViewModel viewModel)
		{
			InitializeComponent();
			BoardName = section?.Board?.Name ?? string.Empty;
			SectionViewModel = viewModel;
			var label = new ZLabel
			{
				Text = Res.GetString("C231DD43-912D-4843-B509-5481FA8320A9", "WEB sections are no longer supported."),
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleCenter,
				Font = new Font(Font.FontFamily, 18f)
			};

			Controls.Add(label);
		}
		#region IBoardSectionControl Members

		string IBoardSectionControl.SectionType { get; } = MENTConstants.WebBrowserSectionType;
		void IBoardSectionControl.Refresh(BoardRefreshEventArgs args) => RefreshCompleted?.Invoke(this, args);
		bool IBoardSectionControl.AcceptDraggedControl(object control) => false;
		public event EventHandler<BoardRefreshEventArgs> RefreshCompleted;
		bool IBoardSectionControl.SuppressBoardRefresh(BoardRefreshEventArgs args) => false;

		public string BoardName { get; }
		public BoardSectionViewModel SectionViewModel { get; }

		#endregion
	}
}
