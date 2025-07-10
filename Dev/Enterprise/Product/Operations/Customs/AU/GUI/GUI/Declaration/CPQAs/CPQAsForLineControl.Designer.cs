namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAsForLineControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.ManditoryQuestionsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// QuestionLabel
			// 
			this.questionLabel.BindTo = "CPDecQuestionsViewCollection.Question";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).QuestionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).Question)));
			this.questionLabel.Name = "QuestionLabel";
			// 
			// ManditoryQuestionsGrid
			// 
			this.ManditoryQuestionsGrid.BindTo = "CPDecQuestionsViewCollection";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)));
			zTextBoxColumnStyleInfo1.Caption = "Entry Line Description";
			zTextBoxColumnStyleInfo1.ColumnName = "EntryLineDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Question ID";
			zCalcEditColumnStyleInfo1.ColumnName = "ON_CPDecNum";
			zTextBoxColumnStyleInfo2.Caption = "Entry Line Question";
			zTextBoxColumnStyleInfo2.ColumnName = "Question";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ON_AnswerCode_List";
			zDropEditColumnStyleInfo1.Caption = "Answer Code";
			zDropEditColumnStyleInfo1.ColumnName = "ON_AnswerCode";
			zTextBoxColumnStyleInfo3.Caption = "Permit/Licence";
			zTextBoxColumnStyleInfo3.ColumnName = "ON_Permit";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ManditoryQuestionsGrid.Name = "ManditoryQuestionsGrid";
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).EntryLineDescriptionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).EntryLineDescription)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).QuestionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).Question)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).ON_AnswerCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).ON_AnswerCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).Lookups.ON_AnswerCode_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).ON_PermitInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).CPDecQuestionsViewCollection)))).ON_Permit)));
			// 
			// CPQAsForLineControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCol" +
				"lection";
			this.Name = "CPQAsForLineControl";
			((System.ComponentModel.ISupportInitialize)(this.ManditoryQuestionsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
