namespace Enterprise.Customs.AU.GUI
{
	partial class ConsolidatedDeclarationControlBagTemplate
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
			this.EntryStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.VoyageFlightNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsolidatedDeclarationDetailsUserControl = new Enterprise.Customs.AU.GUI.ConsolidatedDeclarationDetailsUserControl();
			this.PaymentStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryStyleDropEdit.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.ConsolidatedDeclarationDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclaration);
			// 
			// PaymentStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.PaymentStatusTextBox, "PaymentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclaration)(null)).PaymentStatus)));
			this.PaymentStatusTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("2889B954-1305-41C8-BAD3-00D83D876DFD", "Customs Pay");
			this.PaymentStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 150, true);
			this.PaymentStatusTextBox.Name = "PaymentStatusTextBox";
			this.PaymentStatusTextBox.ReadOnly = true;
			this.PaymentStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 15, true);
			this.PaymentStatusTextBox.TabIndex = 0;
			// 
			// EntryStyleDropEdit
			// 
			this.EntryStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryStyleDropEdit, "EntryStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclaration)(null)).EntryStyle)));
			this.EntryStyleDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("CE037B0A-28BA-4620-9165-D836539B6A11", "Entry Style");
			this.EntryStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 92, true);
			this.EntryStyleDropEdit.Name = "EntryStyleDropEdit";
			this.EntryStyleDropEdit.PreBoundMaxLength = 3;
			this.EntryStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.EntryStyleDropEdit.TabIndex = 1;
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclaration)(null)).VesselName)));
			this.VesselCodeFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("387DCD64-8A13-4D5C-A967-0735E25EFD06", "Vessel");
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 65, true);
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.VesselCodeFindBox.ParentType = null;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
			this.VesselCodeFindBox.TabIndex = 2;
			// 
			// VoyageFlightNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageFlightNoTextBox, "VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.ConsolidatedDeclaration)(null)).VoyageFlightNo)));
			this.VoyageFlightNoTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("A7618F67-822D-432D-894F-BE50D52203F2", "Arrival Flight");
			this.VoyageFlightNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 10, true);
			this.VoyageFlightNoTextBox.Name = "VoyageFlightNoTextBox";
			this.VoyageFlightNoTextBox.ReadOnly = true;
			this.VoyageFlightNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 15, true);
			this.VoyageFlightNoTextBox.TabIndex = 3;
			// 
			// ConsolidatedDeclarationDetailsUserControl
			// 
			this.ConsolidatedDeclarationDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolidatedDeclarationDetailsUserControl, ".");
			this.ConsolidatedDeclarationDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 37, true);
			this.ConsolidatedDeclarationDetailsUserControl.Name = "ConsolidatedDeclarationDetailsUserControl";
			this.ConsolidatedDeclarationDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 20, true);
			this.ConsolidatedDeclarationDetailsUserControl.TabIndex = 4;
			// 
			// ConsolidatedDeclarationControlBagTemplate
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PaymentStatusTextBox);
			this.Controls.Add(this.EntryStyleDropEdit);
			this.Controls.Add(this.VesselCodeFindBox);
			this.Controls.Add(this.VoyageFlightNoTextBox);
			this.Controls.Add(this.ConsolidatedDeclarationDetailsUserControl);
			this.Name = "ConsolidatedDeclarationControlBagTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 130, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryStyleDropEdit.ResumeLayout(true);
			this.EntryStyleDropEdit.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.ConsolidatedDeclarationDetailsUserControl.ResumeLayout(true);
			this.ConsolidatedDeclarationDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZTextBox PaymentStatusTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit EntryStyleDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		internal Enterprise.ZArchitecture.ZTextBox VoyageFlightNoTextBox;
		internal ConsolidatedDeclarationDetailsUserControl ConsolidatedDeclarationDetailsUserControl;

		#endregion
	}
}
