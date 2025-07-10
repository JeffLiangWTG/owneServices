using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.VisualBoards.GUI
{
	public partial class BoardControlsPanel : ZPanel
	{
#if DEBUG
		public
#endif
		const int ControlGripSize = 0;

#if DEBUG
		public
#endif
		const int ButtonPadding = 2;
		const int hideInterval = 2000;
		const int buttonMinSize = 28;
		const int expandButtonMinSize = 10;
		const int searchBoxWidth = 200;
#if DEBUG
		public
#endif
		const int countdownWidth = 35;

		static MultilingualString PauseBoardRefreshToolTipText
		{
			get { return ResString.GetMultilingualString("2228a310-eb4c-432c-8893-60246d3f2942", "Pause board refresh"); }
		}
		static MultilingualString PauseSlideShowTransitionsToolTipText
		{
			get { return ResString.GetMultilingualString("110cdbe1-57a6-4db2-9113-7b6b1ecaf4f7", "Pause slide show transitions"); }
		}
		static MultilingualString ResumeBoardRefreshToolTipText
		{
			get { return ResString.GetMultilingualString("5873c14e-0a66-469b-9dc8-a8bbc769f835", "Resume board refresh"); }
		}
		static MultilingualString ResumeSlideShowTransitionsToolTipText
		{
			get { return ResString.GetMultilingualString("202dc199-5b41-4ef7-9139-80a95c7fd79e", "Resume slide show transitions"); }
		}

		readonly IWindowsTimer hoverTimer;
		readonly bool isSlideShow;
		readonly bool hasMultipleBoards;
		readonly bool hasComponentSections;
		bool isBoardMeetingMode;
		bool isFilterApplied;
		bool isSearchFilterApplied;
		readonly int parentWidth;
		readonly bool showOpenInBrowserButton;

		public bool IsExpanded { get; private set; }

		public BoardControlsPanel(BoardSlideshowViewModel slideShowViewModel, int parentWidth)
		{
			this.DoubleBuffered = true;
			this.SetStyle(ControlStyles.ResizeRedraw, true);
			this.AutoSize = true;
			this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			this.BorderStyle = BorderStyle.FixedSingle;
			this.hoverTimer = ObjectFactory.Get<IWindowsTimer>();
			this.hoverTimer.Interval = hideInterval;
			this.FilterButton = new FilterButton(slideShowViewModel);
			this.BoardPickerButton = new BoardPickerButton();
			this.isSlideShow = slideShowViewModel.IsSlideshow;
			this.hasMultipleBoards = slideShowViewModel.HasMultipleBoards;
			this.hasComponentSections = slideShowViewModel.HasComponentSections;
			this.parentWidth = parentWidth;
			showOpenInBrowserButton = ObjectFactory.Get<IBMSRegistry>().PAVEOnTheWeb;
			ControlDpiScalingHelper.SetHeight(this, buttonMinSize, true);

			InitializeControls();

			if (this.isSlideShow)
			{
				ConfigButton.ToolTipCaption = ResString.GetMultilingualString("B64CCC3A-BDDE-4F79-84A4-0283E940082C", "Edit Slide Show");
			}
		}

		#region Controls

		public ZSearchBox SearchBox;

		public FilterButton FilterButton { get; set; }
		public ZButton BoardMeetingButton { get; set; }
		public ZButton RefreshButton { get; set; }
		public BoardPickerButton BoardPickerButton { get; set; }
		public ZButton OpenInBrowserButton { get; set; }
		public ZButton ConfigButton { get; set; }
		public ZButton NextButton { get; set; }
		public ZButton PreviousButton { get; set; }
		public ZButton PauseResumeButton { get; set; }
		public ZButton ExpandCollapseButton { get; set; }

		public ZLabel CountdownLabel;

		#region EventHandlers

		#region Add EventHadlers

		public void AddBoardMeetingClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				BoardMeetingButton.Click += handler;
			}
		}

		public void AddConfigClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				ConfigButton.Click += handler;
			}
		}

		public void AddOpenInBrowserClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				OpenInBrowserButton.Click += handler;
			}
		}

		public void AddRefreshClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				RefreshButton.Click += handler;
			}
		}

		public void AddPreviousClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				PreviousButton.Click += handler;
			}
		}

		public void AddNextClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				NextButton.Click += handler;
			}
		}

		public void AddPauseResumeClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				PauseResumeButton.Click += handler;
			}
		}

		public void AddSearchPerformedEvent(EventHandler<SearchEventArgs> handler)
		{
			if (handler != null)
			{
				SearchBox.SearchPerformed += handler;
			}
		}

		void ExpandCollapseButton_Click(object sender, EventArgs e)
		{
			var buttonImage = !IsExpanded ? Properties.Resources.double_arrow_right : Properties.Resources.double_arrow_left;
			ExpandCollapseButton.BackgroundImage = buttonImage;
			if (!IsExpanded)
			{
				Expand();
			}
			else
			{
				Collapse(true);
			}
			Refresh();
		}

		#endregion

		#region Remove EventHandlers

		public void RemoveBoardMeetingClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				BoardMeetingButton.Click -= handler;
			}
		}

		public void RemoveConfigClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				ConfigButton.Click -= handler;
			}
		}

		public void RemoveOpenInBrowserClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				OpenInBrowserButton.Click -= handler;
			}
		}

		public void RemoveRefreshClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				RefreshButton.Click -= handler;
			}
		}

		public void RemovePreviousClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				PreviousButton.Click -= handler;
			}
		}

		public void RemoveNextClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				NextButton.Click -= handler;
			}
		}

		public void RemovePauseResumeClickEvent(EventHandler handler)
		{
			if (handler != null)
			{
				PauseResumeButton.Click -= handler;
			}
		}

		public void RemoveSearchPerformedEvent(EventHandler<SearchEventArgs> handler)
		{
			if (handler != null)
			{
				SearchBox.SearchPerformed -= handler;
			}
		}

		#endregion

		#endregion

		void InitializeControls()
		{
			CreateExpandCollapseButton();
			ExpandCollapseButton.Click += ExpandCollapseButton_Click;

			if (hasComponentSections)
			{
				CreateSearchBox();
				FilterButton = SetupControlButton(FilterButton, string.Empty, ResString.GetMultilingualString("c0cfacef-c026-4a71-93f5-8afdb8d3a833", "Filters"), 103);
				FilterButton.Name = "FilterButton";
				BoardMeetingButton = CreateControlButton(string.Empty, ResString.GetMultilingualString("194839D9-9E1C-476F-9E61-5F9A1F5B5083", "Board Meeting (CTRL-M)"), 104, Properties.Resources.meeting_mode);
				BoardMeetingButton.Name = "BoardMeetingModeButton";
			}
			else
			{
				FilterButton.Visible = false;
			}

			BoardPickerButton = SetupControlButton(BoardPickerButton, string.Empty, ResString.GetMultilingualString("AF01D209-43B1-41B6-999A-1283AE1D7E3A", "Open Another Board"), 105);
			BoardPickerButton.Name = "BoardPickerButton";
			if (showOpenInBrowserButton)
			{
				OpenInBrowserButton = CreateControlButton(string.Empty, ResString.GetMultilingualString("dea121e3-235d-4269-ae58-38e052aed204", "Open Board in Browser"), 106, Properties.Resources.web);
				OpenInBrowserButton.Name = "OpenInBrowserButton";
			}
			ConfigButton = CreateControlButton(string.Empty, ResString.GetMultilingualString("d143fbd2-5a3b-4946-9d05-af9912c86c04", "Edit Board"), 107, Properties.Resources.config);
			ConfigButton.Name = "ConfigButton";
			RefreshButton = CreateControlButton(string.Empty, MultilingualString.Join("\r\n", ResString.GetMultilingualString("6133f191-43dd-473b-8253-ad8cc8ef7883", "Refresh (F5)"), ResString.GetMultilingualString("50e51450-544d-40d7-8be9-8ef3d9d7880c", "Reload (Shift+F5)")), 108, Properties.Resources.refresh);
			RefreshButton.Name = "RefreshButton";

			if (isSlideShow)
			{
				PreviousButton = CreateControlButton(string.Empty, ResString.GetMultilingualString("b4c90581-63ef-46b9-a3e7-9d2c9b01b561", "Previous Slide"), 109, Properties.Resources.step_back);
				PreviousButton.Name = "PreviousButton";

				NextButton = CreateControlButton(string.Empty, ResString.GetMultilingualString("66ae301f-7e33-4b8b-b968-9db1512037d9", "Next Slide"), 110, Properties.Resources.step_forward);
				NextButton.Name = "NextButton";
			}

			if (hasMultipleBoards)
			{
				PauseResumeButton = CreateControlButton(string.Empty, ResString.GetMultilingualString("122A39D9-9E1C-476F-9E61-5F9A1F5B5127", "Pause Slide Show"), 111, Properties.Resources.pause);
			}
			else
			{
				PauseResumeButton = CreateControlButton(string.Empty, ResString.GetMultilingualString("68e5298b-0cf3-4b02-9de0-e056a5b07c76", "Pause board refresh"), 111, Properties.Resources.pause);
			}

			PauseResumeButton.Name = "PauseResumeButton";

			CreateCountDownBox();
			ShowHideExtraControls(true);

			ControlDpiScalingHelper.SetWidth(this, ControlDpiScalingHelper.ScaleToCurrentDpiX(3 * ButtonPadding + ControlGripSize) + CountdownLabel.Width, false);
			this.Location = ControlDpiScalingHelper.NewScaledPoint(parentWidth - ControlDpiScalingHelper.UnscaleFromCurrentDpiX(this.Width) - 3, 2, true);
		}

		#region Create Controls

		void CreateSearchBox()
		{
			SearchBox = new ZSearchBox();
			ControlDpiScalingHelper.SetWidth(SearchBox, searchBoxWidth, true);
			SearchBox.Anchor = AnchorStyles.Left;
			ControlDpiScalingHelper.SetTop(SearchBox, 4, true);
			SearchBox.Margin = new Padding();
			this.Controls.Add(SearchBox);
			SearchBox.BringToFront();
		}

		[SuppressMessage("CargoWiseOne", "CW1046", Justification = "The control is not a button")]
		void CreateCountDownBox()
		{
			CountdownLabel = new ZLabel();
			CountdownLabel.Anchor = AnchorStyles.Left;
			CountdownLabel.Name = "TimeUntilRefresh";
			CountdownLabel.Size = ControlDpiScalingHelper.NewScaledSize(countdownWidth, buttonMinSize, true);
			CountdownLabel.Font = new Font("Arial", 8.2f, FontStyle.Bold);
			CountdownLabel.Margin = new Padding();
			this.Controls.Add(CountdownLabel);

			if (hasMultipleBoards)
			{
				ToolTipService.SetToolTip(CountdownLabel, Res.GetString("88b81a4f-a1c9-42e7-8c8d-b492e00c9d3f", "Time to next slide"));
			}
			else
			{
				ToolTipService.SetToolTip(CountdownLabel, Res.GetString("48bc56d0-ee92-40a4-963a-d299955172ed", "Time to board refresh"));
			}
			CountdownLabel.BringToFront();
		}

		ZButton CreateControlButton(string text, MultilingualString mouseHoverText, int tabIndex, Bitmap icon = null)
		{
			return SetupControlButton(new ZButton(), text, mouseHoverText, tabIndex, icon);
		}

		TButton SetupControlButton<TButton>(TButton button, string text, MultilingualString mouseHoverText, int tabIndex, Bitmap icon = null)
			where TButton : ZButton
		{
			button.Anchor = AnchorStyles.Left;
			button.BackgroundImageLayout = ImageLayout.Zoom;
			button.Name = mouseHoverText;

			button.Size = icon == null
				? ControlDpiScalingHelper.NewScaledSize(buttonMinSize, buttonMinSize)
				: ControlDpiScalingHelper.NewScaledSize(Math.Max(buttonMinSize, buttonMinSize), buttonMinSize, true);
			button.TabIndex = tabIndex;
			button.Text = text;
			button.UseVisualStyleBackColor = true;
			button.Margin = new Padding();

			button.ToolTipCaption = mouseHoverText;

			if (icon != null)
			{
				button.BackgroundImage = icon;
			}

			this.Controls.Add(button);
			button.BringToFront();
			return button;
		}

		void CreateExpandCollapseButton()
		{
			var icon = IsExpanded ? Properties.Resources.double_arrow_right : Properties.Resources.double_arrow_left;
			ExpandCollapseButton = new ZButton();
			ExpandCollapseButton.FlatStyle = FlatStyle.Flat;
			ExpandCollapseButton.FlatAppearance.BorderSize = 0;

			ExpandCollapseButton.Size = icon == null
				? ControlDpiScalingHelper.NewScaledSize(expandButtonMinSize, expandButtonMinSize)
				: ControlDpiScalingHelper.NewScaledSize(Math.Max(expandButtonMinSize, expandButtonMinSize), expandButtonMinSize, true);

			ExpandCollapseButton.Anchor = AnchorStyles.Left;
			ExpandCollapseButton.BackgroundImageLayout = ImageLayout.Zoom;
			ExpandCollapseButton.Location = ControlDpiScalingHelper.NewScaledPoint(2, 9, true);
			ExpandCollapseButton.TabIndex = 101;
			ExpandCollapseButton.Text = string.Empty;

			ExpandCollapseButton.Margin = new Padding();

			if (icon != null)
			{
				ExpandCollapseButton.BackgroundImage = icon;
			}

			this.Controls.Add(ExpandCollapseButton);
			ExpandCollapseButton.BringToFront();
		}

		#endregion

		#region Enable / Disable Controls

		IEnumerable<Control> DisableableControls
		{
			get
			{
				yield return PauseResumeButton;
				yield return PreviousButton;
				yield return NextButton;
			}
		}

		public void DisableBoardMovementButtons()
		{
			DisableableControls.ForEach(c => c.Enabled = false);
		}

		public void EnableBoardMovementButtons()
		{
			DisableableControls.ForEach(c => c.Enabled = true);
		}

		#endregion

		#region Pause / Resume

		public void PauseResume(bool pause)
		{
			if (pause)
			{
				PauseResumeButton.BackgroundImage = Properties.Resources.play;

				if (hasMultipleBoards)
				{
					PauseResumeButton.ToolTipCaption = ResumeSlideShowTransitionsToolTipText;
				}
				else
				{
					PauseResumeButton.ToolTipCaption = ResumeBoardRefreshToolTipText;
				}
			}
			else
			{
				PauseResumeButton.BackgroundImage = Properties.Resources.pause;

				if (hasMultipleBoards)
				{
					PauseResumeButton.ToolTipCaption = PauseSlideShowTransitionsToolTipText;
				}
				else
				{
					PauseResumeButton.ToolTipCaption = PauseBoardRefreshToolTipText;
				}
			}
		}

		#endregion

		#region Counter

		public void UpdateTimerValue(TimeSpan remainingTime)
		{
			string minutes = remainingTime.TotalMinutes < 10 ? "0" + remainingTime.Minutes : ((int)remainingTime.TotalMinutes).ToString(CultureInfo.InvariantCulture);
			CountdownLabel.Text = string.Format(CultureInfo.InvariantCulture, "{0}:{1:D2}", minutes, remainingTime.Seconds);  // formatting string
			ControlDpiScalingHelper.SetWidth(CountdownLabel, CountdownLabel.PreferredWidth, false);
		}

		#endregion

		#region Search Box

		public void SetSearchTerm(string term)
		{
			this.SearchBox.SearchTerm = term;
			this.FilterButton.Focus();
		}

		public void ClearSearchBox()
		{
			this.SearchBox.ClearSearchText();
		}

		#endregion

		#endregion

		#region Overrides

		protected override Control.ControlCollection CreateControlsInstance()
		{
			return new ControlCollection(this);
		}

		#endregion

		#region Resizing

#if DEBUG
		public
#endif
 void Collapse(bool forceCollapse = false)
		{
			if (!this.IsDisposed && ((!IsExpanded && !this.ClientRectangle.Contains(this.PointToClient(Control.MousePosition))) || forceCollapse) && (!hasComponentSections || !SearchBox.ContainsFocus))
			{
				this.hoverTimer.Stop();
				IsExpanded = false;
				ShowHideExtraControls(true);
			}
		}

#if DEBUG
		public
#endif
 void Expand()
		{
			if (!this.IsDisposed && !IsExpanded)
			{
				IsExpanded = true;
				ShowHideExtraControls(false);
			}
		}

		void ShowHideExtraControls(bool hide)
		{
			if (hasComponentSections)
			{
				SearchBox.Visible = isSearchFilterApplied || !hide;
				BoardMeetingButton.Visible = isBoardMeetingMode || !hide;
				FilterButton.Visible = isFilterApplied || !hide;
			}

			BoardPickerButton.Visible = !hide;
			ConfigButton.Visible = !hide;
			RefreshButton.Visible = !hide;

			if (showOpenInBrowserButton && OpenInBrowserButton != null)
			{
				OpenInBrowserButton.Visible = !hide;
			}
			if (isSlideShow && NextButton != null && PreviousButton != null)
			{
				NextButton.Visible = !hide;
				PreviousButton.Visible = !hide;
			}
			PauseResumeButton.Visible = !hide;

			ReArrangeControls(hide);
		}

		void ReArrangeControls(bool collapse)
		{
			List<Control> controls;

			if (collapse)
			{
				controls = new List<Control>() { CountdownLabel };
				if (hasComponentSections)
				{
					if (isFilterApplied)
					{
						controls.Insert(controls.IndexOf(CountdownLabel), FilterButton);
						if (isSearchFilterApplied)
						{
							controls.Insert(controls.IndexOf(FilterButton), SearchBox);
						}
					}
					if (isBoardMeetingMode)
					{
						controls.Insert(controls.IndexOf(CountdownLabel), BoardMeetingButton);
					}
				}
			}
			else
			{
				controls = hasComponentSections ?
					new List<Control> { SearchBox, FilterButton, BoardMeetingButton, BoardPickerButton, ConfigButton, RefreshButton, PauseResumeButton, CountdownLabel } :
					new List<Control> { BoardPickerButton, ConfigButton, RefreshButton, PauseResumeButton, CountdownLabel };

				if (isSlideShow)
				{
					controls.InsertRange(hasComponentSections ? 6 : 3, new List<Control> { PreviousButton, NextButton });
				}

				if (showOpenInBrowserButton)
				{
					controls.Insert(hasComponentSections ? 4 : 1, OpenInBrowserButton);
				}
			}
			controls.Insert(0, ExpandCollapseButton);

			PlaceControlsFromLeftToRight(ControlGripSize, controls);
		}

		void PlaceControlsFromLeftToRight(int startLeftMargin, List<Control> controls)
		{
			startLeftMargin = 0;

			if (controls.Any())
			{
				for (int i = 0; i < controls.Count; i++)
				{
					int distance = i == 0 ? startLeftMargin + ButtonPadding : ControlDpiScalingHelper.UnscaleFromCurrentDpiX(controls[i - 1].Right) + ButtonPadding;
					ControlDpiScalingHelper.SetLeft(controls[i], distance, true);
				}
			}
		}

		public void ToggleBoardMeetingButton()
		{
			isBoardMeetingMode = !isBoardMeetingMode;
			if (!IsExpanded)
			{
				BoardMeetingButton.Visible = isBoardMeetingMode;
			}
			ReArrangeControls(!IsExpanded);
		}

		public void ShowFilterButton(IEnumerable<IBoardFilter> filters)
		{
			isFilterApplied = filters.Any();
			isSearchFilterApplied = filters.Any(x => x is SearchFilter);
			if (!IsExpanded)
			{
				FilterButton.Visible = isFilterApplied;
				SearchBox.Visible = isSearchFilterApplied;

				if (isBoardMeetingMode && !isFilterApplied)
				{
					isBoardMeetingMode = false;
					BoardMeetingButton.Visible = false;
				}
			}
			ReArrangeControls(!IsExpanded);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			ExpandCollapseButton.Click -= ExpandCollapseButton_Click;
			base.Dispose(isNotFinalizing);
			if (!FilterButton.IsDisposed)
			{
				FilterButton.Dispose();
			}
			hoverTimer.Dispose();
		}

		#endregion
	}
}
