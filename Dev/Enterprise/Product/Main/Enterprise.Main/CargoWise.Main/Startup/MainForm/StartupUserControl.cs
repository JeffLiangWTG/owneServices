using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Main.Navigation;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Res = CargoWise.Main.Res;
using ResString = CargoWise.Main.ResString;

namespace Enterprise.Startup
{
	[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
	public partial class StartupUserControl : KUserControl
	{
		public StartupUserControl() : this(null) { }

		public StartupUserControl(BusinessObjectFactory factory)
		{
			try
			{
				newsViewModel = new NewsViewModel();
				if (factory is { IsOwnedByCurrentThread: false })
				{
					factory.TakeThreadOwnership();
					firstTimeFactory = factory;
				}

				panelCache = new KFlowLayoutPanel[3, 3];
				labelCache = new ZLabel[3, 3];
				InitializeComponent();
				if (DesignModeFinder.IsDesigning)
				{
					BackColor = Color.White;
					// LoadDesignerNews(); TODO if necessary
				}
				else
				{
					BackColor = SystemDataRegistry.Instance.ColorTheme.MainFormBackgroundColor;
					LoadNews();
				}

				SetupRefreshContextMenu();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Dispose();
				throw;
			}
		}

		void SetupRefreshContextMenu()
		{
			ContextMenu = new ContextMenu();
			var refreshNowMenuItem = new ZMenuItem(ResString.GetMultilingualString("8A04BAD9-B0F1-4BF0-94B0-2B509189C1D4", "Refresh Now"));
			refreshNowMenuItem.Click += (sender, args) => ReloadNews();
			ContextMenu.MenuItems.Add(refreshNowMenuItem);
		}

		#region UI Setup

		void LoadNews()
		{
			try
			{
				SuspendLayout();
				this.SuspendDrawing();
				StartupLayoutPanel.Controls.RemoveAndDisposeAll();
				AddLogos();
				SetupPanels();
			}
			catch (Exception ex)
			{
				StartupLayoutPanel.Controls.RemoveAndDisposeAll();
				var sqlEx = ex.GetFirstOccurrenceOfException<SqlException>();
				var friendlyMessage = sqlEx == null ? null : new DbErrorMatch(sqlEx).GetUserFriendlyMessage(Db.Connection);

				if (string.IsNullOrWhiteSpace(friendlyMessage))
				{
					throw;
				}

				Globals.Message.ShowError(friendlyMessage);
			}
			finally
			{
				this.ResumeDrawing();
				ResumeLayout(false);
				PerformLayout();
			}
		}

		internal Dictionary<string, bool> HideReadItems { get; set; }

#if DEBUG
		internal
#endif
		Dictionary<string, string[]> UnReadItems
		{ get; set; }

		List<Tuple<GlbReleaseNoteCombined, ZLinkLabel>> NoteWithLabels { get; set; }

		BusinessObjectFactory firstTimeFactory;

		void SetupPanels()
		{
			var factory = firstTimeFactory ?? new BusinessObjectFactory { NameForDebugging = "StartupUserControl.News" };
			HideReadItems = new Dictionary<string, bool>();
			UnReadItems = new Dictionary<string, string[]>();
			NoteWithLabels = new List<Tuple<GlbReleaseNoteCombined, ZLinkLabel>>();

			foreach (var panelConfig in NewsSections)
			{
				var shouldDoubleHeight = panelConfig.LayoutPanelID.StartsWith("Bottom-") && NewsSections.All(x => x.LayoutPanelID != panelConfig.LayoutPanelID.Replace("Bottom-", "Top-"));
				AddOrRefreshPanel(panelConfig, shouldDoubleHeight, GetNews(factory, panelConfig.SectionID, panelConfig.HideReadItems));

				if (!HideReadItems.ContainsKey(panelConfig.SectionID))
				{
					HideReadItems.Add(panelConfig.SectionID, panelConfig.HideReadItems);

					if (panelConfig.SectionID == NewsSectionTypeList.Codes.ProductUpdates)
					{
						if (!HideReadItems.ContainsKey(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes))
						{
							HideReadItems.Add(NewsSectionTypeList.Codes.TechnicalAdvisoryNotes, panelConfig.HideReadItems);
						}

						if (!HideReadItems.ContainsKey(NewsSectionTypeList.Codes.BorderWise))
						{
							HideReadItems.Add(NewsSectionTypeList.Codes.BorderWise, panelConfig.HideReadItems);
						}
					}
				}
			}

			firstTimeFactory = null;
		}

		public void ReloadNews()
		{
			try
			{
				SuspendLayout();
				this.SuspendDrawing();
				SetupPanels();
			}
			catch (System.Data.Common.DbException ex)
			{
				StartupLayoutPanel.Controls.RemoveAndDisposeAll();
				Globals.Message.ShowError(Res.GetString("27D92604-6559-4AB4-93F5-0AB466EC23DD",
					"The news can not be loaded. Error: {0}", ex.Message));
			}
			finally
			{
				this.ResumeDrawing();
				ResumeLayout(false);
				PerformLayout();
			}
		}

		public IEnumerable<NewsSection> NewsSections => newsViewModel?.NewsSections;

#if DEBUG
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static bool ThrowExceptionInGetNews { get => NewsViewModel.ThrowExceptionInGetNews; set => NewsViewModel.ThrowExceptionInGetNews = value; }

		internal bool ThrowExceptionInNewsSections { get => newsViewModel.ThrowExceptionInNewsSections; set => newsViewModel.ThrowExceptionInNewsSections = value; }
#endif
		public static GlbReleaseNoteCombined[] GetNews(BusinessObjectFactory factory, string sectionID, bool hideReadItems) => NewsViewModel.GetNews(factory, sectionID, hideReadItems);

		[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
		readonly KFlowLayoutPanel[,] panelCache;

		[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
		readonly ZLabel[,] labelCache;

		KFlowLayoutPanel GetPanelIfExists(NewsSection section)
		{
			var columnRow = GetColumnRowForLayout(section.LayoutPanelID);
			return panelCache[columnRow.Item1, columnRow.Item2];
		}

		ZLabel GetLabelBySection(NewsSection section)
		{
			var columnRow = GetColumnRowForLayout(section.LayoutPanelID);
			return labelCache[columnRow.Item1, columnRow.Item2];
		}

		void AddOrRefreshPanel(NewsSection section, bool doubleHeight, IEnumerable<GlbReleaseNoteCombined> newsItems)
		{
			var sectionPanel = GetPanelIfExists(section);
			if (sectionPanel != null)
			{
				sectionPanel.SuspendLayout();
				sectionPanel.Controls.RemoveAndDisposeAll();
				AddRows(sectionPanel, newsItems, section);
				sectionPanel.ResumeLayout(false);
				sectionPanel.PerformLayout();

				var titleLabel = GetLabelBySection(section);
				titleLabel.SuspendLayout();
				titleLabel.Text = section.SectionName;
				titleLabel.ResumeLayout(false);
				titleLabel.PerformLayout();
			}
			else
			{
				var sectionOuterPanel = new KSplitContainer
				{
					Orientation = Orientation.Horizontal,
					Dock = DockStyle.Fill,
					BackColor = Color.LightGray,
					SplitterWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(1),
					IsSplitterFixed = true,
					FixedPanel = FixedPanel.Panel1,
					Panel1MinSize = ControlDpiScalingHelper.ScaleToCurrentDpiX(28),
				};
				sectionOuterPanel.SuspendLayout();
				sectionOuterPanel.SplitterDistance = ControlDpiScalingHelper.ScaleToCurrentDpiX(1);
				sectionOuterPanel.Panel1.BackColor = SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1;
				sectionOuterPanel.Panel1.Padding = sectionOuterPanel.Panel2.Padding = ControlDpiScalingHelper.NewScaledPadding(1);

				sectionPanel = new KFlowLayoutPanel
				{
					BorderStyle = BorderStyle.None,
					Name = "SectionFlowPanel",
					WrapContents = true,
					Dock = DockStyle.Fill,
					BackColor = Color.White,
					AutoScroll = true
				};
				sectionPanel.SuspendLayout();
				AddRows(sectionPanel, newsItems, section);

				var titleLabel = new ZLabel
				{
					Name = "TitleLabel",
					Text = section.SectionName,
					Font = new Font("Segoe UI", 13),
					Dock = DockStyle.Fill,
					Margin = ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 3),
					Padding = ControlDpiScalingHelper.NewScaledPadding(0),
					ForeColor = SystemDataRegistry.Instance.ColorTheme.NavBarTextColor,
					UseMnemonic = false,
				};

				SetupHover(sectionPanel);
				sectionPanel.MouseWheel += sectionPanel_MouseScroll;
				sectionOuterPanel.Panel1.Controls.Add(titleLabel);
				sectionOuterPanel.Panel2.Controls.Add(sectionPanel);

				var columnRow = GetColumnRowForLayout(section.LayoutPanelID);
				panelCache[columnRow.Item1, columnRow.Item2] = sectionPanel;
				labelCache[columnRow.Item1, columnRow.Item2] = titleLabel;
				StartupLayoutPanel.Controls.Add(sectionOuterPanel, columnRow.Item1, (doubleHeight ? columnRow.Item2 - 1 : columnRow.Item2));
				if (doubleHeight)
				{
					StartupLayoutPanel.SetRowSpan(sectionOuterPanel, 2);
				}

				sectionPanel.ResumeLayout(false);
				sectionOuterPanel.ResumeLayout(false);
			}
		}

		void AddRows(KFlowLayoutPanel sectionPanel, IEnumerable<GlbReleaseNoteCombined> newsItems, NewsSection section)
		{
			var backColor = SystemDataRegistry.Instance.ColorTheme.GridAlternatingRowColor;
			foreach (var item in newsItems)
			{
				AddRow(sectionPanel, item, backColor, section);
				backColor = backColor == Color.White ? SystemDataRegistry.Instance.ColorTheme.GridAlternatingRowColor : Color.White;
			}
		}

		void sectionPanel_MouseScroll(object sender, EventArgs e)
		{
			var control = (Control)sender;
			if (Form.ActiveForm == ParentForm && !control.ContainsFocus)
			{
				control.Focus();    // This makes the mouse wheel work for scrolling
			}
		}

		[SuppressMessage("Microsoft.Reliability", "CA2000:ZLinkLabel objects will dispose after form dispose")]
		void AddRow(FlowLayoutPanel sectionPanel, GlbReleaseNoteCombined item, Color backColor, NewsSection section)
		{
			var linkLabel = new ZLinkLabel
			{
				Name = "LinkLabel",
				Text = (!item.CategoryDisplayName.IsEmpty ? item.CategoryDisplayName + ": " : string.Empty) + item.Title,
				BackColor = backColor,
				Font = item.IsCurrentlyRead ? PanelFontNormal : PanelFontBold,
				Margin = ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 3),
				Padding = ControlDpiScalingHelper.NewScaledPadding(0),
				Dock = DockStyle.Fill,
				AutoSize = true,
				LinkBehavior = LinkBehavior.HoverUnderline,
				UseMnemonic = false,
			};

			linkLabel.Extensions.Remove<LabelCaptionRenderer>();
			sectionPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
			sectionPanel.Controls.Add(linkLabel);
			sectionPanel.SetFlowBreak(linkLabel, true);
			linkLabel.LinkClicked += (_, _) => ShowItem(item, linkLabel);
#if !WINZOR
			SetupHover(linkLabel);
#endif
			if (!item.IsCurrentlyRead && !UnReadItems.ContainsKey(item.PK.ToString()))
			{
				UnReadItems.Add(item.PK.ToString(), new string[] { section.SectionID, section.SectionName });
			}
			NoteWithLabels.Add(new Tuple<GlbReleaseNoteCombined, ZLinkLabel>(item, linkLabel));
		}

		Font PanelFontNormal
		{
			get { return panelFontNormal ??= new Font(OFont.NewsAnnouncementFontName, 8); }
		}

		Font panelFontNormal;

		Font PanelFontBold
		{
			get { return panelFontBold ??= new Font(OFont.NewsAnnouncementFontName, 8, FontStyle.Bold); }
		}

		Font panelFontBold;

		public Dictionary<string, string[]> GetUnReadItems()
		{
			return UnReadItems;
		}

		static void SetupHover(Control control)
		{
			control.MouseEnter += (sender, _) => ((Control)sender).GetParent<SplitContainer>().Panel1.BackColor = SystemDataRegistry.Instance.ColorTheme.NavBarGroupSelected1;
			control.MouseLeave += (sender, _) => ((Control)sender).GetParent<SplitContainer>().Panel1.BackColor = SystemDataRegistry.Instance.ColorTheme.NavBarButtonColor1;
		}

#if DEBUG
		internal
#endif
		void ShowItem(GlbReleaseNoteCombined item, ZLinkLabel linkLabel)
		{
			if (!item.IsDeleted)
			{
				ShowItemCore(item, linkLabel);

				MarkItemAsRead(item, linkLabel);
				NoteWithLabels.FindAll(t => t.Item1.PK == item.PK).ForEach(t => MarkItemAsRead(t.Item1, t.Item2));
			}
			else
			{
				Globals.Message.Show(Res.GetString("E318A9CD-4967-4462-86C9-FC86FD420116", "This news item has been deleted."));
			}
		}
		internal bool AllowAutoLogin { get; set; } = true;
		bool ShouldAutoLogin => AllowAutoLogin && GlbStaff.CurrentUser != null && !GlbStaff.CurrentUser.GS_IsSystemAccount;

#if DEBUG
		protected virtual
#endif
		async void ShowItemCore(GlbReleaseNoteCombined item, ZLinkLabel linkLabel)
		{
			try
			{
				using (new ZWaitCursorChanger())
				{
					if (ShouldAutoLogin && item.IsWiseTechGlobalItemViaTrustedMessaging)
					{
						if (!await ObjectFactory.Get<ISystemUserAccountCollectionTermChecker>().CheckTermAcknowledged())
						{
							return;
						}
					}
					WebUrlLauncher.Launch(ShouldAutoLogin ? item.GetDownloadURL() : item.GF_URL);
				}
			}
			catch (Win32Exception)
			{
				Globals.Message.Show(Res.GetString("98792D8A-18DD-488E-A7E4-3091D2921899", "The URL specified for this news item is invalid or not found."));
			}
			catch (FileNotFoundException)
			{
				Globals.Message.Show(Res.GetString("98792D8A-18DD-488E-A7E4-3091D2921899", "The URL specified for this news item is invalid or not found."));
			}
		}

		void MarkItemAsRead(GlbReleaseNoteCombined item, ZLinkLabel linkLabel)
		{
			if (GlbStaff.CurrentUser != null)
			{
				item.IsCurrentlyRead = true;

				ZExceptionReporting.ProcessWithSaveExceptionHandling(item.Factory.Save, null);
				UnReadItems.Remove(item.PK.ToString());
				NoteWithLabels.RemoveAll(t => t.Item1 == item && t.Item2 == linkLabel);
			}

			if (HideReadItems.TryGetValue(item.GF_Section, out var shouldHideAfterRead))
			{
				if (shouldHideAfterRead)
				{
					linkLabel.Dispose();
				}
				else if (linkLabel.Font.Bold)
				{
					linkLabel.Font = new Font(linkLabel.Font.FontFamily, linkLabel.Font.Size);
				}
			}
		}

		Tuple<int, int> GetColumnRowForLayout(string layoutId)
		{
			switch (layoutId)
			{
				case SystemDataRegistry.NewsSectionLayoutIDs.TopLeft:
					return Tuple.Create(0, 1);

				case SystemDataRegistry.NewsSectionLayoutIDs.TopMiddle:
					return Tuple.Create(1, 1);

				case SystemDataRegistry.NewsSectionLayoutIDs.TopRight:
					return Tuple.Create(2, 1);

				case SystemDataRegistry.NewsSectionLayoutIDs.BottomLeft:
					return Tuple.Create(0, 2);

				case SystemDataRegistry.NewsSectionLayoutIDs.BottomMiddle:
					return Tuple.Create(1, 2);

				case SystemDataRegistry.NewsSectionLayoutIDs.BottomRight:
					return Tuple.Create(2, 2);

				default:
					throw new InvalidOperationException("Unknown layoutId: " + layoutId);
			}
		}

		void AddLogos()
		{
			AddLogo(ControlDpiScalingHelper.NewScaledPadding(10, 0, 0, 0), Size.Empty, AnchorStyles.Left, BrandingFactory.Instance.ProductLogo, 0);
			AddLogo(ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 0), ControlDpiScalingHelper.NewScaledSize(130, 50), AnchorStyles.Right, new Bitmap(BrandingFactory.Instance.CompanyLogoButton), 2);
		}

		void AddLogo([DpiState(DpiState.ScaledVariant)] Padding margin, [DpiState(DpiState.ScaledVariant)] Size size, AnchorStyles anchorStyle, Image image, int column)
		{
			var logoPanel = new Panel
			{
				Dock = DockStyle.Fill,
				Anchor = anchorStyle,
				Margin = margin
			};

			if (!size.IsEmpty)
			{
				logoPanel.Size = size;
			}

			var pictureBox = new PictureBox
			{
				SizeMode = PictureBoxSizeMode.Zoom,
				Dock = DockStyle.Fill,
				Image = image
			};

			logoPanel.Controls.Add(pictureBox);
			StartupLayoutPanel.Controls.Add(logoPanel, column, 0);
		}

		#endregion

		readonly NewsViewModel newsViewModel;
	}
}
