namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAsForDrawbackControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.ManditoryQuestionsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// QuestionLabel
			// 
			this.questionLabel.BindTo = "DrawbackQuestions.Question";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).DrawbackQuestions)))).QuestionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).DrawbackQuestions)))).Question)));
			// 
			// ManditoryQuestionsGrid
			// 
			this.ManditoryQuestionsGrid.BindTo = "DrawbackQuestions";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).DrawbackQuestions)));
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "ID";
			zCalcEditColumnStyleInfo1.ColumnName = "ON_CPDecNum";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.Caption = "Drawback Question";
			zTextBoxColumnStyleInfo1.ColumnName = "Question";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(520);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ON_AnswerCode_List";
			zDropEditColumnStyleInfo1.Caption = "Answer";
			zDropEditColumnStyleInfo1.ColumnName = "ON_AnswerCode";
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).DrawbackQuestions)))).ON_CPDecNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).DrawbackQuestions)))).ON_CPDecNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).DrawbackQuestions)))).QuestionInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).DrawbackQuestions)))).Question)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).DrawbackQuestions)))).Lookups.ON_AnswerCode_List)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).DrawbackQuestions)))).ON_AnswerCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CMRCusEntryCPDec)(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).DrawbackQuestions)))).ON_AnswerCode)));
			// 
			// CPQAsForDrawbackControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.JobDeclaration";
			this.Name = "CPQAsForDrawbackControl";
			((System.ComponentModel.ISupportInitialize)(this.ManditoryQuestionsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
