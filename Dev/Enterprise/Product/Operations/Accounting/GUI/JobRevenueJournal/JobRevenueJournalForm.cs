using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI
{
	public partial class JobRevenueJournalForm : AccountingZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public JobRevenueJournalForm()
		{
		}

		public JobRevenueJournalForm(JobRevenueJournal journal, IJobRevenueJournalFormPresentationProvider jobRevenueJournalPresentationProvider)
			: base(journal)
		{
			if (jobRevenueJournalPresentationProvider != null)
			{
				JobRevenueJournalFormPresentationProvider = jobRevenueJournalPresentationProvider;
			}
			else
			{
				Dispose();
				throw new ArgumentNullException(nameof(jobRevenueJournalPresentationProvider));
			}

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			DisplayModeChanged += new DisplayModeChangedEventHandler(JobRevenueJournalForm_DisplayModeChanged);

			DeactivateSimpleEntry();
		}

		readonly IJobRevenueJournalFormPresentationProvider JobRevenueJournalFormPresentationProvider;

		JobRevenueJournal ParentJournal
		{
			get { return BusinessEntity as JobRevenueJournal; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var saveResult = base.ShowPreSaveDialogs();

			if (saveResult == ContinueWithSave.Yes)
			{
				var preSaveActionResult = JobRevenueJournalFormPresentationProvider.PreSaveActions();
				if (!preSaveActionResult.CanProceed)
				{
					Globals.Message.ShowError(preSaveActionResult.ErrorMessage);
					saveResult = ContinueWithSave.No;
				}
			}

			return saveResult;
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = base.ValidateAndSave();
			if (result == ContinueWithSave.Yes)
			{
				if (AccountingConfigurationRegistry.Instance.JobRevenueJournalPrintPrompting.Value && Env.Security.PrintJobRevenueJournal.IsAllowed)
				{
					if (Globals.Message.Show(Res.GetString("B283CB14-25BC-45C1-BC6C-33092D2B9C3F", "Do you want to print Job Revenue Journal now?")
						, Res.GetString("F2524462-B356-40CE-80B7-A35765D3845F", "Print Job Revenue Journal"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						AccPrintingUtility printUtil = new AccPrintingUtility(BusinessEntity.Factory, Enterprise.Core.Constants.DataContext.JobRevenueJournal);
						printUtil.PrintDocument(ParentJournal, (NoResString)"Job Revenue Journal", Enterprise.DocumentEngine.AllowedDeliveryOptions.All, CargoWise.Types.ZGuid.Empty, true); // Template name
					}
				}

				AdvancedEditModeButton.Enabled = false;
			}

			return result;
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			ParentJournal.Factory.RemoveContext(BusinessContext.CreateJobRevenueJournalNotInRevenueJournalModule);
		}

		public void ActivateSimpleEntry(Job job)
		{
			if (ParentJournal != null && job != null)
			{
				ParentJournal.ActivateSimpleEntry(job);
				SimpleEntryTabPage.TabVisible = true;
				LinesGrid.ReadOnly = true;
				AdvancedEditModeButton.Enabled = true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void DeactivateSimpleEntry()
		{
			if (ParentJournal != null)
			{
				ParentJournal.DeactivateSimpleEntry();
				Application.DoEvents(); // it needs to reset all validation notifications to prevent SimpleEntryTabPage be showed againg due to them.
				SimpleEntryTabPage.TabVisible = false;
				LinesGrid.ReadOnly = false;
				AdvancedEditModeButton.Enabled = false;
			}
		}

		void AdvancedEditModeButton_Click(object sender, EventArgs e)
		{
			DialogResult questionResult = Globals.Message.Show(
				Res.GetString("AC50D32B-F6DE-468C-9BD1-BA89BA70294B",
@"You are trying to switch to advanced edit mode where you can add journal lines directly. Tab for simplified entry will be closed.
Do you want to continue?"),
				Res.GetString("30A5E06E-2E86-42D2-8A07-1FE8615DC1A7", "Advanced Edit Mode"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);

			if (questionResult == DialogResult.Yes)
			{
				DeactivateSimpleEntry();
			}
		}

		void JobRevenueJournalForm_DisplayModeChanged(object sender, DisplayModeChangedEventArgs e)
		{
			DocManagerReadOnlyOverrideHelper.TrySetReadOnlyOverride(BusinessEntity, e.ToMode);
		}
	}
}
