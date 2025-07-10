namespace Enterprise.Customs.GB.Module.OperationalActions.Ccsuk
{
	partial class RenominateUserControl
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
			this.NewAgentCodeFind = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			 	// 


			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions.RenominateApplicator);

			// zDropEdit1
			// 
			this.NewAgentCodeFind.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewAgentCodeFind, "NewAgent");
			this.NewAgentCodeFind.CaptionResourceString = Enterprise.Customs.GB.GUI.Ccsuk.RenominationUserControl.NewAgentCaption;
			this.NewAgentCodeFind.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 3, true);
			this.NewAgentCodeFind.Name = "NewAgentCodeFind";
			this.NewAgentCodeFind.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.NewAgentCodeFind.TabIndex = 0;
			// 
			// RenominationUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.NewAgentCodeFind);
			this.Name = "RenominationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 58, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		// change to ZDropEdit ?
		private ZArchitecture.GUI.ZDropEdit NewAgentCodeFind;

		#endregion
	}
}
