using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.JobManagement
{
	public partial class JobManagementForm : ZForm
	{
		readonly Job relatedJob;
		readonly ZGuid relatedJobPK;

		public JobManagementForm()
		{
		}

		public JobManagementForm(JobProfitLoss profitLoss)
			: base(profitLoss)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton);
			DisableMenuItems();

			if (profitLoss.JobPKs.Any())
			{
				relatedJobPK = profitLoss.JobPKs.First();
				relatedJob = profitLoss.Factory.Load<Job>(relatedJobPK);
			}

			relatedJob?.InitializeParentFromGenericJobWithoutSettingDefaults();

			if (!HasJobError())
			{
				ProfitLossControl.PluginSecurity = relatedJob.PlugInData.InvoicingSupporter.JobInvoicingSecurity;
			}

			JobProfitLossRefreshBindingSuspender = profitLoss.RefreshBindingSuspender.GetSuspender();
		}

		readonly IDisposable JobProfitLossRefreshBindingSuspender;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			AutoAddPreviousNextButtons = true;
			base.OnLoad(e);

			JobProfitLossRefreshBindingSuspender?.Dispose();
		}

		bool HasJobError()
		{
			var jobHasError = false;

			if (relatedJob == null)
			{
				jobHasError = true;
				Globals.Message.ShowError(Res.GetString("2f306333-f33a-4c9d-9c92-0206e6ec342d", "Cannot find corresponding job."));
				ErrorReporter.ReportOnce("JobManagementForm", string.Format(CultureInfo.InvariantCulture, "Cannot find corresponding job for JobManagementForm. Factory is unable to find Job with PK '{0}'.", relatedJobPK)); // Developer exception
			}
			else if (relatedJob.PlugInData == null)
			{
				jobHasError = true;
				Globals.Message.ShowError(Job.GetJobDoesNotHaveParentMessage());
			}
			else if (relatedJob.IsDeleted)
			{
				jobHasError = true;
				Globals.Message.ShowError(Res.GetString("37111666-02fb-447c-aed5-3dc8aac15f26", "Unable to access the operational job record. Please try accessing the job from the relevant operational module."));
			}

			return jobHasError;
		}

		protected override void PopulateDevTools(System.Collections.Generic.List<IDevTool> tools)
		{
			base.PopulateDevTools(tools);
			tools.Add(new JobManagementDocumentCustomisationDevTool());
			tools.Add(new JobManagementConsolDocumentCustomisationDevTool());
		}

		#region GUI Setup

		void DisableMenuItems()
		{
			ZFormMenuStrategy.SetMenuItemEnabled(this, ZFormMenuStrategy.FileNewMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemEnabled(this, ZFormMenuStrategy.FileSaveMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemEnabled(this, ZFormMenuStrategy.FileSaveAndCloseMenuItemName, false);
			ZFormMenuStrategy.SetMenuItemEnabled(this, ZFormMenuStrategy.FileDeleteMenuItemName, false);
			//PostingButtonsUserControl.SaveButton.Enabled = false;
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}

				JobProfitLossRefreshBindingSuspender?.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Operations Job

		void ShowOperationsForm(Job relatedJob)
		{
			if (!HasJobError())
			{
				GenericJob genericJob = (GenericJob)relatedJob.Factory.LoadGenericJob(relatedJob);

				BusinessObject bO = genericJob.Consumer as BusinessObject;
				ControllerID consumerControllerID = genericJob.GetConsumerController();
				if (consumerControllerID != null)
				{
					LastShownForm = ZControllerFactory.Create(consumerControllerID).ShowEditForm(bO);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("f18eaf3c-9088-44d8-bb19-9db0b32cd8ba", "Cannot open corresponding operations job."));
				}
			}
		}

		IZForm LastShownForm;

		void OpenButton_Click(object sender, EventArgs e)
		{
			ShowOperationsForm(relatedJob);
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		#endregion
	}
}

