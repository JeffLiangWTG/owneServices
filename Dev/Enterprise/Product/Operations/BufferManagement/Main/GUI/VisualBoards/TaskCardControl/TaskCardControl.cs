using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TaskCardControl : ZUserControl
	{
#if DEBUG
		public TaskCardControl(ProcessTask task, BMBoardSectionViewModel viewModel)
			: this(CreateMenuStrip(), new TaskCardContent(task, viewModel), viewModel, new CellContent(0, 0, CellContentType.Cards), shouldEnableDragDrop: false, canUseBitmapCache: true)
		{
			shouldDisposeMenuStrip = true;
		}

		public TaskCardControl(ICardContent taskCardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool canUseBitmapCache)
			: this(CreateMenuStrip(), taskCardContent, viewModel, cell, true, canUseBitmapCache)
		{
			shouldDisposeMenuStrip = true;
		}
#endif

		public TaskCardControl(KContextMenuStrip menuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool shouldEnableDragDrop, bool canUseBitmapCache)
			: this(menuStrip, cardContent, viewModel, cell, shouldEnableDragDrop, canUseBitmapCache, renderAsBitmapOnLoad: true, model: null, bitmaps: GetBitmaps(canUseBitmapCache, viewModel, cardContent))
		{
		}

		public TaskCardControl(KContextMenuStrip menuStrip, ICardContent cardContent, BMBoardSectionViewModel viewModel, CellContent cell, bool shouldEnableDragDrop, bool canUseBitmapCache, bool renderAsBitmapOnLoad, ControlCustomisationViewModel model, CardBitmaps bitmaps)
		{
			Argument.NotNull(menuStrip, "menuStrip");
			Argument.NotNull(cardContent, "cardContent");

			InitializeComponent();
#if WINZOR
			borderPanel = new ZPanel();
			WinzorSpecificControls.Add(borderPanel);
#endif

			this.cardContent = cardContent;
			this.cell = cell;
			this.viewModel = viewModel;
			this.DragDropEnabled = shouldEnableDragDrop;

#if !WINZOR

			this.renderAsBitmapOnLoad = renderAsBitmapOnLoad;
			this.Load += TaskCardControl_Load;
			this.canUseBitmapCache = canUseBitmapCache;

			if (bitmaps != null)
			{
				Size = bitmaps.NormalBitmap.Size;
				SetBackgroundImageAndDisposeControls(bitmaps.NormalBitmap, bitmaps);
			}

#endif

			if (BackgroundImage == null)
			{
				var customisationViewModel = model ?? new ControlCustomisationViewModel(viewModel.GetSummaryCard(cardContent.WorkflowType), viewModel, cardContent, cell);
				var customisedControl = CustomisedControlRenderer.Render(customisationViewModel);
				customisedControl.SetDataBinding(cardContent.Bindable, string.Empty);
				customisedControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
				customisedControl.BackColor = Color.Transparent;

				BackColor = GetBackgroundColor(customisationViewModel);
				var noteText = cardContent.NoteText;
				Size = ControlDpiScalingHelper.NewScaledSize(customisedControl.Size.Width, customisedControl.Size.Height + (noteText.IsEmpty ? 0 : NoteTextPanel.Height), false);

				Controls.Add(customisedControl);

#if WINZOR
				var maxZIndexForBorderPanel = 0;
				foreach (var control in customisedControl.Controls)
				{
					if (control is TextBox or ZDateEdit)
					{
						control.Enabled = false;
					}

					var customizationLine = control.Tag as ControlCustomisationBase;
					if (customizationLine is not null && customizationLine.BringToFront)
					{
						if (maxZIndexForBorderPanel == 0)
						{
							maxZIndexForBorderPanel = control.ZIndex;
						}
						else
						{
							maxZIndexForBorderPanel = Math.Min(maxZIndexForBorderPanel, control.ZIndex);
						}
					}
					maxZIndexForBorderPanel = Math.Max(maxZIndexForBorderPanel, control.ZIndex);
				}

				borderPanel.ZIndex = maxZIndexForBorderPanel;
				NoteTextLabel.AllowOverlap(customisedControl);
				NoteTextPanel.AllowOverlap(borderPanel);
#endif

				ControlDpiScalingHelper.SetTop(ref NoteTextPanel, customisedControl.Height, false);
				NoteTextLabel.Text = noteText;
			}

			if (DragDropEnabled)
			{
				var handler = DragDropHelper.AddDragDropSupport(this);
				handler.DragStarting += DragDropHandler_DragStarting;
				handler.Dragging += DragDropHandler_Dragging;
				handler.DragFinished += DragDropHandler_DragFinished;
			}

			SetupMenuItems(menuStrip);

			SetTagTooltip();

			SetBorderStyle(cardContent);
		}
#if WINZOR
		public ZPanel borderPanel;
#endif
		public bool DragDropEnabled { get; }

#if !WINZOR

		readonly bool canUseBitmapCache;
		readonly bool renderAsBitmapOnLoad;

		CardBitmaps GetBitmaps()
		{
			return GetBitmaps(canUseBitmapCache, viewModel, cardContent);
		}

		static CardBitmaps GetBitmaps(bool canUseBitmapCache, BMBoardSectionViewModel viewModel, ICardContent cardContent)
		{
			return canUseBitmapCache ? viewModel.GetCardBitmaps(cardContent) : null;
		}

#else

		static CardBitmaps GetBitmaps(bool canUseBitmapCache, BMBoardSectionViewModel viewModel, ICardContent cardContent) => null;

#endif

		#region For Test
#if DEBUG

		public void OnMouseHover_ForTest()
		{
			base.OnMouseHover(EventArgs.Empty);
		}

		public void MoveToComponent_ForTest(BMComponent component)
		{
			var factory = component.Factory;
			var workflow = CardContent.GetWorkflow(factory);

			TaskCardMenuActionProvider.MoveToComponent(component, workflow, CardContent, factory, viewModel);
		}

#if !WINZOR

		public CardBitmaps CreateCardBitmaps_ForTest(ICardContent content, bool isTemplateTaskCard) => CreateCardBitmaps(content, isTemplateTaskCard);

#endif

#endif
		#endregion

		public ICardContent CardContent
		{
			get { return cardContent; }
		}
		readonly ICardContent cardContent;

		public CellContent Cell
		{
			get { return cell; }
		}
		readonly CellContent cell;

		readonly BMBoardSectionViewModel viewModel;

		internal BMBoardSectionViewModel ViewModel
		{
			get { return viewModel; }
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				components.Dispose();
#if !WINZOR
				BackgroundBitmaps = null;
				BackgroundImage = null;
#endif
#if DEBUG
				if (ContextMenuStrip != null && shouldDisposeMenuStrip)
				{
					ContextMenuStrip.Dispose();
				}
#endif
				ContextMenuStrip = null;
			}
		}

#if DEBUG
		readonly bool shouldDisposeMenuStrip;
#endif

		#endregion

		#region Event handlers

		internal void ReloadAndRedraw(ProcessTask[] tasks)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = GetType().Name + ".ReloadAndRedraw" };

			var workflow = cardContent.GetWorkflow(factory);

			var processHeadersToReload = workflow != null ? new[] { workflow }
					.Concat(workflow.GetWorkflowParents())
					.Concat(workflow.PostrequisiteWorkflows)
					.ToArray() : Enumerable.Empty<ProcessHeader>();

			var taskPKs = Array.Empty<ZGuid>();
			var workflowPKs = processHeadersToReload.Select(p => p.PK).Concat(tasks.Select(t => t.P9_FH_ProcessHeader)).Distinct().ToArray();

			viewModel.RefreshAll(new WorkflowUpdatedOperation(taskPKs, workflowPKs, new[] { Cell }, factory));
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				OnTaskCardControlClicked();
			}
#if !WINZOR
			base.OnMouseDown(e);
#endif
		}

		public void OnTaskCardControlClicked()
		{
			if (Parent != null)
			{
				if (!IsFrontMostCard)
				{
					OnTaskCardControlClicked_PropagateToPanel(this);
				}
				else
				{
					if (IsDisposed)
					{
						var taskCard = Parent.Controls.OfType<TaskCardControl>().FirstOrDefault(card => card.CardContent.TaskIdentifier == CardContent.TaskIdentifier);
						if (taskCard != null)
						{
							taskCard.CloseDetailedCardIfOpen();
							OnTaskCardControlClicked_PropagateToPanel(taskCard);
						}
					}
					else
					{
						ToggleDetailedCard();
					}
				}
			}
		}

		void OnTaskCardControlClicked_PropagateToPanel(TaskCardControl card)
		{
			var panel = Parent as TaskPanel;
			if (panel != null)
			{
				panel.HandleClick(card);
			}
		}

		void OnDragDropEnded_PropagateToPanel()
		{
			var panel = Parent as TaskPanel;
			if (panel != null)
			{
				panel.HandleDragEnded();
			}
		}

		void TaskCardControl_DoubleClick(object sender, EventArgs e)
		{
			OnDoubleClick();
		}

		public void OnDoubleClick()
		{
			ShowParentWorkflow();
		}

		public void ShowParentWorkflow()
		{
			CardLoadResult result;
			if (TryLoadCardBizosDiscardingCardOnFailure(out result))
			{
				if (viewModel.ShowJobCards)
				{
					WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(result.Workflow);
				}
				else
				{
					WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(result.Task, shouldShowTaskFormIfNullParent: true, shouldNavigateToWorkflowIfNullParent: viewModel.ShowWorkflowCards);
				}
			}
		}

#if !WINZOR

		void TaskCardControl_Load(object sender, EventArgs e)
		{
			if (BackgroundImage == null && renderAsBitmapOnLoad)
			{
				var bitmaps = GetOrCreateCardBitmaps();

				if (bitmaps != null)
				{
					SetBackgroundImageAndDisposeControls(bitmaps.NormalBitmap, bitmaps);
				}
			}
		}

#endif

#endregion

		#region Drag and drop

		void DragDropHandler_DragStarting(object sender, MouseEventArgs e)
		{
			OnDragDropStarting();
		}

#if DEBUG
		public
#endif
		void OnDragDropStarting()
		{
			dragStartingLocation = Location;
#if WINZOR
			if (detailedCard != null)
			{
				detailedCard.Close();
			}
#endif
		}

		void DragDropHandler_Dragging(object sender, MouseEventArgs e)
		{
			OnDragging(e);
		}

#if DEBUG
		public
#endif
		void OnDragging(MouseEventArgs e)
		{
			var form = FindForm() as VisualBoardForm;
			if (form != null && Parent is TaskPanel && IsOutsideParentPanel())
			{
				dragStartingPanel = (TaskPanel)Parent;
				Parent.Controls.Remove(this);
				form.Controls.Add(this);
				this.BringToFront();
			}

			var hasMouseMoved = lastMouseLocation != null && lastMouseLocation.Value != e.Location;
			if (hasMouseMoved && detailedCard != null)
			{
				detailedCard.Close();
			}
			lastMouseLocation = e.Location;
		}

		bool IsOutsideParentPanel()
		{
			var topLeftCorner = Location;
			var bottomRightCorner = ControlDpiScalingHelper.NewScaledPoint(Left + Width, Top + Height, false);

			return !Parent.DisplayRectangle.Contains(topLeftCorner) && !Parent.DisplayRectangle.Contains(bottomRightCorner);
		}

		Point dragStartingLocation;
		TaskPanel dragStartingPanel;
		Point? lastMouseLocation;

		void DragDropHandler_DragFinished(object sender, MouseEventArgs e)
		{
			OnDragDropFinished();
			lastMouseLocation = null;
		}

#if DEBUG
		public
#endif
		void OnDragDropFinished()
		{
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(TaskCardControl) + ".OnDragDropFinished" };
			var task = cardContent.GetTask(factory);
			var form = Parent as VisualBoardForm;

			if (task != null && form != null)
			{
				if (!form.PushToNewSection(this))
				{
					if (!dragStartingPanel.HasTask(CardContent))
					{
						dragStartingPanel.Controls.Add(this);
						BringToFront();
						Location = dragStartingLocation;
					}
					else
					{
						Dispose();
					}
				}
			}

			if (Location != dragStartingLocation)
			{
				if (detailedCard != null)
				{
					detailedCard.Close();
				}
				overrideShowingDetailedCard = true;
			}
			dragStartingLocation = Point.Empty;

			OnDragDropEnded_PropagateToPanel();
		}

		bool overrideShowingDetailedCard;

		#endregion

		#region Bitmap Rendering

#if !WINZOR

		internal CardBitmaps GetOrCreateCardBitmaps()
		{
			CardBitmaps BitmapCreator() => CreateCardBitmaps(CardContent, isTemplateTaskCard: false);

			if (canUseBitmapCache)
			{
				return ViewModel.GetOrCreateCardBitmaps(CardContent, BitmapCreator);
			}
			else
			{
				return BitmapCreator();
			}
		}

		internal CardBitmaps CreateCardBitmaps(ICardContent content, bool isTemplateTaskCard)
		{
			var normalBitmap = TicketBitmapHelper.GetRenderedImage(this, content, ViewModel, isTemplateTaskCard);
			var darkenedBitmap = normalBitmap != null ? TicketBitmapHelper.GetDarkenedImage(normalBitmap, content, ViewModel) : null;

			if (darkenedBitmap != null)
			{
				return new CardBitmaps(normalBitmap, darkenedBitmap);
			}

			return null;
		}

		void SetBackgroundImageAndDisposeControls(Image image, CardBitmaps referencedBitmaps)
		{
			Argument.NotNull(image, nameof(image));
			Argument.NotNull(referencedBitmaps, nameof(referencedBitmaps));

			hasBeenBitmapped = true;
			BackgroundBitmaps = referencedBitmaps;
			BackgroundImage = image;
			Controls.RemoveAndDisposeAll();
		}

		public CardBitmaps BackgroundBitmaps { get; private set; }

		internal void SetCardAppearance()
		{
			var bitmaps = GetBitmaps();

			if (bitmaps != null)
			{
				if (IsFrontMostCard)
				{
					SetBackgroundImageAndDisposeControls(bitmaps.NormalBitmap, bitmaps);
				}
				else
				{
					SetBackgroundImageAndDisposeControls(bitmaps.DarkenedBitmap, bitmaps);
				}
			}
		}

#endif

#if WINZOR
		internal void SetCardAppearance()
		{
			NotifyRenderRequired();
		}
#endif

		protected virtual bool IsFrontMostCard
		{
			get
			{
				var taskPanel = Parent as TaskPanel;
				if (taskPanel == null || taskPanel.TaskCards.FirstOrDefault(t => t.Visible) == this)
				{
					return true;
				}
				else
				{
					var horizontalOffset = taskPanel.CardHorizontalOffset;
					return Math.Abs(horizontalOffset - this.Width) < ControlDpiScalingHelper.ScaleToCurrentDpiX(AllowableOverlappingPixelsBeforeDarkeningCards);
				}
			}
		}

		internal bool AreTicketsInWorkflowHighlighted
		{
			get
			{
				return viewModel.FilterManager.AppliedAndInheritedFilters
					.OfType<HighlightTaskCardsInSameWorkflowFilter>()
					.Any(f => f.GetCellVisibilityApplicator(viewModel)(CardContent, Cell));
			}
		}

		const int AllowableOverlappingPixelsBeforeDarkeningCards = 5;

		Color GetBackgroundColor(ControlCustomisationViewModel customisationViewModel)
		{
			var tagBackgroundColor = TagProvider.GetBackgroundColor(CardContent.Definitions, CardContent.ApplicableTagMagnitudes);

			if (tagBackgroundColor.HasValue)
			{
				return tagBackgroundColor.Value;
			}
			else
			{
				return customisationViewModel.Customisation.BackgroundColorValue;
			}
		}

		#endregion

		#region Detailed card

		internal void ToggleDetailedCard()
		{
			if (detailedCard == null)
			{
				ShowDetailedCard();
			}
			else
			{
				CloseDetailedCardIfOpen();
			}
		}

#if DEBUG
		public
#endif
		void ShowDetailedCard()
		{
			CardLoadResult result;

			if (overrideShowingDetailedCard)
			{
				overrideShowingDetailedCard = false;
			}
			else if (detailedCard == null && TryLoadCardBizosDiscardingCardOnFailure(out result, viewModel.FactoryProvider.GetNewEditFactory("TaskCardDetailControl"))) // Its a Name for Debugging.
			{
				var form = (VisualBoardForm)this.FindForm();
				if (form != null)
				{
					foreach (var openCard in form.Controls.OfType<TaskCardDetailControl>().ToArray())
					{
						openCard.Dispose();
						form.Controls.Remove(openCard);
					}

					detailedCard = new TaskCardDetailControl(result.Task, result.Workflow, this, viewModel.GetDetailedCard(cardContent.WorkflowType), viewModel);
					detailedCard.Disposed += DetailedCard_Disposed;

					var relativeStartingLocation = ControlDpiScalingHelper.NewScaledPoint(Width, Height / 3, false);
					form.AddOverlayControl(detailedCard, PointToScreen(detailedCard.Location), relativeStartingLocation, relativeStartingLocation.X);
				}
			}
		}

		internal bool TryLoadCardBizosDiscardingCardOnFailure(out CardLoadResult result, BusinessObjectFactory factory = null)
		{
			var content = CardContent;
			result = CardBusinessObjectLoader.Load(content, viewModel, factory);

			if (result.LoadSucceeded)
			{
				return true;
			}
			else
			{
				var caption = Res.GetString("200b6142-ed82-4c74-b697-24c6d5dda6d8", "This card will now be removed.");
				var fullMessage = result.FailureMessage;
				Globals.Message.Show(fullMessage, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning, DialogResult.OK);
				Dispose();

				viewModel.RefreshAll(new WorkflowUpdatedOperation(new[] { content.TaskIdentifier }, new[] { content.WorkflowIdentifier }, result.Factory));

				return false;
			}
		}

		void DetailedCard_Disposed(object sender, EventArgs e)
		{
			if (detailedCard != null)
			{
				detailedCard.Disposed -= DetailedCard_Disposed;
				detailedCard = null;
			}
		}

		internal void CloseDetailedCardIfOpen()
		{
			if (detailedCard != null)
			{
				detailedCard.Close();
			}
		}

		TaskCardDetailControl detailedCard;

		#endregion

		#region Show Task Form

		internal void ShowTaskDetailsForm(ProcessTask editedTask)
		{
			MainThreadRunner.RunOnMainThread(() =>
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "TaskCardControl ShowTaskDetailsForm Factory" };
				var loadedTask = factory.Load<ProcessTask>(editedTask.PK);
				if (loadedTask != null)
				{
					taskDetailsForm = ZControllerFactory.Create(ControllerIDs.ProcessTasks).ShowEditForm(loadedTask) as KForm;
					if (taskDetailsForm != null)
					{
						taskDetailsForm.BringToFront();
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("2D010102-F0D5-48B7-8761-A0EC21590167", "Task '{0}', has been deleted.", editedTask.P9_Description));
				}
			});
		}

#if DEBUG
		public
#endif
		KForm taskDetailsForm;

		#endregion

		#region Tags

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void SetTagTooltip()
		{
			var distinctMagnitudes = cardContent.ApplicableTagMagnitudes;
			var magnitudesCache = cardContent.Definitions.MagnitudesCache;

			if (distinctMagnitudes.Count > 0)
			{
				var message = Res.GetString("83f3dfbf-afb0-4ce1-b5a7-a2a5ecd14aec", "Applied tags:{0}  {1}",
					/*0*/ System.Environment.NewLine,
					/*1*/ string.Join(System.Environment.NewLine + "  ", distinctMagnitudes.Select(key => magnitudesCache.GetValueSafe(key)).WhereNotNull().Select(m => m.DisplayText).OrderBy(m => m)));

				ToolTipService.SetToolTip(this, message);
			}
		}

		#endregion

		#region OnPaint		

#if !WINZOR

		bool hasBeenBitmapped;

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (BackgroundImage != null && BackgroundImage.IsDisposed())
			{
				BackgroundImage = null;
			}

			base.OnPaintBackground(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			if (!hasBeenBitmapped)
			{
				PaintBorder(e.Graphics, ClientRectangle, borderStyle, borderColor);
			}
		}

		static void PaintBorder(Graphics graphics, Rectangle rect, SizedButtonBorderStyle borderStyle, Color borderColor)
		{
			var borderWidth = GetBorderWidth(borderStyle);

			switch (borderStyle.BorderStyle)
			{
				case VisualBoardButtonBorderStyle.Dashed:
					DrawRectangle(graphics, borderColor, borderWidth, rect, new float[] { 4, 2 });
					break;

				case VisualBoardButtonBorderStyle.Dotted:
					DrawRectangle(graphics, borderColor, borderWidth, rect, new float[] { 2, 2 });
					break;

				default:
					var buttonBorderStyle = (ButtonBorderStyle)borderStyle.BorderStyle;
					ControlPaint.DrawBorder(graphics, rect,
							borderColor, borderWidth, buttonBorderStyle,
							borderColor, borderWidth, buttonBorderStyle,
							borderColor, borderWidth, buttonBorderStyle,
							borderColor, borderWidth, buttonBorderStyle);
					break;
			}
		}

#endif

		internal static int GetBorderWidth(SizedButtonBorderStyle borderStyle)
		{
			return ControlDpiScalingHelper.ScaleToCurrentDpiX(borderStyle.IsLarge ? LargeBorderWidth : SmallBorderWidth);
		}

#if !WINZOR

		static void DrawRectangle(Graphics graphics, Color borderColor, int borderWidth, Rectangle rect, float[] dashPattern)
		{
			using (var pen = new Pen(borderColor, borderWidth))
			{
				pen.DashPattern = dashPattern;
				var offset = borderWidth / 2;
				rect = ControlDpiScalingHelper.NewScaledRectangle(offset, offset, rect.Width - borderWidth, rect.Height - borderWidth, false);
				graphics.DrawRectangle(pen, rect);
			}
		}

#endif

		const int SmallBorderWidth = 1;
		const int LargeBorderWidth = 3;

		internal void SetBorderStyle(ICardContent content)
		{
			borderStyle = content.BorderStyle;
			borderColor = content.BorderColor;
		}

		SizedButtonBorderStyle borderStyle;
		Color borderColor;

		#endregion
	}
}
