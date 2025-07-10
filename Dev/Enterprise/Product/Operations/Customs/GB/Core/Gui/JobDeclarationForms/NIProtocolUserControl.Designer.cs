
namespace Enterprise.Customs.GB.GUI
{
	partial class NIProtocolUserControl
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
            this.NIProtocolGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.JE_NorthernIrelandModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.JE_IsGvmsPortCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.JE_ClaimEuSubsidyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.JE_NiGoodsAtRiskOfMovingToROICheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.NIProtocolGroupBox.SuspendLayout();
            this.JE_NorthernIrelandModeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.Declaration.JobDeclaration);
            // 
            // NIProtocolGroupBox
            // 
            this.NIProtocolGroupBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("NIProtocolUserControl|9771da77-52a6-4dbe-a7fe-c402dd97054c", "NI Protocol / Windsor Framework");
            this.NIProtocolGroupBox.Controls.Add(this.JE_NorthernIrelandModeDropEdit);
            this.NIProtocolGroupBox.Controls.Add(this.JE_IsGvmsPortCheckBox);
            this.NIProtocolGroupBox.Controls.Add(this.JE_ClaimEuSubsidyCheckBox);
            this.NIProtocolGroupBox.Controls.Add(this.JE_NiGoodsAtRiskOfMovingToROICheckBox);
            this.NIProtocolGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NIProtocolGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.NIProtocolGroupBox.Name = "NIProtocolGroupBox";
            this.NIProtocolGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 107, true);
            this.NIProtocolGroupBox.TabIndex = 0;
            this.NIProtocolGroupBox.TabStop = false;
            // 
            // JE_NorthernIrelandModeDropEdit
            // 
            this.JE_NorthernIrelandModeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.JE_NorthernIrelandModeDropEdit, "JE_NorthernIrelandMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_NorthernIrelandMode)));
            this.JE_NorthernIrelandModeDropEdit.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("NIProtocolUserControl|ae9037f9-9845-47a7-82f5-dbc8d421ee86", "NI Mode", "Indication of the style movement of goods between mainland Great Britain and Northern Ireland");
            this.JE_NorthernIrelandModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
            this.JE_NorthernIrelandModeDropEdit.Name = "JE_NorthernIrelandModeDropEdit";
            this.JE_NorthernIrelandModeDropEdit.ShouldResizeByMaxLength = true;
            this.JE_NorthernIrelandModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 17, true);
            this.JE_NorthernIrelandModeDropEdit.TabIndex = 0;
            // 
            // JE_IsGvmsPortCheckBox
            // 
            this.JE_IsGvmsPortCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.JE_IsGvmsPortCheckBox, "JE_IsGvmsPort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_IsGvmsPort)));
            this.JE_IsGvmsPortCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("NIProtocolUserControl|35fa6ce3-81b3-4533-ae9c-91a05263290e", "GVMS Port", "This port participates in the Goods Vehicle Movement System");
            this.JE_IsGvmsPortCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.JE_IsGvmsPortCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 38, true);
            this.JE_IsGvmsPortCheckBox.Name = "JE_IsGvmsPortCheckBox";
            this.JE_IsGvmsPortCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 16, true);
            this.JE_IsGvmsPortCheckBox.TabIndex = 2;
            this.JE_IsGvmsPortCheckBox.UseVisualStyleBackColor = true;
            // 
            // JE_ClaimEuSubsidyCheckBox
            // 
            this.JE_ClaimEuSubsidyCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.JE_ClaimEuSubsidyCheckBox, "JE_ClaimEuSubsidy");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_ClaimEuSubsidy)));
            this.JE_ClaimEuSubsidyCheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("NIProtocolUserControl|192ab1ba-b258-4472-9282-0eb675995d78", "Subsidy", "Claim EU Subsidy", "Request subsidy to cover the cost of EU customs charges for goods dispatched from GB to NI");
            this.JE_ClaimEuSubsidyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.JE_ClaimEuSubsidyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 58, true);
            this.JE_ClaimEuSubsidyCheckBox.Name = "JE_ClaimEuSubsidyCheckBox";
            this.JE_ClaimEuSubsidyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 16, true);
            this.JE_ClaimEuSubsidyCheckBox.TabIndex = 3;
            // 
            // JE_NiGoodsAtRiskOfMovingToROICheckBox
            // 
            this.BindingSource.SetBindingMember(this.JE_NiGoodsAtRiskOfMovingToROICheckBox, "JE_NiGoodsAtRiskOfMovingToROI");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.GB.Business.Declaration.JobDeclaration)(null)).JE_NiGoodsAtRiskOfMovingToROI)));
            this.JE_NiGoodsAtRiskOfMovingToROICheckBox.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("NIProtocolUserControl|6a945533-5806-4b12-84cf-6d9e9d784717", "ROI Risk", "Goods at Risk", "Goods are at risk of moving from Northern Ireland to the Republic of Ireland");
            this.JE_NiGoodsAtRiskOfMovingToROICheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.JE_NiGoodsAtRiskOfMovingToROICheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 78, true);
            this.JE_NiGoodsAtRiskOfMovingToROICheckBox.Name = "JE_NiGoodsAtRiskOfMovingToROICheckBox";
            this.JE_NiGoodsAtRiskOfMovingToROICheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 16, true);
            this.JE_NiGoodsAtRiskOfMovingToROICheckBox.TabIndex = 4;
            this.JE_NiGoodsAtRiskOfMovingToROICheckBox.UseVisualStyleBackColor = true;
            // 
            // NIProtocolUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.NIProtocolGroupBox);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 107, true);
            this.Name = "NIProtocolUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 107, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.NIProtocolGroupBox.ResumeLayout(false);
            this.NIProtocolGroupBox.PerformLayout();
            this.JE_NorthernIrelandModeDropEdit.ResumeLayout(true);
            this.JE_NorthernIrelandModeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox NIProtocolGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JE_NorthernIrelandModeDropEdit;
		private ZArchitecture.GUI.ZCheckBox JE_IsGvmsPortCheckBox;
		private ZArchitecture.GUI.ZCheckBox JE_ClaimEuSubsidyCheckBox;
		private ZArchitecture.GUI.ZCheckBox JE_NiGoodsAtRiskOfMovingToROICheckBox;
	}
}
