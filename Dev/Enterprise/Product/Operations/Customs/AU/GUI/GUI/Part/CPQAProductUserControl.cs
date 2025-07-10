namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAProductUserControl : CPQAControlBase
	{
		public CPQAProductUserControl()
		{
			InitializeComponent();
			this.ManditoryQuestionsGrid.ColumnLayoutContext = nameof(CPQAQuestionsGridContext.Product);
		}
	}
}
