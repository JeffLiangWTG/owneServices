using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public partial class MessageUserControl : EU.GUI.MessageUserControl
	{
		public MessageUserControl()
		{
			InitializeComponent();
			AddMainProcedureColumn();
			gbChiefEDIMenuForEntries = new GbChiefEDIMenuForEntries();
			SetupEntryGridContextMenuAndColumns();
			SetupEntryLineAdditionalDataUserControl();
		}

		public new JobDeclaration JobDeclaration
		{
			get => (JobDeclaration)base.JobDeclaration;
			set => base.JobDeclaration = value;
		}

		public void SetMenusEntryOnPopup(object sender, EventArgs e)
		{
			var clickedEntry = (CusEntryHeader)(EntriesBoundGrid.GetFirstSelectedRow() ?? EntriesBoundGrid.List?.Cast<BusinessObject>().FirstOrDefault());
			if (clickedEntry != null)
			{
				gbChiefEDIMenuForEntries.Entry = clickedEntry;
				gbChiefEDIMenuForEntries.Visible = clickedEntry.Declaration is JobDeclaration declaration && !declaration.IsDeclarationIntegrated && declaration.ApplicationExtender is ChiefApplicationExtender;
			}
			else
			{
				gbChiefEDIMenuForEntries.Visible = false;
			}
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			var showLRNOnEntryGrid = JobDeclaration != null;
			EntriesBoundGrid.SetColumnVisible(showLRNOnEntryGrid, CusEntryHeader.Schema.LRN);
			EntriesBoundGrid.SetAvailability(showLRNOnEntryGrid, CusEntryHeader.Schema.LRN);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (EntriesBoundGrid.ContextMenu != null)
				{
					EntriesBoundGrid.ContextMenu.Popup -= SetMenusEntryOnPopup;
				}
				gbChiefEDIMenuForEntries.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void SetupEntryGridContextMenuAndColumns()
		{
			EntriesBoundGrid.ContextMenu.Popup += SetMenusEntryOnPopup;
			EntriesBoundGrid.ContextMenu.MenuItems.AddRange(new MenuItem[] { gbChiefEDIMenuForEntries });

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("DF4A123A-2D44-4364-ADD7-D1B73829BD7A", "Canceled"),
				ColumnName = CusEntryHeader.Schema.IsCancelledWithCustoms,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(69)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("E199AAE7-D913-4E06-B176-23989BBFE2A9", "LRN"),
				ColumnName = CusEntryHeader.Schema.LRN,
				IsVisible = false,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("FD098815-564A-48C4-84C3-9246B36A1C1C", "ICS"),
				ColumnName = CusEntryHeader.Schema.ImportClearanceStatusICS,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("55336FFD-8E26-4A3F-BC69-2337FCCD18CA", "ROE"),
				ColumnName = CusEntryHeader.Schema.RouteOfEntry,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("15CD49D3-6A88-413A-A1C1-DDC545C1D389", "IRC"),
				ColumnName = CusEntryHeader.Schema.IrcInventoryReturnCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("56765DF0-975E-4594-B3E2-C92FD0C56F08", "SOE"),
				ColumnName = CusEntryHeader.Schema.StyleOfEntrySOE,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("71A6EE64-C3FD-49BF-9272-EDF1EA986358", "Master UCR"),
				ColumnName = CusEntryHeader.Schema.CH_MasterUCR,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});
		}

		void AddMainProcedureColumn()
		{
			EntryLineGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("CBA4A69E-A74F-4AF2-8A90-5391A51E5494", "Main Procedure"),
				ColumnName = "ProcedureCodeWithoutConcession",
				IsVisible = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});
		}

		internal readonly GbChiefEDIMenuForEntries gbChiefEDIMenuForEntries;

		protected override EU.GUI.EntryLineAdditionalDataUserControl GetEntryLineAdditionalData() => new EntryLineAdditionalDataUserControl();

		void SetupEntryLineAdditionalDataUserControl()
		{
			EntryLineAdditionalDataUserControl.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
		}

		protected override void RefreshControlVisibiltyOnShown()
		{
			base.RefreshControlVisibiltyOnShown();
			ShowDeclarationMessagesTabPage();
		}

		#region MCP UNC Messages Tab Page
		void InitializeDeclarationMessagesTabPage()
		{
			declarationMessagesTabPage = new BaseDeclarationTabPage();
			declarationMessagesTabPage.LazyCreateControls += (s, e) => { LoadDeclarationMessagesTabPage(); };
			declarationMessagesTabPage.CaptionResourceString = Res.GetData("367C226F-E929-4B49-B055-D47B326E3673", "Declaration Messages");
			declarationMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, isInStandardDpi: true);
			declarationMessagesTabPage.Name = "DeclarationMessagesTabPage";
			declarationMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 454, isInStandardDpi: true);
			declarationMessagesTabPage.TabIndex = 4;
			EntryLinesMessagesTabControl.SuspendLayout();
			EntryLinesMessagesTabControl.TabPages.Insert(declarationMessagesTabPage, EntryLinesMessagesTabControl.TabPages.Count);
			EntryLinesMessagesTabControl.ResumeLayout(false);
		}

		void LoadDeclarationMessagesTabPage()
		{
			if (declarationMessagesTabPage.Controls.Count == 0 && declarationMessagesTabPage.TabVisible)
			{
				declarationMessagesUserControl = new DiscardedMessageUserControl();
				declarationMessagesUserControl.Dock = DockStyle.Fill;
				declarationMessagesUserControl.Name = "DeclarationMessagesUserControl";
				declarationMessagesUserControl.Visible = false;
				declarationMessagesTabPage.Controls.Add(declarationMessagesUserControl);
				declarationMessagesUserControl.Visible = true;
				declarationMessagesUserControl.SetDataBinding(JobDeclaration, "");
			}
		}

		void ShowDeclarationMessagesTabPage()
		{
			var showDeclarationMessages = JobDeclaration != null && JobDeclaration.IsImport;
			if (showDeclarationMessages && declarationMessagesTabPage == null)
			{
				InitializeDeclarationMessagesTabPage();
			}
			else if (declarationMessagesTabPage != null && !showDeclarationMessages)
			{
				declarationMessagesTabPage.TabRelevant = false;
				declarationMessagesTabPage = null;
			}
		}

		public BaseDeclarationTabPage DeclarationMessagesTabPage => declarationMessagesTabPage;
		BaseDeclarationTabPage declarationMessagesTabPage;
		DiscardedMessageUserControl declarationMessagesUserControl;
		#endregion
	}
}
