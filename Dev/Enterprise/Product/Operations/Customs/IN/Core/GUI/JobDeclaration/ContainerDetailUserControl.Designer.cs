namespace Enterprise.Customs.IN.GUI;

partial class ContainerDetailUserControl
{
	#region Component Designer generated code

	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.SealTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.SealTypeDropEdit.SuspendLayout();
		this.SuspendLayout();
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Business.CusContainer);
		// 
		// SealPartyDropEdit
		// 
		this.SealTypeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.SealTypeDropEdit, "CO_SealType");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IN.Business.CusContainer)(null)).CO_SealType)));
		this.SealTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 46, true);
		this.SealTypeDropEdit.Name = "SealTypeDropEdit";
		this.SealTypeDropEdit.ShouldResizeByMaxLength = true;
		this.SealTypeDropEdit.ShowDescriptionBox = false;
		this.SealTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
		this.SealTypeDropEdit.TabIndex = 4;
		// 
		// ContainerDetailUserControl
		//
		this.Controls.Add(this.SealTypeDropEdit);
		this.Name = "ContainerDetailUserControl";
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.SealTypeDropEdit.ResumeLayout(false);
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	#endregion

	ZArchitecture.GUI.ZDropEdit SealTypeDropEdit;

}
