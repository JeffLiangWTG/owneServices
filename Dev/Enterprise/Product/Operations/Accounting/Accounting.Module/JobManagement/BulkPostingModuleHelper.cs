using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class BulkPostingModuleHelper : IBulkPostingModuleHelper
	{
		public void Initialize(ZString businessObjectName, bool isConsolPosting)
		{
			BusinessObjectName = businessObjectName;
			this.IsConsolPosting = isConsolPosting;
		}

		public void PostTransactions(JobInvoicingPostingOption postingOption, IJobCostingPlugIn[] selectedConsols)
		{
			if (selectedConsols != null && selectedConsols.Length > 0)
			{
				if (CheckSecurityCheckPoint(postingOption, Array.Empty<BusinessObject>()))
				{
					PostTransactionsCore(postingOption, selectedConsols);
				}
			}
			else
			{
				Globals.Message.ShowInformation(message, caption);
			}
		}

		public void PostTransactions(JobInvoicingPostingOption postingOption, BusinessObject[] selectedElements)
		{
			if (selectedElements != null && selectedElements.Length > 0)
			{
				if (CheckSecurityCheckPoint(postingOption, selectedElements))
				{
					PostTransactionsCore(postingOption, selectedElements);
				}
			}
			else
			{
				Globals.Message.ShowInformation(message, caption);
			}
		}

		readonly ZString message = Res.GetString("AB22450A-F0FD-E925-D815-C9296C76BEB3", "Please select a Job before posting.");
		readonly ZString caption = Res.GetString("A887BAF0-E385-D57C-CC99-53E6A8CF0E30", "Select a Job");

		public IMenuItem GetPostMenuItem(PostTransactionsDelegate modulePostTransactionsDelegate, params JobInvoicingPostingOption[] optionsToCreateMenuItemsFor)
		{
			var postMenuItem = new ZMenuItem(ResString.GetMultilingualString("MenuItem.Post", "&Post"));
			const string menuLine = "-";

			Func<JobInvoicingPostingOption, bool> addMenuItemForPostingOptionIfRequired = postingOption =>
				{
					bool isMenuItemAdded = false;
					if (optionsToCreateMenuItemsFor == null || optionsToCreateMenuItemsFor.Length == 0 || optionsToCreateMenuItemsFor.Contains(postingOption))
					{
						MenuItem menuItem = new ZMenuItem(GetMenuNameForPostingOption(postingOption));
						menuItem.Click += delegate { modulePostTransactionsDelegate(postingOption); };
						postMenuItem.MenuItems.Add(menuItem);
						isMenuItemAdded = true;
					}

					return isMenuItemAdded;
				};

			var defaultARInvoiceDateMenuCaption = ARDefaultInvoiceAndPostDateCalculator.GetDefaultDateMenuCaption();
			if (defaultARInvoiceDateMenuCaption != null)
			{
				postMenuItem.MenuItems.Add(new ZMenuItem(defaultARInvoiceDateMenuCaption));
			}

			if (IsConsolPosting)
			{
				var allOptionsToCreateMenuItemsFor = new[] { JobInvoicingPostingOption.Agent, JobInvoicingPostingOption.Costs, JobInvoicingPostingOption.ConsolCosts, JobInvoicingPostingOption.All };
				foreach (var currentOption in allOptionsToCreateMenuItemsFor)
				{
					addMenuItemForPostingOptionIfRequired(currentOption);
				}
			}
			else
			{
				bool isPostAllAdded = addMenuItemForPostingOptionIfRequired(JobInvoicingPostingOption.All);
				if (isPostAllAdded)
				{
					postMenuItem.MenuItems.Add(new ZMenuItem(menuLine));
				}

				bool isAnyInBetweenMenuItemsAdded = false;
				var allConsolOptionsToCreateMenuItemsFor = new[] { JobInvoicingPostingOption.LocalClient, JobInvoicingPostingOption.Agent,
					JobInvoicingPostingOption.Revenue, JobInvoicingPostingOption.AllSisterCompanyCharges, JobInvoicingPostingOption.LocalSisterCompanyChargesOnly, JobInvoicingPostingOption.Disbursement };
				foreach (var currentOption in allConsolOptionsToCreateMenuItemsFor)
				{
					if (addMenuItemForPostingOptionIfRequired(currentOption))
					{
						isAnyInBetweenMenuItemsAdded = true;
					}
				}

				if (isAnyInBetweenMenuItemsAdded)
				{
					postMenuItem.MenuItems.Add(new ZMenuItem(menuLine));
				}

				bool isPostCostAdded = addMenuItemForPostingOptionIfRequired(JobInvoicingPostingOption.Costs);
				if (!isPostCostAdded && postMenuItem.MenuItems.Count > 0)
				{
					postMenuItem.MenuItems.RemoveAt(postMenuItem.MenuItems.Count - 1);
				}
			}

			return postMenuItem;
		}

		#region Implementation

		void PostTransactionsCore(JobInvoicingPostingOption postingOption, IJobCostingPlugIn[] selectedElements)
		{
			if (IsConsolPosting && selectedElements.Length > 0)
			{
				SortJobArrayByJobNumber(selectedElements);
				IConsolBatchPostingDirector director = new ConsolBatchPostingDirectorCreator().GetNewConsolBatchPostingDirector();
				director.RunBatchPosting(postingOption, selectedElements);
			}
		}

		void PostTransactionsCore(JobInvoicingPostingOption postingOption, BusinessObject[] selectedElements)
		{
			if (selectedElements.Length > 0)
			{
				IJobBatchPostingDirector postingDirector = new JobBatchPostingDirectorCreator().GetNewJobBatchPostingDirector();
				if (selectedElements[0] is IJobInvoicingPlugIn)
				{
					IJobInvoicingPlugIn[] selectedJobs = new IJobInvoicingPlugIn[selectedElements.Length];

					for (int i = 0; i < selectedElements.Length; i++)
					{
						selectedJobs[i] = (IJobInvoicingPlugIn)selectedElements[i];
					}
					SortJobArrayByJobNumber(selectedJobs);
					postingDirector.RunBatchPosting(postingOption, selectedJobs, BusinessObjectName);
				}
				else
				{
					postingDirector.RunBatchPosting(postingOption, selectedElements, BusinessObjectName);
				}
			}
		}

		internal void SortJobArrayByJobNumber(object[] arryOfJobs)
		{
			JobNumberComparer comparer = new JobNumberComparer();
			Array.Sort(arryOfJobs, comparer);
		}

		internal class JobNumberComparer : IComparer
		{
			public int Compare(object firstJob, object secondJob)
			{
				if (!(firstJob is IJobNumber))
				{
					throw new InvalidOperationException("JobNumberComparer only compares IJobNumber, but FirstJob's type is: " + firstJob.GetType().FullName);
				}
				else if (!(secondJob is IJobNumber))
				{
					throw new InvalidOperationException("JobNumberComparer only compares IJobNumber, but SecondJob's type is: " + secondJob.GetType().FullName);
				}
				else
				{
					if (firstJob == null)
					{
						throw new InvalidOperationException("JobNumberComparer can't compare due to FirstJob is null");
					}
					else if (secondJob == null)
					{
						throw new InvalidOperationException("JobNumberComparer can't compare due to SecondJob is null");
					}
					else
					{
						return String.Compare(((IJobNumber)firstJob).JobNumber, ((IJobNumber)secondJob).JobNumber);
					}
				}
			}
		}

		bool CheckSecurityCheckPoint(JobInvoicingPostingOption postingOption, BusinessObject[] selectedElements)
		{
			return BulkJobSecurityCheckHelper.CheckSecurityCheckPoint(selectedElements, bizos => GetSecurityCheckPoints(postingOption, bizos));
		}

		SecurityCheckpoint[] GetSecurityCheckPoints(JobInvoicingPostingOption postingOption, BusinessObject[] selectedElements)
		{
			List<SecurityCheckpoint> checkPoints = new List<SecurityCheckpoint>();

			if (IsConsolPosting)
			{
				switch (postingOption)
				{
					case JobInvoicingPostingOption.All:
						checkPoints.Add(Env.Security.ConsolBulkPostWholeConsol);
						break;
					case JobInvoicingPostingOption.Agent:
						checkPoints.Add(Env.Security.ConsolBulkPostAgent);
						break;
					case JobInvoicingPostingOption.Costs:
					case JobInvoicingPostingOption.ConsolCosts:
						checkPoints.Add(Env.Security.ConsolBulkPostCosts);
						break;
				}
			}
			else
			{
				string checkpointName = GetCheckPointNameForPostingOption(postingOption);
				if (!string.IsNullOrEmpty(checkpointName))
				{
					checkPoints.AddRange(BulkJobSecurityCheckHelper.GetSecurityCheckPoints(checkpointName, selectedElements));
				}
			}

			return checkPoints.ToArray();
		}

		string GetCheckPointNameForPostingOption(JobInvoicingPostingOption postingOption)
		{
			string checkpointName = "";

			switch (postingOption)
			{
				case JobInvoicingPostingOption.All:
					checkpointName = SecurityCore.BulkPostAll;
					break;

				case JobInvoicingPostingOption.LocalClient:
					checkpointName = SecurityCore.BulkPostLocalClient;
					break;

				case JobInvoicingPostingOption.Agent:
					checkpointName = SecurityCore.BulkPostOverseas;
					break;

				case JobInvoicingPostingOption.Revenue:
					checkpointName = SecurityCore.BulkPostRevenue;
					break;

				case JobInvoicingPostingOption.Costs:
					checkpointName = SecurityCore.BulkPostCost;
					break;

				case JobInvoicingPostingOption.Disbursement:
					checkpointName = SecurityCore.BulkPostDSB;
					break;

				case JobInvoicingPostingOption.AllSisterCompanyCharges:
					checkpointName = SecurityCore.BulkPostAllSisterCompanyCharges;
					break;

				case JobInvoicingPostingOption.LocalSisterCompanyChargesOnly:
					checkpointName = SecurityCore.BulkPostLocalSisterCompanyChargesOnly;
					break;
			}

			return checkpointName;
		}

		MultilingualString GetMenuNameForPostingOption(JobInvoicingPostingOption postingOption)
		{
			MultilingualString menuName = null;

			switch (postingOption)
			{
				case JobInvoicingPostingOption.All:
					menuName = IsConsolPosting ? Constants.MenuNameConstants.PostWholeConsol : Constants.MenuNameConstants.PostAllChargesAndCosts;
					break;

				case JobInvoicingPostingOption.LocalClient:
					menuName = Constants.MenuNameConstants.PostLocalClientCharges;
					break;

				case JobInvoicingPostingOption.Agent:
					menuName = Constants.MenuNameConstants.PostOverseasAgentCharges;
					break;

				case JobInvoicingPostingOption.Revenue:
					menuName = Constants.MenuNameConstants.PostAllRevenueCharges;
					break;

				case JobInvoicingPostingOption.Costs:
					menuName = IsConsolPosting ? Constants.MenuNameConstants.PostAllCosts : Constants.MenuNameConstants.PostCosts;
					break;

				case JobInvoicingPostingOption.ConsolCosts:
					menuName = IsConsolPosting ? Constants.MenuNameConstants.PostConsolCostsOnly : null;
					break;

				case JobInvoicingPostingOption.Disbursement:
					menuName = Constants.MenuNameConstants.PostDisbursementChargesonly;
					break;

				case JobInvoicingPostingOption.AllSisterCompanyCharges:
					menuName = Constants.MenuNameConstants.PostAllSisterCompanyCharges;
					break;

				case JobInvoicingPostingOption.LocalSisterCompanyChargesOnly:
					menuName = Constants.MenuNameConstants.PostLocalSisterCompanyChargesOnly;
					break;
			}

			return menuName;
		}

		bool IsConsolPosting
		{
			get;
			set;
		}

		ZString BusinessObjectName;

		#endregion
	}
}
