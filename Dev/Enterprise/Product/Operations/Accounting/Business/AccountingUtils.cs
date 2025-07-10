using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.LandTransport;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.TaxDateDefaultingOptionLookups;

namespace Enterprise.Accounting.Business
{
	public partial class AccountingUtils
	{
		public const string ARControl = "GL_AR_CONTROL_ACCOUNT";
		public const string APControl = "GL_AP_CONTROL_ACCOUNT";
		public const string GLJournalClearing = "GL_JOURNAL_CLEARING_ACCOUNT";
		public const string ARJournal = "GL_AR_JOURNAL_ACCOUNT";
		public const string APJournal = "GL_AP_JOURNAL_ACCOUNT";
		public const string ARDiscount = "GL_AR_DISCOUNT_ACCOUNT";
		public const string APDiscount = "GL_AP_DISCOUNT_ACCOUNT";
		public const string FinanceCharges = "GL_FINANCE_CHG_ACCOUNT";
		public const string ExchangeGain = "GL_EXCHANGE_GAIN_ACCOUNT";
		public const string ExchangeLoss = "GL_EXCHANGE_LOSS_ACCOUNT";
		public const string GSTInput = "GL_GST_INPUT_ACCOUNT";
		public const string GSTOutput = "GL_GST_OUTPUT_ACCOUNT";
		public const string WHTInput = "GL_WHT_INPUT_ACCOUNT";
		public const string WHTOutput = "GL_WHT_OUTPUT_ACCOUNT";
		public const string AccruedRevenue = "GL_ACCRUED_REVENUE_ACCOUNT";
		public const string AccruedCost = "GL_ACCRUED_COST_ACCOUNT";
		public const string Overpayments = "GL_OVERPAYMENTS_ACCOUNT";
		public const string CFXAccount = "GL_CFX_ACCOUNT";
		public const string FringeBenefits = "GL_FRINGE_BENEFITS_ACCOUNT";
		public const string PendingGSTInput = "GL_PENDING_GST_INPUT_ACCOUNT";
		public const string PendingGSTOutput = "GL_PENDING_GST_OUTPUT_ACCOUNT";
		public const string PLAppropriation = "GL_PL_APPROPRIATION_ACCOUNT";
		public const string BSAccountStart = "GL_BS_ACCOUNT_START";
		public const string APSuspenseControlAccount = "GL_AP_SUSPENSE_CONTROL_ACCOUNT";
		public const string ARSuspenseControlAccount = "GL_AR_SUSPENSE_CONTROL_ACCOUNT";
		public const string JobRevenueJournalControlAccount = "GL_JOB_REVENUE_JOURNAL_CONTROL_ACCOUNT";

		public const string PeriodSetup = "GL_ACC_PERIOD_SETUP";

		public const string PeriodSetupCompany = "PERIOD_SETUP_COMPANY";
		public const string PeriodSetupPeriodCount = "PERIOD_SETUP_COUNT";
		public const string PeriodSetupStartDate = "PERIOD_SETUP_STARTDATE";
		public const string PeriodSetupPeriodFormat = "PERIOD_SETUP_FORMAT";

		public const string GLAccountNoFormat = "GL_ACC_NO_FORMAT";

		internal const int ObjectCountThresholdToDisableDataRefreshBus = 500;
		internal const int DuplicateInvoiceNumberPeriodMonths = 12;

		public enum TransactionNumberUseCategory
		{
			Unknown,
			APTransaction,
			UATransaction,
			PATransaction,
			INTransaction
		}

		[Flags]
		public enum DuplicateTransactionNumberCheckResult
		{
			None = 0,
			OutsideOfPeriod = 1,
			InsideOfPeriod = 2,
			OutsideButNoPermissions = 4,
			UnableToCheck = 8,
			All = OutsideOfPeriod | InsideOfPeriod | OutsideButNoPermissions | UnableToCheck
		}

		public static class PeriodClosureConfigurationIntervalType
		{
			public const string Minutes = "MINS";
			public const string Days = "DAYS";
			public const string Hours = "HOURS";
		}

		public struct PreviousSameNumberTransactionDetails
		{
			public PreviousSameNumberTransactionDetails(string allowDuplicateInvoiceNumberRule, DuplicateTransactionNumberCheckResult result, ZDateTime previousInvoiceDate, ZInt previousTransactionCount, TransactionNumberUseCategory category)
			{
				AllowDuplicateInvoiceNumberRule = allowDuplicateInvoiceNumberRule;
				Result = result;
				PreviousInvoiceDate = previousInvoiceDate;
				PreviousTransactionCount = previousTransactionCount;
				Category = category;
			}

			public bool HasNotification
			{
				get { return (Result & (DuplicateTransactionNumberCheckResult.All)) != 0; }
			}

			public bool HasError
			{
				get { return HasNotification && !IsDuplicateTransactionNumberAllowed; }
			}

			public bool IsDuplicateTransactionNumberAllowed
			{
				get { return Result == DuplicateTransactionNumberCheckResult.OutsideOfPeriod; }
			}

			public INotificationType NotificationType
			{
				get { return Result == DuplicateTransactionNumberCheckResult.OutsideOfPeriod ? CargoWise.ComponentModel.NotificationType.Warning : CargoWise.ComponentModel.NotificationType.Error; }
			}

			public ZString NotificationMessage
			{
				get
				{
					ZStringBuilder notificationMessage = new ZStringBuilder();

					string duplicateOutsideOfPeriod;
					string duplicateInsideOfPeriod;

					switch (AllowDuplicateInvoiceNumberRule)
					{
						case AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD:
							duplicateOutsideOfPeriod = Res.GetString("90B8E8C0-3130-4936-A1F4-066FC0A0C399", "Last posted transaction’s invoice date is {0:dd-MMM-yy} which is at least 12 months apart. ", PreviousInvoiceDate.Date);
							duplicateInsideOfPeriod = Res.GetString("CE9496DB-3BBD-4663-9605-354D16CB39DF", "Last posted transaction’s invoice date is {0:dd-MMM-yy} which is less than 12 months apart. ", PreviousInvoiceDate.Date);
							break;
						case AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL:
							duplicateOutsideOfPeriod = Res.GetString("A90FD20E-67A6-4CA4-BEC1-A6859AB121D9", "Last posted transaction’s invoice date is {0:dd-MMM-yy} which is in another calendar year. ", PreviousInvoiceDate.Date);
							duplicateInsideOfPeriod = Res.GetString("28343070-1D22-4364-A653-D4BDB8DB39B0", "Last posted transaction’s invoice date is {0:dd-MMM-yy} which is in the same calendar year. ", PreviousInvoiceDate.Date);
							break;
						default:
							throw new ArgumentOutOfRangeException(nameof(AllowDuplicateInvoiceNumberRule), AllowDuplicateInvoiceNumberRule, "unknown value");
					}

					switch (Category)
					{
						case TransactionNumberUseCategory.APTransaction:
							notificationMessage.Append(Res.GetString("F2137D66-D24E-499A-BBB5-D65B1BF09EE2", "The transaction number is already in use. "));
							break;
						case TransactionNumberUseCategory.PATransaction:
							notificationMessage.Append(Res.GetString("7B0DAF3A-4554-4B2F-8F5A-2E1F0FA1430A", "The transaction number is already in use by Transaction Pending Allocation. "));
							break;
						case TransactionNumberUseCategory.UATransaction:
							notificationMessage.Append(Res.GetString("677FAB55-25C0-4966-9E49-63311B38A1D1", "The transaction number is already in use by Unapproved Invoice. "));
							break;
						case TransactionNumberUseCategory.INTransaction:
							notificationMessage.Append(Res.GetString("8AF306FE-EF0E-4C01-90BE-492F86087A74", "The transaction number is already in use by Incomplete Transaction. "));
							break;
					}

					switch (Result)
					{
						case DuplicateTransactionNumberCheckResult.OutsideOfPeriod:
							notificationMessage.Append(duplicateOutsideOfPeriod);
							notificationMessage.Append(Res.GetString("2E748F9C-3A0E-423D-922E-C0BBED616B21", "You are granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number can be used."));
							break;
						case DuplicateTransactionNumberCheckResult.InsideOfPeriod:
							notificationMessage.Append(duplicateInsideOfPeriod);
							notificationMessage.Append(Res.GetString("2D7773A5-16F3-4442-B532-C4BB40DB5BF1", "This transaction number cannot be used. Please enter another one."));
							break;
						case DuplicateTransactionNumberCheckResult.OutsideButNoPermissions:
							notificationMessage.Append(duplicateOutsideOfPeriod);
							notificationMessage.Append(Res.GetString("ADA88E83-AC8E-4C11-9CDB-CBD1F54A863C", "You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one."));
							break;
						case DuplicateTransactionNumberCheckResult.UnableToCheck:
							notificationMessage.Append(Res.GetString("9CA89912-3B1C-4836-B0EC-EE5E6EF783E2", "Cannot check if the duplicate transaction number is allowed as the Invoice Date is empty."));
							break;
					}

					return notificationMessage.ToString();
				}
			}

			public readonly string AllowDuplicateInvoiceNumberRule;
			public readonly DuplicateTransactionNumberCheckResult Result;
			public readonly ZDateTime PreviousInvoiceDate;
			public readonly ZInt PreviousTransactionCount;
			public readonly TransactionNumberUseCategory Category;

			public override bool Equals(object obj)
			{
				var key = (PreviousSameNumberTransactionDetails)obj;
				return key.AllowDuplicateInvoiceNumberRule == this.AllowDuplicateInvoiceNumberRule
						&& key.Result == this.Result
						&& key.PreviousInvoiceDate == this.PreviousInvoiceDate
						&& key.PreviousTransactionCount == this.PreviousTransactionCount
						&& key.Category == this.Category;
			}

			public override int GetHashCode()
			{
				return AllowDuplicateInvoiceNumberRule.GetHashCode() ^ Result.GetHashCode() ^ PreviousInvoiceDate.GetHashCode() ^ PreviousTransactionCount.GetHashCode() ^ Category.GetHashCode();
			}

			public static bool operator ==(PreviousSameNumberTransactionDetails previousAPInvoiceDetails1, PreviousSameNumberTransactionDetails previousAPInvoiceDetails2)
			{
				return previousAPInvoiceDetails1.Equals(previousAPInvoiceDetails2);
			}

			public static bool operator !=(PreviousSameNumberTransactionDetails previousAPInvoiceDetails1, PreviousSameNumberTransactionDetails previousAPInvoiceDetails2)
			{
				return !previousAPInvoiceDetails1.Equals(previousAPInvoiceDetails2);
			}
		}

		#region GST and WHT
		public static bool IsUserCanChangeGST(string ledger)
		{
			bool isApplicable = false;
			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
					isApplicable = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
					break;
				case LedgerTypes.AccountsReceivable:
					isApplicable = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
					break;
			}
			return isApplicable;
		}

		public static bool IsUserCanChangeWHT(string ledger)
		{
			bool isApplicable = false;
			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
					isApplicable = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
					break;
				case LedgerTypes.AccountsReceivable:
					isApplicable = AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyWHTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
					break;
			}
			return isApplicable;
		}

		#endregion

		#region Transaction numbers

		[SuppressMessage("Microsoft.Design", "CA1021: Avoid out parameters")]
		public static bool IsTransactionNumUsedInJobInvoicing(ZString transactionType, ZString transactionNum, ZGuid orgPK, ZGuid transactionHeaderToExcludePK, out ZString jobNumbers)
		{
			var result = false;
			jobNumbers = ZString.Empty;

			// Get related Transaction Types that could be linked to JobCharges
			var apTransactionType = ConvertTransactionTypeToAP(transactionType);
			var uaTransactionType = ConvertTransactionTypeToUA(transactionType);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			DynamicBusinessObjectCollection jobs = new DynamicBusinessObjectCollection(factory);
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@TransactionNum", transactionNum, JobChargeSchema.JR_APInvoiceNum));
			parameters.Add(ZSqlParameter.New("@Creditor", orgPK, JobChargeSchema.JR_OH_CostAccount));
			parameters.Add(ZSqlParameter.New("@CurrentCompany", GlbCompany.CurrentCompany.PK, JobChargeSchema.JR_GC));
			parameters.Add(ZSqlParameter.New("@OSCostAmt", 0m, JobChargeSchema.JR_OSCostAmt));
			parameters.Add(ZSqlParameter.New("@TransactionHeaderToExcludePK", transactionHeaderToExcludePK, AccTransactionHeaderSchema.PK));
			parameters.Add(ZSqlParameter.New("@TransactionType", transactionType, AccTransactionHeaderSchema.AH_TransactionType));
			parameters.Add(ZSqlParameter.New("@APTransactionType", apTransactionType, AccTransactionHeaderSchema.AH_TransactionType));
			parameters.Add(ZSqlParameter.New("@UATransactionType", uaTransactionType, AccTransactionHeaderSchema.AH_TransactionType));

			jobs.Load(@"
	SELECT DISTINCT TOP 10
		" + JobHeaderSchema.Constants.JH_JobNum + @"
	FROM 
		dbo." + JobChargeSchema.Constants.TableName + @"
	INNER JOIN
		dbo." + JobHeaderSchema.Constants.TableName + " ON " + JobHeaderSchema.Constants.PK + " = " + JobChargeSchema.Constants.JR_JH + @"
	LEFT JOIN
		dbo." + AccTransactionLinesSchema.Constants.TableName + " ON " + AccTransactionLinesSchema.Constants.PK + " = " + JobChargeSchema.Constants.JR_AL_APLine + @"
	LEFT JOIN
		dbo." + AccTransactionHeaderSchema.Constants.TableName + " ON " + AccTransactionHeaderSchema.Constants.PK + " = " + AccTransactionLinesSchema.Constants.AL_AH + @"
	WHERE
		" + JobChargeSchema.Constants.JR_GC + @" = @CurrentCompany
		AND " + JobChargeSchema.Constants.JR_OH_CostAccount + @" = @Creditor
		AND " + JobChargeSchema.Constants.JR_APInvoiceNum + @" = @TransactionNum
		AND " + JobChargeSchema.Constants.JR_OSCostAmt + @" <> @OSCostAmt
		AND " + JobHeaderSchema.Constants.JH_ParentTableCode + " <> '" + RatingHeaderSchema.Constants.Prefix + @"'
		AND (" + AccTransactionLinesSchema.Constants.AL_AH + @" IS NULL OR " + AccTransactionLinesSchema.Constants.AL_AH + @" <> @TransactionHeaderToExcludePK)
		AND (" + AccTransactionHeaderSchema.Constants.AH_TransactionType + @" IS NULL OR " + AccTransactionHeaderSchema.Constants.AH_TransactionType + @" IN (@TransactionType, @APTransactionType, @UATransactionType))
	ORDER BY " + JobHeaderSchema.Constants.JH_JobNum,
				 parameters);

			if (jobs.Count > 0)
			{
				result = true;

				var jobsBuilder = new ZStringBuilder();
				foreach (DynamicBusinessObject jobNumber in jobs)
				{
					jobsBuilder.Append((ZString)jobNumber[JobHeaderSchema.Constants.JH_JobNum]);
				}
				jobNumbers = jobsBuilder.ToStringWithDelimiterBetweenAppends(", ");
			}

			return result;
		}

		static ZQuery GetTransactionFilter(ZString transactionNum, ZGuid orgPK, ZGuid transactionHeaderToExcludePK, ZString ledger, ZString transactionType, bool ignoreTransactionType = false)
		{
			return GetTransactionFilter(transactionNum, orgPK, transactionHeaderToExcludePK, new ZString[] { ledger }, new ZString[] { transactionType }, ignoreTransactionType);
		}

		static ZQuery GetTransactionFilter(ZString transactionNum, ZGuid orgPK, ZGuid transactionHeaderToExcludePK, ZString[] ledgers, ZString[] transactionTypes, bool ignoreTransactionType = false)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledgers);
			if (!ignoreTransactionType)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transactionTypes);
			}
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transactionNum);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OH, orgPK);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			if (transactionHeaderToExcludePK.IsValid)
			{
				query.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transactionHeaderToExcludePK);
			}
			return query;
		}

		public static ZQuery GetTransactionFilter(ZGuid transactionBelongsToGroupGuid, ZGuid companyPK, byte transactionCount)
		{
			var filter = new ZQuery();
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, transactionBelongsToGroupGuid);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, companyPK);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, transactionCount);
			return filter;
		}

		public static PreviousSameNumberTransactionDetails APTransactionNumberExists(ZString transactionType, ZString transactionNum, ZGuid orgPK, ZDateTime invoiceDate)
		{
			var result = new PreviousSameNumberTransactionDetails();

			var transactionTypeAsAP = ConvertTransactionTypeToAP(transactionType);
			if (!transactionTypeAsAP.IsEmpty)
			{
				var query = GetTransactionFilter(transactionNum, orgPK, ZGuid.Empty, LedgerTypes.AccountsPayable, transactionTypeAsAP, AccountingConfigurationRegistry.Instance.PayableEnforceUniqueTransactionNumber.Value);
				var queryForDuplicateTransactionNumber = string.Format(CultureInfo.InvariantCulture, @"SELECT {0}, {1} FROM {2} {3} ORDER BY {4} DESC",
					AccTransactionHeaderSchema.Constants.AH_InvoiceDate,
					AccTransactionHeaderSchema.Constants.AH_TransactionCount,
					AccTransactionHeaderSchema.Constants.TableName,
					query.GetAsWhereClause(false),
					AccTransactionHeaderSchema.Constants.AH_InvoiceDate);

				result = FindPreviousTransactionNumber(queryForDuplicateTransactionNumber, new ZSqlParameterCollection(query.Params), invoiceDate, TransactionNumberUseCategory.APTransaction) ?? result;
			}

			return result;
		}

		public static AccTransactionHeader[] GetDuplicateNumbersTransactionList(ZString transactionType, ZString transactionNum, ZGuid orgPK)
		{
			return GetDuplicateNumbersTransactionList(transactionType, transactionNum, orgPK, Guid.Empty);
		}

		// this method provides a list of transactions with duplicate numbers for critical validation error reporting
		// it uses simplified query in comparision to checking that transaction number is unique; as we want to limit it to single DB hit		
		public static AccTransactionHeader[] GetDuplicateNumbersTransactionList(ZString transactionType, ZString transactionNum, ZGuid orgPK, ZGuid transactionHeaderToExcludePK)
		{
			var transactionTypeAsAP = ConvertTransactionTypeToAP(transactionType);
			var transactionTypeAsUA = ConvertTransactionTypeToUA(transactionType);
			var transactionTypeAsIN = ConvertTransactionTypeToIN(ConvertTransactionTypeUAOnlyToIN(transactionTypeAsUA));
			var ledgers = new ZString[] { LedgerTypes.AccountsPayable, LedgerTypes.UnapprovedPayableTransactions, LedgerTypes.IncompleteTransactions };
			var transactionTypes = new ZString[] { transactionTypeAsAP, transactionTypeAsUA, transactionTypeAsIN };
			var query = GetTransactionFilter(transactionNum, orgPK, transactionHeaderToExcludePK, ledgers, transactionTypes);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			return factory.Load(typeof(AccTransactionHeader), query) as AccTransactionHeader[];
		}

		static PreviousSameNumberTransactionDetails? FindPreviousTransactionNumber(ZString queryForDuplicateTransactionNumber, ZSqlParameterCollection parameters, ZDateTime invoiceDate, TransactionNumberUseCategory category)
		{
			var previousInvoices = FindTransactionNumberExists(queryForDuplicateTransactionNumber, parameters);
			if (previousInvoices != null)
			{
				var allowDuplicateInvoiceNumberRule = AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.Value;
				var checkResult = GetDuplicateTransactionNumberCheckResult(allowDuplicateInvoiceNumberRule, invoiceDate, previousInvoices);
				return new PreviousSameNumberTransactionDetails(allowDuplicateInvoiceNumberRule, checkResult.result,
					checkResult.invoiceDate, previousInvoices[0].transactionCount, category);
			}

			return null;
		}

		public static PreviousSameNumberTransactionDetails UATransactionNumberExists(ZString transactionType, ZString transactionNum, ZGuid orgPK, ZGuid transactionHeaderToExcludePK, ZDateTime invoiceDate)
		{
			PreviousSameNumberTransactionDetails? result = null;

			var transactionTypeAsUA = ConvertTransactionTypeToUA(transactionType);
			if (!transactionTypeAsUA.IsEmpty)
			{
				var parameters = new ZSqlParameterCollection();
				var sqlParameters = new UniqueTransactionNumberQueryGeneratorParameters(AccTransactionHeaderSchema.Constants.AH_OH, parameters);

				var queryForDuplicateTransactionNumber = GetQueryForDuplicateTransactionNumber(LedgerTypes.UnapprovedPayableTransactions, transactionTypeAsUA, transactionNum, orgPK, transactionHeaderToExcludePK, sqlParameters);

				result = FindPreviousTransactionNumber(queryForDuplicateTransactionNumber, parameters, invoiceDate, TransactionNumberUseCategory.UATransaction);
				if (result == null)
				{
					result = INTransactionNumberExists(ConvertTransactionTypeUAOnlyToIN(transactionTypeAsUA), transactionNum, orgPK, transactionHeaderToExcludePK, invoiceDate, true);
				}
			}
			return result ?? new PreviousSameNumberTransactionDetails();
		}

		public static PreviousSameNumberTransactionDetails PATransactionNumberExists(ZString transactionType, ZString transactionNum, ZGuid orgPK, ZGuid transactionHeaderToExcludePK, ZDateTime invoiceDate)
		{
			var result = new PreviousSameNumberTransactionDetails();

			if (!transactionNum.IsEmpty)
			{
				var parameters = new ZSqlParameterCollection();

				var linkedToApprovalRequestJoinClause =
	@"		LEFT JOIN dbo.GenApprovalRequest ON AH_PK = XP_ParentID AND XP_SubSystem = @SubSystem AND XP_ApprovalType = @ApprovalType";
				parameters.Add(ZSqlParameter.New("@SubSystem", Constants.GenApprovalRequestSubSystem.Accounting, GenApprovalRequestSchema.XP_SubSystem));
				parameters.Add(ZSqlParameter.New("@ApprovalType", Constants.GenApprovalRequestApprovalType.TransactionPendingAllocation, GenApprovalRequestSchema.XP_ApprovalType));

				var linkedToApprovalRequestWhereClause = string.Format(CultureInfo.InvariantCulture,
@"		AND (XP_ParentID IS NULL OR XP_ApprovalStatus NOT IN ('{0}', '{1}'))", GenApprovalRequestApprovalStatus.Cancelled, GenApprovalRequestApprovalStatus.Rejected);

				var sqlParameters = new UniqueTransactionNumberQueryGeneratorParameters(AccTransactionHeaderSchema.Constants.AH_OH, parameters);
				sqlParameters.additionalFromClause = linkedToApprovalRequestJoinClause;
				sqlParameters.additionalWhereClause = linkedToApprovalRequestWhereClause;

				var queryForDuplicateTransactionNumber = GetQueryForDuplicateTransactionNumber(LedgerTypes.TransactionsPendingAllocation, TransactionTypeToPA(transactionType), transactionNum, orgPK, transactionHeaderToExcludePK, sqlParameters);

				result = FindPreviousTransactionNumber(queryForDuplicateTransactionNumber, parameters, invoiceDate, TransactionNumberUseCategory.PATransaction) ?? result;
			}

			return result;
		}

		public static PreviousSameNumberTransactionDetails INTransactionNumberExists(ZString transactionType, ZString transactionNum, ZGuid orgPK, ZGuid transactionHeaderToExcludePK, ZDateTime invoiceDate)
		{
			return INTransactionNumberExists(transactionType, transactionNum, orgPK, transactionHeaderToExcludePK, invoiceDate, false);
		}

		static PreviousSameNumberTransactionDetails INTransactionNumberExists(ZString transactionType, ZString transactionNum, ZGuid orgPK, ZGuid transactionHeaderToExcludePK, ZDateTime invoiceDate, bool linkedToApprovalRequest)
		{
			var result = new PreviousSameNumberTransactionDetails();

			if (!(transactionNum.IsEmpty || linkedToApprovalRequest && !AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Value))
			{
				var parameters = new ZSqlParameterCollection();

				var linkedToApprovalRequestJoinClause = "";
				var linkedToApprovalRequestWhereClause = "";
				if (AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.Value)
				{
					linkedToApprovalRequestJoinClause =
	@"		LEFT JOIN dbo.GenApprovalRequest ON AH_PK = XP_ParentID AND XP_SubSystem = @SubSystem AND XP_ApprovalType = @ApprovalType";
					parameters.Add(ZSqlParameter.New("@SubSystem", Constants.GenApprovalRequestSubSystem.Accounting, GenApprovalRequestSchema.XP_SubSystem));
					parameters.Add(ZSqlParameter.New("@ApprovalType", Constants.GenApprovalRequestApprovalType.APInvoiceCharges, GenApprovalRequestSchema.XP_ApprovalType));

					if (linkedToApprovalRequest)
					{
						linkedToApprovalRequestWhereClause = string.Format(CultureInfo.InvariantCulture,
	@"		AND XP_ParentID IS NOT NULL
		AND XP_ApprovalStatus NOT IN ('{0}', '{1}')", GenApprovalRequestApprovalStatus.Cancelled, GenApprovalRequestApprovalStatus.Rejected);
					}
					else
					{
						linkedToApprovalRequestWhereClause = "AND XP_ParentID IS NULL";
					}
				}

				var sqlParameters = new UniqueTransactionNumberQueryGeneratorParameters(AccTransactionHeaderSchema.Constants.AH_OH, parameters);
				sqlParameters.additionalFromClause = linkedToApprovalRequestJoinClause;
				sqlParameters.additionalWhereClause = linkedToApprovalRequestWhereClause;

				var queryForDuplicateTransactionNumber = GetQueryForDuplicateTransactionNumber(LedgerTypes.IncompleteTransactions, ConvertTransactionTypeToIN(transactionType), transactionNum, orgPK, transactionHeaderToExcludePK, sqlParameters);

				result = FindPreviousTransactionNumber(queryForDuplicateTransactionNumber, parameters, invoiceDate, TransactionNumberUseCategory.INTransaction) ?? result;
			}

			return result;
		}

		public static void AddTransactionNumInfoNotification(PreviousSameNumberTransactionDetails previousDuplicateTransactionNumberDetails, string defaultErrorMsg, InvoicingBase parent)
		{
			if (previousDuplicateTransactionNumberDetails.HasNotification)
			{
				var skipDefaultErrorLedgerTypes = new[] { LedgerTypes.AccountsPayable, LedgerTypes.TransactionsPendingAllocation };

				if (skipDefaultErrorLedgerTypes.Contains((string)parent.AH_Ledger))
				{
					parent.AH_TransactionNumInfo.AddNotification(previousDuplicateTransactionNumberDetails.NotificationType, previousDuplicateTransactionNumberDetails.NotificationMessage);
					if (previousDuplicateTransactionNumberDetails.IsDuplicateTransactionNumberAllowed)
					{
						parent.SetPreviousSameNumberTransactionDetails(previousDuplicateTransactionNumberDetails);
					}
				}
				else
				{
					parent.AH_TransactionNumInfo.AddError(defaultErrorMsg);
				}
			}
		}

		internal static ZString GetQueryForDuplicateTransactionNumber(ZString ledger, ZString transactionType, ZString transactionNum, ZGuid orgPK, ZGuid transactionHeaderToExcludePK,
			UniqueTransactionNumberQueryGeneratorParameters sqlParameters)
		{
			ZString result = ZString.Empty;
			if (!transactionType.IsEmpty)
			{
				var query = GetTransactionFilter(transactionNum, orgPK, transactionHeaderToExcludePK, ledger, transactionType);

				sqlParameters.parameters.AddRange(query.Params);

				result = string.Format(CultureInfo.InvariantCulture, @"SELECT {0}, {1} FROM {2} {3} {4} {5} ORDER BY {6} DESC",
					AccTransactionHeaderSchema.Constants.AH_InvoiceDate,
					AccTransactionHeaderSchema.Constants.AH_TransactionCount,
					AccTransactionHeaderSchema.Constants.TableName,
					sqlParameters.additionalFromClause,
					query.GetAsWhereClause(false),
					sqlParameters.additionalWhereClause,
					AccTransactionHeaderSchema.Constants.AH_InvoiceDate);
			}

			return result;
		}

		internal static ZString GetQueryForUniqueTransactionNumber(ZString ledger, ZString transactionType, ZString transactionNum, ZGuid orgPK, ZGuid transactionHeaderToExcludePK,
			UniqueTransactionNumberQueryGeneratorParameters sqlParameters)
		{
			if (transactionType.IsEmpty)
			{
				return ZString.Empty;
			}

			var query = GetTransactionFilter(transactionNum, orgPK, transactionHeaderToExcludePK, ledger, transactionType);

			sqlParameters.parameters.AddRange(query.Params);

			string sql = string.Format(@"SELECT {0} FROM {1} {2} {3} {4}",
				sqlParameters.selectClause,
				AccTransactionHeaderSchema.Constants.TableName,
				sqlParameters.additionalFromClause,
				query.GetAsWhereClause(false),
				sqlParameters.additionalWhereClause);
			return sql;
		}

		static (ZDateTime invoiceDate, ZByte transactionCount)[] FindTransactionNumberExists(ZString queryForDuplicateTransactionNumber, ZSqlParameterCollection parameters)
		{
			if (queryForDuplicateTransactionNumber.IsEmpty)
			{
				return null;
			}

			var headers = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			headers.Load(queryForDuplicateTransactionNumber, parameters);

			if (headers.Count > 0)
			{
				return headers.Select(x => ((ZDateTime)x[AccTransactionHeaderSchema.Constants.AH_InvoiceDate],
						(ZByte)x[AccTransactionHeaderSchema.Constants.AH_TransactionCount])).ToArray();
			}
			else
			{
				return null;
			}
		}

		static (DuplicateTransactionNumberCheckResult result, ZDateTime invoiceDate) GetDuplicateTransactionNumberCheckResult(string allowDuplicateInvoiceNumberRule, ZDateTime invoiceDate, (ZDateTime invoiceDate, ZByte transactionCount)[] previousInvoices)
		{
			if (invoiceDate.IsEmpty || !invoiceDate.IsValid)
			{
				return (DuplicateTransactionNumberCheckResult.UnableToCheck, ZDateTime.Empty);
			}

			if (allowDuplicateInvoiceNumberRule == AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD)
			{
				foreach (var previousInvoice in previousInvoices)
				{
					if (previousInvoice.invoiceDate.Date > invoiceDate.AddMonths(-DuplicateInvoiceNumberPeriodMonths).Date &&
						previousInvoice.invoiceDate.Date < invoiceDate.AddMonths(DuplicateInvoiceNumberPeriodMonths).Date)
					{
						return (DuplicateTransactionNumberCheckResult.InsideOfPeriod, previousInvoice.invoiceDate);
					}
				}
			}
			else if (allowDuplicateInvoiceNumberRule == AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL)
			{
				foreach (var previousInvoice in previousInvoices)
				{
					if (previousInvoice.invoiceDate.Year == invoiceDate.Year)
					{
						return (DuplicateTransactionNumberCheckResult.InsideOfPeriod, previousInvoice.invoiceDate);
					}
				}
			}
			else
			{
				throw new ArgumentOutOfRangeException(nameof(allowDuplicateInvoiceNumberRule), allowDuplicateInvoiceNumberRule, "unknown value");
			}

			if (Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed)
			{
				return (DuplicateTransactionNumberCheckResult.OutsideOfPeriod, previousInvoices[0].invoiceDate);
			}

			return (DuplicateTransactionNumberCheckResult.OutsideButNoPermissions, previousInvoices[0].invoiceDate);
		}

		internal class UniqueTransactionNumberQueryGeneratorParameters
		{
			public UniqueTransactionNumberQueryGeneratorParameters(ZString selectClause, ZSqlParameterCollection parameters)
			{
				this.selectClause = selectClause;
				this.parameters = parameters;
			}

			public ZString selectClause;
			public ZString additionalFromClause;
			public ZString additionalWhereClause;
			public ZSqlParameterCollection parameters;
		}

		static ZString ConvertTransactionTypeToUA(ZString transactionType)
		{
			var result = ZString.Empty;
			switch (transactionType)
			{
				case TransactionTypes.InvoicePendingAllocation:
				case TransactionTypes.Invoice:
				case TransactionTypes.UAInvoice:
					result = TransactionTypes.UAInvoice;
					break;
				case TransactionTypes.CreditNotePendingAllocation:
				case TransactionTypes.CreditNote:
				case TransactionTypes.UACreditNote:
					result = TransactionTypes.UACreditNote;
					break;
			}

			return result;
		}

		static ZString ConvertTransactionTypeToAP(ZString transactionType)
		{
			var result = ZString.Empty;
			switch (transactionType)
			{
				case TransactionTypes.InvoicePendingAllocation:
				case TransactionTypes.UAInvoice:
				case TransactionTypes.Invoice:
					result = TransactionTypes.Invoice;
					break;
				case TransactionTypes.CreditNotePendingAllocation:
				case TransactionTypes.UACreditNote:
				case TransactionTypes.CreditNote:
					result = TransactionTypes.CreditNote;
					break;
				case TransactionTypes.AdjustmentNote:
					result = TransactionTypes.AdjustmentNote;
					break;
			}

			return result;
		}

		static ZString TransactionTypeToPA(ZString transactionType)
		{
			var result = ZString.Empty;
			switch (transactionType)
			{
				case TransactionTypes.UAInvoice:
				case TransactionTypes.Invoice:
				case TransactionTypes.InvoicePendingAllocation:
					result = TransactionTypes.InvoicePendingAllocation;
					break;
				case TransactionTypes.UACreditNote:
				case TransactionTypes.CreditNote:
				case TransactionTypes.CreditNotePendingAllocation:
					result = TransactionTypes.CreditNotePendingAllocation;
					break;
			}

			return result;
		}

		static ZString ConvertTransactionTypeUAOnlyToIN(ZString transactionType)
		{
			var result = ZString.Empty;
			switch (transactionType)
			{
				case TransactionTypes.UAInvoice:
					result = TransactionTypes.IncompleteInvoice;
					break;
				case TransactionTypes.UACreditNote:
					result = TransactionTypes.IncompleteCreditNote;
					break;
			}

			return result;
		}

		static ZString ConvertTransactionTypeToIN(ZString transactionType)
		{
			var result = ZString.Empty;
			switch (transactionType)
			{
				case TransactionTypes.Invoice:
				case TransactionTypes.IncompleteInvoice:
					result = TransactionTypes.IncompleteInvoice;
					break;
				case TransactionTypes.CreditNote:
				case TransactionTypes.IncompleteCreditNote:
					result = TransactionTypes.IncompleteCreditNote;
					break;
				case TransactionTypes.AdjustmentNote:
				case TransactionTypes.IncompleteAdjustmentNote:
					result = TransactionTypes.IncompleteAdjustmentNote;
					break;
			}

			return result;
		}

		internal static ZString ConvertTransactionTypeFromAPToINSafe(ZString transactionType)
		{
			var result = ConvertTransactionTypeToIN(transactionType);

			return result.IsEmpty ? transactionType : result;
		}

		static ZString ConvertTransactionTypeFromINToAP(ZString transactionType)
		{
			var result = ZString.Empty;
			switch (transactionType)
			{
				case TransactionTypes.IncompleteInvoice:
				case TransactionTypes.Invoice:
					result = TransactionTypes.Invoice;
					break;
				case TransactionTypes.IncompleteCreditNote:
				case TransactionTypes.CreditNote:
					result = TransactionTypes.CreditNote;
					break;
				case TransactionTypes.IncompleteAdjustmentNote:
				case TransactionTypes.AdjustmentNote:
					result = TransactionTypes.AdjustmentNote;
					break;
			}

			return result;
		}

		internal static ZString ConvertTransactionTypeFromINToAPSafe(ZString transactionType)
		{
			var result = ConvertTransactionTypeFromINToAP(transactionType);

			return result.IsEmpty ? transactionType : result;
		}

		internal static ZString ConvertTransactionTypeFromINToUASafe(ZString transactionType)
		{
			var result = ConvertTransactionTypeFromINToAP(transactionType);
			result = ConvertTransactionTypeToUA(result);

			return result.IsEmpty ? transactionType : result;
		}

		#endregion

		#region ChequeBook Number Allocation

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public ZDecimal GetNextChequeNumberFromActiveChequeBookWithLock(AccChequeBook chequeBook, BusinessObjectFactory factory)
		{
			if (chequeBook == null || factory == null)
			{
				return 0;
			}

			if (!factory.IsInTransaction)
			{
				ErrorReporter.ReportOnce("AccountingUtils.GetNextChequeNumberFromActiveChequeBookWithLock", "Current check number should be increased inside transaciton.");
			}

			string selectOrInsertSql = string.Format(@"
					DECLARE @CurrentValue decimal
					DECLARE @MaxValue decimal
					SELECT @CurrentValue = AK_CurrentNo, @MaxValue = AK_LastNo FROM dbo.AccChequeBook WITH(UPDLOCK, ROWLOCK)             
					WHERE AK_PK = @PK AND AK_IsActive = 1

					IF (@CurrentValue IS NULL)  
						RAISERROR('{0}', 16, 1)
					ELSE
						BEGIN
						IF (@CurrentValue + 1 > @MaxValue)
						BEGIN
							UPDATE dbo.AccChequeBook
							SET AK_IsActive = 0,
								AK_CurrentNo = @CurrentValue + 1,
								AK_SystemLastEditTimeUtc = GETUTCDATE(),
								AK_SystemLastEditUser = @SystemLastEditUser
							WHERE AK_PK = @PK
							SELECT @CurrentValue
						END
						ELSE
						BEGIN
							UPDATE dbo.AccChequeBook
							SET AK_CurrentNo = @CurrentValue + 1,
								AK_SystemLastEditTimeUtc = GETUTCDATE(),
								AK_SystemLastEditUser = @SystemLastEditUser
							WHERE AK_PK = @PK
							SELECT @CurrentValue
						END
					END",
					AccountingConstants.ChequeNumberAllocationErrorMessages.ChequeBookIsFullExceptionMessage);

			var command = ((IDbConnected)factory).Connection.Command(selectOrInsertSql);
			command.AddParameterBasedOnDbColumn("@PK", chequeBook.PK.ToGuid(), AccChequeBookSchema.PK);
			command.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);

			try
			{
				object resultObject = command.ExecuteScalar();
				ZDecimal result = 0;

				if (resultObject != null)
				{
					result = Convert.ToDecimal(resultObject, CultureInfo.InvariantCulture);
				}

				return result;
			}
			catch (System.Data.Common.DbException e)
			{
				if (string.Compare(
					e.Message,
					AccountingConstants.ChequeNumberAllocationErrorMessages.ChequeBookIsFullExceptionMessage,
					StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					ZDataException innerEx = new ZDataException(new Exception(AccountingConstants.ChequeNumberAllocationErrorMessages.ChequeBookIsFullExceptionMessage), ((INeedRow)chequeBook).Row, ((IDbConnected)factory).Connection);
					AllocationChequeBookException ex = new AllocationChequeBookException(innerEx, factory);
					throw ex;
				}
				else
				{
					DbErrorType chequeNumberErrorType = new DbErrorHandler(e, ((IDbConnected)factory).Connection).ExceptionType;

					if (chequeNumberErrorType == DbErrorType.TimeoutExpired || chequeNumberErrorType == DbErrorType.DeadlockError)
					{
						ZDataException innerEx = new ZDataException(new Exception(AccountingConstants.ChequeNumberAllocationErrorMessages.ChequeBookIsBusyExceptionMessage), ((INeedRow)chequeBook).Row, ((IDbConnected)factory).Connection);
						AllocationSaveException ex = new AllocationSaveException(innerEx, factory);
						throw ex;
					}
				}

				throw;
			}
		}

		#endregion

		#region Arithmetical operations

		public static decimal Round(decimal amount, ZString currencyNK)
		{
			if (currencyNK.IsValid)
			{
				return Utilities.Round(amount, new Currency(currencyNK).Decimals);
			}
			else
			{
				return amount;
			}
		}

		public static decimal Round(decimal amount, RefCurrency currency)
		{
			ZDecimal result = amount;
			if (currency != null)
			{
				result = Round(amount, currency.RX_Code);
			}
			return result;
		}

		#endregion

		#region CompanyFilter

		public static ZQuery GenerateCompanyFilter(SchemaColumn columnToFilter, BusinessObjectFactory factory)
		{
			return new ZQuery(columnToFilter, GlbCompany.CurrentCompany.Branches.GetPKs());
		}

		#endregion

		#region Comparison operations and sorting

		public int CompareTransactionsBySettlementGroupAndOrganisation(TransactionHeader transaction1, TransactionHeader transaction2)
		{
			if (transaction1 != null && transaction2 != null)
			{
				if (transaction1.OH_APSettlementGroup.IsEmpty)
				{
					if (transaction2.OH_APSettlementGroup.IsEmpty)
					{
						//Settlement group is not specified. Compare by AH_OH
						return CompareTransactionsByAH_OH(transaction1, transaction2);
					}
					else
					{
						return -1;
					}
				}
				else
				{
					if (transaction2.OH_APSettlementGroup.IsEmpty)
					{
						return 1;
					}
					else
					{
						return transaction1.OH_APSettlementGroup.CompareTo(transaction2.OH_APSettlementGroup);
					}
				}
			}
			else
			{
				return 0;
			}
		}

		int CompareTransactionsByAH_OH(TransactionHeader transaction1, TransactionHeader transaction2)
		{
			if (transaction1.AH_OH.IsEmpty)
			{
				if (transaction2.AH_OH.IsEmpty)
				{
					return 0;
				}
				else
				{
					return -1;
				}
			}
			else
			{
				if (transaction2.AH_OH.IsEmpty)
				{
					return 1;
				}
				else
				{
					return transaction1.AH_OH.CompareTo(transaction2.AH_OH);
				}
			}
		}

		public List<List<TransactionHeader>> SplitTransactionBatchOnCollectionByChequeBookParameter(ICollection transactions, BusinessObjectFactory factory)
		{
			var unsortedPaymentCollection = GetPaymentsFromMixedTransactionsCollection(transactions, factory).Cast<Payment>().
				Where(x => ((IChequeNumberAutoAllocation)x).IsAutoAllocationEnabled).ToList();

			var result = new List<List<TransactionHeader>>();
			if (unsortedPaymentCollection.Any())
			{
				var paymentsByBook = unsortedPaymentCollection.GroupBy(x => x.ChequeBook);
				foreach (var payments in paymentsByBook)
				{
					result.Add(payments.OrderBy(x => x.Header != null ? x.Header.OH_Code : ZString.Empty).ToList<TransactionHeader>());
				}
			}
			return result;
		}

		List<TransactionHeader> GetPaymentsFromMixedTransactionsCollection(ICollection transactions, BusinessObjectFactory factory)
		{
			var payments = new List<TransactionHeader>();

			if (transactions is TransactionCreatorHashtable)
			{
				foreach (PaymentApprovalBase approval in ((TransactionCreatorHashtable)transactions).GetAllAPPaymentApprovals())
				{
					if (approval.NewPayment != null && ((IChequeNumberAutoAllocation)approval.NewPayment).IsAutoAllocationEnabled)
					{
						payments.Add(approval.NewPayment);
					}
				}
			}
			else if (transactions is APPaymentApprovalWithoutAuthorisationCollection)
			{
				foreach (APPaymentApprovalWithoutAuthorisation paymentApproval in transactions)
				{
					if (paymentApproval.NewPayment != null && ((IChequeNumberAutoAllocation)paymentApproval.NewPayment).IsAutoAllocationEnabled)
					{
						payments.Add(paymentApproval.NewPayment);
					}
				}
			}
			else if (transactions is TransactionHeaderCollection)
			{
				payments = ((TransactionHeaderCollection)transactions).Cast<TransactionHeader>().ToList();
			}

			return payments;
		}

		#endregion

		#region Filter Constants

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be used as constant in printing")]
		public static class AccountingDocumentTitles
		{
			public const string ReceiptMatching = "Receipt Matching";
			public const string ReceiptJournal = "Receipt Journal";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded filter types")]
		public static class OrganisationFilterTypes
		{
			public const string Creditor = "Creditor";
			public const string Debtor = "Debtor";
			public const string CreditorDebtor = "Creditor/Debtor";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded filter types")]
		public static class NumberFilterTypes
		{
			public const string None = "None";
			public const string All = "All";
			public const string Common = "Common";
			public const string JobNumber = "Job #";
			public const string TransactionNumber = "Transaction #";
			public const string ConsolidationNumber = "Job Invoice #";
			public const string ChequeReferenceNumber = "Check/Reference #";
			public const string DepositBatchNumber = "Deposit Batch #";
			public const string DDRBatchNumber = "DDR Batch #";
			public const string MatchGroupNumber = "Match Group #";
			public const string GovtComplianceNumber = "Compliance #";
			public const string InternalReferenceNumber = "Internal Reference #";
			public const string SupplierCostReference = "Supplier Cost Reference";
			public const string CollectionBatchNumber = "Collection Batch #";
			public const string OrderNumber = "Order #";
			public const string InvoiceTransactionReference = "Invoice Transaction Reference";
			public const string SupportingDocumentNumber = "Supporting Document Number";
			public const string InvoiceRemittanceReference = "Invoice Remittance Reference";
			public const string InvoiceRemittanceType = "Invoice Remittance Type";
			public const string DocumentNumber = "Document #";
			public const string EInvoicingBatchNumber = "E-Reporting Batch";
			public const string EInvoicingGovernmentAllocatedNumber = "E-Reporting Govt #";
			public const string EInvoicingeHubAllocatedNumber = "E-Reporting eHub #";
			public const string EInvoicingAuthorisationNumber = "E-Reporting Auth #";
			public const string CollectionBatchType = "Collection Batch Type";
			public const string CashAdvanceNumber = "Advance Payment #";
			public const string GovernmentAllocatedID = "Government Allocated Number";
			public const string RelatedDisbursementTransactions = "Related Disbursement Transactions";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded filter types")]
		public static class DsbJobCloseBatchFilterTypes
		{
			public const string BatchNumber = "Batch Number";
			public const string ApprovingUser = "Approving User";
			public const string ApprovalStatus = "Approval Status";
			public const string ApprovalDate = "Approval Date";
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Filter types")]
		public static class DateFilterTypes
		{
			public const string None = "None";
			public const string All = "All";
			public const string PostDate = "Post Date";
			public const string ReverseDate = "Reverse Date";
			public const string TransactionDate = "Transaction Date";
			public const string DueDate = "Due Date";
			public const string MatchDate = "Match Date";
			public const string DateShownInStatement = "Shown In Statement";
			public const string ComplianceDocDate = "Compliance Doc Date";
			public const string RequisitionDate = "Payment Requested Date";
			public const string RequisitionStatus = "Payment Requisition Status";
			public const string FullyPaidDate = "Fully Paid Date";
			public const string DocumentReceivedDate = "Document Received Date";
			public const string DateUploaded = "Date Uploaded";
		}

		public static class PaymentStatusTypes
		{
			public const string All = "ALL";
			public const string Unpaid = "UNPAID";
			public const string Paid = "PAID";
			public const string PartPaid = "PARTPAID";
		}

		public static class CollectionBatchStatusTypes
		{
			public const string All = "ALL";
			public const string IncludeInActiveBatch = "IAB";
			public const string NotIncludeInActiveBatch = "NAB";
		}

		public static class ModesAndTypesFilterTypes
		{
			[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be used as constant in printing")]
			public const string TransactionType = "Transaction Type";
			public static readonly ResourceString TransactionTypeDescription = ResString.GetMultilingualString("Accounting|TransactionFilter|TransactionType", "Transaction Type");
		}

		public static ZQuery GetPaymentStatusFilter(string paymentStatus)
		{
			ZQuery result = new ZQuery();

			if (paymentStatus == PaymentStatusTypes.Paid)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.NotEqual, null);
			}
			if (paymentStatus == PaymentStatusTypes.Unpaid)
			{
				result.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, null);
			}
			else if (paymentStatus == AccountingUtils.PaymentStatusTypes.PartPaid)
			{
				ZString filter = "{OutstandingAmount} <> ( {InvoiceAmount} + {GSTAmount} )";
				filter = filter.Replace("{OutstandingAmount}", AccTransactionHeaderSchema.Constants.AH_OutstandingAmount);
				filter = filter.Replace("{InvoiceAmount}", AccTransactionHeaderSchema.Constants.AH_InvoiceAmount);
				filter = filter.Replace("{GSTAmount}", AccTransactionHeaderSchema.Constants.AH_GSTAmount);

				result.AddFilterAndZSQLParameterCollection(filter, new ZSqlParameterCollection());
				result.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, null);
			}

			return result;
		}

		#endregion

		#region TransactionTypeList

		[SuppressWeaklyTypedCollectionMessage]
		public static IList GetTransactionTypeList(ZString ledger)
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(String.Empty, Res.GetString("Accounting|GenericTransactionFilter|AllTransactionTypes", "All Transaction Types"));
			switch (ledger)
			{
				case LedgerTypes.AccountsReceivable:
				case LedgerTypes.AccountsPayable:
					list.AddPair(TransactionTypes.AdjustmentNote, Res.GetString("Accounting|GenericTransactionFilter|AdjustmentNote", "Adjustment Note"));
					list.AddPair(TransactionTypes.Contra, Res.GetString("Accounting|GenericTransactionFilter|Contra", "Contra"));
					list.AddPair(TransactionTypes.CreditNote, Res.GetString("Accounting|GenericTransactionFilter|CreditNote", "Credit Note"));
					list.AddPair(TransactionTypes.Discount, Res.GetString("Accounting|GenericTransactionFilter|Discount", "Discount"));
					list.AddPair(TransactionTypes.ExchangeDifference, Res.GetString("Accounting|GenericTransactionFilter|ExchangeDifference", "Exchange Difference"));
					list.AddPair(TransactionTypes.Invoice, Res.GetString("Accounting|GenericTransactionFilter|Invoice", "Invoice"));
					list.AddPair(TransactionTypes.Journal, Res.GetString("Accounting|GenericTransactionFilter|Journal", "Journal"));
					list.AddPair(TransactionTypes.Overpayment, Res.GetString("Accounting|GenericTransactionFilter|Overpayment", "Overpayment"));
					list.AddPair(TransactionTypes.Payment, Res.GetString("Accounting|GenericTransactionFilter|Payment", "Payment"));
					list.AddPair(TransactionTypes.Receipt, Res.GetString("Accounting|GenericTransactionFilter|Receipt", "Receipt"));
					list.AddPair(TransactionTypes.Transfer, Res.GetString("Accounting|GenericTransactionFilter|Transfer", "Transfer"));
					break;

				case LedgerTypes.CashBook:
					list.AddPair(TransactionTypes.Transfer, Res.GetString("Accounting|GenericTransactionFilter|Transfer", "Transfer"));
					list.AddPair(TransactionTypes.ExchangeDifference, Res.GetString("Accounting|GenericTransactionFilter|CurrencyAdjustment", "Currency Adjustment"));
					list.AddPair(TransactionTypes.DirectPayment, Res.GetString("Accounting|GenericTransactionFilter|DirectPayment", "Direct Payment"));
					list.AddPair(TransactionTypes.DirectReceipt, Res.GetString("Accounting|GenericTransactionFilter|DirectReceipt", "Direct Receipt"));
					break;

				case LedgerTypes.JobCosting:
					list.AddPair(TransactionTypes.Journal, Res.GetString("Accounting|GenericTransactionFilter|Journal", "Journal"));
					list.AddPair(TransactionLineTypes.WIP, Res.GetString("Accounting|GenericTransactionFilter|WorkInProgress", "Work In Progress"));
					list.AddPair(TransactionLineTypes.Accrual, Res.GetString("Accounting|GenericTransactionFilter|Accrual", "Accrual"));
					list.AddPair(TransactionTypes.JobRevenueJournal, Res.GetString("Accounting|GenericTransactionFilter|JobRevenueJournal", "Job Revenue Journal"));
					break;

				case LedgerTypes.General:
					list.AddPair(TransactionTypes.GLStandardJournal, Res.GetString("Accounting|GenericTransactionFilter|GeneralJournal", "General Journal"));
					list.AddPair(TransactionTypes.GLAutoJournal, Res.GetString("Accounting|GenericTransactionFilter|AutoJournal", "Auto Journal"));
					list.AddPair(TransactionTypes.GLReversingJournal, Res.GetString("Accounting|GenericTransactionFilter|ReverseJournal", "Reverse Journal"));
					list.AddPair(TransactionTypes.GLNoteJournal, Res.GetString("Accounting|GenericTransactionFilter|NoteJournal", "Note Journal"));
					break;

				case "":
					list.AddPair(TransactionTypes.AdjustmentNote, Res.GetString("Accounting|GenericTransactionFilter|AdjustmentNote", "Adjustment Note"));
					list.AddPair(TransactionTypes.Contra, Res.GetString("Accounting|GenericTransactionFilter|Contra", "Contra"));
					list.AddPair(TransactionTypes.CreditNote, Res.GetString("Accounting|GenericTransactionFilter|CreditNote", "Credit Note"));
					list.AddPair(TransactionTypes.Discount, Res.GetString("Accounting|GenericTransactionFilter|Discount", "Discount"));
					list.AddPair(TransactionTypes.ExchangeDifference, Res.GetString("Accounting|GenericTransactionFilter|ExchangeDifferenceCurrencyAdjustment", "Exchange Difference / Currency Adjustment"));
					list.AddPair(TransactionTypes.Invoice, Res.GetString("Accounting|GenericTransactionFilter|Invoice", "Invoice"));
					list.AddPair(TransactionTypes.Journal, Res.GetString("Accounting|GenericTransactionFilter|Journal", "Journal"));
					list.AddPair(TransactionTypes.Overpayment, Res.GetString("Accounting|GenericTransactionFilter|Overpayment", "Overpayment"));
					list.AddPair(TransactionTypes.Payment, Res.GetString("Accounting|GenericTransactionFilter|Payment", "Payment"));
					list.AddPair(TransactionTypes.Receipt, Res.GetString("Accounting|GenericTransactionFilter|Receipt", "Receipt"));
					list.AddPair(TransactionTypes.Transfer, Res.GetString("Accounting|GenericTransactionFilter|Transfer", "Transfer"));
					list.AddPair(TransactionTypes.DirectPayment, Res.GetString("Accounting|GenericTransactionFilter|DirectPayment", "Direct Payment"));
					list.AddPair(TransactionTypes.DirectReceipt, Res.GetString("Accounting|GenericTransactionFilter|DirectReceipt", "Direct Receipt"));
					list.AddPair(TransactionLineTypes.WIP, Res.GetString("Accounting|GenericTransactionFilter|WorkInProgress", "Work In Progress"));
					list.AddPair(TransactionLineTypes.Accrual, Res.GetString("Accounting|GenericTransactionFilter|Accrual", "Accrual"));
					list.AddPair(TransactionTypes.JobRevenueJournal, Res.GetString("Accounting|GenericTransactionFilter|JobRevenueJournal", "Job Revenue Journal"));
					list.AddPair(TransactionTypes.GLStandardJournal, Res.GetString("Accounting|GenericTransactionFilter|GeneralJournal", "General Journal"));
					list.AddPair(TransactionTypes.GLAutoJournal, Res.GetString("Accounting|GenericTransactionFilter|AutoJournal", "Auto Journal"));
					list.AddPair(TransactionTypes.GLReversingJournal, Res.GetString("Accounting|GenericTransactionFilter|ReverseJournal", "Reverse Journal"));
					list.AddPair(TransactionTypes.GLNoteJournal, Res.GetString("Accounting|GenericTransactionFilter|NoteJournal", "Note Journal"));
					break;
			}

			return list;
		}

		#endregion

		public static bool IsVietnamCompanyEInvoicingEnabled => GlbCompany.CurrentCompany.Country.Code == CountryCodes.VietNam && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

		public static bool ShouldPortugalStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting(ZString ledgerType, ZString transactionType)
		{
			var transactionTypes = new ZString[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote };
			return GlbCompany.CurrentCompany.Country.Code == CountryCodes.Portugal
					&& ledgerType == LedgerTypes.AccountsReceivable
					&& transactionTypes.Contains(transactionType)
					&& AccountingConfigurationRegistry.Instance.ARStoreAndUseInvoiceIssuerAndRecepientInformationDuringPostWhenPrinting.Value;
		}

		#region Compliance Report

		public static bool DoesCountrySupportSAFTGeneration => ((ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IComplianceReportGUIActionProvider>)?.Get())?.IsCountrySupportGenerateSAFT ?? false;

		#endregion

		#region Matching

		public static bool IsUserCanChangeMatchDateToBackDate(string ledger)
		{
			bool isApplicable = false;
			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
					isApplicable = Env.Security.PayablesAllowMatchDateToBeBackDated.IsAllowed;
					break;
				case LedgerTypes.AccountsReceivable:
					isApplicable = Env.Security.ReceivablesAllowMatchDateToBeBackDated.IsAllowed;
					break;
			}
			return isApplicable;
		}

		public static ReadOnlyCodeDescriptionPairList GetMatchStatusReasonCodeList() =>
		AccountingConfigurationRegistry.Instance.MatchStatusReason.Value.GetCodeDescriptionPairList();

		public static ReadOnlyCodeDescriptionPairList GetMatchStatusList() => AccountingConfigurationRegistry.Instance.MatchStatus.Value.GetCodeDescriptionPairList();

		#endregion

		#region Queries

		public static ZDBOnlySubQuery GetMasterBillSubQueryForJobHeader(SQLComparisonOperator @operator, ZString masterBill)
		{
			var jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
			var oldValue = masterBill;

			var maxLength = Math.Max(Math.Max(JobConsolSchema.JK_MasterBillNum.MaxLength, JobShipmentSchema.JS_HouseBill.MaxLength), CusDecHouseBillSchema.CU_BillNum.MaxLength);

			if (!AnyNumberNotExceedingMaxLength(ref masterBill, maxLength))
			{
				jobSubQuery.IsNoResultQuery = true;
				return jobSubQuery;
			}

			masterBill = oldValue;

			if (AnyNumberNotExceedingMaxLength(ref masterBill, JobConsolSchema.JK_MasterBillNum.MaxLength))
			{
				var shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
				var pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				var consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);

				consolSubQuery.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_MasterBillNum, @operator, masterBill);
				pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
				shipmentSubQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.False);
				shipmentSubQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);
				jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);
			}

			masterBill = oldValue;

			if (AnyNumberNotExceedingMaxLength(ref masterBill, JobShipmentSchema.JS_HouseBill.MaxLength))
			{
				var jobSubQuery2 = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
				var agencyShipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
				agencyShipmentSubQuery.AddToFilter_PossiblyCommaSeparated(JobShipmentSchema.JS_HouseBill, @operator, masterBill);
				agencyShipmentSubQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.True);
				jobSubQuery2.AddSubQuery(JobHeaderSchema.JH_ParentID, agencyShipmentSubQuery, JoinCondition.Or);
				jobSubQuery.AddAsUnionQuery(jobSubQuery2);
			}

			masterBill = oldValue;

			if (AnyNumberNotExceedingMaxLength(ref masterBill, CusDecHouseBillSchema.CU_BillNum.MaxLength))
			{
				var masterBillSubQueryForDeclaration = new ZDBOnlySubQuery(typeof(Bill), CusDecHouseBillSchema.CU_JE);
				masterBillSubQueryForDeclaration.AddToFilter(CusDecHouseBillSchema.CU_BillType, BillTypeList.Codes.MasterBill);
				var masterBillNumFilter = new ZQuery().AddToFilter_PossiblyCommaSeparated(CusDecHouseBillSchema.CU_BillNum, @operator, masterBill.Replace("-", "").Replace(" ", ""));
				masterBillNumFilter.AddToFilter_PossiblyCommaSeparated(JoinCondition.Or, CusDecHouseBillSchema.CU_BillNum, @operator, masterBill);
				masterBillSubQueryForDeclaration.AddToFilter(masterBillNumFilter);

				var jobSubQuery3 = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
				var standAloneDeclarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
				standAloneDeclarationSubQuery.AddToFilter(JobDeclarationSchema.JE_JS, SQLComparisonOperator.Equal, null);
				standAloneDeclarationSubQuery.AddSubQuery(masterBillSubQueryForDeclaration, JoinCondition.And);
				jobSubQuery3.AddSubQuery(JobHeaderSchema.JH_ParentID, standAloneDeclarationSubQuery, JoinCondition.Or);
				jobSubQuery.AddAsUnionQuery(jobSubQuery3);
			}

			masterBill = oldValue;

			if (AnyNumberNotExceedingMaxLength(ref masterBill, JobConsolSchema.JK_MasterBillNum.MaxLength))
			{
				var jobSubQuery4 = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
				var cfsSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
				cfsSubQuery.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_MasterBillNum, @operator, masterBill);
				cfsSubQuery.AddToFilter(JobConsolSchema.JK_IsCFS, SQLComparisonOperator.Equal, true);
				jobSubQuery4.AddSubQuery(JobHeaderSchema.JH_ParentID, cfsSubQuery, JoinCondition.Or);
				jobSubQuery.AddAsUnionQuery(jobSubQuery4);

				var jobSubQuery5 = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);
				jobSubQuery5.AddToFilter(JobHeaderSchema.JH_ParentTableCode, SQLComparisonOperator.Equal, JobConsolSchema.Constants.Prefix);
				var oldGatewayConsolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
				oldGatewayConsolSubQuery.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_MasterBillNum, @operator, masterBill);
				var oldGatewayQuery = new ZDBOnlyQuery(typeof(JobHeader));
				oldGatewayQuery.AddToFilter(JobHeaderSchema.JH_JobNum, SQLComparisonOperator.EndsWith, Constants.GatewaySuffixForJobHeaderDeprecated);
				oldGatewayQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, oldGatewayConsolSubQuery, JoinCondition.And);
				var newGatewayConsolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
				newGatewayConsolSubQuery.AddToFilter(JobConsolSchema.JK_SendingForwarderHandlingType, new[] { AgentStatusList.Codes.GatewayAgent, AgentStatusList.Codes.GatewayAgentWithTariff });
				newGatewayConsolSubQuery.AddToFilter(JoinCondition.Or, JobConsolSchema.JK_ReceivingForwarderHandlingType, new[] { AgentStatusList.Codes.GatewayAgent, AgentStatusList.Codes.GatewayAgentWithTariff });
				newGatewayConsolSubQuery.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_MasterBillNum, @operator, masterBill);
				newGatewayConsolSubQuery.AddToFilter(JobConsolSchema.JK_IsForwarding, true);
				newGatewayConsolSubQuery.AddToFilter(JobConsolSchema.JK_IsCFS, false);
				var gatewayQuery = new ZDBOnlyQuery(typeof(JobHeader));
				gatewayQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, newGatewayConsolSubQuery, JoinCondition.And);
				gatewayQuery.AddToFilter(oldGatewayQuery, JoinCondition.Or);
				jobSubQuery5.AddToFilter(gatewayQuery);
				jobSubQuery.AddAsUnionQuery(jobSubQuery5);
			}

			return jobSubQuery;
		}

		public static ZDBOnlySubQuery GetCoLoadMasterBillSubQueryForJobHeader(SQLComparisonOperator @operator, ZString coLoadMasterBill)
		{
			var jobSubQuery = new ZDBOnlySubQuery(typeof(Job), JobHeaderSchema.PK);

			if (!AnyNumberNotExceedingMaxLength(ref coLoadMasterBill, JobConsolSchema.JK_CoLoadMasterBill.MaxLength))
			{
				jobSubQuery.IsNoResultQuery = true;
			}
			else
			{
				var shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
				var pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				var consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);

				consolSubQuery.AddToFilter_PossiblyCommaSeparated(JobConsolSchema.JK_CoLoadMasterBill, @operator, coLoadMasterBill);
				consolSubQuery.AddToFilter(JoinCondition.And, JobConsolSchema.JK_AgentType, SQLComparisonOperator.Equal, new[] { Constants.AgentType.CoLoad });
				pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
				shipmentSubQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.False);
				shipmentSubQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);
				jobSubQuery.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);
			}
			return jobSubQuery;
		}

		public static ZDBOnlyQuery GetHouseBillQueryForJobHeader(SQLComparisonOperator @operator, ZString houseBill)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobHeader));
			ZDBOnlySubQuery shipmentSubQuery = null;
			ZDBOnlySubQuery jobDeclarationSubQuery = null;
			var oldValue = houseBill;

			if (AnyNumberNotExceedingMaxLength(ref houseBill, JobShipmentSchema.JS_HouseBill.MaxLength))
			{
				shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
				shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(JobShipmentSchema.JS_HouseBill, @operator, houseBill);
				result.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);
			}

			houseBill = oldValue;

			if (AnyNumberNotExceedingMaxLength(ref houseBill, JobDeclarationSchema.JE_HouseBill.MaxLength))
			{
				jobDeclarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
				jobDeclarationSubQuery.AddToFilter_PossiblyCommaSeparated(JobDeclarationSchema.JE_HouseBill, @operator, houseBill);
				result.AddSubQuery(JobHeaderSchema.JH_ParentID, jobDeclarationSubQuery, JoinCondition.Or);
			}

			if (shipmentSubQuery == null && jobDeclarationSubQuery == null)
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}

		public static ZDBOnlyQuery GetCarrierQueryForJobHeader(ZGuid carrierPK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobHeader));

			ZDBOnlyQuery result1 = new ZDBOnlyQuery(typeof(JobHeader));
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			ZDBOnlySubQuery consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
			ZDBOnlySubQuery addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_ShippingLineAddress);
			addressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, carrierPK);
			consolSubQuery.AddSubQuery(addressSubQuery, JoinCondition.And);
			pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);
			shipmentSubQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.False);
			shipmentSubQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);
			result1.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);

			ZDBOnlyQuery result2 = new ZDBOnlyQuery(typeof(JobHeader));
			ZDBOnlySubQuery consolSubQuery2 = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
			ZDBOnlySubQuery addressSubQuery2 = new ZDBOnlySubQuery(typeof(OrgAddress), JobConsolSchema.JK_OA_ShippingLineAddress);
			addressSubQuery2.AddToFilter(OrgAddressSchema.OA_OH, carrierPK);
			consolSubQuery2.AddSubQuery(addressSubQuery2, JoinCondition.And);
			result2.AddSubQuery(JobHeaderSchema.JH_ParentID, consolSubQuery2, JoinCondition.And);

			ZDBOnlyQuery result3 = new ZDBOnlyQuery(typeof(JobHeader));
			ZDBOnlySubQuery shipmentSubQuery3 = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			shipmentSubQuery3.AddToFilter(JobShipmentSchema.JS_OH_DeliveryAgent, carrierPK);
			shipmentSubQuery3.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.True);
			result3.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery3, JoinCondition.And);

			result.AddToFilter(result1);
			result.AddToFilter(result2, JoinCondition.Or);
			result.AddToFilter(result3, JoinCondition.Or);
			return result;
		}

		public static ZDBOnlyQuery GetGatewayJobsQueryForJobHeader(SQLComparisonOperator @operator, ZString shipmentJobNumber)
		{
			var sql = @"{0} in 
(
	Select JH_PK
	From dbo.JobConsol
	Inner Join dbo.JobConShipLink on JN_JK = JK_PK
	Inner Join dbo.JobShipment on JN_JS = JS_PK
	Inner Join dbo.JobHeader on JK_PK = JH_ParentID
	Where 
		JS_UniqueConsignRef {1} {2}
		AND JH_GC = @CompanyPK

	UNION ALL

	Select JH_PK
	From dbo.JobShipment 
	Inner Join dbo.JobHeader on JS_PK = JH_ParentID
	Where
		JS_UniqueConsignRef {1} {2}
		AND JH_GC = @CompanyPK
)";
			return GetJobQueryTunedForMultiJobReferenceSearch(@operator, shipmentJobNumber, JobShipmentSchema.JS_UniqueConsignRef, sql, "@relatedJobNum");
		}

		public static ZDBOnlyQuery GetConsolNumberQueryForJobHeader(SQLComparisonOperator @operator, ZString consolID)
		{
			var sql = @"{0} in 
						(
						Select JH_PK
						From dbo.JobConsol
						Inner Join dbo.JobConShipLink on JN_JK = JK_PK
						Inner Join dbo.JobShipment on JN_JS = JS_PK
						Inner Join dbo.JobHeader on JS_PK = JH_ParentID
						Where JK_UniqueConsignRef {1} {2}
						AND JH_GC = @CompanyPK
						)";

			return GetJobQueryTunedForMultiJobReferenceSearch(@operator, consolID, JobConsolSchema.JK_UniqueConsignRef, sql, "@ConsolID");
		}

		[SuppressMessage("Enterprise", "EDI003", Justification = "Don't have access to the alternatives it wants")]
		public static ZDBOnlyQuery GetContainerNumberQueryForJobHeader(SQLComparisonOperator @operator, ZString value)
		{
			var nonEmptyContainerClause = value.IsEmpty || @operator.IsNegativeSQLOperator() ? string.Empty : "AND JC_ContainerNum <> ''";

			var sql = @"
{0} in
( 
	SELECT JH_PK FROM dbo.JobHeader --CFS
	INNER JOIN dbo.JobShipment ON JH_ParentID = JS_PK
	INNER JOIN dbo.JobConShipLink ON JN_JS = JS_PK
	INNER JOIN dbo.JobContainer ON JC_JK = JN_JK 
	WHERE JC_ContainerNum {1} {2} " + nonEmptyContainerClause + @"
		AND JS_IsCancelled = 0
		AND JS_IsCFSRegistered = 1
		AND JH_GC = @CompanyPK

	UNION ALL

	SELECT JH_PK FROM dbo.JobHeader --CFS Gate Pass and Forwarding Shipments/Bookings
	INNER JOIN dbo.JobShipment ON JH_ParentID = JS_PK
	INNER JOIN dbo.JobPackLines ON JL_JS = JS_PK
	INNER JOIN dbo.JobContainerPackPivot ON J6_JL = JL_PK
	INNER JOIN dbo.JobContainer ON JC_PK = J6_JC 
	WHERE JC_ContainerNum {1} {2} " + nonEmptyContainerClause + @"
		AND JS_IsCancelled = 0
		AND ((JS_IsCFSRegistered = 1 AND JS_TranshipToOtherCFS = 1)
			OR JS_IsForwardRegistered = 1 
			OR JS_IsBooking = 1)
		AND JH_GC = @CompanyPK

	UNION ALL

	SELECT JH_PK FROM dbo.JobHeader --CFS Load List
	INNER JOIN dbo.JobConsol ON JK_PK = JH_ParentID
	INNER JOIN dbo.JobContainer ON JC_JK = JK_PK 
	WHERE JC_ContainerNum {1} {2} " + nonEmptyContainerClause + @"
		AND JK_IsCancelled = 0
		AND JK_IsCFS = 1
		AND JH_GC = @CompanyPK
	
	UNION ALL

	SELECT JH_PK FROM dbo.JobHeader --Local Transport
	INNER JOIN dbo.JobCartage ON JJ_PK = JH_ParentID
	INNER JOIN dbo.JobBookedCtgMove ON EW_JJ = JJ_PK
	INNER JOIN dbo.JobContainer ON JC_PK = EW_JC_Container
	WHERE JC_ContainerNum {1} {2} " + nonEmptyContainerClause + @"
		AND JJ_IsCancelled = 0
		AND JH_GC = @CompanyPK
	
	UNION ALL

	SELECT JH_PK FROM dbo.JobHeader --Customs Declaration
	INNER JOIN dbo.JobDeclaration ON JE_PK = JH_ParentID
	INNER JOIN dbo.CusContainer ON CO_JE = JE_PK 
	WHERE CO_ContainerNumber {1} {2} 
		AND JE_IsCancelled = 0
		AND JH_GC = @CompanyPK
	
	UNION ALL

	SELECT JH_PK FROM dbo.JobHeader --Container Detention
	INNER JOIN dbo.JobContainerMove ON E9_NC = JH_ParentID
	INNER JOIN dbo.RefContainerStock ON R6_PK = E9_R6
	WHERE R6_ContainerNum {1} {2}
		AND JH_GC = @CompanyPK

	UNION ALL

	SELECT JH_PK FROM dbo.JobHeader  --Shipping Booking and Bill of lading
	INNER JOIN dbo.JobShipment ON JH_ParentID = JS_PK
	INNER JOIN dbo.JobContainer ON JC_JS_FCLBookingOnlyLink = JS_PK 
	WHERE JC_ContainerNum {1} {2} " + nonEmptyContainerClause + @"
		AND JS_IsCancelled = 0
		AND JH_GC = @CompanyPK
)";

			return GetJobQueryTunedForMultiJobReferenceSearch(@operator, value, JobContainerSchema.JC_ContainerNum, sql, "@ContainerNumber");
		}

		[SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		static ZDBOnlyQuery GetJobQueryTunedForMultiJobReferenceSearch(SQLComparisonOperator @operator, ZString value, SchemaStringColumn column, string sql, string paramName)
		{
			var query = new ZDBOnlyQuery(typeof(JobHeader));
			if (!AccountingUtils.AnyNumberNotExceedingMaxLength(ref value, column.MaxLength))
			{
				query.IsNoResultQuery = true;
				return query;
			}

			var parameters = GetParametersForMultiSearchOptimizationIfApplicable(@operator, value);

			var anyResult = false;

			foreach (var singleNumber in parameters.multiNumber)
			{
				var innerQuery = new ZDBOnlyQuery(typeof(JobHeader));
				var sQL = string.Format(CultureInfo.InvariantCulture, sql, JobHeaderSchema.PK.Name,
					parameters.usingOptimization ? parameters.operatorString : @operator.ComparisonText(singleNumber),
					parameters.usingOptimization ? singleNumber : (ZString)paramName);
				var sqlParameters = new ZSqlParameterCollection();
				if (!parameters.usingOptimization)
				{
					sqlParameters.Add(paramName, @operator.EscapedSqlValue(singleNumber), column);
				}
				sqlParameters.Add("@CompanyPK", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);
				innerQuery.AddFilterAndZSQLParameterCollection(sQL, sqlParameters);
				anyResult = true;
				query.AddToFilter(innerQuery, @operator.IsNegativeSQLOperator() ? JoinCondition.And : JoinCondition.Or);
			}

			if (!anyResult)
			{
				query.IsNoResultQuery = true;
			}

			return query;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Progrmamatic constant")]
		public static (ZString[] multiNumber, bool usingOptimization, string operatorString) GetParametersForMultiSearchOptimizationIfApplicable(SQLComparisonOperator @operator, ZString value)
		{
			var multiNumber = new ZString[] { value };
			var operatorString = string.Empty;
			var multiSearchSeparator = RawDataRegistry.Instance.MultiSearchSeparator.Value;
			if (!string.IsNullOrEmpty(multiSearchSeparator))
			{
				if (value.Contains(multiSearchSeparator, StringComparison.Ordinal) && (@operator == SQLComparisonOperator.Equal || @operator == SQLComparisonOperator.NotEqual))
				{
					multiNumber = new ZString[] { ("('" + value.Split(multiSearchSeparator).Select(a => DataUtils.EscapeSingleQuotes(a)).Aggregate((x, y) => x + "', '" + y) + "')") };

					if (@operator == SQLComparisonOperator.Equal)
					{
						operatorString = "IN";
					}
					else if (@operator == SQLComparisonOperator.NotEqual)
					{
						operatorString = "NOT IN";
					}
				}
				else
				{
					multiNumber = value.Split(multiSearchSeparator);
				}
			}

			return (multiNumber, !string.IsNullOrEmpty(operatorString), operatorString);
		}

		public static ZDBOnlyQuery GetCustomsEntryNumberQueryForJobHeader(SQLComparisonOperator @operator, ZString customsEntryNo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobHeader));

			if (AnyNumberNotExceedingMaxLength(ref customsEntryNo, CusEntryNumSchema.CE_EntryNum.MaxLength))
			{
				ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
				shipmentSubQuery.AddToFilter(ForwardingShipmentFilterProvider.GetJobShipmentFromEntryNumber(@operator, customsEntryNo));
				result.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);

				ZDBOnlySubQuery declarationSubQuery = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.PK);
				declarationSubQuery.AddToFilter(ObjectFactory.Get<ICustomsFilterProvider>().GetJobDeclarationFromEntryNumber(@operator, customsEntryNo));
				result.AddSubQuery(JobHeaderSchema.JH_ParentID, declarationSubQuery, JoinCondition.Or);
			}
			else
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}

		public static ZDBOnlyQuery GetFlightVoyageNumberAndVesselQueryForJobHeader(SQLComparisonOperator @operator, ZString flightOrVoyageNo, ZString vesselNK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobHeader));

			if (!AnyNumberNotExceedingMaxLength(ref flightOrVoyageNo, JobVoyageSchema.JV_VoyageFlight.MaxLength)
				|| !AnyNumberNotExceedingMaxLength(ref vesselNK, JobVoyageSchema.JV_RV_NKVessel.MaxLength))
			{
				result.IsNoResultQuery = true;
				return result;
			}

			ZDBOnlyQuery result2 = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery conShipSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			ZDBOnlySubQuery consolTransportSubQuery = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			ZDBOnlyQuery consolTransportSubQuery1 = new ZDBOnlyQuery(typeof(Transport));
			ZDBOnlySubQuery sailingSubQuery = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			ZDBOnlySubQuery voyDestinationSubQuery = new ZDBOnlySubQuery(typeof(AutoJobVoyDestination), JobVoyDestinationSchema.PK);
			ZDBOnlySubQuery voySubQuery = new ZDBOnlySubQuery(typeof(AutoJobVoyage), JobVoyageSchema.PK);

			if (!(vesselNK == ""))
			{
				voySubQuery.AddToFilter_PossiblyCommaSeparated(JobVoyageSchema.JV_RV_NKVessel, @operator, vesselNK);
			}
			voySubQuery.AddToFilter_PossiblyCommaSeparated(JobVoyageSchema.JV_VoyageFlight, @operator, flightOrVoyageNo);
			voyDestinationSubQuery.AddSubQuery(JobVoyDestinationSchema.JB_JV, voySubQuery, JoinCondition.And);
			sailingSubQuery.AddSubQuery(JobSailingSchema.JX_JB, voyDestinationSubQuery, JoinCondition.And);
			consolTransportSubQuery.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingSubQuery, JoinCondition.And);

			if (!(vesselNK == ""))
			{
				consolTransportSubQuery1.AddToFilter_PossiblyCommaSeparated(JobConsolTransportSchema.JW_Vessel, @operator, vesselNK);
			}
			conShipSubQuery.AddSubQuery(JobConShipLinkSchema.JN_JK, consolTransportSubQuery, JoinCondition.And);
			shipmentSubQuery.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.False);
			shipmentSubQuery.AddSubQuery(conShipSubQuery, JoinCondition.And);

			ZDBOnlySubQuery sailingSubQuery4 = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			ZDBOnlySubQuery voyDestinationSubQuery4 = new ZDBOnlySubQuery(typeof(AutoJobVoyDestination), JobVoyDestinationSchema.PK);
			ZDBOnlySubQuery voySubQuery4 = new ZDBOnlySubQuery(typeof(AutoJobVoyage), JobVoyageSchema.PK);

			if (!(vesselNK == ""))
			{
				voySubQuery4.AddToFilter_PossiblyCommaSeparated(JobVoyageSchema.JV_RV_NKVessel, @operator, vesselNK);
			}
			voySubQuery4.AddToFilter_PossiblyCommaSeparated(JobVoyageSchema.JV_VoyageFlight, @operator, flightOrVoyageNo);
			voyDestinationSubQuery4.AddSubQuery(JobVoyDestinationSchema.JB_JV, voySubQuery4, JoinCondition.And);
			sailingSubQuery4.AddSubQuery(JobSailingSchema.JX_JB, voyDestinationSubQuery4, JoinCondition.And);
			shipmentSubQuery.AddSubQuery(JobShipmentSchema.JS_JX, sailingSubQuery4, JoinCondition.Or);

			ZDBOnlySubQuery conShipSubQuery2 = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			ZDBOnlySubQuery consolTransportSubQuery2 = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);

			consolTransportSubQuery2.AddToFilter(JobConsolTransportSchema.JW_IsLinked, ZBool.False);

			if (!(vesselNK == ""))
			{
				consolTransportSubQuery2.AddToFilter_PossiblyCommaSeparated(JobConsolTransportSchema.JW_Vessel, @operator, vesselNK);
			}

			consolTransportSubQuery2.AddToFilter_PossiblyCommaSeparated(JobConsolTransportSchema.JW_VoyageFlight, @operator, flightOrVoyageNo);

			conShipSubQuery2.AddSubQuery(JobConShipLinkSchema.JN_JK, consolTransportSubQuery2, JoinCondition.And);
			shipmentSubQuery.AddSubQuery(conShipSubQuery2, JoinCondition.Or);

			result.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);

			ZDBOnlySubQuery consolSubQuery2 = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConsolSchema.PK);
			ZDBOnlySubQuery consolTransportSubQuery3 = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);
			ZDBOnlyQuery consolTransportSubQuery4 = new ZDBOnlyQuery(typeof(Transport));
			ZDBOnlySubQuery sailingSubQuery2 = new ZDBOnlySubQuery(typeof(JobSailing), JobSailingSchema.PK);
			ZDBOnlySubQuery voyDestinationSubQuery2 = new ZDBOnlySubQuery(typeof(AutoJobVoyDestination), JobVoyDestinationSchema.PK);
			ZDBOnlySubQuery voySubQuery2 = new ZDBOnlySubQuery(typeof(AutoJobVoyage), JobVoyageSchema.PK);

			if (!(vesselNK == ""))
			{
				voySubQuery2.AddToFilter_PossiblyCommaSeparated(JobVoyageSchema.JV_RV_NKVessel, @operator, vesselNK);
			}
			voySubQuery2.AddToFilter_PossiblyCommaSeparated(JobVoyageSchema.JV_VoyageFlight, @operator, flightOrVoyageNo);
			voyDestinationSubQuery2.AddSubQuery(JobVoyDestinationSchema.JB_JV, voySubQuery2, JoinCondition.And);
			sailingSubQuery2.AddSubQuery(JobSailingSchema.JX_JB, voyDestinationSubQuery2, JoinCondition.And);
			consolTransportSubQuery3.AddSubQuery(JobConsolTransportSchema.JW_JX, sailingSubQuery2, JoinCondition.And);

			if (!(vesselNK == ""))
			{
				consolTransportSubQuery4.AddToFilter_PossiblyCommaSeparated(JobConsolTransportSchema.JW_Vessel, @operator, vesselNK);
			}
			consolTransportSubQuery4.AddToFilter_PossiblyCommaSeparated(JobConsolTransportSchema.JW_VoyageFlight, @operator, flightOrVoyageNo);
			consolTransportSubQuery3.AddToFilter(consolTransportSubQuery4, JoinCondition.Or);
			consolSubQuery2.AddSubQuery(consolTransportSubQuery3, JoinCondition.And);

			result.AddSubQuery(JobHeaderSchema.JH_ParentID, consolSubQuery2, JoinCondition.Or);

			ZDBOnlyQuery result5 = new ZDBOnlyQuery(typeof(ForwardingShipment));
			ZDBOnlySubQuery shipmentSubQuery5 = new ZDBOnlySubQuery(typeof(ForwardingShipment), JobShipmentSchema.PK);
			ZDBOnlySubQuery conShipSubQuery5 = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			ZDBOnlySubQuery consolTransportSubQuery5 = new ZDBOnlySubQuery(typeof(Transport), JobConsolTransportSchema.JW_ParentGUID);

			if (!(vesselNK == ""))
			{
				consolTransportSubQuery5.AddToFilter_PossiblyCommaSeparated(JobConsolTransportSchema.JW_Vessel, @operator, vesselNK);
			}
			consolTransportSubQuery5.AddToFilter_PossiblyCommaSeparated(JobConsolTransportSchema.JW_VoyageFlight, @operator, flightOrVoyageNo);
			conShipSubQuery5.AddSubQuery(JobConShipLinkSchema.JN_JK, consolTransportSubQuery5, JoinCondition.And);
			shipmentSubQuery5.AddToFilter(JobShipmentSchema.JS_IsShipping, ZBool.True);
			shipmentSubQuery5.AddSubQuery(conShipSubQuery5, JoinCondition.And);

			result.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery5, JoinCondition.Or);

			Type voyageAccountType = ObjectFactory.GetType<Freight.Integration.Agency.IVoyageAccount>();
			ZDBOnlySubQuery voyAccountSubQuery = new ZDBOnlySubQuery(voyageAccountType, JobVoyAccountSchema.PK);
			voyAccountSubQuery.AddSubQuery(JobVoyAccountSchema.NA_JV, voySubQuery, JoinCondition.And);
			result.AddSubQuery(JobHeaderSchema.JH_ParentID, voyAccountSubQuery, JoinCondition.Or);

			return result;
		}

		public static ZDBOnlyQuery GetOrderNumberQueryForJobHeader(SQLComparisonOperator @operator, ZString orderNo)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(JobHeader));

			if (AnyNumberNotExceedingMaxLength(ref orderNo, JobOrderHeaderSchema.JD_OrderNumber.MaxLength))
			{
				ZDBOnlyQuery dbOnlyResult1 = new ZDBOnlyQuery(typeof(JobHeader));
				ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobShipmentSchema.PK);
				ZDBOnlySubQuery orderSubQuery = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JS);
				orderSubQuery.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, @operator, orderNo);
				shipmentSubQuery.AddSubQuery(orderSubQuery, JoinCondition.And);
				dbOnlyResult1.AddSubQuery(JobHeaderSchema.JH_ParentID, shipmentSubQuery, JoinCondition.And);
				result.AddToFilter(dbOnlyResult1);

				ZDBOnlyQuery dbOnlyResult2 = new ZDBOnlyQuery(typeof(JobHeader));
				ZDBOnlySubQuery orderSubQuery2 = new ZDBOnlySubQuery(typeof(Order), JobOrderHeaderSchema.JD_JE);
				ZDBOnlySubQuery declarationSubQuery2 = new ZDBOnlySubQuery(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration), JobDeclarationSchema.PK);
				orderSubQuery2.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, JobOrderHeaderSchema.JD_OrderNumber, @operator, orderNo);
				declarationSubQuery2.AddSubQuery(orderSubQuery2, JoinCondition.And);
				dbOnlyResult2.AddSubQuery(JobHeaderSchema.JH_ParentID, declarationSubQuery2, JoinCondition.And);

				result.AddToFilter(dbOnlyResult2, JoinCondition.Or);
			}
			else
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}

		public static ZDBOnlyQuery GetConsignmentRunsheetQueryForJobHeader(SQLComparisonOperator @operator, ZString runsheetNumber)
		{
			var result = new ZDBOnlyQuery(typeof(JobHeader));

			if (AnyNumberNotExceedingMaxLength(ref runsheetNumber, DtbConsignmentRunSheetSchema.KG_RunSheetNumber.MaxLength))
			{
				var runSheetSubQuery = new ZDBOnlySubQuery(typeof(IDtbConsignmentRunSheet), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet);
				runSheetSubQuery.AddToFilter_PossiblyCommaSeparated(DtbConsignmentRunSheetSchema.KG_RunSheetNumber, @operator, runsheetNumber);

				var runSheetInstructionSubQuery = new ZDBOnlySubQuery(typeof(IDtbConsignmentRunSheetInstruction), DtbConsignmentActionSchema.LTA_K1_RunSheetInstruction);
				runSheetInstructionSubQuery.AddSubQuery(runSheetSubQuery, JoinCondition.And);

				var consignmentActionSubQuery = new ZDBOnlySubQuery(typeof(IDtbConsignmentAction), DtbConsignmentAddressSchema.PK, DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress);
				consignmentActionSubQuery.AddSubQuery(runSheetInstructionSubQuery, JoinCondition.And);

				var dtbConsignmentAddressSubQuery = new ZDBOnlySubQuery(typeof(IDtbConsignmentAddress), DtbConsignmentSchema.PK, DtbConsignmentAddressSchema.LTS_LTC_Consignment);
				dtbConsignmentAddressSubQuery.AddSubQuery(consignmentActionSubQuery, JoinCondition.And);

				var dtbConsignmentSubQuery = new ZDBOnlySubQuery(typeof(IDtbConsignment), DtbConsignmentSchema.PK);
				dtbConsignmentSubQuery.AddSubQuery(dtbConsignmentAddressSubQuery, JoinCondition.And);

				result.AddSubQuery(JobHeaderSchema.JH_ParentID, dtbConsignmentSubQuery, JoinCondition.And);
			}
			else
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}

		public static ZDBOnlyQuery GetTransportBookingReferenceQueryForJobHeader(SQLComparisonOperator @operator, ZString transportBookingReference)
		{
			var result = new ZDBOnlyQuery(typeof(JobHeader));

			if (AnyNumberNotExceedingMaxLength(ref transportBookingReference, DtbBookingSchema.KM_TransportReference.MaxLength))
			{
				// related
				var bookingSubFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBooking>(), DtbBookingSchema.KM_KB_Booking);
				bookingSubFilter.AddToFilter_PossiblyCommaSeparated(DtbBookingSchema.KM_TransportReference, @operator, transportBookingReference);

				var bookingConsolidationSubFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBookingConsolidation>(), DtbBookingConsolidationSchema.KB_ParentID);
				bookingConsolidationSubFilter.AddSubQuery(bookingSubFilter, JoinCondition.And);

				var parentFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<IViewTransportBookingParents>(), ViewTransportBookingParentsSchema.PK);
				parentFilter.AddSubQuery(bookingConsolidationSubFilter, JoinCondition.And);

				// standalone
				var bookingStandaloneFilter = new ZDBOnlySubQuery(ObjectFactory.GetType<IDtbBooking>(), DtbBookingSchema.PK);
				bookingStandaloneFilter.AddToFilter_PossiblyCommaSeparated(DtbBookingSchema.KM_TransportReference, @operator, transportBookingReference);

				result.AddSubQuery(JobHeaderSchema.JH_ParentID, parentFilter, JoinCondition.Or);
				result.AddSubQuery(JobHeaderSchema.JH_ParentID, bookingStandaloneFilter, JoinCondition.Or);
			}
			else
			{
				result.IsNoResultQuery = true;
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public static bool AnyNumberNotExceedingMaxLength(ref ZString value, int maxLength)
		{
			//Algorithm: Remove all entries that were above max length, then return true if at least one entry was below max length.

			var multiSearchSeparator = EnvProxy.Instance.Registry.MultiSearchSeparator;

			if (!value.Contains(multiSearchSeparator, StringComparison.Ordinal))
			{
				if (value.Length <= maxLength)
				{
					return true;
				}
				value = ZString.Empty;
				return false;
			}
			var numbers = value.Split(multiSearchSeparator).Select(x => x.Trim()).Select(x => (x.Length <= maxLength) ? x : ZString.Empty).Where(x => !x.IsEmpty).ToList();
			var result = numbers.Any();
			if (result)
			{
				value = numbers.Aggregate((x, y) => x + multiSearchSeparator + y);
			}
			return result;
		}

		public static string GetCommaSeparatedGuidsForInClause(IEnumerable<ZGuid> pks)
		{
			var resultBuilder = new ZStringBuilder();
			foreach (var pk in pks.Distinct())
			{
				resultBuilder.Append(pk.ToString());
			}
			return string.Format(CultureInfo.InvariantCulture, "'{0}'", resultBuilder.ToStringWithDelimiterBetweenAppends("','"));
		}

		public static ZDBOnlyQuery GetOrphanWIPsOrAccrualsFilterQuery()
		{
			string queryText = string.Format(@"{0} IN (
				SELECT AL_PK
				FROM dbo.AccTransactionLines
				WHERE
				AL_LineType = @WIP
				AND AL_ReverseDate IS NULL
				AND NOT EXISTS (SELECT JR_AL_ARLine FROM dbo.JobCharge WHERE JR_AL_ARLine = AL_PK)
		
				UNION ALL

				SELECT AL_PK
				FROM dbo.AccTransactionLines
				WHERE
				AL_LineType = @ACR 
				AND AL_ReverseDate IS NULL
				AND NOT EXISTS (SELECT JR_AL_APLine FROM dbo.JobCharge WHERE JR_AL_APLine = AL_PK)
			)", AccTransactionLinesSchema.PK.Name);

			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@WIP", TransactionLineTypes.WIP, AccTransactionLinesSchema.AL_LineType);
			parameters.Add("@ACR", TransactionLineTypes.Accrual, AccTransactionLinesSchema.AL_LineType);
			query.AddFilterAndZSQLParameterCollection(queryText, parameters);

			return query;
		}

		public static ZDBOnlyQuery GetAccountingDateQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var modelViewHelper = new ModelViewColumnQueryHelper<BaseJobDeclaration>();
			var accountingDateQuery = modelViewHelper.GetDateFilterQuery(BaseJobDeclaration.Schema.PK, ModelViewPK, ModelView, ModelViewK84AccountingDate, comparisonOperator, date1, date2);

			var jobHeaderQuery = new ZDBOnlyQuery(typeof(JobHeader));
			var declarationSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobHeaderSchema.JH_ParentID);
			declarationSubQuery.AddToFilter(accountingDateQuery);

			var lvxJobSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), JobDeclarationSchema.PK);
			lvxJobSubQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, Customs.Common.CA.CAJobMessageTypeList.Codes.LVSForConsolidation);
			var invoiceSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobDeclarationSchema.PK, JobComInvoiceHeaderSchema.JZ_JE);
			var genPivotSubQuery = new ZDBOnlySubQuery(typeof(GenPivot), JobComInvoiceHeaderSchema.PK, GenPivotSchema.XX_Relation1ID);

			var lvsJobSubQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), GenPivotSchema.XX_Relation2ID);
			lvsJobSubQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, Customs.Common.CA.CAJobMessageTypeList.Codes.LowValueShipments);
			lvsJobSubQuery.AddToFilter(accountingDateQuery);

			genPivotSubQuery.AddToFilter(GenPivotSchema.XX_RelationType, GenPivotTypeDecider.Types.InvoiceRelatedDeclarationGenPivot);
			genPivotSubQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, JobComInvoiceHeaderSchema.Constants.Prefix);
			genPivotSubQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, JobDeclarationSchema.Constants.Prefix);
			genPivotSubQuery.AddSubQuery(lvsJobSubQuery, JoinCondition.And);

			invoiceSubQuery.AddSubQuery(genPivotSubQuery, JoinCondition.And);
			lvxJobSubQuery.AddSubQuery(invoiceSubQuery, JoinCondition.And);

			declarationSubQuery.AddAsUnionQuery(lvxJobSubQuery);

			jobHeaderQuery.AddSubQuery(declarationSubQuery, JoinCondition.And);
			jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);

			return jobHeaderQuery;
		}
		const string ModelView = "CAJobDeclaration";
		const string ModelViewPK = "JE_PK";
		const string ModelViewK84AccountingDate = "JE_K84AccountingDate";

		public static ZQuery GetComplianceNumberQuery(SQLComparisonOperator @operator, ZString complianceNumber)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			var notIn = false;
			var joinCondition = JoinCondition.Or;
			var complianceOperator = @operator;
			if (@operator.IsNegativeSQLOperator())
			{
				notIn = true;
				joinCondition = JoinCondition.And;
				complianceOperator = @operator.GetNegatingSQLOperatorIfNotInSubquery();
			}

			var complianceSubQuery = GetComplianceNumberSubQuery(complianceOperator, complianceNumber, notIn);
			query.AddSubQuery(AccTransactionHeaderSchema.PK, complianceSubQuery, JoinCondition.And);

			if (@operator == SQLComparisonOperator.IsBlank)
			{
				var blankSubQuery = GetComplianceNumberSubQuery(@operator, complianceNumber, true);
				query.AddSubQuery(AccTransactionHeaderSchema.PK, blankSubQuery, JoinCondition.Or);
				joinCondition = JoinCondition.And;
			}

			query.AddToFilter_PossiblyCommaSeparated(joinCondition, AccTransactionHeaderSchema.AH_TransactionReference, @operator, complianceNumber);

			return query;
		}

		static ZDBOnlySubQuery GetComplianceNumberSubQuery(SQLComparisonOperator @operator, ZString complianceNumber, bool notIn)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH, notIn);
			var subQuery1 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_AL);
			var subQuery2 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentLine), AccComplianceDocumentLineSchema.PK);
			var subQuery3 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentHeader), AccComplianceDocumentHeaderSchema.PK);
			if (!(notIn && @operator == SQLComparisonOperator.IsBlank))
			{
				subQuery3.AddToFilter_PossiblyCommaSeparated(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, @operator, complianceNumber);
			}
			subQuery2.AddSubQuery(AccComplianceDocumentLineSchema.ADL_ADH, subQuery3, JoinCondition.And);
			subQuery1.AddSubQuery(AccComplianceDocumentPivotSchema.ADP_ADL, subQuery2, JoinCondition.And);
			subQuery.AddSubQuery(AccTransactionLinesSchema.PK, subQuery1, JoinCondition.And);

			return subQuery;
		}

		#endregion

		#region Collection Batch

		public static AccBankAccountCollection GetAccBankAccountCollection(BusinessObjectFactory factory)
		{
			var branchFilter = new ZQuery(AccBankAccountSchema.AB_GB, GlbBranch.CurrentBranch.PK);
			branchFilter.AddToFilter(JoinCondition.Or, AccBankAccountSchema.AB_GB, SQLComparisonOperator.Equal, null);

			var bankFilter = new ZQuery(AccBankAccountSchema.AB_GC, GlbCompany.CurrentCompany.PK);
			bankFilter.AddToFilter(AccBankAccountSchema.AB_IsActive, true);
			bankFilter.AddToFilter(branchFilter, JoinCondition.And);

			return new AccBankAccountCollection(factory, bankFilter);
		}

		#endregion

		#region Transaction Line Department Validation

		public static ZString GetDeptNotInChargeDeptListMessage(InvoicingLineBase line)
		{
			ZString errorMessage = ZString.Empty;

			if (!line.GenericCharge.IsEmpty && line.GenericCharge.IsValid
				&& line.GenericTransactionCharge != null && !line.GenericTransactionCharge.IsDeleted && line.Department != null)
			{
				if (!line.GenericTransactionCharge.VC_DepartmentFilterList.Contains("ALL") &&
					!line.GenericTransactionCharge.VC_DepartmentFilterList.Contains(line.Department.GE_Code))
				{
					errorMessage = Res.GetString("b28af03d-df42-42bc-9742-3d57b63d96a3", "The department {0} is not contained in the department filter list for the entered charge.\r\nThe list is: {1}", line.Department.GE_Code, line.GenericTransactionCharge.VC_DepartmentFilterList);
				}
			}
			return errorMessage;
		}

		#endregion

		public static bool IsAllowFuturePostingRegistryEnabled => AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.Value;
		public static bool DoesUserHaveFuturePostingSecurity => Env.Security.CashBookAllowFuturePostingOfTransactions.IsAllowedWithConstraint();

		public static string ConvertPostingOptionToHumanReadableName(JobInvoicingPostingOption postingOption, bool isThisConsolPosting)
		{
			string result = postingOption.ToString();
			switch (postingOption)
			{
				case JobInvoicingPostingOption.Gateway:
					result = Res.GetString("ad710ca2-5850-45a4-b5f3-fdf9d224751d", "Post Gateway Agent Charges");
					break;
				case JobInvoicingPostingOption.Agent:
					result = Res.GetString("a021c632-a634-44c9-87c0-e42cf4fe951e", "Post Overseas Agent Charges");
					break;
				case JobInvoicingPostingOption.LocalClient:
					result = Res.GetString("3d27c413-5f7b-450a-b262-589c71580ea8", "Post Local Client Charges");
					break;
				case JobInvoicingPostingOption.Revenue:
					result = Res.GetString("c5ddcd38-24e0-4a26-8ea5-d3dab3c4bb40", "Post All Revenue Charges");
					break;
				case JobInvoicingPostingOption.Costs:
					result = Res.GetString("3befecab-c0fe-43fc-8ca9-f49fe3bfb87d", "Post All Costs");
					break;
				case JobInvoicingPostingOption.ConsolCosts:
					result = Res.GetString("a393c713-47d4-4d03-bf04-c3aa8ae7cd3c", "Post Consol Costs Only");
					break;
				case JobInvoicingPostingOption.All:
					result = isThisConsolPosting ? Res.GetString("0ec66312-314b-44d0-9862-7e43537ee1a8", "Post Whole Consol") : Res.GetString("b96627f4-a2e9-465e-b8d2-c0c80ab921a6", "Post All Charges and Costs");
					break;
				case JobInvoicingPostingOption.Disbursement:
					result = Res.GetString("fd86384d-49e5-456b-bd2a-25fd89713926", "Post Disbursement Charges only");
					break;
				case JobInvoicingPostingOption.CustomsDSBChargeAPOnly:
					result = Res.GetString("d2b4371a-f396-402f-a26f-cfb435fff7e9", "Post Customs Disbursement Costs only");
					break;
				case JobInvoicingPostingOption.CustomsDSBChargeAROnly:
					result = Res.GetString("60cc9bdd-cda0-4721-a5ad-07a14cb26e89", "Post Customs Disbursement Charges only");
					break;
				case JobInvoicingPostingOption.AllSisterCompanyCharges:
					result = Res.GetString("9b094a8f-ec63-4354-896f-dd23cfe5c372", "Post Charges for All Group Companies");
					break;
				case JobInvoicingPostingOption.LocalSisterCompanyChargesOnly:
					result = Res.GetString("c064a036-a9a1-4260-b183-e2340b5ecd71", "Post Charges for Group Companies in My Login Country/Region");
					break;
			}

			return result;
		}

		public static string ConvertApprovingOptionToHumanReadableName(int levelRequired, ZString approvingOption)
		{
			string result = approvingOption.ToString();
			switch (approvingOption)
			{
				case ApprovalCredentialOption.SingleLogin:
					result = Res.GetString("049bb03a-2740-459b-a34f-7a638f34a6b0", "Single Level {0}", levelRequired);
					break;
				case ApprovalCredentialOption.DoubleLogin:
					result = Res.GetString("78c40fe2-b686-4320-805f-09d7928249c1", "Two Level {0}", levelRequired);
					break;
				case ApprovalCredentialOption.SequentialLogin:
					result = Res.GetString("d6778d69-3603-4200-802b-ce14a6d69dce", "Seq {0}", BuildSequenceText(levelRequired));
					break;
			}
			return result;
		}

		static string BuildSequenceText(int levelRequired)
		{
			var result = "";
			for (int i = 1; i < levelRequired; i++)
			{
				result += (i.ToString(CultureInfo.InvariantCulture) + ",");
			}
			result += levelRequired.ToString(CultureInfo.InvariantCulture);
			return result;
		}

		public static bool DeleteFileSafe(string path)
		{
			bool result = false;

			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				try
				{
					File.Delete(path);
					result = true;
				}
				// Swallow possible exception to prevent breaking normal execution
				catch (IOException) { }
				catch (UnauthorizedAccessException) { }
			}

			return result;
		}

		public static bool DeleteDirectorySafe(string path)
		{
			bool result = false;

			if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
			{
				try
				{
					Directory.Delete(path);
					result = true;
				}
				// Swallow possible exception to prevent breaking normal execution
				catch (IOException) { }
				catch (UnauthorizedAccessException) { }
			}

			return result;
		}

		public static void CopyPersistentValuesOnDataRowLevel(BusinessObject sourceBizO, BusinessObject destinationBizO)
		{
			destinationBizO.CopyPersistentValuesFrom(sourceBizO, new BusinessObjectCloneArgs(Array.Empty<string>(), true));
			if (!destinationBizO.HasChanges)
			{
				destinationBizO.HasChanges = destinationBizO.ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(info => info.IsPersistent && info.HasChanges);
			}
		}

		public static ZDecimal GetPeriodEndRateExchangeRate(BusinessObjectFactory factory, ZString currency, ZInt period, GlbCompany company = null)
		{
			if (company == null)
			{
				company = GlbCompany.CurrentCompany;
			}

			ZDecimal result = 0M;
			var lastDayOfPeriod = new AccountingPeriodCalculator(factory, company).GetLastDayForPeriod(period);
			if (lastDayOfPeriod.IsValid)
			{
				result = ((ICompany)company).ExchangeRate.GetRate(currency, ExchangeRateType.PeriodEnd, lastDayOfPeriod.ToDateTime());
			}

			return result;
		}

		public static ZDecimal GetExchangeRate(ZString currency, ExchangeRateType exchangeRateType, ZDateTime dateTime)
		{
			return currency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency ? Env.CurrentCompany.ExchangeRate.GetRate(currency, exchangeRateType, dateTime.ToDateTime()) : 1m;
		}

		public static ZDecimal GetGLJournalExchangeRate(BusinessObjectFactory factory, ZString accountType, ZString currency, ZInt period, ZDateTime? postDate = null, GlbCompany company = null)
		{
			if (company == null)
			{
				company = GlbCompany.CurrentCompany;
			}

			var lastDayOfPeriod = new AccountingPeriodCalculator(factory, company).GetLastDayForPeriod(period);
			var rateType = GetGLJournalExchangeRateType(company.PK.ToGuid(), accountType);
			var date = rateType == ExchangeRateTypes.Code.PeriodEndRate ? lastDayOfPeriod : (postDate == ZDateTime.Empty ? null : postDate) ?? lastDayOfPeriod;

			ZDecimal result = 0M;
			if (date.IsValid)
			{
				result = currency != company.GC_RX_NKLocalCurrency ? ((ICompany)company).ExchangeRate.GetRate(currency, ZArchitecture.Environment.ExchangeRate.GetExchangeRateType(rateType), date.ToDateTime()) : 1m;
			}

			return result;
		}

		public static string GetGLJournalExchangeRateType(Guid companyPk, string accountType)
		{
			var rateType = ExchangeRateTypes.Code.PeriodEndRate;
			var gLJournalExchangeRateTypeRegistry = AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateType.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty);
			if (accountType == AccountType.ProfitAndLossAccount)
			{
				rateType = gLJournalExchangeRateTypeRegistry.ProfitAndLossAccountTypeExchangeRateType;
			}
			else if (accountType == AccountType.BalanceSheetAccount)
			{
				rateType = gLJournalExchangeRateTypeRegistry.BalanceSheetAccountTypeExchangeRateType;
			}

			return rateType;
		}

		public static GlbCompany[] GetAllActiveCompanies(BusinessObjectFactory factory)
		{
			ZQuery companyQuery = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, SQLComparisonOperator.NotEqual, null);
			companyQuery.AddToFilter(GlbCompanySchema.GC_IsActive, true);

			return factory.Load<GlbCompany>(companyQuery);
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Using direct SQL for performance")]
		public static ImmutableList<string> GetAllActiveCompaniesCountries(DbConnection dbConn)
		{
			const string selectCompanySql = "SELECT DISTINCT GC_RN_NKCountryCode FROM dbo.GlbCompany WHERE GC_IsActive = 1 AND GC_OH_OrgProxy IS NOT NULL";

			var result = new List<string>();
			using (var command = dbConn.Command(selectCompanySql))
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(reader.GetString(0));
				}
			}
			return ImmutableList.ToImmutableList(result);
		}

		public static GlbBranch GetTopOneActiveBranchOfCompany(ZGuid companyPK, BusinessObjectFactory factory)
		{
			var filter = new ZQuery(GlbBranchSchema.GB_IsActive, true).AddToFilter(GlbBranchSchema.GB_GC, companyPK);
			filter.OrderBy = GlbBranchSchema.GB_Code.Name;
			return factory.LoadTop1<GlbBranch>(filter);
		}

		public static GlbBranchCollection GetBranchesOfCompany(ZGuid companyPK, BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(GlbBranchSchema.GB_IsActive, true);
			filter.AddToFilter(GlbBranchSchema.GB_GC, companyPK);
			var branches = new GlbBranchCollection(factory, filter);
			branches.Load();

			return branches;
		}

		public static GlbBranchCollection GetBranchesOfCompanySortedByTimeZone(ZGuid companyPK, BusinessObjectFactory factory)
		{
			var branches = GetBranchesOfCompany(companyPK, factory);

			branches.Sort(new Comparison<GlbBranch>((x, y) =>
				x.HomePort.StandardZoneUTCOffset < y.HomePort.StandardZoneUTCOffset ? 1 :
				x.HomePort.StandardZoneUTCOffset > y.HomePort.StandardZoneUTCOffset ? -1 :
				x.GB_Code > y.GB_Code ? 1 : x.GB_Code < y.GB_Code ? -1 : 0));

			return branches;
		}

		public class CommandTimeoutInitializer : IDisposable
		{
			[SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
			public CommandTimeoutInitializer(int timeoutInSeconds)
			{
				DefaultTimeout = Db.Connection.DefaultCommandTimeOutInSeconds;
				Db.Connection.DefaultCommandTimeOutInSeconds = timeoutInSeconds;
			}

			readonly int DefaultTimeout;

			#region IDisposable Members

			public void Dispose()
			{
				Db.Connection.DefaultCommandTimeOutInSeconds = DefaultTimeout;
			}

			#endregion
		}

		public static int ChunkBatchSize
		{
			get
			{
				return
#if DEBUG
 Globals.IsTest ? 5 :
#endif
 500;
			}
		}

		public static IEnumerable<IList<T>> ChunksOf<T>(IEnumerable<T> sequence, int size)
		{
			Argument.GreaterThanZero(size, (NoResString)"batch size length");

			List<T> chunk = new List<T>(size);

			foreach (T element in sequence)
			{
				chunk.Add(element);
				if (chunk.Count == size)
				{
					yield return chunk;
					chunk = new List<T>(size);
				}
			}
			if (chunk.Count > 0)
			{
				yield return chunk;
			}
		}

		public static AccTransactionHeader GetFirstARTransactionFromJob(IJobHeader job)
		{
			if (job != null)
			{
				var filter = new JobARInvoicePrintingFilter(job, job.PK, job.Factory, job.JH_GC);
				filter.EnableAccTransactionHeaderIndexHints = true;
				filter.RefreshInvoiceList();
				return filter.Transactions.OfType<AccTransactionHeader>().Where(x => !x.AH_PostDate.IsEmpty).OrderBy(x => x.AH_PostDate).FirstOrDefault();
			}
			else
			{
				return null;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void DeleteQueuePeriod(BusinessObjectFactory periodFactory, ZGuid periodPK)
		{
			var cmdText = FormattableString.Invariant($@"DELETE dbo.AccCurrencyAdjustmentQueue WHERE ACA_ParentID = @ParentID");

			using (var cmd = ((IDbConnected)periodFactory).Connection.Command(cmdText))
			{
				cmd.CommandType = CommandType.Text;
				cmd.AddParameter("@ParentID", SqlDbType.UniqueIdentifier, periodPK.ToGuid());
				cmd.ExecuteNonQuery();
			}
		}

		public static ZDate GetChargeTaxDate(ChargeWithCost charge, ZDate invoiceDate, ZString ledger)
		{
			var result = ZDate.Today;

			if (charge.JR_IsApportioned) // if isApportioned (JR_E6), then the charge must come from FCN consol cost
			{
				result = GetTaxDateForApportionedChargeFromConsol(charge, invoiceDate);
			}
			else
			{
				var job = charge.InvoicingJob;
				if (job != null)
				{
					var plugIn = job.PlugInData?.InvoicingSupporter;
					var taxDateOption = ((IPostingJob)job).GetTaxDateDefaultingOptionForJob(charge.Factory, ledger);
					if (taxDateOption != null)
					{
						result = ((IPostingJob)job).GetTaxDateBasedOnRegistryDefaultingOption(plugIn, taxDateOption.TaxDateOption, invoiceDate).Item1;
					}
				}
			}
			return result;
		}

		static ZDate GetTaxDateForApportionedChargeFromConsol(ChargeWithCost charge, ZDate invoiceDate)
		{
			var result = ZDate.Today;
			var consol = charge.ParentConsolCost?.Consol;
			if (consol != null)
			{
				var costSupporter = consol.CostSupporter;

				var cachedKey = string.Format("GetTaxDateForApportionedChargeFromConsol{0}", consol.PK);
				var taxDateDefaultingOption = charge.Factory.GetCachedValue(cachedKey, () =>
				{
					return AccountingUtils.GetTaxDateDefaultingOptionForCostSupporter(costSupporter, LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.ForwardingConsolCode);
				});

				if (taxDateDefaultingOption != null && taxDateDefaultingOption.TaxDateOption == TaxDateDefaultingOption.Code.InvoiceDate)
				{
					result = invoiceDate;
				}
				// for any other option, just use Today's date since FCN can only support INV and TDY.
			}
			return result;
		}

		public static TaxDateDefaultingOption GetTaxDateDefaultingOptionForInvoicingSupporter(IJobInvoicingSupporter plugIn, ZString ledger)
		{
			TaxDateDefaultingOption result = null;
			if (plugIn != null)
			{
				var jobType = plugIn.ConsumerType.Code;
				var mode = plugIn.TransportMode;
				var direction = JobInvoicingPlugInExtensions.GetJobDirection(plugIn);
				result = GetMatchedTaxDateDefaultingOptionFromRegistry(ledger, jobType, mode, direction);
			}
			return result;
		}

		public static TaxDateDefaultingOption GetTaxDateDefaultingOptionForCostSupporter(IGenericJobCostSupporter costSupporter, ZString ledger, ZString consumerType)
		{
			TaxDateDefaultingOption result = null;
			if (costSupporter != null)
			{
				var jobType = consumerType;
				var mode = costSupporter.TransportMode;
				var direction = GetConsolDirection(costSupporter);
				result = GetMatchedTaxDateDefaultingOptionFromRegistry(LedgerTypes.AccountsPayable, jobType, mode, direction);
			}
			return result;
		}

		static TaxDateDefaultingOption GetMatchedTaxDateDefaultingOptionFromRegistry(ZString ledger, ZString jobType, ZString mode, ZString direction) =>
			AccountingConfigurationRegistry.Instance.TaxDateDefaultingOption.Value.Cast<TaxDateDefaultingOption>().FirstOrDefault(x =>
						(x.JobType == jobType || x.JobType == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All)
						&& (x.Ledger == ledger || x.Ledger == LedgerTypeAdditionalCodes.All)
						&& (x.DirectionCode == direction || x.DirectionCode == FreightShipmentDirection.Code.All || x.DirectionCode.IsEmpty)
						&& (x.Mode == mode || x.Mode == JobConfigurationSelectorLookups.ModeAdditionalCodes.All || x.Mode.IsEmpty));

		static ZString GetConsolDirection(IGenericJobCostSupporter costSupporter)
		{
			var direction = ZString.Empty;
			if (costSupporter != null)
			{
				if (costSupporter.Direction == Directions.Import)
				{
					direction = FreightShipmentDirection.Code.Import;
				}
				else if (costSupporter.Direction == Directions.Export)
				{
					direction = FreightShipmentDirection.Code.Export;
				}
				else if (costSupporter.Direction == Directions.Domestic)
				{
					direction = FreightShipmentDirection.Code.Domestic;
				}
				else
				{
					direction = FreightShipmentDirection.Code.Other;
				}
			}
			return direction;
		}

		public static byte[] ParseHex(string s)
		{
			if (string.IsNullOrEmpty(s))
			{
				return Array.Empty<byte>();
			}
			if (s.Length % 2 != 0)
			{
				throw new ArgumentOutOfRangeException(nameof(s), "String length must be divisible by 2.");
			}
			if (!s.All(ch => (ch >= 'a' && ch <= 'f')
						  || (ch >= 'A' && ch <= 'F')
						  || (ch >= '0' && ch <= '9')))
			{
				throw new ArgumentOutOfRangeException(nameof(s), "String must consist of hex characters 0..F.");
			}

			var result = new byte[s.Length / 2];
			for (int i = 0; i < result.Length; i++)
			{
				var hexByte = s.Substring(i * 2, 2);
				result[i] = byte.Parse(hexByte, NumberStyles.HexNumber, DefaultCulture.Instance);
			}
			return result;
		}

		#region Profit Share Charge Code

		public static bool ChargeCodeExistsAndBelongsToCompany(BusinessObjectFactory factory, ZGuid chargeCodePK, ZGuid companyPK)
		{
			if (chargeCodePK.IsEmpty)
			{
				return false;
			}

			var chargeCode = factory.Load<AccChargeCode>(chargeCodePK);
			return chargeCode != null && chargeCode.AC_GC == companyPK;
		}

		public static bool ValidateProfitShareRegistry(BusinessObjectFactory factory, ZGuid companyPK, out string errorMessage)
		{
			errorMessage = string.Empty;
			var profitShareChargeCode = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode;

			if (!ChargeCodeExistsAndBelongsToCompany(factory, profitShareChargeCode.Value, companyPK))
			{
				errorMessage = ProfitShareErrorMessages.InvalidRegistry(profitShareChargeCode);
				return false;
			}

			var profitShareChargeCodesPerParty = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty;

			foreach (ChargeCodeWithType chargeCodesPerParty in profitShareChargeCodesPerParty.Value)
			{
				if (!chargeCodesPerParty.UseDefaultProfitShareChargeCode && !ChargeCodeExistsAndBelongsToCompany(factory, chargeCodesPerParty.ChargeCode, companyPK))
				{
					errorMessage = ProfitShareErrorMessages.InvalidRegistry(profitShareChargeCodesPerParty);
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Registry Information

		public static string CollectRegistryInfoForOrganizationCreditLimitCheck()
		{
			var registryInfo = new ZStringBuilder();
			registryInfo.Append($".{AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.Caption}: {AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.Value.ToYesNoString()}");
			registryInfo.Append($".{AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.Caption}: {AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance.Value.ToYesNoString()}");
			registryInfo.Append($".{AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.Caption}: {AccountingConfigurationRegistry.Instance.UseWebServiceForTransactionPaymentStatus.Value.ToYesNoString()}");
			registryInfo.Append($".{AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.Caption}: {AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue.Value.ToYesNoString()}");
			registryInfo.Append($".{AccountingConfigurationRegistry.Instance.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Caption}: {AccountingConfigurationRegistry.Instance.EnableBillingTabBackgroundValidationWhenCreditCheckUsesWebService.Value.ToYesNoString()}");

			return registryInfo.ToStringWithNewLineBetweenAppends();
		}

		#endregion

		public static bool AreDatesInTheSameCalendarMonth(params ZDateTime[] dates)
		{
			return dates == null || dates.AllSame(x => x.Year) && dates.AllSame(x => x.Month);
		}

		public static ZString GetElectronicProcessingChargeCurrency(Job job)
		{
			var currencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			if (AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCurrency.Value.Any())
			{
				currencyCode = AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCurrency.Value
					.Cast<ElectronicProcessingChargeCurrency>()
					.OrderByDescending(x => x.ValidFromDate)
					.FirstOrDefault(x => x.ValidFromDate <= job.JH_A_JOP)
					?.Currency
					?.RX_Code ?? string.Empty;
			}

			return currencyCode;
		}

		public static string GetElectronicProcessingChargeCurrencyNoExchangeRateErrorMessage(Job job) => Res.GetString("9CF4F9EB-EC49-457C-8DDB-C05B5892052B", "The Invoicing Job cannot be created due to missing {0} exchange rate required for the creation of the disbursement license fee transactions.", GetElectronicProcessingChargeCurrency(job));

		public static bool EnableElectronicProcessingCharge(IJobInvoicingPlugIn parent)
		{
			var jobType = parent?.InvoicingSupporter?.ConsumerType?.Code;
			return !string.IsNullOrEmpty(jobType)
				&& AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality.Value
				&& AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value.Cast<ElectronicProcessingChargeConfiguration>().Any(x => x.JobType == jobType && ZDateTime.Today >= x.StartDate && (x.EndDate.IsEmpty || ZDateTime.Today < x.EndDate.AddDays(1)));
		}

		public static bool ShouldShowRelatedDisbursementTransactions(string countryCode = null)
		{
			countryCode = countryCode ?? GlbCompany.CurrentCompany.Country.Code;

			var provider = (ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<IRelatedDisbursementTransaction>(countryCode));
			return AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value
					&& (provider?.IsEnableRelatedDisbursementTransaction() ?? false);
		}

		#region EDW

#if DEBUG
		public static IDisposable TemporarilySetDataWarehouseServerToNull()
		{
			isDataWarehouseServerNullForTest = true;

			return new DisposableAction(new Action(() =>
			{
				isDataWarehouseServerNullForTest = false;
			}));
		}

		public static bool isDataWarehouseServerNullForTest { get; private set; }
#endif

		public static bool IsEDWEnabled()
		{
#if DEBUG
			if (isDataWarehouseServerNullForTest)
			{
				return false;
			}
#endif
			return string.IsNullOrEmpty(BiServiceTaskHelpers.IsEdwEnabled());
		}

		#endregion
	}
}
