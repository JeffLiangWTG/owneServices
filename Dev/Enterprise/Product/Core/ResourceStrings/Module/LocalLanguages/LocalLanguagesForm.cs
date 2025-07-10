using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ResourceStrings.Module
{
	public partial class LocalLanguagesForm : ZForm
	{
		public LocalLanguagesForm()
		{
			InitializeComponent();
		}

		public LocalLanguagesForm(RefLocalLanguage language)
			: base(language)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
		}
	}
}
