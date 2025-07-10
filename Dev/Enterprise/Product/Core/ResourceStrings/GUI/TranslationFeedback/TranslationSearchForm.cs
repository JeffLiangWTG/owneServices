using System;
using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public partial class TranslationSearchForm : ZChildForm
	{
		public static void Open()
		{
			if (instanceForm == null || instanceForm.IsDisposed)
			{
				if (instanceCriteria == null)
				{
					instanceCriteria = new TranslationSearchCriteria();
				}
				instanceForm = new TranslationSearchForm(instanceCriteria);
			}
			instanceForm.Show();
		}

		internal static TranslationSearchForm instanceForm;
		internal static TranslationSearchCriteria instanceCriteria;

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public TranslationSearchForm()
		{
			InitializeComponent();
		}

		public TranslationSearchForm(TranslationSearchCriteria bo)
			: base(bo)
		{
			InitializeComponent();
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void searchButton_Click(object sender, EventArgs e)
		{
			this.ValidateAndSave();
		}

		protected override void SaveInternal()
		{
			TopLevelTranslationFeedbackCollection searchResults;
			using (var progressForm = new ProgressForm())
			{
				progressForm.Status = "Searching...";
				progressForm.ShowCancelButton = false;
				progressForm.ShowProgressBar = false;
				progressForm.Show();
				progressForm.Update();

				searchResults = TranslationFeedbackFactory.Search(new BusinessObjectFactory(), (TranslationSearchCriteria)this.BusinessEntity);
			}
			if (searchResults.Count == 0)
			{
				Globals.Message.Show("No matching translations found");
			}
			else
			{
				var form = new TranslationFeedbackCreateForm(searchResults, null);
				if (((TranslationSearchCriteria)this.BusinessEntity).Replace)
				{
					form.translationFeedbackMainUserControl.translationFeedbackEntriesListControl.ShowOriginalTranslation = true;
				}
				form.Show();
			}
		}
	}
}
