namespace Enterprise.Customs.DE.ExitControl.GUI
{
	partial class ConsignmentAdditionalInformationTabUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AdditionalInformationSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ConsignmentAdditionalInformationGridUserControl = new Enterprise.Customs.DE.ExitControl.GUI.ConsignmentAdditionalInformationGridUserControl();
			this.FullTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInformationSplitContainer)).BeginInit();
			this.AdditionalInformationSplitContainer.Panel1.SuspendLayout();
			this.AdditionalInformationSplitContainer.Panel2.SuspendLayout();
			this.AdditionalInformationSplitContainer.SuspendLayout();
			this.ConsignmentAdditionalInformationGridUserControl.SuspendLayout();
			this.FullTypeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.ExitControl.Business.ExitControlAdditionalInfoCollection);
			// 
			// AdditionalInformationSplitContainer
			// 
			this.AdditionalInformationSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInformationSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInformationSplitContainer.Name = "AdditionalInformationSplitContainer";
			this.AdditionalInformationSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// AdditionalInformationSplitContainer.Panel1
			// 
			this.AdditionalInformationSplitContainer.Panel1.Controls.Add(this.ConsignmentAdditionalInformationGridUserControl);
			// 
			// AdditionalInformationSplitContainer.Panel2
			// 
			this.AdditionalInformationSplitContainer.Panel2.Controls.Add(this.FullTypeCodeFindBox);
			this.AdditionalInformationSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 365, true);
			this.AdditionalInformationSplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(50);
			this.AdditionalInformationSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(307);
			this.AdditionalInformationSplitContainer.TabIndex = 0;
			// 
			// ConsignmentAdditionalInformationGridUserControl
			// 
			this.ConsignmentAdditionalInformationGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsignmentAdditionalInformationGridUserControl, ".");
			this.ConsignmentAdditionalInformationGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignmentAdditionalInformationGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsignmentAdditionalInformationGridUserControl.Name = "ConsignmentAdditionalInformationGridUserControl";
			this.ConsignmentAdditionalInformationGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 307, true);
			this.ConsignmentAdditionalInformationGridUserControl.TabIndex = 0;
			// 
			// FullTypeCodeFindBox
			// 
			this.FullTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FullTypeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.ExitControl.Business.ExitControlAdditionalInfo)(null)).CSI_Code)));
			this.FullTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 14, true);
			this.FullTypeCodeFindBox.Name = "FullTypeCodeFindBox";
			this.FullTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FullTypeCodeFindBox.ParentType = null;
			this.FullTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.FullTypeCodeFindBox.TabIndex = 0;
			this.FullTypeCodeFindBox.TabStop = false;
			// 
			// ConsignmentAdditionalInformationTabUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalInformationSplitContainer);
			this.Name = "ConsignmentAdditionalInformationTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 365, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalInformationSplitContainer.Panel1.ResumeLayout(false);
			this.AdditionalInformationSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInformationSplitContainer)).EndInit();
			this.AdditionalInformationSplitContainer.ResumeLayout(false);
			this.AdditionalInformationSplitContainer.PerformLayout();
			this.ConsignmentAdditionalInformationGridUserControl.ResumeLayout(true);
			this.ConsignmentAdditionalInformationGridUserControl.PerformLayout();
			this.FullTypeCodeFindBox.ResumeLayout(true);
			this.FullTypeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal CargoWise.Windows.UI.KSplitContainer AdditionalInformationSplitContainer;
		internal ConsignmentAdditionalInformationGridUserControl ConsignmentAdditionalInformationGridUserControl;
		internal ZArchitecture.GUI.ZCodeFindBox FullTypeCodeFindBox;
	}
}
