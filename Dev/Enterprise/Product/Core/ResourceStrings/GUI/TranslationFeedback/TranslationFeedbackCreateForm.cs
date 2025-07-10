using System;
using System.Windows.Forms;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public partial class TranslationFeedbackCreateForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public TranslationFeedbackCreateForm()
		{
			InitializeComponent();
		}

#if DEBUG
		public TranslationFeedbackCreateForm(TopLevelTranslationFeedbackCollection entries)
			: this(entries, null)
		{ }
#endif

		public TranslationFeedbackCreateForm(TopLevelTranslationFeedbackCollection entries, Control sourceControl)
			: base(entries)
		{
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			this.sourceControl = sourceControl;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		protected override void SaveInternal()
		{
			base.SaveInternal();
			RefreshSourceControl();
		}

		void RefreshSourceControl()
		{
			if (sourceControl != null)
			{
				sourceControl.UpdateCaption();
			}
		}

		readonly Control sourceControl;
	}
}
