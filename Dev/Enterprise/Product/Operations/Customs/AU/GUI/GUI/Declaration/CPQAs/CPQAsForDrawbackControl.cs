namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAsForDrawbackControl : CPQAControlBase
	{
		public CPQAsForDrawbackControl()
		{
			InitializeComponent();
			ManditoryQuestionsGrid.ColumnLayoutContext = nameof(CPQAQuestionsGridContext.Drawback);
		}
	}
}
