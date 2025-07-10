using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class LinkedeNettEDIMessagePluginUserControl
	{
void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.MessagesGrid = new ZArchitecture.ZGrid();
			this.linkedeNettEDIMessageUserControl1 = new LinkedeNettEDIMessageUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EDIMessage);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.MessagesGrid);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.linkedeNettEDIMessageUserControl1);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 310, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.SplitContainer.TabIndex = 1;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((EDIMessage)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(null)).EM_ApplicationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(null)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(null)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(null)).EM_ReceiveTransmit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(null)).EM_Status)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_ApplicationCode";
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo3.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo4.ColumnName = "EM_ReceiveTransmit";
			zTextBoxColumnStyleInfo5.ColumnName = "EM_Status";
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.GridId = "206af300-1788-48d4-9ecc-f9d06de6267f";
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "MessagesGrid";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 100, true);
			this.MessagesGrid.TabIndex = 1;
			// 
			// linkedeNettEDIMessageUserControl1
			// 
			this.BindingSource.SetBindingMember(this.linkedeNettEDIMessageUserControl1, ".");
			this.linkedeNettEDIMessageUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.linkedeNettEDIMessageUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.linkedeNettEDIMessageUserControl1.Name = "linkedeNettEDIMessageUserControl1";
			this.linkedeNettEDIMessageUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 206, true);
			this.linkedeNettEDIMessageUserControl1.TabIndex = 0;
			// 
			// LinkedeNettEDIMessagePluginUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SplitContainer);
			this.Name = "LinkedeNettEDIMessagePluginUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			this.SplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.ResumeLayout(false);
		}

	}
}