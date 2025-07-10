namespace Enterprise.BufferManagement.GUI
{
	partial class DateAcceptabilityControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DateAcceptabilityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LegendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DateAcceptabilityPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DateAcceptabilityPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.ProcessHeader);
			// 
			// DateAcceptabilityDropEdit
			// 
			this.DateAcceptabilityDropEdit.AllowDrop = true;
			this.DateAcceptabilityDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DateAcceptabilityDropEdit, "FH_DateAcceptability");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.ProcessHeader)(null)).FH_DateAcceptability)));
			this.DateAcceptabilityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 0, true);
			this.DateAcceptabilityDropEdit.Name = "DateAcceptabilityDropEdit";
			this.DateAcceptabilityDropEdit.PreBoundMaxLength = 3;
			this.DateAcceptabilityDropEdit.ShowDescriptionBox = false;
			this.DateAcceptabilityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.DateAcceptabilityDropEdit.TabIndex = 0;
			// 
			// LegendButton
			// 
			this.LegendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LegendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 0, true);
			this.LegendButton.Name = "LegendButton";
			this.LegendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 20, true);
			this.LegendButton.TabIndex = 1;
			this.LegendButton.Text = "?";
			this.LegendButton.UseVisualStyleBackColor = true;
			this.LegendButton.Click += new System.EventHandler(this.LegendButton_Click);
			// 
			// DateAcceptabilityPictureBox
			// 
			this.DateAcceptabilityPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DateAcceptabilityPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 0, true);
			this.DateAcceptabilityPictureBox.Name = "DateAcceptabilityPictureBox";
			this.DateAcceptabilityPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.DateAcceptabilityPictureBox.TabIndex = 2;
			this.DateAcceptabilityPictureBox.TabStop = false;
			// 
			// DateAcceptabilityControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DateAcceptabilityPictureBox);
			this.Controls.Add(this.LegendButton);
			this.Controls.Add(this.DateAcceptabilityDropEdit);
			this.Name = "DateAcceptabilityControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DateAcceptabilityPictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit DateAcceptabilityDropEdit;
		public ZArchitecture.GUI.ZButton LegendButton;
		public Enterprise.ZArchitecture.GUI.ZPictureBox DateAcceptabilityPictureBox;
	}
}
