namespace Enterprise.Customs.AU.Declaration.GUI
{
    public partial class CPQACollectionUserControl
    {
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.BindTo = "Questions";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)));
			this.zGrid1.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.BindToList = "Lookups+CPQuestions";
			zCodeFindBoxColumnStyleInfo1.Caption = "Question ID";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "QuestionID";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.CMRLodgementQuestion;
			zDateEditColumnStyleInfo1.Caption = "Start Date";
			zDateEditColumnStyleInfo1.ColumnName = "ON_CPDecStartDate";
			zTextBoxColumnStyleInfo1.Caption = "Question";
			zTextBoxColumnStyleInfo1.ColumnName = "Question";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ON_AnswerCode_List";
			zDropEditColumnStyleInfo1.Caption = "Answer Code";
			zDropEditColumnStyleInfo1.ColumnName = "ON_AnswerCode";
			zTextBoxColumnStyleInfo2.Caption = "Permit";
			zTextBoxColumnStyleInfo2.ColumnName = "ON_Permit";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.zGrid1.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.EnableToolTips = false;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 456, true);
			this.zGrid1.TabIndex = 0;
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).QuestionIDInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).QuestionID)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).Lookups.CPQuestions)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).ON_CPDecStartDate)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).ON_CPDecStartDateInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).QuestionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).Question)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).ON_AnswerCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).ON_AnswerCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).Lookups.ON_AnswerCode_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).ON_PermitInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA)(null)).Questions)))).ON_Permit)));
			// 
			// CPQACollectionUserControl
			// 
			this.Controls.Add(this.zGrid1);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.OrganisationCPQA";
			this.Name = "CPQACollectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(776, 456, true);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}

		ZArchitecture.ZGrid zGrid1;
	}
}
