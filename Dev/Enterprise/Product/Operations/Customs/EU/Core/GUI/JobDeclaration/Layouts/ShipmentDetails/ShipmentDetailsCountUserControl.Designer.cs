namespace Enterprise.Customs.EU.GUI
{
	partial class ShipmentDetailsCountUserControl
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
            this.ContainerCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.TotalNoOfPiecesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // ContainerCountCalcEdit
            // 
            this.ContainerCountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.ContainerCountCalcEdit, "JE_ContainerCount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_ContainerCount)));
            this.ContainerCountCalcEdit.CaptionResourceString = null;
            this.ContainerCountCalcEdit.DecimalPlaces = 2;
            this.ContainerCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 0, true);
            this.ContainerCountCalcEdit.Name = "ContainerCountCalcEdit";
            this.ContainerCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
            this.ContainerCountCalcEdit.TabIndex = 2;
            this.ContainerCountCalcEdit.Text = "0";
            this.ContainerCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // TotalNoOfPiecesCalcEdit
            // 
            this.TotalNoOfPiecesCalcEdit.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.TotalNoOfPiecesCalcEdit, "JE_TotalNoOfPieces");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_TotalNoOfPieces)));
            this.TotalNoOfPiecesCalcEdit.CaptionResourceString = null;
            this.TotalNoOfPiecesCalcEdit.DecimalPlaces = 2;
            this.TotalNoOfPiecesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TotalNoOfPiecesCalcEdit.Name = "TotalNoOfPiecesCalcEdit";
            this.TotalNoOfPiecesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
            this.TotalNoOfPiecesCalcEdit.TabIndex = 1;
            this.TotalNoOfPiecesCalcEdit.Text = "0";
            this.TotalNoOfPiecesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // ShipmentDetailsCountUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.TotalNoOfPiecesCalcEdit);
            this.Controls.Add(this.ContainerCountCalcEdit);
            this.Name = "ShipmentDetailsCountUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit ContainerCountCalcEdit;
		internal ZArchitecture.ZCalcEdit TotalNoOfPiecesCalcEdit;
	}
}
