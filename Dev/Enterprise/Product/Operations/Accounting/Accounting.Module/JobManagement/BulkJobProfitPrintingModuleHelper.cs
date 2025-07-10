using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
#if DEBUG
	internal
#endif
	class BulkJobProfitPrintingModuleHelper : IBulkJobProfitPrintingModuleHelper
	{
		#region IBulkJobProfitPrintingModuleHelper Members

		IMenuItem IBulkJobProfitPrintingModuleHelper.GetMenuItem(JobProfitPrintingDelegate jobProfitPrintingDelegate)
		{
			var menuItem = new ZMenuItem(Constants.MenuNameConstants.PrintJobProfitDoc);
			menuItem.Click += delegate { jobProfitPrintingDelegate(); };
			return menuItem;
		}

		void IBulkJobProfitPrintingModuleHelper.PrintJobProfitDocument(BusinessObjectFactory factory, BusinessObject[] selectedJobs)
		{
			if (selectedJobs != null && selectedJobs.Any())
			{
				var jobsWithoutParent = new List<BusinessObject>();
				var jobsThatInactive = new List<BusinessObject>();
				var errorMessage = new ZStringBuilder();

				foreach (var selectedJob in selectedJobs)
				{
					var job = GetJobFromBusinessObject(factory, selectedJob);
					if (job != null)
					{
						if (!job.JobHasParent())
						{
							jobsWithoutParent.Add(selectedJob);
						}
						if (!job.JH_IsActive)
						{
							jobsThatInactive.Add(selectedJob);
						}
					}
				}

				if (jobsWithoutParent.Any())
				{
					errorMessage.Append(Res.GetString("fb7295f1-3b9d-46aa-ab8e-158a23fe5119", @"Job Profit Document will not be generated for following job(s) with missing/invalid parent: {0}.
You can use the filter 'Missing/Invalid Job Parent' in Job Management module to list all jobs without a valid parent.", string.Join(", ", jobsWithoutParent.Select(x => GetJobNumber(x)))));
					selectedJobs = selectedJobs.Except(jobsWithoutParent).ToArray();
				}

				if (jobsThatInactive.Any())
				{
					errorMessage.Append(jobsWithoutParent.Any() ? "\r\n\r\n" : "");
					errorMessage.Append(Res.GetString("5623D8E3-83DB-4A56-B047-69FEC2B6A84C", @"Job Profit Document will not be generated for following job(s) which is inactive: {0}.", string.Join(", ", jobsThatInactive.Select(x => GetJobNumber(x)))));
					selectedJobs = selectedJobs.Except(jobsThatInactive).ToArray();
				}

				if (!selectedJobs.Any() && !errorMessage.IsEmpty)
				{
					Globals.Message.ShowError(errorMessage.ToString());
				}
				else if (CheckSecurityCheckPoint(selectedJobs))
				{
					PrintJobProfitDocument(factory, selectedJobs, errorMessage.ToString());
				}
			}
			else
			{
				var message = Res.GetString("BF22450A-00FD-4925-B815-E9296C71BEB1", "Please select a Job before printing.");
				var caption = Res.GetString("0887BAF0-D385-457C-BB99-40E6A8CF0E3E", "Select a Job");
				Globals.Message.ShowInformation(message, caption);
			}
		}

		#endregion

		#region Implementation

		protected virtual void PrintJobProfitDocument(BusinessObjectFactory factory, BusinessObject[] selectedElements, string errorMessage = "")
		{
			if (selectedElements.Any())
			{
				var validJobsPrintedWithSeparator = string.Join(", ", selectedElements.Select(x => GetJobNumber(x)));
				var userQuestion = string.IsNullOrEmpty(errorMessage)
					? Res.GetString("08b458b7-9f12-42ed-bdd5-d9b8238bf1fd", "Are you sure you want to print Job Profit Document for following job(s): {0}", validJobsPrintedWithSeparator)
					: Res.GetString("9b36d2be-2a9b-414b-ad60-39ef5c461e73", @"{0}

Job Profit Document can be generated for following job(s): {1}.
Do you want to continue?", errorMessage, validJobsPrintedWithSeparator);

				if (Globals.Message.Show(userQuestion, Res.GetString("fb11e82b-677f-4af0-9162-fc149eb97321", "Job Profit Document Printing"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					var docPrinter = CreateJobDocumentPrinter(factory);
					if (ZFormModaliser.ShowDialogAndDispose(CreatePrintingForm(docPrinter)) == DialogResult.OK)
					{
						docPrinter.PrintJobProfitDocuments(factory, GetPrintItems(factory, docPrinter, selectedElements).ToArray());
					}
				}
			}
		}

		protected virtual ZString GetJobNumber(BusinessObject bizObj)
		{
			ZString result = ZString.Empty;
			JobHeader job = bizObj as JobHeader;
			if (job != null)
			{
				result = job.JH_JobNum;
			}
			else
			{
				IJobInvoicingPlugIn jobInvoicing = bizObj as IJobInvoicingPlugIn;
				if (jobInvoicing != null)
				{
					result = jobInvoicing.JobNumber;
				}
			}

			return result;
		}

		protected virtual JobProfitDocumentPrintingForm CreatePrintingForm(JobDocumentPrinter docPrinter)
		{
			return new JobProfitDocumentPrintingForm(docPrinter);
		}

		protected virtual JobDocumentPrinter CreateJobDocumentPrinter(BusinessObjectFactory factory)
		{
			return new JobDocumentPrinter(factory);
		}

		protected virtual List<JobDocumentPrintItem> GetPrintItems(BusinessObjectFactory factory, JobDocumentPrinter docPrinter, BusinessObject[] selectedElements)
		{
			List<JobDocumentPrintItem> jobPrinters = new List<JobDocumentPrintItem>();
			foreach (BusinessObject element in selectedElements)
			{
				Job job = GetJobFromBusinessObject(factory, element);
				if (job != null)
				{
					jobPrinters.Add(new JobDocumentPrintItem(docPrinter, job, factory));
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("8634a0ea-191c-4cf9-97c4-d2adf2274a8f", "There was an error preparing the Job Profit Document"));
				}
			}

			return jobPrinters;
		}

		Job GetJobFromBusinessObject(BusinessObjectFactory factory, BusinessObject bizObj)
		{
			Job job = bizObj as Job;
			if (job == null)
			{
				JobManagement jobManagement = bizObj as JobManagement;
				ZGuid jobPK = jobManagement != null ? jobManagement.PK : ZGuid.Empty;
				job = factory.Load<Job>(jobPK);
			}

			if (job == null)
			{
				IJobInvoicingPlugIn plugIn = bizObj as IJobInvoicingPlugIn;
				if (plugIn != null)
				{
					job = new Job.Loader(factory, plugIn).Load();
				}
			}

			return job;
		}

		bool CheckSecurityCheckPoint(BusinessObject[] selectedElements)
		{
			return BulkJobSecurityCheckHelper.CheckSecurityCheckPoint(selectedElements, bizos => GetSecurityCheckPoints(bizos));
		}

		SecurityCheckpoint[] GetSecurityCheckPoints(BusinessObject[] selectedElements)
		{
			return GetSecurityCheckPointsCore(selectedElements);
		}

		protected virtual SecurityCheckpoint[] GetSecurityCheckPointsCore(BusinessObject[] selectedElements)
		{
			return BulkJobSecurityCheckHelper.GetSecurityCheckPoints(SecurityCore.BulkPrintJobProfitDoc, selectedElements);
		}
		#endregion
	}
}
