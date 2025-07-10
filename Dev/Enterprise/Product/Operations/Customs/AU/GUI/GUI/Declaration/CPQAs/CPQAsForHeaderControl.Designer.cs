namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAsForHeaderControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.ManditoryQuestionsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// QuestionLabel
			// 
			this.questionLabel.BindTo = "Questions.Question";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).QuestionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).Question)));
			this.questionLabel.Name = "QuestionLabel";
			// 
			// ManditoryQuestionsGrid
			// 
			this.ManditoryQuestionsGrid.BindTo = "Questions";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)));
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "ID";
			zCalcEditColumnStyleInfo1.ColumnName = "ON_CPDecNum";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.Caption = "Entry Header Question";
			zTextBoxColumnStyleInfo1.ColumnName = "Question";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(330);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ON_AnswerCode_List";
			zDropEditColumnStyleInfo1.Caption = "Answer";
			zDropEditColumnStyleInfo1.ColumnName = "ON_AnswerCode";
			zCheckBoxColumnStyleInfo1.Caption = "Is Conditional";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsOptionalQuestion";
			zTextBoxColumnStyleInfo2.Caption = "Question Type";
			zTextBoxColumnStyleInfo2.ColumnName = "QuestionType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ManditoryQuestionsGrid.Name = "ManditoryQuestionsGrid";
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).QuestionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).Question)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).ON_AnswerCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).ON_AnswerCode)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).Lookups.ON_AnswerCode_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).IsOptionalQuestion)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).IsOptionalQuestionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).QuestionTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeader)(((object)(((Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCollection)(null)))))).Questions)))).QuestionType)));
			// 
			// CPQAsForHeaderControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.CusEntryHeaderMessageStatusFilteredCol" +
				"lection";
			this.Name = "CPQAsForHeaderControl";
			((System.ComponentModel.ISupportInitialize)(this.ManditoryQuestionsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
