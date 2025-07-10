namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	partial class DocBuilderDataSourceControl
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
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FreightRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.BrokerageRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.DocBuilderDataSource);
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("97a73a2b-665d-46e3-95a5-47425704beb6", "Data Source");
			this.MainGroupBox.Controls.Add(this.FreightRadioButton);
			this.MainGroupBox.Controls.Add(this.BrokerageRadioButton);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 87, true);
			this.MainGroupBox.TabIndex = 0;
			this.MainGroupBox.TabStop = false;
			// 
			// FreightRadioButton
			// 
			this.FreightRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.FreightRadioButton, "Freight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngineCore.Registry.DocBuilderDataSource)(null)).Freight)));
			this.FreightRadioButton.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("71740467-b8b8-46a6-9895-845752f84e48", "Always use data from Freight (least compliant)");
			this.FreightRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FreightRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(23, 55, true);
			this.FreightRadioButton.Name = "FreightRadioButton";
			this.FreightRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 24, true);
			this.FreightRadioButton.TabIndex = 1;
			this.FreightRadioButton.TabStop = true;
			this.FreightRadioButton.UseVisualStyleBackColor = true;
			// 
			// BrokerageRadioButton
			// 
			this.BrokerageRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.BrokerageRadioButton, "Brokerage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngineCore.Registry.DocBuilderDataSource)(null)).Brokerage)));
			this.BrokerageRadioButton.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("9fb98799-0a9a-4477-a13b-8a66df7a1de4", "Always first use data from Brokerage (most compliant)");
			this.BrokerageRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BrokerageRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 25, true);
			this.BrokerageRadioButton.Name = "BrokerageRadioButton";
			this.BrokerageRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 24, true);
			this.BrokerageRadioButton.TabIndex = 0;
			this.BrokerageRadioButton.TabStop = true;
			this.BrokerageRadioButton.UseVisualStyleBackColor = true;
			// 
			// DocBuilderDataSourceControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Name = "DocBuilderDataSourceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 93, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		internal ZArchitecture.GUI.ZRadioButton FreightRadioButton;
		internal ZArchitecture.GUI.ZRadioButton BrokerageRadioButton;
	}
}
