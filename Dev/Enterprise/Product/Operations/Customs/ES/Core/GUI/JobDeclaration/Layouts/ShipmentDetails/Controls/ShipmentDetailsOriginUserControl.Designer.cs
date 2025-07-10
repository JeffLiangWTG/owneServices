namespace Enterprise.Customs.ES.GUI
{
	partial class ShipmentDetailsOriginUserControl
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
			this.EstimatedDepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.GoodsOriginDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EstimatedDepartureDateEdit.SuspendLayout();
			this.OriginFindBox.SuspendLayout();
			this.GoodsOriginDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// EstimatedDepartureDateEdit
			// 
			this.EstimatedDepartureDateEdit.AllowDrop = true;
			this.EstimatedDepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EstimatedDepartureDateEdit, "JE_DateAtOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).JE_DateAtOrigin)));
			this.EstimatedDepartureDateEdit.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("B6FF13D0-2424-49C8-849A-58D983A0FBD6", "ETD");
			this.EstimatedDepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 0, true);
			this.EstimatedDepartureDateEdit.Name = "EstimatedDepartureDateEdit";
			this.EstimatedDepartureDateEdit.TabIndex = 2;
			// 
			// OriginFindBox
			// 
			this.OriginFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginFindBox, "JE_RL_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).JE_RL_NKOrigin)));
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OriginFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.OriginFindBox.Name = "OriginFindBox";
			this.OriginFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OriginFindBox.ParentType = null;
			this.OriginFindBox.PreBoundMaxLength = 5;
			this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.OriginFindBox.TabIndex = 0;
			// 
			// GoodsOriginDropEdit
			// 
			this.GoodsOriginDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsOriginDropEdit, "JE_GoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).JE_GoodsOrigin)));
			this.GoodsOriginDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 0, true);
			this.GoodsOriginDropEdit.Name = "GoodsOriginDropEdit";
			this.GoodsOriginDropEdit.PreBoundMaxLength = 2;
			this.GoodsOriginDropEdit.ShowDescriptionBox = false;
			this.GoodsOriginDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.GoodsOriginDropEdit.TabIndex = 1;
			// 
			// ShipmentDetailsOriginUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EstimatedDepartureDateEdit);
			this.Controls.Add(this.GoodsOriginDropEdit);
			this.Controls.Add(this.OriginFindBox);
			this.Name = "ShipmentDetailsOriginUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EstimatedDepartureDateEdit.ResumeLayout(true);
			this.EstimatedDepartureDateEdit.PerformLayout();
			this.OriginFindBox.ResumeLayout(true);
			this.OriginFindBox.PerformLayout();
			this.GoodsOriginDropEdit.ResumeLayout(true);
			this.GoodsOriginDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDateEdit EstimatedDepartureDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit GoodsOriginDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox OriginFindBox;
	}
}
