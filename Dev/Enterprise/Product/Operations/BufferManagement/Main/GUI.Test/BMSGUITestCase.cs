using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	/// <summary>
	/// Methods to help test Visual Boards, and Visual Board sub-components
	/// </summary>
	public abstract class BMSGUITestCase : BMSGUITestCaseWithFactory
	{
		#region Assertions

		public static void AssertExpandedChannel(VisualBoardForm form, BMBoardSection sectionContainingExpectedExpandedChannel, GlbStaff expectedExpandedChannelResource)
		{
			Application.DoEvents();

			CombineAssertions(() =>
			{
				foreach (var control in form.FindAll<BMComponentControl>())
				{
					var section = sectionContainingExpectedExpandedChannel.Factory.Load<BMBoardSection>(control.ViewModel.SectionPK);
					var numHeadingColumns = section.SectionConfiguration.ShowZones ? 2 : section.SectionConfiguration.IsBucket ? 0 : 1;

					var channelIndex = control.ViewModel.SectionPK == sectionContainingExpectedExpandedChannel.PK
						? control.ViewModel.PrimaryChannels.IndexOf(c => c.EntityPK == expectedExpandedChannelResource.PK) + numHeadingColumns
						: -1;

					for (var i = numHeadingColumns; i < control.ViewModel.PrimaryChannels.Count() + numHeadingColumns; i++)
					{
						var expectedChannelSize = ControlDpiScalingHelper.ScaleToCurrentDpiX(i == channelIndex ? 300 : 100);
						var actualChannelSize = control.ViewModel.Orientation == BMBoardSectionOrientation.Vertical
							? Convert.ToInt32(control.Table.ColumnStyles[i].Width)
							: Convert.ToInt32(control.Table.RowStyles[i].Height);

						AssertEquals(FormattableString.Invariant($"Column width at index {i} for section {control.ViewModel.SectionName}"), expectedChannelSize, actualChannelSize);
					}
				}
			});
		}

		public static void AssertBitmapEquals(string message, TaskCardControl card1, TaskCardControl card2)
		{
			AssertBitmapEquals(message, GetTicketBitmap(card1), GetTicketBitmap(card2));
		}

		public static void AssertBitmapNotEquals(string message, TaskCardControl card1, TaskCardControl card2)
		{
			AssertBitmapNotEquals(message, GetTicketBitmap(card1), GetTicketBitmap(card2));
		}

		static Bitmap GetTicketBitmap(TaskCardControl ticket)
		{
#if !WINZOR
			return (Bitmap)ticket.CreateCardBitmaps_ForTest(ticket.CardContent, isTemplateTaskCard: false).NormalBitmap;
#else
			return null;
#endif
		}

		public static void AssertTasksShown(BMComponentControl control, params ProcessTask[] tasks)
		{
			AssertTasksShown(string.Empty, control, tasks);
		}

		public static void AssertTasksShown(string message, BMComponentControl control, params ProcessTask[] tasks)
		{
			AssertTasksShown(message, control, shouldBeShown: true, tasks: tasks);
		}

		public static void AssertTasksNotShown(BMComponentControl control, params ProcessTask[] tasks)
		{
			AssertTasksNotShown(string.Empty, control, tasks);
		}

		public static void AssertTasksNotShown(string message, BMComponentControl control, params ProcessTask[] tasks)
		{
			AssertTasksShown(message, control, shouldBeShown: false, tasks: tasks);
		}

		public static void AssertVisibilityTaskCard(BusinessObjectFactory factory, IEnumerable<TaskCardControl> formTaskCards, ProcessHeader workflow, bool taskCardVisibility)
		{
			bool isVisible = false;
			var taskcard = formTaskCards.FirstOrDefault(t => t.CardContent.GetWorkflow(factory).PK == workflow.PK);

			if (taskcard != null)
			{
				isVisible = taskcard.Visible;
			}
			AssertEquals("Visibility of " + workflow.Description + " should be " + taskCardVisibility.ToString(), taskCardVisibility, isVisible);
		}

		public static void AssertTasksInChannel(Control control, BusinessObject channelBizo, params ProcessTask[] tasksInChannel)
		{
			AssertTasksInChannel("Expected tasks to be in channel " + channelBizo.HumanReadableName, control, channelBizo, tasksInChannel);
		}

		public static void AssertTasksInChannel(string message, Control control, BusinessObject channelBizo, params ProcessTask[] tasksInChannel)
		{
			CombineAssertions(message, () =>
			{
				foreach (var task in tasksInChannel)
				{
					var ticket = FindTaskCardControls(control, task).FirstOrDefault(t => t.Cell.Channel.EntityPK == channelBizo.PK);

					AssertNotNull($"Desc: {task.P9_Description}, ID: {task.P9_TaskID}", ticket);
				}
			});
		}

		public static void AssertTasksNotInChannel(Control control, BusinessObject channelBizo, ProcessTask[] tasksNotInChannel)
		{
			CombineAssertions("Expected tasks NOT to be in channel " + channelBizo.HumanReadableName, () =>
			{
				foreach (var task in tasksNotInChannel)
				{
					var ticket = FindTaskCardControls(control, task).FirstOrDefault(t => t.Cell.Channel.EntityPK == channelBizo.PK);

					AssertNull($"Desc: {task.P9_Description}, ID: {task.P9_TaskID}", ticket);
				}
			});
		}

		static void AssertTasksShown(string message, BMComponentControl control, bool shouldBeShown, params ProcessTask[] tasks)
		{
			var ticketControls = control.FindAll<TaskCardControl>().ToArray();

			CombineAssertions(message, () =>
			{
				foreach (var task in tasks)
				{
					var assertionMessage = string.Format("Should {0}be a ticket shown for task {1}", shouldBeShown ? "" : "NOT ", task);
					var taskCard = ticketControls.Where(t => t.Visible && t.CardContent.TaskIdentifier == task.PK).SingleOrDefault();

					if (shouldBeShown)
					{
						AssertNotNull(assertionMessage, taskCard);
					}
					else
					{
						AssertNull(assertionMessage, taskCard);
					}
				}
			});
		}

		public static ZToolStripMenuItem GetMenuItem(VisualBoardForm form, BMBoardSection section, string menuText)
		{
			var control = form.FindAll<BMComponentControl>().Single(c => c.ViewModel.SectionPK == section.PK);
			return control.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().FirstOrDefault(i => i.Text == menuText);
		}

		public static void AssertSubMenuItemChecked(ZToolStripMenuItem menuItem, int subMenuItemIndex, bool isChecked)
		{
			var subMenuItem = (ZToolStripMenuItem)menuItem.DropDownItems[subMenuItemIndex];
			AssertEquals(isChecked, subMenuItem.Checked);
			if (isChecked)
			{
				AssertEquals(new Size(16, 16), subMenuItem.Image.Size);

				var expectedPixels = GetPixels(new Bitmap(Properties.Resources.tick));
				var actualPixels = GetPixels(new Bitmap(subMenuItem.Image));
				AssertEquals(actualPixels, expectedPixels);
			}
			else
			{
				AssertNull(subMenuItem.Image);
			}
		}

		public static void AssertSubMenuItemChecked(ZToolStripMenuItem menuItem, string text, bool isChecked)
		{
			menuItem.ShowDropDown();
			var dropDownItems = menuItem.DropDownItems.Cast<ZToolStripMenuItem>();
			var subMenuItem = dropDownItems.Single(i => i.Text == text);
			AssertEquals(isChecked, subMenuItem.Checked);
		}

		public static void AssertLoadingIndicatorMessagesShownWhilstReloadingBoard(VisualBoardForm form, params LoadingMessage[] expectedIndicatorMessagesPerSection)
		{
			AssertLoadingIndicatorMessagesShownWhilstPerformingAction(form, () =>
			{
				form.ReloadBoard();
				form.AwaitAll();
			},
			expectedIndicatorMessagesPerSection);
		}

		public static void AssertLoadingIndicatorMessagesShownWhilstRefreshingBoard(VisualBoardForm form, params LoadingMessage[] expectedIndicatorMessagesPerSection)
		{
			AssertLoadingIndicatorMessagesShownWhilstPerformingAction(form, formAction: () =>
			{
				form.RefreshBoardAndWait();
			},
			expectedIndicatorMessagesPerSection: expectedIndicatorMessagesPerSection);
		}

		public static void AssertLoadingIndicatorMessagesShownWhilstRefreshingBoard_TriggeredByUser(VisualBoardForm form, params LoadingMessage[] expectedIndicatorMessagesPerSection)
		{
			AssertLoadingIndicatorMessagesShownWhilstPerformingAction(form, formAction: () =>
			{
				form.RefreshBoardAndWait();
			},
			expectedIndicatorMessagesPerSection: expectedIndicatorMessagesPerSection);
		}

		public static void AssertLoadingIndicatorMessagesShownWhilstPerformingAction(VisualBoardForm form, Action formAction, params LoadingMessage[] expectedIndicatorMessagesPerSection)
		{
			var loadingIndicatorControls = new Dictionary<ZGuid, Control>();
			var shownIndicatorMessagesPerSection = new List<LoadingMessage>();
			var controlsToUnsubscribeHandler = new List<Control>();

			var handler = new ControlEventHandler((sender, args) =>
			{
				var loadingIndicatorControl = args.Control.Controls.Find("loadingIndicatorLabel", true).SingleOrDefault();

				if (loadingIndicatorControl != null)
				{
					var parentControl = loadingIndicatorControl.GetParent<BMComponentControl>();

					if (loadingIndicatorControls.ContainsKey(parentControl.ViewModel.SectionPK))
					{
						Fail("Only one loading control should be shown - its text should be changed rather than re-creating the control and risk flickering.");
					}

					loadingIndicatorControls.Add(parentControl.ViewModel.SectionPK, loadingIndicatorControl);
					loadingIndicatorControl.TextChanged += (s, e) =>
					{
						shownIndicatorMessagesPerSection.Add(new LoadingMessage(parentControl.ViewModel.SectionName, loadingIndicatorControl.Text));
					};
				}
			});

			var handlerOfHandlers = new ControlEventHandler((sender, args) =>
			{
				foreach (var control in args.Control.FindAll<BMComponentControl>())
				{
					control.ControlAdded += handler;

					controlsToUnsubscribeHandler.Add(control);
				}
			});

			foreach (var control in form.FindAll<BMComponentControl>())
			{
				control.ControlAdded += handler;
				controlsToUnsubscribeHandler.Add(control);
			}

			form.ControlAdded += handlerOfHandlers;

			try
			{
				formAction();
			}
			finally
			{
				foreach (var control in controlsToUnsubscribeHandler)
				{
					control.ControlAdded -= handler;
				}

				form.ControlAdded -= handlerOfHandlers;
			}

			AssertContainsExactElementsInAnyOrder(expectedIndicatorMessagesPerSection, shownIndicatorMessagesPerSection);

			foreach (var kvp in loadingIndicatorControls)
			{
				AssertEquals("Loading indicator controls should be disposed after refresh is finished", true, kvp.Value.IsDisposed);
			}
		}

		#endregion

		#region Helper Builders

		public static VisualBoardForm GetAndShowVisualBoardForm(BMBoardSection section, int width = 800, int height = 800, Action<VisualBoardForm, int, int> formAction = null)
		{
			return GetAndShowVisualBoardForm(section.Board, width, height, formAction);
		}

		public static VisualBoardForm GetAndShowVisualBoardForm(IBMBoard board, int width = 800, int height = 800, Action<VisualBoardForm, int, int> formAction = null)
		{
			return GetAndShowVisualBoardForm((BMBoard)board, width, height, formAction);
		}

		public static VisualBoardForm GetAndShowVisualBoardForm(BMBoard board, int width = 800, int height = 800, Action<VisualBoardForm, int, int> formAction = null)
		{
			if (!board.IsInDatabase)
			{
				throw new InvalidOperationException("Save your factory first or you'll get a stack overflow. VERY UNFAIR!");
			}

			return GetAndShowVisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board), width, height, formAction);
		}

		public static VisualBoardForm GetAndShowVisualBoardForm(BMBoardSlideshow slideshow, int width = 800, int height = 800, Action<VisualBoardForm, int, int> formAction = null)
		{
			if (!slideshow.IsInDatabase)
			{
				throw new InvalidOperationException("Save your factory first or you'll get a stack overflow. VERY UNFAIR!");
			}

			return GetAndShowVisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(slideshow), width, height, formAction);
		}

		public static VisualBoardForm GetAndShowVisualBoardForm(BoardSlideshowViewModel viewModel, int width = 800, int height = 800, Action<VisualBoardForm, int, int> formAction = null)
		{
			var form = new VisualBoardForm(viewModel);
			form.MaximumSize = ControlDpiScalingHelper.NewScaledSize(width, height);
			form.Size = ControlDpiScalingHelper.NewScaledSize(width, height);
			viewModel.BoardForm = form;

			form.Show();

			formAction?.Invoke(form, width, height);

			Application.DoEvents();

			return form;
		}

		public static ReleaseSchedulerCellTasksControl CreateReleaseSchedulerCellTasksControl(CellContent cell, IEnumerable<ICardContent> cardContents, BMBoardSectionViewModel viewModel, BusinessObjectFactory factory)
		{
			var menu = TaskCardControl.CreateMenuStrip();
			var control = new ReleaseSchedulerCellTasksControl(cell, cardContents, viewModel, factory, menu);
			control.Disposed += (s, e) => menu.Dispose();
			return control;
		}

		public static CellTasksControl CreateCellTasksControl(CellContent cell, IEnumerable<ICardContent> cardContents, BMBoardSectionViewModel viewModel)
		{
			var menu = TaskCardControl.CreateMenuStrip();
			var control = new CellTasksControl(cell, cardContents, viewModel, menu);
			control.Disposed += (s, e) => menu.Dispose();
			return control;
		}

		public static TaskPanel CreateTaskPanel(CellContent cellContent, BMBoardSectionViewModel viewModel, BMBoardSection section = null, bool useCachingRenderer = false)
		{
			var menu = TaskCardControl.CreateMenuStrip();
			BMComponentControl control = null;

			if (section != null)
			{
				control = new BMComponentControl(section.SectionConfiguration, viewModel);
			}

			var cachedTaskCardRenderer = (CachingTaskCardRenderer)(useCachingRenderer ? TaskCardRendererFactory.GetRenderer(viewModel, isReleaseScheduler: false, cachingRendererBuilder: r => new CachingTaskCardRenderer(r)) : null);
			var panel = new TaskPanel(cellContent, viewModel, menu, cachingCardRenderer: cachedTaskCardRenderer, componentControl: control, section: section);
			panel.Disposed += (s, e) =>
			{
				menu.Dispose();
				control?.Dispose();
				cachedTaskCardRenderer?.Dispose();
			};
			return panel;
		}

		#endregion

		#region Helper Methods

		public static byte[] GetPixels(Bitmap bmp)
		{
			var bitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
			var length = bitmapData.Stride * bitmapData.Height;

			byte[] bytes = new byte[length];
			System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
			bmp.UnlockBits(bitmapData);
			return bytes;
		}

		public static void FindAndClickRiskFilterMenuItem(BMComponentControl control)
		{
			FindAndClickContextMenuItem(control, "Risk Filter");
		}

		public static void FindAndClickContextMenuItem(BMComponentControl control, string menuItemText)
		{
			control.ContextMenuStrip.Items.Cast<ToolStripItem>().First(i => i.Text == menuItemText).PerformClick();
		}

		public static void ToggleHeadingExpansion(BMComponentControl control, CellContent headingCellToExpand)
		{
			control.HeaderClicked(headingCellToExpand);
		}

		public static ChannelHeaderControl FindChannelHeaderControl(Control control, IBusiness channelEntity)
		{
			return control.FindAll<ChannelHeaderControl>().Single(c => c.Channel.EntityPK == channelEntity.Identifier);
		}

		public static TaskCardControl FindTaskCardControl(Control control, IProcessTask task)
		{
			return FindTaskCardControls(control, task).FirstOrDefault();
		}

		public static TaskCardControl[] FindTaskCardControls(Control control, IProcessTask task)
		{
			return FindTaskCardControls(control).Where(c => c.CardContent.TaskIdentifier == task.PK).ToArray();
		}

		public static TaskCardControl[] FindTaskCardControls(Control control)
		{
			return control.FindAll<TaskCardControl>().ToArray();
		}

		public static StackedTaskCardControl FindStackedTaskCardControl(Control control, IProcessHeader workflow)
		{
			return FindStackedTaskCardControls(control).FirstOrDefault(c => c.CardContent.WorkflowIdentifier == workflow.PK);
		}

		public static StackedTaskCardControl[] FindStackedTaskCardControls(Control control)
		{
			return control.FindAll<StackedTaskCardControl>().ToArray();
		}

		public static TaskCardDetailControl FindOrShowDetailedTicket(Control control, IProcessTask task)
		{
			var detailedTicket = control.FindSingleOrDefault<TaskCardDetailControl>(d => d.ProcessTask.PK == task.PK);

			if (detailedTicket == null)
			{
				var ticket = FindTaskCardControl(control, task);

				ticket.ShowDetailedCard();

				detailedTicket = control.FindSingleOrDefault<TaskCardDetailControl>(d => d.ProcessTask.PK == task.PK);
			}

			return detailedTicket;
		}

		public static bool DragTaskTicketToCell(TaskCardControl taskTicket, BMComponentControl control, CellContent cell = null)
		{
			var left = control.Left + (cell != null ? control.Table.Left + control.Table.GetColumnWidths().Take(cell.Column + 1).Sum() : 0);
			var top = control.Top + (cell != null ? control.Table.Top + control.Table.GetRowHeights().Take(cell.Row + 1).Sum() : 0);

			taskTicket.OnDragDropStarting();
			taskTicket.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
			taskTicket.Location = new Point(left, top);
			taskTicket.OnDragging(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
			taskTicket.OnDragDropFinished();

			Application.DoEvents();

			return taskTicket.IsDisposed;
		}

		public static TaskCardControl CreateDummyTaskCard(BusinessObjectFactory factory)
		{
			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(factory).ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow);

			return new TaskCardControl(task, BMSTestHelper.CreateDummyViewModel(factory));
		}

		public static CellTasksControl ShowAndGetCellTasksControl(TaskPanel taskPanel)
		{
			taskPanel.FindAndClickButton("TotalTasksButton");

			Application.DoEvents();

			return taskPanel.FindForm().FindSingle<CellTasksControl>();
		}

		public static void SetFilterStripName(ZFilterStrip strip, string name)
		{
			strip.Focus();
			strip.CurrentDataItem.FilterDescription = name;

			Application.DoEvents();
		}

		public static void CloseOpenVisualBoardFormsUnsafe()
		{
			var forms = Application.OpenForms.OfType<VisualBoardForm>().ToArray();
			foreach (var form in forms)
			{
				form.Close();
				Application.DoEvents();
			}

			foreach (var form in forms)
			{
				Assert(form.IsDisposed);
			}
		}

		public static BusinessObjectFactory GetSetupTasksFactory()
		{
			return VisualBoardsTestHelper.GetActiveFactory("LoadCardContents");
		}

		public static ZToolStripMenuItem GetHighlightLabelMenuItem(AcceptabilityBandTileControl tile)
		{
			return tile.ContextMenuStrip.Items.Cast<ZToolStripMenuItem>().Single(i => i.Text == AcceptabilityBandTileControl.ShowMatchingItemsLabel);
		}

		public static void ToggleShowMatchingItems(AcceptabilityBandTileControl tile)
		{
			GetHighlightLabelMenuItem(tile).PerformClick();
		}

		internal static void DoEvents()
		{
			Application.DoEvents();
		}

		internal static void PulseAndDoEvents(IAutoRefresher autoRefresh, int totalPulse, TriggerableAsyncStrategy triggerableAsyncStrategy = null)
		{
			((TriggerableAutoRefresher)autoRefresh).Pulse(totalPulse);
			DoAsyncSynchronously_ForTest(triggerableAsyncStrategy);
		}

		internal static void DoAsyncSynchronously_ForTest(TriggerableAsyncStrategy triggerable = null)
		{
			Application.DoEvents();
			triggerable?.DoAllActions();
		}

		#endregion

		#region Task Status Helpers

		public static void PlayTask(IProcessTask task, GlbStaff resource, Control controlOrFormContainingTicket)
		{
			SetTaskStatus(task, resource, controlOrFormContainingTicket, ProcessTaskStatusCodeList.Codes.Working);
		}

		public static void CloseTask(IProcessTask task, GlbStaff resource, Control controlOrFormContainingTicket)
		{
			SetTaskStatus(task, resource, controlOrFormContainingTicket, ProcessTaskStatusCodeList.Codes.Closed);
		}

		public static void SuspendTask(IProcessTask task, GlbStaff resource, Control controlOrFormContainingTicket)
		{
			SetTaskStatus(task, resource, controlOrFormContainingTicket, ProcessTaskStatusCodeList.Codes.Suspended);
		}

		static void SetTaskStatus(IProcessTask task, GlbStaff resource, Control controlOrFormContainingTicket, string status)
		{
			var taskCard = controlOrFormContainingTicket.FindAll<TaskCardControl>().Single(t => t.CardContent.TaskIdentifier == task.PK && ((t.Cell.Channel == null && resource == null) || t.Cell.Channel.EntityPK == resource.PK));
			taskCard.ShowDetailedCard();

			var detailedCard = controlOrFormContainingTicket.FindForm().FindAll<TaskCardDetailControl>().Single();
			var button = GetButtonToPress(status, detailedCard);
			button.PerformClick_ForTest();

			Application.DoEvents();
		}

		static GenericStatusChangeButton GetButtonToPress(string status, TaskCardDetailControl detailedCard)
		{
			var taskStatusControl = detailedCard.FindAll<TaskStatusControl>().Single();

			switch (status)
			{
				case ProcessTaskStatusCodeList.Codes.Working:
					return taskStatusControl.WorkingStatusButton;

				case ProcessTaskStatusCodeList.Codes.Suspended:
					return taskStatusControl.SuspendedStatusButton;

				case ProcessTaskStatusCodeList.Codes.Closed:
					return taskStatusControl.ClosedStatusButton;

				default:
					throw new ArgumentException("Invalid argument", nameof(status));
			}
		}

		#endregion

		#region Filter Strips

		public static void SetRuntimeFilterStrips(BMComponentControl control, params FilterStripsTestHelper.FilterStripDefinition[] filterStripDefs)
		{
			SetRuntimeFilterStrips(control, false, filterStripDefs);
		}

		public static void SetRuntimeFilterStrips(BMComponentControl control, bool setTaskFilters, params FilterStripsTestHelper.FilterStripDefinition[] filterStripDefs)
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
			{
				var zForm = (ZForm)f;
				var viewModel = (StmModuleFilterViewModel)((ZForm)f).DataSource;
				var filter = setTaskFilters ? viewModel.TaskFilter : viewModel.WorkflowFilter;

				if (setTaskFilters)
				{
					filter.S9_ModuleID = ModuleIDs.ProcessTasks.Name;
				}

				FilterStripsTestHelper.AddFilterStrips(filter, filterStripDefs);
			});

			control.ShowFilterDialog();
		}

		#endregion
	}

	#region Extension Methods

	public static class TestExtensionMethods
	{
		public static T[] FindMatchingResults_ForTest<T>(this ZFilterGridModule module, BusinessObjectFactory factory = null)
			where T : BusinessObject
		{
			((ZFilterStripBaseControl)module.EmbeddedControl).FirePerformSearch();

			var results = module.GridCollection.Cast<T>().ToArray();

			if (factory != null)
			{
				results = results.Select(b => factory.Load<T>(b.PK)).ToArray();
			}

			return results;
		}

		public static T WrapWithShownForm<T>(this T control)
			where T : Control
		{
			Form form = null;
			try
			{
				form = new ZForm();
				control.Disposed += (s, e) => form.Dispose();
				form.Controls.Add(control);
				form.Show();
			}
			catch
			{
				try
				{
					form?.Dispose();
				}
				catch
				{
				}

				throw;
			}
			return control;
		}

		public static void FindAndClickButton(this Control control, string buttonName)
		{
			var button = (ZButton)control.Controls.Find(buttonName, true)[0];

			button.PerformClick();
		}

		public static void OnPopup_Exposed(this MenuItem menuItem)
		{
			var method = menuItem.GetType().GetMethod("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(menuItem, new object[] { EventArgs.Empty });
		}

		public static void AddCard(this TaskPanel panel, ICardContent card, BusinessObjectFactory factory = null, bool unique = true)
		{
			if (factory == null)
			{
				factory = new BusinessObjectFactory();
			}

			SetupDummyAllocationMapIfNecessary(panel, factory);
			panel.SectionViewModel.ComponentGrid.CardAllocationMap.AddCard_ForTest(panel.Cell, card, unique);
		}

		public static void AddTask(this TaskPanel panel, ProcessTask task, bool unique = true)
		{
			var viewModel = panel.SectionViewModel;
			if (!viewModel.ShowWorkflowOrJobWorkflowCards)
			{
				panel.AddCard(new TaskCardContent(task, viewModel), task.Factory, unique);
			}
			else
			{
				panel.AddCard(new WorkflowCardContent(task.GetProcessHeader(), task, viewModel), task.Factory, unique);
			}
		}

		static void SetupDummyAllocationMapIfNecessary(TaskPanel panel, BusinessObjectFactory factory)
		{
			var viewModel = panel.SectionViewModel;
			if (viewModel.ComponentGrid.CardAllocationMap == null)
			{
				var section = factory.Load<BMBoardSection>(viewModel.SectionPK);
				panel.SectionViewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, Array.Empty<ProcessTask>());
			}
		}

		public static void Save_ForTest(this TaskCardDetailControl control, bool ensureNoValidationErrors = true, bool doEvents = true)
		{
			if (ensureNoValidationErrors)
			{
				var bizosToValidate = new BusinessObject[] { control.ProcessTask, control.ProcessTask.GetProcessHeader() };

				foreach (var bizo in bizosToValidate)
				{
					bizo.RunPreSaveValidation();

					BMSTestCaseWithFactory.AssertNoErrors(bizo);
				}
			}

			control.FindSingle<SaveButton>().PerformClick();

			if (doEvents)
			{
				BMSGUITestCase.DoEvents();
			}
		}

		#region Filter Application

		public static bool IsApplicable(this IBoardFilter filter, ProcessTask task, CellContent cell, BMBoardSectionViewModel boardSection)
		{
			return filter.IsApplicable(new TaskCardContent(task, boardSection), cell, boardSection);
		}

		public static bool IsApplicable(this IBoardFilter filter, ICardContent cardContent, CellContent cell, BMBoardSectionViewModel boardSection)
		{
			var taskCard = cardContent as TaskCardContent;
			var workflowCard = cardContent as WorkflowCardContent;
			var factory = taskCard != null ? taskCard.Task.Factory
				: workflowCard != null ? workflowCard.Workflow.Factory
				: new BusinessObjectFactory();

			var applicator = new CardVisibilityFilterApplicator(factory, boardSection, new[] { filter });
			return applicator.IsApplicable(cardContent, cell);
		}

		public static void SetupTasksForTest(this TaskPanel panel, BusinessObjectFactory factory, IEnumerable<IBoardFilter> filters, bool requiresFullRedraw = true)
		{
			var viewModel = panel.SectionViewModel;
			viewModel.FilterManager.Clear();

			foreach (var filter in filters)
			{
				viewModel.FilterManager.ApplyFilter(filter, requiresFullRedraw);
			}

			viewModel.ComponentGrid.ShowAllocatedTasks(factory, viewModel, viewModel.ComponentGrid.CardAllocationMap, new[] { panel.Cell }, requiresFullRedraw);
		}

		public static void SetupTasksForTest(this TaskPanel panel, BusinessObjectFactory factory, IEnumerable<ICardContent> cards, IEnumerable<IBoardFilter> filters, bool requiresFullRedraw = true)
		{
			if (cards != null)
			{
				foreach (var card in cards)
				{
					panel.AddCard(card);
				}
			}

			panel.SetupTasksForTest(factory, filters, requiresFullRedraw);
		}

		#endregion
	}

	#endregion
}
