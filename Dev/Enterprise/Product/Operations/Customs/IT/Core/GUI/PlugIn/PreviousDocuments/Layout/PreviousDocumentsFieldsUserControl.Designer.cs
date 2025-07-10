namespace Enterprise.Customs.IT.GUI
{
	partial class PreviousDocumentsFieldsUserControl
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
			this.ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument);
			// 
			// ItemNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ItemNumberCalcEdit, "CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_ItemNumber)));
			this.ItemNumberCalcEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("41839d70-297b-46ca-815c-ea8f6c028489", "Item No.", "Goods Item Identifier", "[12 01 007 000] Goods Item Identifier");
			this.ItemNumberCalcEdit.DecimalPlaces = 2;
			this.ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 17, true);
			this.ItemNumberCalcEdit.Name = "ItemNumberCalcEdit";
			this.ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 20, true);
			this.ItemNumberCalcEdit.TabIndex = 8;
			this.ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PreviousDocumentsFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ItemNumberCalcEdit);
			this.Name = "PreviousDocumentsFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(696, 139, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit ItemNumberCalcEdit;
	}
}
