namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAProductUserControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.ManditoryQuestionsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// QuestionLabel
			// 
			this.BindingSource.SetBindingMember(this.questionLabel, "Questions.Question");
			this.questionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(697, 53, true);
			// 
			// ManditoryQuestionsGrid
			// 
			this.BindingSource.SetBindingMember(this.ManditoryQuestionsGrid, "Questions");
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Question ID";
			zCalcEditColumnStyleInfo1.ColumnName = "ON_CPDecNum";
			zTextBoxColumnStyleInfo1.Caption = "Question";
			zTextBoxColumnStyleInfo1.ColumnName = "Question";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.ON_AnswerCode_List";
			zDropEditColumnStyleInfo1.Caption = "Answer";
			zDropEditColumnStyleInfo1.ColumnName = "ON_AnswerCode";
			zTextBoxColumnStyleInfo2.Caption = "Permit";
			zTextBoxColumnStyleInfo2.ColumnName = "ON_Permit";
			zDateEditColumnStyleInfo1.Caption = "Start Date";
			zDateEditColumnStyleInfo1.ColumnName = "ON_CPDecStartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Caption = "End Date";
			zDateEditColumnStyleInfo2.ColumnName = "ON_CPDecEndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ManditoryQuestionsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ManditoryQuestionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 248, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CusClassPartPivot);
			// 
			// CPQAProductUserControl
			// 
			this.Name = "CPQAProductUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 320, true);
			((System.ComponentModel.ISupportInitialize)(this.ManditoryQuestionsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
