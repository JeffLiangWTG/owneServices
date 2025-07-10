using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.GUI.JobManagement;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Integration.MasterFiles;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Module for JobManagement.
	/// </summary>
	public partial class JobManagementModule : JobManagementModuleBase
#if DEBUG
, IBulkPostingModuleInternalsForTesting
#endif
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.JobManagement; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new JobManagementFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new JobManagementFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.JobManagement);
		}

		protected static string PrintItemMenuText
		{
			get { return Res.GetString("e1c332bb-41ab-4d6c-9c49-57a5969164c5", "Print"); }
		}
		protected static string DisplayItemMenuText
		{
			get { return Res.GetString("a2a750ce-f64f-47cf-bc35-f2d72d697f79", "View"); }
		}
		protected static MultilingualString ViewOperationsDetailsItemMenuText
		{
			get { return ResString.GetMultilingualString("76cf9d36-056a-42f6-a4bf-585985730c87", "View Operations Details"); }
		}
		protected static string CloseSingleJobItemMenuText
		{
			get { return Res.GetString("55bdfcab-ff5a-480d-b873-4a0515bdafed", "Close Job"); }
		}
		protected static MultilingualString CloseAllJobItemMenuText
		{
			get { return ResString.GetMultilingualString("c5660020-daef-4aa6-a24d-30974e6d04c0", "Close Job(s)"); }
		}
		protected static MultilingualString MarkJobHeaderAsInactiveItemMenuText
		{
			get { return ResString.GetMultilingualString("1A99E820-DCA2-471D-A63F-0DDD82E2D65D", "Mark Job Header as Inactive"); }
		}
		protected static MultilingualString ReOpenJobItemMenuText
		{
			get { return ResString.GetMultilingualString("85e3ba64-1286-4bf8-8399-ebc4b2f702cd", "Re-Open Job"); }
		}
		protected static string ViewProfitLossMenuText
		{
			get { return Res.GetString("ebf4a43f-fe79-418a-9529-f07c48604ee9", "View Job Profit/Loss"); }
		}
		protected static MultilingualString BulkJobCloseMenuText
		{
			get { return ResString.GetMultilingualString("1aec4c60-bfd2-4db9-a2d3-3e1cae7ecd24", "Bulk Job Close"); }
		}
		protected static MultilingualString DiagnoseAutoJobClosureMenuText
		{
			get { return ResString.GetMultilingualString("448b3fce-66f5-435c-af97-4cd0c94aacf9", "Diagnose Automatic Job Closure"); }
		}

		internal protected static string MissingJobMessage => Res.GetString("d1b44fd4-dbd1-43cd-9728-bdba8e682bd1", "Error loading job(s), the following job(s) might have been deleted by another user. ");
		internal protected static string MissingDeactivatedJobMessage => Res.GetString("4FF12191-5B5A-46D2-A6BE-728E127D023A", "Error loading job(s), the following job(s) might have been marked as inactive by another user. ");
		internal protected static string UserFriendlyConcurrencyMessage => Res.GetString("1f80f73f-c208-4ab0-98c7-2d3823627acf", "While you were working, another user has modified the following job(s). Please refresh the grid and try again. ");

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItems = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItems.Remove(DataTransferMenuItem);

			menuItems.Add(new ZMenuItem(ViewOperationsDetailsItemMenuText, new EventHandler(ViewOperationsDetails)));
			menuItems.Add(new ZMenuItem(CloseAllJobItemMenuText, new EventHandler(CloseMultipleJobEventHandler)));
			menuItems.Add(new ZMenuItem(ReOpenJobItemMenuText, new EventHandler(ReOpenJobEventHandler)));
			menuItems.Add(new ZMenuItem(MarkJobHeaderAsInactiveItemMenuText, new EventHandler(MarkJobHeaderAsInactiveEventHandler)));

			menuItems.Add(DataTransferMenuItem);

			KMenuItem postMenuItem = BulkPostingHelper.GetPostMenuItem(new PostTransactionsDelegate(PostTransactions)) as KMenuItem;
			menuItems.Add(postMenuItem);

			menuItems.Add(BulkJobProfitPrintingHelper.GetMenuItem(PrintJobProfitDocument) as MenuItem);

			menuItems.Add(new ZMenuItem(BulkJobCloseMenuText, new EventHandler(BulkJobCloseEventHandler)));
			menuItems.Add(new ZMenuItem(DiagnoseAutoJobClosureMenuText, new EventHandler(DiagnoseAutoJobClosureEventHandler)));

			return menuItems.ToArray();
		}

		#region Posting

		void PostTransactions(JobInvoicingPostingOption postingOption)
		{
			Job[] selectedJobs = new Job[Grid.SelectedElements.Length];
			for (int i = 0; i < Grid.SelectedElements.Length; i++)
			{
				selectedJobs[i] = Factory.Load<Job>(((IIdentified)Grid.SelectedElements[i]).Identifier);
			}

#if DEBUG
			if (((IBulkPostingModuleInternalsForTesting)this).DontDoActualPosting)
			{
				fLastUsedPostingOptionForTest = postingOption;
			}
			else
#endif
			{
				BulkPostingHelper.PostTransactions(postingOption, selectedJobs);
			}
		}

		BulkPostingModuleHelper BulkPostingHelper
		{
			get
			{
				if (bulkPostingHelper == null)
				{
					bulkPostingHelper = new BulkPostingModuleHelper();
					bulkPostingHelper.Initialize(Res.GetString("Accounting|JobManagement|JobBusinessObjectName", "Job"), false);
				}

				return bulkPostingHelper;
			}
		}
		BulkPostingModuleHelper bulkPostingHelper;

		#endregion

		#region IBulkPostingModuleInternalsForTesting Members
#if DEBUG

		JobInvoicingPostingOption IBulkPostingModuleInternalsForTesting.LastUsedPostingOptionForTest
		{
			get { return fLastUsedPostingOptionForTest; }
		}

		JobInvoicingPostingOption fLastUsedPostingOptionForTest;

		IMenuItem IBulkPostingModuleInternalsForTesting.PostMenuItem
		{
			get
			{
				MenuItem menuItem = GetNewActionMenuItems().FindByText("&Post");
				IMenuItem zMenuItem = menuItem as IMenuItem;

				if (menuItem != null && zMenuItem == null)
				{
					throw new ApplicationException("PostMenuItem shoud has type ZMenuItem.");
				}

				return zMenuItem;
			}
		}

		bool IBulkPostingModuleInternalsForTesting.DontDoActualPosting
		{
			get;
			set;
		}

#endif
		#endregion

		#region PrintJobProfitDocument

		void PrintJobProfitDocument()
		{
			BulkJobProfitPrintingHelper.PrintJobProfitDocument(Factory, Grid.SelectedElements);
		}

		IBulkJobProfitPrintingModuleHelper BulkJobProfitPrintingHelper
		{
			get
			{
				if (bulkJobProfitPrintingHelper == null)
				{
					bulkJobProfitPrintingHelper = ObjectFactory.Get<IBulkJobProfitPrintingModuleHelper>();
				}

				return bulkJobProfitPrintingHelper;
			}
		}
		IBulkJobProfitPrintingModuleHelper bulkJobProfitPrintingHelper;

		#endregion

		#region Job Menu Related Methods

		protected virtual JobManagement SelectedJob
		{
			get
			{
				return CurrentBusinessObjectInGrid as JobManagement;
			}
		}

		IZForm CurrentEditForm;

		protected void ViewOperationsDetails(object sender, EventArgs e)
		{
			if (SelectedJob != null)
			{
				GenericJob genericJob = SelectedJob.LoadGenericJob<GenericJob>();
				if (genericJob != null)
				{
					BusinessObject bO = genericJob.Consumer as BusinessObject;
					ControllerID consumerControllerID = genericJob.GetConsumerController();
					if (consumerControllerID != null)
					{
						CurrentEditForm = ZControllerFactory.Create(consumerControllerID).ShowEditForm(bO);
					}
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("df943ada-8357-4bcc-964f-98b85e99daae", "Cannot open corresponding operations job."));
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("bfcb5008-6f89-4743-a7a2-452efd0be453", "No Jobs are selected"));
			}
		}

		protected void MarkJobHeaderAsInactiveEventHandler(object sender, EventArgs e)
		{
			if (!Env.Security.DeleteInvoicingJobHeader.IsAllowed)
			{
				Env.Security.DeleteInvoicingJobHeader.ShowError();
			}
			else
			{
				if (SelectedJobs != null && SelectedJobs.Length > 0)
				{
					var jobsInactive = new List<Job>();
					var jobsCannotBeMarked = new List<Job>();
					var jobsConfirmMark = new List<Job>();
					var reasonsNotAbleToDeactivate = new List<string>();

					foreach (var selectedJob in SelectedJobs)
					{
						var factoryForDeactivatingJob = new BusinessObjectFactory();
						var jobToDeactivate = factoryForDeactivatingJob.Load<Job>(selectedJob.PK);

						if (jobToDeactivate == null || !jobToDeactivate.JH_IsActive)
						{
							jobsInactive.Add(jobToDeactivate);
						}
						else
						{
							var reasonNotAbleToDeactivate = jobToDeactivate.CheckIfCanDeactiveJobInCurrentCompany(jobToDeactivate.JH_JobNum);
							if (!reasonNotAbleToDeactivate.IsEmpty)
							{
								reasonsNotAbleToDeactivate.Add(reasonNotAbleToDeactivate);
							}
							else
							{
								var consumer = GetConsumer((JobManagement)selectedJob);
								if (!InvoicingPluginToFreight.IsInvoiceDeletionAllowedByParent(consumer))
								{
									jobsCannotBeMarked.Add(jobToDeactivate);
								}
								else
								{
									jobsConfirmMark.Add(jobToDeactivate);
								}
							}
						}
					}
					DeactiveJobsAndShowMessages(jobsInactive, jobsCannotBeMarked, jobsConfirmMark, reasonsNotAbleToDeactivate);
				}
			}
		}

		void DeactiveJobsAndShowMessages(List<Job> jobsInactive, List<Job> jobsCannotBeMarked, List<Job> jobsConfirmMark, List<string> reasonsNotAbleToDeactivate)
		{
			var confirmMessage = new ZStringBuilder();
			if (jobsInactive.Any())
			{
				confirmMessage.AppendLine(MissingDeactivatedJobMessage + string.Join(", ", jobsInactive.Select(x => x.JH_JobNum))).AppendLine();
			}
			if (reasonsNotAbleToDeactivate.Any())
			{
				foreach (var reasonNotAbleToDeactivate in reasonsNotAbleToDeactivate)
				{
					confirmMessage.AppendLine(reasonNotAbleToDeactivate).AppendLine();
				}
			}
			if (jobsCannotBeMarked.Any())
			{
				confirmMessage.AppendLine(Res.GetString("D39EB9C8-E818-48C8-A443-73ABD23F6670", "The following invoice(s) cannot be marked as inactive. ") + string.Join(", ", jobsCannotBeMarked.Select(x => x.JH_JobNum))).AppendLine();
			}
			if (jobsConfirmMark.Any())
			{
				confirmMessage.Append(Res.GetString("7f6a5f70-b4c4-4606-b891-b138eb651b67", "Are you sure you want to mark following Invoicing Job Header(s) as inactive? {0}", string.Join(", ", jobsConfirmMark.Select(x => x.JH_JobNum))));
				var caption = Res.GetString("481536BA-9B5F-4DE0-979B-4928BCA02524", "Mark Invoicing Job Header(s) as Inactive");
				var result = Globals.Message.Show(confirmMessage.ToString(), caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (result == DialogResult.Yes)
				{
					var jobsMarkedSuccessfully = new List<Job>();
					var jobsMarkedFailed = new List<Job>();
					var markMessage = new ZStringBuilder();
					var deactivateJobCaption = Res.GetString("A33C9B50-3371-49FE-B332-4358C3C7C430", "Mark Job as Inactive");
					foreach (var jobConfirmMarkItem in jobsConfirmMark)
					{
						try
						{
							DeactiveJobCore(jobConfirmMarkItem);
							jobsMarkedSuccessfully.Add(jobConfirmMarkItem);
						}
						catch (ZSaveConcurrencyException)
						{
							jobsMarkedFailed.Add(jobConfirmMarkItem);
						}
					}
					if (jobsMarkedFailed.Any())
					{
						markMessage.AppendLine(UserFriendlyConcurrencyMessage + string.Join(", ", jobsMarkedFailed.Select(x => x.JH_JobNum))).AppendLine();
					}
					if (jobsMarkedSuccessfully.Any())
					{
						markMessage.Append(Res.GetString("28C8F07E-4EC3-43FE-BBAB-11B4C69431A1", "The following job(s) has been marked as inactive. ") + string.Join(", ", jobsMarkedSuccessfully.Select(x => x.JH_JobNum)));
						Globals.Message.ShowInformation(markMessage.ToString(), deactivateJobCaption);
					}
					else
					{
						Globals.Message.ShowError(markMessage.ToString(), deactivateJobCaption);
					}
				}
			}
			else
			{
				var caption = Res.GetString("8022C292-6B2F-41E8-A0FF-9138753AC5B4", "Mark Job as Inactive");
				Globals.Message.ShowInformation(confirmMessage.ToString(), caption);
			}
		}

		protected virtual void DeactiveJobCore(Job jobToDeactivate)
		{
			jobToDeactivate.MarkAsInactive();
			jobToDeactivate.Factory.Save();
		}

		protected virtual void DeleteJobCore(Job jobToDelete)
		{
			jobToDelete.Delete();
			jobToDelete.Factory.Save();
		}

		protected void ReOpenJobEventHandler(object sender, EventArgs e)
		{
			if (SelectedJob != null)
			{
				var factoryForJobReopening = new BusinessObjectFactory();
				var jobToReOpen = factoryForJobReopening.Load<Job>(SelectedJob.PK);
				var result = JobReopenSecurityCheckHelper.CanReopenJobWithSecurity_NonInteractiveSecurityCheck(Factory, jobToReOpen);
				if (!result.CheckResult)
				{
					result.SecurityCheckpoint.ShowError();
				}
				else
				{
					var caption = Res.GetString("3dd9ea90-8a52-4e2c-9c42-d17c09732aa3", "Reopen job");

					if (jobToReOpen == null)
					{
						Globals.Message.ShowInformation(MissingJobMessage, caption);
					}
					else if (!jobToReOpen.JH_IsActive)
					{
						Globals.Message.ShowError(Res.GetString("7FFE4134-1B95-4595-B77B-8E8527A8C18B", "The following job is inactive. Please activate it before closing: \r\n{0}", jobToReOpen.JH_JobNum), caption);
					}
					else if (jobToReOpen.JH_Status != JobHeaderStatus.Closed.Code)
					{
						Globals.Message.ShowInformation(Res.GetString("57c482ec-66b2-4348-a19f-bed445124f59", "This job is already open"), caption);
					}
					else
					{
						var message = Res.GetString("23702b6c-0729-4481-be70-0e21fe21815c", "Are you sure you want to reopen this job?");
						var messageBoxResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
						if (messageBoxResult == DialogResult.Yes)
						{
							try
							{
								ReopenJobCore(jobToReOpen);
								Globals.Message.ShowInformation(Res.GetString("16759180-8479-4233-b58a-aa0797f790af", "Job was successfully reopened and an email has been sent to the appropriate notification group"), caption);
							}
							catch (ZSaveConcurrencyException)
							{
								Globals.Message.ShowError(UserFriendlyConcurrencyMessage, caption);
							}
						}
					}
				}
			}
		}

		protected virtual void ReopenJobCore(Job jobToReOpen)
		{
			jobToReOpen.ReOpen();
			jobToReOpen.Factory.Save();
		}

		protected void CloseMultipleJobEventHandler(object sender, EventArgs e)
		{
			if (SelectedJobs.Length > 0)
			{
				CloseJobs(SelectedJobs);
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("6e8501d2-7005-40af-afc2-198f8ad77228", "No Jobs are selected to close."), Res.GetString("55bdfcab-ff5a-480d-b873-4a0515bdafed", "Close Job"));
			}
		}

		protected void BulkJobCloseEventHandler(object sender, EventArgs e)
		{
			var security = Env.Security.CloseJobsInBulk;
			if (!security.IsAllowed)
			{
				security.ShowError();
			}
			else
			{
				var filterObject = ((JobManagementFilterBusinessObject)FilterBusinessObject);
				var bulkJobCloseForm = ZControllerFactory.Create(ControllerIDs.BulkJobClose).ShowFormForNewEntity(new BulkJobCloseProcessor((q) => filterObject.GetAdditionalFilterForBulkJobClosure(q)));

#if DEBUG
				bulkJobCloseForm_TestOnly = bulkJobCloseForm;
#endif

			}
		}

		protected void DiagnoseAutoJobClosureEventHandler(object sender, EventArgs e)
		{
			if (SelectedJobs.Length > 0)
			{
				var monitor = new JCSDiagnosisMonitor(SelectedJobs.Select(bo => bo.PK));
				ZFormModaliser.ShowDialogAndDispose(new AutoJobClosureDiagnosisForm(monitor), Grid.FindForm());
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("58683df5-a74e-4011-ac65-1e7b4aa866b3", "No job is selected for diagnosis."), Res.GetString("048b1f47-fa5b-4f14-9e7d-9cc305b2bd15", "Auto Job Closure"));
			}
		}

		protected void CloseJobs(BusinessObject[] jobsToCloseFromModuleFactory)
		{
			var jobPKs = jobsToCloseFromModuleFactory.Select(x => x.PK);
			var factoryForJobClosure = new BusinessObjectFactory();
			var reloadedJobs = factoryForJobClosure.Load<Job>(new ZQuery(JobHeaderSchema.PK, jobPKs)).OrderBy(x => x.JH_JobNum);

			foreach (Job job in reloadedJobs)
			{
				job.InitializeParentFromGenericJobWithoutSettingDefaults();
			}

			var security = reloadedJobs.Count() == 1 ? Env.Security.CloseSingleJob : Env.Security.CloseMultipleJobs;

			if (!reloadedJobs.Any())
			{
				Globals.Message.ShowError(Res.GetString("bdd2a351-0d42-4689-b3e5-edb23dac4bf3", "Please select some jobs to close."));
			}
			else if (!security.IsAllowed)
			{
				security.ShowError();
			}
			else
			{
				CloseJobsCore(reloadedJobs);
			}
		}

#if DEBUG
		protected BusinessObjectFactory testFactory;
#endif

		void CloseJobsCore(IEnumerable<Job> jobsToClose)
		{
			var message = Res.GetString("988aec8e-8f03-4a2d-ba2e-0330f5d5239d", "You are about to close the following jobs and reverse all related WIPs and ACRs:\r\n\r\n{0}\r\n\r\nAre you sure you want to continue?", ManualJobClosureHelper.GetJobNumbers(jobsToClose));
			var result = Globals.Message.Show(message, Res.GetString("578ba191-c124-4f91-a5ea-507cdb30b051", "Close jobs"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result == DialogResult.Yes)
			{
				var jobsWithUnpostedConsolCosts = jobsToClose.Where(x => x.UnpostedConsolCostsLinkedToThisJob.Any(y => !y.CanDelete));
				var msgCanNotDeltedUnpostedConsolCosts = Res.GetString("3AB3B4B9-4378-48C2-B9C4-4CC5CA6A57F5", "The following job(s) contains unposted consol costs which can't be deleted.");

				var jobPKs = jobsToClose.Except(jobsWithUnpostedConsolCosts).Select(x => x.PK);
				var factory = new BusinessObjectFactory();
#if DEBUG
				factory = testFactory ?? factory;
#endif
				var reloadedJobs = factory.Load<Job>(new ZQuery(JobHeaderSchema.PK, jobPKs)).OrderBy(x => x.JH_JobNum);

				var jobCloseHelper = ManualJobClosureHelper.CreateHelper(reloadedJobs);
				var messageForDiscardedJobs = jobCloseHelper.GetAllErrorMessages();
				var msgCanNotUpdate = Res.GetString("3A93A407-17FA-43FD-AB01-DC8FDDDED8F7", "The following job(s) contains posted disbursement clearing balance and can only be closed via the Auto Job Closure process.");
				jobCloseHelper.DeleteConsolCostsLinkedToUnpostedApportionedChargesIfAny();

				var updatedJobs = new List<Job>();
				var canNotUpdatedJobs = new List<Job>();

				foreach (Job job in jobCloseHelper.JobsThatCanBeClosed)
				{
					if (job.GetShouldJobBeClosedByDsbBatch())
					{
						canNotUpdatedJobs.Add(job);
						continue;
					}

					job.Close(new Job.ErrorMessageHandler(Job_OnCloseJobError), new EventHandler<UserQueryEventArgs>(Job_OnCloseJobYesNoQuestion));
					if (job.IsClosed)
					{
						updatedJobs.Add(job);
					}
				}
				if (updatedJobs.Count > 0)
				{
					try
					{
						factory.Save();

						var closeMsgBuilder = new ZStringBuilder(Res.GetString("9dd9482c-d171-439d-8f6b-47a925007b76", "The following jobs were closed successfully:")).AppendLine().Append(ManualJobClosureHelper.GetJobNumbers(updatedJobs));
						if (!string.IsNullOrEmpty(messageForDiscardedJobs))
						{
							closeMsgBuilder.AppendLine().AppendLine().Append(messageForDiscardedJobs);
						}

						if (canNotUpdatedJobs.Count > 0)
						{
							closeMsgBuilder.AppendLine().AppendLine().AppendLine(msgCanNotUpdate).Append(ManualJobClosureHelper.GetJobNumbers(canNotUpdatedJobs));
						}

						if (jobsWithUnpostedConsolCosts.Any())
						{
							closeMsgBuilder.AppendLine().AppendLine().AppendLine(msgCanNotDeltedUnpostedConsolCosts).Append(ManualJobClosureHelper.GetJobNumbers(jobsWithUnpostedConsolCosts));
						}

						Globals.Message.ShowInformation(closeMsgBuilder.ToString(), Res.GetString("578ba191-c124-4f91-a5ea-507cdb30b051", "Close jobs"));
					}
					catch (ZSaveConcurrencyException)
					{
						Globals.Message.ShowWarning(Res.GetString("6ab8cbe2-a94f-4c84-b55c-eb5965db2761", "Please try to close these jobs again.  The selected jobs have not been closed.\r\nOne or more of these jobs has been modified by another user. The other user's changes have been merged with yours."));
					}
					catch (ZSaveException ex) when (ex.IndexNameIfUniqueIndexViolation == JobHeaderSchema.Constants.Indexes.NR_UX__JH_JobNum_JH_GC)
					{
						var helper = ObjectFactory.Get<IJobNumUniqueIndexErrorHelper>();
						var errorMsg = helper.GetErrorMessage(ex);
						if (!string.IsNullOrEmpty(errorMsg))
						{
							Globals.Message.ShowError(errorMsg);
						}
						else
						{
							throw;
						}
					}
					catch (OnSavingCriticalCheckException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
				else
				{
					var msgToShow = new ZStringBuilder();
					if (canNotUpdatedJobs.Count > 0)
					{
						msgToShow.AppendLine(msgCanNotUpdate).AppendLine(ManualJobClosureHelper.GetJobNumbers(canNotUpdatedJobs)).AppendLine();
					}

					if (!string.IsNullOrEmpty(messageForDiscardedJobs))
					{
						msgToShow.Append(messageForDiscardedJobs);
					}

					if (jobsWithUnpostedConsolCosts.Any())
					{
						msgToShow.AppendLine(msgCanNotDeltedUnpostedConsolCosts).Append(ManualJobClosureHelper.GetJobNumbers(jobsWithUnpostedConsolCosts));
					}

					if (!msgToShow.IsEmpty)
					{
						Globals.Message.ShowError(msgToShow.ToString(), Res.GetString("565b0ee9-9708-4e75-b731-1b9d181c49ba", "Close jobs"));
					}
				}
			}
		}

		void Job_OnCloseJobError(Job job, string errorMessage)
		{
			Globals.Message.ShowError(errorMessage);
		}

		void Job_OnCloseJobYesNoQuestion(object sender, UserQueryEventArgs e)
		{
			Job job = sender as Job;
			DialogResult result = Globals.Message.Show(e.QueryMessage, Res.GetString("91D8FC2E-7481-479F-AE99-620337712164", "Job {0}", job.JH_JobNum), MessageBoxButtons.YesNo, e.Response ? DialogResult.Yes : DialogResult.No);
			e.Response = result == DialogResult.Yes;
		}

		protected virtual BusinessObject[] SelectedJobs
		{
			get
			{
				return Grid.SelectedElements;
			}
		}

#endregion

		public override ToolBarButton[] ToolBarButtons
		{
			get
			{
				List<ToolBarButton> buttons = new List<ToolBarButton>(base.ToolBarButtons);
				buttons.Remove(buttons.FindByText(ViewOperationsDetailsItemMenuText));
				return buttons.ToArray();
			}
		}

		#region GetConsumer

		IJobInvoicingPlugIn GetConsumer(JobManagement selectedJob)
		{
			var genericJob = selectedJob.LoadGenericJob<GenericJob>();
			return genericJob != null ? genericJob.Consumer : null;
		}

		#endregion

		#region TestCase
#if DEBUG

		internal IZForm bulkJobCloseForm_TestOnly;
#endif
#endregion
	}
}
