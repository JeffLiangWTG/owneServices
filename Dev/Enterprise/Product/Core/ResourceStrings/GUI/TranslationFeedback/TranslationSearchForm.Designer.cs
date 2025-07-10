namespace Enterprise.ResourceStrings.GUI
{
	partial class TranslationSearchForm
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
			this.translationSearchControl = new Enterprise.ResourceStrings.GUI.TranslationSearchControl();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.searchButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 168, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ResourceStrings.Business.TranslationSearchCriteria);
			// 
			// translationSearchControl
			// 
			this.translationSearchControl.AllowDrop = true;
			this.translationSearchControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.translationSearchControl, ".");
			this.translationSearchControl.CaptionResourceString = null;
			this.translationSearchControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 12, true);
			this.translationSearchControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 120, true);
			this.translationSearchControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 120, true);
			this.translationSearchControl.Name = "translationSearchControl";
			this.translationSearchControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 120, true);
			this.translationSearchControl.TabIndex = 1;
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("0fb4388d-e490-4774-96c0-da2ebd6910f5", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 138, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// searchButton
			// 
			this.searchButton.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("9f4fbea4-53c5-45a5-92db-4564e18dac0c", "Search");
			this.searchButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 138, true);
			this.searchButton.Name = "searchButton";
			this.searchButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.searchButton.TabIndex = 2;
			this.searchButton.UseVisualStyleBackColor = true;
			this.searchButton.Click += new System.EventHandler(this.searchButton_Click);
			// 
			// TranslationSearchForm
			// 
			this.AcceptButton = this.searchButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ResourceStrings.GUI.Res.GetData("5239a040-73c4-402d-889a-8c0a4acf8c03", "Translation Search");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(482, 192, true);
			this.Controls.Add(this.translationSearchControl);
			this.Controls.Add(this.searchButton);
			this.Controls.Add(this.cancelButton);
			this.DataSourceType = typeof(Enterprise.ResourceStrings.Business.TranslationSearchCriteria);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 220, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 220, true);
			this.Name = "TranslationSearchForm";
			this.Text = "TranslationSearchForm";
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.searchButton, 0);
			this.Controls.SetChildIndex(this.translationSearchControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal TranslationSearchControl translationSearchControl;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton searchButton;
	}
}
