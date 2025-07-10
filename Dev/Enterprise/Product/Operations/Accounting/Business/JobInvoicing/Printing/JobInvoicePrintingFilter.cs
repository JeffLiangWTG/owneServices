using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract class JobInvoicePrintingFilter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string DebtorOrCreditor = "DebtorOrCreditor";
			public const string TransactionType = "TransactionType";
			public const string Job = "JobNumber";
			public const string IncludePrinted = "IncludePrinted";
			public const string Transactions = "Transactions";
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are get_DoesTransactionBelongsToAnyGroup and get_Ledger. All possible values are expected and handled.")]
		public JobInvoicePrintingFilter(IBusiness hostBusinessObject, ZGuid jobHeaderPK)
			: this(hostBusinessObject, jobHeaderPK, new BusinessObjectFactory() { NameForDebugging = "JobInvoicePrintingFilter_Ctor" })
		{
			RefreshInvoiceList();
		}

		public JobInvoicePrintingFilter(IBusiness hostBusinessObject, ZGuid jobHeaderPK, BusinessObjectFactory factory, ZDateTime postDateFilterFrom, ZDateTime postDateFilterTo)
			: this(hostBusinessObject, jobHeaderPK, factory)
		{
			this.postDateFilterFrom = postDateFilterFrom;
			this.postDateFilterTo = postDateFilterTo;
		}

		public JobInvoicePrintingFilter(IBusiness hostBusinessObject, ZGuid jobHeaderPK, BusinessObjectFactory factory)
		{
			this.HostBusinessObject = hostBusinessObject;
			this.JobHeaderPK = jobHeaderPK;
			this.Factory = factory;
			this.IsPrintingFilterForConsol = false;
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are get_DoesTransactionBelongsToAnyGroup and get_Ledger. All possible values are expected and handled.")]
		public JobInvoicePrintingFilter(ForwardingConsol consol, ZGuid[] jobHeaderPKs)
		{
			this.HostBusinessObject = consol;
			this.JobHeaderPKs = jobHeaderPKs;
			this.Factory = new BusinessObjectFactory() { NameForDebugging = "JobInvoicePrintingFilter_Ctor2" };
			this.IsPrintingFilterForConsol = true;

			RefreshInvoiceList();
		}

		public void ResetInvoiceList()
		{
			JobNumber = ZGuid.Empty;
			DebtorOrCreditor = ZGuid.Empty;
			TransactionType = ZString.Empty;
			IncludePrinted = true;
			RefreshInvoiceList();
		}

		public void RefreshJobNumbersCollection(ZGuid[] jobHeaderPKs)
		{
			ZQuery jobsQuery = BuildFilter(JobHeaderSchema.PK, jobHeaderPKs);
			if (jobHeaderPKs.Length == 0)
			{
				jobsQuery.IsNoResultQuery = true;
			}
			JobNumbers.Load(jobsQuery);
		}

		readonly bool IsPrintingFilterForConsol;
		readonly ZGuid JobHeaderPK;
		readonly ZGuid[] JobHeaderPKs;

		protected readonly ZDateTime postDateFilterFrom;
		protected readonly ZDateTime postDateFilterTo;

		new protected BusinessObjectFactory Factory;
		public IBusiness HostBusinessObject { get; private set; }

#if DEBUG
		public ZGuid GetJobHeaderPK_ForTestOnly()
		{
			return JobHeaderPK;
		}
#endif

		public bool IsFreightConsol
		{
			get
			{
				var forwardingConsol = HostBusinessObject as ForwardingConsol;
				if (forwardingConsol != null)
				{
					return IsPrintingFilterForConsol || !forwardingConsol.IsGateway();
				}

				return false;
			}
		}

		public ZString ConsolNumber
		{
			get { return IsFreightConsol ? ((IJobCostingPlugIn)HostBusinessObject).JK_UniqueConsignRef : ZString.Empty; }
		}

		#region Transactions

		LightWeightTransactionHeaderCollection fTransactions;

		public LightWeightTransactionHeaderCollection Transactions
		{
			get
			{
				if (fTransactions == null)
				{
					fTransactions = new LightWeightTransactionHeaderCollection(Factory, IsPrintingFilterForConsol);
					fTransactions.SetReadOnlyIncludingChildren(true);
				}
				return fTransactions;
			}
		}

		#region Transactions

		LightWeightTransactionHeaderCollectionView filteredTransactions;

		public LightWeightTransactionHeaderCollectionView FilteredTransactions
		{
			get
			{
				if (filteredTransactions == null && Transactions != null)
				{
					filteredTransactions = new LightWeightTransactionHeaderCollectionView(Transactions);
				}
				return filteredTransactions;
			}
		}

		#endregion

		[ModuleID(ModuleId.ARTransaction)]
		public class LightWeightTransactionHeaderCollection : BusinessObjectCollection<TransactionHeader>
		{
			public LightWeightTransactionHeaderCollection(BusinessObjectFactory factory, bool isPrintingFilterForConsol)
				: base(factory)
			{
				this.isPrintingFilterForConsol = isPrintingFilterForConsol;
			}

			public bool IsPrintingFilterForConsol { get { return isPrintingFilterForConsol; } }
			readonly bool isPrintingFilterForConsol;

			protected override bool AllowNewCore
			{
				get { return false; }
			}

			protected override BusinessObject AddNewCore()
			{
				throw new NotSupportedException("This collection contains abstract type entity.");
			}

			protected override BusinessObject AddNewCore(Type bizOType)
			{
				throw new NotSupportedException("This collection contains abstract type entity.");
			}
		}

		[ModuleID(ModuleId.ARTransaction)]
		public class LightWeightTransactionHeaderCollectionView : BusinessObjectCollectionView<TransactionHeader>
		{
			public LightWeightTransactionHeaderCollectionView(LightWeightTransactionHeaderCollection collectionToFilter)
				: base(collectionToFilter)
			{
			}

			protected override bool AllowNewCore
			{
				get { return false; }
			}

			protected override BusinessObject AddNewCore()
			{
				throw new NotSupportedException("This collection contains abstract type entity.");
			}

			protected override BusinessObject AddNewCore(Type bizOType)
			{
				throw new NotSupportedException("This collection contains abstract type entity.");
			}

			#region IsThisPartOfTheCollection

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				var invoice = ((TransactionHeader)element);
				var job = Factory.Load<Job>(invoice.AH_JH);
				if (job != null)
				{
					job.InitializeParentFromGenericJobWithoutSettingDefaults();
				}
				var result = true;
				var hasChargeViewingRestriction = true;
				if (CollectionToFilter is LightWeightTransactionHeaderCollection)
				{
					if (((LightWeightTransactionHeaderCollection)CollectionToFilter).IsPrintingFilterForConsol)
					{
						hasChargeViewingRestriction = job != null && !job.IsAllowedToViewConsolCostsFromOtherBranchesOrDepartments;
					}
					else
					{
						hasChargeViewingRestriction = job != null && !job.IsAllowedToViewChargesFromOtherBranchesOrDepartments;
					}
				}
				else
				{
					hasChargeViewingRestriction = false;
				}

				if (hasChargeViewingRestriction && job.Branch != null && job.Department != null)
				{
					if (job.Branch != GlbBranch.CurrentBranch || job.Department != GlbDepartment.CurrentDepartment)
					{
						result = AllowedToLogin(job.Branch, job.Department);
					}
				}
				return result;
			}

			[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Combining Cache key, not related to GUI")]
			bool AllowedToLogin(GlbBranch branch, GlbDepartment department)
			{
				return Factory.GetCachedValue("Login BRN:" + branch.GB_Code + " DEP:" + department.GE_Code, delegate
				{
					var security = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
					return security.Login.IsAllowed;
				});
			}

			#endregion
		}
		public void RefreshInvoiceList()
		{
			BusinessObject host = HostBusinessObject as BusinessObject;
			if ((host == null || !host.IsInDatabase))
			{
				return;
			}

			Transactions.Load(ZQuery.NoResultQuery);

			foreach (var ledgerType in Ledger)
			{
				Transactions.AddRange(Factory.Load<TransactionHeader>(GetQueryOfJobInvoice(ledgerType)));
			}
		}

		#region GetQueryOfHeaders

#if DEBUG
		public ZQuery GetQueryOfJobInvoice_ForTestOnly(ZString ledgerType)
		{
			return GetQueryOfJobInvoice(ledgerType);
		}
#endif

		ZQuery GetQueryOfJobInvoice(ZString ledgerType)
		{
			ZDBOnlyQuery masterQuery;

			if (IsFreightConsol)
			{
				if (JobNumber.IsValid)
				{
					masterQuery = GetQueryWithJob(JobNumber.ToGuid(), ledgerType);
				}
				else
				{
					masterQuery = GetQueryWithoutJob(ledgerType);
				}
			}
			else
			{
				var jobHeaderPK = JobHeaderPK.IsValid ? JobHeaderPK.ToGuid() : Guid.Empty;
				masterQuery = GetQueryWithJob(jobHeaderPK, ledgerType);
			}

			masterQuery = AppendCommonFilters(masterQuery, ledgerType);
			return masterQuery;
		}

		ZDBOnlyQuery GetQueryWithJob(Guid jobPK, ZString ledgerType)
		{
			var masterQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			masterQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GetCompanyPK().ToGuid());

			var query1 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			query1.AddToFilter(AccTransactionHeaderSchema.AH_JH, jobPK);
			AddAccTransactionHeaderIndexHints(query1, jobPK, ledgerType);

			var subQuery1 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
			var subQuery2 = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			subQuery2.AddToFilter(JobHeaderSchema.PK, jobPK);
			var subQuery3 = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			subQuery3.AddToFilter(JobHeaderSchema.JH_JH_ParentJob, jobPK);

			subQuery2.AddAsUnionQuery(subQuery3);

			subQuery1.AddSubQuery(AccTransactionLinesSchema.AL_JH, subQuery2, JoinCondition.And);
			query1.AddSubQuery(subQuery1, JoinCondition.Or);

			if (IsFreightConsol && ledgerType == LedgerTypes.AccountsReceivable)
			{
				var query0 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query0.AddToFilter(query1, JoinCondition.And);
				AppendARFilters(query0);
				masterQuery.AddToFilter(query0, JoinCondition.And);
			}
			else
			{
				masterQuery.AddToFilter(query1, JoinCondition.And);
			}

			return masterQuery;
		}

		protected virtual void AddAccTransactionHeaderIndexHints(ZDBOnlyQuery query, Guid jobPK, ZString ledgerType)
		{
		}

		ZDBOnlyQuery GetQueryWithoutJob(ZString ledgerType)
		{
			var masterQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			masterQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GetCompanyPK().ToGuid());

			var query1 = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.AL_AH);
			var query2 = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			var query2b = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			var query3 = new ZDBOnlySubQuery(typeof(AutoJobShipment), JobShipmentSchema.PK);
			var query4 = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			var query5 = new ZDBOnlySubQuery(typeof(AutoJobConsol), JobConsolSchema.PK);

			query5.AddToFilter(JobConsolSchema.JK_UniqueConsignRef, ConsolNumber);
			query4.AddSubQuery(JobConShipLinkSchema.JN_JK, query5, JoinCondition.And);
			query3.AddSubQuery(JobShipmentSchema.PK, query4, JoinCondition.And);

			var query6 = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			var query7 = new ZDBOnlySubQuery(typeof(AutoJobShipment), JobShipmentSchema.PK);
			var query8 = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
			var query9 = new ZDBOnlySubQuery(typeof(AutoJobConsol), JobConsolSchema.PK);
			query9.AddToFilter(JobConsolSchema.JK_UniqueConsignRef, ConsolNumber);

			query8.AddSubQuery(JobConShipLinkSchema.JN_JK, query9, JoinCondition.And);
			query7.AddSubQuery(JobShipmentSchema.PK, query8, JoinCondition.And);
			query6.AddSubQuery(JobHeaderSchema.JH_ParentID, query7, JoinCondition.And);
			query6.AddToFilter(JobHeaderSchema.JH_GC, GetCompanyPK().ToGuid());
			query6.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix);

			query2.AddSubQuery(JobHeaderSchema.JH_ParentID, query3, JoinCondition.Or);
			query2.AddToFilter(JobHeaderSchema.JH_GC, GetCompanyPK().ToGuid());
			query2.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix);

			query2b.AddSubQuery(JobHeaderSchema.JH_JH_ParentJob, query6, JoinCondition.And);

			query1.AddSubQuery(AccTransactionLinesSchema.AL_JH, query2, JoinCondition.And);
			query1.AddSubQuery(AccTransactionLinesSchema.AL_JH, query2b, JoinCondition.Or);
			query1.AddToFilter(AccTransactionLinesSchema.AL_AH, SQLComparisonOperator.NotEqual, DBNull.Value);

			if (IsFreightConsol && ledgerType == LedgerTypes.AccountsReceivable)
			{
				var query0 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query0.AddSubQuery(query1, JoinCondition.And);
				AppendARFilters(query0);
				masterQuery.AddToFilter(query0, JoinCondition.And);
			}
			else
			{
				masterQuery.AddSubQuery(query1, JoinCondition.And);
			}

			return masterQuery;
		}

		ZDBOnlyQuery AppendCommonFilters(ZDBOnlyQuery masterQuery, ZString ledgerType)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			if (ledgerType == LedgerTypes.AccountsReceivable || ledgerType == LedgerTypes.AccountsPayable)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledgerType);

				var query_1 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query_1.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionType, new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote });
				query.AddToFilter(query_1, JoinCondition.And);
			}
			else if (!IsFreightConsol && ledgerType == LedgerTypes.JobCosting)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledgerType);
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
			}

			masterQuery.AddToFilter(query, JoinCondition.And);

			if (TransactionType != string.Empty)
			{
				var query1 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query1.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionType);
				masterQuery.AddToFilter(query1, JoinCondition.And);
			}

			if (!IncludePrinted)
			{
				var query2 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query2.AddToFilter(AccTransactionHeaderSchema.AH_InvoicePrinted, false);
				masterQuery.AddToFilter(query2, JoinCondition.And);
			}

			if (DebtorOrCreditor.IsValid)
			{
				var query3 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query3.AddToFilter(AccTransactionHeaderSchema.AH_OH, DebtorOrCreditor.ToGuid());
				masterQuery.AddToFilter(query3, JoinCondition.And);
			}

			if (!IncludeReversals)
			{
				var query4 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				var query4_1 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query4_1.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
				var query4_2 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query4_2.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, DBNull.Value);
				query4.AddToFilter(query4_1, JoinCondition.Or);
				query4.AddToFilter(query4_2, JoinCondition.Or);
				masterQuery.AddToFilter(query4, JoinCondition.And);
			}

			if (postDateFilterFrom.IsValid)
			{
				var query5 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query5.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualTo, postDateFilterFrom);
				masterQuery.AddToFilter(query5, JoinCondition.And);
			}

			if (postDateFilterTo.IsValid)
			{
				var query6 = new ZDBOnlyQuery(typeof(AccTransactionHeader));
				query6.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, postDateFilterTo);
				masterQuery.AddToFilter(query6, JoinCondition.And);
			}

			return masterQuery;
		}

		ZDBOnlyQuery AppendARFilters(ZDBOnlyQuery masterQuery)
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_GC, GetCompanyPK().ToGuid());
			query.AddToFilter(AccTransactionHeaderSchema.AH_JobNumber, ConsolNumber);
			masterQuery.AddToFilter(query, JoinCondition.Or);

			return masterQuery;
		}

		#endregion

		#endregion

		protected virtual ZGuid GetCompanyPK()
		{
			return GlbCompany.CurrentCompany.PK;
		}

		protected abstract IEnumerable<string> Ledger
		{
			get;
		}

		public virtual bool IncludeReversals
		{
			get { return true; }
		}

		#region Jobs On Consol Collection

		protected JobsOnConsolCollection fJobNumbers;

		public JobsOnConsolCollection JobNumbers
		{
			get
			{
				if (fJobNumbers == null)
				{
					fJobNumbers = new JobsOnConsolCollection(Factory);
				}
				if (JobHeaderPKs != null)
				{
					fJobNumbers.ReplaceRelationshipFilter(BuildFilter(JobHeaderSchema.PK, JobHeaderPKs));
				}

				return fJobNumbers;
			}
		}

		protected ZQuery BuildFilter(SchemaColumn column, ZGuid[] jobHeaderPKs)
		{
			ZQuery filter = new ZQuery();
			BusinessObject host = HostBusinessObject as BusinessObject;
			filter.FetchOnlyFromLocalCache = host != null && !host.IsInDatabase;
			filter.AddToFilter(column, jobHeaderPKs);
			return filter;
		}

		#endregion

		#region JobsOnConsolCollection Class

		public class JobsOnConsolCollection : JobHeaderCollection
		{
			public JobsOnConsolCollection(BusinessObjectFactory factory)
				: base(factory)
			{
				fRelationshipFilter = new ZQuery();
			}

			public void ReplaceRelationshipFilter(ZQuery filter)
			{
				fRelationshipFilter = (filter != null) ? new ZQuery(filter) : new ZQuery();
			}

			ZQuery fRelationshipFilter;

			protected override ZQuery CreateRelationshipFilter()
			{
				return fRelationshipFilter;
			}
		}

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			DebtorOrCreditor = ZGuid.Empty;
			JobNumber = ZGuid.Empty;
			TransactionType = ZString.Empty;
			IncludePrinted = true;
		}

		#endregion

		#region Include Printed

		protected ZBool includePrinted;

		public ZBool IncludePrinted
		{
			get { return includePrinted; }
			set { SetNonPersistentPropertyValue(IncludePrintedInfo, ref includePrinted, value); }
		}

		public ZPropertyInfo IncludePrintedInfo
		{
			get { return GetZPropertyInfo(Schema.IncludePrinted); }
		}

		#endregion

		#region Job Number

		protected ZGuid fJobNumber;

		[List("JobNumbers")]
		public ZGuid JobNumber
		{
			get { return fJobNumber; }
			set
			{
				fJobNumber = value;
				JobNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo JobNumberInfo
		{
			get { return GetZPropertyInfo(Schema.Job); }
		}

		#endregion

		#region Transaction Type

		protected ZString fTransactionType;
		[MaxLength(3)]
		[List("TransactionTypeList")]
		public ZString TransactionType
		{
			get { return fTransactionType; }
			set
			{
				if (fTransactionType != value)
				{
					CheckMaximumLength(TransactionTypeInfo, value);
					fTransactionType = value;
					TransactionTypeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TransactionTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TransactionType); }
		}

		public CodeDescriptionPairList fTransactionTypeList;

		public virtual CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (fTransactionTypeList == null)
				{
					fTransactionTypeList = new CodeDescriptionPairList(OLookUpEditType.InvoicePrintingTransactionTypes);
					fTransactionTypeList.RemoveCode(Enterprise.ZArchitecture.Core.TransactionTypes.AdjustmentNote);
				}
				return fTransactionTypeList;
			}
		}

		#endregion

		#region DebtorOrCreditor

		protected ZGuid fDebtorOrCreditor;

		[List("DebtorOrCreditorList")]
		public ZGuid DebtorOrCreditor
		{
			get { return fDebtorOrCreditor; }
			set
			{
				fDebtorOrCreditor = value;
				DebtorOrCreditorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DebtorOrCreditorInfo
		{
			get { return GetZPropertyInfo(Schema.DebtorOrCreditor); }
		}

		OrganisationsFindBoxCollection fDebtorOrCreditorList;
		public OrganisationsFindBoxCollection DebtorOrCreditorList
		{
			get
			{
				if (fDebtorOrCreditorList == null)
				{
					fDebtorOrCreditorList = GetDebtorOrCreditorFindBoxCollectionCore();
				}
				return fDebtorOrCreditorList;
			}
		}

		protected abstract OrganisationsFindBoxCollection GetDebtorOrCreditorFindBoxCollectionCore();
		#endregion
	}
}
