namespace Enterprise.Customs.CA.GUI
{
	partial class CARMSOAStatementHeaderDetailsUserControl
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
			this.MessageTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatementTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StatementNumberZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterCustomsIDZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImporterGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PrintDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StatementAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DueDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatementTypeDropEdit.SuspendLayout();
			this.ImporterGuidFindBox.SuspendLayout();
			this.PrintDateDateEdit.SuspendLayout();
			this.DueDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusStatementHeader);
			// 
			// MessageTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTypeTextBox, "ARLMessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).ARLMessageType)));
			this.MessageTypeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8c029a73-8568-46bf-8fb8-588ecd87e814", "Message Type");
			this.MessageTypeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 3, true);
			this.MessageTypeTextBox.Name = "MessageTypeTextBox";
			this.MessageTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.MessageTypeTextBox.TabIndex = 0;
			// 
			// StatementTypeDropEdit
			// 
			this.StatementTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatementTypeDropEdit, "ConvertedStatementType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).ConvertedStatementType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).Lookups.CARMSOAConvertedStatementTypes)));
			this.StatementTypeDropEdit.BindToList = "Lookups.CARMSOAConvertedStatementTypes";
			this.StatementTypeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("24b52959-8a41-499c-b148-eab4809a51c3", "Statement Type");
			this.StatementTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 3, true);
			this.StatementTypeDropEdit.Name = "StatementTypeDropEdit";
			this.StatementTypeDropEdit.PreBoundMaxLength = 2;
			this.StatementTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.StatementTypeDropEdit.TabIndex = 1;
			// 
			// StatementNumberZTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatementNumberZTextBox, "B2_StatementNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_StatementNumber)));
			this.StatementNumberZTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6fc9b418-f43f-4303-9346-5522efd46d5d", "Statement Number");
			this.StatementNumberZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 29, true);
			this.StatementNumberZTextBox.Name = "StatementNumberZTextBox";
			this.StatementNumberZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 20, true);
			this.StatementNumberZTextBox.TabIndex = 2;
			// 
			// ImporterCustomsIDZTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterCustomsIDZTextBox, "B2_ImporterCustomsID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_ImporterCustomsID)));
			this.ImporterCustomsIDZTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("782C4C17-9306-4F22-802C-184B54212D0B", "Business Number");
			this.ImporterCustomsIDZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 55, true);
			this.ImporterCustomsIDZTextBox.Name = "ImporterCustomsIDZTextBox";
			this.ImporterCustomsIDZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ImporterCustomsIDZTextBox.TabIndex = 3;
			// 
			// ImporterGuidFindBox
			// 
			this.ImporterGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterGuidFindBox, "B2_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_OH_Importer)));
			this.ImporterGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("84cbb931-e439-44a2-9824-bcbf7da13afa", "Importer");
			this.ImporterGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 81, true);
			this.ImporterGuidFindBox.Name = "ImporterGuidFindBox";
			this.ImporterGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ImporterGuidFindBox.ParentType = null;
			this.ImporterGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 20, true);
			this.ImporterGuidFindBox.TabIndex = 4;
			// 
			// PrintDateDateEdit
			// 
			this.PrintDateDateEdit.AllowDrop = true;
			this.PrintDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PrintDateDateEdit, "B2_PrintDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_PrintDate)));
			this.PrintDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b80a635e-5c68-4584-ac2e-bcfdccca445e", "Statement Date");
			this.PrintDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 107, true);
			this.PrintDateDateEdit.Name = "PrintDateDateEdit";
			this.PrintDateDateEdit.TabIndex = 5;
			// 
			// StatementAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StatementAmountCalcEdit, "B2_StatementAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_StatementAmount)));
			this.StatementAmountCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("2E5A7133-DAFF-42C9-A816-C9CC7E3C0396", "Statement Total");
			this.StatementAmountCalcEdit.DecimalPlaces = 2;
			this.StatementAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 107, true);
			this.StatementAmountCalcEdit.Name = "StatementAmountCalcEdit";
			this.StatementAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 20, true);
			this.StatementAmountCalcEdit.TabIndex = 6;
			this.StatementAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.StatementAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// DueDateDateEdit
			// 
			this.DueDateDateEdit.AllowDrop = true;
			this.DueDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DueDateDateEdit, "B2_DueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.CusStatementHeader)(null)).B2_DueDate)));
			this.DueDateDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("81be7346-319a-4017-af1e-34d11e070acb", "Payment Due Date");
			this.DueDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 132, true);
			this.DueDateDateEdit.Name = "DueDateDateEdit";
			this.DueDateDateEdit.TabIndex = 7;
			// 
			// CARMSOAStatementHeaderDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessageTypeTextBox);
			this.Controls.Add(this.StatementTypeDropEdit);
			this.Controls.Add(this.StatementNumberZTextBox);
			this.Controls.Add(this.ImporterCustomsIDZTextBox);
			this.Controls.Add(this.ImporterGuidFindBox);
			this.Controls.Add(this.PrintDateDateEdit);
			this.Controls.Add(this.StatementAmountCalcEdit);
			this.Controls.Add(this.DueDateDateEdit);
			this.Name = "CARMSOAStatementHeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1023, 180, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatementTypeDropEdit.ResumeLayout(true);
			this.StatementTypeDropEdit.PerformLayout();
			this.ImporterGuidFindBox.ResumeLayout(true);
			this.ImporterGuidFindBox.PerformLayout();
			this.PrintDateDateEdit.ResumeLayout(true);
			this.PrintDateDateEdit.PerformLayout();
			this.DueDateDateEdit.ResumeLayout(true);
			this.DueDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox MessageTypeTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StatementTypeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox StatementNumberZTextBox;
		private Enterprise.ZArchitecture.ZTextBox ImporterCustomsIDZTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ImporterGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit PrintDateDateEdit;
		private Enterprise.ZArchitecture.ZCalcEdit StatementAmountCalcEdit;
		private ZArchitecture.GUI.ZDateEdit DueDateDateEdit;
	}
}
