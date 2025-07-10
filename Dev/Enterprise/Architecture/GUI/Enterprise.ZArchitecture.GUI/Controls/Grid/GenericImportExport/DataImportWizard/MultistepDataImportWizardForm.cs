using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	public class MultistepDataImportWizardForm : DataImportWizardForm
	{
		readonly DataTransferProcessor[] processors;
		MultistepProgressForm progressForm;

		[Obsolete("This constructor is just for the designer", true)]
		public MultistepDataImportWizardForm()
		{
		}

		public MultistepDataImportWizardForm(ImportWizardFactory wizardFactory, IImportCollectionInfo collectionInfo, string contextKey, DataTransferProcessor[] processors, string moduleName = "")
			: base(wizardFactory, collectionInfo, contextKey, moduleName)
		{
			this.processors = Argument.NotNull(processors, "processors");
			progressForm = new MultistepProgressForm(1 + processors.Length);
		}

		public MultistepDataImportWizardForm(IImportCollectionInfo collectionInfo, string contextKey, DataTransferProcessor[] processors, string moduleName = "")
			: this(new ImportWizardFactory(), collectionInfo, contextKey, processors, moduleName)
		{
		}

		protected override bool DoProcessing()
		{
			ProcessWhileShowingProgress();
			return true;
		}

		protected override ProgressForm CreateProgressForm()
		{
			return progressForm;
		}

		protected override void DisposeProgressForm()
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (progressForm != null)
			{
				progressForm.Cancelled -= ImportProgressForm_Cancelled;
				progressForm.Dispose();
				progressForm = null;
			}

			base.Dispose(disposing);
		}

		void ProcessWhileShowingProgress()
		{
			var cursorBefore = Cursor;
			Cursor = Cursors.WaitCursor;

			try
			{
				foreach (var processor in processors)
				{
					processor.ProgressChanged += Wizard_ProgressChanged;
					processor.Saving += Wizard_Saving;
					processor.SavingComplete += Wizard_SavingComplete;
					try
					{
						processor.Import();
					}
					finally
					{
						processor.ProgressChanged -= Wizard_ProgressChanged;
						processor.Saving -= Wizard_Saving;
						processor.SavingComplete -= Wizard_SavingComplete;
					}
				}
			}
			finally
			{
				base.DisposeProgressForm();
				Cursor = cursorBefore;
			}
		}
	}
}
