namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class DepartureDetailsUserControl
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
			this.DepartureGoodsLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TNNDocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TNNDocumentTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// DepartureGoodsLocationCodeFindBox
			// 
			this.DepartureGoodsLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartureGoodsLocationCodeFindBox, "MovementHeader.GoodsLocation.CGL_AdditionalIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).MovementHeader.GoodsLocation.CGL_AdditionalIdentifier)));
			this.DepartureGoodsLocationCodeFindBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("A796CD8C-21B3-46CD-9C15-C997EAE18279", "Departure Goods Location");
			this.DepartureGoodsLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DepartureGoodsLocationCodeFindBox.Name = "DepartureGoodsLocationCodeFindBox";
			this.DepartureGoodsLocationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DepartureGoodsLocationCodeFindBox.ParentType = null;
			this.DepartureGoodsLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			// 
			// TNNDocumentTypeDropEdit
			// 
			this.TNNDocumentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TNNDocumentTypeDropEdit, "TNNDocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).TNNDocumentType)));
			this.TNNDocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 302, true);
			this.TNNDocumentTypeDropEdit.Name = "TNNDocumentTypeDropEdit";
			this.TNNDocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.TNNDocumentTypeDropEdit.TabIndex = 0;
			// 
			// DepartureDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DepartureGoodsLocationCodeFindBox);
			this.Controls.Add(this.TNNDocumentTypeDropEdit);
			this.Name = "DepartureDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 116, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TNNDocumentTypeDropEdit.ResumeLayout(true);
			this.TNNDocumentTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox DepartureGoodsLocationCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit TNNDocumentTypeDropEdit;
	}
}
