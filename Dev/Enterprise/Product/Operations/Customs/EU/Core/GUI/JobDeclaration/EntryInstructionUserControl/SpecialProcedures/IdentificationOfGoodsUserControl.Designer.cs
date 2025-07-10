namespace Enterprise.Customs.EU.GUI
{
	partial class IdentificationOfGoodsUserControl
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
            this.IdentificationofGoodsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.IdentificationofGoodsSubGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.IdentificationOfGoodsCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.IdentificationOfGoodsDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ProcessedProductsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.GoodsDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.CommodityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RateOfYieldTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.IdentificationofGoodsGroupBox.SuspendLayout();
            this.IdentificationofGoodsSubGroupBox.SuspendLayout();
            this.IdentificationOfGoodsCodeDropEdit.SuspendLayout();
            this.ProcessedProductsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // IdentificationofGoodsGroupBox
            // 
            this.IdentificationofGoodsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4D772B0E-65CB-43DA-A906-8BCA3855137E", "Identification Of Goods");
            this.IdentificationofGoodsGroupBox.Controls.Add(this.IdentificationofGoodsSubGroupBox);
            this.IdentificationofGoodsGroupBox.Controls.Add(this.ProcessedProductsGroupBox);
            this.IdentificationofGoodsGroupBox.Controls.Add(this.RateOfYieldTextBox);
            this.IdentificationofGoodsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.IdentificationofGoodsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.IdentificationofGoodsGroupBox.Name = "IdentificationofGoodsGroupBox";
            this.IdentificationofGoodsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 235, true);
            this.IdentificationofGoodsGroupBox.TabIndex = 1;
            this.IdentificationofGoodsGroupBox.TabStop = false;
            // 
            // IdentificationofGoodsSubGroupBox
            // 
            this.IdentificationofGoodsSubGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IdentificationofGoodsSubGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("7FE9D022-184D-42CD-A713-4B6017B7E9EA", "Identification Of Goods");
            this.IdentificationofGoodsSubGroupBox.Controls.Add(this.IdentificationOfGoodsCodeDropEdit);
            this.IdentificationofGoodsSubGroupBox.Controls.Add(this.IdentificationOfGoodsDetailsTextBox);
            this.IdentificationofGoodsSubGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 146, true);
            this.IdentificationofGoodsSubGroupBox.Name = "IdentificationofGoodsSubGroupBox";
            this.IdentificationofGoodsSubGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(481, 83, true);
            this.IdentificationofGoodsSubGroupBox.TabIndex = 3;
            this.IdentificationofGoodsSubGroupBox.TabStop = false;
            // 
            // IdentificationOfGoodsCodeDropEdit
            // 
            this.IdentificationOfGoodsCodeDropEdit.AllowDrop = true;
            this.IdentificationOfGoodsCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.IdentificationOfGoodsCodeDropEdit, "CustomsEntryInstructions.ZG_IdOfGoodCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_IdOfGoodCode)));
            this.IdentificationOfGoodsCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 25, true);
            this.IdentificationOfGoodsCodeDropEdit.Name = "IdentificationOfGoodsCodeDropEdit";
            this.IdentificationOfGoodsCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 15, true);
            this.IdentificationOfGoodsCodeDropEdit.TabIndex = 2;
            // 
            // IdentificationOfGoodsDetailsTextBox
            // 
            this.IdentificationOfGoodsDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.IdentificationOfGoodsDetailsTextBox, "CustomsEntryInstructions.IdentificationofGoodsDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).IdentificationofGoodsDetails)));
            this.IdentificationOfGoodsDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 51, true);
            this.IdentificationOfGoodsDetailsTextBox.Name = "IdentificationOfGoodsDetailsTextBox";
            this.IdentificationOfGoodsDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 15, true);
            this.IdentificationOfGoodsDetailsTextBox.TabIndex = 3;
            // 
            // ProcessedProductsGroupBox
            // 
            this.ProcessedProductsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProcessedProductsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("569861DD-5564-4F64-A282-4A99618839C5", "Processed Products");
            this.ProcessedProductsGroupBox.Controls.Add(this.GoodsDescriptionTextBox);
            this.ProcessedProductsGroupBox.Controls.Add(this.CommodityCodeTextBox);
            this.ProcessedProductsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 51, true);
            this.ProcessedProductsGroupBox.Name = "ProcessedProductsGroupBox";
            this.ProcessedProductsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(478, 83, true);
            this.ProcessedProductsGroupBox.TabIndex = 2;
            this.ProcessedProductsGroupBox.TabStop = false;
            // 
            // GoodsDescriptionTextBox
            // 
            this.GoodsDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.GoodsDescriptionTextBox, "CustomsEntryInstructions.ProcessedProductDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ProcessedProductDescription)));
            this.GoodsDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 51, true);
            this.GoodsDescriptionTextBox.Name = "GoodsDescriptionTextBox";
            this.GoodsDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 15, true);
            this.GoodsDescriptionTextBox.TabIndex = 3;
            // 
            // CommodityCodeTextBox
            // 
            this.CommodityCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.CommodityCodeTextBox, "CustomsEntryInstructions.ZG_ProcessedProductsCommodityCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_ProcessedProductsCommodityCode)));
            this.CommodityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 25, true);
            this.CommodityCodeTextBox.Name = "CommodityCodeTextBox";
            this.CommodityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 15, true);
            this.CommodityCodeTextBox.TabIndex = 2;
            // 
            // RateOfYieldTextBox
            // 
            this.RateOfYieldTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.RateOfYieldTextBox, "CustomsEntryInstructions.ZG_RateOfYield");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_RateOfYield)));
            this.RateOfYieldTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 25, true);
            this.RateOfYieldTextBox.Name = "RateOfYieldTextBox";
            this.RateOfYieldTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 15, true);
            this.RateOfYieldTextBox.TabIndex = 1;
            // 
            // IdentificationOfGoodsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.IdentificationofGoodsGroupBox);
            this.Name = "IdentificationOfGoodsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 235, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.IdentificationofGoodsGroupBox.ResumeLayout(false);
            this.IdentificationofGoodsGroupBox.PerformLayout();
            this.IdentificationofGoodsSubGroupBox.ResumeLayout(false);
            this.IdentificationofGoodsSubGroupBox.PerformLayout();
            this.IdentificationOfGoodsCodeDropEdit.ResumeLayout(true);
            this.IdentificationOfGoodsCodeDropEdit.PerformLayout();
            this.ProcessedProductsGroupBox.ResumeLayout(false);
            this.ProcessedProductsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
		internal ZArchitecture.GUI.ZGroupBox IdentificationofGoodsGroupBox;
		internal ZArchitecture.ZTextBox RateOfYieldTextBox;
		internal ZArchitecture.ZTextBox IdentificationOfGoodsDetailsTextBox;
		internal ZArchitecture.GUI.ZGroupBox ProcessedProductsGroupBox;
		internal ZArchitecture.ZTextBox CommodityCodeTextBox;
		internal ZArchitecture.ZTextBox GoodsDescriptionTextBox;
		internal ZArchitecture.GUI.ZGroupBox IdentificationofGoodsSubGroupBox;
		internal ZArchitecture.GUI.ZDropEdit IdentificationOfGoodsCodeDropEdit;
	}
}
