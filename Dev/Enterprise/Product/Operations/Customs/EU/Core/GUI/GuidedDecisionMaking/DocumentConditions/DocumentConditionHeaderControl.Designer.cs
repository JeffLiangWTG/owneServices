using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.GUI
{
	partial class DocumentConditionHeaderControl
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
			this.ConditionDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsSatisfiedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.IsSatisfiedResultLabel = new Enterprise.ZArchitecture.ZLabel();
			this.kTableLayoutPanel1 = new CargoWise.Windows.UI.KTableLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.kTableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.GuidedDecisionMakingCondition);
			// 
			// ConditionDescriptionLabel
			// 
			this.ConditionDescriptionLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ConditionDescriptionLabel, "ConditionDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingCondition)(null)).ConditionDescription)));
			this.kTableLayoutPanel1.SetColumnSpan(this.ConditionDescriptionLabel, 2);
			this.ConditionDescriptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConditionDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ConditionDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 0, true);
			this.ConditionDescriptionLabel.Name = "ConditionDescriptionLabel";
			this.ConditionDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 12, true);
			this.ConditionDescriptionLabel.TabIndex = 0;
			this.ConditionDescriptionLabel.UseMnemonic = false;
			// 
			// IsSatisfiedLabel
			// 
			this.IsSatisfiedLabel.AutoSize = true;
			this.IsSatisfiedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IsSatisfiedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.IsSatisfiedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 12, true);
			this.IsSatisfiedLabel.Name = "IsSatisfiedLabel";
			this.IsSatisfiedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.IsSatisfiedLabel.TabIndex = 1;
			this.IsSatisfiedLabel.Text = "Condition Satisfied:";
			this.IsSatisfiedLabel.UseMnemonic = false;
			// 
			// IsSatisfiedResultLabel
			// 
			this.IsSatisfiedResultLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSatisfiedResultLabel, "IsSatisfiedTextForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingCondition)(null)).IsSatisfiedTextForBinding)));
			this.IsSatisfiedResultLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IsSatisfiedResultLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.IsSatisfiedResultLabel.IsFontBold = true;
			this.IsSatisfiedResultLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 12, true);
			this.IsSatisfiedResultLabel.Name = "IsSatisfiedResultLabel";
			this.IsSatisfiedResultLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 20, true);
			this.IsSatisfiedResultLabel.TabIndex = 2;
			this.IsSatisfiedResultLabel.UseMnemonic = false;
			// 
			// kTableLayoutPanel1
			// 
			this.kTableLayoutPanel1.AutoSize = true;
			this.kTableLayoutPanel1.ColumnCount = 2;
			this.kTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.kTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.kTableLayoutPanel1.Controls.Add(this.IsSatisfiedLabel, 0, 1);
			this.kTableLayoutPanel1.Controls.Add(this.IsSatisfiedResultLabel, 1, 1);
			this.kTableLayoutPanel1.Controls.Add(this.ConditionDescriptionLabel, 0, 0);
			this.kTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kTableLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kTableLayoutPanel1.Name = "kTableLayoutPanel1";
			this.kTableLayoutPanel1.RowCount = 2;
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.kTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.kTableLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 32, true);
			this.kTableLayoutPanel1.TabIndex = 3;
			// 
			// DocumentConditionHeaderControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.Controls.Add(this.kTableLayoutPanel1);
			this.Name = "DocumentConditionHeaderControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(522, 32, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.kTableLayoutPanel1.ResumeLayout(false);
			this.kTableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel ConditionDescriptionLabel;
		private ZArchitecture.ZLabel IsSatisfiedLabel;
		private ZArchitecture.ZLabel IsSatisfiedResultLabel;
		private CargoWise.Windows.UI.KTableLayoutPanel kTableLayoutPanel1;
	}
}
