using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class CPQAControlBase : ZUserControl
	{
		[CodeAlive("Used in CPQAClassificationUserControl, CPQAsForDrawbackControl, CPQAsForHeaderControl, CPQAsForLineControl and CPQAProductUserControl")]
		protected enum CPQAQuestionsGridContext
		{
			Header,
			Line,
			Drawback,
			Classification,
			Product
		}

		public CPQAControlBase()
		{
			InitializeComponent();
		}
	}
}
