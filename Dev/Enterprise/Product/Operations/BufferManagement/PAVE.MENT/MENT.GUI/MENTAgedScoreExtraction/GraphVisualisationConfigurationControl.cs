using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class GraphVisualisationConfigurationControl : ZUserControl
	{
		public GraphVisualisationConfigurationControl()
		{
			InitializeComponent();

			refreshCategorySequenceZButton.Click += refreshCategorySequenceZButton_Click;

			SetDataSourceBinding(refreshCategorySequenceZButton, "IsEnabledForBinding", "AllowPopulateCategorySequenceCollection");
		}

		protected override void OnBindingContextChanged(EventArgs e)
		{
			base.OnBindingContextChanged(e);

			EnableOrDisableControls();
		}

		bool IsConfigurationEnabled
		{
			get { return Visualisation != null && !Visualisation.IsDeleted && Visualisation.Extraction != null; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			EnableOrDisableControls();
		}

		void EnableOrDisableControls()
		{
			previewButton.Enabled = IsConfigurationEnabled;
			zGrid1.Enabled = IsConfigurationEnabled;

			refreshCategorySequenceZButton.ReadOnly =
				Visualisation == null
				|| Visualisation.IsDeleted
				|| Visualisation.Extraction == null;
		}

		void PreviewButton_Click(object sender, EventArgs e)
		{
			Visualise();
		}

		void Visualise()
		{
			if (IsConfigurationEnabled)
			{
				var needsSave = Visualisation.HasChanges || (Visualisation.Extraction != null && Visualisation.Extraction.HasChanges);
				if (needsSave)
				{
					var result = Globals.Message.Show(Res.GetString("67a4ed9d-8498-4d87-8d40-1beae90ea1b7", "You must save this form before previewing this Visualization / Extraction. Would you like to save now?"), Res.GetString("8e84429b-cebe-46e9-b342-d72ff29e6c10", "Cannot Preview"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result == DialogResult.Yes)
					{
						needsSave = ((ZForm)FindForm()).FireSaveButton() == ContinueWithSave.No;
					}
				}

				if (needsSave)
				{
					return;
				}

				if (Visualisation != null)
				{
					if (!Visualisation.Extraction.IsInstantaneous || (Visualisation.Extraction.RelatedQuery.MAQ_IsActive && Visualisation.Extraction.IsInstantaneous))
					{
						GetAndShowPreviewVisualisationForm();
					}
					else
					{
						Globals.Message.Show(Res.GetString("10b9adfb-2b02-4d73-af4e-d276da9712d0", "This Query is in-active. Queries must be active to be instantaneous"), CannotPreviewString, MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("e19e1bc1-d554-4edb-ab52-0fe4e2968b08", "Please create an extraction before previewing"), CannotPreviewString, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for handling disposables in factory methods")]
		PreviewVisualisationForm GetAndShowPreviewVisualisationForm()
		{
			try
			{
				var viewModelProvider = new VisualisationViewModelProvider(Visualisation.Extraction, Visualisation);
				form = new PreviewVisualisationForm(viewModelProvider, FactoryProvider);
				form.Show();

				return form;
			}
			catch
			{
				try
				{
					if (form != null)
					{
						form.Dispose();
					}
				}
				catch { }
				throw;
			}
		}

		PreviewVisualisationForm form;

		void refreshCategorySequenceZButton_Click(object sender, EventArgs e)
		{
			if (Visualisation != null && !Visualisation.IsDeleted && Visualisation.Extraction != null)
			{
				var needsSave = Visualisation.HasChanges || (Visualisation.Extraction != null && Visualisation.Extraction.HasChanges);
				if (needsSave)
				{
					var result = Globals.Message.Show(Res.GetString("fc9a2aac-7709-448b-814b-d4659b72dbad", "You must save this form before previewing this Visualization / Extraction. Would you like to save now?"), Res.GetString("ce49a74d-69e8-44ea-bd4f-0880ac423402", "Cannot Populate"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
					if (result == DialogResult.Yes)
					{
						needsSave = ((ZForm)FindForm()).FireSaveButton() == ContinueWithSave.No;
					}
				}

				if (needsSave)
				{
					return;
				}

				var parent = Parent as ChartSectionConfigurationControl;
				var chartSectionConfiguration = parent?.BindingSource.DataSource as ChartSectionConfiguration;
				if (chartSectionConfiguration != null && !chartSectionConfiguration.OverrideDefaultVisualisation)
				{
					Globals.Message.Show(
						Res.GetString("61EC478E-B9B0-4C09-AD0F-4A710266E6B5", "'Override Default Visualization' should be enabled in order to populate the grid."),
						Res.GetString("B8957E6C-50DF-4CD6-BD9B-F19B7A78B8B6", "Cannot Populate"),
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				Visualisation.PopulateCategorySequenceCollection(FactoryProvider);
			}
			else
			{
				Globals.Message.Show(Res.GetString("083c6627-4dc9-4812-a818-3b57046383c8", "Please create an extraction before populating"), Res.GetString("9d91d841-b1cb-4c72-be04-5979c0234320", "Cannot Populate"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		string CannotPreviewString
		{
			get { return Res.GetString("3af62d94-86ca-4dcc-8cf0-ab1682c1e18f", "Cannot Preview"); }
		}

		MENTAgedScoreVisualisation Visualisation
		{
			get { return (MENTAgedScoreVisualisation)BindingSource.Current; }
		}

		IVisualisationFactoryProvider FactoryProvider
		{
			get
			{
				if (provider == null)
				{
					provider = new MENTStandAloneFactoryProvider();
				}
				return provider;
			}
		}

		IVisualisationFactoryProvider provider;
	}
}
