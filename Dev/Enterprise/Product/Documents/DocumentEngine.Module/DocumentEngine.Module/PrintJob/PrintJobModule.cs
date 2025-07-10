using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Module
{
	[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
	public class PrintJobModule : ZFilterGridModule, IPrintJobView
	{
		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.PrintJob; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.PrintJob);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new PrintJobFilterControl(GridCollection, (PrintJobFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmPrintJobCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new PrintJobFilterBusinessObject();
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.PrintJobs; }
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> collection = new List<MenuItem>(base.GetNewStandardMenuItems());
			collection.Remove(ViewMenuItem);
			return collection.ToArray();
		}

		protected override SortInfo DefaultSortOrder
		{
			get { return new SortInfo(StmPrintJob.Schema.SP_RunDateTime, System.ComponentModel.ListSortDirection.Ascending); }
		}

		#region Disallow New/Edit

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;

		#endregion

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			IPrintJobManager manager = ObjectFactory.Get<IPrintJobManager>();

			PrintJobDeleteCommand cmd = new PrintJobDeleteCommand(this, manager);
			cmd.Execute();

			return null;
		}

		#region IPrintJobView Members

		StmPrintJob[] IPrintJobView.GetSelectedJobs()
		{
			List<BusinessObject> objects = new List<BusinessObject>(GetSelectedBusinessObjects());
			return objects.ConvertAll(input => { return (StmPrintJob)input; }).ToArray();
		}

		bool IPrintJobView.AskUserConfirmation(string message)
		{
			ZMessageBox msg = new ZMessageBox(message, Res.GetString("96762b89-7b7e-4d41-85ea-a49fd4c152e3", "Confirmation"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			return ZFormModaliser.ShowDialogAndDispose(msg) == DialogResult.Yes;
		}

		#endregion

		#region Reset Status

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems())
			{
				new ZMenuItem(MenuItemResetStatusToQueued, ResetStatusToQUE),
				new ZMenuItem(MenuItemResetStatusToFailed, ResetStatusToFAL)
			};

			return result.ToArray();
		}

		protected static MultilingualString MenuItemResetStatusToQueued => ResString.GetMultilingualString("5F0ACF7E-7858-4463-BEA7-63AAA98E53E2", "&Reset Status To QUE");

		protected static MultilingualString MenuItemResetStatusToFailed => ResString.GetMultilingualString("B64D76AE-49BC-4264-A51F-CC4172AF85A4", "&Reset Status To FAL");

		void ResetStatusToFAL(object sender, EventArgs e)
		{
			if (ResetStatusTo(PrintJobStatus.FAL) > 0)
			{
				PerformSearch();
			}
		}

		void ResetStatusToQUE(object sender, EventArgs e)
		{
			if (ResetStatusTo(PrintJobStatus.QUE) > 0)
			{
				PerformSearch();
			}
		}

		int ResetStatusTo(PrintJobStatus newStatus)
		{
			try
			{
				var itemsAffected = 0;

				var selectedJobs = ((IPrintJobView)this).GetSelectedJobs();
				if (!selectedJobs.Any())
				{
					ShowError(Res.GetString("D1A168F1-5040-4392-9971-91465252E796", "Please select one or more Jobs to reset the Status."));
					return 0;
				}

				var selectedCount = selectedJobs.Length;
				var factory = new BusinessObjectFactory();
				var query = new ZQuery();
				var selectedPks = selectedJobs.Select(j => j.PK);
				query.AddToFilter(StmPrintJobSchema.PK, SQLComparisonOperator.Equal, selectedPks);
				var jobs = factory.Load<StmPrintJob>(query);
				jobs.ForEach(j =>
				{
					if (j.SP_Status != newStatus.ToString())
					{
						j.SP_Status = newStatus.ToString();
						if (newStatus == PrintJobStatus.QUE)
						{
							j.SP_RetryAttempts = 0;
						}
						itemsAffected++;
					}
				});
				factory.Save();

				if (itemsAffected > 0)
				{
					ShowInformation(Res.GetString("0C24E29A-B420-416F-9793-844F69B0CA6A", "Reset Status to {0} for {1} Job(s) from {2} selected.", newStatus, itemsAffected, selectedCount));
					return itemsAffected;
				}
				ShowInformation(Res.GetString("CE41FF95-EEFD-455B-B520-C1E1E1241224", "No Jobs changed Status to {0} from {1} selected.", newStatus, selectedCount));
			}
			catch (ZSaveException e)
			{
				ErrorReporter.ReportOnce("Exception-Resetting-Job-Status", "Exception Resetting Job Status", e);
			}

			return 0;
		}

		void ShowInformation(string message)
		{
			Globals.Message.ShowInformation(message);
		}

		void ShowError(string message)
		{
			Globals.Message.ShowError(message);
		}

		#endregion
	}
}
