
namespace Enterprise.Customs.CA.GUI
{
	partial class HTSTariffBulkChangeStartForm
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
		protected override void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HTSTariffBulkChangeStartForm));
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffUpdateUserFileZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffChangeGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.OrLabel1 = new CargoWise.Windows.UI.KLabel();
			this.TariffUpdateCustomsFileZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffFinalUpdateZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.label1 = new CargoWise.Windows.UI.KLabel();
			this.AutomaticConvertZCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.InfoLabel = new CargoWise.Windows.UI.KLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffChangeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 555, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.TariffBulkChange);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(916, 526, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.CaptionResourceString = Res.GetData("32aae291-5376-4f30-bd80-82b4a6e6ca90", "&Close");
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// TariffUpdateUserFileZButton
			// 
			this.TariffUpdateUserFileZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 182, true);
			this.TariffUpdateUserFileZButton.Name = "TariffUpdateUserFileZButton";
			this.TariffUpdateUserFileZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TariffUpdateUserFileZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TariffUpdateUserFileZButton.TabIndex = 3;
			this.TariffUpdateUserFileZButton.CaptionResourceString = Res.GetData("357d5fb5-99e8-43cd-9b50-b3c1757fc66d", "Perform Tariff Bulk Change Using Your/Other Party Concordance File");
			this.TariffUpdateUserFileZButton.UseVisualStyleBackColor = true;
			this.TariffUpdateUserFileZButton.Click += new System.EventHandler(this.TariffBulkChangeFile);
			// 
			// TariffChangeGroupBox
			// 
			this.TariffChangeGroupBox.Controls.Add(this.OrLabel1);
			this.TariffChangeGroupBox.Controls.Add(this.TariffUpdateCustomsFileZButton);
			this.TariffChangeGroupBox.Controls.Add(this.TariffFinalUpdateZButton);
			this.TariffChangeGroupBox.Controls.Add(this.label1);
			this.TariffChangeGroupBox.Controls.Add(this.AutomaticConvertZCheckBox);
			this.TariffChangeGroupBox.Controls.Add(this.InfoLabel);
			this.TariffChangeGroupBox.Controls.Add(this.TariffUpdateUserFileZButton);
			this.TariffChangeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 26, true);
			this.TariffChangeGroupBox.Name = "TariffChangeGroupBox";
			this.TariffChangeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(965, 424, true);
			this.TariffChangeGroupBox.TabIndex = 40;
			this.TariffChangeGroupBox.TabStop = false;
			this.TariffChangeGroupBox.Text = "Tariff Change Details";
			// 
			// OrLabel1
			// 
			this.OrLabel1.AutoSize = true;
			this.OrLabel1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.OrLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(313, 194, true);
			this.OrLabel1.Name = "OrLabel1";
			this.OrLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(25, 15, true);
			this.OrLabel1.TabIndex = 47;
			this.OrLabel1.Text = "OR";
			// 
			// TariffUpdateCustomsFileZButton
			// 
			this.TariffUpdateCustomsFileZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 182, true);
			this.TariffUpdateCustomsFileZButton.Name = "TariffUpdateCustomsFileZButton";
			this.TariffUpdateCustomsFileZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TariffUpdateCustomsFileZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TariffUpdateCustomsFileZButton.TabIndex = 2;
			this.TariffUpdateCustomsFileZButton.CaptionResourceString = Res.GetData("0edff036-6514-43a4-8186-07521c1afeaa", "Perform HS2022 Tariff Bulk Change Using Concordance File Supplied by the CBSA\r\n");
			this.TariffUpdateCustomsFileZButton.UseVisualStyleBackColor = true;
			this.TariffUpdateCustomsFileZButton.Click += new System.EventHandler(this.TariffBulkChangeImbeded);
			// 
			// TariffFinalUpdateZButton
			// 
			this.TariffFinalUpdateZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(668, 182, true);
			this.TariffFinalUpdateZButton.Name = "TariffFinalUpdateZButton";
			this.TariffFinalUpdateZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TariffFinalUpdateZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TariffFinalUpdateZButton.TabIndex = 4;
			this.TariffFinalUpdateZButton.CaptionResourceString = Res.GetData("6b9fc899-4596-4603-a8f3-1a81c48d845e", "Apply All Pending Changes to Data Base");
			this.TariffFinalUpdateZButton.UseVisualStyleBackColor = true;
			this.TariffFinalUpdateZButton.Click += new System.EventHandler(this.ApplyTariffChanges);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 52, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 111, true);
			this.label1.TabIndex = 41;
			this.label1.Text = resources.GetString("label1.Text");
			// 
			// AutomaticConvertZCheckBox
			// 
			this.AutomaticConvertZCheckBox.AutoSize = true;
			this.AutomaticConvertZCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutomaticConvertZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 20, true);
			this.AutomaticConvertZCheckBox.Name = "AutomaticConvertZCheckBox";
			this.AutomaticConvertZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 16, true);
			this.AutomaticConvertZCheckBox.TabIndex = 1;
			this.AutomaticConvertZCheckBox.CaptionResourceString = Res.GetData("3308b75c-9385-4cf8-9cca-826e4754a616", "Automatically Convert One Tariff to One Other Tariff");
			this.AutomaticConvertZCheckBox.UseVisualStyleBackColor = true;
			// 
			// InfoLabel
			// 
			this.InfoLabel.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.InfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 254, true);
			this.InfoLabel.Name = "InfoLabel";
			this.InfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 71, true);
			this.InfoLabel.TabIndex = 40;
			this.InfoLabel.Text = resources.GetString("InfoLabel.Text");
			// 
			// HTSTariffBulkChangeStartForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 579, true);
			this.Controls.Add(this.TariffChangeGroupBox);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Business.TariffBulkChange);
			this.DataSourceTypeName = "Enterprise.Customs.Business.TariffBulkChange";
			this.Name = "HTSTariffBulkChangeStartForm";
			this.Text = "Tariff Bulk Change";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.TariffChangeGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffChangeGroupBox.ResumeLayout(false);
			this.TariffChangeGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton TariffUpdateUserFileZButton;
		private CargoWise.Windows.UI.KGroupBox TariffChangeGroupBox;
		private CargoWise.Windows.UI.KLabel InfoLabel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AutomaticConvertZCheckBox;
		private CargoWise.Windows.UI.KLabel label1;
		private Enterprise.ZArchitecture.GUI.ZButton TariffFinalUpdateZButton;
		private CargoWise.Windows.UI.KLabel OrLabel1;
		internal ZArchitecture.GUI.ZButton TariffUpdateCustomsFileZButton;
	}
}

