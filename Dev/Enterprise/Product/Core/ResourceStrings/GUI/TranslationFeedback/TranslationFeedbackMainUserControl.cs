using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ResourceStrings.GUI
{
	public partial class TranslationFeedbackMainUserControl : ZUserControl
	{
		public TranslationFeedbackMainUserControl()
		{
			InitializeComponent();
			translationFeedbackContextsControl.grid.DoubleClick += new EventHandler(TranslationFeedbackContextsControlGrid_DoubleClick);
			otherTranslationsControl.grid.DoubleClick += new EventHandler(OtherTranslationsControlGrid_DoubleClick);
			otherTranslationsControl.grid.ReadOnly = true;
		}

		internal void TranslationFeedbackContextsControlGrid_DoubleClick(object sender, EventArgs e)
		{
			if (Globals.IsDebugMode || TranslationFeedbackConfiguration.IsMasterDatabase)
			{
				foreach (StmTranslationFeedbackResource context in translationFeedbackContextsControl.grid.SelectedElements)
				{
					new HelpDataStringForm(context.TargetData).Show();
				}
			}
		}

		internal void OtherTranslationsControlGrid_DoubleClick(object sender, EventArgs e)
		{
			var selectedElements = otherTranslationsControl.grid.SelectedElements;
			if (selectedElements.Length > 0)
			{
				var factory = new BusinessObjectFactory();
				var entries = new TopLevelTranslationFeedbackCollection(factory);
				foreach (var entry in selectedElements)
				{
					entries.Add(factory.ImportFromAnotherFactory(entry));
				}
				factory.Saved += new BusinessObjectFactory.SavedEventHandler(otherFactory_Saved);
				ZFormModaliser.Show(new TranslationFeedbackCreateForm(entries, null), FindForm());
			}
		}

		void otherFactory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			foreach (StmTranslationFeedback entry in translationFeedbackEntriesListControl.Entries)
			{
				entry.ReloadOtherTranslations();
			}
		}

		internal void selectAllContextsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (translationFeedbackEntriesListControl.CurrentDataItem != null)
			{
				foreach (StmTranslationFeedbackResource entry in ((StmTranslationFeedback)translationFeedbackEntriesListControl.CurrentDataItem).AllContexts)
				{
					entry.Update = true;
				}
			}
		}

		internal void selectOneContextLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (translationFeedbackEntriesListControl.CurrentDataItem != null)
			{
				foreach (StmTranslationFeedbackResource entry in ((StmTranslationFeedback)translationFeedbackEntriesListControl.CurrentDataItem).AllContexts)
				{
					if (!entry.ReadOnly)
					{
						entry.Update = entry.XQ_ResourceStringKey == ((StmTranslationFeedback)translationFeedbackEntriesListControl.CurrentDataItem).ResourceStringKey;
					}
				}
			}
		}
	}
}
