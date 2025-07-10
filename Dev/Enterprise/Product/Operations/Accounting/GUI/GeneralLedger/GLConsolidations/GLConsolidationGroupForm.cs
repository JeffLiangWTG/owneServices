using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLConsolidations
{
	public partial class GLConsolidationGroupForm : ZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public GLConsolidationGroupForm()
		{
			InitializeComponent();
		}

		public GLConsolidationGroupForm(AccConsolidationGroup group)
			: base(group)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		AccConsolidationGroup ConsolidationGroup
		{
			get { return (AccConsolidationGroup)BusinessEntity; }
		}

		void GenerateExportFilesButton_Click(object sender, EventArgs e)
		{
			if (!ConsolidationGroup.IsInDatabase || ConsolidationGroup.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("d7f22c3d-8285-488a-be69-5a47e539b3a8", "Please save this Consolidation Batch before trying to customize the export file format."));
			}
			else
			{
				// add a mutex around this bit and show progress / wait screen
				new ConsolidationBatchCreator(ConsolidationGroup.PK).ReadDataAndCreateBatches();
				var adapter = new ConsolidationBatchExportAdapter(new BusinessObjectFactory(), ConsolidationGroup.PK, true);
				bool doExport = false;

				using (var exportForm = new GLConsolidationExportForm(adapter))
				{
					doExport = ZFormModaliser.ShowDialogWithoutDispose(exportForm) == System.Windows.Forms.DialogResult.OK;
				}

				if (doExport)
				{
					adapter.PopulateRowsForExport();
					IExportCollectionInfo collectionInfo = adapter.GetMultiTypeCollectionInfo();

					using (var form = new DataExportWizardForm(collectionInfo, "GLConsolidations"))
					{
						ZFormModaliser.ShowDialogWithoutDispose(form);
					}

					new AccUsageCollectorProvider().ReportAccConsolidationGroupsGenerateExportFile();
				}
			}
		}

		void CreateEliminationJournalCreateButton_Click(object sender, EventArgs e)
		{
			if (!ConsolidationGroup.IsInDatabase || ConsolidationGroup.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("05970D5D-A135-4961-B1EA-3D6266C9C088", "Please save this Consolidation Batch before trying to create elimination journal."));
			}
			else
			{
				// add a mutex around this whole thing 
				// show progress / wait screen
				new ConsolidationBatchCreator(ConsolidationGroup.PK).ReadDataAndCreateBatches();
				var adapter = new ConsolidationBatchExportAdapter(new BusinessObjectFactory(), ConsolidationGroup.PK, false);
				bool shouldCreateJournals = false;

				using (var eliminationJournalCreationForm = new GLEliminationJournalCreationForm(adapter))
				{
					shouldCreateJournals = ZFormModaliser.ShowDialogWithoutDispose(eliminationJournalCreationForm) == System.Windows.Forms.DialogResult.OK;
				}

				if (shouldCreateJournals)
				{
					var journalCreator = new EliminationJournalCreator();
					journalCreator.ShowError += ShowErrorMessageToUser;
					journalCreator.NoTransactionFound += NoTransactionFoundMessageToUser;
					journalCreator.Create(adapter.GetBatchPKs(true));
					new AccUsageCollectorProvider().ReportAccConsolidationGroupsCreateEliminationJournals();
				}
			}
		}

		void ShowErrorMessageToUser(string message, string caption)
		{
			Globals.Message.ShowError(message, caption);
		}

		void NoTransactionFoundMessageToUser(string message, string caption)
		{
			Globals.Message.ShowInformation(message, caption);
		}
	}
}
