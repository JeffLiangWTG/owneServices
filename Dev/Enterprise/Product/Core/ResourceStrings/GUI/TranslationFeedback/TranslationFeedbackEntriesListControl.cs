using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public partial class TranslationFeedbackEntriesListControl : ZUserControl
	{
		public TranslationFeedbackEntriesListControl()
		{
			InitializeComponent();
			new ColumnResizer(grid, StmTranslationFeedback.Schema.XT_Source, StmTranslationFeedback.Schema.XT_OriginalTranslation, StmTranslationFeedback.Schema.XT_SuggestedTranslation);
			new TranslationFeedbackDiffHighlighter(grid);
			this.Load += TranslationFeedbackEntriesListControl_Load;
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		void TranslationFeedbackEntriesListControl_Load(object sender, EventArgs e)
		{
			new RowResizer(grid, StmTranslationFeedback.Schema.XT_Source, StmTranslationFeedback.Schema.XT_SuggestedTranslation);
			grid.PerformLayout();
		}

		public StmTranslationFeedbackCollection Entries
		{
			get { return (StmTranslationFeedbackCollection)DataSource; }
		}

		public bool ShowOriginalTranslation
		{
			get { return grid.GetColumnStyle(StmTranslationFeedback.Schema.XT_OriginalTranslation).IsVisible; }
			set
			{
				grid.GetColumnStyle(StmTranslationFeedback.Schema.XT_OriginalTranslation).IsVisible = value;
				if (value)
				{
					grid.GridId = "2b9604eb-adf8-4a31-82cf-9221b1d0121b";
				}
				else
				{
					grid.GridId = "fb43c799-f320-468e-b5f2-d6d2f08ae35a";
				}
			}
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			if (Entries != null)
			{
				SetLanguageCaptions();
			}
		}

		void SetLanguageCaptions()
		{
			if (Entries.Count > 0)
			{
				var languages = Entries[0].Lookups.Languages;
				grid.Columns[StmTranslationFeedback.Schema.XT_Source].ColumnStyle.HeaderText = languages.GetDescriptionFromCode(Res.DefaultLanguage);
				grid.Columns[StmTranslationFeedback.Schema.XT_OriginalTranslation].ColumnStyle.HeaderText = Res.GetString("b1b93a5a-5bb9-4be2-a9a0-45325c712432", "Original {0}", languages.GetDescriptionFromCode(Entries[0].XT_Language));
				if (grid.Columns[StmTranslationFeedback.Schema.XT_OriginalTranslation].IsVisible)
				{
					grid.Columns[StmTranslationFeedback.Schema.XT_SuggestedTranslation].ColumnStyle.HeaderText = Res.GetString("dd6d0c04-5a33-4872-9ebf-2d69e9e7240f", "Suggested {0}", languages.GetDescriptionFromCode(Entries[0].XT_Language));
				}
				else
				{
					grid.Columns[StmTranslationFeedback.Schema.XT_SuggestedTranslation].ColumnStyle.HeaderText = languages.GetDescriptionFromCode(Entries[0].XT_Language);
				}
			}
		}
	}
}
