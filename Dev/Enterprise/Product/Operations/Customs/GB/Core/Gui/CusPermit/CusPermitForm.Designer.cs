using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	partial class CusPermitForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			queryTab = new ZTabPage();
			historicalQueryPanel = new ZGroupBox();
			queryDetailsPanel = new ZGroupBox();
			queryGrid = new ZGrid();
			zWebBrowser1 = new HtmlInterpretationBox();
			zWebBrowser2 = new HtmlInterpretationBox();
			horizontalSplitter2 = new CargoWise.Windows.UI.KSplitter();

			this.MainTabControl.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			historicalQueryPanel.SuspendLayout();
			queryDetailsPanel.SuspendLayout();
			zWebBrowser1.SuspendLayout();
			zWebBrowser2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();

			queryTab.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CusPermitForm|30C98E25-3508-46EC-B274-CCBF9C107C90", "License Enquiries");

			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 567, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 540, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 540, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 540, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 567, true);

			// 
			// CusPermitForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 623, true);
			this.DataSourceType = typeof(Enterprise.Customs.GB.Business.CusPermitHeader);
			this.Name = "CusPermitForm";

			//Queries Panel
			historicalQueryPanel.Dock = DockStyle.Top;
			historicalQueryPanel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CusPermitForm|6ADB2CFD-C025-49D9-80E6-E2133AAA894D", "Queries");
			historicalQueryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			historicalQueryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 300, true);

			//Query Details Panel
			queryDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			queryDetailsPanel.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("CusPermitForm|B5BC41EC-0CA0-427D-ADF8-EECFF6C4C2A0", "Query Details");
			queryDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 304, true);
			queryDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 319, true);

			// 
			// HorizontalSplitter2
			// 
			horizontalSplitter2.Dock = System.Windows.Forms.DockStyle.Top;
			horizontalSplitter2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 305, true);
			horizontalSplitter2.Name = "HorizontalSplitter";
			horizontalSplitter2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(966, 3, true);
			horizontalSplitter2.TabIndex = 6;
			horizontalSplitter2.TabStop = false;

			// 
			// zWebBrowser1
			//

			this.BindingSource.SetBindingMember(this.zWebBrowser1, "CusPermitHeaderQueries.EM_MessageInterpretation");
			zWebBrowser1.AllowWebBrowserDrop = false;
			zWebBrowser1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			zWebBrowser1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			zWebBrowser1.Name = "zWebBrowser1";
			zWebBrowser1.ScriptErrorsSuppressed = true;
			zWebBrowser1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 480, true);
			zWebBrowser1.Dock = DockStyle.Left;
			zWebBrowser1.TabStop = false;
			zWebBrowser1.Url = new System.Uri("about:blank", System.UriKind.Absolute);

			// 
			// zWebBrowser2
			//
			this.BindingSource.SetBindingMember(this.zWebBrowser2, "CusPermitHeaderQueries.LinkedMessageInterpretation");
			zWebBrowser2.AllowWebBrowserDrop = false;
			zWebBrowser2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 480, true);
			zWebBrowser2.Dock = DockStyle.Fill;
			zWebBrowser2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(290, 16, true);
			zWebBrowser2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			zWebBrowser2.Name = "zWebBrowser2";
			zWebBrowser2.ScriptErrorsSuppressed = true;
			zWebBrowser2.TabStop = false;
			zWebBrowser2.Url = new System.Uri("about:blank", System.UriKind.Absolute);

			//QueryGrid
			queryGrid.AllowNavigation = false;
			queryGrid.ReadOnly = true;
			queryGrid.IsWholeRowSelectedOnClick = true;
			this.BindingSource.SetBindingMember(queryGrid, "CusPermitHeaderQueries");
			queryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			var dateColumn = new ZDateEditColumnStyleInfo();
			dateColumn.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("176b916c-1234-42a2-a39d-00ae92c5a60b", "", "Created Date", "Created", "");
			dateColumn.ColumnName = "EM_SystemCreateTimeUtc";
			dateColumn.IsReadOnly = true;
			dateColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			var hasResponseColumn = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			hasResponseColumn.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("fa8a1f35-3ef0-43e2-b94b-9938297eb2d9", "Has response?");
			hasResponseColumn.ColumnName = "HasLinkedMessage";
			hasResponseColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			var badgeColumn = new ZTextBoxColumnStyleInfo();
			badgeColumn.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("D6A83BD5-82C9-458C-805A-F1E63BB81AE0", "", "Badge", "");
			badgeColumn.ColumnName = "EM_MessageOwner";
			badgeColumn.IsReadOnly = true;
			badgeColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			var msgNoColumn = new ZTextBoxColumnStyleInfo();
			msgNoColumn.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("5070FD92-674A-4A56-BCF4-D27B5D802C02", "Message Number", "");
			msgNoColumn.ColumnName = "EM_MessageNum";
			msgNoColumn.IsReadOnly = true;
			msgNoColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			var statusColumn = new ZTextBoxColumnStyleInfo();
			statusColumn.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("74102F86-DA26-408B-8413-4A5847F0122D", "Status", "");
			statusColumn.ColumnName = "EM_Status";
			statusColumn.IsReadOnly = true;
			statusColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			var lastUpdatedColumn = new ZDateEditColumnStyleInfo();
			lastUpdatedColumn.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("BFB9A8EB-8DFC-453D-9FD4-2498C5A7AC75", "Last Updated", "");
			lastUpdatedColumn.ColumnName = "EM_SystemLastEditTimeUtc";
			lastUpdatedColumn.IsReadOnly = true;
			lastUpdatedColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);


			queryGrid.ColumnStyles.Add(dateColumn);
			queryGrid.ColumnStyles.Add(hasResponseColumn);
			queryGrid.ColumnStyles.Add(badgeColumn);
			queryGrid.ColumnStyles.Add(msgNoColumn);
			queryGrid.ColumnStyles.Add(statusColumn);
			queryGrid.ColumnStyles.Add(lastUpdatedColumn);

			queryDetailsPanel.Controls.Add(zWebBrowser2);
			queryDetailsPanel.Controls.Add(zWebBrowser1);
			historicalQueryPanel.Controls.Add(queryGrid);


			queryTab.Controls.Add(queryDetailsPanel);
			queryTab.Controls.Add(horizontalSplitter2);
			queryTab.Controls.Add(historicalQueryPanel);

			zWebBrowser1.ResumeLayout();
			zWebBrowser1.PerformLayout();
			zWebBrowser2.ResumeLayout();
			zWebBrowser2.PerformLayout();

			historicalQueryPanel.ResumeLayout();
			historicalQueryPanel.PerformLayout();
			queryDetailsPanel.ResumeLayout();
			queryDetailsPanel.PerformLayout();
			MainTabControl.Controls.Add(queryTab);
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private HtmlInterpretationBox zWebBrowser1;
		private HtmlInterpretationBox zWebBrowser2;
		private ZGrid queryGrid;
		private ZTabPage queryTab;
		private ZGroupBox historicalQueryPanel;
		private ZGroupBox queryDetailsPanel;
		private KSplitter horizontalSplitter2;

		#endregion
	}
}
