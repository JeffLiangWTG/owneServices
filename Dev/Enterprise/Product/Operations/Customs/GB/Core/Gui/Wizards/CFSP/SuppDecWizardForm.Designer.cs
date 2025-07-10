namespace Enterprise.Customs.GB.GUI.Wizards
{
	partial class SuppDecWizardForm
	{ 
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.PackagesCountTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zDropEdit2 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.NumberOfPackagesRemainingBalanceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 308, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 22, true);
			this.MainStatusBar.TabIndex = 13;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Business.Wizards.CFSP.SuppDecWizard);
			// 
			// PackagesCountTextBox
			// 
			this.PackagesCountTextBox.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.PackagesCountTextBox, "NumberPackagesToDeclare");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.GB.Business.Wizards.CFSP.SuppDecWizard)(null)).NumberPackagesToDeclare)));
			this.PackagesCountTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 155, true);
			this.PackagesCountTextBox.Name = "PackagesCountTextBox";
			this.PackagesCountTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.PackagesCountTextBox.TabIndex = 7;
			// 
			// OkButton
			// 
			this.OkButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.OkButton.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("84e019cf-88cd-46bc-82e1-86610617575c", "Create");
			this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 273, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 12;
			this.OkButton.UseVisualStyleBackColor = true;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.AllowDrop = true;
			this.zDropEdit1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.zDropEdit1, "DeclarationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Wizards.CFSP.SuppDecWizard)(null)).DeclarationType)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 226, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.zDropEdit1.TabIndex = 10;
			// 
			// zDropEdit2
			// 
			this.zDropEdit2.AllowDrop = true;
			this.zDropEdit2.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.zDropEdit2, "SupplementaryProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.GB.Business.Wizards.CFSP.SuppDecWizard)(null)).SupplementaryProcedure)));
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 250, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.zDropEdit2.TabIndex = 11;
			// 
			// zTextBox1
			// 
			this.zTextBox1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.zTextBox1, "NumberOfPackagesOfParentBox6");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.GB.Business.Wizards.CFSP.SuppDecWizard)(null)).NumberOfPackagesOfParentBox6)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 41, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.zTextBox1.TabIndex = 1;
			// 
			// zTextBox2
			// 
			this.zTextBox2.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.zTextBox2, "NumberOfPackagesDeclaredOnSiblingDeclarations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.GB.Business.Wizards.CFSP.SuppDecWizard)(null)).NumberOfPackagesDeclaredOnSiblingDeclarations)));
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 79, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.zTextBox2.TabIndex = 3;
			// 
			// zTextBox3
			// 
			this.zTextBox3.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.zTextBox3, "NumberOfPackagesRemainingOnSiblingDeclarations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.GB.Business.Wizards.CFSP.SuppDecWizard)(null)).NumberOfPackagesRemainingOnSiblingDeclarations)));
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 117, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.zTextBox3.TabIndex = 5;
			// 
			// NumberOfPackagesRemainingBalanceTextBox
			// 
			this.NumberOfPackagesRemainingBalanceTextBox.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.NumberOfPackagesRemainingBalanceTextBox, "NumberOfPackagesRemainingBalance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.GB.Business.Wizards.CFSP.SuppDecWizard)(null)).NumberOfPackagesRemainingBalance)));
			this.NumberOfPackagesRemainingBalanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 193, true);
			this.NumberOfPackagesRemainingBalanceTextBox.Name = "NumberOfPackagesRemainingBalanceTextBox";
			this.NumberOfPackagesRemainingBalanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.NumberOfPackagesRemainingBalanceTextBox.TabIndex = 9;
			// 
			// zLabel3
			// 
			this.zLabel3.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.zLabel3.IsFontBold = true;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 141, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 10, true);
			this.zLabel3.TabIndex = 6;
			this.zLabel3.Text = "-";
			this.zLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel2
			// 
			this.zLabel2.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.zLabel2.IsFontBold = true;
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 103, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 10, true);
			this.zLabel2.TabIndex = 4;
			this.zLabel2.Text = "=";
			this.zLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 65, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 10, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = "-";
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// zTextBox4
			// 
			this.zTextBox4.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.zTextBox4, "NumberOfSiblingDeclarations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.GB.Business.Wizards.CFSP.SuppDecWizard)(null)).NumberOfSiblingDeclarations)));
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 9, true);
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.zTextBox4.TabIndex = 0;
			// 
			// zLabel4
			// 
			this.zLabel4.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.zLabel4.IsFontBold = true;
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 179, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 10, true);
			this.zLabel4.TabIndex = 8;
			this.zLabel4.Text = "=";
			this.zLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// SuppDecWizardForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("f00c085f-4e86-45ea-894c-ac569a5eb442", "Supplementary Declaration Wizard");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 330, true);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.zDropEdit2);
			this.Controls.Add(this.NumberOfPackagesRemainingBalanceTextBox);
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.PackagesCountTextBox);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zTextBox4);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.zTextBox3);
			this.Controls.Add(this.zTextBox2);
			this.DataSourceType = typeof(Enterprise.Customs.GB.Business.Wizards.CFSP.SuppDecWizard);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "SuppDecWizardForm";
			this.Controls.SetChildIndex(this.zTextBox2, 0);
			this.Controls.SetChildIndex(this.zTextBox3, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.zTextBox4, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.PackagesCountTextBox, 0);
			this.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.Controls.SetChildIndex(this.NumberOfPackagesRemainingBalanceTextBox, 0);
			this.Controls.SetChildIndex(this.zDropEdit2, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox PackagesCountTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton OkButton;
		private ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private ZArchitecture.GUI.ZDropEdit zDropEdit2;
		private ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.ZTextBox zTextBox2;
		private ZArchitecture.ZTextBox zTextBox3; 
		private ZArchitecture.ZTextBox zTextBox4;
		private ZArchitecture.ZTextBox NumberOfPackagesRemainingBalanceTextBox;
		private ZArchitecture.ZLabel zLabel3;
		private ZArchitecture.ZLabel zLabel2;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.ZLabel zLabel4;
	}
}
