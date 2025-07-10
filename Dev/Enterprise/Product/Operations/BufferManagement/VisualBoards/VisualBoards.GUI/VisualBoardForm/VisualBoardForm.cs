using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Modules;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Telemetry;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Microsoft.Win32;

#if !WINZOR
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
#endif

namespace Enterprise.VisualBoards.GUI
{
	public partial class VisualBoardForm : ZChildForm, IVisualBoardForm, ICustomerServiceMenuSectionCodeOverridable, IDataBoundControl
	{
#if DEBUG
		public VisualBoardForm()
		{
			if (Globals.IsTest)
			{
				throw new InvalidOperationException("This constructor is for the designer only. Using this in tests is wrong. Use BMSGUITestCase.GetAndShowVisualBoardForm instead.");
			}

			InitializeComponent();
		}

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			currentNotificationHandler = base.FormNotificationHandler;
		}

		public INotificationHandler FormNotificationHandler_Expose => currentNotificationHandler;
		INotificationHandler currentNotificationHandler;

#endif

		public VisualBoardForm(BoardSlideshowViewModel viewModel)
		{
			ControllerID = ControllerIDs.VisualBoard;
			DisplayMode = ODisplayMode.Browse;
			currentBoardEditDates = new Dictionary<ZGuid, ZDateTime>();
			customisationBoardEditDates = new Dictionary<ZGuid, ZDateTime>();

			this.viewModel = viewModel;

			this.RefreshStarted += VisualBoardForm_RefreshStarted;
			this.RefreshFinished += VisualBoardForm_RefreshFinished;

			showOpenInBrowserButton = ObjectFactory.Get<IBMSRegistry>().PAVEOnTheWeb;

			InitializeComponent();
			AddControlPanel();
			this.controlsPanel.LocationChanged += ControlsPanel_LocationChanged;

			HelpMenuItem = new ZMenuItem();
			ServiceRequestMenuItem.AddServiceRequestMenuItem(HelpMenuItem, this);

			FormLoadedWithArgs += (s, e) => InitializeWithLoadedArgs(e.Args?.ToArray());
			SystemUpdateCompleteLabel.Click += (s, e) => OnSystemUpdateCompleteLabelClick();

			RegisterHotKeys();
			SetFormText();
		}

		#region IDataBoundControl DataSource

		object IDataBoundControl.DataSource
		{ get { return BusinessEntityForPersistingForm; } }

		#endregion

		void SetFormText()
		{
			Text = BoardViewModel.BoardName;
		}

		void InitializeWithLoadedArgs(string[] args)
		{
			IsWindowsSessionActive = args == null || !args.Any(x => string.Equals(x, SessionInactiveArgName, StringComparison.OrdinalIgnoreCase));
			var autoRefreshArg = args?.SingleOrDefault(x => x.StartsWith(AutoRefreshTimerArgPrefix, StringComparison.OrdinalIgnoreCase));

			if (autoRefreshArg != null)
			{
				var timeUntilRefreshString = autoRefreshArg.Substring(AutoRefreshTimerArgPrefix.Length);
				timeUntilFirstRefresh = TimeSpan.ParseExact(timeUntilRefreshString, AutoRefreshTimerArgTimeFormat, CultureInfo.InvariantCulture);
				SystemUpdateCompleteLabel.Visible = true;
				controlsPanel.Visible = true;
				controlsPanel.BringToFront();
			}
#if WINZOR
			controlsPanel.BringToFront();
#endif

		}

		protected override IEnumerable<string> GetFormArgsToPersistOnClose()
		{
			var result = base.GetFormArgsToPersistOnClose()?.ToList() ?? new List<string>();
			result.Add(AutoRefreshTimerArgPrefix + autoRefresh.TimeUntilRefresh.ToString(AutoRefreshTimerArgTimeFormat, CultureInfo.InvariantCulture));

			if (!IsWindowsSessionActive)
			{
				result.Add(SessionInactiveArgName);
			}

			return result;
		}

		void OnSystemUpdateCompleteLabelClick()
		{
			SystemUpdateCompleteLabel.Visible = false;
			OnMouseButtonClick();
		}

		#region For Test
#if DEBUG
		public void SystemUpdateCompleteLabelClick_ForTest()
		{
			OnSystemUpdateCompleteLabelClick();
		}
#endif
		#endregion

		const string AutoRefreshTimerArgPrefix = "AutoRefresh:"; // Querystring argument name
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "TimeSpan format")]
		const string AutoRefreshTimerArgTimeFormat = "c";
		const string SessionInactiveArgName = "SessionInactive"; // Querystring argument name
		public const string OpenOnTheWebArgName = "OpenOnTheWeb"; // Querystring argument name

		static readonly Guid WebRecentItemId = new Guid("0a358a4f-84da-4b26-8d26-60912ac92e19"); // Constant guid XOR'd with boardPK to generate unique URI's.

		readonly Dictionary<ZGuid, ZDateTime> currentBoardEditDates;
		readonly Dictionary<ZGuid, ZDateTime> customisationBoardEditDates;
		BoardSlideshowViewModel viewModel;
		readonly bool showOpenInBrowserButton;
		TimeSpan timeUntilFirstRefresh = TimeSpan.MinValue;

		public WeakReference CurrentlyShownCellTasksControl { get; set; }

		public BoardViewModel BoardViewModel => viewModel?.CurrentBoardViewModel;

		public BoardSlideshowViewModel SlideShowViewModel
		{
			get { return viewModel; }
			private set { viewModel = value; }
		}

		#region ZForm Overrides

		public override IBusiness BusinessEntityForPersistingForm
		{
			get => viewModel?.Source;
		}

		ThreadLocal<BusinessObjectFactory> factories = new ThreadLocal<BusinessObjectFactory>(() => new BusinessObjectFactory { NameForDebugging = "VisualBoardForm.ThreadLocal" });

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == (Keys.Control | Keys.H))
			{
				ZFormMenuStrategy.CopyHyperlinkToClipboard(this);
				return true;
			}
			else
			{
				var handlers = GetSectionControls().OfType<IHotkeyHandler>().Where(h => h.ShouldHandle(keyData)).ToArray();

				foreach (var control in handlers)
				{
					control.HandleHotkeys(keyData);
				}

				return base.ProcessCmdKey(ref msg, keyData);
			}
		}

		internal bool HandlePressedKeys(ref Message msg, Keys keyData)
		{
			return ProcessCmdKey(ref msg, keyData);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			IsWindowsSessionActive = true;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			IsWindowsSessionActive = true;
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			if (AsyncStrategy.Default is DefaultAsyncStrategy && !IsUpdatingSessionStateOnActivationSuspended && !IsDisposing && !IsDisposed)
			{
				IsWindowsSessionActive = true;
			}
		}

		bool IsUpdatingSessionStateOnActivationSuspended => suspendUpdatingSessionStateOnActivationCount > 0;

		int suspendUpdatingSessionStateOnActivationCount;

		public IDisposable SuspendUpdatingSessionStateOnActivation()
		{
			suspendUpdatingSessionStateOnActivationCount++;
			return new DisposableAction(() => suspendUpdatingSessionStateOnActivationCount--);
		}

		protected void OnMouseButtonClick()
		{
			IsWindowsSessionActive = true;
		}

#if !WINZOR

		protected override void WndProc(ref Message m)
		{
			const int errorFileNotFound = 0x2;                      // ERROR_FILE_NOT_FOUND - The system cannot find the file specified.
			const int errorNetworkPathNotFound = 0x35;              // ERROR_BAD_NETPATH - The network path was not found.
			const int errorNetworkNameNoLongerAvailable = 0x40;     // ERROR_NETNAME_DELETED - The specified network name is no longer available.
			const int errorCreatingTopLevelChildWindow = 0x57E;     // ERROR_TLW_WITH_WSCHILD - Cannot create a top-level child window.

#if NETFRAMEWORK
			if (m.Msg == WindowsMessage.WM_PARENTNOTIFY && (m.WParam == (IntPtr)WindowsMessage.WM_LBUTTONDOWN || m.WParam == (IntPtr)WindowsMessage.WM_RBUTTONDOWN))
#else
			if (m.Msg == WindowsMessage.WM_PARENTNOTIFY && (m.WParam == WindowsMessage.WM_LBUTTONDOWN || m.WParam == WindowsMessage.WM_RBUTTONDOWN))
#endif
			{
				OnMouseButtonClick();
			}

			try
			{
				base.WndProc(ref m);
			}
			catch (Win32Exception ex) when
				(ex.NativeErrorCode == errorFileNotFound || ex.NativeErrorCode == errorNetworkPathNotFound || ex.NativeErrorCode == errorNetworkNameNoLongerAvailable ||
				(ex.NativeErrorCode == errorCreatingTopLevelChildWindow && ex.Source == "System.Windows.Forms" && ex.Message.Contains((NoResString)"Error creating window handle"))) // This is an exception message
			{
				// likely we are experiencing an intermittent network outage
				// We're probably just closing the form as WinForms decides to show a tooltip. For some reason these operations don't seem to be synchronised well.
				// No one can figure out why or how this problem happens, and nothing is actually observed to go wrong for the user, so we're ignoring the exception.
			}
		}

#endif

		protected override void SaveToRecentItems()
		{
			MainThreadRunner.RunOnMainThread(() => base.SaveToRecentItems());
		}

		#endregion

		#region Shortcuts

#if DEBUG
		public void OnKeyDown_ForTest(KeyEventArgs e)
		{
			OnKeyDown(e);
		}
#endif

		void RegisterHotKeys()
		{
			Hotkeys.RegisterHotKey(Keys.F5, () => RefreshBoardOnShortcut());
			Hotkeys.RegisterHotKey(Keys.Shift | Keys.F5, () => ReloadBoardOnShortcut(), Res.GetString("12fd0c75-dc21-4289-9c14-5f776198c971", "Reload Board"));
			Hotkeys.RegisterHotKey(Keys.Escape, () => CloseAllOpenOverlayControls());
			Hotkeys.RegisterHotKey(Keys.Control | Keys.S, () => SaveDetailedCards());
			Hotkeys.RegisterHotKey(Keys.Control | Keys.M, () => ToggleBoardMeetingMode());
		}

		public void CloseAllOpenOverlayControls()
		{
			foreach (var control in overlayControls.ToArray())
			{
				control.Dispose();
				Controls.Remove(control);
			}
			overlayControls.Clear();
		}

		public void RedrawAllOverlayControls()
		{
			foreach (var control in overlayControls)
			{
				control.Refresh();
			}
		}

		void SaveDetailedCards()
		{
			foreach (var openSaveableControl in this.Controls.OfType<ISaveableControl>().ToArray())
			{
				if (openSaveableControl != null)
				{
					openSaveableControl.Save();
					openSaveableControl.Close();
				}
			}
		}

		#endregion

		#region Life cycle

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			nextRefreshArgs = new BoardRefreshEventArgs { IsInitialLoad = true };
			StartAutoRefresher();
#if !WINZOR
			RegisterSessionSwitchHandling();
#endif
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && !IsDisposed)
				{
					UnhookFilterEventsFromCurrentBoard();

					if (autoRefresh != null)
					{
						autoRefresh.Dispose();
						autoRefresh.PropertyChanged -= AutoRefresher_PropertyChanged;
					}

					(SlideShowViewModel.Dispatcher as IDisposable)?.Dispose();

					DisposeBoardMeetingForm();

					this.RefreshStarted -= VisualBoardForm_RefreshStarted;
					this.RefreshFinished -= VisualBoardForm_RefreshFinished;
#if !WINZOR
					UnregisterSessionSwitchHandling();
#endif

					if (VisualBoardTableLayoutPanel != null)
					{
						VisualBoardTableLayoutPanel.Dispose();
						VisualBoardTableLayoutPanel = null;
					}

					if (controlsPanel != null)
					{
						RemoveControlPanel();
						controlsPanel.Dispose();
					}

					if (HelpMenuItem != null)
					{
						HelpMenuItem.Dispose();
					}

					var factoriesToDispose = factories;
					factories = null;
					factoriesToDispose.Dispose();

#if DEBUG
					if (forceDbHitInsideDispose)
					{
						new BusinessObjectFactory().Load<IBMBoard>(ZGuid.Empty);
					}
#endif
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

#if DEBUG
		public void ForceDisposeToHitDb()
		{
			forceDbHitInsideDispose = true;
		}

		bool forceDbHitInsideDispose;
#endif

		#endregion

		#region ReloadBoardCore

		void ReloadBoardCore(BoardRefreshEventArgs args)
		{
			args.IsReloading = true;

			IsInReload = true;

			if (!args.IsInitialLoad && !args.IsSlideshowProgression)
			{
				ReloadSlideshowViewModelNow();
				boardDataSourceCacheByBoard.Clear();
			}

			var dataSourceAndReload = GetRelevantDataSource();
			var shouldReloadLayout = dataSourceAndReload.Item2;
			if (args.IsSlideshowProgression)
			{
				args.IsInitialLoad = dataSourceAndReload.Item3;

#if DEBUG
				BoardRefreshEventArgs_ForTest = args;
#endif
			}

			HookEventsToCurrentBoard();
			SetupTableLayout(dataSourceAndReload.Item1, shouldReloadLayout, args, out var wasLayoutRetrievedFromCache);

			(SlideShowViewModel.Dispatcher as IDisposable)?.Dispose();

			var newDispatcher = new SubscribableDispatcher(this);

			try
			{
				BoardViewModel.RefreshAll(new BoardReloadOperation(newDispatcher));
			}
			catch
			{
				newDispatcher.Dispose();
				throw;
			}

			SlideShowViewModel.Dispatcher = newDispatcher;

#if DEBUG
			BeforeRefreshBoardAction_ForTest?.Invoke();
#endif

			if (!args.TriggeredBySlideshowControl || !wasLayoutRetrievedFromCache || args.CachedLayoutWasDisposed) // Either we refresh (which calls onRefreshFinished (which enables controls)) or we are done with reloading and call through directly to EnableSlideShowContrls
			{
				RefreshBoardCore(args);
			}
			else
			{
				OnRefreshFinished(args);
			}
		}

#if DEBUG
		public BoardRefreshEventArgs BoardRefreshEventArgs_ForTest { get; private set; }
		public Action BeforeRefreshBoardAction_ForTest { get; set; }
#endif

		Tuple<BoardDataSource, bool, bool> GetRelevantDataSource()
		{
			using (PerformanceStatisticsCollector.StartMonitoring(GetType().Name + ".GetRelevantDataSource", BoardViewModel.BoardName))
			{
				var containsKey = boardDataSourceCacheByBoard.ContainsKey(BoardViewModel.BoardPK);

				var isInitialLoad = !containsKey;

				var boardFactory = BoardViewModel.FactoryProvider.GetNewBackgroundThreadLoaderFactory("VisualBoardForm.GetRelevantDataSource");
				var reloadedBoard = boardFactory.Load<IBMBoard>(BoardViewModel.BoardPK);

				if (!containsKey || IsReloadRequired(reloadedBoard))
				{
					var dataSource = BoardViewModel.Build(reloadedBoard);

					boardDataSourceCacheByBoard[BoardViewModel.BoardPK] = dataSource;

					CacheBoardLastEditTime(reloadedBoard);

					return Tuple.Create(dataSource, true, isInitialLoad);
				}
				else
				{
					return Tuple.Create(boardDataSourceCacheByBoard[BoardViewModel.BoardPK], false, isInitialLoad);
				}
			}
		}

		readonly Dictionary<ZGuid, BoardDataSource> boardDataSourceCacheByBoard = new Dictionary<ZGuid, BoardDataSource>();

		#endregion

		#region Table Layout

		void SetupTableLayout(BoardDataSource dataSource, bool ignoreCache, BoardRefreshEventArgs args, out bool wasLayoutRetrievedFromCache)
		{
			using (new DisposableAction(SuspendLayout, ResumeLayout))
			{
				if (VisualBoardTableLayoutPanel != null)
				{
					CleanupTableLayoutAppropriately(args);
				}

				try
				{
					wasLayoutRetrievedFromCache = SetupTableLayoutCore(dataSource, ignoreCache, args);
				}
				catch (Win32Exception ex) when (ex.NativeErrorCode == 0 && ex.Source == "System.Windows.Forms" && ex.Message.Contains((NoResString)"Error creating window handle")) // This is an exception message
				{
					// Possible low memory condition
					ShowErrorLoadingBoardMessage();
					wasLayoutRetrievedFromCache = false;
				}

				SetFormText();
			}
		}

		protected virtual bool SetupTableLayoutCore(BoardDataSource dataSource, bool ignoreCache, BoardRefreshEventArgs args)
		{
			bool wasLayoutRetrievedFromCache = false;
			VisualBoardTableLayoutPanel = GetOrCreateTableLayoutForCurrentBoard(dataSource, args.IsSlideshowProgression, ignoreCache, out wasLayoutRetrievedFromCache);
			VisualBoardTableLayoutPanel.Size = ControlDpiScalingHelper.NewScaledSize(this.ClientSize.Width, this.ClientSize.Height - MainStatusBar.Height, false);

			Controls.Add(VisualBoardTableLayoutPanel);
			controlsPanel.AllowOverlap(VisualBoardTableLayoutPanel);
			return wasLayoutRetrievedFromCache;
		}

		void CleanupTableLayoutAppropriately(BoardRefreshEventArgs args)
		{
			try
			{
				if (args.IsSlideshowProgression && VisualBoardTableLayoutPanel.Controls.Count > 0)
				{
					var boardPK = viewModel.PreviousBoardViewModel.BoardPK;
					var existingPanel = panelCacheByBoard.ContainsKey(boardPK) ? panelCacheByBoard[boardPK] : null;

					if (existingPanel != null && existingPanel != VisualBoardTableLayoutPanel && !existingPanel.IsDisposed)
					{
						existingPanel.Dispose();
					}

					var taskPanels = this.FindAll<ITasksDetails>();
					var cardsCount = taskPanels.Sum(p => p.CardsCount);

					var bmsRegistry = ObjectFactory.Get<IBMSRegistry>();

					if (cardsCount < bmsRegistry.MaxNumberOfItemsAllowedToCacheBoardInSlideShow &&
						(panelCacheByBoard.Count < bmsRegistry.MaxNumberOfBoardsAllowedToCacheInSlideShow || panelCacheByBoard.ContainsKey(boardPK)))
					{
						panelCacheByBoard[boardPK] = VisualBoardTableLayoutPanel;
					}
					else
					{
						if (panelCacheByBoard.ContainsKey(boardPK))
						{
							panelCacheByBoard.Remove(boardPK);
						}

						VisualBoardTableLayoutPanel.Dispose();
						DoMemoryCleanup();

						args.CachedLayoutWasDisposed = true;
					}
				}
				else
				{
					VisualBoardTableLayoutPanel.Dispose();
				}

				Controls.Remove(VisualBoardTableLayoutPanel);
			}
			catch (InvalidOperationException ex) when (!ex.IsCriticalException())
			{
				ShowErrorLoadingBoardMessage();
			}
		}

		void ShowErrorLoadingBoardMessage()
		{
			var boardName = Text.IsNullOrEmpty() ? BoardViewModel.BoardName : Text;
			Globals.Message.Show(Res.GetString("47AD8290-69EF-44FB-8034-ED10A1A0E495", "The board [{0}] failed to display properly. Please close and reopen this window.", boardName));
		}

		KTableLayoutPanel GetOrCreateTableLayoutForCurrentBoard(BoardDataSource dataSource, bool isSlideshowProgression, bool forceRender, out bool wasLayoutRetrievedFromCache)
		{
			if (!forceRender && isSlideshowProgression && panelCacheByBoard.ContainsKey(BoardViewModel.BoardPK))
			{
				wasLayoutRetrievedFromCache = true;

				return panelCacheByBoard[BoardViewModel.BoardPK];
			}
			else
			{
				wasLayoutRetrievedFromCache = false;

				return VisualBoardSectionsRenderer.Render(dataSource.SectionsAndViewModels);
			}
		}

		readonly Dictionary<ZGuid, KTableLayoutPanel> panelCacheByBoard = new Dictionary<ZGuid, KTableLayoutPanel>();

		#endregion

		#region Control Buttons

#if DEBUG
		public
#endif
		BoardControlsPanel controlsPanel;

		void AddControlPanel()
		{
			try
			{
				controlsPanel = new BoardControlsPanel(SlideShowViewModel, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ClientRectangle.Width));
				controlsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
				if (SlideShowViewModel.HasComponentSections)
				{
					controlsPanel.AddSearchPerformedEvent(SearchBox_SearchPerformed);
					controlsPanel.AddBoardMeetingClickEvent(BoardMeetingModeButton_Click);
				}
				controlsPanel.AddConfigClickEvent(ConfigurationButton_Click);
				controlsPanel.AddRefreshClickEvent(RefreshButton_Click);

				if (showOpenInBrowserButton)
				{
					controlsPanel.AddOpenInBrowserClickEvent(OpenInBrowserButton_Click);
				}

				if (SlideShowViewModel.IsSlideshow)
				{
					controlsPanel.AddPreviousClickEvent(PreviousBoard_Click);
					controlsPanel.AddNextClickEvent(NextBoard_Click);
				}

				controlsPanel.AddPauseResumeClickEvent(PauseResumeRefreshButton_Click);
			}
			catch
			{
				controlsPanel?.Dispose();
				throw;
			}

			Controls.Add(controlsPanel);
		}

		void RemoveControlPanel()
		{
			if (SlideShowViewModel.HasComponentSections)
			{
				controlsPanel.RemoveSearchPerformedEvent(SearchBox_SearchPerformed);
				controlsPanel.RemoveBoardMeetingClickEvent(BoardMeetingModeButton_Click);
			}

			controlsPanel.RemoveConfigClickEvent(ConfigurationButton_Click);
			controlsPanel.RemoveRefreshClickEvent(RefreshButton_Click);

			if (showOpenInBrowserButton)
			{
				controlsPanel.RemoveOpenInBrowserClickEvent(OpenInBrowserButton_Click);
			}

			if (SlideShowViewModel.HasMultipleBoards)
			{
				controlsPanel.RemovePreviousClickEvent(PreviousBoard_Click);
				controlsPanel.RemoveNextClickEvent(NextBoard_Click);
			}
			controlsPanel.RemovePauseResumeClickEvent(PauseResumeRefreshButton_Click);
		}

		void ResumeRefresh()
		{
			if (autoRefresh != null && autoRefresh.IsPaused)
			{
				autoRefresh.IsPaused = false;
				controlsPanel.PauseResume(autoRefresh.IsPaused);
			}
		}

		void PauseRefresh()
		{
			if (autoRefresh != null)
			{
				autoRefresh.IsPaused = true;
				controlsPanel.PauseResume(autoRefresh.IsPaused);
			}
		}

		void PauseResumeRefreshButton_Click(object sender, EventArgs e)
		{
			if (SlideShowViewModel.HasMultipleBoards && !IsSlideshowRunning && IsInBoardMeeting)
			{
				boardMeetingModeForm.Close();
			}
			else
			{
				if (autoRefresh.IsPaused)
				{
					ResumeRefresh();
				}
				else
				{
					PauseRefresh();
				}
			}
		}

		void SearchBox_SearchPerformed(object sender, SearchEventArgs e)
		{
			var filter = new SearchFilter(e.SearchTerm);

			if (e.Cleared)
			{
				SlideShowViewModel.FilterManager.RemoveFilter(filter, shouldRefresh: true);
			}
			else
			{
				filter.RestoreVisualStateAfterFilterRemovedAction = () =>
				{
					this.BeginInvokeSafe(() => controlsPanel.ClearSearchBox());
				};

				SlideShowViewModel.FilterManager.ApplyFilter(filter);
				controlsPanel.SetSearchTerm(filter.RawSearchTerm);
			}
		}

		public ZButton RefreshButton
		{
			get { return controlsPanel.RefreshButton; }
		}

		#endregion

		#region Navigation

		public bool IsSlideshowRunning
		{
			get { return SlideShowViewModel.HasMultipleBoards && autoRefresh != null && !autoRefresh.IsPaused; }
		}

		void NextBoard_Click(object sender, EventArgs e)
		{
			if (SlideShowViewModel.HasMultipleBoards)
			{
				MoveNext(triggeredFromButtonClick: true);
			}
			else
			{
				Globals.Message.ShowInformation(OnlyOneBoardMessage);
			}
		}

		public void MoveNext(bool triggeredFromButtonClick = false)
		{
			MoveToNextSlide(viewModel.MoveForward, triggeredFromButtonClick, triggeredBySlideShowTimer: !triggeredFromButtonClick);
		}

		void PreviousBoard_Click(object sender, EventArgs e)
		{
			if (SlideShowViewModel.HasMultipleBoards)
			{
				MovePrevious();
			}
			else
			{
				Globals.Message.ShowInformation(OnlyOneBoardMessage);
			}
		}

		static MultilingualString OnlyOneBoardMessage => ResString.GetMultilingualString("1A82B823-658C-4186-B282-3661A86034AF", "There is only one board on this Slide Show.");

		public void MovePrevious()
		{
			MoveToNextSlide(viewModel.MoveBackwards, triggeredFromButtonClick: true, triggeredBySlideShowTimer: false);
		}

		void MoveToNextSlide(Action reloadAction, bool triggeredFromButtonClick, bool triggeredBySlideShowTimer)
		{
			nextRefreshArgs = new BoardRefreshEventArgs
			{
				TriggeredBySlideshowControl = triggeredFromButtonClick,
				TriggeredBySlideshowTimer = triggeredBySlideShowTimer,
				ReloadAction = reloadAction,
				ShouldReload = true
			};
			DisableSlideShowControls();
			RefreshBoard();
		}

		#endregion

		#region Refresh

		#region OnRefresh/OnReload

		public event EventHandler<BoardRefreshEventArgs> RefreshStarted;
		public event EventHandler<BoardRefreshEventArgs> RefreshFinished;
		public event EventHandler<BoardRefreshEventArgs> ReloadStarted;

		public void VisualBoardForm_RefreshStarted(object sender, BoardRefreshEventArgs args)
		{
			IsInRefresh = true;
		}

		public void VisualBoardForm_RefreshFinished(object sender, BoardRefreshEventArgs args)
		{
			EnableSlideShowControls();
			IsInRefresh = false;
			IsInReload = false;
			IsWaitingForRefresh = false;
			hasBeenRefreshedAtLeastOnce = true;

#if DEBUG
			RefreshCount_ForTest++;
#endif
		}

#if DEBUG
		public int RefreshCount_ForTest { get; private set; }

		public void ExpandButtonPanelAndClickRefreshButton()
		{
			controlsPanel.Expand();
			RefreshButton.PerformClick();
		}
#endif

		void EnableSlideShowControls()
		{
			if (SlideShowViewModel.HasMultipleBoards)
			{
				controlsPanel.EnableBoardMovementButtons();
			}
		}

		void DisableSlideShowControls()
		{
			if (SlideShowViewModel.HasMultipleBoards && controlsPanel.IsExpanded)
			{
				controlsPanel.DisableBoardMovementButtons();
			}
		}

		public bool IsWaitingForRefresh { get; private set; }
		public bool IsInRefresh { get; private set; }
		public bool IsInReload { get; private set; }

		#endregion

		void ReloadBoard(BoardRefreshEventArgs args)
		{
			using var activity = TelemetryService.ActivitySource.StartActivity($"{nameof(VisualBoardForm)}.{nameof(ReloadBoard)}");
			OnReloadStarted(args);
			UnhookFilterEventsFromCurrentBoard();

			if (args.ReloadAction != null)
			{
				args.ReloadAction();
				autoRefresh.RefreshDelay = TimeSpan.FromSeconds(SlideShowViewModel.CurrentBoardViewModel.IntervalInSlideShowSeconds);
			}

			DisposeBoardMeetingForm();
			ReloadBoardCore(args);
		}

		IAutoRefresher autoRefresh;

		void StartAutoRefresher()
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("VisualBoardForm");
			}
			else if (autoRefresh == null && SlideShowViewModel != null)
			{
				var isDelayedStart = timeUntilFirstRefresh > TimeSpan.MinValue;
				autoRefresh = AsyncStrategy.Default.GetAutoRefresher(PerformAutoRefresherAction, GetRefreshInterval(), () => BoardVisible);

				if (autoRefresh == null)
				{
					return;
				}

				if (isDelayedStart)
				{
					autoRefresh.TimeUntilRefresh = timeUntilFirstRefresh;
				}

				autoRefresh.PropertyChanged += AutoRefresher_PropertyChanged;
				if (SlideShowViewModel.HasMultipleBoards)
				{
					viewModel.MoveBackwards(); //Start new slideshow from -1 slide, as the autorefresher fires the action on start
				}

				autoRefresh.Start(refreshImmediately: !isDelayedStart);
			}
		}

		bool BoardVisible => !IsFormMinimised && IsWindowsSessionActive;

		bool IsFormMinimised => WindowState == FormWindowState.Minimized;

		public bool IsWindowsSessionActive
		{
			get { return isWindowsSessionActive; }
			set
			{
				isWindowsSessionActive = value;
				if (!hasBeenRefreshedAtLeastOnce && IsWindowsSessionActive)
				{
					RefreshBoard();
				}
			}
		}

		bool isWindowsSessionActive = true;

		bool hasBeenRefreshedAtLeastOnce;

		#region Session Switch
#if !WINZOR

		public EnterpriseChannel EnterpriseChannel { get; protected set; } = EnterpriseChannel.Instance;

		void RegisterSessionSwitchHandling()
		{
			if (IsUsingRemoteDesktopServices)
			{
				EnterpriseChannel.ClientSessionSwitch += OnSessionSwitch;
				EnterpriseChannel.ServerSessionSwitch += OnSessionSwitch;
			}
			else
			{
				var callback = new WtsSessionSwitchCallback(null, HandleSessionSwitchMessage);
				var otherVisualBoardForm = ZApplication.GetOpenForms().OfType<VisualBoardForm>().Except(this).FirstOrDefault(x => x.sessionChangeMonitor != null);
				sessionChangeMonitor = otherVisualBoardForm?.sessionChangeMonitor;

				if (sessionChangeMonitor == null)
				{
					sessionChangeMonitor = new HiddenFormWtsSessionChangeMonitor(sessionChangeCallbackIdentifier, callback);
				}
				else
				{
					sessionChangeMonitor.TryAddCallback(sessionChangeCallbackIdentifier, callback);
				}
			}
		}

		void UnregisterSessionSwitchHandling()
		{
			if (IsUsingRemoteDesktopServices)
			{
				if (EnterpriseChannel.ClientSessionSwitch != null)
				{
					EnterpriseChannel.ClientSessionSwitch -= OnSessionSwitch;
				}

				if (EnterpriseChannel.ServerSessionSwitch != null)
				{
					EnterpriseChannel.ServerSessionSwitch -= OnSessionSwitch;
				}
			}
			else if (sessionChangeMonitor != null)
			{
				sessionChangeMonitor.TryRemoveCallback(sessionChangeCallbackIdentifier);

				if (sessionChangeMonitor.CallbackCount == 0 && !sessionChangeMonitor.IsClosed)
				{
					sessionChangeMonitor.Close();
				}

				sessionChangeMonitor = null;
			}
		}

		HiddenFormWtsSessionChangeMonitor sessionChangeMonitor;

		readonly Guid sessionChangeCallbackIdentifier = Guid.NewGuid();

		protected bool IsUsingRemoteDesktopServices => EnterpriseChannel?.IsConnected ?? false;

		void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
		{
			HandleSessionSwitchMessage(e.Reason);
		}

		static IEnumerable<SessionSwitchReason> SessionNotLoggedInReasons => new[] { SessionSwitchReason.SessionLock, SessionSwitchReason.ConsoleDisconnect, SessionSwitchReason.RemoteDisconnect, SessionSwitchReason.SessionLogoff };

		protected void HandleSessionSwitchMessage(SessionSwitchReason reason)
		{
			IsWindowsSessionActive = !SessionNotLoggedInReasons.Contains(reason);
		}

#endif
		#endregion

		TimeSpan GetRefreshInterval()
		{
			return SlideShowViewModel.HasMultipleBoards ? TimeSpan.FromSeconds(SlideShowViewModel.CurrentBoardViewModel.IntervalInSlideShowSeconds) : TimeSpan.FromSeconds(SlideShowViewModel.RefreshSeconds);
		}

		void AutoRefresher_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (autoRefresh != null && e.PropertyName == "TimeUntilRefresh" && IsHandleCreated)
			{
				var timeToRefresh = autoRefresh.TimeUntilRefresh;
				var timeToShow = timeToRefresh > TimeSpan.Zero && BoardVisible ? timeToRefresh : TimeSpan.Zero;
				using (var activity = TelemetryService.ActivitySource.StartActivity($"{nameof(VisualBoardForm)}.{nameof(AutoRefresher_PropertyChanged)}"))
				{
					if (activity != null)
					{
						activity.ActivityTraceFlags = System.Diagnostics.ActivityTraceFlags.None;
					}
					this.BeginInvokeSafe(() => controlsPanel.UpdateTimerValue(timeToShow));
				}
			}
		}

#if DEBUG
		public AutoResetEvent PerformAutoRefresherActionCompleted_ForTest = new AutoResetEvent(false);
#endif

		Task PerformAutoRefresherAction(CancellationToken token)
		{
			using var activity = TelemetryService.ActivitySource.StartRootActivity($"{nameof(VisualBoardForm)}.{nameof(PerformAutoRefresherAction)}");
			if (token.IsCancellationRequested)
			{
				IsWaitingForRefresh = false;
				token.ThrowIfCancellationRequested();
			}

			var completionSource = new TaskCompletionSource<ulong>();
			var task = completionSource.Task;
			if (IsHandleCreated)
			{
				this.BeginInvokeSafe(() =>
				{
					var args = GetNextRefreshArgs();
					DisableSlideShowControls();
					ExecuteRefreshAction(args);
					var memory = DoMemoryCleanup();
					if (memory != 0)
					{
						var message = GetInsufficientMemoryLogMessage(memory);

						MainThreadRunner.RunOnMainThread(() => ErrorReporter.ReportOnce(message));
					}
				});
			}
			completionSource.SetResult(0);
			IsWaitingForRefresh = false;
			return task;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Used in logging only")]
		string GetInsufficientMemoryLogMessage(ulong insufficientMemory)
		{
			var message = $"Running low on virtual memory: {insufficientMemory} MB. Board name: {BoardViewModel.BoardName}, Total tickets: {BoardViewModel.NumberOfTicketsToShow}{System.Environment.NewLine}";// Log Information
			message += string.Join(System.Environment.NewLine, BoardViewModel.GetSections().Select(section => $"Section: Type: {section.SectionType}, Name: {section.SectionName}, {section.SectionDetailsForLog}"));// Log Information

			return message.Trim(',', ' ');
		}

		public bool ShouldReportLowMemory { get; private set; } = !Globals.IsTest;

#if DEBUG
		public IDisposable EnableShouldReportLowMemory_ForTest()
		{
			var original = ShouldReportLowMemory;
			ShouldReportLowMemory = true;

			return new DisposableAction(() => ShouldReportLowMemory = original);
		}

#endif

		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect", Scope = "member", Target = "Enterprise.VisualBoards.GUI.VisualBoardForm.#DoMemoryCleanup()")]  // Really need GC.Collect here
		[SuppressMessage("CargoWiseOne", "CW1056:DoNotUseGCCollect", Justification = "We may have some memory issues")]
		ulong DoMemoryCleanup()
		{
			const int memoryThresholdMB = 100;
			if (ZSystemInformation.Instance.AvailableVirtualMemory < memoryThresholdMB)
			{
				GC.Collect(); // We may have some memory issues
				if (ShouldReportLowMemory)
				{
					var availableMemory = ZSystemInformation.Instance.AvailableVirtualMemory;
					if (availableMemory < memoryThresholdMB)
					{
						return availableMemory;
					}
				}
			}
			return 0;
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			if (ModifierKeys == Keys.Control)
			{
				ForceReloadOnNextRefresh();
			}

			var args = nextRefreshArgs ?? (nextRefreshArgs = BoardRefreshEventArgs.Empty);
			args.TriggeredByUserRefreshingBoardOrSection = true;

			RefreshBoard();
		}

#if DEBUG
		public void RefreshNow_ForTest(bool forceReload = false)
		{
			var args = GetNextRefreshArgs();

			if (forceReload)
			{
				args.ShouldReload = true;
			}

			ExecuteRefreshAction(args);
		}
#endif

		public void RefreshBoard()
		{
			using var activity = TelemetryService.ActivitySource.StartActivity($"{GetType().Name}.{nameof(RefreshBoard)}");
			if (!IsDisposed && !IsDisposing)
			{
				if (nextRefreshArgs != null && nextRefreshArgs.ShouldReload && IsInBoardMeeting)
				{
					LeaveBoardMeetingMode();
				}

				if (autoRefresh != null && !autoRefresh.IsDisposed)
				{
					IsWaitingForRefresh = true;

					try
					{
						autoRefresh.Refresh();
					}
					catch (ObjectDisposedException)
					{ }
				}
			}
		}

		void ReloadBoardOnShortcut()
		{
			nextRefreshArgs = new BoardRefreshEventArgs { ShouldReload = true, TriggeredByShortcut = true };
			RefreshBoard();
		}

		void RefreshBoardOnShortcut()
		{
			nextRefreshArgs = new BoardRefreshEventArgs { TriggeredByShortcut = true };
			RefreshBoard();
		}

		void ReloadBoardOnClosedConfigForm()
		{
			nextRefreshArgs = new BoardRefreshEventArgs { ShouldReload = true, TriggeredByBoardConfigForm = true };
			RefreshBoard();
		}

		public void ReloadBoard()
		{
			nextRefreshArgs = new BoardRefreshEventArgs { ShouldReload = true, IsForcedReload = true };
			RefreshBoard();
		}

		void ReloadSlideshowViewModel()
		{
			this.BeginInvokeSafe(ReloadSlideshowViewModelNow);
		}

		void ReloadSlideshowViewModelNow()
		{
			var existingSource = SlideShowViewModel.Source;
			var factory = new BusinessObjectFactory { NameForDebugging = "VisualBoardForm.ReloadSlideshowViewModel", RefreshEnabled = false };
			var slideshow = (IVisualBoardProvider)factory.Load(existingSource.GetType(), existingSource.PK);

			var newSlideShowViewModel = new BoardSlideshowViewModel(slideshow);
			var factoryProvider = SlideShowViewModel.FactoryProvider.Clone(newSlideShowViewModel);

			newSlideShowViewModel.BoardForm = this;
			newSlideShowViewModel.FactoryProvider = factoryProvider;
			newSlideShowViewModel.Dispatcher = SlideShowViewModel.Dispatcher;

			SlideShowViewModel = newSlideShowViewModel;
		}

		public void ForceReloadOnNextRefresh()
		{
			nextRefreshArgs = new BoardRefreshEventArgs { ShouldReload = true };
		}

		BoardRefreshEventArgs GetNextRefreshArgs()
		{
			var args = nextRefreshArgs ?? BoardRefreshEventArgs.Empty;
			nextRefreshArgs = null;

			var reloadedBoard = LoadCurrentBoardInNewFactory();
			if (!args.ShouldReload)
			{
				args.ShouldReload = IsReloadRequired(reloadedBoard);
			}

			if (!args.TriggeredByUserRefreshingBoardOrSection && !args.TriggeredByShortcut && !args.TriggeredByBoardConfigForm && !args.IsInitialLoad)
			{
				args.TriggeredByRefreshTimer = true;
			}

			if (!args.IsForcedReload && !args.TriggeredBySlideshowControl && !args.TriggeredByUserRefreshingBoardOrSection && !args.TriggeredBySlideshowTimer && !args.TriggeredByBoardConfigForm && !args.TriggeredByShortcut && SlideShowViewModel.HasMultipleBoards)
			{
				args = new BoardRefreshEventArgs
				{
					TriggeredBySlideshowControl = false,
					TriggeredBySlideshowTimer = true,
					ReloadAction = viewModel.MoveForward,
					ShouldReload = true,
					IsInitialLoad = args.IsInitialLoad,
				};
			}

			if (IsBoardDeleted(reloadedBoard))
			{
				args.ShouldReloadSlides = true;
			}
			return args;
		}

		BoardRefreshEventArgs nextRefreshArgs;

		bool IsBoardDeleted(IBMBoard board)
		{
			return board == null || ((BusinessObject)board).IsDeleted;
		}

		bool IsReloadRequired(IBMBoard board)
		{
			var containsKey = boardDataSourceCacheByBoard.ContainsKey(BoardViewModel.BoardPK);
			var needReloadSection = containsKey && boardDataSourceCacheByBoard[BoardViewModel.BoardPK].SectionsAndViewModels.Any(sav => sav.ViewModel.ReloadRequired(sav.Section));

			var lastEdit = LastEditIfExists;

			return !lastEdit.Item1 || GetBoardLastEditTime(board) != lastEdit.Item2 || LastCustomisationEditIfExists != GetBoardCustomisationLastEditTime(board) || needReloadSection;
		}

		Tuple<bool, ZDateTime> LastEditIfExists
		{
			get
			{
				var reloadRequired = currentBoardEditDates.TryGetValue(BoardViewModel.BoardPK, out var currentLastEdit);
				return Tuple.Create(reloadRequired, currentLastEdit);
			}
		}

		ZDateTime LastCustomisationEditIfExists
		{
			get
			{
				customisationBoardEditDates.TryGetValue(BoardViewModel.BoardPK, out var currentLastEdit);
				return currentLastEdit;
			}
		}

		void CacheBoardLastEditTime(IBMBoard board)
		{
			currentBoardEditDates[BoardViewModel.BoardPK] = GetBoardLastEditTime(board);
			customisationBoardEditDates[BoardViewModel.BoardPK] = GetBoardCustomisationLastEditTime(board);
		}

		IBMBoard LoadCurrentBoardInNewFactory()
		{
			return new ReadOnlyBusinessObjectFactory { NameForDebugging = GetType().Name + ".LoadCurrentBoardInNewFactory" }.Load<IBMBoard>(BoardViewModel.BoardPK);
		}

		ZDateTime GetBoardLastEditTime(IBMBoard board)
		{
			return board != null ? board.MB_SystemLastEditTimeUtc : ZDateTime.Invalid;
		}

		ZDateTime GetBoardCustomisationLastEditTime(IBMBoard board)
		{
			return board != null ? board.CustomisationLastEditTimeUTC : ZDateTime.Invalid;
		}

		void ExecuteRefreshAction(BoardRefreshEventArgs args)
		{
			if (!IsInRefresh && !IsInReload && !this.IsDisposed && !GetSectionControls().Any(c => c.SuppressBoardRefresh(args)))
			{
				if (args.ShouldReloadSlides)
				{
					ReloadSlidesAndMoveToNextAvailable();
				}
				else if (!IsInBoardMeeting || args.TriggeredByUserRefreshingBoardOrSection || args.TriggeredByBoardConfigForm || args.TriggeredByShortcut || args.TriggeredBySlideshowControl)
				{
					if (args.ShouldReload)
					{
						ReloadBoard(args);
					}
					else
					{
						RefreshBoardCore(args);
					}
				}
			}
		}

		void ReloadSlidesAndMoveToNextAvailable()
		{
			SlideShowViewModel.ReloadBoards();

			if (!SlideShowViewModel.BoardViewModels.Any())
			{
				Globals.Message.Show(Res.GetString("e11b3f59-a7bf-4649-be59-a30e581faa2f", "The board you were looking at no longer exists."));
				Close();
			}
			else
			{
				MoveNext();
			}
		}

		void OnRefreshStarted(BoardRefreshEventArgs args)
		{
			RefreshStarted?.Invoke(this, args);
		}

		void OnReloadStarted(BoardRefreshEventArgs args)
		{
			ReloadStarted?.Invoke(this, args);

			SystemUpdateCompleteLabel.Visible = false;
		}

		void RefreshBoardCore(BoardRefreshEventArgs args)
		{
			KeepSessionAlive();
			OnRefreshStarted(args);

			if (!IsDisposed)
			{
				try
				{
					ClearCachedMenuItems();
					CloseCardsAndRefreshSections(args);
				}
				catch (OdysseyDataException)
				{
					Dispose();
				}
			}
		}

		void ClearCachedMenuItems()
		{
			controlsPanel.BoardPickerButton.ClearMenuItemCache();
		}

		void KeepSessionAlive()
		{
#if !WINZOR

			if (Globals.IsTest || Handle == UnsafeNativeMethods.GetForegroundWindow())
			{
				ObjectFactory.Get<IKeepSessionAlive>().KeepAlive();
			}

#endif
		}

		void CloseCardsAndRefreshSections(BoardRefreshEventArgs args)
		{
			this.BeginInvokeSafe(() =>
			{
				using (new ZWaitCursorChanger(this))
				{
					CloseAllOpenOverlayControls();

					if (!args.IsInitialLoad)
					{
						SlideShowViewModel.RefreshServices(BoardServiceStalenessPolicy.StaleBeforeBoardRefresh);
					}

					RefreshSectionControls(args);
				}
			});
		}

		void RefreshSectionControls(BoardRefreshEventArgs args)
		{
			RefreshedSectionControls = new HashSet<IBoardSectionControl>();

			var sectionControls = GetSectionControls().ToArray();

			BoardViewModel.RefreshAll(new FullRefreshOperation());

			foreach (var control in sectionControls)
			{
				StartPerformanceStatisticsCollection(control.SectionViewModel);

				control.RefreshCompleted += SectionControl_RefreshCompleted;
				control.Refresh(args);
			}

			if (!sectionControls.Any())
			{
				SectionControl_RefreshCompleted(null, args);
			}
		}

		HashSet<IBoardSectionControl> RefreshedSectionControls;

		void SectionControl_RefreshCompleted(object sender, BoardRefreshEventArgs args)
		{
			if (sender != null)
			{
				var control = (IBoardSectionControl)sender;
				control.RefreshCompleted -= SectionControl_RefreshCompleted;
				RefreshedSectionControls.Add(control);

				StopPerformanceStatisticsCollection(control.SectionViewModel);
			}

			if (RefreshedSectionControls.SetEquals(GetSectionControls()))
			{
				OnRefreshFinished(args);
			}
		}

		void OnRefreshFinished(BoardRefreshEventArgs args)
		{
			RefreshFinished?.Invoke(this, args);
		}

		public IEnumerable<IBoardSectionControl> GetSectionControls()
		{
			return this.FindAll<IBoardSectionControl>(maxLevelsDeep: 2);
		}

		readonly Dictionary<ZGuid, Tuple<SpecificPerformanceStatisticsCollector, IDisposable>> refreshSectionDisposableActions = new Dictionary<ZGuid, Tuple<SpecificPerformanceStatisticsCollector, IDisposable>>();

		void StartPerformanceStatisticsCollection(BoardSectionViewModel sectionViewModel)
		{
			if (!refreshSectionDisposableActions.ContainsKey(sectionViewModel.SectionPK))
			{
				var collector = new SpecificPerformanceStatisticsCollector();

				var statisticsNames = sectionViewModel.CreatePerformanceStatisticsNamePair();
				var tuple = Tuple.Create(collector, collector.StartMonitoring(statisticsNames.Item1, statisticsNames.Item2));

				collector.ThreadSentry.RelinquishThreadOwnership();

				refreshSectionDisposableActions.Add(sectionViewModel.SectionPK, tuple);
			}
		}

		void StopPerformanceStatisticsCollection(BoardSectionViewModel sectionViewModel)
		{
			var key = sectionViewModel.SectionPK;

			if (refreshSectionDisposableActions.TryGetValue(key, out var statisticsDisposable))
			{
				refreshSectionDisposableActions.Remove(key);

				var collector = statisticsDisposable.Item1;

				if (!collector.ThreadSentry.IsOwner)
				{
					collector.ThreadSentry.TakeThreadOwnership();
				}

				statisticsDisposable.Item2.Dispose();

#if DEBUG
				collector.AttemptFlush(force: true); // Normally happens in a background thread, but we haven't got time for that.
#endif
			}
		}

		#endregion

		#region Open in Browser

		void OpenInBrowserButton_Click(object sender, EventArgs e)
		{
			OpenBoardOnWeb(BoardViewModel.BoardPK, BoardViewModel.BoardHumanReadableShortcutName);
		}

		public static void OpenBoardOnWeb(ZGuid boardPK, string boardHumanReadableShortcutName)
		{
			var baseURL = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			if (string.IsNullOrEmpty(baseURL))
			{
				var result = ResString.GetMultilingualString("5508dd27-ff25-4ee2-9409-c8201f9bbf8c", "This Visual Board cannot be opened in a browser as GLOW has not been configured for this client.");
				Globals.Message.ShowError(result);
				return;
			}

			var url = UrlBuilder.GenerateURL(new Uri(baseURL), "Goto/Board", additionalQueryStrings: new[] { ("entityPK", boardPK.ToString()) });
			WebUrlLauncher.Launch(url.ToString());

			SaveBoardOnWebToRecentItems(boardPK, Res.GetString("875f1aa3-2182-45fe-bfce-76a9e8fcd038", "{0} (Web)", boardHumanReadableShortcutName));
		}

		static void SaveBoardOnWebToRecentItems(ZGuid boardPK, string boardHumanReadableShortcutName)
		{
			var favoriteProvider = ObjectFactory.Get<IFavoriteProvider>();
			var args = new string[] { OpenOnTheWebArgName };
			var recordUri = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.VisualBoard, boardPK, args);
			var recordKey = Combine(boardPK.ToGuid(), WebRecentItemId);
			var linkWrapper = new LinkWrapper(ModuleIDs.VisualBoard.Name, recordKey, recordUri, boardHumanReadableShortcutName);

			if (linkWrapper != null)
			{
				favoriteProvider.AddToRecentItems(linkWrapper);
			}
		}

		public static Guid Combine(Guid x, Guid y)
		{
			var a = x.ToByteArray();
			var b = y.ToByteArray();
			return new Guid(BitConverter.GetBytes(BitConverter.ToUInt64(a, 0) ^ BitConverter.ToUInt64(b, 8))
				.Concat(BitConverter.GetBytes(BitConverter.ToUInt64(a, 8) ^ BitConverter.ToUInt64(b, 0))).ToArray());
		}

		#endregion

		#region Config

		void ConfigurationButton_Click(object sender, EventArgs e)
		{
			var boardProvider = SlideShowViewModel.Source;
			ShowBoardConfigForm(boardProvider);
		}

		void ShowBoardConfigForm(IVisualBoardProvider boardProvider)
		{
			MainThreadRunner.RunOnMainThread(() =>
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "VisualBoardForm.ShowBoardConfigForm" };
				var reloadedBoardProvider = boardProvider.ReloadInFactory(factory);

				var controller = ZControllerFactory.Create(boardProvider.ControllerID);
				var form = (ZForm)controller.ShowEditForm((BusinessObject)reloadedBoardProvider);

				if (form != null) // form can be null when the user has neither edit nor view rights to show the form
				{
					form.Closing += (s, e) =>
					{
						ReloadSlideshowAndBoardIfRequired(form);
					};

					if (!IsDisposed && !Disposing)
					{
						ZFormModaliser.Show(form, this);
					}
					else
					{
						form.Show();
					}
				}
			});
		}

		void ReloadSlideshowAndBoardIfRequired(ZForm form)
		{
			var businessEntity = form.BusinessEntity;

			if (businessEntity is IBMBoardSlideshow)
			{
				ReloadSlideshowViewModel();
				ReloadBoardOnClosedConfigForm();
			}
			else if (businessEntity is IBMBoard bmBoard && IsReloadRequired(bmBoard))
			{
				SlideShowViewModel.ReloadCurrentBoardViewModel();
				ReloadBoardOnClosedConfigForm();
			}
		}

		#endregion

		#region Board Meeting Mode

		public bool IsInBoardMeeting => SlideShowViewModel.IsInBoardMeeting;

		void BoardMeetingModeButton_Click(object sender, EventArgs e)
		{
			ToggleBoardMeetingMode();
		}

		void ToggleBoardMeetingMode()
		{
			if (!IsInBoardMeeting)
			{
				EnterBoardMeetingMode();
			}
			else
			{
				LeaveBoardMeetingMode();
			}
		}

		public void EnterBoardMeetingMode()
		{
			if (!IsInBoardMeeting)
			{
				PauseRefresh();

				var filter = new BoardMeetingModeFilter
				{
					RestoreVisualStateAfterFilterRemovedAction = UndoBoardMeetingModeFilter
				};

				controlsPanel.ToggleBoardMeetingButton();
				SlideShowViewModel.FilterManager.ApplyFilter(filter);

				if (boardMeetingModeForm == null)
				{
					boardMeetingModeForm = new BoardMeetingModeForm(this);
					boardMeetingModeForm.Show();
				}
			}
		}

		public void LeaveBoardMeetingMode()
		{
			controlsPanel.ToggleBoardMeetingButton();
			SlideShowViewModel.FilterManager.RemoveFilters(typeof(BoardMeetingModeFilter), shouldRefresh: true);

			ResumeRefresh();
		}

		void UndoBoardMeetingModeFilter()
		{
			DisposeBoardMeetingForm();

			if (autoRefresh != null)
			{
				autoRefresh.IsPaused = false;
			}
		}

		void DisposeBoardMeetingForm()
		{
			if (boardMeetingModeForm != null)
			{
				boardMeetingModeForm.Dispose();
				boardMeetingModeForm = null;
			}
		}

		BoardMeetingModeForm boardMeetingModeForm;

		#endregion

		#region Resize

		void VisualBoardForm_ResizeBegin(object sender, EventArgs e)
		{
			this.SuspendLayout();
		}

		void VisualBoardForm_ResizeEnd(object sender, EventArgs e)
		{
			this.ResumeLayout();
			ControlsPanel_LocationChanged(sender, e);
		}

		void ControlsPanel_LocationChanged(object sender, EventArgs e)
		{
			if (controlsPanel != null && !ClientRectangle.IntersectsWith(controlsPanel.Bounds) && ClientRectangle.Height != 0 && ClientRectangle.Width != 0)
			{
				var controlsPanelPointX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ClientRectangle.Width - controlsPanel.Width);

				controlsPanel.Location = ControlDpiScalingHelper.NewScaledPoint(controlsPanelPointX - 3, 2, true);
			}
		}

		#endregion

		#region AddOverlayControl

		public void AddOverlayControl(Control control, Point relativeStartingLocation, Point relativeOffset, int xJumpBackPixels)
		{
			var location = this.PointToClient(relativeStartingLocation);
			location = ControlDpiScalingHelper.NewScaledPoint(location.X + relativeOffset.X, location.Y + relativeOffset.Y, false);

			if (location.X + control.Width > this.Width)
			{
				var proposedXPosition = location.X - control.Width - xJumpBackPixels;
				location = ControlDpiScalingHelper.NewScaledPoint(Math.Max(0, proposedXPosition), location.Y, false);
			}

			var scaledBottomOfKickForm = ControlDpiScalingHelper.ScaleToCurrentDpiY(BottomOfFormKick);
			if (location.Y + control.Height > this.Height - scaledBottomOfKickForm)
			{
				var proposedYPosition = this.Height - control.Height - scaledBottomOfKickForm;
				location = ControlDpiScalingHelper.NewScaledPoint(location.X, proposedYPosition, false);
			}

			control.Location = location;
			if (!control.IsDisposed)
			{
				Controls.Add(control);
				control.BringToFront();
				overlayControls.Add(control);
				control.Disposed += OverlayControl_Disposed;
			}
		}

		void OverlayControl_Disposed(object sender, EventArgs e)
		{
			if (sender is Control control && overlayControls.Contains(control))
			{
				control.Disposed -= OverlayControl_Disposed;
				Controls.Remove(control);
				overlayControls.Remove(control);
				PerformLayout();
			}
		}

		readonly List<Control> overlayControls = new List<Control>();

		const int BottomOfFormKick = 40;

		#endregion

		#region PushToNewSection

		public bool PushToNewSection(Control control)
		{
			var component = GetBoardSectionForMovedControl(control);
			var result = component != null && component.AcceptDraggedControl(control);

			if (result)
			{
				control.Dispose();
			}

			return result;
		}

		IBoardSectionControl GetBoardSectionForMovedControl(Control control)
		{
			var topLeftPoint = control.Location;
			var bottomRightPoint = ControlDpiScalingHelper.NewScaledPoint(control.Left + control.Width, control.Top + control.Height, false);

			return (
				from componentControl in GetSectionControls()
				let x = (Control)componentControl
				where x.Bounds.Contains(topLeftPoint) || x.Bounds.Contains(bottomRightPoint)
				select componentControl
				).FirstOrDefault();
		}

		#endregion

		#region Filters

		void HookEventsToCurrentBoard()
		{
			if (viewModel != null)
			{
				SlideShowViewModel.FilterManager.ApplyingFilters += Filters_ApplyingFilters;
				SlideShowViewModel.FilterManager.FiltersUpdated += Filters_FiltersUpdated;
			}
		}

		void UnhookFilterEventsFromCurrentBoard()
		{
			if (viewModel != null)
			{
				SlideShowViewModel.FilterManager.ApplyingFilters -= Filters_ApplyingFilters;
				SlideShowViewModel.FilterManager.FiltersUpdated -= Filters_FiltersUpdated;
			}
		}

		void Filters_ApplyingFilters(object sender, EventArgs e)
		{
		}

		void Filters_FiltersUpdated(object sender, EventArgs e)
		{
			controlsPanel.ShowFilterButton(viewModel.FilterManager.AppliedAndChildFilters);
		}

		#endregion

		#region IVisualBoard Members

		void IVisualBoardForm.NotifyFullReloadRequired()
		{
			ReloadBoard();
		}

		#endregion

		#region ICustomerServiceMenuSectionCodeOverridable Members

		string ICustomerServiceMenuSectionCodeOverridable.SectionCode =>
			ModuleTreeCustomerServiceMenuSectionList.Codes.BufferManagement;

		#endregion

		#region For Test

#if DEBUG

		public IAutoRefresher AutoRefreshForTest
		{
			get { return autoRefresh; }
		}

		public MenuItem HelpMenuItemForTest => HelpMenuItem.MenuItems[0];

		public void CloseFormSafelyWhenShownOnBackgroundThreadWithoutSleep_ForTest(AutoResetEvent openFormMethodCompletedEvent)
		{
			DialogResult = DialogResult.OK;
			if (!openFormMethodCompletedEvent.WaitOne(5000))
			{
				throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, "Timeout has occurred before openForm-Method-Completed event!"));
			}
		}

		public void NextButton_ExpandAndPerformClickOnBackgroundThread()
		{
			this.Invoke(new Action(() =>
			{
				this.controlsPanel.Expand(); // performClick while control is not visible / cannot be selected, will not trigger the click event
				this.controlsPanel.NextButton.PerformClick();
			}));
		}

		public Dictionary<ZGuid, KTableLayoutPanel> GetPanelCacheByBoard()
		{
			return panelCacheByBoard;
		}

		public void ShowPreviousChannel_ForTest()
		{
			var message = new Message();
			HandlePressedKeys(ref message, BoardMeetingModeShortcuts.PreviousChannelKeyboardShortcut);
		}

		public void ShowNextChannel_ForTest()
		{
			var message = new Message();
			HandlePressedKeys(ref message, BoardMeetingModeShortcuts.NextChannelKeyboardShortcut);
		}

		public void SetVisualBoardTableLayoutPanel_ForTest(KTableLayoutPanel newLayoutPanel)
		{
			Controls.Remove(VisualBoardTableLayoutPanel);
			Controls.Add(newLayoutPanel);
			VisualBoardTableLayoutPanel = newLayoutPanel;
		}

#if !WINZOR
		public HiddenFormWtsSessionChangeMonitor SessionChangeMonitor_ExposedForTest => sessionChangeMonitor;
#endif

#endif
		#endregion
	}
}
