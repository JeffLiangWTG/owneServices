namespace Enterprise.Customs.DE.GUI
{
	public partial class ImportBottomSectionUserControl
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.BottomGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PreviousDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.TransportPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransportIDInLandTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalWarningUserControl = new Enterprise.Customs.GUI.MessageSendingFormBottomSectionUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomGroupBox.SuspendLayout();
			this.TransportPanel.SuspendLayout();
			this.AdditionalWarningUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction);
			// 
			// BottomGroupBox
			// 
			this.BottomGroupBox.Controls.Add(this.PreviousDocumentsUserControl);
			this.BottomGroupBox.Controls.Add(this.TransportPanel);
			this.BottomGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 100, true);
			this.BottomGroupBox.Name = "BottomGroupBox";
			this.BottomGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 258, true);
			this.BottomGroupBox.TabIndex = 3;
			this.BottomGroupBox.TabStop = false;
			// 
			// PreviousDocumentsUserControl
			// 
			this.PreviousDocumentsUserControl.AllowDrop = true;
			this.PreviousDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 46, true);
			this.PreviousDocumentsUserControl.Name = "PreviousDocumentsUserControl";
			this.PreviousDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 209, true);
			this.PreviousDocumentsUserControl.TabIndex = 2;
			// 
			// TransportPanel
			// 
			this.TransportPanel.Controls.Add(this.TransportIDInLandTextBox);
			this.TransportPanel.Controls.Add(this.GoodsLocationTextBox);
			this.TransportPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TransportPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TransportPanel.Name = "TransportPanel";
			this.TransportPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 30, true);
			this.TransportPanel.TabIndex = 0;
			// 
			// TransportIDInLandTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportIDInLandTextBox, "TransportIDInLand");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).TransportIDInLand)));
			this.TransportIDInLandTextBox.CaptionResourceString = null;
			this.TransportIDInLandTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 5, true);
			this.TransportIDInLandTextBox.Name = "TransportIDInLandTextBox";
			this.TransportIDInLandTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TransportIDInLandTextBox.TabIndex = 0;
			// 
			// GoodsLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.GoodsLocationTextBox, "GoodsLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).GoodsLocation)));
			this.GoodsLocationTextBox.CaptionResourceString = null;
			this.GoodsLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 5, true);
			this.GoodsLocationTextBox.Name = "GoodsLocationTextBox";
			this.GoodsLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.GoodsLocationTextBox.TabIndex = 1;
			// 
			// AdditionalWarningUserControl
			// 
			this.AdditionalWarningUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalWarningUserControl, "ActionParent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Business.IJobDeclarationMessageSendingObjectParent)(((Enterprise.Customs.DE.Business.Declaration.ImportEntryMessageSendingAction)(null)).ActionParent)));
			this.AdditionalWarningUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.AdditionalWarningUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalWarningUserControl.Name = "AdditionalWarningUserControl";
			this.AdditionalWarningUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 100, true);
			this.AdditionalWarningUserControl.TabIndex = 4;
			// 
			// ImportBottomSectionUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BottomGroupBox);
			this.Controls.Add(this.AdditionalWarningUserControl);
			this.Name = "ImportBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 358, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomGroupBox.ResumeLayout(false);
			this.BottomGroupBox.PerformLayout();
			this.TransportPanel.ResumeLayout(false);
			this.TransportPanel.PerformLayout();
			this.AdditionalWarningUserControl.ResumeLayout(true);
			this.AdditionalWarningUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl PreviousDocumentsUserControl;
		internal ZArchitecture.GUI.ZGroupBox BottomGroupBox;
		internal Enterprise.Customs.GUI.MessageSendingFormBottomSectionUserControl AdditionalWarningUserControl;
		internal ZArchitecture.ZTextBox TransportIDInLandTextBox;
		internal ZArchitecture.ZTextBox GoodsLocationTextBox;
		internal ZArchitecture.GUI.ZPanel TransportPanel;

	}
}
