using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TaskPanel : ZPanel, ITasksControl, ITasksDetails
	{
		public TaskPanel(CellContent cellContent, BMBoardSectionViewModel viewModel, KContextMenuStrip taskCardMenuStrip, ITaskCardRenderer cachingCardRenderer = null, BMComponentControl componentControl = null, BMBoardSection section = null)
		{
			Argument.NotNull(taskCardMenuStrip, "taskCardMenuStrip");

			Name = "TaskPanel";
			cell = cellContent;
			sectionViewModel = viewModel;
			layoutStrategy = TaskPanelLayoutStrategyFactory.GetStrategy(viewModel);
			cardRenderer = cachingCardRenderer ?? TaskCardRendererFactory.GetRenderer(viewModel, this is ReleaseSchedulerTaskPanel);
			sharedMenuStrip = taskCardMenuStrip;

			if (componentControl != null && section != null)
			{
				ContextMenuStrip = new LazyContextMenuStrip((strip, control) => componentControl.PopulateContextMenu(strip, section, section.Component, EditSchedulesThisCell_Click, OpenJobWorkflowsThisCell_Click, this), clearOnClose: false);
			}

			cell.ContentRefreshed += Cell_TasksShown;
			cell.BackgroundColorChanged += Cell_BackgroundColorChanged;
		}

#if !WINZOR

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;   //Turn on WS_EX_COMPOSITED This double buffers rendering and stops flickering.
				return cp;
			}
		}

#endif

		readonly KContextMenuStrip sharedMenuStrip;
		readonly TaskPanelLayoutStrategy layoutStrategy;
		protected readonly CellContent cell;
		readonly BMBoardSectionViewModel sectionViewModel;
		readonly ITaskCardRenderer cardRenderer;

#if DEBUG
		public Action PreProcessUpdateCardAction_ForTest;
#endif

#if WINZOR
		public override bool UseParentDivForLayout => true;
#endif

		IReadOnlyCollection<ICardContent> Cards
		{
			get
			{
				var grid = SectionViewModel.ComponentGrid;
				if (grid != null)
				{
					var allocationMap = grid.CardAllocationMap;
					if (allocationMap != null)
					{
						return allocationMap.GetCards(cell);
					}
				}

				return new ReadOnlyCollection<ICardContent>(new List<ICardContent>());
			}
		}

		protected internal KContextMenuStrip SharedMenuStrip
		{
			get { return sharedMenuStrip; }
		}

		public CellContent Cell
		{
			get { return cell; }
		}

		public BMBoardSectionViewModel SectionViewModel
		{
			get { return sectionViewModel; }
		}

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);
			if (isNotFinalizing)
			{
				cell.ContentRefreshed -= Cell_TasksShown;
				cell.BackgroundColorChanged -= Cell_BackgroundColorChanged;

				if (ContextMenuStrip != null)
				{
					ContextMenuStrip.Dispose();
				}
			}
		}

		internal bool HasTask(ICardContent content)
		{
			return TaskCards.Any(x => x.CardContent.TaskIdentifier == content.TaskIdentifier);
		}

		#region Resize

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			Invalidate();
		}

		#endregion

		#region RefreshTasks

		void Cell_TasksShown(object sender, ContentRefreshedEventArgs e)
		{
			var requiresRedraw = e.RequiresFullRefresh || SectionViewModel.FilterManager.AppliedFilters.Any(f => f.RequiresRedraw);
			SetupTasks(requiresRedraw);
		}

		#endregion

		#region Background

		void Cell_BackgroundColorChanged(object sender, EventArgs e)
		{
			this.BeginInvokeSafe(() =>
			{
				BackColor = cell.BackColor ?? Color.Empty;
			});
		}

#if !WINZOR

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			var fadeColor = cell.BackgroundFadeColor;
			if (fadeColor != null)
			{
				this.PaintGradientBackground(cell.BackColor.Value, fadeColor.Value, sectionViewModel.GradientAngle, e.Graphics);
			}
			else
			{
				base.OnPaintBackground(e);
			}

			if (Border != null)
			{
				ControlPaint.DrawBorder(e.Graphics, ClientRectangle, Border.BorderColor, Border.LeftThickness, Border.LeftBorderStyle,
					Border.BorderColor, Border.TopThickness, Border.TopBorderStyle,
					Border.BorderColor, Border.RightThickness, Border.RightBorderStyle,
					Border.BorderColor, Border.BottomThickness, Border.BottomBorderStyle);
			}
		}

#endif

		public BorderSpec Border
		{
			get
			{
				if (border == null && !string.IsNullOrEmpty(cell.BorderThickness))
				{
					border = new BorderSpec(cell.BorderThickness) { BorderColor = Color.Black };
					border.SetBorderStyles(ButtonBorderStyle.Solid);
				}
				return border;
			}
			set
			{
				border = value;
			}
		}
		BorderSpec border;

		#endregion

		#region SetupControls

		public void SetupTasks(bool disposeExistingTickets)
		{
			SuspendLayout();
			if (disposeExistingTickets)
			{
				foreach (var cardControl in Controls.OfType<TaskCardControl>())
				{
					cardControl.BackgroundImage = null;
				}

				Controls.RemoveAndDisposeAll();
			}

			ShowTasks();

			try
			{
				ShowTotalTasksButton();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("Error when creating total tasks button", ex);
			}
			ResumeLayout();
		}

		void ShowTasks()
		{
			var cardContents = SetupTaskCards();

			if (cellTasksControl != null)
			{
				cellTasksControl.SetupTasks(cardContents);
			}
		}

		ICardContent[] SetupTaskCards()
		{
			var sortedCardContents = this.SortCards(Cards, sectionViewModel);

			layoutStrategy.LayoutCards(sortedCardContents, this, new TaskControlFilterApplicator(SectionViewModel, Cell), cardRenderer);
			SetStackAppearance();

			return sortedCardContents;
		}

		public IEnumerable<TaskCardControl> TaskCards => Controls.OfType<TaskCardControl>();

		public const int TaskCardPadding = 3; // Here instead of this.Padding so the TotalTasksButton can be in the very top corner
		public const int TaskCardVerticalOffsetPixels = 10;
		public const int TaskCardMaxHorizontalOffsetPixels = 15;

		#endregion

		#region Total tasks

		void ShowTotalTasksButton()
		{
			if (totalTasksButton != null)
			{
				if (totalTasksButton.IsHandleCreated)
				{
					totalTasksButton.Dispose();
				}

				totalTasksButton = null;
			}

			var tasksCount = Cards.Where(card => sectionViewModel.ComponentGrid.FilterMap.IsVisible(Cell, card)).Count();

			if (tasksCount > 0)
			{
				totalTasksButton = CreateTotalTasksButton(tasksCount);

				Controls.Add(totalTasksButton);
				totalTasksButton.BringToFront();
				ControlDpiScalingHelper.SetLeft(ref totalTasksButton, this.Width - totalTasksButton.Width, false);
			}
		}

		ZButton CreateTotalTasksButton(int tasksCount)
		{
			var newTotalTasksButton = new ZButton();
			newTotalTasksButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
			newTotalTasksButton.AutoSize = true;
			newTotalTasksButton.ForeColor = Color.Black;
			newTotalTasksButton.Margin = Padding.Empty;
			newTotalTasksButton.Name = "TotalTasksButton";
			newTotalTasksButton.Padding = Padding.Empty;
			newTotalTasksButton.Size = ControlDpiScalingHelper.NewScaledSize(20, 20);
			newTotalTasksButton.Text = tasksCount.ToString(CultureInfo.CurrentCulture);
			newTotalTasksButton.TextAlign = ContentAlignment.MiddleCenter;
			newTotalTasksButton.UseVisualStyleBackColor = false;

			ControlDpiScalingHelper.SetTop(newTotalTasksButton, 0, true);

			newTotalTasksButton.Font = new Font(newTotalTasksButton.Font.FontFamily, 7f);
			newTotalTasksButton.Click += TotalTasksButton_Click;

			return newTotalTasksButton;
		}

		void TotalTasksButton_Click(object sender, EventArgs e)
		{
			var button = (Control)sender;
			var location = ControlDpiScalingHelper.NewScaledPoint(button.Left + button.Width, button.Top, false);
			ShowTasksFlyover(location);
		}

		internal void ShowTasksFlyover(Point location)
		{
			var form = (VisualBoardForm)this.FindForm();
			if (cellTasksControl == null)
			{
				cellTasksControl = ConstructCellTasksControl(this.SortCards(Cards, sectionViewModel).ToArray(), sectionViewModel);
				cellTasksControl.SetupTasks();
				cellTasksControl.Disposed += CellTasksControl_Disposed;
				form.AddOverlayControl(cellTasksControl, this.PointToScreen(location), Point.Empty, this.Width);
				if (ModifierKeysProvider.ModifierKeys != Keys.Control)
				{
					var currentlyShownCellTasksControl = form.CurrentlyShownCellTasksControl != null ? (IDisposable)form.CurrentlyShownCellTasksControl.Target : null;
					if (currentlyShownCellTasksControl != null && currentlyShownCellTasksControl != cellTasksControl)
					{
						currentlyShownCellTasksControl.Dispose();
						currentlyShownCellTasksControl = null;
					}

					form.CurrentlyShownCellTasksControl = new WeakReference(cellTasksControl);
				}
			}
			else
			{
				cellTasksControl.Dispose();
			}
		}

		protected virtual CellTasksControl ConstructCellTasksControl(IEnumerable<ICardContent> cardContents, BMBoardSectionViewModel viewModel)
		{
			return new CellTasksControl(cell, cardContents, viewModel, sharedMenuStrip);
		}

		void CellTasksControl_Disposed(object sender, EventArgs e)
		{
			var form = (VisualBoardForm)FindForm();
			var control = GetCurrentlyShownCellTasksControl(form);
			if (control != null && control == cellTasksControl)
			{
				control.Dispose();
				form.CurrentlyShownCellTasksControl = null;
			}
			if (cellTasksControl != null)
			{
				cellTasksControl.Disposed -= CellTasksControl_Disposed;
				cellTasksControl = null;
			}
		}

		static CellTasksControl GetCurrentlyShownCellTasksControl(VisualBoardForm form)
		{
			return form != null && form.CurrentlyShownCellTasksControl != null ? (CellTasksControl)form.CurrentlyShownCellTasksControl.Target : null;
		}

		CellTasksControl cellTasksControl;
		ZButton totalTasksButton;

		public IModifierKeysProvider ModifierKeysProvider
		{
			get { return modifierKeysProvider ?? (modifierKeysProvider = new ControlModifierKeysProvider()); }
			set { modifierKeysProvider = value; }
		}
		IModifierKeysProvider modifierKeysProvider;

		public interface IModifierKeysProvider
		{
			Keys ModifierKeys { get; }
		}

		class ControlModifierKeysProvider : IModifierKeysProvider
		{
			Keys IModifierKeysProvider.ModifierKeys
			{
				get { return Control.ModifierKeys; }
			}
		}

		#endregion

		#region Event Handlers

		public void EditSchedulesThisCell_Click(object sender, EventArgs e)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": EditSchedulesThisCell_Click" };
			var errorMessage = Res.GetString("e3713db2-f890-4c07-bd06-f00b80f2d0b1", "This cell has no workflows to edit schedules for.");
			BMComponentControl.EditSchedules(GetAllCellWorkflowPKs(), factory, errorMessage);
		}

		public void OpenJobWorkflowsThisCell_Click(object sender, EventArgs e)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ": OpenJobWorkflowsThisCell_Click" };
			var errorMessage = Res.GetString("f2cf3234-ca4d-4476-9d51-d37f6ba71eeb", "This cell has no workflows to open.");
			BMComponentControl.OpenJobWorkflows(GetAllCellWorkflowPKs(), factory, errorMessage);
		}

		IEnumerable<ZGuid> GetAllCellWorkflowPKs()
		{
			return Cards.Select(x => x.WorkflowIdentifier).ToHashSet();
		}

		#endregion

		#region ITasksControl

		CellContent ITasksControl.Cell
		{
			get { return cell; }
		}

		#endregion

		#region ITasksDetails

		int ITasksDetails.CardsCount
		{
			get { return Cards.Count; }
		}

		#endregion

		#region Layout

		internal void HandleClick(TaskCardControl taskCardControl)
		{
			layoutStrategy.HandleClick(taskCardControl, this);

			BringTotalTasksButtonToFront();
		}

		internal void HandleDragEnded()
		{
			BringTotalTasksButtonToFront();
		}

		void BringTotalTasksButtonToFront()
		{
			totalTasksButton?.BringToFront();
		}

		internal void SetStackAppearance()
		{
			foreach (var taskCard in TaskCards)
			{
				taskCard.SetCardAppearance();
			}
		}

		internal int CardHorizontalOffset { get; set; }

		#endregion
	}
}
