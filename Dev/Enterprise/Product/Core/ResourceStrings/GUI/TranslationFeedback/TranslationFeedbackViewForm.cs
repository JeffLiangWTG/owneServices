using System;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public partial class TranslationFeedbackViewForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public TranslationFeedbackViewForm()
		{
			InitializeComponent();
		}

		public TranslationFeedbackViewForm(TopLevelTranslationFeedbackCollection entries)
			: base(entries)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			this.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.ShowOriginalTranslation = true;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
