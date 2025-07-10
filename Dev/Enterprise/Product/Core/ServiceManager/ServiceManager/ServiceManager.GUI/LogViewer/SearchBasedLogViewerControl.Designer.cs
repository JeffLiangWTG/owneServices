using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	partial class SearchBasedLogViewerControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
            this.HostNameLabel = new Enterprise.ZArchitecture.ZLabel();
            this.hostNameDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ProcessIdLabel = new Enterprise.ZArchitecture.ZLabel();
            this.processIdDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.severityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.dateToUtcDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.SeverityLabel = new Enterprise.ZArchitecture.ZLabel();
            this.DateFromLocalLabel = new Enterprise.ZArchitecture.ZLabel();
            this.DateToUtcLabel = new Enterprise.ZArchitecture.ZLabel();
            this.dateFromLocalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.dateToLocalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.DateFromUtcLabel = new Enterprise.ZArchitecture.ZLabel();
            this.DateToLocalLabel = new Enterprise.ZArchitecture.ZLabel();
            this.dateFromUtcDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.DateToLabel = new Enterprise.ZArchitecture.ZLabel();
            this.DateFromLabel = new Enterprise.ZArchitecture.ZLabel();
            this.searchFilterApplyButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
            this.eventGrid = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
            this.EventLabel = new Enterprise.ZArchitecture.ZLabel();
            this.MessageTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.hostNameDropEdit.SuspendLayout();
            this.processIdDropEdit.SuspendLayout();
            this.severityDropEdit.SuspendLayout();
            this.dateToUtcDateEdit.SuspendLayout();
            this.dateFromLocalDateEdit.SuspendLayout();
            this.dateToLocalDateEdit.SuspendLayout();
            this.dateFromUtcDateEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventGrid)).BeginInit();
            this.eventGrid.SuspendLayout();
            this.MessageTextBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.SearchBasedLogViewer);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.HostNameLabel);
            this.splitContainer1.Panel1.Controls.Add(this.hostNameDropEdit);
            this.splitContainer1.Panel1.Controls.Add(this.ProcessIdLabel);
            this.splitContainer1.Panel1.Controls.Add(this.processIdDropEdit);
            this.splitContainer1.Panel1.Controls.Add(this.severityDropEdit);
            this.splitContainer1.Panel1.Controls.Add(this.dateToUtcDateEdit);
            this.splitContainer1.Panel1.Controls.Add(this.SeverityLabel);
            this.splitContainer1.Panel1.Controls.Add(this.DateFromLocalLabel);
            this.splitContainer1.Panel1.Controls.Add(this.DateToUtcLabel);
            this.splitContainer1.Panel1.Controls.Add(this.dateFromLocalDateEdit);
            this.splitContainer1.Panel1.Controls.Add(this.dateToLocalDateEdit);
            this.splitContainer1.Panel1.Controls.Add(this.DateFromUtcLabel);
            this.splitContainer1.Panel1.Controls.Add(this.DateToLocalLabel);
            this.splitContainer1.Panel1.Controls.Add(this.dateFromUtcDateEdit);
            this.splitContainer1.Panel1.Controls.Add(this.DateToLabel);
            this.splitContainer1.Panel1.Controls.Add(this.DateFromLabel);
            this.splitContainer1.Panel1.Controls.Add(this.searchFilterApplyButton);
            this.splitContainer1.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 452, true);
            this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(4);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(20);
            this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(120);
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // HostNameLabel
            // 
            this.HostNameLabel.AutoSize = true;
            this.HostNameLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|334e524b-55ae-4bc8-95fc-d358e20d9afc", "Host");
            this.HostNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.HostNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(535, 44, true);
            this.HostNameLabel.Name = "HostNameLabel";
            this.HostNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 13, true);
            this.HostNameLabel.TabIndex = 0;
            this.HostNameLabel.UseMnemonic = false;
            // 
            // hostNameDropEdit
            // 
            this.hostNameDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.hostNameDropEdit, "HostName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(null)).HostName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(null)).HostNameList)));
            this.hostNameDropEdit.BindToList = "HostNameList";
            this.hostNameDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.hostNameDropEdit, false);
            this.hostNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 42, true);
            this.hostNameDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 17, true);
            this.hostNameDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 17, true);
            this.hostNameDropEdit.Name = "hostNameDropEdit";
            this.hostNameDropEdit.ShowDescriptionBox = false;
            this.hostNameDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.hostNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 17, true);
            this.hostNameDropEdit.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
            this.hostNameDropEdit.TabIndex = 24;
            // 
            // ProcessIdLabel
            // 
            this.ProcessIdLabel.AutoSize = true;
            this.ProcessIdLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|298f11ef-e08b-43f8-9a46-d218eea598d5", "Process ID");
            this.ProcessIdLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.ProcessIdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(505, 68, true);
            this.ProcessIdLabel.Name = "ProcessIdLabel";
            this.ProcessIdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 13, true);
            this.ProcessIdLabel.TabIndex = 0;
            this.ProcessIdLabel.UseMnemonic = false;
            // 
            // processIdDropEdit
            // 
            this.processIdDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.processIdDropEdit, "ProcessId");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(null)).ProcessId)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(null)).ProcessIdList)));
            this.processIdDropEdit.BindToList = "ProcessIdList";
            this.processIdDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.processIdDropEdit, false);
            this.processIdDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 66, true);
            this.processIdDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 17, true);
            this.processIdDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 17, true);
            this.processIdDropEdit.Name = "processIdDropEdit";
            this.processIdDropEdit.ShowDescriptionBox = false;
            this.processIdDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.processIdDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 17, true);
            this.processIdDropEdit.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 17, true);
            this.processIdDropEdit.TabIndex = 26;
            // 
            // severityDropEdit
            // 
            this.severityDropEdit.AllowDrop = true;
            this.severityDropEdit.AutoSize = true;
            this.BindingSource.SetBindingMember(this.severityDropEdit, "Severity");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(null)).Severity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(null)).SeverityList)));
            this.severityDropEdit.BindToList = "SeverityList";
            this.severityDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.severityDropEdit, false);
            this.severityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 10, true);
            this.severityDropEdit.Name = "severityDropEdit";
            this.severityDropEdit.ShowDescriptionBox = false;
            this.severityDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.severityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 23, true);
            this.severityDropEdit.TabIndex = 14;
            // 
            // dateToUtcDateEdit
            // 
            this.dateToUtcDateEdit.AllowDrop = true;
            this.dateToUtcDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.dateToUtcDateEdit, "ToDateTimeUtc");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(null)).ToDateTimeUtc)));
            this.dateToUtcDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.dateToUtcDateEdit, false);
            this.dateToUtcDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 66, true);
            this.dateToUtcDateEdit.Name = "dateToUtcDateEdit";
            this.dateToUtcDateEdit.TabIndex = 22;
            // 
            // SeverityLabel
            // 
            this.SeverityLabel.AutoSize = true;
            this.SeverityLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|6b27ef1b-af93-4bf7-af43-c3def64eb064", "Severity");
            this.SeverityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.SeverityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 14, true);
            this.SeverityLabel.Name = "SeverityLabel";
            this.SeverityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(45, 13, true);
            this.SeverityLabel.TabIndex = 0;
            this.SeverityLabel.UseMnemonic = false;
            // 
            // DateFromLocalLabel
            // 
            this.DateFromLocalLabel.AutoSize = true;
            this.DateFromLocalLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|68d0880f-bb7b-47db-99f0-94a72c2a241e", "(Local)");
            this.DateFromLocalLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.DateFromLocalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 44, true);
            this.DateFromLocalLabel.Name = "DateFromLocalLabel";
            this.DateFromLocalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 10, true);
            this.DateFromLocalLabel.TabIndex = 0;
            this.DateFromLocalLabel.Text = "(Local)";
            this.DateFromLocalLabel.UseMnemonic = false;
            // 
            // DateToUtcLabel
            // 
            this.DateToUtcLabel.AutoSize = true;
            this.DateToUtcLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|1adec507-73ae-4343-bce7-acc3f27db20f", "UTC");
            this.DateToUtcLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.DateToUtcLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 71, true);
            this.DateToUtcLabel.Name = "DateToUtcLabel";
            this.DateToUtcLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 10, true);
            this.DateToUtcLabel.TabIndex = 0;
            this.DateToUtcLabel.Text = "(UTC)";
            this.DateToUtcLabel.UseMnemonic = false;
            // 
            // dateFromLocalDateEdit
            // 
            this.dateFromLocalDateEdit.AllowDrop = true;
            this.dateFromLocalDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.dateFromLocalDateEdit, "FromDateTimeLocal");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(null)).FromDateTimeLocal)));
            this.dateFromLocalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.dateFromLocalDateEdit, false);
            this.dateFromLocalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 39, true);
            this.dateFromLocalDateEdit.Name = "dateFromLocalDateEdit";
            this.dateFromLocalDateEdit.TabIndex = 16;
            // 
            // dateToLocalDateEdit
            // 
            this.dateToLocalDateEdit.AllowDrop = true;
            this.dateToLocalDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.dateToLocalDateEdit, "ToDateTimeLocal");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(null)).ToDateTimeLocal)));
            this.dateToLocalDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.dateToLocalDateEdit, false);
            this.dateToLocalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 66, true);
            this.dateToLocalDateEdit.Name = "dateToLocalDateEdit";
            this.dateToLocalDateEdit.TabIndex = 20;
            // 
            // DateFromUtcLabel
            // 
            this.DateFromUtcLabel.AutoSize = true;
            this.DateFromUtcLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|3016a8e0-7521-4df8-b0e6-30df863b1de7", "UTC");
            this.DateFromUtcLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.DateFromUtcLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 44, true);
            this.DateFromUtcLabel.Name = "DateFromUtcLabel";
            this.DateFromUtcLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 10, true);
            this.DateFromUtcLabel.TabIndex = 0;
            this.DateFromUtcLabel.Text = "(UTC)";
            this.DateFromUtcLabel.UseMnemonic = false;
            // 
            // DateToLocalLabel
            // 
            this.DateToLocalLabel.AutoSize = true;
            this.DateToLocalLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|130ed5a1-93db-436b-9eaf-7cbb1843f5d7", "Local");
            this.DateToLocalLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Small;
            this.DateToLocalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(45, 71, true);
            this.DateToLocalLabel.Name = "DateToLocalLabel";
            this.DateToLocalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 10, true);
            this.DateToLocalLabel.TabIndex = 0;
            this.DateToLocalLabel.Text = "(Local)";
            this.DateToLocalLabel.UseMnemonic = false;
            // 
            // dateFromUtcDateEdit
            // 
            this.dateFromUtcDateEdit.AllowDrop = true;
            this.dateFromUtcDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.dateFromUtcDateEdit, "FromDateTimeUtc");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(null)).FromDateTimeUtc)));
            this.dateFromUtcDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.dateFromUtcDateEdit, false);
            this.dateFromUtcDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 39, true);
            this.dateFromUtcDateEdit.Name = "dateFromUtcDateEdit";
            this.dateFromUtcDateEdit.TabIndex = 18;
            // 
            // DateToLabel
            // 
            this.DateToLabel.AutoSize = true;
            this.DateToLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|1adec507-73ae-4343-bce7-acc3f27db20f", "To");
            this.DateToLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.DateToLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 68, true);
            this.DateToLabel.Name = "DateToLabel";
            this.DateToLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 13, true);
            this.DateToLabel.TabIndex = 0;
            this.DateToLabel.Text = "To";
            this.DateToLabel.UseMnemonic = false;
            // 
            // DateFromLabel
            // 
            this.DateFromLabel.AutoSize = true;
            this.DateFromLabel.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|68d0880f-bb7b-47db-99f0-94a72c2a241e", "From");
            this.DateFromLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.DateFromLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 42, true);
            this.DateFromLabel.Name = "DateFromLabel";
            this.DateFromLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
            this.DateFromLabel.TabIndex = 0;
            this.DateFromLabel.Text = "From";
            this.DateFromLabel.UseMnemonic = false;
            // 
            // searchFilterApplyButton
            // 
            this.searchFilterApplyButton.BackColor = System.Drawing.SystemColors.ControlLight;
            this.searchFilterApplyButton.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|e35ddf88-af9f-4439-ab8a-67ca8ce68193", "Find");
            this.searchFilterApplyButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.searchFilterApplyButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.searchFilterApplyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(75, 94, true);
            this.searchFilterApplyButton.Name = "searchFilterApplyButton";
            this.searchFilterApplyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 23, true);
            this.searchFilterApplyButton.TabIndex = 28;
            this.searchFilterApplyButton.ToolTipCaption = null;
            this.searchFilterApplyButton.UseVisualStyleBackColor = false;
            this.searchFilterApplyButton.Click += new System.EventHandler(this.searchFilterApplyButton_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.eventGrid);
            this.splitContainer2.Panel1.Controls.Add(this.EventLabel);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.MessageTextBox);
            this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 331, true);
            this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(197);
            this.splitContainer2.SplitterWidth = 17;
            this.splitContainer2.TabIndex = 0;
            // 
            // eventGrid
            // 
            this.eventGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.eventGrid, "EventList");
            this.eventGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("b113511c-b6b0-43ae-adde-3c6053292e41", "Seq. Number");
            zCalcEditColumnStyleInfo1.ColumnName = "ElasticSequenceId";
            zCalcEditColumnStyleInfo1.Decimals = 0;
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|a71b2776-926e-4bc5-b9e1-1cf2df166fe5", "Time (UTC)");
            zDateEditColumnStyleInfo1.ColumnName = "DateTime";
            zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.IsMandatory = true;
            zDateEditColumnStyleInfo1.IsReadOnly = true;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|7c70f184-ce3b-4ed8-9158-a008147de891", "Time (Local)");
            zDateEditColumnStyleInfo2.ColumnName = "DateTimeLocal";
            zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.IsReadOnly = true;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|3d373868-a56d-447a-aec0-19ed1f64c0ff", "Host");
            zTextBoxColumnStyleInfo1.ColumnName = "HostName";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|4b8474c9-f6f5-4463-ae6d-c6ccac48cafc", "Severity");
            zTextBoxColumnStyleInfo2.ColumnName = "Type";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("7878EDF1-A8B1-4EF4-AC4E-FF85FCF32E47", "Process ID");
            zCalcEditColumnStyleInfo2.ColumnName = "ProcessId";
            zCalcEditColumnStyleInfo2.Decimals = 0;
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.ServiceManager.GUI.Res.GetData("LogViewerControl|54082bfe-f379-4221-b270-d8248207d4a5", "Message");
            zTextBoxColumnStyleInfo3.ColumnName = "Message";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.IsReadOnly = true;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(550);
            this.eventGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.eventGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.eventGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.eventGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.eventGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.eventGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.eventGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.eventGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.eventGrid.ForceShowExportToExcelMenuItem = true;
            this.eventGrid.GridId = "188125e9-1c25-4472-a74f-afdd4fb146be";
            this.eventGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.eventGrid.IsWholeRowSelectedOnClick = true;
            this.eventGrid.LayoutKey = "eventGrid";
            this.eventGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
            this.eventGrid.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
            this.eventGrid.Name = "eventGrid";
            this.eventGrid.ReadOnly = true;
            this.eventGrid.ShouldSetErrorsOnTabPage = false;
            this.eventGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 172, true);
            this.eventGrid.TabIndex = 30;
            this.eventGrid.ColourDeciding += new System.EventHandler<Enterprise.ZArchitecture.ColourDecidingEventArgs>(this.eventGrid_ColourDeciding);
            // 
            // EventLabel
            // 
            this.EventLabel.AutoSize = true;
            this.EventLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.EventLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.EventLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.EventLabel.Name = "EventLabel";
            this.EventLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
            this.EventLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 25, true);
            this.EventLabel.TabIndex = 0;
            this.EventLabel.UseMnemonic = false;
            // 
            // MessageTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageTextBox, "EventList.FormattedMessageAsBlob");
            this.MessageTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageTextBox.IsToolBarVisible = false;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTextBox, false);
            this.MessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MessageTextBox.MaxLength = 10000000;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.ParentZForm = null;
            this.MessageTextBox.PopupFormCaption = null;
            this.MessageTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this.MessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 117, true);
            this.MessageTextBox.TabIndex = 34;
            // 
            // SearchBasedLogViewerControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.splitContainer1);
            this.Name = "SearchBasedLogViewerControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(895, 452, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer1.PerformLayout();
            this.hostNameDropEdit.ResumeLayout(true);
            this.hostNameDropEdit.PerformLayout();
            this.processIdDropEdit.ResumeLayout(true);
            this.processIdDropEdit.PerformLayout();
            this.severityDropEdit.ResumeLayout(true);
            this.severityDropEdit.PerformLayout();
            this.dateToUtcDateEdit.ResumeLayout(true);
            this.dateToUtcDateEdit.PerformLayout();
            this.dateFromLocalDateEdit.ResumeLayout(true);
            this.dateFromLocalDateEdit.PerformLayout();
            this.dateToLocalDateEdit.ResumeLayout(true);
            this.dateToLocalDateEdit.PerformLayout();
            this.dateFromUtcDateEdit.ResumeLayout(true);
            this.dateFromUtcDateEdit.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eventGrid)).EndInit();
            this.eventGrid.ResumeLayout(false);
            this.eventGrid.PerformLayout();
            this.MessageTextBox.ResumeLayout(true);
            this.MessageTextBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private CargoWise.Windows.UI.KSplitContainer splitContainer2;
		private Enterprise.ZArchitecture.GUI.ZRichTextBox MessageTextBox;
		private Enterprise.ZArchitecture.ZLabel EventLabel;
		private Enterprise.ZArchitecture.GUI.ZDisplayGrid eventGrid;

		private Enterprise.ZArchitecture.GUI.ZButton searchFilterApplyButton;
		private Enterprise.ZArchitecture.ZLabel SeverityLabel;
		private ZArchitecture.GUI.ZDropEdit severityDropEdit;
		private Enterprise.ZArchitecture.ZLabel HostNameLabel;
		private ZArchitecture.GUI.ZDropEdit hostNameDropEdit;
		private Enterprise.ZArchitecture.ZLabel ProcessIdLabel;
		private ZArchitecture.GUI.ZDropEdit processIdDropEdit;
		private ZArchitecture.ZLabel DateToLabel;
		private ZArchitecture.GUI.ZDateEdit dateToUtcDateEdit;
		private ZArchitecture.ZLabel DateToUtcLabel;
		private ZArchitecture.GUI.ZDateEdit dateToLocalDateEdit;
		private ZArchitecture.ZLabel DateToLocalLabel;
		private ZArchitecture.ZLabel DateFromLabel;
		private ZArchitecture.GUI.ZDateEdit dateFromUtcDateEdit;
		private ZArchitecture.ZLabel DateFromUtcLabel;
		private ZArchitecture.GUI.ZDateEdit dateFromLocalDateEdit;
		private ZArchitecture.ZLabel DateFromLocalLabel;
	}
}
