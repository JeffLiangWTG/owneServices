namespace Enterprise.PAVE.MENT.GUI
{
	partial class MENTControl
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
		void InitializeComponent()
		{
			this.mentCollectionControl2 = new Enterprise.PAVE.MENT.GUI.MENTCollectionControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.PAVE.MENT.Business.MENTAcceptabilityBandViewModel);
			// 
			// mentCollectionControl2
			// 
			this.mentCollectionControl2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.mentCollectionControl2, ".");
			this.mentCollectionControl2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mentCollectionControl2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mentCollectionControl2.Name = "mentCollectionControl2";
			this.mentCollectionControl2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 730, true);
			this.mentCollectionControl2.TabIndex = 0;
			// 
			// MENTControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mentCollectionControl2);
			this.Name = "MENTControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(770, 730, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private MENTCollectionControl mentCollectionControl2;

		//private ZArchitecture.GUI.ZCheckBox MENTCheckBox;
	}
}
