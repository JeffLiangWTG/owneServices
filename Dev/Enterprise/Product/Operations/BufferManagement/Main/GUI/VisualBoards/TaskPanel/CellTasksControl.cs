using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class CellTasksControl : ZUserControl, ITasksControl
	{
		public CellTasksControl()
		{
			InitializeComponent();
		}

		public CellTasksControl(CellContent cell, IEnumerable<ICardContent> cardContents, BMBoardSectionViewModel viewModel, KContextMenuStrip taskMenuStrip)
		{
			InitializeComponent();

			this.ChannelDayHeaderLabel.Text = cell.Label;
			this.cell = cell;
			AllCardContents = cardContents.ToArray();
			this.viewModel = viewModel;
			sharedMenuStrip = taskMenuStrip;

#if !WINZOR
			DragDropHelper.AddDragDropSupport(this, new DragDropDescriptor { AllowDragOutsideParentBounds = false, ControlTypesToIgnore = new[] { typeof(TaskCardControl) } });
#endif
		}

		readonly KContextMenuStrip sharedMenuStrip;
		readonly CellContent cell;
		readonly BMBoardSectionViewModel viewModel;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected ICardContent[] AllCardContents { get; set; }

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		protected CardContentAndCardLabel[] CardContents { get; set; }

		public CellContent Cell
		{
			get { return cell; }
		}

		protected BMBoardSectionViewModel ViewModel
		{
			get { return viewModel; }
		}

		#region SetupTasks

		void ITasksControl.SetupTasks(bool disposeExistingTickets) => SetupTasks();

		public void SetupTasks()
		{
			SetupTasksCore();
		}

		public void SetupTasks(IEnumerable<ICardContent> cardContents)
		{
			if (cardContents != null)
			{
				AllCardContents = cardContents.ToArray();
			}

			SetupTasksCore();
		}

		void SetupTasksCore()
		{
			var startingScrollPosition = TaskCardsPanel.AutoScrollPosition;

			TaskCardsPanel.Controls.RemoveAndDisposeAll();

			GenerateFetchHint(AllCardContents);

			var cards = this.SortCards(AllCardContents, ViewModel);
			var detailedCards = GenerateExtraDetailForCards(cards, ViewModel.ShowJobCards);

			var filterMap = ViewModel.ComponentGrid.FilterMap;
			CardContents = detailedCards.Where(card => filterMap == null || filterMap.IsVisible(Cell, card.CardContent)).Take(BMConstants.NumberOfCardsToShow).ToArray();

			if (CardContents.Length == 0)
			{
				Close();
			}
			else
			{
				Control previousControl = null;
				for (int i = 0; i < CardContents.Length; i++)
				{
					int top = 0;
					if (previousControl != null && previousControl.Visible)
					{
						top = previousControl.Height + previousControl.Top + ControlDpiScalingHelper.ScaleToCurrentDpiY(CardPadding);
					}

					var cardContent = CardContents[i].CardContent;
					var control = new NonReorderingTaskCardControl(sharedMenuStrip, cardContent, viewModel, cell, canUseBitmapCache: false)
					{
						Location = ControlDpiScalingHelper.NewScaledPoint(0, top, false),
					};
					ControlDpiScalingHelper.SetWidth(control, CloseTaskCardButton.Left + CloseTaskCardButton.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(CardPadding), false);

					if (control.Visible)
					{
						TaskCardsPanel.Controls.Add(control);
						OnTaskCardAdded(control, CardContents[i].CardLabel);
					}
					else
					{
						control.Dispose();
					}

					previousControl = TaskCardsPanel.Controls.OfType<Control>().LastOrDefault();
				}

				if (previousControl != null)
				{
					var activeForm = FindForm() ?? Form.ActiveForm;
					var maxHeight = activeForm != null ? activeForm.ClientSize.Height * 7 / 8 : this.Width;
					var proposedHeight = previousControl.Top + previousControl.Height + ControlDpiScalingHelper.ScaleToCurrentDpiX(CardPadding) + TaskCardsPanel.Top;

					if (!hasRemovedScrollbar && proposedHeight <= maxHeight)
					{
						ControlDpiScalingHelper.SetWidth(this, this.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(15), false);
						hasRemovedScrollbar = true;
					}
					ControlDpiScalingHelper.SetHeight(this, Math.Min(maxHeight, proposedHeight), false);
				}

				if (cards.Any())
				{
					TaskCardsPanel.AutoScrollPosition = ControlDpiScalingHelper.NewScaledPoint(Math.Abs(startingScrollPosition.X), Math.Abs(startingScrollPosition.Y), false);

					OnCardsSetup();
				}
				else
				{
					Close();
				}
			}
		}

		void GenerateFetchHint<T>(IEnumerable<T> items)
		{
			var workflowCardContents = items.OfType<IBizoCardContent>();

			var headers = new List<ProcessHeader>();
			headers.AddRange(workflowCardContents.Select(x => x.Workflow));

			var factory = headers.FirstOrDefault()?.Factory;

			if (factory != null)
			{
				var processHeaderPKs = headers.Select(s => s.PK);

				ProcessHeaderLink.LoadLinksIntoFactoryWithoutOptimisingForUnsavedObjects(factory, processHeaderPKs);

				var headerTagLinkQuery = new ZQuery(TagLinkSchema.TGL_ParentId, processHeaderPKs);
				factory.AddFetchHint(TagLinkSchema.Instance, headerTagLinkQuery);

				var tagLinkQuery = new ZQuery(TagLinkSchema.TGL_ParentId, workflowCardContents.Select(x => x.Task).Select(s => s.PK));
				factory.AddFetchHint(TagLinkSchema.Instance, tagLinkQuery);
			}
		}
		IEnumerable<CardContentAndCardLabel> GenerateExtraDetailForCards(IEnumerable<ICardContent> cards, bool showJobWorkflow)
		{
			foreach (var card in cards)
			{
				yield return new CardContentAndCardLabel { CardContent = card, CardLabel = GetLabelForCard(card, showJobWorkflow) };
			}
		}

		bool hasRemovedScrollbar;

		public const int CardPadding = 5;

		protected virtual CardLabel GetLabelForCard(ICardContent cardContent, bool showJobWorkflow)
		{
			return new CardLabel();
		}

		protected virtual void OnTaskCardAdded(TaskCardControl taskCardControl, CardLabel labels)
		{
		}

		protected virtual void OnCardsSetup()
		{
#if DEBUG
			SetupCardsCalledCount++;
#endif
		}

#if DEBUG
		public int SetupCardsCalledCount { get; private set; }
#endif

		#endregion

		#region Close

		void CloseTaskCardButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected virtual void Close()
		{
			if (Parent != null)
			{
				Parent.Controls.Remove(this);
			}
			Dispose();
		}

		#endregion

		protected class CardLabel
		{
			public string Label { get; set; }
			public string Tooltip { get; set; }
			public bool UseDifferentColor { get; set; }
		}

		protected class CardContentAndCardLabel
		{
			public ICardContent CardContent { get; set; }
			public CardLabel CardLabel { get; set; }
		}

		#region ITasksControl

		CellContent ITasksControl.Cell
		{
			get { return cell; }
		}

		#endregion
	}
}
