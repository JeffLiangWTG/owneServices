using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class AccountingBillingService : IAccountingBillingService
	{
		public BillingActionResult CreateJobHeader(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk, Guid localClientAddressPk)
		{
			var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(operationsJobTableCode ?? string.Empty, false);
			if (type == null)
			{
				return new BillingActionResult(GetInvalidTablePrefixMessage(), true);
			}

			using (Env.SetTemporaryUserContext(staffPk, branchPk, departmentPk))
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "Accounting WebService Create Job Header" };
				var operationsJob = factory.Load(operationsJobTableCode, operationsJobPk);
				var success = JobHeaderHelper.LoadOrCreateJobWithAddress(branchPk, localClientAddressPk, operationsJob);
				if (success)
				{
					factory.Save();

					return new BillingActionResult(string.Empty, false);
				}

				return new BillingActionResult(Res.GetString("054409DD-5D63-448D-893A-B1CCC27F2CFB", "Cannot load/create job header."), true);
			}
		}

		public BillingActionResult PostRevenue(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk)
		{
			return PostTransactions<IRevenuePosterCreator>(
				operationsJobPk,
				operationsJobTableCode,
				staffPk,
				branchPk,
				departmentPk,
				(creator, provider) => creator.CreateRevenuePoster(provider));
		}

		public BillingActionResult PostCost(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk)
		{
			return PostTransactions<ICostPosterCreator>(
				operationsJobPk,
				operationsJobTableCode,
				staffPk,
				branchPk,
				departmentPk,
				(creator, provider) => creator.CreateCostPoster(provider));
		}

		public BillingActionResult PostOverseasAgentCharges(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk)
		{
			return PostTransactions<IPostOverseasAgentChargesProcessorCreator>(
				operationsJobPk,
				operationsJobTableCode,
				staffPk,
				branchPk,
				departmentPk,
				(creator, provider) => creator.CreateOverseasAgentChargesPoster(provider));
		}

		BillingActionResult PostTransactions<T>(Guid operationsJobPk, string operationsJobTableCode, Guid staffPk, Guid branchPk, Guid departmentPk, Func<T, IWorkflowProvider, IProcessor> getPoster)
		{
			var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(operationsJobTableCode ?? string.Empty, false);

			if (type == null)
			{
				return new BillingActionResult(GetInvalidTablePrefixMessage(), true);
			}

			var notifications = new ProcessNotifications();

			using (Env.SetTemporaryUserContext(staffPk, branchPk, departmentPk))
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "Accounting Web Service Posting" };
				var operationsJob = factory.Load(operationsJobTableCode, operationsJobPk);

				if (operationsJob == null)
				{
					return new BillingActionResult(Res.GetString("D8378879-29B0-49C7-83EF-DD529BF8F042", "Operations job with Table-prefix '{0}' and PK '{1}' could not be found.", operationsJobTableCode, operationsJobPk), true);
				}

				var poster = getPoster(ObjectFactory.Get<T>(), operationsJob as IWorkflowProvider);
				if (poster == null)
				{
					return new BillingActionResult(Res.GetString("4E5B9C85-55B8-484A-916F-DBA3C5E1E467", "Operation job is not valid."), true);
				}

				try
				{
					poster.Process(notifications, CancellationToken.None);
					factory.Save();
				}
				catch (LogSubscriberToAbortLogGroupProcessingSilentlyException ex)
				{
					var newFactory = new BusinessObjectFactory();
					ex.Emails?.ForEach(x => x.Create(newFactory));
					newFactory.Save();
				}
			}

			return notifications.HasErrors() ? new BillingActionResult(string.Join("; ", notifications.GetErrors().Select(e => e.Message)), false) : new BillingActionResult(string.Empty, false);
		}

		public BillingActionResult SplitApportionAmount(Guid jobConsolCostPK, Guid staffPk, Guid branchPk, Guid departmentPk)
		{
			using (Env.SetTemporaryUserContext(staffPk, branchPk, departmentPk))
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Accounting WebService Split Apportion Amount" };
				var jobConsolCost = factory.Load<JobConsolCost>(jobConsolCostPK);
				if (jobConsolCost == null)
				{
					return new BillingActionResult(Res.GetString("80E5C681-1F7E-4936-BB38-B5A9654CEE7D", "Consol Cost could not be found."), true);
				}
				var listing = new ApportionmentListing(factory, jobConsolCost.Consol);
				listing.PrepareForConsolCosting();
				jobConsolCost.SplitApportionAmount();
				jobConsolCost.ApportionGSTCharges();
				factory.Save();

				return new BillingActionResult(string.Empty, false);
			}
		}

		class ProcessNotifications : List<INotification>, INotifications
		{
		}

		string GetInvalidTablePrefixMessage()
		{
			return Res.GetString("1B78091C-3710-42FA-8916-B9C75D69914A", "Table prefix is invalid.");
		}
	}
}
