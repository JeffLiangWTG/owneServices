using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public partial class TranslationFeedbackContextsControl : ZUserControl
	{
		public TranslationFeedbackContextsControl()
		{
			InitializeComponent();
		}

		public StmTranslationFeedbackResourceCollection Entries
		{
			get { return (StmTranslationFeedbackResourceCollection)DataSource; }
		}
	}
}
