using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobDraftInvoicePrintingFilter : NonPersistentBusinessObject
	{
		public abstract class Schema
		{
			public const string Creditor = "Creditor";
			public const string TransactionType = "TransactionType";
			public const string Transactions = "Transactions";
		}

		public JobDraftInvoicePrintingFilter(BusinessObject jobParent) : base(new BusinessObjectFactory() { NameForDebugging = "JobDraftInvoicePrintingFilter_ReadOnly" })
		{
			JobParent = jobParent;

			RefreshInvoiceList();
		}

		#region Properties

		#region Transactions

		public AccDraftInvoiceHeaderCollection Transactions
		{
			get
			{
				if (transactions == null)
				{
					transactions = new AccDraftInvoiceHeaderCollection(Factory);
					transactions.SetReadOnlyIncludingChildren(true);
				}
				return transactions;
			}
		}
		AccDraftInvoiceHeaderCollection transactions;

		#endregion

		#region Transaction Type

		[MaxLength(3)]
		[List("TransactionTypeList")]
		public ZString TransactionType
		{
			get { return transactionType; }
			set
			{
				if (transactionType != value)
				{
					CheckMaximumLength(TransactionTypeInfo, value);
					transactionType = value;
					TransactionTypeInfo.RefreshBinding();
				}
			}
		}
		ZString transactionType;

		public ZPropertyInfo TransactionTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TransactionType); }
		}

		public CodeDescriptionPairList TransactionTypeList
		{
			get
			{
				if (transactionTypeList == null)
				{
					transactionTypeList = new CodeDescriptionPairList(OLookUpEditType.InvoicePrintingTransactionTypes);
					transactionTypeList.RemoveCode(TransactionTypes.AdjustmentNote);
				}
				return transactionTypeList;
			}
		}
		CodeDescriptionPairList transactionTypeList;

		#endregion

		#region Creditor

		[List("CreditorList")]
		public ZGuid Creditor
		{
			get { return creditor; }
			set
			{
				creditor = value;
				CreditorInfo.RefreshBinding();
			}
		}
		ZGuid creditor;

		public ZPropertyInfo CreditorInfo
		{
			get { return GetZPropertyInfo(Schema.Creditor); }
		}

		public OrganisationsFindBoxCollection CreditorList
		{
			get
			{
				if (creditorList == null)
				{
					creditorList = new CreditorCollection(Factory);
				}
				return creditorList;
			}
		}
		OrganisationsFindBoxCollection creditorList;

		#endregion

		#endregion

		public void ResetInvoiceList()
		{
			Creditor = ZGuid.Empty;
			TransactionType = ZString.Empty;
			RefreshInvoiceList();
		}

		public void RefreshInvoiceList()
		{
			if ((JobParent == null || !JobParent.IsInDatabase))
			{
				return;
			}

			Transactions.RemoveAll();

			var allTransactions = new AccDraftInvoiceHeaderCollection(Factory);
			allTransactions.Load(GetQueryOfJobInvoice());
			Transactions.AddRange(allTransactions.Where(x => x.PostingStatus != "PST"));
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Creditor = ZGuid.Empty;
			TransactionType = ZString.Empty;
		}

		ZQuery GetQueryOfJobInvoice()
		{
			var masterQuery = new ZDBOnlyQuery(typeof(AccDraftInvoiceHeader));
			masterQuery.AddToFilter(AccDraftInvoiceHeaderSchema.AIH_GC_Company, GlbCompany.CurrentCompany.PK);

			var jobQuery = new ZDBOnlySubQuery(typeof(AccDraftInvoiceJob), AccDraftInvoiceJobSchema.AIJ_AIC_Cluster);
			jobQuery.AddToFilter(AccDraftInvoiceJobSchema.AIJ_ParentID, JobParent.PK);
			if (JobParent is ForwardingShipment)
			{
				var jobConsolShipmentLinkQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JK);
				jobConsolShipmentLinkQuery.AddToFilter(JobConShipLinkSchema.JN_JS, JobParent.PK);

				jobQuery.AddSubQuery(AccDraftInvoiceJobSchema.AIJ_ParentID, JobConShipLinkSchema.JN_JK, jobConsolShipmentLinkQuery, JoinCondition.Or);
			}

			var jobClusterQuery = new ZDBOnlySubQuery(typeof(AccDraftInvoiceJobCluster), AccDraftInvoiceJobClusterSchema.AIC_AIH_Header);
			jobClusterQuery.AddSubQuery(jobQuery, JoinCondition.And);

			masterQuery.AddSubQuery(jobClusterQuery, JoinCondition.And);

			if (Creditor.IsValid)
			{
				masterQuery.AddToFilter(AccDraftInvoiceHeaderSchema.AIH_OH_Creditor, Creditor);
			}

			if (!TransactionType.IsEmpty)
			{
				masterQuery.AddToFilter(AccDraftInvoiceHeaderSchema.AIH_TransactionType, TransactionType);
			}

			return masterQuery;
		}

		public readonly BusinessObject JobParent;
	}
}
