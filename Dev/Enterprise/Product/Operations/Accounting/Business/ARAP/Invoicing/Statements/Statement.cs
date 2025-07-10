using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[Serializable]
	public class PublishedARInvoiceDocumentNotFoundException : Exception
	{
		public PublishedARInvoiceDocumentNotFoundException() : base() { }

#if NETFRAMEWORK
		protected PublishedARInvoiceDocumentNotFoundException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	public partial class Statement : StatementBase, IDocumentSupportable, IAccountFeeInvoiceSupportable
	{
		protected Statement(GlbBranch branch)
			: base(new BusinessObjectFactory())
		{
			if (branch == null)
			{
				throw new ArgumentException("Should provide proper Glb branch");
			}

			this.fBranch = branch;
			this.fDoAccountFeeTransaction = AccountFeeSettings.HasAccountFeeSettings(branch.Company.PK);
		}

		public static Statement New(GlbBranch branch)
		{
			var overridden = OverridableNewDelegate.Value;
			return overridden != null ? overridden(branch) : new Statement(branch);
		}

		#region Lookups

		#region DocumentToPrint_List

		public CodeDescriptionPairList DocumentToPrint_List
		{
			get
			{
				if (fDocumentToPrint_List == null)
				{
					fDocumentToPrint_List = new CodeDescriptionPairList();
					fDocumentToPrint_List.AddPair(Core.Constants.StatementCollectionLetterType.StatementOfAccount, Res.GetString("Accounting|StatementCollectionLetterTypeList|StatementOfAccount", "Statement of Account"));
					fDocumentToPrint_List.AddPair(Core.Constants.StatementCollectionLetterType.FirstReminder, Res.GetString("Accounting|StatementCollectionLetterTypeList|FirstReminder", "First Reminder"));
					fDocumentToPrint_List.AddPair(Core.Constants.StatementCollectionLetterType.SecondReminder, Res.GetString("Accounting|StatementCollectionLetterTypeList|SecondReminder", "Second Reminder"));
					fDocumentToPrint_List.AddPair(Core.Constants.StatementCollectionLetterType.CollectionLetter, Res.GetString("Accounting|StatementCollectionLetterTypeList|CollectionLetter", "Collection Letter"));
					fDocumentToPrint_List.AddPair(Core.Constants.StatementCollectionLetterType.DemandLetter, Res.GetString("Accounting|StatementCollectionLetterTypeList|DemandLetter", "Demand Letter"));
				}

				return fDocumentToPrint_List;
			}
		}

		CodeDescriptionPairList fDocumentToPrint_List;

		#endregion

		#region DocumentToPrint_List

		public CodeDescriptionPairList IssueStatementPack_List
		{
			get
			{
				if (fIssueStatementPack_List == null)
				{
					fIssueStatementPack_List = new CodeDescriptionPairList();

					fIssueStatementPack_List.AddPair(AccountingConstants.IssueStatementPackType.Default, Res.GetString("Accounting|IssueStatementPackTypeList|Default", "Default (as per Organization setup)"));
					fIssueStatementPack_List.AddPair(AccountingConstants.IssueStatementPackType.StatementOnly, Res.GetString("Accounting|IssueStatementPackTypeList|StatementOnly", "Print Statement only"));
					fIssueStatementPack_List.AddPair(AccountingConstants.IssueStatementPackType.StatementAndInvoices, Res.GetString("Accounting|IssueStatementPackTypeList|StatementAndInvoices", "Print Statement and Attach Invoices"));
				}

				return fIssueStatementPack_List;
			}
		}

		CodeDescriptionPairList fIssueStatementPack_List;

		#endregion

		#region Organisation_List

		public OrgHeaderCollection Organisation_List
		{
			get
			{
				if (fOrganisation_List == null)
				{
					ZQuery filter = new ZQuery();
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));

					ZDBOnlySubQuery isDebtorSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					isDebtorSubQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					isDebtorSubQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					query.AddSubQuery(isDebtorSubQuery, JoinCondition.Or);

					ZQuery currentCompanyBranchesSubFilter = new ZQuery();
					currentCompanyBranchesSubFilter.DefaultJoinCondition = JoinCondition.Or;
					foreach (GlbBranch branch in GlbCompany.CurrentCompany.Branches)
					{
						currentCompanyBranchesSubFilter.AddToFilter(AccTransactionHeaderSchema.AH_GB, branch.PK);
					}

					ZDBOnlySubQuery outstandingTransactionsSubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.AH_OH);
					outstandingTransactionsSubQuery.DefaultJoinCondition = JoinCondition.And;
					outstandingTransactionsSubQuery.AddToFilter(currentCompanyBranchesSubFilter, JoinCondition.And);
					outstandingTransactionsSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
					outstandingTransactionsSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.Equal, null);
					outstandingTransactionsSubQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, SQLComparisonOperator.NotEqual, null);
					query.AddSubQuery(outstandingTransactionsSubQuery, JoinCondition.Or);

					filter.AddToFilter(query, JoinCondition.And);

					fOrganisation_List = new OrgHeaderCollection(new BusinessObjectFactory(), query);
				}

				return fOrganisation_List;
			}
		}

		OrgHeaderCollection fOrganisation_List;

		#endregion

		#region DebtorGroup_List

		public OrgDebtorGroupCollection DebtorGroup_List
		{
			get
			{
				if (fDebtorGroup_List == null)
				{
					fDebtorGroup_List = new OrgDebtorGroupCollection(Factory);
				}

				return fDebtorGroup_List;
			}
		}

		protected OrgDebtorGroupCollection fDebtorGroup_List;

		#endregion

		#region Branch_List

		public GlbBranchCollection Branch_List
		{
			get
			{
				if (fBranch_List == null)
				{
					fBranch_List = new GlbBranchCollection(Factory);
				}

				return fBranch_List;
			}
		}

		protected GlbBranchCollection fBranch_List;

		#endregion

		#region AccountsRelationship_List

		public ReadOnlyCodeDescriptionPairList AccountsRelationShip_List
		{
			get
			{
				if (fAccountsRelationShip_List == null)
				{
					fAccountsRelationShip_List = Env.Registry.ReceivablesCategoryList;
				}
				return fAccountsRelationShip_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fAccountsRelationShip_List;

		#endregion

		#region ConsolidationCategory_List

		public ReadOnlyCodeDescriptionPairList ConsolidationCategory_List =>
			fConsolidationCategory_List
			?? (fConsolidationCategory_List = AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.ToReadOnlyCodeDescriptionPairList());

		ReadOnlyCodeDescriptionPairList fConsolidationCategory_List;

		#endregion

		#region CreditRating_List

		public ReadOnlyCodeDescriptionPairList CreditRating_List
		{
			get
			{
				if (fCreditRating_List == null)
				{
					fCreditRating_List = Env.Registry.ARCreditRatingList;
				}
				return fCreditRating_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fCreditRating_List;

		#endregion

		#region SalesRep_List

		public GlbStaffCollection SalesRep_List
		{
			get
			{
				if (fSalesRep_List == null)
				{
					fSalesRep_List = new GlbStaffCollection(Factory);
				}

				return fSalesRep_List;
			}
		}

		protected GlbStaffCollection fSalesRep_List;

		#endregion

		#region CustomerServiceRep_List

		public GlbStaffCollection CustomerServiceRep_List
		{
			get
			{
				if (fCustomerServiceRep_List == null)
				{
					fCustomerServiceRep_List = new GlbStaffCollection(Factory);
				}

				return fCustomerServiceRep_List;
			}
		}

		protected GlbStaffCollection fCustomerServiceRep_List;

		#endregion

		#region CreditController_List

		public GlbStaffCollection CreditController_List
		{
			get
			{
				if (fCreditController_List == null)
				{
					fCreditController_List = new GlbStaffCollection(Factory);
				}

				return fCreditController_List;
			}
		}

		protected GlbStaffCollection fCreditController_List;

		#endregion

		#region CreditStatements_List

		public CodeDescriptionPairList CreditStatements_List
		{
			get
			{
				if (fCreditStatements_List == null)
				{
					fCreditStatements_List = new CodeDescriptionPairList();

					fCreditStatements_List.AddPair(CreditOptions.ExcludeDocumentsInCredit, Res.GetString("7e45cf4d-f49e-4a4f-b3dd-9b3a82b3b17b", "Exclude Documents with a Credit Total"));
					fCreditStatements_List.AddPair(CreditOptions.OnlyDocumentsInCredit, Res.GetString("9b7c591b-7b80-405a-9485-79d5eeb4b638", "Only Documents with a Credit Total"));
					fCreditStatements_List.AddPair(CreditOptions.AllDocuments, Res.GetString("00c6fdff-45cf-4951-97b3-38fb4a1515e1", "All Documents including those with a Credit Total"));
				}

				return fCreditStatements_List;
			}
		}

		CodeDescriptionPairList fCreditStatements_List;

		public static class CreditOptions
		{
			public const string ExcludeDocumentsInCredit = "EXC";
			public const string OnlyDocumentsInCredit = "CRD";
			public const string AllDocuments = "ALL";
		}

		#endregion

		#region TransactionCurrency_List

		public RefCurrencyCollection TransactionCurrency_List
		{
			get
			{
				if (fTransactionCurrency_List == null)
				{
					fTransactionCurrency_List = new RefCurrencyCollection(Factory);
				}

				return fTransactionCurrency_List;
			}
		}

		protected RefCurrencyCollection fTransactionCurrency_List;

		#endregion

		#region TransactionBranch_List

		public GlbBranchCollection TransactionBranch_List
		{
			get
			{
				if (fTransactionBranch_List == null)
				{
					fTransactionBranch_List = new GlbBranchCollection(Factory);
				}

				return fTransactionBranch_List;
			}
		}

		protected GlbBranchCollection fTransactionBranch_List;

		#endregion

		#region TransactionDepartment_List

		public GlbDepartmentCollection TransactionDepartment_List
		{
			get
			{
				if (fTransactionDepartment_List == null)
				{
					fTransactionDepartment_List = new GlbDepartmentCollection(Factory);
				}

				return fTransactionDepartment_List;
			}
		}

		protected GlbDepartmentCollection fTransactionDepartment_List;

		#endregion

		#region GroupByAccountMovementSOALine_List

		public CodeDescriptionPairList GroupByAccountMovementSOALine_List
		{
			get
			{
				if (fGroupByAccountMovementSOALine_List == null)
				{
					fGroupByAccountMovementSOALine_List = new CodeDescriptionPairList();
					fGroupByAccountMovementSOALine_List.AddPair(AccountMovementLineGroupingOptions.NO_GROUPING, Res.GetString("4E004B48-6607-4033-B1BE-6A4DF0CCCF27", "No Grouping"));
					fGroupByAccountMovementSOALine_List.AddPair(AccountMovementLineGroupingOptions.GROUP_BY_TRANSACTION_BRANCH, Res.GetString("59479A06-CF31-44FD-BEDB-EF1AADD82563", "Group by Transaction Branch"));
					fGroupByAccountMovementSOALine_List.AddPair(AccountMovementLineGroupingOptions.GROUP_BY_TRANSACTION_DEPARTMENT, Res.GetString("F9B3C677-122A-45C4-A5D0-CD5ED8116C3D", "Group by Transaction Department"));
					fGroupByAccountMovementSOALine_List.AddPair(AccountMovementLineGroupingOptions.GROUP_BY_TRANSACTION_BRANCH_DEPARTMENT, Res.GetString("2FAC2217-FCFB-4A3B-B927-4777681DBFF4", "Group by Transaction Branch and Department"));
				}

				return fGroupByAccountMovementSOALine_List;
			}
		}

		CodeDescriptionPairList fGroupByAccountMovementSOALine_List;

		#endregion

		#endregion

		#region Properties

		#region Document to Print GroupBox Properties

		#region DocumentToPrint

		[List("DocumentToPrint_List")]
		[MaxLength(3)]
		public ZString DocumentToPrint
		{
			get { return fDocumentToPrint; }
			set
			{
				if (value != fDocumentToPrint)
				{
					CheckMaximumLength(DocumentToPrintInfo, value);
					fDocumentToPrint = value;

					if (!IsValidationSuspended)
					{
						ValidateDocumentToPrint();
					}

					DocumentToPrintInfo.RefreshBinding();
					RaiseDocumentToPrintChangedEvent();
				}
			}
		}

		ZString fDocumentToPrint;

		public ZPropertyInfo DocumentToPrintInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DocumentToPrint));
			}
		}

		void RaiseDocumentToPrintChangedEvent()
		{
			if (fDocumentToPrint == Core.Constants.StatementCollectionLetterType.StatementOfAccount)
			{
				OutstandingAmountGreaterThan = 0;
			}
			else
			{
				CutOffPeriod = 0;
			}
		}

		#endregion

		#region IssueStatementPack

		[MaxLength(4)]
		[List("IssueStatementPack_List")]
		public ZString IssueStatementPack
		{
			get { return fIssueStatementPack; }
			set
			{
				if (value != fIssueStatementPack)
				{
					CheckMaximumLength(IssueStatementPackInfo, value);
					fIssueStatementPack = value;

					if (!IsValidationSuspended)
					{
						ValidateIssueStatementPack();
					}

					IssueStatementPackInfo.RefreshBinding();
				}
			}
		}

		ZString fIssueStatementPack;

		public ZPropertyInfo IssueStatementPackInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(IssueStatementPack));
			}
		}

		#endregion

		#region IssueBySettlementGroup

		public ZBool IssueBySettlementGroup
		{
			get { return fIssueBySettlementGroup; }
			set
			{
				if (value != fIssueBySettlementGroup)
				{
					fIssueBySettlementGroup = value;
					IssueBySettlementGroupInfo.RefreshBinding();
				}
			}
		}

		ZBool fIssueBySettlementGroup;
		public ZPropertyInfo IssueBySettlementGroupInfo
		{
			get { return GetZPropertyInfo(nameof(IssueBySettlementGroup)); }
		}

		#endregion

		#region IssueByTransactionBranch

		public ZBool IssueByTransactionBranch
		{
			get { return fIssueByTransactionBranch; }
			set
			{
				if (value != fIssueByTransactionBranch)
				{
					fIssueByTransactionBranch = value;
					IssueByTransactionBranchInfo.RefreshBinding();
				}
			}
		}

		ZBool fIssueByTransactionBranch;
		public ZPropertyInfo IssueByTransactionBranchInfo
		{
			get { return GetZPropertyInfo(nameof(IssueByTransactionBranch)); }
		}

		#endregion

		#region IssueByTransactionDepartment

		public ZBool IssueByTransactionDepartment
		{
			get { return fIssueByTransactionDepartment; }
			set
			{
				if (value != fIssueByTransactionDepartment)
				{
					fIssueByTransactionDepartment = value;
					IssueByTransactionDepartmentInfo.RefreshBinding();
				}
			}
		}

		ZBool fIssueByTransactionDepartment;
		public ZPropertyInfo IssueByTransactionDepartmentInfo
		{
			get { return GetZPropertyInfo(nameof(IssueByTransactionDepartment)); }
		}

		#endregion

		#region IncludeDebtorSummaryPage

		public ZBool IncludeDebtorSummaryPage
		{
			get { return fIncludeDebtorSummaryPage; }
			set
			{
				if (value != fIncludeDebtorSummaryPage)
				{
					fIncludeDebtorSummaryPage = value;
					IncludeDebtorSummaryPageInfo.RefreshBinding();
				}
			}
		}

		ZBool fIncludeDebtorSummaryPage;
		public ZPropertyInfo IncludeDebtorSummaryPageInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeDebtorSummaryPage)); }
		}

		#endregion

		#endregion

		#region Organisation GroupBox Properties

		#region Organisation

		[List("Organisation_List")]
		public virtual ZGuid OH_PK
		{
			get { return fOH_PK; }
			set
			{
				if (value != OH_PK)
				{
					fOH_PK = value;
					HasChanges = true;

					if (!IsValidationSuspended)
					{
						ValidateOH_PK();
					}

					OH_PKInfo.RefreshBinding();
				}
			}
		}

		ZGuid fOH_PK = ZGuid.Empty;

		public ZPropertyInfo OH_PKInfo
		{
			get { return GetZPropertyInfo(nameof(OH_PK)); }
		}

		#endregion

		#region BatchOfOrganisationsToPrint

		List<ZGuid> fBatchOfOrganisationsToPrint;
		public List<ZGuid> BatchOfOrganisationsToPrint
		{
			get
			{
				if (fBatchOfOrganisationsToPrint == null)
				{
					fBatchOfOrganisationsToPrint = new List<ZGuid>();
				}
				return fBatchOfOrganisationsToPrint;
			}
		}

		#endregion

		#region Debtor Group

		[List("DebtorGroup_List")]
		public virtual ZGuid OJ_PK
		{
			get { return fOJ_PK; }
			set
			{
				if (value != OJ_PK)
				{
					fOJ_PK = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateOJ_PK();
					}
					OJ_PKInfo.RefreshBinding();
				}
			}
		}

		ZGuid fOJ_PK = ZGuid.Empty;

		public ZPropertyInfo OJ_PKInfo
		{
			get { return GetZPropertyInfo(nameof(OJ_PK)); }
		}

		#endregion

		#region Branch

		[List("Branch_List")]
		public virtual ZGuid GB_PK
		{
			get { return fGB_PK; }
			set
			{
				if (value != GB_PK)
				{
					fGB_PK = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateGB_PK();
					}
					GB_PKInfo.RefreshBinding();
				}
			}
		}

		ZGuid fGB_PK = ZGuid.Empty;

		public ZPropertyInfo GB_PKInfo
		{
			get { return GetZPropertyInfo(nameof(GB_PK)); }
		}

		#endregion

		#region Accounts RelationShip

		[List("AccountsRelationShip_List")]
		[MaxLength(3)]
		public ZString AccountsRelationShip
		{
			get { return fAccountsRelationShip; }
			set
			{
				if (fAccountsRelationShip != value)
				{
					CheckMaximumLength(AccountsRelationShipInfo, value);
					fAccountsRelationShip = value;
					HasChanges = true;

					if (!IsValidationSuspended)
					{
						ValidateAccountsRelationShip();
					}

					AccountsRelationShipInfo.RefreshBinding();
				}
			}
		}

		ZString fAccountsRelationShip;

		public ZPropertyInfo AccountsRelationShipInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AccountsRelationShip));
			}
		}

		#endregion

		#region Consolidation Category

		[List("ConsolidationCategory_List")]
		[MaxLength(3)]
		public ZString ConsolidationCategory
		{
			get { return fConsolidationCategory; }
			set
			{
				if (fConsolidationCategory != value)
				{
					CheckMaximumLength(ConsolidationCategoryInfo, value);
					fConsolidationCategory = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateConsolidationCategory();
					}
					ConsolidationCategoryInfo.RefreshBinding();
				}
			}
		}

		ZString fConsolidationCategory;

		public ZPropertyInfo ConsolidationCategoryInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ConsolidationCategory));
			}
		}

		#endregion

		#region Credit Rating

		[List("CreditRating_List")]
		[MaxLength(3)]
		public ZString CreditRating
		{
			get { return fCreditRating; }
			set
			{
				if (fCreditRating != value)
				{
					CheckMaximumLength(CreditRatingInfo, value);
					fCreditRating = value;
					HasChanges = true;

					if (!IsValidationSuspended)
					{
						ValidateCreditRating();
					}

					CreditRatingInfo.RefreshBinding();
				}
			}
		}

		ZString fCreditRating;

		public ZPropertyInfo CreditRatingInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CreditRating));
			}
		}

		#endregion

		#region Sales Rep

		[List("SalesRep_List")]
		public virtual ZGuid SalesRep_PK
		{
			get { return fSalesRep_PK; }
			set
			{
				if (value != fSalesRep_PK)
				{
					fSalesRep_PK = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateSalesRep_PK();
					}
					SalesRep_PKInfo.RefreshBinding();
				}
			}
		}

		ZGuid fSalesRep_PK = ZGuid.Empty;

		public ZPropertyInfo SalesRep_PKInfo
		{
			get { return GetZPropertyInfo(nameof(SalesRep_PK)); }
		}

		ZString SalesRep_Code
		{
			get
			{
				GlbStaff staff = Factory.Load<GlbStaff>(SalesRep_PK);
				return staff == null ? ZString.Empty : staff.GS_Code;
			}
		}

		#endregion

		#region Customer Service Rep

		[List("CustomerServiceRep_List")]
		public virtual ZGuid CustomerServiceRep_PK
		{
			get { return fCustomerServiceRep_PK; }
			set
			{
				if (value != fCustomerServiceRep_PK)
				{
					fCustomerServiceRep_PK = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateCustomerServiceRep_PK();
					}
					CustomerServiceRep_PKInfo.RefreshBinding();
				}
			}
		}

		ZGuid fCustomerServiceRep_PK = ZGuid.Empty;

		public ZPropertyInfo CustomerServiceRep_PKInfo
		{
			get { return GetZPropertyInfo(nameof(CustomerServiceRep_PK)); }
		}

		ZString CustomerServiceRep_Code
		{
			get
			{
				GlbStaff staff = Factory.Load<GlbStaff>(CustomerServiceRep_PK);
				return staff == null ? ZString.Empty : staff.GS_Code;
			}
		}

		#endregion

		#region Credit Controller

		[List("CreditController_List")]
		public virtual ZGuid CreditController_PK
		{
			get { return fCreditController_PK; }
			set
			{
				if (value != fCreditController_PK)
				{
					fCreditController_PK = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateCreditController_PK();
					}
					CreditController_PKInfo.RefreshBinding();
				}
			}
		}

		ZGuid fCreditController_PK = ZGuid.Empty;

		public ZPropertyInfo CreditController_PKInfo
		{
			get { return GetZPropertyInfo(nameof(CreditController_PK)); }
		}

		ZString CreditController_Code
		{
			get
			{
				GlbStaff staff = Factory.Load<GlbStaff>(CreditController_PK);
				return staff == null ? ZString.Empty : staff.GS_Code;
			}
		}

		#endregion

		#region Credit Statement Options

		[List("CreditStatements_List")]
		[MaxLength(3)]
		public ZString CreditStatements
		{
			get { return fCreditStatements; }
			set
			{
				if (value != fCreditStatements)
				{
					CheckMaximumLength(CreditStatementsInfo, value);
					fCreditStatements = value;
					HasChanges = true;

					if (!IsValidationSuspended)
					{
						ValidateCreditStatements();
					}

					CreditStatementsInfo.RefreshBinding();
				}
			}
		}

		ZString fCreditStatements = CreditOptions.ExcludeDocumentsInCredit;

		public ZPropertyInfo CreditStatementsInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(CreditStatements));
			}
		}

		#endregion

		#endregion

		#region Transaction GroupBox Properties

		#region Transaction Currency

		[List("TransactionCurrency_List")]
		public virtual ZGuid RX_PK
		{
			get { return fRX_PK; }
			set
			{
				if (value != RX_PK)
				{
					fRX_PK = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateRX_PK();
					}
					RX_PKInfo.RefreshBinding();
				}
			}
		}

		ZGuid fRX_PK = ZGuid.Empty;

		public ZPropertyInfo RX_PKInfo
		{
			get { return GetZPropertyInfo(nameof(RX_PK)); }
		}

		#endregion

		#region Transaction Branch

		[List("TransactionBranch_List")]
		public virtual ZGuid TransactionBranch_PK
		{
			get { return fTransactionBranch_PK; }
			set
			{
				if (value != TransactionBranch_PK)
				{
					fTransactionBranch_PK = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateTransactionBranch_PK();
					}
					TransactionBranch_PKInfo.RefreshBinding();
				}
			}
		}

		ZGuid fTransactionBranch_PK = ZGuid.Empty;

		public ZPropertyInfo TransactionBranch_PKInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionBranch_PK)); }
		}

		public bool TransactionBranch_PK_ReadOnly
		{
			get { return !IssueByTransactionBranch; }
		}

		#endregion

		#region Transaction Department

		[List("TransactionDepartment_List")]
		public virtual ZGuid TransactionDepartment_PK
		{
			get { return fTransactionDepartment_PK; }
			set
			{
				if (value != TransactionDepartment_PK)
				{
					fTransactionDepartment_PK = value;
					HasChanges = true;
					if (!IsValidationSuspended)
					{
						ValidateTransactionDepartment_PK();
					}
					TransactionDepartment_PKInfo.RefreshBinding();
				}
			}
		}

		ZGuid fTransactionDepartment_PK = ZGuid.Empty;

		public ZPropertyInfo TransactionDepartment_PKInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionDepartment_PK)); }
		}

		public bool TransactionDepartment_PK_ReadOnly
		{
			get { return !IssueByTransactionDepartment; }
		}

		#endregion

		#region Transaction CutOff Date

		public virtual ZDateTime CutOffDate
		{
			get { return fCutOffDate; }
			set
			{
				if (value != CutOffDate)
				{
					fCutOffDate = value;
					HasChanges = true;
					SetDefaultValuesForAccountFee();

					if (!IsValidationSuspended)
					{
						ValidateCutOffDate();
					}
					CutOffDateInfo.RefreshBinding();
				}
			}
		}

		ZDateTime fCutOffDate = ZDateTime.Empty;

		public ZPropertyInfo CutOffDateInfo
		{
			get { return GetZPropertyInfo(nameof(CutOffDate)); }
		}

		#endregion

		#region Statement Only: CutOff Period

		public virtual ZInt CutOffPeriod
		{
			get { return fCutOffPeriod; }
			set
			{
				if (value != CutOffPeriod)
				{
					fCutOffPeriod = value;
					HasChanges = true;
					SetDefaultValuesForAccountFee();

					if (!IsValidationSuspended)
					{
						ValidateCutOffPeriod();
					}
					CutOffPeriodInfo.RefreshBinding();
				}
			}
		}

		ZInt fCutOffPeriod = 0;

		public ZPropertyInfo CutOffPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(CutOffPeriod)); }
		}

		ZDateTime EndOfPeriod
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (CutOffPeriod > 0)
				{
					result = PeriodCalculator.GetLastDayForPeriod(CutOffPeriod);
				}
				else
				{
					result = PeriodCalculator.GetLastDayForPeriod(ZDateTime.Now);
				}

				if (!result.IsValid)
				{
					result = ZDateTime.Now.Date.AddDays(1);
				}

				return result;
			}
		}

		#endregion

		#region Collection Letter Only: Outstanding Amount Greater Than

		// eg. user enters a filter value of 100
		// transaction a with +100 as well as -100

		public ZDecimal OutstandingAmountGreaterThan
		{
			get { return fOutstandingAmountGreaterThan; }
			set
			{
				if (value != fOutstandingAmountGreaterThan)
				{
					fOutstandingAmountGreaterThan = value;
					if (!IsValidationSuspended)
					{
						ValidateOutstandingAmountGreaterThan();
					}
					OutstandingAmountGreaterThanInfo.RefreshBinding();
				}
			}
		}

		ZDecimal fOutstandingAmountGreaterThan;

		public ZPropertyInfo OutstandingAmountGreaterThanInfo
		{
			get { return GetZPropertyInfo(nameof(OutstandingAmountGreaterThan)); }
		}

		#endregion

		#region Collection Letter Only: Disbursement Invoices Only

		public ZBool DisbursementInvoicesOnly
		{
			get { return disbursementInvoicesOnly; }
			set { SetNonPersistentPropertyValue(DisbursementInvoicesOnlyInfo, ref disbursementInvoicesOnly, value); }
		}

		ZBool disbursementInvoicesOnly;

		public ZPropertyInfo DisbursementInvoicesOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(DisbursementInvoicesOnly)); }
		}

		#endregion

		#endregion

		#region Company

		public GlbCompany Company
		{
			get { return Branch.Company; }
		}

		#endregion

		#region Branch

		public GlbBranch Branch
		{
			get { return fBranch; }
		}

		readonly GlbBranch fBranch;

		#endregion

		#region PrintAccountMovementSOA properties

		#region PrintAccountMovementSOA
		public ZBool PrintAccountMovementSOA
		{
			get { return fPrintAccountMovementSOA; }
			set
			{
				if (SetNonPersistentPropertyValue(PrintAccountMovementSOAInfo, ref fPrintAccountMovementSOA, value))
				{
					if (fPrintAccountMovementSOA)
					{
						if (string.IsNullOrEmpty(GroupByAccountMovementSOALine))
						{
							GroupByAccountMovementSOALine = AccountMovementLineGroupingOptions.NO_GROUPING;
						}

						if (!IsValidationSuspended)
						{
							ValidateDocumentToPrint();
							ValidatePrintAccountMovementFromDate();
							ValidatePrintAccountMovementToDate();
						}
					}

					ClearInappropriateFieldsForAccountMovementSOA();
				}
			}
		}
		ZBool fPrintAccountMovementSOA;

		public ZPropertyInfo PrintAccountMovementSOAInfo
		{
			get { return GetZPropertyInfo(nameof(PrintAccountMovementSOA)); }
		}

		#endregion

		#region PrintAccountMovementFromDate
		public ZDateTime PrintAccountMovementFromDate
		{
			get { return fPrintAccountMovementFromDate; }
			set
			{
				if (SetNonPersistentPropertyValue(PrintAccountMovementFromDateInfo, ref fPrintAccountMovementFromDate, value))
				{
					SetDefaultValuesForAccountFee();

					if (!IsValidationSuspended)
					{
						ValidatePrintAccountMovementFromDate();
					}
				}
			}
		}
		ZDateTime fPrintAccountMovementFromDate;

		public ZPropertyInfo PrintAccountMovementFromDateInfo
		{
			get { return GetZPropertyInfo(nameof(PrintAccountMovementFromDate)); }
		}
		#endregion

		#region PrintAccountMovementToDate
		public ZDateTime PrintAccountMovementToDate
		{
			get { return fPrintAccountMovementToDate; }
			set
			{
				if (SetNonPersistentPropertyValue(PrintAccountMovementToDateInfo, ref fPrintAccountMovementToDate, value))
				{
					SetDefaultValuesForAccountFee();

					if (!IsValidationSuspended)
					{
						ValidatePrintAccountMovementToDate();
					}
				}
			}
		}
		ZDateTime fPrintAccountMovementToDate;

		public ZPropertyInfo PrintAccountMovementToDateInfo
		{
			get { return GetZPropertyInfo(nameof(PrintAccountMovementToDate)); }
		}
		#endregion

		#region GroupByAccountMovementSOALine

		[List("GroupByAccountMovementSOALine_List")]
		[MaxLength(3)]
		public ZString GroupByAccountMovementSOALine
		{
			get { return fGroupByAccountMovementSOALine; }
			set
			{
				SetNonPersistentPropertyValue(GroupByAccountMovementSOALineInfo, ref fGroupByAccountMovementSOALine, value);
				if (!IsValidationSuspended)
				{
					GroupByAccountMovementSOALineInfo.ClearAllNotifications();
					ListValidation.ErrorIfInvalidCode(GroupByAccountMovementSOALineInfo, GroupByAccountMovementSOALine_List);
				}
			}
		}
		ZString fGroupByAccountMovementSOALine;

		public ZPropertyInfo GroupByAccountMovementSOALineInfo
		{
			get { return GetZPropertyInfo(nameof(GroupByAccountMovementSOALine)); }
		}
		#endregion

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateDocumentToPrint();
			ValidatePrintAccountMovementSOA();

			ValidateOH_PK();
			ValidateOJ_PK();
			ValidateGB_PK();

			ValidateAccountsRelationShip();
			ValidateConsolidationCategory();
			ValidateCreditRating();

			ValidateSalesRep_PK();
			ValidateCustomerServiceRep_PK();
			ValidateCreditController_PK();

			ValidateCreditStatements();

			ValidateRX_PK();
			ValidateCutOffDate();
			ValidateCutOffPeriod();
			ValidateOutstandingAmountGreaterThan();

			base.RunPreSaveValidationCore();
		}

		public void ValidateDocumentToPrint()
		{
			DocumentToPrintInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(DocumentToPrintInfo, DocumentToPrint_List);
			CheckUserHasSecurityRightsToPrintDocument();
			CheckCorrectDocumentTypeIsSelected();
		}

		void CheckUserHasSecurityRightsToPrintDocument()
		{
			SecurityCheckpoint checkpoint = null;

			switch (DocumentToPrint)
			{
				case Core.Constants.StatementCollectionLetterType.StatementOfAccount:
					checkpoint = Env.Security.ReceivablesPrintStatement;
					break;

				case Core.Constants.StatementCollectionLetterType.FirstReminder:
					checkpoint = Env.Security.ReceivablesPrintFirstReminder;
					break;

				case Core.Constants.StatementCollectionLetterType.SecondReminder:
					checkpoint = Env.Security.ReceivablesPrintSecondReminder;
					break;

				case Core.Constants.StatementCollectionLetterType.CollectionLetter:
					checkpoint = Env.Security.ReceivablesPrintCollectionLetter;
					break;

				case Core.Constants.StatementCollectionLetterType.DemandLetter:
					checkpoint = Env.Security.ReceivablesPrintDemandLetter;
					break;
			}

			if (checkpoint != null && !checkpoint.IsAllowed)
			{
				ZString errorMessageToDisplay = Res.GetString("ab9f0547-0849-4d47-b73b-ed41fd649a6a", "You do not have the appropriate security rights to print this type of document.") + System.Environment.NewLine;
				errorMessageToDisplay += Res.GetString("d108160a-a45a-4ce2-8a9d-bf4c64cc9f2c", "If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow Access to:") + System.Environment.NewLine;
				errorMessageToDisplay += checkpoint.DisplayTextPathToSecurityRight;

				DocumentToPrintInfo.AddError(errorMessageToDisplay);
			}
		}

		void CheckCorrectDocumentTypeIsSelected()
		{
			if (PrintAccountMovementSOA && DocumentToPrint != Core.Constants.StatementCollectionLetterType.StatementOfAccount)
			{
				DocumentToPrintInfo.AddError(Res.GetString("7fed3ad2-c681-4856-a65d-aba99c4121ef", "Account Movement Listing can be printed as Statement of Account only. Please select 'SOA' as Document To print or untick 'Print Account Movement Listing'."));
			}
		}

		public virtual void ValidateOH_PK()
		{
			OH_PKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(OH_PKInfo, Organisation_List);
		}

		public virtual void ValidateOJ_PK()
		{
			OJ_PKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(OJ_PKInfo, DebtorGroup_List);
		}

		public virtual void ValidateGB_PK()
		{
			GB_PKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(GB_PKInfo, Branch_List);
		}

		public void ValidateAccountsRelationShip()
		{
			AccountsRelationShipInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(AccountsRelationShipInfo, AccountsRelationShip_List);
		}

		public void ValidateConsolidationCategory()
		{
			ConsolidationCategoryInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ConsolidationCategoryInfo, ConsolidationCategory_List);
		}

		public void ValidateCreditRating()
		{
			CreditRatingInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CreditRatingInfo, CreditRating_List);
		}

		public virtual void ValidateSalesRep_PK()
		{
			SalesRep_PKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(SalesRep_PKInfo, SalesRep_List);
		}

		public virtual void ValidateCustomerServiceRep_PK()
		{
			CustomerServiceRep_PKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(CustomerServiceRep_PKInfo, CustomerServiceRep_List);
		}

		public virtual void ValidateCreditController_PK()
		{
			CreditController_PKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(CreditController_PKInfo, CreditController_List);
		}

		public void ValidateCreditStatements()
		{
			CreditStatementsInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CreditStatementsInfo, CreditStatements_List);
		}

		public void ValidateIssueStatementPack()
		{
			IssueStatementPackInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(IssueStatementPackInfo, IssueStatementPack_List);
		}

		public virtual void ValidateRX_PK()
		{
			RX_PKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(RX_PKInfo, TransactionCurrency_List);
		}

		public virtual void ValidateTransactionBranch_PK()
		{
			TransactionBranch_PKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(TransactionBranch_PKInfo, TransactionBranch_List);
		}

		public virtual void ValidateTransactionDepartment_PK()
		{
			TransactionDepartment_PKInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(TransactionDepartment_PKInfo, TransactionDepartment_List);
		}

		public virtual void ValidateCutOffDate()
		{
			CutOffDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeAndRange(CutOffDateInfo);
		}

		public virtual void ValidateCutOffPeriod()
		{
			CutOffPeriodInfo.ClearAllNotifications();

			if (CutOffPeriod != 0)
			{
				if (!PeriodCalculator.IsPeriodValid(CutOffPeriod))
				{
					CutOffPeriodInfo.AddError(Res.GetString("500f5824-184f-4230-9fdc-0542cd503cad", "You must enter a valid period."));
				}
				else if (AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.Value && !PeriodCalculator.IsCurrentPeriod(CutOffPeriod))
				{
					CutOffPeriodInfo.AddError(Res.GetString("22f257f0-4cac-41c9-89d6-f89ec272f86f", "With the Invoice Payment Web Service enabled it must be only the current period"));
				}
			}
		}

		public void ValidateOutstandingAmountGreaterThan()
		{
			OutstandingAmountGreaterThanInfo.ClearAllNotifications();

			if (OutstandingAmountGreaterThan < 0)
			{
				OutstandingAmountGreaterThanInfo.AddError(Res.GetString("d06a4bb3-e330-49b1-8c60-08a5a3f079f0", "Outstanding Amount must be greater than zero"));
			}
		}

		public void ValidatePrintAccountMovementSOA()
		{
			if (PrintAccountMovementSOA && !IsValidationSuspended)
			{
				ValidatePrintAccountMovementFromDate();
				ValidatePrintAccountMovementToDate();
				ValidateGroupByAccountMovementSOALine();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property Name")]
		public void ValidatePrintAccountMovementFromDate()
		{
			PrintAccountMovementFromDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PrintAccountMovementFromDateInfo, "Print Account Movement From Date");
			TypeValidation.CheckValidSmallDateTime(PrintAccountMovementFromDateInfo);
			TypeValidation.CheckValidZDateTimeAndRange(PrintAccountMovementFromDateInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Property Name")]
		public void ValidatePrintAccountMovementToDate()
		{
			PrintAccountMovementToDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PrintAccountMovementToDateInfo, "Print Account Movement To Date");
			TypeValidation.CheckValidSmallDateTime(PrintAccountMovementToDateInfo);
			TypeValidation.CheckValidZDateTimeAndRange(PrintAccountMovementToDateInfo);
		}

		public void ValidateGroupByAccountMovementSOALine()
		{
			GroupByAccountMovementSOALineInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(GroupByAccountMovementSOALineInfo, GroupByAccountMovementSOALine_List);
		}

		#endregion

		#region Printing

		bool UseNewPrintStreaming
		{
			get { return AccountingMasterFilesRegistry.Instance.StatementUsePrintStreaming.Value; }
		}

		public event NoDocumentsToPrintEventHandler NoDocumentsToPrint;

		public void PrintStatements()
		{
			PrintTask task = UseNewPrintStreaming ? GetPrintTaskIEnum() : GetPrintTask();

			if (task != null)
			{
				task.Run(Env.Security.None);
			}
		}

		ZGuid MenuPKForStatementOfAccount
		{
			get { return new ZGuid("9e1d99c6-6189-4497-a271-10605e14a4be"); }
		}

		public DocumentPrintSet GetPrintTask()
		{
			DynamicBusinessObjectCollection businessObjCollection = GetBusinessObjCollection();

			if (businessObjCollection.Count != 0)
			{
				Dictionary<string, DocumentPack> docPackHash = new Dictionary<string, DocumentPack>();
				Dictionary<string, PrintStatement> printStatements = new Dictionary<string, PrintStatement>();

				foreach (BusinessObject currentBisObj in businessObjCollection)
				{
					ZGuid organisationPK = new ZGuid(currentBisObj[Constants.OH_PK]);
					ZString currencyNK = new ZString(currentBisObj[Constants.AH_RX_NKTransactionCurrency]);
					ZString organisationCode = (ZString)currentBisObj[Constants.OH_Code];
					ZBool isMultipleCurrency = false;
					ZDecimal openingBalance = 0m;
					if (PrintAccountMovementSOA)
					{
						isMultipleCurrency = (ZInt)currentBisObj[Constants.isMultipleCurrency] == 1;
						openingBalance = (ZDecimal)currentBisObj[Constants.OpeningBalance];
					}

					string key = organisationCode + Company.PK.ToString();

					ZGuid transactionBranchPKForDocPack = ZGuid.Empty;
					if (IssueByTransactionBranch)
					{
						transactionBranchPKForDocPack = new ZGuid(currentBisObj[Constants.AH_GB]);
						key += transactionBranchPKForDocPack.ToString();
					}

					ZGuid transactionDepartmentPKForDocPack = ZGuid.Empty;
					if (IssueByTransactionDepartment)
					{
						transactionDepartmentPKForDocPack = new ZGuid(currentBisObj[Constants.AH_GE]);
						key += transactionDepartmentPKForDocPack.ToString();
					}

					key += currencyNK.ToString();

					printStatements.Add(key, AppendToDocumentPack(docPackHash, organisationPK, currencyNK, transactionBranchPKForDocPack, transactionDepartmentPKForDocPack, isMultipleCurrency, openingBalance, organisationCode + Company.PK.ToString()));
				}

				foreach (PrintStatement printStatementBizObj in printStatements.Values)
				{
					if (ShouldAttachInvoices(printStatementBizObj))
					{
						AppendTransactionsToDocumentPack(docPackHash, printStatementBizObj);
					}
				}

				DocumentCommand statementOfAccountCommand = GetStatementOfAccountCommand(printStatements);
				ArrayList codes = new ArrayList(docPackHash.Keys);
				codes.Sort();
				DocumentCommand statementSummaryCommand = GetStatementSummaryCommand(codes, docPackHash, printStatements);
				DocumentCommand taskCommand = (statementSummaryCommand ?? statementOfAccountCommand);

				DocumentPrintSet task = UseNewPrintStreaming ? new DocumentPrintSet(taskCommand) : new DocumentPrintSet(taskCommand, null);
				if (!UseNewPrintStreaming)
				{
					task.Clear();
				}

				task.DeliveryInstructionsDefaultPK = MenuPKForStatementOfAccount;

				foreach (string code in codes)
				{
					DocumentPack pack = docPackHash[code];
					if (IncludeDebtorSummaryPage && pack.Count > 1)
					{
						pack.InsertAtStart(GetSummaryDocumentPack(code, printStatements).GetFirstReport());
					}
					task.Add(pack);
				}
				return task;
			}
			else
			{
				ShowNoDocumentsToPrintMessage();
				return null;
			}
		}

		#region TestIEnum

		SortedDictionary<string, List<BusinessObject>> DataGroupedByOrgCode
		{
			get
			{
				return dataGroupedByOrgCode ?? (dataGroupedByOrgCode = GetDataGroupedByOrgCode());
			}
		}
		SortedDictionary<string, List<BusinessObject>> dataGroupedByOrgCode;

		SortedDictionary<string, List<BusinessObject>> GetDataGroupedByOrgCode()
		{
			var result = new SortedDictionary<string, List<BusinessObject>>();
			var businessObjCollection = GetBusinessObjCollection();

			if (businessObjCollection.Any())
			{
				foreach (BusinessObject bizObj in businessObjCollection)
				{
					var organisationCode = (ZString)bizObj[Constants.OH_Code];
					var groupByOrgCodeKey = organisationCode + Company.PK.ToString();

					if (result.Keys.Contains(groupByOrgCodeKey))
					{
						var value = result[groupByOrgCodeKey];
						value.Add(bizObj);
					}
					else
					{
						var value = new List<BusinessObject>();
						value.Add(bizObj);
						result.Add(groupByOrgCodeKey, value);
					}
				}
			}

			return result;
		}

		void ClearCachedData()
		{
			dataGroupedByOrgCode = null;
		}

		public DocumentPrintSet GetPrintTaskIEnum()
		{
			ClearCachedData();
			var printStatementsByOrgCode = new SortedDictionary<string, List<PrintStatement>>();

			foreach (var orgStatementGroup in DataGroupedByOrgCode)
			{
				var code = orgStatementGroup.Key;
				var printStatements = new List<PrintStatement>();
				printStatements.AddRange(orgStatementGroup.Value.Select(x => GetPrintStatement(x)));

				printStatementsByOrgCode.Add(code, printStatements);
			}

			if (printStatementsByOrgCode.Any())
			{
				var firstStatement = printStatementsByOrgCode.Values.First().First();
				var statementOfAccountCommand = GetStatementDocumentCommand(firstStatement);

				DocumentCommand statementSummaryCommand = null;
				if (IncludeDebtorSummaryPage)
				{
					foreach (var statementsByOrgCode in printStatementsByOrgCode)
					{
						var code = statementsByOrgCode.Key;
						var statements = statementsByOrgCode.Value;

						if (statements.Count > 1)
						{
							statementSummaryCommand = (DocumentCommand)GetSummaryDocumentPackFromListOfPrintStatements(code, statements.Skip(1).ToList()).StmMenuCommand;

							break;
						}
					}
				}

				var taskCommand = statementSummaryCommand ?? statementOfAccountCommand;
				DocumentPrintSet task;

				var totalDocument = printStatementsByOrgCode.Sum(x => x.Value.Sum(y => 1 + (ShouldAttachInvoices(y) ? y.TransactionCountForAttachment : 0)));

				if ((DataGroupedByOrgCode.Count > 1 && totalDocument > PrintTask.MaxPreviewCount)
#if DEBUG
 || (Globals.IsTest && TestAboveMaxPreviewCount)
#endif
)
				{
					task = new DocumentPrintSetWithStreaming(taskCommand, DataGroupedByOrgCode.Count, GetPacksIEnum(), DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.Value);
				}
				else
				{
					task = new DocumentPrintSet(taskCommand);
					task.AddRange(GetPacksIEnum().ToList());
				}
				task.DeliveryInstructionsDefaultPK = MenuPKForStatementOfAccount;

				return task;
			}
			else
			{
				ShowNoDocumentsToPrintMessage();
				return null;
			}
		}

		IEnumerable<DocumentPack> GetPacksIEnum()
		{
			foreach (var bisObjs in dataGroupedByOrgCode)
			{
				var code = bisObjs.Key;
				var businessObjectsByOrgCode = bisObjs.Value;

				var firstStatement = GetPrintStatement(businessObjectsByOrgCode.First());

				var statements = new List<PrintStatement>();
				statements.Add(firstStatement);
				var command = GetStatementDocumentCommand(firstStatement);
				var docPack = new DocumentPack(command, firstStatement, null, null);

				var organisation = Factory.Load<OrgHeader>(firstStatement.OrganisationPK);
				docPack.ForceBusinessObjectToLogAgainst(organisation.CompanyData);

				// statement for the same Org should be in the same DocPack
				if (businessObjectsByOrgCode.Count > 1)
				{
					foreach (var businessObject in businessObjectsByOrgCode.Skip(1))
					{
						var nextStatementBisObj = GetPrintStatement(businessObject);
						docPack.AddReportsToPack(command, null, nextStatementBisObj, null);
						statements.Add(nextStatementBisObj);
					}
				}

				// attach invoices if required
				foreach (var statement in statements)
				{
					if (ShouldAttachInvoices(statement))
					{
						AppendTransactionsToDocumentPack(docPack, statement.GetInvoicesForAttachment());
					}
				}

				// prepare summary page if required
				if (IncludeDebtorSummaryPage && docPack.Count > 1)
				{
					docPack.InsertAtStart(GetSummaryDocumentPackFromListOfPrintStatements(code, statements).GetFirstReport());
				}

				yield return docPack;
			}
		}

		DocumentPack GetSummaryDocumentPackFromListOfPrintStatements(string code, List<PrintStatement> listOfPrintStatements)
		{
			Dictionary<string, PrintStatement> printStatements = new Dictionary<string, PrintStatement>();

			foreach (var printStatement in listOfPrintStatements)
			{
				string key = code;
				ZGuid transactionBranchPKForDocPack = ZGuid.Empty;
				if (IssueByTransactionBranch)
				{
					transactionBranchPKForDocPack = new ZGuid(printStatement.TransactionBranchPK);
					key += transactionBranchPKForDocPack.ToString();
				}

				ZGuid transactionDepartmentPKForDocPack = ZGuid.Empty;
				if (IssueByTransactionDepartment)
				{
					transactionDepartmentPKForDocPack = new ZGuid(printStatement.TransactionDepartmentPK);
					key += transactionDepartmentPKForDocPack.ToString();
				}

				key += printStatement.CurrencyNK.ToString();

				printStatements.Add(key, printStatement);
			}

			return GetSummaryDocumentPack(code, printStatements);
		}

		PrintStatement GetPrintStatement(BusinessObject currentBisObj)
		{
			ZGuid transactionBranchPK = ZGuid.Empty;
			if (IssueByTransactionBranch)
			{
				transactionBranchPK = new ZGuid(currentBisObj[Constants.AH_GB]);
			}

			ZGuid transactionDepartmentPK = ZGuid.Empty;
			if (IssueByTransactionDepartment)
			{
				transactionDepartmentPK = new ZGuid(currentBisObj[Constants.AH_GE]);
			}

			ZBool isMultipleCurrency = false;
			ZDecimal openingBalance = 0M;
			ZString statementCurrency = "";
			if (PrintAccountMovementSOA)
			{
				isMultipleCurrency = ((ZInt)currentBisObj[Constants.isMultipleCurrency] == 1);
				openingBalance = (ZDecimal)currentBisObj[Constants.OpeningBalance];
			}

			PrintStatement statementBisObj = GetNewPrintStatement(new BusinessObjectFactory(), isMultipleCurrency, openingBalance);
			statementBisObj.DocumentToPrint = DocumentToPrint;
			statementBisObj.CurrencyNK = new ZString(currentBisObj[Constants.AH_RX_NKTransactionCurrency]);
			statementBisObj.OrganisationPK = new ZGuid(currentBisObj[Constants.OH_PK]);
			statementBisObj.IssueBySettlementGroup = IssueBySettlementGroup;
			statementBisObj.IssueByTransactionBranch = IssueByTransactionBranch;
			statementBisObj.IssueByTransactionDepartment = IssueByTransactionDepartment;
			statementBisObj.TransactionBranchPK = transactionBranchPK;
			statementBisObj.TransactionDepartmentPK = transactionDepartmentPK;

			SetStatementDisplayDateForStatement(statementBisObj);

			if (DocumentToPrint == Core.Constants.StatementCollectionLetterType.StatementOfAccount)
			{
				SetPropertiesOnPrintStatementForStatements(statementBisObj);
			}
			else
			{
				SetPropertiesOnPrintStatementForCollectionLetters(statementBisObj);
			}
			return statementBisObj;
		}

		#endregion

		DocumentCommand GetStatementOfAccountCommand(Dictionary<string, PrintStatement> printStatements)
		{
			return GetStatementDocumentCommand(printStatements.First().Value);
		}

		DocumentCommand GetStatementSummaryCommand(ArrayList codes, Dictionary<string, DocumentPack> docPackHash, Dictionary<string, PrintStatement> printStatements)
		{
			DocumentCommand statementSummaryCommand = null;
			foreach (string code in codes)
			{
				DocumentPack pack = docPackHash[code];
				if (IncludeDebtorSummaryPage && pack.Count > 1)
				{
					statementSummaryCommand = (DocumentCommand)GetSummaryDocumentPack(code, printStatements).StmMenuCommand;
					break;
				}
			}
			return statementSummaryCommand;
		}

		bool ShouldAttachInvoices(PrintStatement statementBizObj)
		{
			bool attachInvoices = false;
			if (IssueStatementPack == AccountingConstants.IssueStatementPackType.StatementOnly)
			{
				attachInvoices = false;
			}
			else if (IssueStatementPack == AccountingConstants.IssueStatementPackType.StatementAndInvoices)
			{
				attachInvoices = true;
			}
			else if (IssueStatementPack == AccountingConstants.IssueStatementPackType.Default && statementBizObj.Organisation != null)
			{
				attachInvoices = statementBizObj.Organisation.MiscServ.OM_ARCombinedStatementInvoice;
			}
			return attachInvoices;
		}

		protected void ShowNoDocumentsToPrintMessage()
		{
			ZString documentName;

			switch (DocumentToPrint)
			{
				case Core.Constants.StatementCollectionLetterType.FirstReminder:
					documentName = Res.GetString("872cd58b-55dc-49c2-b7c8-bad672d4f89c", "first reminder letters");
					break;

				case Core.Constants.StatementCollectionLetterType.SecondReminder:
					documentName = Res.GetString("184a2f37-8962-4ed4-b49c-d2d094768eb0", "second reminder letters");
					break;

				case Core.Constants.StatementCollectionLetterType.CollectionLetter:
					documentName = Res.GetString("f1103ed2-9dd3-4a2d-acf7-3420ebfa7c99", "collection letters");
					break;

				case Core.Constants.StatementCollectionLetterType.DemandLetter:
					documentName = Res.GetString("7d9fd3f8-0971-4858-ad2a-49a15e0d9c27", "demand letters");
					break;

				default:
					documentName = Res.GetString("5981c007-abf6-4f6a-8492-fcbd56ecea22", "statements");
					break;
			}

			ZString message = Res.GetString("603e5069-ebae-437c-aa45-6731e3b533f0", "Based on the given criteria, there are currently no {0} to print.", documentName);
			ZString caption = Res.GetString("c101a1b7-96d9-440d-9032-0d6d796a2497", "No {0} to Print", documentName);

			RaiseStatementEvent(message, caption);
		}

		void RaiseStatementEvent(string message, string caption)
		{
			if (NoDocumentsToPrint != null)
			{
				NoDocumentsToPrint(this, new NoDocumentsToPrintEventArgs(message, caption));
			}
		}

		protected internal virtual PrintStatement GetNewPrintStatement(BusinessObjectFactory statementFactory = null, bool isMultipleCurrency = false, decimal openingBalance = -1M)
		{
			if (PrintAccountMovementSOA)
			{
				return new PrintStatementForAccountMovement(statementFactory ?? Factory, Branch, isMultipleCurrency, openingBalance);
			}
			else
			{
				return new PrintStatement(statementFactory ?? Factory, Branch);
			}
		}

		DocumentPack GetSummaryDocumentPack(string code, Dictionary<string, PrintStatement> printStatements)
		{
			Dictionary<string, PrintStatement> packsPrintStatements = new Dictionary<string, PrintStatement>();
			foreach (string key in printStatements.Keys)
			{
				if (key.Contains(code))
				{
					packsPrintStatements.Add(key, printStatements[key]);
				}
			}

			PrintSummary summary = new PrintSummary(UseNewPrintStreaming ? new BusinessObjectFactory() : Factory, packsPrintStatements);

			ZString summaryMenuName = (NoResString)"Statement Summary";
			ZQuery commandFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, summaryMenuName);
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsPublished, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueChar);
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Equal, DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.Value ? string.Empty : Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments);
			DocumentCommandCollection documentCommands = new DocumentCommandCollection(summary);
			documentCommands.Load();
			BusinessObject[] commands = documentCommands.Find(commandFilter);
			if (commands.Length < 1)
			{
				throw new PublishedARInvoiceDocumentNotFoundException();
			}
			return new DocumentPack((DocumentCommand)commands[0], summary, null, null);
		}

		protected PrintStatement AppendToDocumentPack(Dictionary<string, DocumentPack> docPackHash, ZGuid organisationPK, ZString currencyNK, ZGuid transactionBranchPK, ZGuid transactionDepartmentPK, bool isMultipleCurrency, ZDecimal openingBalance, string key)
		{
			PrintStatement statementBisObj = GetNewPrintStatement(null, isMultipleCurrency, openingBalance);

			statementBisObj.DocumentToPrint = DocumentToPrint;
			statementBisObj.CurrencyNK = currencyNK;
			statementBisObj.OrganisationPK = organisationPK;
			statementBisObj.IssueBySettlementGroup = IssueBySettlementGroup;
			statementBisObj.IssueByTransactionBranch = IssueByTransactionBranch;
			statementBisObj.IssueByTransactionDepartment = IssueByTransactionDepartment;
			statementBisObj.TransactionBranchPK = transactionBranchPK;
			statementBisObj.TransactionDepartmentPK = transactionDepartmentPK;

			SetStatementDisplayDateForStatement(statementBisObj);

			if (DocumentToPrint == Core.Constants.StatementCollectionLetterType.StatementOfAccount)
			{
				SetPropertiesOnPrintStatementForStatements(statementBisObj);
			}
			else
			{
				SetPropertiesOnPrintStatementForCollectionLetters(statementBisObj);
			}

			DocumentPack docPack = null;

			DocumentCommand command = GetStatementDocumentCommand(statementBisObj);

			if (docPackHash.ContainsKey(key))
			{
				docPack = docPackHash[key];
				docPack.AddReportsToPack(command, null, statementBisObj, null);
			}
			else
			{
				docPack = new DocumentPack(command, statementBisObj, null, null);

				OrgHeader organisation = Factory.Load<OrgHeader>(statementBisObj.OrganisationPK);
				docPack.ForceBusinessObjectToLogAgainst(organisation.CompanyData);

				docPackHash.Add(key, docPack);
			}

			return statementBisObj;
		}

		DocumentCommand GetStatementDocumentCommand(PrintStatement statementBisObj)
		{
			ZQuery commandFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, TemplateNameToUse);
			if (statementBisObj.DocumentSupporter != null)
			{
				commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_BusinessContext, statementBisObj.DocumentSupporter.BusinessContext.ToString());
			}
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsSystemDefined, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueChar);
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsPublished, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueChar);
			if (DocumentsDataRegistry.Instance.UseNewDocBuilderStatementDocument.Value)
			{
				commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Equal, string.Empty);
			}
			else
			{
				commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Equal, Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments);
				commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsClientSpecific, SQLComparisonOperator.Equal, IsClientSpecific ? Core.Constants.BooleanTrueChar : Core.Constants.BooleanFalseChar);

				if (SpecificCountryCode != ZString.Empty)
				{
					commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, SpecificCountryCode);
				}
			}
			var documentCommands = new DocumentCommandCollection(statementBisObj);
			documentCommands.LoadWithMoreFiltering(commandFilter);
			if (documentCommands.Count == 0)
			{
				throw new PublishedARInvoiceDocumentNotFoundException();
			}
			return documentCommands[0];
		}

		void AppendTransactionsToDocumentPack(DocumentPack docPack, TransactionHeaderCollection transactions)
		{
			Argument.NotNull(docPack, "DocPack");
			Argument.NotNull(transactions, "Transactions");

			foreach (TransactionHeader transaction in transactions)
			{
				if (PrintStatement.AttachmentTransactionTypes.Any(x => x == transaction.AH_TransactionType))
				{
					if (transaction.AH_TransactionType == ZArchitecture.Core.TransactionTypes.InvoiceBatch)
					{
						var invoiceCommandFilter = new ZQuery();
						invoiceCommandFilter.AddToFilter(StmMenuItemSchema.SU_MenuName, "Invoice Statement Document");
						invoiceCommandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsPublished, Core.Constants.BooleanTrueChar);
						var invoiceDocumentCommands = new DocumentCommandCollection(transaction);
						invoiceDocumentCommands.LoadWithMoreFiltering(invoiceCommandFilter);
						if (invoiceDocumentCommands.Count == 0)
						{
							throw new PublishedARInvoiceDocumentNotFoundException();
						}

						docPack.AddReportsToPack(invoiceDocumentCommands[0], null, transaction, null);
					}
					else
					{
						string cmdName = transaction.EnterpriseInvoiceMenuName;
						string menuName = string.IsNullOrEmpty(cmdName) ? (NoResString)"Invoice" : cmdName;
						InvoicePrintCommandManager manager = InvoicePrintCommandManager.New((InvoicingBase)transaction, menuName);
						if (manager != null && manager.Command == null)
						{
							throw new UnableToFindInvoiceDocumentCommandException("Unable to find Invoice document command for transaction " + transaction.AH_TransactionNum);
						}

						docPack.AddReportsToPack(manager.Command, null, transaction, null);
					}
				}
			}
		}

		protected void AppendTransactionsToDocumentPack(Dictionary<string, DocumentPack> docPackHash, PrintStatement statementBisObj)
		{
			string key = statementBisObj.Organisation.OH_Code + Company.PK.ToString();

			if (docPackHash.ContainsKey(key))
			{
				DocumentPack docPack = docPackHash[key];

				AppendTransactionsToDocumentPack(docPack, statementBisObj.LoadAttachmentInvoices());
			}
		}

		protected DynamicBusinessObjectCollection GetBusinessObjCollection()
		{
			ZStringBuilder sqlStringBuilder = new ZStringBuilder();
			ZSqlParameterCollection sqlParams = new ZSqlParameterCollection();

			AddStandardSelectClause(sqlStringBuilder, sqlParams);

			AddFilterForOneOrganisation(sqlStringBuilder, sqlParams);
			AddFilterForBatchOfOrganisations(sqlStringBuilder, sqlParams);
			AddFilterForOneDebtorGroup(sqlStringBuilder, sqlParams);
			AddFilterForOrganisationBranch(sqlStringBuilder, sqlParams);

			AddFilterForAccountsRelationship(sqlStringBuilder, sqlParams);
			AddFilterForConsolidationCategory(sqlStringBuilder, sqlParams);
			AddFilterForCreditRating(sqlStringBuilder, sqlParams);

			AddFilterForSalesRep(sqlStringBuilder, sqlParams);
			AddFilterForCustomerServiceRep(sqlStringBuilder, sqlParams);
			AddFilterForCreditController(sqlStringBuilder, sqlParams);

			if (!PrintAccountMovementSOA)
			{
				AddFilterForCurrency(sqlStringBuilder, sqlParams);
				AddFilterForTransactionBranch(sqlStringBuilder, sqlParams);
				AddFilterForTransactionDepartment(sqlStringBuilder, sqlParams);

				if (DocumentToPrint == Core.Constants.StatementCollectionLetterType.StatementOfAccount)
				{
					AddFilterForInvoiceDateOnOrBefore(sqlStringBuilder, sqlParams);
					AddFilterForPostDateOnOrBefore(sqlStringBuilder, sqlParams);
					AddAdditionalFilterForStatementOfAccount(sqlStringBuilder, sqlParams);
				}
				else
				{
					AddFilterForDueDatesOnOrBefore(sqlStringBuilder, sqlParams);
					AddFilterForOutStandingAmount(sqlStringBuilder, sqlParams);
				}

				AddFilterForDisbursementInvoicesOnly(sqlStringBuilder, sqlParams);
				AddFilterForTransactionsInActiveBatch(sqlStringBuilder, sqlParams);
			}

			AddGroupByStatement(sqlStringBuilder, sqlParams);
			AddHavingFilter(sqlStringBuilder, sqlParams);

			AddOrderByStatement(sqlStringBuilder, sqlParams);

			DynamicBusinessObjectCollection businessObjCollection = new DynamicBusinessObjectCollection(Factory);
			businessObjCollection.Load(sqlStringBuilder.ToString(), sqlParams);
			return businessObjCollection;
		}

		void SetStatementDisplayDateForStatement(PrintStatement statementBizObj)
		{
			if (PrintAccountMovementSOA)
			{
				statementBizObj.StatementDisplayDate = PrintAccountMovementToDate;
			}
			else
			{
				ZDateTime earliestDate = ZDateTime.Today;

				if (CutOffDate.IsValid && CutOffDate.Date < earliestDate)
				{
					earliestDate = CutOffDate.Date;
				}

				if (CutOffPeriod > 0 && EndOfPeriod.IsValid && EndOfPeriod.Date < earliestDate)
				{
					earliestDate = EndOfPeriod.Date;
				}

				statementBizObj.StatementDisplayDate = earliestDate;
			}
		}

		protected virtual void SetPropertiesOnPrintStatementForStatements(PrintStatement statementBizObj)
		{
			statementBizObj.CutOffDate = (CutOffDate.IsValid) ? CutOffDate.Date.AddDays(1) : ZDateTime.Empty;
			statementBizObj.EndOfPeriod = (!EndOfPeriod.IsEmpty && EndOfPeriod.IsValid) ? EndOfPeriod.Date.AddDays(1) : ZDateTime.Empty;
			statementBizObj.DisbursementInvoicesOnly = DisbursementInvoicesOnly;
			statementBizObj.IncludeTransactionsInActiveBatch = IncludeTransactionsInActiveBatch;

			if (PrintAccountMovementSOA && statementBizObj is PrintStatementForAccountMovement)
			{
				var statementBizObjForAccMov = statementBizObj as PrintStatementForAccountMovement;
				statementBizObjForAccMov.PostDateFrom = PrintAccountMovementFromDate;
				statementBizObjForAccMov.PostDateTo = PrintAccountMovementToDate;
				statementBizObjForAccMov.GroupBy = GroupByAccountMovementSOALine;
			}
		}

		void SetPropertiesOnPrintStatementForCollectionLetters(PrintStatement statementBizObj)
		{
			statementBizObj.CutOffDate = (CutOffDate.IsValid) ? CutOffDate.Date.AddDays(1) : ZDateTime.Empty;
			statementBizObj.OutStandingAmountGreaterThan = OutstandingAmountGreaterThan;
			statementBizObj.DisbursementInvoicesOnly = DisbursementInvoicesOnly;
			statementBizObj.IncludeTransactionsInActiveBatch = IncludeTransactionsInActiveBatch;
		}

		#endregion

		#region SQL Generation

		string OrganisationHeader
		{
			get { return IssueBySettlementGroup ? Constants.SettlementOrg : Constants.TransactionOrg; }
		}

		string OrganisationCompanyData
		{
			get { return IssueBySettlementGroup ? Constants.SettlementCompanyData : Constants.TransactionCompanyData; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne",
			"CA1052:StaticHolderTypesShouldBeSealed", Justification = "This class has derived class")]
		public class Constants
		{
			public const string SettlementOrg = "SettlementOrg";
			public const string SettlementCompanyData = "SettlementCompanyData";

			public const string TransactionOrg = "TransactionOrg";
			public const string TransactionCompanyData = "TransactionCompanyData";

			public const string StmntCurrencyTable = "StmntCurrencyTable";
			public const string isMultipleCurrency = "isMultipleCurrency";
			public const string OpeningBalance = "OpeningBalance";
			public const string ClosingBalance = "ClosingBalance";
			public const string OrgHeader = OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName;
			public const string OH_PK = OrgHeaderSchema.Constants.PK;
			public const string OH_Code = OrgHeaderSchema.Constants.OH_Code;

			public const string OrgCompanyData = OrgCompanyDataSchema.Constants.SqlSchemaName + "." + OrgCompanyDataSchema.Constants.TableName;
			public const string OB_PK = OrgCompanyDataSchema.Constants.PK;
			public const string OB_OH = OrgCompanyDataSchema.Constants.OB_OH;
			public const string OB_GC = OrgCompanyDataSchema.Constants.OB_GC;
			public const string OB_ARCategory = OrgCompanyDataSchema.Constants.OB_ARCategory;
			public const string OB_OJ_ARDebtorGroup = OrgCompanyDataSchema.Constants.OB_OJ_ARDebtorGroup;
			public const string OB_ARCreditRating = OrgCompanyDataSchema.Constants.OB_ARCreditRating;
			//public const string OB_OH_ARSettlementGroup = OrgCompanyDataSchema.Constants.OB_OH_ARSettlementGroup;
			public const string OB_ARConsolidatedAccountingCategory = OrgCompanyDataSchema.Constants.OB_ARConsolidatedAccountingCategory;

			public const string AH_TransTypeIndex = "TransTypeIndex";

			public const string AccTransactionHeader = AccTransactionHeaderSchema.Constants.SqlSchemaName + "." + AccTransactionHeaderSchema.Constants.TableName;
			public const string AH_PK = AccTransactionHeaderSchema.Constants.PK;
			public const string AH_RX_NKTransactionCurrency = AccTransactionHeaderSchema.Constants.AH_RX_NKTransactionCurrency;
			public const string AH_GB = AccTransactionHeaderSchema.Constants.AH_GB;
			public const string AH_GC = AccTransactionHeaderSchema.Constants.AH_GC;
			public const string AH_GE = AccTransactionHeaderSchema.Constants.AH_GE;
			public const string AH_OH = AccTransactionHeaderSchema.Constants.AH_OH;
			public const string AH_Ledger = AccTransactionHeaderSchema.Constants.AH_Ledger;
			public const string AH_TransactionType = AccTransactionHeaderSchema.Constants.AH_TransactionType;
			public const string AH_TransactionNum = AccTransactionHeaderSchema.Constants.AH_TransactionNum;
			public const string AH_DueDate = AccTransactionHeaderSchema.Constants.AH_DueDate;
			public const string AH_LocalTotal = MasterFiles.Business.AccTransactionHeader.Schema.AH_LocalTotal;
			public const string AH_OutstandingAmount = AccTransactionHeaderSchema.Constants.AH_OutstandingAmount;
			public const string AH_PostDate = AccTransactionHeaderSchema.Constants.AH_PostDate;
			public const string AH_FullyPaidDate = AccTransactionHeaderSchema.Constants.AH_FullyPaidDate;
			public const string AH_InvoiceDate = AccTransactionHeaderSchema.Constants.AH_InvoiceDate;
			public const string AH_OSTotal = AccTransactionHeaderSchema.Constants.AH_OSTotal;
			public const string AH_TransactionCategory = AccTransactionHeaderSchema.Constants.AH_TransactionCategory;
			public const string AH_AH_InvoiceStatement = AccTransactionHeaderSchema.Constants.AH_AH_InvoiceStatement;
			public const string AH_Description = AccTransactionHeaderSchema.Constants.AH_Desc;

			public const string AccTransactionMatchLink = AccTransactionMatchLinkSchema.Constants.SqlSchemaName + "." + AccTransactionMatchLinkSchema.Constants.TableName;
			public const string AP_PK = AccTransactionMatchLinkSchema.Constants.PK;
			public const string AP_AH = AccTransactionMatchLinkSchema.Constants.AP_AH;
			public const string AP_Amount = AccTransactionMatchLinkSchema.Constants.AP_Amount;
			public const string AP_MatchDate = AccTransactionMatchLinkSchema.Constants.AP_MatchDate;

			public const string GlbBranch = GlbBranchSchema.Constants.SqlSchemaName + "." + GlbBranchSchema.Constants.TableName;
			public const string GB_PK = GlbBranchSchema.Constants.PK;
			public const string GB_GC = GlbBranchSchema.Constants.GB_GC;
			public const string OB_GB_ControllingBranch = OrgCompanyDataSchema.Constants.OB_GB_ControllingBranch;

			public const string GlbCompany = GlbCompanySchema.Constants.SqlSchemaName + "." + GlbCompanySchema.Constants.TableName;
			public const string GC_PK = GlbCompanySchema.Constants.PK;
			public const string GC_RX_NKLocalCurrency = GlbCompanySchema.Constants.GC_RX_NKLocalCurrency;

			public const string OrgStaffAssignments = OrgStaffAssignmentsSchema.Constants.SqlSchemaName + "." + OrgStaffAssignmentsSchema.Constants.TableName;
			public const string O8_OH = OrgStaffAssignmentsSchema.Constants.O8_OH;
			public const string O8_GC = OrgStaffAssignmentsSchema.Constants.O8_GC;
			public const string O8_Role = OrgStaffAssignmentsSchema.Constants.O8_Role;
			public const string O8_Department = OrgStaffAssignmentsSchema.Constants.O8_Department;
			public const string O8_GS_NKPersonResponsible = OrgStaffAssignmentsSchema.Constants.O8_GS_NKPersonResponsible;

			public const string RelatedParty = OrgRelatedPartySchema.Constants.SqlSchemaName + "." + OrgRelatedPartySchema.Constants.TableName;
			public const string PR_OH_Parent = OrgRelatedPartySchema.Constants.PR_OH_Parent;
			public const string PR_PartyType = OrgRelatedPartySchema.Constants.PR_PartyType;
			public const string PR_OH_RelatedParty = OrgRelatedPartySchema.Constants.PR_OH_RelatedParty;
			public const string PR_GC = OrgRelatedPartySchema.Constants.PR_GC;

			public const string AccCollectionOrder = AccCollectionOrderSchema.Constants.SqlSchemaName + "." + AccCollectionOrderSchema.Constants.TableName;
			public const string ACO_PK = AccCollectionOrderSchema.Constants.PK;
			public const string ACO_ACB = AccCollectionOrderSchema.Constants.ACO_ACB;
			public const string AccCollectionOrderLine = AccCollectionOrderLineSchema.Constants.SqlSchemaName + "." + AccCollectionOrderLineSchema.Constants.TableName;
			public const string AOL_AH = AccCollectionOrderLineSchema.Constants.AOL_AH;
			public const string AOL_IsCancelled = AccCollectionOrderLineSchema.Constants.AOL_IsCancelled;
			public const string AOL_ACO = AccCollectionOrderLineSchema.Constants.AOL_ACO;

			public const string AccCollectionBatch = AccCollectionBatchSchema.Constants.SqlSchemaName + "." + AccCollectionBatchSchema.Constants.TableName;
			public const string ACB_PK = AccCollectionBatchSchema.Constants.PK;
			public const string ACB_Type = AccCollectionBatchSchema.Constants.ACB_Type;
		}

		bool isPreviousPeriod
		{
			get { return !EndOfPeriod.IsEmpty && EndOfPeriod < ZDateTime.Today; }
		}

		protected void AddStandardSelectClause(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			stringBuilder.Append("SELECT " + OrganisationHeader + "." + Constants.OH_PK + ", ");

			if (PrintAccountMovementSOA)
			{
				stringBuilder.Append(Constants.StmntCurrencyTable + "." + Constants.AH_RX_NKTransactionCurrency + ", ");
				stringBuilder.Append(ZString.Format((NoResString)@"	CASE
															WHEN {0}.{1} = {2} THEN 1
															ELSE 0
														END AS {3}, ",
									Constants.StmntCurrencyTable, //0
									Constants.AH_RX_NKTransactionCurrency, //1
									Constants.GC_RX_NKLocalCurrency, //2
									Constants.isMultipleCurrency)); //3

				stringBuilder.Append(ZString.Format((NoResString)@"{0}.OpeningBalance AS {1}, ", Constants.StmntCurrencyTable, Constants.OpeningBalance));
				stringBuilder.Append(ZString.Format((NoResString)@"{0}.ClosingBalance AS {1}, ", Constants.StmntCurrencyTable, Constants.ClosingBalance));
			}
			else
			{
				stringBuilder.Append(Constants.AccTransactionHeader + "." + Constants.AH_RX_NKTransactionCurrency + ", ");
			}

			if (IssueByTransactionBranch)
			{
				stringBuilder.Append(Constants.AccTransactionHeader + "." + Constants.AH_GB + ", ");
			}
			if (IssueByTransactionDepartment)
			{
				stringBuilder.Append(Constants.AccTransactionHeader + "." + Constants.AH_GE + ", ");
			}
			stringBuilder.Append(OrganisationHeader + "." + Constants.OH_Code + " ");

			stringBuilder.Append("FROM " + Constants.AccTransactionHeader + " ");

			stringBuilder.Append("INNER JOIN " + Constants.GlbBranch + " ");
			stringBuilder.Append("ON " + Constants.GB_PK + " = " + Constants.AH_GB + " ");

			stringBuilder.Append("INNER JOIN " + Constants.GlbCompany + " ");
			stringBuilder.Append("ON " + Constants.GB_GC + " = " + Constants.GC_PK + " ");

			stringBuilder.Append("INNER JOIN " + Constants.OrgCompanyData + " " + OrganisationCompanyData + " ");
			stringBuilder.Append("ON " + OrganisationCompanyData + "." + Constants.OB_OH + " = " + Constants.AH_OH + " ");
			stringBuilder.Append("AND " + OrganisationCompanyData + "." + Constants.OB_GC + " = " + Constants.GC_PK + " ");

			if (IssueBySettlementGroup)
			{
				stringBuilder.Append("LEFT JOIN " + Constants.RelatedParty + " ");
				stringBuilder.Append("ON " + Constants.RelatedParty + "." + Constants.PR_OH_Parent + " = " + Constants.AH_OH + " ");
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @ARSettlement ", Constants.RelatedParty, Constants.PR_PartyType));
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @CurrentCompany ", Constants.RelatedParty, Constants.PR_GC));

				stringBuilder.Append("INNER JOIN " + Constants.OrgHeader + " " + Constants.SettlementOrg + " ");
				stringBuilder.Append("ON " + Constants.SettlementOrg + "." + Constants.OH_PK + " = ");
				stringBuilder.Append(string.Format("ISNULL( {0},{1}) ", Constants.PR_OH_RelatedParty, Constants.AH_OH));
			}
			else
			{
				stringBuilder.Append("INNER JOIN " + Constants.OrgHeader + " " + Constants.TransactionOrg + " ");
				stringBuilder.Append("ON " + Constants.TransactionOrg + "." + Constants.OH_PK + " = " + Constants.AH_OH + " ");
			}

			if (isPreviousPeriod)
			{
				stringBuilder.Append("LEFT JOIN " + "(SELECT ISNULL(SUM(" + Constants.AP_Amount + "),0) SumOfMatches, " + Constants.AP_AH + " ");
				stringBuilder.Append("                FROM " + Constants.AccTransactionMatchLink + " ");
				stringBuilder.Append("                WHERE " + Constants.AP_MatchDate + " < @MatchDateOnOrBefore ");
				stringBuilder.Append(string.Format((NoResString)"                OR {0} IS NULL ", Constants.AP_MatchDate));
				stringBuilder.Append(string.Format((NoResString)"                GROUP BY {0}) Matches ON {1} = {2} ", Constants.AP_AH, Constants.AH_PK, Constants.AP_AH));

				sqlParams.Add(ZSqlParameter.New("@MatchDateOnOrBefore", EndOfPeriod.Date.AddDays(1), AccTransactionMatchLinkSchema.AP_MatchDate));
			}

			if (PrintAccountMovementSOA)
			{
				ZString orgPKExpression = !OH_PK.IsEmpty ? "@Organisation" : "NULL";
				stringBuilder.Append(ZString.Format(@"INNER JOIN (SELECT * FROM fnGetAROutstandingBalance(@PostDateFrom, @PostDateTo, @Company, {3})) {0} ON {1}.{2} = {0}.{2} ",
														Constants.StmntCurrencyTable, //0
														Constants.AccTransactionHeader, //1
														Constants.AH_OH, //2
														orgPKExpression)); //3
			}

			stringBuilder.Append("WHERE " + Constants.AH_Ledger + " = @Ledger ");
			stringBuilder.Append(string.Format((NoResString)"AND {0} != @InvoiceBatchTransactionType ", Constants.AH_TransactionType));
			stringBuilder.Append(string.Format((NoResString)"AND {0} = @Company ", Constants.GC_PK));
																						 // Unpaid or partially paid

			if (PrintAccountMovementSOA)
			{
				stringBuilder.Append(string.Format((NoResString)" AND ({0}.{1} between @PostDateFrom and @PostDateTo) ", Constants.AccTransactionHeader, Constants.AH_PostDate));

				sqlParams.Add(ZSqlParameter.New("@PostDateFrom", PrintAccountMovementFromDate.Date, AccTransactionHeaderSchema.AH_PostDate));
				sqlParams.Add(ZSqlParameter.New("@PostDateTo", PrintAccountMovementToDate.EndOfDay(), AccTransactionHeaderSchema.AH_PostDate));
			}
			else
			{
				if (isPreviousPeriod)
				{
					stringBuilder.Append($"AND (SumOfMatches != {AccTransactionHeader.Schema.AH_LocalTotal} OR SumOfMatches IS NULL) ");
				}
				else
				{
					stringBuilder.Append(string.Format((NoResString)"AND {0} IS NULL ", Constants.AH_FullyPaidDate));
				}

				stringBuilder.Append($"AND {AccTransactionHeader.Schema.AH_LocalTotal} != 0 ");
			}

			sqlParams.Add(ZSqlParameter.New("@Ledger", ZArchitecture.Core.LedgerTypes.AccountsReceivable, AccTransactionHeaderSchema.AH_Ledger));
			sqlParams.Add(ZSqlParameter.New("@InvoiceBatchTransactionType", ZArchitecture.Core.TransactionTypes.InvoiceBatch, AccTransactionHeaderSchema.AH_TransactionType));
			sqlParams.Add(ZSqlParameter.New("@Company", Company.PK, GlbCompanySchema.PK));
			sqlParams.Add(ZSqlParameter.New("@ARSettlement", RelatedPartyTypeList.Codes.ARSettlementGroup, OrgRelatedPartySchema.PR_PartyType));
			sqlParams.Add(ZSqlParameter.New("@CurrentCompany", Company.PK, OrgRelatedPartySchema.PR_GC));
		}

		protected void AddFilterForOneOrganisation(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!OH_PK.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @Organisation ", OrganisationHeader, Constants.OH_PK));
				sqlParams.Add(ZSqlParameter.New("@Organisation", OH_PK, OrgHeaderSchema.PK));
			}
		}

		protected void AddFilterForBatchOfOrganisations(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (BatchOfOrganisationsToPrint.Count > 0)
			{
				ZString resultString = "AND " + OrganisationHeader + "." + Constants.OH_PK;
				for (int i = 0; i < BatchOfOrganisationsToPrint.Count; i++)
				{
					resultString += ((i == 0) ? string.Format((NoResString)" IN (") : ",") + "@Organisation" + i.ToString();
					sqlParams.Add(ZSqlParameter.New("@Organisation" + i.ToString(), BatchOfOrganisationsToPrint[i], OrgHeaderSchema.PK));
				}
				resultString += ") ";
				stringBuilder.Append(resultString);
			}
		}

		protected void AddFilterForOneDebtorGroup(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!OJ_PK.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @ARGroup ", OrganisationCompanyData, Constants.OB_OJ_ARDebtorGroup));
				sqlParams.Add(ZSqlParameter.New("@ARGroup", OJ_PK, OrgCompanyDataSchema.OB_OJ_ARDebtorGroup));
			}
		}

		protected void AddFilterForOrganisationBranch(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!GB_PK.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @Branch ", OrganisationCompanyData, Constants.OB_GB_ControllingBranch));
				sqlParams.Add(ZSqlParameter.New("@Branch", GB_PK, GlbBranchSchema.PK));
			}
		}

		protected void AddFilterForAccountsRelationship(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!AccountsRelationShip.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @AccountsRelationship ", OrganisationCompanyData, Constants.OB_ARCategory));
				sqlParams.Add(ZSqlParameter.New("@AccountsRelationship", AccountsRelationShip, OrgCompanyDataSchema.OB_ARCategory));
			}
		}

		protected void AddFilterForConsolidationCategory(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!ConsolidationCategory.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @ConsolidationCategory ", OrganisationCompanyData, Constants.OB_ARConsolidatedAccountingCategory));
				sqlParams.Add(ZSqlParameter.New("@ConsolidationCategory", ConsolidationCategory, OrgCompanyDataSchema.OB_ARConsolidatedAccountingCategory));
			}
		}

		protected void AddFilterForCreditRating(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!CreditRating.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @CreditRating ", OrganisationCompanyData, Constants.OB_ARCreditRating));
				sqlParams.Add(ZSqlParameter.New("@CreditRating", CreditRating, OrgCompanyDataSchema.OB_ARCreditRating));
			}
		}

		protected void AddFilterForSalesRep(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForStaffAssignment(SalesRep_Code, SalesRepStaffAssignmentsTableName, "SalesRep",
				StaffAssignmentRoles.Codes.SalesRep, stringBuilder, sqlParams);
		}

		protected void AddFilterForCustomerServiceRep(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForStaffAssignment(CustomerServiceRep_Code, CustomerServiceRepStaffAssignmentsTableName, "CustomerServiceRep",
				StaffAssignmentRoles.Codes.CustomerServiceRep, stringBuilder, sqlParams);
		}

		protected void AddFilterForCreditController(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForStaffAssignment(CreditController_Code, CreditControllerStaffAssignmentsTableName, "CreditController",
				StaffAssignmentRoles.Codes.CreditController, stringBuilder, sqlParams);
		}

		void AddFilterForStaffAssignment(ZString staffCode, ZString staffAssignmentsRenamedTableName, ZString assignmentDescription,
			string roleType, ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!staffCode.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} IN (", OrganisationHeader, Constants.OH_PK));

				stringBuilder.Append("SELECT " + Constants.O8_OH + " ");
				stringBuilder.Append("FROM " + Constants.OrgStaffAssignments + " ");
				stringBuilder.Append("WHERE " + Constants.O8_GS_NKPersonResponsible + " = @" + assignmentDescription + " ");
				stringBuilder.Append(string.Format((NoResString)"AND {0} = @{1}Role ", Constants.O8_Role, assignmentDescription));
				stringBuilder.Append(string.Format((NoResString)"AND {0} = @{1}Department ", Constants.O8_Department, assignmentDescription));
				stringBuilder.Append(string.Format((NoResString)"AND {0} = @{1}Company ", Constants.O8_GC, assignmentDescription));

				stringBuilder.Append(")");

				sqlParams.Add(ZSqlParameter.New("@" + assignmentDescription, staffCode, OrgStaffAssignmentsSchema.O8_GS_NKPersonResponsible));
				sqlParams.Add(ZSqlParameter.New(string.Format((NoResString)"@{0}Role", assignmentDescription), roleType, OrgStaffAssignmentsSchema.O8_Role));
				sqlParams.Add(ZSqlParameter.New(string.Format((NoResString)"@{0}Department", assignmentDescription), "ALL", OrgStaffAssignmentsSchema.O8_Department));
				sqlParams.Add(ZSqlParameter.New(string.Format((NoResString)"@{0}Company", assignmentDescription), Company.PK, OrgStaffAssignmentsSchema.O8_GC));
			}
		}

		protected void AddFilterForCurrency(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!RX_PK.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @Currency ", Constants.AccTransactionHeader, Constants.AH_RX_NKTransactionCurrency));
				RefCurrency currency = Factory.Load<RefCurrency>(RX_PK);
				sqlParams.Add(ZSqlParameter.New("@Currency", currency != null ? currency.RX_Code : ZString.Empty, AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency));
			}
		}

		protected void AddFilterForTransactionBranch(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (IssueByTransactionBranch && !TransactionBranch_PK.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @TransactionBranch ", Constants.AccTransactionHeader, Constants.AH_GB));
				sqlParams.Add(ZSqlParameter.New("@TransactionBranch", TransactionBranch_PK, AccTransactionHeaderSchema.AH_GB));
			}
		}

		protected void AddFilterForTransactionDepartment(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (IssueByTransactionDepartment && !TransactionDepartment_PK.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @TransactionDepartment ", Constants.AccTransactionHeader, Constants.AH_GE));
				sqlParams.Add(ZSqlParameter.New("@TransactionDepartment", TransactionDepartment_PK, AccTransactionHeaderSchema.AH_GE));
			}
		}

		protected virtual void AddAdditionalFilterForStatementOfAccount(ZStringBuilder sqlStringBuilder, ZSqlParameterCollection sqlParams)
		{
		}

		protected void AddFilterForInvoiceDateOnOrBefore(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!CutOffDate.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0} < @CutOffDate ", Constants.AH_InvoiceDate));
				sqlParams.Add(ZSqlParameter.New("@CutOffDate", CutOffDate.Date.AddDays(1), AccTransactionHeaderSchema.AH_InvoiceDate));
			}
		}

		protected void AddFilterForPostDateOnOrBefore(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!EndOfPeriod.IsEmpty && EndOfPeriod.IsValid)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0} < @PostDateOnOrBefore ", Constants.AH_PostDate));
				sqlParams.Add(ZSqlParameter.New("@PostDateOnOrBefore", EndOfPeriod.Date.AddDays(1), AccTransactionHeaderSchema.AH_PostDate));
			}
		}

		protected void AddFilterForDueDatesOnOrBefore(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (!CutOffDate.IsEmpty)
			{
				stringBuilder.Append(string.Format((NoResString)"AND {0} < @CutOffDate ", Constants.AH_DueDate));
				sqlParams.Add(ZSqlParameter.New("@CutOffDate", CutOffDate.Date.AddDays(1), AccTransactionHeaderSchema.AH_DueDate));
			}
		}

		protected void AddFilterForOutStandingAmount(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (OutstandingAmountGreaterThan != 0)
			{
				if (isPreviousPeriod)
				{
					stringBuilder.Append($"AND ABS({AccTransactionHeader.Schema.AH_LocalTotal} - ISNULL(SumOfMatches, 0) ) >= @OutstandingAmount ");
				}
				else
				{
					stringBuilder.Append(string.Format((NoResString)"AND ABS({0}) >= @OutstandingAmount ", Constants.AH_OutstandingAmount));
				}
				sqlParams.Add(ZSqlParameter.New("@OutstandingAmount", OutstandingAmountGreaterThan, AccTransactionHeaderSchema.AH_OutstandingAmount));
			}
		}

		protected void AddFilterForDisbursementInvoicesOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (DisbursementInvoicesOnly)
			{
				ZStringBuilder disbursementInStatement = new ZStringBuilder();
				string[] disbursementTypes = InvoiceTypeCalculationProvider.DisbursementInvoiceTypes;
				for (int i = 1; i <= disbursementTypes.Length; i++)
				{
					string parameterName = "@Disbursement" + i;
					disbursementInStatement.Append(parameterName);
					if (i < disbursementTypes.Length)
					{
						disbursementInStatement.Append(", ");
					}
					sqlParams.Add(ZSqlParameter.New(parameterName, disbursementTypes[i - 1], AccTransactionHeaderSchema.AH_TransactionCategory));
				}
				stringBuilder.Append(string.Format((NoResString)"AND {0} in ({1}) ", Constants.AH_TransactionCategory, disbursementInStatement));
			}
		}

		protected void AddGroupByStatement(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			stringBuilder.Append(string.Format((NoResString)"GROUP BY {0}.{1}, ", OrganisationHeader, Constants.OH_PK));

			if (PrintAccountMovementSOA)
			{
				stringBuilder.Append(Constants.StmntCurrencyTable + "." + Constants.AH_RX_NKTransactionCurrency + ", ");
				stringBuilder.Append(Constants.GlbCompany + "." + Constants.GC_RX_NKLocalCurrency + ", ");
				stringBuilder.Append(Constants.StmntCurrencyTable + "." + Constants.OpeningBalance + ", ");
				stringBuilder.Append(Constants.StmntCurrencyTable + "." + Constants.ClosingBalance + ", ");
			}
			else
			{
				stringBuilder.Append(Constants.AH_RX_NKTransactionCurrency + ", ");
			}

			if (IssueByTransactionBranch)
			{
				stringBuilder.Append(Constants.AccTransactionHeader + "." + Constants.AH_GB + ", ");
			}
			if (IssueByTransactionDepartment)
			{
				stringBuilder.Append(Constants.AccTransactionHeader + "." + Constants.AH_GE + ", ");
			}
			stringBuilder.Append(OrganisationHeader + "." + Constants.OH_Code + " ");
		}

		protected void AddHavingFilter(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			if (CreditStatements == CreditOptions.ExcludeDocumentsInCredit)
			{
				if (PrintAccountMovementSOA)
				{
					stringBuilder.Append(string.Format((NoResString)"HAVING {0}.{1} > 0 ", Constants.StmntCurrencyTable, Constants.ClosingBalance));
				}
				else
				{
					if (isPreviousPeriod)
					{
						stringBuilder.Append($"HAVING SUM({AccTransactionHeader.Schema.AH_LocalTotal}) - SUM(ISNULL(Matches.SumOfMatches, 0)) > 0 ");
					}
					else
					{
						stringBuilder.Append(string.Format((NoResString)"HAVING SUM({0}) > 0 ", Constants.AH_OutstandingAmount));
					}
				}
			}
			else if (CreditStatements == CreditOptions.OnlyDocumentsInCredit)
			{
				if (PrintAccountMovementSOA)
				{
					stringBuilder.Append(string.Format((NoResString)"HAVING {0}.{1} <= 0 ", Constants.StmntCurrencyTable, Constants.ClosingBalance));
				}
				else
				{
					if (isPreviousPeriod)
					{
						stringBuilder.Append($"HAVING SUM({AccTransactionHeader.Schema.AH_LocalTotal}) - SUM(ISNULL(Matches.SumOfMatches, 0)) <= 0 ");
					}
					else
					{
						stringBuilder.Append(string.Format((NoResString)"HAVING SUM({0}) <= 0 ", Constants.AH_OutstandingAmount));
					}
				}
			}
		}

		protected void AddOrderByStatement(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			stringBuilder.Append(string.Format((NoResString)"ORDER BY {0}.{1}", OrganisationHeader, Constants.OH_Code));
		}

		#endregion

		#region Implementation

		protected AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return fPeriodCalculator;
			}
		}

		AccountingPeriodCalculator fPeriodCalculator;

		protected override void SetDefaultValues()
		{
			bool oldHasChanges = HasChanges;

			DocumentToPrint = Core.Constants.StatementCollectionLetterType.StatementOfAccount;
			IssueStatementPack = AccountingConstants.IssueStatementPackType.Default;
			IssueBySettlementGroup = false;

			OH_PK = ZGuid.Empty;
			OJ_PK = ZGuid.Empty;
			GB_PK = ZGuid.Empty;
			AccountsRelationShip = ZString.Empty;
			ConsolidationCategory = ZString.Empty;
			CreditRating = ZString.Empty;
			SalesRep_PK = ZGuid.Empty;
			CustomerServiceRep_PK = ZGuid.Empty;
			CreditController_PK = ZGuid.Empty;
			CreditStatements = CreditOptions.ExcludeDocumentsInCredit;

			RX_PK = ZGuid.Empty;
			CutOffDate = ZDateTime.Empty;
			CutOffPeriod = 0;
			OutstandingAmountGreaterThan = 0;
			DisbursementInvoicesOnly = false;
			IncludeTransactionsInActiveBatch = true;

			HasChanges = oldHasChanges;
		}

		static string StandardStatementAndCollectionLetterTemplateName
		{
			get
			{
				return (NoResString)"Statement Of Account";
			}
		}

		ZString TemplateNameToUse
		{
			get
			{
				ZString result = ZString.Empty;
				switch (DocumentToPrint)
				{
					case Core.Constants.StatementCollectionLetterType.StatementOfAccount:
						result = StatementMenuName;
						break;

					case Core.Constants.StatementCollectionLetterType.FirstReminder:
						result = FirstReminderLetterMenuName;
						break;

					case Core.Constants.StatementCollectionLetterType.SecondReminder:
						result = SecondReminderLetterMenuName;
						break;

					case Core.Constants.StatementCollectionLetterType.CollectionLetter:
						result = CollectionLetterMenuName;
						break;

					case Core.Constants.StatementCollectionLetterType.DemandLetter:
						result = DemandLetterMenuName;
						break;
				}

				return result;
			}
		}

		protected virtual ZString SpecificCountryCode
		{
			get { return ZString.Empty; }
		}

		protected virtual ZBool IsClientSpecific
		{
			get { return ZBool.False; }
		}

		protected ZString StatementMenuName
		{
			get { return StandardStatementAndCollectionLetterTemplateName; }
		}

		protected virtual ZString FirstReminderLetterMenuName
		{
			get { return StandardStatementAndCollectionLetterTemplateName; }
		}

		protected virtual ZString SecondReminderLetterMenuName
		{
			get { return StandardStatementAndCollectionLetterTemplateName; }
		}

		protected virtual ZString CollectionLetterMenuName
		{
			get { return StandardStatementAndCollectionLetterTemplateName; }
		}

		protected virtual ZString DemandLetterMenuName
		{
			get { return StandardStatementAndCollectionLetterTemplateName; }
		}

		void ClearInappropriateFieldsForAccountMovementSOA()
		{
			if (PrintAccountMovementSOA)
			{
				IncludeDebtorSummaryPage = false;
				IssueBySettlementGroup = false;
				IssueByTransactionBranch = false;
				IssueByTransactionDepartment = false;

				TransactionBranch_PK = ZGuid.Empty;
				TransactionDepartment_PK = ZGuid.Empty;
				CutOffDate = ZDateTime.Empty;
				CutOffPeriod = ZInt.Zero;
				DisbursementInvoicesOnly = false;
				IncludeTransactionsInActiveBatch = true;
			}
			else
			{
				PrintAccountMovementFromDate = ZDateTime.Empty;
				PrintAccountMovementToDate = ZDateTime.Empty;
				GroupByAccountMovementSOALine = ZString.Empty;

				DocumentToPrintInfo.ClearAllNotifications();
				PrintAccountMovementToDateInfo.ClearAllNotifications();
				PrintAccountMovementFromDateInfo.ClearAllNotifications();
				GroupByAccountMovementSOALineInfo.ClearAllNotifications();
			}
		}

		protected delegate Statement NewDelegate(GlbBranch branch);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		const string SalesRepStaffAssignmentsTableName = "SalesRepStaffAssignment";
		const string CustomerServiceRepStaffAssignmentsTableName = "CustomerServiceRepAssignment";
		const string CreditControllerStaffAssignmentsTableName = "CreditControllerAssignment";

		#endregion

		#region Account Fee

		#region DoAccountFeeTransaction

		public ZBool DoAccountFeeTransaction
		{
			get { return fDoAccountFeeTransaction; }
			set
			{
				if (SetNonPersistentPropertyValue(DoAccountFeeTransactionInfo, ref fDoAccountFeeTransaction, value))
				{
					if (!DoAccountFeeTransaction)
					{
						ClearAccountFeeDateFields();
					}
					else
					{
						SetDefaultValuesForAccountFee();
					}

					RefreshBindingIncludingChildren();
				}
			}
		}

		ZBool fDoAccountFeeTransaction;

		public ZPropertyInfo DoAccountFeeTransactionInfo
		{
			get { return GetZPropertyInfo(nameof(DoAccountFeeTransaction)); }
		}

		#endregion

		#region AccountFeeInvoiceCreator

		public AccountFeeInvoiceCreator AccountFeeInvoiceCreator
		{
			get
			{
				if (accountFeeInvoiceCreator == null)
				{
					accountFeeInvoiceCreator = new AccountFeeInvoiceCreator(this);
					RegisterEditableChildObject(AccountFeeInvoiceCreator);
				}
				return accountFeeInvoiceCreator;
			}
		}

		AccountFeeInvoiceCreator accountFeeInvoiceCreator;

		#endregion

		#region Functions

		public void SetDefaultValuesForAccountFee()
		{
			if (DoAccountFeeTransaction)
			{
				if (!AccountFeeInvoiceCreator.AccFeeFromDate.IsValid || !AccountFeeInvoiceCreator.AccFeeToDate.IsValid)
				{
					if (PrintAccountMovementSOA)
					{
						AccountFeeInvoiceCreator.AccFeeToDate = PrintAccountMovementToDate;
						AccountFeeInvoiceCreator.AccFeeFromDate = PrintAccountMovementFromDate;
					}
					else if (!PrintAccountMovementSOA && CutOffDate.IsValid)
					{
						AccountFeeInvoiceCreator.AccFeeToDate = CutOffDate;
						AccountFeeInvoiceCreator.AccFeeFromDate = CutOffDate.AddMonths(-1).AddDays(1);
					}
					else if (!PrintAccountMovementSOA && !CutOffPeriod.IsEmpty && CutOffPeriod > 0)
					{
						AccountFeeInvoiceCreator.AccFeeToDate = PeriodCalculator.GetLastDayForPeriod(CutOffPeriod);
						AccountFeeInvoiceCreator.AccFeeFromDate = AccountFeeInvoiceCreator.AccFeeToDate.AddMonths(-1).AddDays(1);
					}
				}
				if (!AccountFeeInvoiceCreator.AccFeePostDate.IsValid && (CutOffDate.IsValid || PrintAccountMovementToDate.IsValid || (!CutOffPeriod.IsEmpty && CutOffPeriod > 0)))
				{
					AccountFeeInvoiceCreator.AccFeePostDate = AccountFeeInvoiceCreator.AccFeeToDate;
				}
				if (!AccountFeeInvoiceCreator.AccFeeInvoiceDate.IsValid && AccountFeeInvoiceCreator.AccFeeToDate.IsValid)
				{
					AccountFeeInvoiceCreator.AccFeeInvoiceDate = AccountFeeInvoiceCreator.AccFeeToDate;
				}

				AccountFeeInvoiceCreator.SetDefaultTaxID();
			}
		}

		public void ClearAccountFeeDateFields()
		{
			AccountFeeInvoiceCreator.AccFeeFromDateInfo.ClearValue();
			AccountFeeInvoiceCreator.AccFeeToDateInfo.ClearValue();
			AccountFeeInvoiceCreator.AccFeeInvoiceDateInfo.ClearValue();
			AccountFeeInvoiceCreator.AccFeePostDateInfo.ClearValue();
			AccountFeeInvoiceCreator.AccFeeInvoiceDescriptionInfo.ClearValue();
			AccountFeeInvoiceCreator.AccFeeTaxIDInfo.ClearValue();

			AccountFeeInvoiceCreator.AccFeeFromDateInfo.ClearAllNotifications();
			AccountFeeInvoiceCreator.AccFeeToDateInfo.ClearAllNotifications();
			AccountFeeInvoiceCreator.AccFeeInvoiceDateInfo.ClearAllNotifications();
			AccountFeeInvoiceCreator.AccFeePostDateInfo.ClearAllNotifications();
			AccountFeeInvoiceCreator.AccFeeInvoiceDescriptionInfo.ClearAllNotifications();
			AccountFeeInvoiceCreator.AccFeeTaxIDInfo.ClearAllNotifications();
		}

		#endregion

		#endregion

		#region IAccountFeeInvoiceSupportable

		public bool CreateAccountFee
		{
			get { return DoAccountFeeTransaction; }
		}

		public string GetOrgPKQueryForAccountFee(ZSqlParameterCollection sqlParams)
		{
			var result = string.Empty;

			if (!OH_PK.IsEmpty
				|| !OJ_PK.IsEmpty
				|| !GB_PK.IsEmpty
				|| !AccountsRelationShip.IsEmpty
				|| !ConsolidationCategory.IsEmpty
				|| !CreditRating.IsEmpty
				|| !SalesRep_PK.IsEmpty
				|| !CustomerServiceRep_PK.IsEmpty
				|| !CreditController_PK.IsEmpty)
			{
				ZStringBuilder stringBuilder = new ZStringBuilder();
				stringBuilder.Append("SELECT " + OrganisationHeader + "." + Constants.OH_PK + " ");
				stringBuilder.Append("FROM " + Constants.OrgHeader + " " + OrganisationHeader + " ");

				stringBuilder.Append("INNER JOIN " + Constants.OrgCompanyData + " " + OrganisationCompanyData + " ");
				stringBuilder.Append("ON " + OrganisationCompanyData + "." + Constants.OB_OH + " = " + Constants.OH_PK + " ");

				stringBuilder.Append("INNER JOIN " + Constants.GlbCompany + " ");
				stringBuilder.Append("ON " + OrganisationCompanyData + "." + Constants.OB_GC + " = " + Constants.GC_PK + " ");

				if (!GB_PK.IsEmpty)
				{
					stringBuilder.Append("INNER JOIN " + Constants.GlbBranch + " ");
					stringBuilder.Append("ON " + Constants.OB_GB_ControllingBranch + " = " + Constants.GB_PK + " ");
				}

				if (IssueBySettlementGroup)
				{
					stringBuilder.Append("LEFT JOIN " + Constants.RelatedParty + " ");
					stringBuilder.Append("ON " + Constants.RelatedParty + "." + Constants.PR_OH_Parent + " = " + Constants.OH_PK + " ");
					stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @ARSettlement ", Constants.RelatedParty, Constants.PR_PartyType));
					stringBuilder.Append(string.Format((NoResString)"AND {0}.{1} = @CurrentCompany ", Constants.RelatedParty, Constants.PR_GC));

					sqlParams.Add(ZSqlParameter.New("@ARSettlement", RelatedPartyTypeList.Codes.ARSettlementGroup, OrgRelatedPartySchema.PR_PartyType));
					sqlParams.Add(ZSqlParameter.New("@CurrentCompany", Company.PK, OrgRelatedPartySchema.PR_GC));
				}

				stringBuilder.Append("WHERE " + Constants.GC_PK + " = @Company ");

				AddFilterForOneOrganisation(stringBuilder, sqlParams);
				AddFilterForOneDebtorGroup(stringBuilder, sqlParams);
				AddFilterForOrganisationBranch(stringBuilder, sqlParams);
				AddFilterForAccountsRelationship(stringBuilder, sqlParams);
				AddFilterForConsolidationCategory(stringBuilder, sqlParams);
				AddFilterForCreditRating(stringBuilder, sqlParams);
				AddFilterForSalesRep(stringBuilder, sqlParams);
				AddFilterForCustomerServiceRep(stringBuilder, sqlParams);
				AddFilterForCreditController(stringBuilder, sqlParams);

				result = stringBuilder.ToString();
			}

			return result;
		}

		#endregion

		#if DEBUG

		[ThreadStatic]
		public static bool TestAboveMaxPreviewCount;

		protected PrintTask GetPrintTaskForTest()
		{
			return UseNewPrintStreaming ? GetPrintTaskIEnum() : GetPrintTask();
		}

		#endif

		public DocumentSupporter DocumentSupporter
		{
			get { return new StatementDocumentSupporter(this); }
		}

		public static class AccountMovementLineGroupingOptions
		{
			public const string NO_GROUPING = "NON";
			public const string GROUP_BY_TRANSACTION_BRANCH = "GGB";
			public const string GROUP_BY_TRANSACTION_DEPARTMENT = "GGE";
			public const string GROUP_BY_TRANSACTION_BRANCH_DEPARTMENT = "GBE";
		}
	}

	public partial class StatementDocumentSupporter : DocumentSupporter
	{
		public StatementDocumentSupporter(Statement statement)
			: base(statement)
		{
		}

		protected Statement Statement
		{
			get { return (Statement)BusinessObject; }
		}

		#region Overrides

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.ReceivablesCustomiseStatements; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.Statement; }
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Enterprise.Core.Constants.DataContext.Statement, Enterprise.Core.Constants.DataContext.StatementSummary, Core.Constants.DataContext.GenericFreightJob };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Enterprise.Core.Constants.DataContext.GenericFreightJob)
			{
				return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Statement.GetNewPrintStatement());
			}
			else
			{
				return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "This is business context in the database")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is business context in the database")]
		protected override void CustomizeApplicableMenusFilterCore(ZQuery filter)
		{
			ZQuery[] parts = filter.GetCompositeParts();
			filter.Clear();
			string businessContextFilter = string.Format("{0} = '{1}'", StmMenuItemSchema.Constants.SU_BusinessContext, "Statement");
			foreach (var part in parts)
			{
				if (businessContextFilter.Equals(part.LiteralTextADO))
				{
					part.AddToFilter(JoinCondition.Or, StmMenuItemSchema.SU_BusinessContext, "StatementSummary");
				}
				filter.AddToFilter(part);
			}
		}

		protected override bool IgnoreHasChangesCore()
		{
			return true;
		}

		#endregion
	}

	public delegate void NoDocumentsToPrintEventHandler(object sender, NoDocumentsToPrintEventArgs e);

	public class NoDocumentsToPrintEventArgs : EventArgs
	{
		public NoDocumentsToPrintEventArgs(string message, string caption)
		{
			this.Message = message;
			this.Caption = caption;
		}

		public readonly string Message;
		public readonly string Caption;
	}
}
