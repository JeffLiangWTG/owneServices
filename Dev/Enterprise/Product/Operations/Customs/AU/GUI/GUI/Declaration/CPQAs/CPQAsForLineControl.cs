namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAsForLineControl : CPQAControlBase
	{
		public CPQAsForLineControl()
		{
			InitializeComponent();
			ManditoryQuestionsGrid.ColumnLayoutContext = nameof(CPQAQuestionsGridContext.Line);
		}
	}
}
