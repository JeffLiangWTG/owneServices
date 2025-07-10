using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Messaging.GUI
{
	partial class InterchangeEventUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo indexColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo eventTimeColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo eventCodeColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo eventTextColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo xtMsgIdColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo interchangeLinkColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.xtInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.xtMsgIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.showAllxTEventsLogCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.showRelatedxTEventsLogCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.xtMsgStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.xtMsgEventRefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();			
			this.xtEventsGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EventsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.xtInformationGroupBox.SuspendLayout();
			this.xtEventsGridGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EventsGrid)).BeginInit();
			this.EventsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIInterchange);
			// 
			// xtInformationGroupBox
			// 
			this.xtInformationGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("{C293DA3E-D3FD-4BE1-B3A4-30E6AFCF9D44}", "xT Message Information");
			this.xtInformationGroupBox.Controls.Add(this.xtMsgIdTextBox);
			this.xtInformationGroupBox.Controls.Add(this.xtMsgEventRefreshButton);
			this.xtInformationGroupBox.Controls.Add(this.showAllxTEventsLogCheckBox);
			this.xtInformationGroupBox.Controls.Add(this.showRelatedxTEventsLogCheckBox);
			this.xtInformationGroupBox.Controls.Add(this.xtMsgStatusTextBox);
			this.xtInformationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.xtInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.xtInformationGroupBox.Name = "xtInformationGroupBox";
			this.xtInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 66, true);
			this.xtInformationGroupBox.TabIndex = 0;
			this.xtInformationGroupBox.TabStop = false;
			this.xtInformationGroupBox.Text = "xT Message Information";
			// 
			// xtMsgIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.xtMsgIdTextBox, "EI_XTInternalMsgID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ulong)(((EDIInterchange)null).EI_XTInternalMsgID)));
			this.xtMsgIdTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("{10B53A62-4532-4DBD-9B4F-72CE70F340C9}", "xT Message ID");
			this.xtMsgIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 31, true);
			this.xtMsgIdTextBox.Name = "xtMsgIdTextBox";
			this.xtMsgIdTextBox.ReadOnly = true;
			this.xtMsgIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 23, true);
			this.xtMsgIdTextBox.TabIndex = 1;
			// 
			// xtMsgStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.xtMsgStatusTextBox, "EI_XTInternalMsgStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((EDIInterchange)null).EI_XTInternalMsgStatus)));
			this.xtMsgStatusTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("D52B97C4-C8AD-41B9-870B-EFCE3463298F", "xT Message Status");
			this.xtMsgStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 31, true);
			this.xtMsgStatusTextBox.Name = "xtMsgStatusTextBox";
			this.xtMsgStatusTextBox.ReadOnly = true;
			this.xtMsgStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.xtMsgStatusTextBox.TabIndex = 2;
			// 
			// xtMsgEventRefreshButton
			//
			this.xtMsgEventRefreshButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.xtMsgEventRefreshButton.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("76E052D1-2FF7-42BC-A359-7020650D93B6", "Refresh");
			this.xtMsgEventRefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(485, 31, true);
			this.xtMsgEventRefreshButton.Name = "xtMsgEventRefreshButton";
			this.xtMsgEventRefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			this.xtMsgEventRefreshButton.UseVisualStyleBackColor = true;
			this.xtMsgEventRefreshButton.Click += new System.EventHandler(this.xtMsgEventRefreshButton_Click);
			this.xtMsgEventRefreshButton.TabIndex = 3;
			// 
			// showAllxTEventsLogCheckBox
			// 
			this.BindingSource.SetBindingMember(this.showAllxTEventsLogCheckBox, "EI_ShowAllxTEventsLog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((EDIInterchange)null).EI_ShowAllxTEventsLog)));
			this.showAllxTEventsLogCheckBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("C4025412-ACC7-4C81-940A-F7A6AE1DCB1C", "Show All xT Events Log");
			this.showAllxTEventsLogCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 31, true);
			this.showAllxTEventsLogCheckBox.Name = "showAllxTEventsLogCheckBox";
			this.showAllxTEventsLogCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.showAllxTEventsLogCheckBox.UseVisualStyleBackColor = false;
			this.showAllxTEventsLogCheckBox.TabIndex = 4;
			this.showAllxTEventsLogCheckBox.EditableInViewMode = true;
			// 
			// showRelatedxTEventsLogCheckBox
			// 
			this.BindingSource.SetBindingMember(this.showRelatedxTEventsLogCheckBox, "EI_ShowRelatedxTEventsLog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((EDIInterchange)null).EI_ShowRelatedxTEventsLog)));
			this.showRelatedxTEventsLogCheckBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("B6580114-A6E6-4208-B724-7E1ACC353FCC", "Show Related xT Events Log");
			this.showRelatedxTEventsLogCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(780, 31, true);
			this.showRelatedxTEventsLogCheckBox.Name = "showRelatedxTEventsLogCheckBox";
			this.showRelatedxTEventsLogCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 23, true);
			this.showRelatedxTEventsLogCheckBox.UseVisualStyleBackColor = false;
			this.showRelatedxTEventsLogCheckBox.TabIndex = 5;
			this.showRelatedxTEventsLogCheckBox.EditableInViewMode = true;
			// 
			// xtEventsGridGroupBox
			//
			this.xtEventsGridGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
					| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.xtEventsGridGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("{51123D35-13CF-4C31-ADF8-E5ED08326450}", "Events");
			this.xtEventsGridGroupBox.Controls.Add(this.EventsGrid);
			this.xtEventsGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 70, true);
			this.xtEventsGridGroupBox.Name = "xtEventsGridGroupBox";
			this.xtEventsGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 380, true);
			this.xtEventsGridGroupBox.TabIndex = 6;
			this.xtEventsGridGroupBox.TabStop = false;
			this.xtEventsGridGroupBox.Text = "Events";
			// 
			// EventsGrid
			//
			this.EventsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EventsGrid, "XtMessageEvents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check((((Enterprise.Messaging.Business.XtMessageEventsCollection)((EDIInterchange)(null)).XtMessageEvents)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZInt)(((Enterprise.Messaging.Business.XtMessageEvent)(null)).Sequence)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.XtMessageEvent)(null)).LogTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZInt)(((Enterprise.Messaging.Business.XtMessageEvent)(null)).LogEvent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.XtMessageEvent)(null)).LogText)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.XtMessageEvent)(null)).XtMsgId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.XtMessageEvent)(null)).InterchangeId)));
			this.EventsGrid.CaptionVisible = false;
			indexColumnStyleInfo.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("{4CD0526E-A20C-49E4-AB28-257C50678C22}", "Sequence");
			indexColumnStyleInfo.ColumnName = "Sequence";
			indexColumnStyleInfo.DefaultCollectionIndex = 0;
			indexColumnStyleInfo.IsReadOnly = true;
			indexColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			eventTimeColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			eventTimeColumnStyleInfo.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("{C6840463-0695-4E7E-98ED-4E325BE92458}", "Event Time");
			eventTimeColumnStyleInfo.ColumnName = "LogTime";
			eventTimeColumnStyleInfo.DefaultCollectionIndex = 0;
			eventTimeColumnStyleInfo.IsReadOnly = true;
			eventTimeColumnStyleInfo.IsSortable = true;
			eventTimeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			eventCodeColumnStyleInfo.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("{D42B5843-6151-4BD3-875D-6B554BB2BE3D}", "Event");
			eventCodeColumnStyleInfo.ColumnName = "LogEvent";
			eventCodeColumnStyleInfo.DefaultCollectionIndex = 0;
			eventCodeColumnStyleInfo.IsReadOnly = true;
			eventCodeColumnStyleInfo.IsSortable = true;
			eventCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			eventTextColumnStyleInfo.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("{05AB21A6-C1A6-404D-B70C-D17D62ABB9E9}", "Event Text");
			eventTextColumnStyleInfo.ColumnName = "LogText";
			eventTextColumnStyleInfo.DefaultCollectionIndex = 0;
			eventTextColumnStyleInfo.IsReadOnly = true;
			eventTextColumnStyleInfo.IsSortable = true;
			eventTextColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			xtMsgIdColumnStyleInfo.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("{AD27B1AE-7941-4D5E-8575-C3740E1BE668}", "xT Message ID");
			xtMsgIdColumnStyleInfo.ColumnName = "XtMsgId";
			xtMsgIdColumnStyleInfo.DefaultCollectionIndex = 0;
			xtMsgIdColumnStyleInfo.IsReadOnly = true;
			xtMsgIdColumnStyleInfo.IsSortable = true;
			xtMsgIdColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			interchangeLinkColumnStyleInfo.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("{B6F10F93-4723-49F4-B23F-DE8123FFEE80}", "Interchange Number");
			interchangeLinkColumnStyleInfo.ColumnName = "InterchangeId";
			interchangeLinkColumnStyleInfo.DefaultCollectionIndex = 0;
			interchangeLinkColumnStyleInfo.IsReadOnly = true;
			interchangeLinkColumnStyleInfo.IsSortable = true;
			interchangeLinkColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.EventsGrid.ColumnStyles.Add(indexColumnStyleInfo);
			this.EventsGrid.ColumnStyles.Add(eventTimeColumnStyleInfo);
			this.EventsGrid.ColumnStyles.Add(eventCodeColumnStyleInfo);
			this.EventsGrid.ColumnStyles.Add(eventTextColumnStyleInfo);
			this.EventsGrid.ColumnStyles.Add(xtMsgIdColumnStyleInfo);
			this.EventsGrid.ColumnStyles.Add(interchangeLinkColumnStyleInfo);
			this.EventsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EventsGrid.GridId = "1abdc0de-d95f-4773-be32-6171451f5fda";
			this.EventsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EventsGrid.LayoutKey = "EventGrid";
			this.EventsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.EventsGrid.Name = "EventsGrid";
			this.EventsGrid.ReadOnly = true;
			this.EventsGrid.ShouldSetErrorsOnTabPage = false;
			this.EventsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 380, true);
			this.EventsGrid.TabIndex = 7;
			this.EventsGrid.IsWholeRowSelectedOnClick = true;
			this.EventsGrid.DoubleClick += new System.EventHandler(this.EventsGrid_DoubleClick);
			// 
			// InterchangeEventUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.xtInformationGroupBox);
			this.Controls.Add(this.xtEventsGridGroupBox);
			this.Name = "InterchangeEventUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 455, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.xtInformationGroupBox.ResumeLayout(false);
			this.xtInformationGroupBox.PerformLayout();
			this.xtEventsGridGroupBox.ResumeLayout(false);
			this.xtEventsGridGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.EventsGrid)).EndInit();
			this.EventsGrid.ResumeLayout(false);
			this.EventsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.ZGrid EventsGrid;
		ZArchitecture.GUI.ZGroupBox xtInformationGroupBox;
		ZArchitecture.GUI.ZGroupBox xtEventsGridGroupBox;

		ZTextBox xtMsgIdTextBox;
		ZArchitecture.GUI.ZCheckBox showAllxTEventsLogCheckBox;
		ZArchitecture.GUI.ZCheckBox showRelatedxTEventsLogCheckBox;
		ZTextBox xtMsgStatusTextBox;
		ZArchitecture.GUI.ZButton xtMsgEventRefreshButton;
	}
}
