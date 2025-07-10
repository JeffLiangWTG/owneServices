namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class DDTNumberUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DDTNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoToUrlButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader);
			// 
			// DDTNumberTextBox
			// 
			this.DDTNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DDTNumberTextBox, "SRH_Reference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).SRH_Reference)));
			this.DDTNumberTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("0D9AFAFC-3F93-4816-8EF7-2613DFAE7AE5", "TSD Number");
			this.DDTNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DDTNumberTextBox.Name = "DDTNumberTextBox";
			this.DDTNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DDTNumberTextBox.TabIndex = 0;
			// 
			// GoToUrlButton
			// 
			this.GoToUrlButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.GoToUrlButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GoToUrlButton, false);
			this.GoToUrlButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 0, true);
			this.GoToUrlButton.Name = "GoToUrlButton";
			this.GoToUrlButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(23, 20, true);
			this.GoToUrlButton.TabIndex = 27;
			this.GoToUrlButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.GoToUrlButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.GoToUrlButton.ToolTipCaption = null;
			this.GoToUrlButton.UseVisualStyleBackColor = true;
			this.GoToUrlButton.Click += new System.EventHandler(this.GoToUrlButton_Click);
			// 
			// DDTNumberUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DDTNumberTextBox);
			this.Controls.Add(this.GoToUrlButton);
			this.Name = "DDTNumberUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox DDTNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZButton GoToUrlButton;
	}
}
