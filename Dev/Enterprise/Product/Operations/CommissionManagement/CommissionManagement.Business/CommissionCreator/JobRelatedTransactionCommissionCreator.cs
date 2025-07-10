using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class JobRelatedTransactionCommissionCreator : TransactionCommissionCreator
	{
		public JobRelatedTransactionCommissionCreator(ICommissionableTransaction transaction, Tuple<IJobHeader, ZDateTime> closedJobAndDate, DataTable effectiveDateCacheTable = null)
			: base(transaction, effectiveDateCacheTable)
		{
			Argument.NotNull(transaction, "transaction");

			if (!transaction.IsJobRelated)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Must be job-related transaction, but passed in transaction (PK:{0}) with empty AH_JH and no job related transaction lines", transaction.PK));
			}
			else if (!transaction.AH_TransactionBelongsToGroup.IsEmpty && transaction.AH_IsCancelled)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Passed in transaction (PK:{0}) which is a reversal. Use {1} to create commission for reversal transactions", transaction.PK, nameof(ReversalTransactionCommissionCreator)));
			}

			this.closedJobAndDate = closedJobAndDate;
		}

		readonly Tuple<IJobHeader, ZDateTime> closedJobAndDate;

		#region Create Commission

		public override void CreateCommissions(CreateCommissionContext context)
		{
			if (!Transaction.AH_IsCancelled)
			{
				var commissionDate = MainTransaction.AH_PostDate.Date;
				if (closedJobAndDate != null && OrganisationsDataRegistry.Instance.CommissionRecognitionDate.Value == CommissionRecognitionDateTypeList.Codes.PostDateOfFirstArTransaction)
				{
					var firstARInvoice = AccountingUtils.GetFirstARTransactionFromJob(closedJobAndDate.Item1);

					if (firstARInvoice != null)
					{
						commissionDate = firstARInvoice.AH_PostDate.Date;
					}
				}

				var snapshotDateTime = ZDateTime.Empty;
				var snapshotEventCode = string.Empty;

				if (closedJobAndDate != null)
				{
					snapshotDateTime = closedJobAndDate.Item2;

					if (context.RegeneratingCommissions)
					{
						snapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.Regenerated;
					}
					else
					{
						snapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.JobClosure;
					}
				}
				else
				{
					snapshotDateTime = Transaction.AH_PostDate;
					snapshotEventCode = AccCommissionHeaderSnapshotEventList.Codes.Posted;
				}

				var transactionLinesGroupedByApportionedJob = Transaction.Lines.Cast<TransactionLine>().GroupBy(x => x.AL_JH);
				foreach (var apportionedJobGrouping in transactionLinesGroupedByApportionedJob)
				{
					var apportionedJobPk = apportionedJobGrouping.Key;
					if (closedJobAndDate != null && !apportionedJobPk.IsEmpty && apportionedJobPk != closedJobAndDate.Item1.PK)
					{
						// Do not create commission for lines not apportioned to closed job
						continue;
					}

					var apportionedJob = !apportionedJobPk.IsEmpty ? Factory.Load<JobHeader>(apportionedJobPk) : null;
					var customerPk = GetCustomerPk(Transaction, apportionedJob);
					if (customerPk.IsEmpty)
					{
						continue;
					}

					PopulateEffectiveDateForTriggerType(customerPk);

					var commissionDateByChargeDictionary = new Dictionary<ZGuid, ZDate>();
					if (closedJobAndDate != null && OrganisationsDataRegistry.Instance.CommissionRecognitionDate.Value == CommissionRecognitionDateTypeList.Codes.AccountingRevenueRecognition)
					{
						SetCommissionDateForCharges(closedJobAndDate.Item1, commissionDateByChargeDictionary);
					}
					else
					{
						commissionDateByChargeDictionary[ZGuid.Empty] = commissionDate;
					}

					var product = GetCommissionableProductCode(apportionedJob);
					var service = GetCommissionableServiceCode();
					var subModule = GetCommissionableSubModuleCode();
					var mode = GetCommissionableMode(apportionedJob);
					var origin = GetCommissionableOrigin(apportionedJob);
					var destination = GetCommissionableDestination(apportionedJob);
					var builder = new CommissionHeaderBuilder(Factory, context, GetShouldCreateCommissionPredicate(apportionedJob));
					var buildItemArgs = new CommissionHeaderBuildItemArgs(Transaction, apportionedJob ?? (BusinessObject)MainTransaction, customerPk, product, service, subModule, commissionDateByChargeDictionary, snapshotDateTime, snapshotEventCode, mode, origin, destination);
					var buildItem = new CommissionHeaderBuildItem(context, buildItemArgs, GetCreatePercentageCommissionLineGroupsDelegate(apportionedJobGrouping, commissionDateByChargeDictionary), effectiveDateCacheTable);
					builder.Create(buildItem);
				}
			}
			else
			{
				ReversalTransactionCommissionCreator.CreateReversalTransactionCommissionsIfRequired(Transaction);
			}
		}

		void SetCommissionDateForCharges(IJobHeader job, Dictionary<ZGuid, ZDate> commissionDateByChargeDictionary)
		{
			var commissionLinesHelper = new CommissionLinesHelper();

			var allTransactionLinesForJob = commissionLinesHelper.GetTransactionLines(job);
			var chargeCodePKList = allTransactionLinesForJob.Select(l => l.AL_AC).Distinct();

			foreach (var chargeCodePK in chargeCodePKList)
			{
				if (commissionDateByChargeDictionary.ContainsKey(chargeCodePK))
				{
					continue;
				}

				var line = allTransactionLinesForJob.FirstOrDefault(l => l.AL_AC == chargeCodePK);
				if (line != null)
				{
					var commissionDate = line.AL_LineType == TransactionLineTypes.Revenue || line.AL_LineType == TransactionLineTypes.Cost ? (ZDate)line.AL_ReverseDate : (ZDate)line.AL_PostDate;
					commissionDateByChargeDictionary[chargeCodePK] = commissionDate;
				}
			}
		}

		public static ZString GetCommissionableMode(JobHeader job)
		{
			if (job == null)
			{
				return OrgCommissionAgreementItemLookups.AllModesCode;
			}

			var plugin = job.Parent as IJobInvoicingPlugIn;
			var invoicingSupporter = plugin?.InvoicingSupporter;
			if (invoicingSupporter == null)
			{
				return ZString.Empty;
			}

			switch (invoicingSupporter.ConsumerType.Code)
			{
				case JobInvoicingConsumerTypes.ShipmentCode:
					return invoicingSupporter.TransportMode;
				case JobInvoicingConsumerTypes.BrokerageCode:
					return invoicingSupporter.IsImport
						? Core.Constants.FreightShipmentDirection.Code.Import
						: invoicingSupporter.IsExport
							? Core.Constants.FreightShipmentDirection.Code.Export
							: Core.Constants.FreightShipmentDirection.Code.All;
				case JobInvoicingConsumerTypes.AgencyBillOfLadingCode:
				case JobInvoicingConsumerTypes.AgencyBookingCode:
					return invoicingSupporter.ContainerMode;
				default:
					return OrgCommissionAgreementItemLookups.AllModesCode;
			}
		}

		public static ZString GetCommissionableOrigin(JobHeader job)
		{
			if (job == null)
			{
				return ZString.Empty;
			}

			var plugin = job.Parent as IJobInvoicingPlugIn;
			var invoicingSupporter = plugin?.InvoicingSupporter;
			if (invoicingSupporter == null || invoicingSupporter.Origin == null)
			{
				return ZString.Empty;
			}

			return invoicingSupporter.Origin.Code;
		}

		public static ZString GetCommissionableDestination(JobHeader job)
		{
			if (job == null)
			{
				return ZString.Empty;
			}

			var plugin = job.Parent as IJobInvoicingPlugIn;
			var invoicingSupporter = plugin?.InvoicingSupporter;
			if (invoicingSupporter == null || invoicingSupporter.Destination == null)
			{
				return ZString.Empty;
			}

			return invoicingSupporter.Destination.Code;
		}

		public Func<ICommissionAgreementAndRates, bool> GetShouldCreateCommissionPredicate(JobHeader apportionedJob)
		{
			if (apportionedJob == null || closedJobAndDate != null)
			{
				return null;
			}

			if (OrganisationsDataRegistry.Instance.CommissionTransactionJobTrigger.Value == CommissionTransactionJobTriggerList.Codes.RevenueProfitBasedCommissionCalculationsToBeCreatedAtClsStatus)
			{
				return agreementAndRates => false;
			}

			return agreementAndRates => agreementAndRates.CommissionAgreement.CA0_CommissionBasis != CommissionBasisType.Codes.PRF;
		}

		#endregion

		static ZGuid GetCustomerPk(ICommissionableTransaction transaction, JobHeader job)
		{
			if (job != null)
			{
				var customerAddress = job.LocalChargesAddr;
				if (customerAddress != null && !customerAddress.OA_OH.IsEmpty)
				{
					return customerAddress.OA_OH;
				}
			}

			return transaction.AH_OH;
		}

		#region Commission Agreement Item

		public static ZString GetCommissionableProductCode(JobHeader job)
		{
			if (job == null)
			{
				return OrgCommissionAgreementItemLookups.AllProductsCode;
			}

			if (job.Parent == null)
			{
				job.InitializeParentFromGenericJobWithoutSettingDefaults();
			}

			var plugin = job.Parent as IJobInvoicingPlugIn;
			var invoicingSupporter = plugin != null ? plugin.InvoicingSupporter : null;
			if (invoicingSupporter == null)
			{
				return OrgCommissionAgreementItemLookups.AllProductsCode;
			}

			return invoicingSupporter.ConsumerType.Code;
		}

		public static ZString GetCommissionableServiceCode()
		{
			return OrgCommissionAgreementItemLookups.AllServicesCode;
		}

		public static ZString GetCommissionableSubModuleCode()
		{
			return OrgCommissionAgreementItemLookups.AllSubModulesCode;
		}

		#endregion

		#region Queries

		public static ZDBOnlyQuery GetJobRelatedTransactionsQuery(JobHeader job)
		{
			var result = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			result.AddToFilter(TransactionCommissionCreator.GetIsCommissionableTransactionQuery());

			var jobRelatedPart = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			jobRelatedPart.AddToFilter(AccTransactionHeaderSchema.AH_JH, job.PK);
			var containsJobRelatedLineQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
			containsJobRelatedLineQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, job.PK);
			jobRelatedPart.AddSubQuery(containsJobRelatedLineQuery, JoinCondition.Or);

			result.AddToFilter(jobRelatedPart);

			return result;
		}

		#endregion
	}
}
