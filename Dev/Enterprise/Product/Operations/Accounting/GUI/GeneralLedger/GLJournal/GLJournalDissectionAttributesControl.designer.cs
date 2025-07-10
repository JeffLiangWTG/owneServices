namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class GLJournalDissectionAttributesControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo attributeTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo attributeValueDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo attributeValueIDGuidFindBoxColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo applicableAlternateChartTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DissectionAttributesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DissectionAttributesGrid)).BeginInit();
			this.DissectionAttributesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// DissectionAttributesGrid
			// 
			this.DissectionAttributesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DissectionAttributesGrid, "AccTransactionLineDissectionAttributes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.Base.Transaction.DependentTransactionLine)(((System.Collections.IList)(((Business.Base.Transaction.TransactionHeaderWithLines)(null)).Lines)).SyncRoot)).AccTransactionLineDissectionAttributes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.AccTransactionLineDissectionAttribute)(((System.Collections.IList)(((Business.Base.Transaction.DependentTransactionLine)(((System.Collections.IList)(((Business.Base.Transaction.TransactionHeaderWithLines)(null)).Lines)).SyncRoot)).AccTransactionLineDissectionAttributes)).SyncRoot)).ALD_Attribute)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.AccTransactionLineDissectionAttribute)(((System.Collections.IList)(((Business.Base.Transaction.DependentTransactionLine)(((System.Collections.IList)(((Business.Base.Transaction.TransactionHeaderWithLines)(null)).Lines)).SyncRoot)).AccTransactionLineDissectionAttributes)).SyncRoot)).ALD_AttributeValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((MasterFiles.Business.AccTransactionLineDissectionAttribute)(((System.Collections.IList)(((Business.Base.Transaction.DependentTransactionLine)(((System.Collections.IList)(((Business.Base.Transaction.TransactionHeaderWithLines)(null)).Lines)).SyncRoot)).AccTransactionLineDissectionAttributes)).SyncRoot)).ALD_AttributeValueID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.AccTransactionLineDissectionAttribute)(((System.Collections.IList)(((Business.Base.Transaction.DependentTransactionLine)(((System.Collections.IList)(((Business.Base.Transaction.TransactionHeaderWithLines)(null)).Lines)).SyncRoot)).AccTransactionLineDissectionAttributes)).SyncRoot)).ApplicableAlternateChart)));
			this.DissectionAttributesGrid.CaptionVisible = false;
			attributeTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2bcf5357-82f2-4d26-b907-96b7f81f35f8", "Attribute");
			attributeTextBoxColumnStyleInfo.ColumnName = "ALD_Attribute";
			attributeTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			attributeTextBoxColumnStyleInfo.IsReadOnly = true;
			attributeTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			attributeValueDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("08733f10-ed74-4458-bd8c-bb0736ad2368", "Code Value");
			attributeValueDropEditColumnStyleInfo.ColumnName = "ALD_AttributeValue";
			attributeValueDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			attributeValueDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			attributeValueIDGuidFindBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("72a666bb-b5a8-432f-9504-752a877cd642", "ID Value");
			attributeValueIDGuidFindBoxColumnStyleInfo.ColumnName = "ALD_AttributeValueID";
			attributeValueIDGuidFindBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			attributeValueIDGuidFindBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			applicableAlternateChartTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("877fa924-f0e5-4c36-85d3-e0de8eb1ad06", "Applicable Alternate Chart");
			applicableAlternateChartTextBoxColumnStyleInfo.ColumnName = "ApplicableAlternateChart";
			applicableAlternateChartTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			applicableAlternateChartTextBoxColumnStyleInfo.IsReadOnly = true;
			applicableAlternateChartTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DissectionAttributesGrid.ColumnStyles.Add(attributeTextBoxColumnStyleInfo);
			this.DissectionAttributesGrid.ColumnStyles.Add(attributeValueDropEditColumnStyleInfo);
			this.DissectionAttributesGrid.ColumnStyles.Add(attributeValueIDGuidFindBoxColumnStyleInfo);
			this.DissectionAttributesGrid.ColumnStyles.Add(applicableAlternateChartTextBoxColumnStyleInfo);
			this.DissectionAttributesGrid.GridId = "3b4dba4d-c957-4c1f-8a7e-22fe0799e036";
			this.DissectionAttributesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DissectionAttributesGrid.LayoutKey = "DissectionAttributesGrid";
			this.DissectionAttributesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DissectionAttributesGrid.Name = "DissectionAttributesGrid";
			this.DissectionAttributesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 91, true);
			this.DissectionAttributesGrid.TabIndex = 0;
			// 
			// GLJournalDissectionAttributesControl
			//
			this.Controls.Add(this.DissectionAttributesGrid);
			this.CaptionRenderingEnabled = true;
			this.Name = "GLJournalDissectionAttributesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(468, 91, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DissectionAttributesGrid)).EndInit();
			this.DissectionAttributesGrid.ResumeLayout(false);
			this.DissectionAttributesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		Enterprise.ZArchitecture.ZGrid DissectionAttributesGrid;
	}
}
