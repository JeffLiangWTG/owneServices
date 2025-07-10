namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAsForHeaderControl : CPQAControlBase
	{
		public CPQAsForHeaderControl()
		{
			InitializeComponent();
			ManditoryQuestionsGrid.ColumnLayoutContext = nameof(CPQAQuestionsGridContext.Header);
		}
	}
}
