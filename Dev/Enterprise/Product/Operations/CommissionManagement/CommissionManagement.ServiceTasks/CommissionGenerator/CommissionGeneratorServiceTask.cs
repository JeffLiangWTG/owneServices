using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.CommissionManagement.Business;
using Enterprise.CommissionManagement.ServiceTasks.CommissionGenerator;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"CGN", 
	"Commission Generator Service Task", 
	"SAL", 
	typeof(CommissionGeneratorServiceTask), 
	IsMandatory = true, 
	MinimumPeriod = "30minutes",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "30minutes"
	)]

[assembly: HostedServiceBusinessObjectBinding("CGN", OrgCommissionCalculationQueueSchema.Constants.TableName, new string[] { }, "Commission Generator Queue")]
namespace Enterprise.CommissionManagement.ServiceTasks.CommissionGenerator
{
	public class CommissionGeneratorServiceTask : ServiceProviderImpl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception is logged")]
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			ServiceLogger.Log(LogType.Information, "Commission generation starting.");

			var masterFactoryProvider = CreateBusinessObjectFactoryProvider();
			masterFactoryProvider.Current.RefreshEnabled = false;

			ErrorCount = 0;
			var commissionCalculationQueueCount = 0;

			GetQueueReader().Process((queueItem, e) =>
			{
				if (queueItem == null)
				{
					return;
				}

				e.Cancel |= youMustReactToThisToken.IsCancellationRequested;
				if (e.Cancel)
				{
					return;
				}

				switch (queueItem.CAQ_Operation)
				{
					case OrgCommissionCalculationQueueOperationCodeList.Codes.Approval:
						ApproveItem(queueItem, masterFactoryProvider);
						break;
					case OrgCommissionCalculationQueueOperationCodeList.Codes.Creation:
						CreateItem(queueItem, masterFactoryProvider);
						break;
					case OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration:
						RegenerateItem(queueItem, masterFactoryProvider);
						break;
				}
				commissionCalculationQueueCount++;
			}, 20);

			if(ErrorCount > 0)
			{
				ServiceLogger.Log(LogType.Error, $"{ErrorCount} out of {commissionCalculationQueueCount} Commission generations have failed.");
			}
			else
			{
				ServiceLogger.Log(LogType.Information, "Commission generation completed.");
			}
		}

		protected virtual BusinessObjectFactoryProvider CreateBusinessObjectFactoryProvider()
		{
			return new BusinessObjectFactoryProvider();
		}

		void ApproveItem(OrgCommissionCalculationQueue queueItem, BusinessObjectFactoryProvider masterFactoryProvider)
		{
			var factory = masterFactoryProvider.Current;
			var agreement = queueItem.CommissionAgreement;

			if (agreement == null)
			{
				ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "Unable to find agreement with PK: {0}.", queueItem.CAQ_CA0));
				return;
			}

			if (Db.Connection.TryGetLock(CommissionAgreementApprover.AgreementLockPrefix + agreement.PK, out var sqlLock))
			{
				try
				{
					ServiceLogger.Log(LogType.Debug,
						string.Format(CultureInfo.CurrentCulture,
							"Generating commissions for agreement with PK: {0}, ID: {1}, minumum invoice posted date: {2}, override existing commissions: {3}.",
							queueItem.CAQ_CA0,
							agreement.AgreementId,
							queueItem.CAQ_MinimumInvoicePostedDate,
							queueItem.CAQ_OverwriteExistingCommissions));

					var date = queueItem.CAQ_MinimumInvoicePostedDate;
					var overwrite = queueItem.CAQ_OverwriteExistingCommissions;

					CommissionAgreementApprover.New(factory).CreateCommissions(GetCreateCommissionContext(date, overwrite, agreement), null);

					masterFactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorCount++;
					ServiceLogger.Log(LogType.Debug, "Error generating commissions: " + ex.Message);
				}
				finally
				{
					sqlLock.Dispose();
				}
			}
			else
			{
				ServiceLogger.Log(LogType.Debug, string.Format(CultureInfo.CurrentCulture, "Unable to acquire lock on agreement with PK: {0}.", queueItem.CAQ_CA0));
			}
		}

		void CreateItem(OrgCommissionCalculationQueue queueItem, BusinessObjectFactoryProvider masterFactoryProvider)
		{
			try
			{
				if (queueItem.Job is Job job)
				{
					ServiceLogger.Log(LogType.Debug, $"Creating Commissions for Job: {job.JH_JobNum}, PK: {job.PK}, Closed Date: {queueItem.CAQ_JobClosedDate}");
					var jobInMasterFactory = masterFactoryProvider.Current.Load<IJobHeader>(queueItem.CAQ_JH);
					using (DisposableEnvironment.ForBranch(jobInMasterFactory.JH_GB.ToGuid()))
					{
						((Job)jobInMasterFactory).InitializeParentFromGenericJobWithSettingDefaults();
						var jobClosedCommissionCreator = new JobClosedCommissionCreator(jobInMasterFactory, queueItem.CAQ_JobClosedDate);
						jobClosedCommissionCreator.CreateCommissionsOnJobClosed();
					}
				}
				else if (queueItem.CommissionableTransaction is InvoicingBase transaction)
				{
					ServiceLogger.Log(LogType.Debug, $"Creating Commissions for Transaction: {transaction.AH_TransactionNum}, PK: {transaction.PK}");
					var transactionInMasterFactory = masterFactoryProvider.Current.Load<ITransactionHeader>(queueItem.CAQ_AH) as ICommissionableTransaction;

					using (DisposableEnvironment.ForBranch(transactionInMasterFactory.AH_GB.ToGuid()))
					{
						var invoicePostedCreateCommissions = new InvoicePostedCreateCommissions(transactionInMasterFactory);
						invoicePostedCreateCommissions.CreateCommissions();
					}
				}

				masterFactoryProvider.Current.Load<OrgCommissionCalculationQueue>(queueItem.PK).Delete();
				masterFactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorCount++;
				ServiceLogger.Log(LogType.Debug, "Error regenerating commissions: " + ex.Message);
			}
		}

		void RegenerateItem(OrgCommissionCalculationQueue queueItem, BusinessObjectFactoryProvider masterFactoryProvider)
		{
			try
			{
				if (queueItem.Job is Job job)
				{
					ServiceLogger.Log(LogType.Debug, $"Regenerating Commissions for Job: {job.JH_JobNum}, PK: {job.PK}");
					using (DisposableEnvironment.ForBranch(job.JH_GB.ToGuid()))
					{
						job.RegenerateCommissions(masterFactoryProvider.Current, ServiceLogger);
					}
				}
				else if (queueItem.CommissionableTransaction is InvoicingBase transaction)
				{
					ServiceLogger.Log(LogType.Debug, $"Regenerating Commissions for Transaction: {transaction.AH_TransactionNum}, PK: {transaction.PK}");
					transaction.RegenerateCommissions(masterFactoryProvider.Current, ServiceLogger);
				}

				masterFactoryProvider.Current.Load<OrgCommissionCalculationQueue>(queueItem.PK).Delete();
				masterFactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorCount++;
				ServiceLogger.Log(LogType.Debug, "Error regenerating commissions: " + ex.Message);
			}
		}

		protected virtual CreateCommissionContext GetCreateCommissionContext(ZDateTime date, ZBool overwrite, OrgCommissionAgreement agreement)
		{
			return new CreateCommissionContext
			{
				FromDate = date,
				OverwriteOldValues = overwrite,
				AgreementsBeingApproved = new HashSet<OrgCommissionAgreement>(new[] { agreement })
			};
		}

		protected virtual DbOnlyBusinessObjectQueue<OrgCommissionCalculationQueue> GetQueueReader()
		{
			var query = new ZQuery();
			query.OrderBy = OrgCommissionCalculationQueueSchema.Constants.CAQ_SystemCreateTimeUtc;
			return new DbOnlyBusinessObjectQueue<OrgCommissionCalculationQueue>(query);
		}

		public int ErrorCount { get; set; }
	}
}
