namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class BillingSystemChooserForm
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.systemGrid = new Enterprise.ZArchitecture.ZGrid();
			this.button1 = new CargoWise.Windows.UI.KButton();
			this.selectAllButton = new CargoWise.Windows.UI.KButton();
			this.deselectAllButton = new CargoWise.Windows.UI.KButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.systemGrid)).BeginInit();
			this.systemGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 544, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.BillingSystemWrapperCollection);
			// 
			// systemGrid
			// 
			this.systemGrid.AllowNavigation = false;
			this.systemGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.systemGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.BillingSystemWrapper)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Billing.Business.BillingSystemWrapper)(null)).IsEnabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.BillingSystemWrapper)(null)).SystemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.BillingSystemWrapper)(null)).SystemDescription)));
			this.systemGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.Caption = "Enabled";
			zCheckBoxColumnStyleInfo2.ColumnName = "IsEnabled";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "SystemCode";
			zTextBoxColumnStyleInfo3.ColumnName = "SystemCode";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.Caption = "Description";
			zTextBoxColumnStyleInfo4.ColumnName = "SystemDescription";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.systemGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.systemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.systemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.systemGrid.GridId = "4bfc071c-29b8-4761-9fcb-a5d23d188c42";
			this.systemGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.systemGrid.LayoutKey = "systemGrid";
			this.systemGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.systemGrid.Name = "systemGrid";
			this.systemGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 497, true);
			this.systemGrid.TabIndex = 0;
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.IsCaptionOverridden = true;
			this.button1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 515, true);
			this.button1.Name = "button1";
			this.button1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.button1.TabIndex = 3;
			this.button1.Text = "OK";
			this.button1.ToolTipCaption = null;
			this.button1.UseVisualStyleBackColor = true;
			// 
			// selectAllButton
			// 
			this.selectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.selectAllButton.IsCaptionOverridden = true;
			this.selectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 515, true);
			this.selectAllButton.Name = "selectAllButton";
			this.selectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.selectAllButton.TabIndex = 1;
			this.selectAllButton.Text = "Select All";
			this.selectAllButton.ToolTipCaption = null;
			this.selectAllButton.UseVisualStyleBackColor = true;
			this.selectAllButton.Click += new System.EventHandler(this.selectAllButton_Click);
			// 
			// deselectAllButton
			// 
			this.deselectAllButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.deselectAllButton.IsCaptionOverridden = true;
			this.deselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 515, true);
			this.deselectAllButton.Name = "deselectAllButton";
			this.deselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.deselectAllButton.TabIndex = 2;
			this.deselectAllButton.Text = "Deselect All";
			this.deselectAllButton.ToolTipCaption = null;
			this.deselectAllButton.UseVisualStyleBackColor = true;
			this.deselectAllButton.Click += new System.EventHandler(this.deselectAllButton_Click);
			// 
			// BillingSystemChooserForm
			// 
			this.AcceptButton = this.button1;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.button1;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 568, true);
			this.Controls.Add(this.deselectAllButton);
			this.Controls.Add(this.selectAllButton);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.systemGrid);
			this.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.BillingSystemWrapperCollection);
			this.MinimizeBox = false;
			this.Name = "BillingSystemChooserForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Select Billing Systems";
			this.Controls.SetChildIndex(this.systemGrid, 0);
			this.Controls.SetChildIndex(this.button1, 0);
			this.Controls.SetChildIndex(this.selectAllButton, 0);
			this.Controls.SetChildIndex(this.deselectAllButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.systemGrid)).EndInit();
			this.systemGrid.ResumeLayout(false);
			this.systemGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid systemGrid;
		private CargoWise.Windows.UI.KButton button1;
		private CargoWise.Windows.UI.KButton selectAllButton;
		private CargoWise.Windows.UI.KButton deselectAllButton;
	}
}
