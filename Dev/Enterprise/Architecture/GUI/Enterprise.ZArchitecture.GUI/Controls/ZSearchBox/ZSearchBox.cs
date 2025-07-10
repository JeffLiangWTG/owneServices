using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Drawing.Colors;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Drawing;
using Enterprise.ZArchitecture.GUI.Forms;

namespace Enterprise.ZArchitecture.GUI.SearchBox
{
	[ToolboxItem(false)]
	public partial class ZSearchBox : KUserControl // We don't need ZArch here
	{
		public delegate IEnumerable<IDisplayItem> SearchDelegate(string query);

		public SearchDelegate Search;

		internal string lastSearch;
#if !WINZOR
		MouseHook globalMouseHook;
#endif
		public bool AutoSearch { get; set; }

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Good luck translating this")]
		const string SearchIcon = "🔎";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Good luck translating this")]
		const string CloseIcon = "🗙";

		public ZSearchBoxResults SearchResultsDisplay => searchResultsDisplay;

		// UserControl.DefaultBackColor is 'readonly' so we'll hide it and use a writeable one that a user can set in the designer
		public new Color DefaultBackColor { get; set; } = Color.LightGray;
		public Color SearchingBackColor { get; set; } = ColourHelper.ShiftBrightness(Color.LightYellow, -0.15f);
		Color OriginalBackColor;

		public Timer AutoSearchTimer { get; set; } = new Timer();
		public int AutoSearchDelay { get; set; } = 500;

		public ZSearchBox()
		{
			InitializeComponent();
			Init();
			ResizeAndRepositionComponents();
			ResizeAndRepositionSearchResults();
			ResetSearchBox();
			HideResults();
			HookMouseEvents(this);
		}

		public void Init()
		{
			OriginalBackColor = tbSearch.BackColor;
			AutoSearchTimer.Interval = AutoSearchDelay;
			AutoSearchTimer.Tick += (a, b) =>
			{
				AutoSearchTimer.Stop();
				PerformSearch();
			};

			searchResultsDisplay.OnClose += (sender, e) => HideResults();
			searchResultsDisplay.BackColor = ColourHelper.ShiftBrightness(DefaultBackColor, -0.25f);
			btnSearch.Text = SearchIcon;
			searchResultsDisplay.VisibleChanged += (sender, e) => btnSearch.Text = searchResultsDisplay.Visible ? CloseIcon : SearchIcon;
			tbSearch.Clear();

#if WINZOR

			btnSearch.PreventDefaultMouseDown = true;
			tbSearch.LostFocus += (_, _) => HideResults();
#else
			globalMouseHook = new MouseHook();
			globalMouseHook.MouseDown += CloseResultsWhenMouseClicksOutsideResults;
#endif
		}

#if !WINZOR
		void CloseResultsWhenMouseClicksOutsideResults(object sender, MouseHookEventArgs e)
		{
			if (!ClientRectangle.Contains(PointToClient(Cursor.Position))
				&& !SearchResultsDisplay.ClientRectangle.Contains(SearchResultsDisplay.PointToClient(Cursor.Position)))
			{
				HideResults();
			}
		}
#endif

		void HookMouseEvents(Control c)
		{
			c.MouseEnter += SearchBox_MouseEnter_SetHoverColour;
			c.MouseLeave += SearchBox_MouseLeave_ClearHoverColour;
			foreach (Control cc in c.Controls)
			{
				HookMouseEvents(cc);
			}
		}

		public void Clear() => searchResultsDisplay.SetDataSource(null);

		#region Events

		#region Search Button

		void btnSearch_Click(object sender, EventArgs e)
		{
			if (searchResultsDisplay.Visible)
			{
				HideResults();
			}
			else
			{
				PerformSearch();
			}
		}

		#endregion

		#region Search Box

		void SearchBox_BackColorChanged(object sender, EventArgs e)
		{
			btnSearch.ApplyTheme(CreateAutoButtonTheme());
		}

		void SearchBox_Click(object sender, EventArgs e)
		{
			_ = tbSearch.Focus();
			if (!string.IsNullOrEmpty(tbSearch.Text) && searchResultsDisplay.HasResults)
			{
				ShowResults();
			}
		}

		void SearchBox_EnabledChanged(object sender, EventArgs e)
		{
			tbSearch.Clear();
			if (!Enabled)
			{
				HideResults();
			}
		}

		void SearchBox_ForeColorChanged(object sender, EventArgs e)
		{
			btnSearch.ApplyTheme(CreateAutoButtonTheme());
		}

		void SearchBox_Leave(object sender, EventArgs e)
		{
			if (!ContainsFocus && !searchResultsDisplay.ContainsFocus)
			{
				ResetSearchBox();
			}
		}

		void SearchBox_MouseEnter_SetHoverColour(object sender, EventArgs e)
		{
			BackColor = ColourHelper.ShiftBrightness(BackColor, -0.15f);
		}

		void SearchBox_MouseLeave_ClearHoverColour(object sender, EventArgs e)
		{
			BackColor = DefaultBackColor;
		}

		void SearchBox_Layout(object sender, LayoutEventArgs e)
		{
			ResizeAndRepositionComponents();
			ResizeAndRepositionSearchResults();
		}

		#endregion

		#region Search Text Box

		void tbSearch_Enter(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(tbSearch.Text) && searchResultsDisplay.HasResults)
			{
				ShowResults();
			}
		}

		void tbSearch_TextChanged(object sender, EventArgs e)
		{
			if (AutoSearch && !string.IsNullOrEmpty(tbSearch.Text))
			{
				AutoSearchTimer.Stop();
				AutoSearchTimer.Start();
			}
		}

		void tbSearch_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (SearchResultsDisplay.Visible && lastSearch == tbSearch.Text)
			{
				searchResultsDisplay.SendKeyPress(e);
				return;
			}

			if (e.KeyChar == (char)Keys.Enter)
			{
				PerformSearch();
				e.Handled = true;
			}

			if (e.KeyChar == (char)Keys.Escape)
			{
				if (string.IsNullOrEmpty(tbSearch.Text))
				{
					_ = Parent.Focus();
				}
				else
				{
					tbSearch.Clear();
				}
				e.Handled = true;
			}
		}

		void tbSearch_KeyDown(object sender, KeyEventArgs e)
		{
			searchResultsDisplay.SendKeyDown(e);
		}

		#endregion

		#endregion

		#region Functionality

		protected internal ButtonTheme CreateAutoButtonTheme() => new ButtonTheme(
			ForeColor, BackColor,
			Color.LightGray, ColourHelper.ShiftBrightness(BackColor, -0.1f),
			Color.White, ColourHelper.ShiftBrightness(BackColor, -0.20f),
			0);

		protected internal virtual void ShowResults()
		{
			if (ParentForm != null && !ParentForm.Controls.Contains(searchResultsDisplay))
			{
				ParentForm.Controls.Add(searchResultsDisplay);
			}

			ResizeAndRepositionSearchResults();
			searchResultsDisplay.Visible = true;
			searchResultsDisplay.BringToFront();
			_ = tbSearch.Focus();
#if !WINZOR
			globalMouseHook.Install();
#endif
		}

		protected internal virtual void HideResults()
		{
#if !WINZOR
			globalMouseHook.Uninstall();
#endif
			SearchResultsDisplay.Visible = false;
		}

		protected internal void ResetSearchBox()
		{
			tbSearch.Clear();
			BackColor = DefaultBackColor;
			ActiveControl = null; // removes focus/caret from text box
		}

		const int SearchResultsDisplayDefaultHeight = 384; // couldn't find a good algorithmic way to set this, so hardcoding should be fine for now
		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "all values are scaled")]
		protected internal void ResizeAndRepositionSearchResults()
		{
			if (searchResultsDisplay == null)
			{
				return;
			}

			var offset = ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			if (ParentForm?.WindowState == FormWindowState.Normal)
			{
				if (ParentForm is ZMainForm parentForm)
				{
					offset = new Point(parentForm.BorderSize, parentForm.BorderSize);
				}
			}

			searchResultsDisplay.Width = Width * 2;
			searchResultsDisplay.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(SearchResultsDisplayDefaultHeight);
			searchResultsDisplay.Location = new Point(Left - (searchResultsDisplay.Width - Width) + offset.X, Bottom + offset.Y);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "all values are scaled")]
		protected internal void ResizeAndRepositionComponents()
		{
			btnSearch.Height = pnSearch.Height;
			btnSearch.Width = btnSearch.Height;
			pnSearchBox.Width = Width - btnSearch.Width;
			tbSearch.Width = pnSearchBox.Width;
		}

		protected internal virtual void PerformSearch()
		{
			var searchTerm = tbSearch.Text;
			if (searchTerm.Equals(lastSearch))
			{
				ShowResults();
				return;
			}

			lastSearch = searchTerm;

			searchResultsDisplay.SetDataSource(new[]
			{
				DisplayItemFactory.CreateHeadingItem("Searching...")
			});

			searchTask = Task.Run(() => SearchCore(searchTerm))
				.ContinueWith(
					task => HandleResults(searchTerm, task.Result),
					TaskScheduler.FromCurrentSynchronizationContext());
		}

		internal Task searchTask;

		List<IDisplayItem> SearchCore(string searchTerm)
		{
			var results = new List<IDisplayItem>();
			if (string.IsNullOrWhiteSpace(searchTerm))
			{
				results.Add(DisplayItemFactory.CreateErrorItem(ResString.GetMultilingualString("DF9D9D40-9688-49A7-AC73-08D9F3DBF20B", "Empty search term")));
			}
			else
			{
				if (Search is null)
				{
					ErrorReporter.ReportOnce($"The OnSearch handler was null; no search results will ever be returned. The parent control needs to set this; check the control hierarchy: {ControlStatisticsDescription.GetDescription(this)}"); // This is a dev exception, user hopefully never sees this
				}
				else
				{
					try
					{
						// UX hint that search is occurring
						if (tbSearch.InvokeRequired)
						{
							Invoke(() => tbSearch.BackColor = SearchingBackColor);
						}
						else
						{
							tbSearch.BackColor = SearchingBackColor;
						}

						results = Search(searchTerm).ToList();
						if (!results.Any())
						{
							results.Add(DisplayItemFactory.CreateErrorItem(ResString.GetMultilingualString("8151BE52-9798-404C-90CA-A72937D8DBBA", "No results found")));
						}
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						results.Add(DisplayItemFactory.CreateErrorItem(ResString.GetMultilingualString("3A31F26A-47CC-4921-BA13-956A239858D5", "Unhandled error caught when calling Search")));
						ErrorReporter.ReportOnce("Unhandled exception caught when calling Search", e); // This is a dev exception, user hopefully never sees this
					}
					finally
					{
						if (tbSearch.InvokeRequired)
						{
							Invoke(() => tbSearch.BackColor = OriginalBackColor);
						}
						else
						{
							tbSearch.BackColor = OriginalBackColor;
						}
					}
				}
			}

			return results;
		}

		internal void HandleResults(string searchTerm, IEnumerable<IDisplayItem> results)
		{
			if (tbSearch.Text == searchTerm)
			{
				searchResultsDisplay.SetDataSource(results);
				ShowResults();
			}
		}

		#endregion
	}
}
