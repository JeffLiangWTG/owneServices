using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.CalendarArithmetic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ReleaseSchedulerCellTasksControl : CellTasksControl
	{
		#region Construction

		public static ReleaseSchedulerCellTasksControl GetCellTasksControl(CellContent cell, IEnumerable<ICardContent> cardContents, BMBoardSectionViewModel viewModel, KContextMenuStrip menuStrip)
		{
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(ReleaseSchedulerCellTasksControl) };

			cardContents = factory
				.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.PK, cardContents.Select(c => c.TaskIdentifier)))
				.Select(task => new WorkflowCardContent(task.GetProcessHeader(), task, viewModel));

			return new ReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, factory, menuStrip);
		}

#if DEBUG
		public
#endif
 ReleaseSchedulerCellTasksControl(CellContent cell, IEnumerable<ICardContent> cardContents, BMBoardSectionViewModel viewModel, BusinessObjectFactory factory, KContextMenuStrip menuStrip)
			: base(cell, cardContents, viewModel, menuStrip)
		{
			this.factory = factory;
			section = factory.Load<BMBoardSection>(viewModel.SectionPK);
			context = WorkingTimeContext.Create(section, factory.Load<GlbStaff>(cell.Channel.EntityPK));
			workTimeArithmetic = context.GetWorkTimeArithmetic(factory);

			ResetLocalTime();
			InitializeComponent();

			SaveButton.Visible = IsUnReleasedCell;
		}

		#endregion

		readonly BusinessObjectFactory factory;
		readonly BMBoardSection section;
		readonly WorkingTimeContext context;
		readonly IWorkTimeArithmetic workTimeArithmetic;
		readonly Dictionary<TaskCardControl, int> taskCardPositions = new Dictionary<TaskCardControl, int>();
		readonly HashSet<ZGuid> nudgedWorkflows = new HashSet<ZGuid>();

		#region OnCardsSetup

		Dictionary<ZGuid, int> startingPositions;

		protected override void OnCardsSetup()
		{
			base.OnCardsSetup();

			if (startingPositions == null)
			{
				startingPositions = taskCardPositions.ToDictionary(pair => pair.Key.CardContent.WorkflowIdentifier, pair => pair.Value);
			}
		}

		#endregion

		#region OnTaskCardAdded

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		decimal runningEstimateTotalHours;
		ZDateTime runningStartLocalTime;
		bool hasMarkedAsBeyondConstraintOffset;

		protected override CardLabel GetLabelForCard(ICardContent cardContent, bool showJobWorkflow)
		{
			var estimatesForThisWorkflow = GetEstimatesForWorkflow(cardContent, Cell.Channel, showJobWorkflow);

			runningEstimateTotalHours += estimatesForThisWorkflow;
			var totalHours = runningEstimateTotalHours;

			if (IsUnReleasedCell && !(Cell.Channel is NonConstrainedResourcesChannel))
			{
				var releasedCell = GetReleasedCellInChannel();
				if (releasedCell != null)
				{
					totalHours += ComponentGrid.GetTaskEstimatesInCells(ViewModel.ComponentGrid.CardAllocationMap, releasedCell.PrimaryAxis.WrapWithEnumerable(), releasedCell.SecondaryAxis.WrapWithEnumerable());
				}
			}

			var cardLabel = CreateRunningTotalText(estimatesForThisWorkflow, totalHours);
			cardLabel.Tooltip = CreateRunningTotalTooltip(cardLabel.Tooltip);

			return cardLabel;
		}

		[SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		CardLabel CreateRunningTotalText(decimal estimatesForThisWorkflow, decimal totalHours)
		{
			var time = string.Empty;
			var hasBeenMarked = false;

			if (!hasMarkedAsBeyondConstraintOffset && !(Cell.Channel is NonConstrainedResourcesChannel))
			{
				var constraint = section.Component.ChildComponents.OrderBy(c => c.FC_DisplaySequence).FirstOrDefault(c => c.IsConstraint);

				if (constraint != null && totalHours * 60 >= constraint.FC_OffsetInMinutes)
				{
					hasBeenMarked = hasMarkedAsBeyondConstraintOffset = true;

					time = constraint.FC_OffsetInMinutes >= 60
						? string.Format(CultureInfo.InvariantCulture, "{0} {1}", constraint.FC_OffsetInMinutes / 60, Res.GetString("e57f71d9-5343-4e5b-aad1-bdefee3530be", "hours"))
						: string.Format(CultureInfo.InvariantCulture, "{0} {1}", constraint.FC_OffsetInMinutes, Res.GetString("224399cf-494c-4da6-a389-d9fa6e14df90", "minutes"));
				}
			}

			var text = Res.GetString("fd1de744-f6f9-4ee5-9aca-2b9c502baf21", "Queue hours: {0}", TimeSpan.FromHours((double)totalHours).ToHoursAndMinutesString());

			if (IsUnReleasedCell)
			{
				var projectedCompletionTime = workTimeArithmetic.GetDateTimeInWorkingHoursFutureOrPast(runningStartLocalTime.ToDateTime(), (double)estimatesForThisWorkflow, checkStaffHolidays: true);
				runningStartLocalTime = projectedCompletionTime;

				text += System.Environment.NewLine + Res.GetString("a535280d-b52d-4b87-8ea5-ee1cadf51719", "Estimated completion: {0}", new ZDateTime(projectedCompletionTime).ToLongTimeString());
			}

			return new CardLabel { Label = text, Tooltip = time, UseDifferentColor = hasBeenMarked };
		}

		string CreateRunningTotalTooltip(string time)
		{
			return Res.GetString("7b5ef97d-efa1-4aad-875d-e7ac0de0fac0", "This is the first workflow beyond the constraint offset of {0}.", time);
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		protected override void OnTaskCardAdded(TaskCardControl taskCardControl, CardLabel labels)
		{
			base.OnTaskCardAdded(taskCardControl, labels);

			var position = taskCardPositions.Count;
			taskCardPositions[taskCardControl] = position;

			if (IsUnReleasedCell)
			{
#if !WINZOR
				var handler = DragDropHelper.AddDragDropSupport(taskCardControl, new DragDropDescriptor { AllowHorizontalDrag = false, AllowDragOutsideParentBounds = false });
				handler.Dragging += DragDropHandler_Dragging;
				handler.DragFinished += DragDropHandler_DragFinished;
#endif
			}

			var label = GetRunningTotalLabel(taskCardControl, labels.Label, labels.UseDifferentColor);
			TaskCardsPanel.Controls.Add(label);
			ControlDpiScalingHelper.SetLeft(ref label, (TaskCardsPanel.Width - label.Width) / 2, false);
			ToolTipService.SetToolTip(label, labels.Tooltip);

			if (startingPositions != null)
			{
				UpdateStartingPositions(taskCardControl, position);
			}
		}

		ZLabel GetRunningTotalLabel(TaskCardControl taskCardControl, string label, bool differentForeColor)
		{
			var runningTotalLabel = new ZLabel
			{
				Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
				AutoSize = true,
				Margin = Padding.Empty,
				Padding = Padding.Empty,
				Text = label,
				TextAlign = ContentAlignment.MiddleCenter
			};
			ControlDpiScalingHelper.SetTop(runningTotalLabel, taskCardControl.Top + taskCardControl.Height, false);

			if (differentForeColor)
			{
				runningTotalLabel.ForeColor = Color.Fuchsia;
			}

			return runningTotalLabel;
		}

		decimal GetEstimatesForWorkflow(ICardContent cardContent, IVisualBoardChannel primaryChannel, bool showJobWorkflow)
		{
			return (from t in cardContent.GetWorkflow(factory).Tasks
					where primaryChannel == null || primaryChannel.IsInChannel(t, showJobWorkflow)
					where t.P9_Status != ProcessTaskStatusCodeList.Codes.Closed
					where t.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled
					select t.RelevantEstimateHours).Sum(_ => _);
		}

		void UpdateStartingPositions(TaskCardControl taskCardControl, int position)
		{
			if (startingPositions.ContainsKey(taskCardControl.CardContent.WorkflowIdentifier))
			{
				var startingPosition = startingPositions[taskCardControl.CardContent.WorkflowIdentifier];

				Image image = null;

				if (position > startingPosition)
				{
					image = Properties.Resources.down_vote;
					hasChanges = true;
				}
				else if (position < startingPosition)
				{
					image = Properties.Resources.up_vote;
					hasChanges = true;
				}

				if (image != null)
				{
					GetAndAddOptimisticPictureBox(image, taskCardControl);
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		OptimisticPictureBox GetAndAddOptimisticPictureBox(Image image, TaskCardControl taskCardControl)
		{
			var newPictureBox = new OptimisticPictureBox(shouldDisposeImageOnControlDispose: false);
			try
			{
				newPictureBox.Image = image;
				newPictureBox.Size = image.Size;
				newPictureBox.Name = "VoteUpDownImage";
				ControlDpiScalingHelper.SetLeft(newPictureBox, taskCardControl.Width - image.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(CardPadding), false);
				ControlDpiScalingHelper.SetTop(newPictureBox, taskCardControl.Height - image.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(CardPadding), false);
				taskCardControl.Controls.Add(newPictureBox);
				return newPictureBox;
			}
			catch
			{
				try
				{
					newPictureBox.Dispose();
				}
				catch { }
				throw;
			}
		}

		void ResetLocalTime()
		{
			runningStartLocalTime = ZDateTime.UtcNow.ToLocalBranchTime(context.Branch);

			var releasedCell = GetReleasedCellInChannel();
			if (releasedCell != null)
			{
				var totalReleasedtime = ComponentGrid.GetTaskEstimatesInCells(ViewModel.ComponentGrid.CardAllocationMap, releasedCell.PrimaryAxis.WrapWithEnumerable(), releasedCell.SecondaryAxis.WrapWithEnumerable());
				runningStartLocalTime = workTimeArithmetic.GetDateTimeInWorkingHoursFutureOrPast(runningStartLocalTime.ToDateTime(), (double)totalReleasedtime, checkStaffHolidays: true);
			}
		}

		CellContent GetReleasedCellInChannel()
		{
			return ViewModel.ComponentGrid.Cells.FirstOrDefault(c => c.Channel == Cell.Channel && c.SecondaryChannel is ReleaseSchedulerReleasedChannel);
		}

		bool IsUnReleasedCell
		{
			get { return Cell.SecondaryChannel is ReleaseSchedulerUnReleasedChannel; }
		}

		#endregion

		#region Drag/Drop Sequencing

		bool hasDragged;

		void DragDropHandler_Dragging(object sender, MouseEventArgs e)
		{
			hasDragged = true;
		}

		void DragDropHandler_DragFinished(object sender, MouseEventArgs e)
		{
			if (hasDragged && e.Button == MouseButtons.Left)
			{
				try
				{
					OnDragDropFinished((TaskCardControl)sender);
				}
				finally
				{
					hasDragged = false;
				}
			}
		}

#if DEBUG
		public
#endif
 void OnDragDropFinished(TaskCardControl taskCard)
		{
			var oldIndex = taskCardPositions[taskCard];
			var newIndex = GetCardIndex(taskCard);
			var hasMoved = newIndex != oldIndex;

			if (hasMoved)
			{
				var sequence = taskCardPositions.Keys.Select(c => c.CardContent.GetWorkflow(factory)).ToArray();

				try
				{
					if (newIndex < oldIndex)
					{
						NudgeUpToPosition(sequence, oldIndex, newIndex);
					}
					else
					{
						NudgeDownToPosition(sequence, oldIndex, newIndex);
					}
				}
				catch (ProcessHeader.NudgeOutOfRangeException)
				{
					Globals.Message.Show(Res.GetString("bbd236cd-c570-49ef-9ffb-906ae6752275", "Cannot nudge outside of range ({0}, {1})", short.MinValue, short.MaxValue));
				}

				var sortedWorkflows = WorkflowTransferOrderSorter.Sort(AllCardContents.Select(c => c.GetWorkflow(factory))).Select(c => c.PK).ToArray();
				AllCardContents = AllCardContents.OrderBy(c => Array.IndexOf(sortedWorkflows, c.WorkflowIdentifier)).ToArray();
			}

			taskCardPositions.Clear();
			runningEstimateTotalHours = 0m;
			ResetLocalTime();
			hasMarkedAsBeyondConstraintOffset = false;

			hasChanges = false;

			SetupTasks(AllCardContents);
		}

		int GetCardIndex(TaskCardControl taskCard)
		{
			var taskCards = taskCard.Parent.Controls.OfType<TaskCardControl>()
				.OrderBy(c => c.Top)
				.ThenByDescending(c => c == taskCard)
				.Select((card, index) => new { TaskCard = card, Index = index });

			return taskCards.First(t => t.TaskCard == taskCard).Index;
		}

		void NudgeDownToPosition(ProcessHeader[] sequence, int oldIndex, int newIndex)
		{
			var workflow = sequence[oldIndex];
			do
			{
				sequence = WorkflowTransferOrderSorter.Sort(sequence).ToArray();

				if (Array.IndexOf(sequence, workflow) == newIndex)
				{
					break;
				}

				for (int i = 0; i <= newIndex; i++)
				{
					var currentWorkflow = sequence[i];

					if (currentWorkflow != workflow)
					{
						currentWorkflow.NudgeUp(false);

						if (!nudgedWorkflows.Contains(currentWorkflow.PK))
						{
							nudgedWorkflows.Add(currentWorkflow.PK);
						}
					}
				}
			} while (true);
		}

		void NudgeUpToPosition(ProcessHeader[] sequence, int oldIndex, int newIndex)
		{
			var workflow = sequence[oldIndex];
			do
			{
				sequence = WorkflowTransferOrderSorter.Sort(sequence).ToArray();

				if (Array.IndexOf(sequence, workflow) == newIndex)
				{
					break;
				}

				workflow.NudgeUp(false);

				if (!nudgedWorkflows.Contains(workflow.PK))
				{
					nudgedWorkflows.Add(workflow.PK);
				}

				for (int i = 0; i < newIndex; i++)
				{
					var currentWorkflow = sequence[i];
					currentWorkflow.NudgeUp(false);

					if (!nudgedWorkflows.Contains(currentWorkflow.PK))
					{
						nudgedWorkflows.Add(currentWorkflow.PK);
					}
				}
			} while (true);
		}

		#endregion

		#region Save

		bool hasChanges;

		void SaveButton_Click(object sender, EventArgs e)
		{
			if (hasChanges)
			{
				Save();
			}
		}

#if DEBUG
		public
#endif
 void Save()
		{
			factory.SaveHandlingZSaveExceptions();

			if (!IsDisposed)
			{
				Dispose();
			}

			ViewModel.RefreshAll(new WorkflowUpdatedOperation(Array.Empty<ZGuid>(), nudgedWorkflows.ToArray(), new BusinessObjectFactory { NameForDebugging = "Release Scheduler Refreshed" }));
		}

		#endregion

		#region Close

		protected override void Close()
		{
			if (hasChanges)
			{
				var caption = Res.GetString("53629407-f0ad-4da0-b7ec-59f55976bdf3", "Unsaved Changes");
				var message = Res.GetString("96a6b9ab-be90-4669-9ad5-a00c6fc8b28c", "There are unsaved sequencing changes. Would you like to save the changes?");
				var result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNoCancel, DialogResult.No);

				if (result == DialogResult.Yes)
				{
					Save();
				}
				else if (result == DialogResult.No)
				{
					base.Close();
				}
			}
			else
			{
				base.Close();
			}
		}

		#endregion
	}
}
