using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public class BoardSectionDiagramControl : NetworkDiagramUserControl, IBoardSectionControl
	{
		public BoardSectionDiagramControl(IBMBoardSection section, BoardSectionViewModel viewModel)
		{
			SectionViewModel = viewModel;
			if (Globals.IsWinzor)
			{
				var winzorDIAErrorLabel = CreateZLabel(Res.GetString("99F6B60D-97E8-4AAE-B730-96FEAA6734A0", "Network Diagram section on a Visual Board is not supported in CargoWise Experimental Web Version."));
				Controls.Add(winzorDIAErrorLabel);
			}
			else if (!viewModel.IsPreview)
			{
				config = section.Configuration as DiagramBoardSectionConfiguration;
				SetupDiagram(viewModel.FactoryProvider.GetBoardGUIThreadFactory());
			}
			else
			{
				var label = CreateZLabel(Res.GetString("1b2a4119-2cf8-402b-9c85-0a79c1058ab3", "Diagram cannot be shown in preview"));
				Controls.Add(label);
			}
		}

		ZLabel CreateZLabel(String labelText)
		{
			var label = new ZLabel
			{
				Text = labelText,
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleCenter,
				Font = OFont.GetHeaderFont()
			};
			label.Font = new Font(Font.FontFamily, 18f);
			return label;
		}

		readonly DiagramBoardSectionConfiguration config;

		void SetupDiagram(BusinessObjectFactory factory)
		{
			var diagram = factory.Load<BMNCNShape>(config.DiagramPK);

			Controls.RemoveAndDisposeAll();
			SetDataBinding(diagram, string.Empty);

			if (Network != null)
			{
				Network.IsReadOnly = true;
			}
		}

		protected override bool RibbonIsEnabled => false;

		#region IBoardSectionControl Members

		string IBoardSectionControl.SectionType
		{
			get { return CCPMConstants.NetworkDiagramSectionType; }
		}

		void IBoardSectionControl.Refresh(BoardRefreshEventArgs args)
		{
			if (Network != null)
			{
				Network.FullRefresh();
			}
			if (RefreshCompleted != null)
			{
				RefreshCompleted(this, args);
			}
		}

		bool IBoardSectionControl.AcceptDraggedControl(object control)
		{
			return false;
		}

		public event EventHandler<BoardRefreshEventArgs> RefreshCompleted;

		bool IBoardSectionControl.SuppressBoardRefresh(BoardRefreshEventArgs args)
		{
			return false;
		}

		public BoardSectionViewModel SectionViewModel { get; }

		#endregion
	}
}
