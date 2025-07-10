namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class SimilarIncidentsUserControl
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
            this.panelFilters = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.buttonFind = new Enterprise.ZArchitecture.GUI.ZButton();
            this.checkBoxSameClient = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.optionGroupBox = new CargoWise.Windows.UI.KGroupBox();
            this.radioButtonNoProduct = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            this.radioButtonSameProduct = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            this.radioButtonSameProductArea = new Enterprise.ZArchitecture.GUI.ZRadioButton();
            this.dateRangeControl = new Enterprise.ZArchitecture.GUI.Internal.ZDateRangeControl();
            this.dropDownStatus = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.panelIncidents = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.hostControl = new CargoWise.Windows.UI.KElementHost();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.panelFilters.SuspendLayout();
            this.optionGroupBox.SuspendLayout();
            this.dateRangeControl.SuspendLayout();
            this.dropDownStatus.SuspendLayout();
            this.panelIncidents.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.SimilarIncidentsWrapper);
            // 
            // panelFilters
            // 
            this.panelFilters.Controls.Add(this.buttonFind);
            this.panelFilters.Controls.Add(this.checkBoxSameClient);
            this.panelFilters.Controls.Add(this.optionGroupBox);
            this.panelFilters.Controls.Add(this.radioButtonNoProduct);
			this.panelFilters.Controls.Add(this.radioButtonSameProduct);
            this.panelFilters.Controls.Add(this.radioButtonSameProductArea);
            this.panelFilters.Controls.Add(this.dateRangeControl);
            this.panelFilters.Controls.Add(this.dropDownStatus);
            this.panelFilters.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilters.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.panelFilters.Name = "panelFilters";
            this.panelFilters.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 221, true);
            this.panelFilters.TabIndex = 0;
            // 
            // buttonFind
            // 
            this.buttonFind.CaptionResourceString = ZClientEDI.Res.GetData("97700105-f55b-4ac4-8a34-d469037e08c7", "Find");
            this.buttonFind.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 190, true);
            this.buttonFind.Name = "buttonFind";
            this.buttonFind.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.buttonFind.TabIndex = 5;
            this.buttonFind.ToolTipCaption = null;
            this.buttonFind.UseVisualStyleBackColor = true;
            this.buttonFind.Click += new System.EventHandler(this.ButtonFind_Click);
            // 
            // checkBoxSameClient
            // 
            this.checkBoxSameClient.AutoSize = true;
            this.BindingSource.SetBindingMember(this.checkBoxSameClient, "Filter.SameClient");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.SimilarIncidentsWrapper)(null)).Filter.SameClient)));
            this.checkBoxSameClient.CaptionResourceString = ZClientEDI.Res.GetData("5f3b3bdd-129d-4534-81d2-dd11305d9c80", "Similar Incidents for this Client only");
            this.checkBoxSameClient.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 60, true);
            this.checkBoxSameClient.Name = "checkBoxSameClient";
            this.checkBoxSameClient.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.checkBoxSameClient.TabIndex = 1;
            this.checkBoxSameClient.UseVisualStyleBackColor = true;
            // 
            // optionGroupBox
            // 
            this.optionGroupBox.Controls.Add(this.radioButtonNoProduct);
            this.optionGroupBox.Controls.Add(this.radioButtonSameProduct);
            this.optionGroupBox.Controls.Add(this.radioButtonSameProductArea);
            this.optionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 87, true);
            this.optionGroupBox.Name = "optionGroupBox";
            this.optionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 100, true);
            this.optionGroupBox.TabIndex = 2;
            this.optionGroupBox.TabStop = false;
            // 
            // radioButtonNoProduct
            // 
            this.radioButtonNoProduct.AutoCheck = true;
            this.radioButtonNoProduct.AutoSize = true;
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            this.radioButtonNoProduct.CaptionResourceString = ZClientEDI.Res.GetData("f1fe24aa-7b94-4181-b434-eb80c22b7ace", "No additional product filters");
            this.radioButtonNoProduct.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10,20,  true);
            this.radioButtonNoProduct.Name = "radioButtonNoProduct";
            this.radioButtonNoProduct.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 13, true);
            this.radioButtonNoProduct.TabIndex = 2;
            this.radioButtonNoProduct.UseVisualStyleBackColor = true;
            this.radioButtonNoProduct.Checked = true;
            // 
            // radioButtonSameProduct
            // 
            this.radioButtonSameProduct.AutoCheck = false;
            this.radioButtonSameProduct.AutoSize = true;
            this.BindingSource.SetBindingMember(this.radioButtonSameProduct, "Filter.SameProduct");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.SimilarIncidentsWrapper)(null)).Filter.SameProduct)));
            this.radioButtonSameProduct.CaptionResourceString = ZClientEDI.Res.GetData("bc198c4b-5040-462e-8d74-daf4e8f0e438", "Similar Incidents for this Product");
            this.radioButtonSameProduct.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10,50, true);
            this.radioButtonSameProduct.Name = "radioButtonSameProduct";
            this.radioButtonSameProduct.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 13, true);
            this.radioButtonSameProduct.TabIndex = 3;
            this.radioButtonSameProduct.UseVisualStyleBackColor = true;
            // 
            // radioButtonSameProductArea
            // 
            this.radioButtonSameProductArea.AutoCheck = false;
            this.radioButtonSameProductArea.AutoSize = true;
            this.BindingSource.SetBindingMember(this.radioButtonSameProductArea, "Filter.SameProductArea");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.SimilarIncidentsWrapper)(null)).Filter.SameProductArea)));
            this.radioButtonSameProductArea.CaptionResourceString = ZClientEDI.Res.GetData("ca1e8a21-7cac-4002-8f03-b41bdb4384b7", "Similar Incidents for this Product Area");
            this.radioButtonSameProductArea.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10,80, true);
            this.radioButtonSameProductArea.Name = "radioButtonSameProductArea";
            this.radioButtonSameProductArea.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 13, true);
            this.radioButtonSameProductArea.TabIndex = 4;
            this.radioButtonSameProductArea.UseVisualStyleBackColor = true;
            // 
            // dateRangeControl
            // 
            this.dateRangeControl.AllowDrop = true;
            this.dateRangeControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateRangeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
            this.dateRangeControl.Name = "dateRangeControl";
            this.dateRangeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 25, true);
            this.dateRangeControl.TabIndex = 0;
            // 
            // dropDownStatus
            // 
            this.dropDownStatus.AllowDrop = true;
            this.dropDownStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.dropDownStatus, "Filter.IncidentStatusFilter");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation.SimilarIncidentsWrapper)(null)).Filter.IncidentStatusFilter)));
            this.dropDownStatus.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.dropDownStatus.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.dropDownStatus.Name = "dropDownStatus";
            this.dropDownStatus.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(301, 17, true);
            this.dropDownStatus.TabIndex = 0;
            // 
            // panelIncidents
            // 
            this.panelIncidents.Controls.Add(this.hostControl);
            this.panelIncidents.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelIncidents.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.panelIncidents.Name = "panelIncidents";
            this.panelIncidents.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 218, true);
            this.panelIncidents.TabIndex = 1;
            // 
            // hostControl
            // 
            this.hostControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hostControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.hostControl.Name = "hostControl";
            this.hostControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 418, true);
            this.hostControl.TabIndex = 0;
            // 
            // SimilarIncidentsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.panelIncidents);
            this.Controls.Add(this.panelFilters);
            this.Name = "SimilarIncidentsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 599, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.panelFilters.ResumeLayout(false);
            this.panelFilters.PerformLayout();
            this.optionGroupBox.ResumeLayout(false);
            this.optionGroupBox.PerformLayout();
            this.dateRangeControl.ResumeLayout(true);
            this.dateRangeControl.PerformLayout();
            this.dropDownStatus.ResumeLayout(true);
            this.dropDownStatus.PerformLayout();
            this.panelIncidents.ResumeLayout(false);
            this.panelIncidents.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel panelFilters;
		private ZArchitecture.GUI.ZDropEdit dropDownStatus;
		private ZArchitecture.GUI.ZPanel panelIncidents;
		private ZArchitecture.GUI.ZButton buttonFind;
		private ZArchitecture.GUI.ZCheckBox checkBoxSameClient;
		private CargoWise.Windows.UI.KGroupBox optionGroupBox;
		private ZArchitecture.GUI.ZRadioButton radioButtonNoProduct;
		private ZArchitecture.GUI.ZRadioButton radioButtonSameProduct;
		private ZArchitecture.GUI.ZRadioButton radioButtonSameProductArea;
		private Enterprise.ZArchitecture.GUI.Internal.ZDateRangeControl dateRangeControl;
		private CargoWise.Windows.UI.KElementHost hostControl;
	}
}
