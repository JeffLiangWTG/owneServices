namespace Enterprise.Customs.KR.GUI
{
	partial class EntryInstructionLayoutUserControl
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
      this.TotalPackagesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
      this.TotalPackagesUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.AgreedRateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.UseTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
      this.UseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.TotalPackagesUQDropEdit.SuspendLayout();
      this.AgreedRateDropEdit.SuspendLayout();
      this.UseTypeDropEdit.SuspendLayout();
      this.UseDateEdit.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
      // 
      // TotalPackagesCalcEdit
      // 
      this.BindingSource.SetBindingMember(this.TotalPackagesCalcEdit, "CustomsEntryInstructions.CEI_PackQty");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_PackQty)));
      this.TotalPackagesCalcEdit.DecimalPlaces = 0;
      this.TotalPackagesCalcEdit.Decimals = 0;
      this.TotalPackagesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
      this.TotalPackagesCalcEdit.Name = "TotalPackagesCalcEdit";
      this.TotalPackagesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 25, true);
      this.TotalPackagesCalcEdit.TabIndex = 0;
      this.TotalPackagesCalcEdit.Text = "0";
      this.TotalPackagesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
      this.TotalPackagesCalcEdit.TrackDisposedAccess = true;
      // 
      // TotalPackagesUQDropEdit
      // 
      this.TotalPackagesUQDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.TotalPackagesUQDropEdit, "JE_TotalNoOfPacksPackType");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_TotalNoOfPacksPackType)));
      this.TotalPackagesUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 3, true);
      this.TotalPackagesUQDropEdit.Name = "TotalPackagesUQDropEdit";
      this.TotalPackagesUQDropEdit.PreBoundMaxLength = 3;
      this.TotalPackagesUQDropEdit.ReadOnly = true;
      this.TotalPackagesUQDropEdit.ShowDescriptionBox = false;
      this.TotalPackagesUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 25, true);
      this.TotalPackagesUQDropEdit.TabIndex = 1;
      // 
      // AgreedRateDropEdit
      // 
      this.AgreedRateDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.AgreedRateDropEdit, "CustomsEntryInstructions.CEI_AgreedRateApp");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_AgreedRateApp)));
      this.AgreedRateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 30, true);
      this.AgreedRateDropEdit.Name = "AgreedRateDropEdit";
      this.AgreedRateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 25, true);
      this.AgreedRateDropEdit.TabIndex = 2;
      // 
      // UseTypeDropEdit
      // 
      this.UseTypeDropEdit.AllowDrop = true;
      this.BindingSource.SetBindingMember(this.UseTypeDropEdit, "CustomsEntryInstructions.CEI_BondedFactoryUseCode");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_BondedFactoryUseCode)));
      this.UseTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 59, true);
      this.UseTypeDropEdit.Name = "UseTypeDropEdit";
      this.UseTypeDropEdit.PreBoundMaxLength = 1;
      this.UseTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 25, true);
      this.UseTypeDropEdit.TabIndex = 3;
      // 
      // UseDateEdit
      // 
      this.UseDateEdit.AllowDrop = true;
      this.UseDateEdit.AutoCompleteMonthThreshold = 1;
      this.BindingSource.SetBindingMember(this.UseDateEdit, "CustomsEntryInstructions.CEI_BondedFactoryArrivalDate");
      // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
      CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_BondedFactoryArrivalDate)));
      this.UseDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
      this.UseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 85, true);
      this.UseDateEdit.Name = "UseDateEdit";
      this.UseDateEdit.TabIndex = 4;
      // 
      // EntryInstructionLayoutUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.Controls.Add(this.UseTypeDropEdit);
      this.Controls.Add(this.UseDateEdit);
      this.Controls.Add(this.AgreedRateDropEdit);
      this.Controls.Add(this.TotalPackagesUQDropEdit);
      this.Controls.Add(this.TotalPackagesCalcEdit);
      this.Name = "EntryInstructionLayoutUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(408, 172, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.TotalPackagesUQDropEdit.ResumeLayout(true);
      this.TotalPackagesUQDropEdit.PerformLayout();
      this.AgreedRateDropEdit.ResumeLayout(true);
      this.AgreedRateDropEdit.PerformLayout();
      this.UseTypeDropEdit.ResumeLayout(true);
      this.UseTypeDropEdit.PerformLayout();
      this.UseDateEdit.ResumeLayout(true);
      this.UseDateEdit.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZCalcEdit TotalPackagesCalcEdit;
		public ZArchitecture.GUI.ZDropEdit TotalPackagesUQDropEdit;
		public ZArchitecture.GUI.ZDropEdit AgreedRateDropEdit;
		public ZArchitecture.GUI.ZDropEdit UseTypeDropEdit;
		public ZArchitecture.GUI.ZDateEdit UseDateEdit;
	}
}
