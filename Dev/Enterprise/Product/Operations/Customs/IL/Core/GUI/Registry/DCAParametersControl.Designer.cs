namespace Enterprise.Customs.IL.GUI
{
	partial class DCAParametersControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.peekWayZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.serviceNamesZDisplayGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			this.allServicesZRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.specificServicesZRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.maxMessagesZIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.peekWayZDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.serviceNamesZDisplayGrid)).BeginInit();
			this.serviceNamesZDisplayGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.DCAParameters);
			// 
			// peekWayZDropEdit
			// 
			this.peekWayZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.peekWayZDropEdit, "PeekWay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.DCAParameters)(null)).PeekWay)));
			this.peekWayZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 2, true);
			this.peekWayZDropEdit.Name = "peekWayZDropEdit";
			this.peekWayZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
			this.peekWayZDropEdit.TabIndex = 0;
			// 
			// serviceNamesZDisplayGrid
			// 
			this.serviceNamesZDisplayGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.serviceNamesZDisplayGrid, "Services");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Business.DCAParameters)(null)).Services)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.DCAService)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.DCAParameters)(null)).Services)).SyncRoot)).Name)));
			this.serviceNamesZDisplayGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.IL.GUI.Res.GetData("F019365C-94BF-4BF4-90A1-C54EE11EE66D", "Service Names");
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(320);
			this.serviceNamesZDisplayGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.serviceNamesZDisplayGrid.GridId = "64394eed-59ab-4ed0-b828-f0c8297beeed";
			this.serviceNamesZDisplayGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.serviceNamesZDisplayGrid.LayoutKey = "serviceNamesZDisplayGrid";
			this.serviceNamesZDisplayGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 51, true);
			this.serviceNamesZDisplayGrid.Name = "serviceNamesZDisplayGrid";
			this.serviceNamesZDisplayGrid.ShouldSetErrorsOnTabPage = false;
			this.serviceNamesZDisplayGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 142, true);
			this.serviceNamesZDisplayGrid.TabIndex = 2;
			// 
			// allServicesZRadioButton
			// 
			this.allServicesZRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.allServicesZRadioButton, "AllServices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IL.Business.DCAParameters)(null)).AllServices)));
			this.allServicesZRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 22, true);
			this.allServicesZRadioButton.Name = "allServicesZRadioButton";
			this.allServicesZRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 24, true);
			this.allServicesZRadioButton.TabIndex = 0;
			this.allServicesZRadioButton.TabStop = true;
			this.allServicesZRadioButton.UseVisualStyleBackColor = true;
			this.allServicesZRadioButton.CheckedChanged += new System.EventHandler(this.allServicesZRadioButton_CheckedChanged);
			// 
			// specificServicesZRadioButton
			// 
			this.specificServicesZRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.specificServicesZRadioButton, "SpecificServices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IL.Business.DCAParameters)(null)).SpecificServices)));
			this.specificServicesZRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 22, true);
			this.specificServicesZRadioButton.Name = "specificServicesZRadioButton";
			this.specificServicesZRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 24, true);
			this.specificServicesZRadioButton.TabIndex = 1;
			this.specificServicesZRadioButton.TabStop = true;
			this.specificServicesZRadioButton.UseVisualStyleBackColor = true;
			// 
			// maxMessagesZIntEdit
			// 
			this.BindingSource.SetBindingMember(this.maxMessagesZIntEdit, "MaxMessagesPerIteration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.IL.Business.DCAParameters)(null)).MaxMessagesPerIteration)));
			this.maxMessagesZIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 204, true);
			this.maxMessagesZIntEdit.Name = "maxMessagesZIntEdit";
			this.maxMessagesZIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 15, true);
			this.maxMessagesZIntEdit.TabIndex = 3;
			// 
			// DCAParametersControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.peekWayZDropEdit);
			this.Controls.Add(this.maxMessagesZIntEdit);
			this.Controls.Add(this.specificServicesZRadioButton);
			this.Controls.Add(this.allServicesZRadioButton);
			this.Controls.Add(this.serviceNamesZDisplayGrid);
			this.Name = "DCAParametersControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 342, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.peekWayZDropEdit.ResumeLayout(true);
			this.peekWayZDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.serviceNamesZDisplayGrid)).EndInit();
			this.serviceNamesZDisplayGrid.ResumeLayout(false);
			this.serviceNamesZDisplayGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit peekWayZDropEdit;
		private ZArchitecture.GUI.ZDisplayGrid serviceNamesZDisplayGrid;
		private ZArchitecture.GUI.ZRadioButton allServicesZRadioButton;
		private ZArchitecture.GUI.ZRadioButton specificServicesZRadioButton;
		private ZArchitecture.GUI.ZIntEdit maxMessagesZIntEdit;
	}
}
