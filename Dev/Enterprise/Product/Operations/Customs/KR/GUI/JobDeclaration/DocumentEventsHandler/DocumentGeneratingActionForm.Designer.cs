
namespace Enterprise.Customs.KR.GUI
{
	partial class DocumentGeneratingActionForm
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
		new void InitializeComponent()
		{
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
      Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
      Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
      this.EntriesToDeliverGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.EntriesGrid = new Enterprise.ZArchitecture.ZGrid();
      this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
      this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
      this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
      this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
      this.zPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
      ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.EntriesToDeliverGroupBox.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).BeginInit();
      this.EntriesGrid.SuspendLayout();
      this.zPanel1.SuspendLayout();
      this.zPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // MainStatusBar
      // 
      this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 252, true);
      this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 24, true);
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.DocumentGeneratingActionCollection);
      // 
      // EntriesToDeliverGroupBox
      // 
      this.EntriesToDeliverGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("33293f7e-5503-41cd-9929-53283040b893", "Entries");
      this.EntriesToDeliverGroupBox.Controls.Add(this.EntriesGrid);
      this.EntriesToDeliverGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.EntriesToDeliverGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true);
      this.EntriesToDeliverGroupBox.Name = "EntriesToDeliverGroupBox";
      this.EntriesToDeliverGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 191, true);
      this.EntriesToDeliverGroupBox.TabIndex = 1;
      this.EntriesToDeliverGroupBox.TabStop = false;
      // 
      // EntriesGrid
      // 
      this.EntriesGrid.AllowNavigation = false;
      this.BindingSource.SetBindingMember(this.EntriesGrid, ".");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.DocumentGeneratingAction)(null)))));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.DocumentGeneratingAction)(null)).EntryNumber)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.DocumentGeneratingAction)(null)).ToBeDelivered)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.DocumentGeneratingAction)(null)).StatusDescription)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.DocumentGeneratingAction)(null)).PaymentInvoiceNumber)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.DocumentGeneratingAction)(null)).ReceivedDate)));
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.KR.Business.DocumentGeneratingAction)(null)).IncludingCurrentDifference)));
      this.EntriesGrid.CaptionVisible = false;
      zTextBoxColumnStyleInfo1.Caption = "";
      zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("99105aaa-1bb8-4cb4-9707-82743c05cc57", "Entry Number");
      zTextBoxColumnStyleInfo1.ColumnName = "EntryNumber";
      zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      zCheckBoxColumnStyleInfo1.AllowMultipleMacroses = false;
      zCheckBoxColumnStyleInfo1.Caption = "";
      zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("7c9cd239-180f-4afd-96f3-fd06d94f3f89", "To be delivered");
      zCheckBoxColumnStyleInfo1.ColumnName = "ToBeDelivered";
      zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
      zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
      zTextBoxColumnStyleInfo2.Caption = "";
      zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("bfde50df-6d72-4ead-ad35-5a4cc48f40f8", "Status");
      zTextBoxColumnStyleInfo2.ColumnName = "StatusDescription";
      zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      zTextBoxColumnStyleInfo3.ColumnName = "PaymentInvoiceNumber";
      zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
      zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
      zDateEditColumnStyleInfo1.ColumnName = "ReceivedDate";
      zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
      zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
      zCheckBoxColumnStyleInfo2.ColumnName = "IncludingCurrentDifference";
      zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
      zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
      this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
      this.EntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
      this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
      this.EntriesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
      this.EntriesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
      this.EntriesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
      this.EntriesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
      this.EntriesGrid.GridId = "116187ec-16b3-42fa-8722-4372f0f50914";
      this.EntriesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.EntriesGrid.LayoutKey = "EntriesGrid";
      this.EntriesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
      this.EntriesGrid.Name = "EntriesGrid";
      this.EntriesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 175, true);
      this.EntriesGrid.TabIndex = 0;
      // 
      // OKButton
      // 
      this.OKButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.OKButton.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("e8fb1998-e915-42a9-a2e9-ee565018c2f6", "OK");
      this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 7, true);
      this.OKButton.Name = "OKButton";
      this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 23, true);
      this.OKButton.TabIndex = 2;
      this.OKButton.ToolTipCaption = null;
      this.OKButton.UseVisualStyleBackColor = true;
      this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
      // 
      // CancelButton
      // 
      this.CancelButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
      this.CancelButton.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("5cabfd33-83bf-4ca3-bd50-66e2f7d0aef9", "Cancel");
      this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 7, true);
      this.CancelButton.Name = "CancelButton";
      this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
      this.CancelButton.TabIndex = 3;
      this.CancelButton.ToolTipCaption = null;
      this.CancelButton.UseVisualStyleBackColor = true;
      this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
      // 
      // zLabel1
      // 
      this.zLabel1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("5a6f206a-5163-41b8-a34b-f285473b822e", "Please select entries to be printed or delivered.");
      this.zLabel1.Dock = System.Windows.Forms.DockStyle.Top;
      this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
      this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.zLabel1.Name = "zLabel1";
      this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 22, true);
      this.zLabel1.TabIndex = 4;
      this.zLabel1.UseMnemonic = false;
      // 
      // zPanel1
      // 
      this.zPanel1.Controls.Add(this.EntriesToDeliverGroupBox);
      this.zPanel1.Controls.Add(this.zLabel1);
      this.zPanel1.Controls.Add(this.zPanel2);
      this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.zPanel1.Name = "zPanel1";
      this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 252, true);
      this.zPanel1.TabIndex = 0;
      // 
      // zPanel2
      // 
      this.zPanel2.Controls.Add(this.CancelButton);
      this.zPanel2.Controls.Add(this.OKButton);
      this.zPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 213, true);
      this.zPanel2.Name = "zPanel2";
      this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 39, true);
      this.zPanel2.TabIndex = 0;
      // 
      // DocumentGeneratingActionForm
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("d5c0e89d-9fba-4f08-b1d6-1bc96cbcec2f", "Select Entry");
      this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 276, true);
      this.Controls.Add(this.zPanel1);
      this.DataSourceType = typeof(Enterprise.Customs.KR.Business.DocumentGeneratingActionCollection);
      this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(587, 313, true);
      this.Name = "DocumentGeneratingActionForm";
      this.Controls.SetChildIndex(this.MainStatusBar, 0);
      this.Controls.SetChildIndex(this.zPanel1, 0);
      ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.EntriesToDeliverGroupBox.ResumeLayout(false);
      this.EntriesToDeliverGroupBox.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.EntriesGrid)).EndInit();
      this.EntriesGrid.ResumeLayout(false);
      this.EntriesGrid.PerformLayout();
      this.zPanel1.ResumeLayout(false);
      this.zPanel1.PerformLayout();
      this.zPanel2.ResumeLayout(false);
      this.zPanel2.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox EntriesToDeliverGroupBox;
		private ZArchitecture.ZGrid EntriesGrid;
		private ZArchitecture.GUI.ZButton OKButton;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZPanel zPanel2;
	}
}
