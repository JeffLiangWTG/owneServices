using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	TransactionsPendingForEReportingCheckServiceTask.Code,
	"Transactions Pending for E-Reporting Check Service Task",
	"ACC",
	typeof(TransactionsPendingForEReportingCheckServiceTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	IsScheduleReadOnly = true,
	ActiveByDefault = true,
	MinimumPeriod = "1day",
	DefaultScheduleRunEvery = "1day"
	)
]
namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class TransactionsPendingForEReportingCheckServiceTask : ServiceProviderImpl
	{
		public const string Code = "ETP";

		bool ShouldSendNotification(EInvoicingPendingTransactionsNotificationGroup group)
		{
			return group.GroupPK != Guid.Empty;
		}

		public override void RunTask(CancellationToken token)
		{
			ServiceLogger.Log(LogType.Information, "Transactions Pending for E-Reporting Check Service started.");

			foreach (var company in GlbCompany.GetActiveCompanies())
			{
				var transactionsToNotifyForCompany = new List<(ZGuid transactionPK, ZString transactionType, ZString transactionNum)>();
				var branches = company.ActiveBranches.OrderBy(x => x.GB_Code).Where(x => ShouldSendNotification(GetNotificationGroupWithFallback(x)));
				var shouldSendCompanyLevelNotificationEmail = company.ActiveBranches.All(branch => !ShouldSendNotification(GetNotificationGroupWithoutFallback(branch)));

				foreach (var branch in branches)
				{
					token.ThrowIfCancellationRequested();

					using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
					{
						var notificationGroup = GetNotificationGroupWithFallback(branch);
						var transactionsToNotifyForBranch = LoadPendingTransactionsForBranch(branch, notificationGroup);

						if (shouldSendCompanyLevelNotificationEmail)
						{
							transactionsToNotifyForCompany.AddRange(transactionsToNotifyForBranch);

							if (branch == branches.Last())
							{
								ServiceLogger.Log(LogType.Information, $"There are totally {transactionsToNotifyForCompany.Count} transactions for company: {company.GC_Code}.");
								SendNotificationEmail(transactionsToNotifyForCompany, notificationGroup.GroupPK.ToGuid(), company.GC_Code);
							}
						}
						else
						{
							SendNotificationEmail(transactionsToNotifyForBranch, notificationGroup.GroupPK.ToGuid(), branch.GB_Code);
						}
					}
				}
			}

			ServiceLogger.Log(LogType.Information, "Transactions Pending for E-Reporting Check Service ended.");
		}

		IEnumerable<(ZGuid transactionPK, ZString transactionType, ZString transactionNum)> LoadPendingTransactionsForBranch(GlbBranch branch, EInvoicingPendingTransactionsNotificationGroup notificationGroup)
		{
			var statusQuery = ElectronicInvoicingHelper.GetAIPStatusQuery(Constants.EInvoicingPivotState.Pending);
			statusQuery.AddToFilter(AccTransactionHeaderSchema.AH_GB, branch.PK);
			statusQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, branch.GB_GC);
			statusQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);

			var notificationCutoffDate = CalculateNotificationCutoffDate(notificationGroup.Days);
			switch (notificationGroup.DateType)
			{
				case EInvoicingPendingTransactionsNotificationGroup.DateTypeList.PostDate:
					statusQuery.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThan, notificationCutoffDate);
					break;
				case EInvoicingPendingTransactionsNotificationGroup.DateTypeList.InvoiceDate:
					statusQuery.AddToFilter(AccTransactionHeaderSchema.AH_InvoiceDate, SQLComparisonOperator.LessThan, notificationCutoffDate);
					break;
			}

			var sql = string.Format(CultureInfo.InvariantCulture, @"SELECT {0}, {1}, {2} FROM {3} {4}",
				AccTransactionHeaderSchema.Constants.PK,
				AccTransactionHeaderSchema.Constants.AH_TransactionType,
				AccTransactionHeaderSchema.Constants.AH_TransactionNum,
				AccTransactionHeaderSchema.Constants.TableName,
				statusQuery.GetAsWhereClause(combineFilterAndParams: false));

			var transactionCollection = new DynamicBusinessObjectCollection(Factory);
			transactionCollection.Load(sql, statusQuery.Params);

			ServiceLogger.Log(LogType.Information, $"There are {transactionCollection.Count} transactions before {notificationCutoffDate.ToString("MM-dd-yyyy")} for branch: {branch.Company.GC_Code} - {branch.GB_Code}.");

			return transactionCollection.Select(x =>
				((ZGuid)x[AccTransactionHeaderSchema.Constants.PK],
				(ZString)x[AccTransactionHeaderSchema.Constants.AH_TransactionType],
				(ZString)x[AccTransactionHeaderSchema.Constants.AH_TransactionNum])
			);
		}

		void SendNotificationEmail(IEnumerable<(ZGuid transactionPK, ZString transactionType, ZString transactionNum)> transactionsToNotify, Guid recipientId, string code)
		{
			if (transactionsToNotify.Any())
			{
				new TransactionsPendingForEReportingCheckEmail(code, recipientId, transactionsToNotify).Send();
				ServiceLogger.Log(LogType.Information, $"Email has been sent to {code}.");
			}
			else
			{
				ServiceLogger.Log(LogType.Information, $"No email will be sent.");
			}
		}

		EInvoicingPendingTransactionsNotificationGroup GetNotificationGroupWithoutFallback(GlbBranch branch)
		{
			return AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.GetValueWithoutFallback(Guid.Empty, branch.PK.ToGuid(), Guid.Empty);
		}

		EInvoicingPendingTransactionsNotificationGroup GetNotificationGroupWithFallback(GlbBranch branch)
		{
			return AccountingMasterFilesRegistry.Instance.EInvoicingPendingTransactionsNotificationGroup.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
		}

		ZDate CalculateNotificationCutoffDate(int daysBeforeToday)
		{
			var localDate = ZDateTime.Now.Date;
			daysBeforeToday -= 1;
			return localDate.AddDays(-daysBeforeToday);
		}

		ReadOnlyBusinessObjectFactory Factory => factory ?? (factory = new ReadOnlyBusinessObjectFactory());
		ReadOnlyBusinessObjectFactory factory;
	}
}
