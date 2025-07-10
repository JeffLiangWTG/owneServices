namespace Enterprise.Customs.DE.GUI
{
	partial class CusGoodsLocationUserControl
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
			this.LoadingPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalIdentifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LoadingPlaceTextBox.SuspendLayout();
			this.AdditionalIdentifierDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusGoodsLocation);
			// 
			// LoadingPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoadingPlaceTextBox, "LoadingPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusGoodsLocation)(null)).LoadingPlace)));
			this.LoadingPlaceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LoadingPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LoadingPlaceTextBox.Name = "LoadingPlaceTextBox";
			this.LoadingPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 16, true);
			this.LoadingPlaceTextBox.TabIndex = 0;
			// 
			// AdditionalIdentifierDropEdit
			// 
			this.AdditionalIdentifierDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalIdentifierDropEdit, "CGL_AdditionalIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CusGoodsLocation)(null)).CGL_AdditionalIdentifier)));
			this.AdditionalIdentifierDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalIdentifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 9, true);
			this.AdditionalIdentifierDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 7, 2, 2, true);
			this.AdditionalIdentifierDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 0, true);
			this.AdditionalIdentifierDropEdit.Name = "AdditionalIdentifierDropEdit";
			this.AdditionalIdentifierDropEdit.ShouldResizeByMaxLength = true;
			this.AdditionalIdentifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.AdditionalIdentifierDropEdit.TabIndex = 7;
			// 
			// CusGoodsLocationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LoadingPlaceTextBox);
			this.Controls.Add(this.AdditionalIdentifierDropEdit);
			this.Name = "CusGoodsLocationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 499, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LoadingPlaceTextBox.ResumeLayout(true);
			this.LoadingPlaceTextBox.PerformLayout();
			this.AdditionalIdentifierDropEdit.ResumeLayout(true);
			this.AdditionalIdentifierDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZTextBox LoadingPlaceTextBox;
		internal ZArchitecture.GUI.ZDropEdit AdditionalIdentifierDropEdit;
	}
}
