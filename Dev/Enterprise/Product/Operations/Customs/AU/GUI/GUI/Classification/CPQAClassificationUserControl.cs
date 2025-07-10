namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAClassificationUserControl : CPQAControlBase
	{
		public CPQAClassificationUserControl()
		{
			InitializeComponent();
			this.ManditoryQuestionsGrid.ColumnLayoutContext = nameof(CPQAQuestionsGridContext.Classification);
		}
	}
}
