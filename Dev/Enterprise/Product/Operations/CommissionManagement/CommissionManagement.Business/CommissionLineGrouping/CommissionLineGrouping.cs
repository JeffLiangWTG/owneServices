using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public interface ICommissionLineGrouping
	{
		ZString SourceTableCode { get; }
		JobHeader Job { get; }
		ICommissionableTransaction Transaction { get; }
	}

	public abstract class CommissionLineGrouping<TGrouping, TLine> : AutoCommissionLineGrouping, ICommissionLineGrouping
		where TGrouping : CommissionLineGrouping<TGrouping, TLine>
		where TLine : BusinessObject, IViewCommissionLineProvider
	{
		#region Schema
		public new class Schema : AutoCommissionLineGrouping.Schema
		{
			public const string SourceNumber = "SourceNumber";
			public const string FirstRecognitionDate = "FirstRecognitionDate";

			public const string TotalCommissionableAmountInTransactionCurrency = "TotalCommissionableAmountInTransactionCurrency";

			public const string TotalCommissionableAmountInLocalCurrency = "TotalCommissionableAmountInLocalCurrency";
			public const string ShareCommissionAmountInLocalCurrency = "ShareCommissionAmountInLocalCurrency";
			public const string EntityCommissionAmountInLocalCurrency = "EntityCommissionAmountInLocalCurrency";

			public const string TotalCommissionableAmountInPreferredCurrency = "TotalCommissionableAmountInPreferredCurrency";
			public const string ShareCommissionAmountInPreferredCurrency = "ShareCommissionAmountInPreferredCurrency";
			public const string EntityCommissionAmountInPreferredCurrency = "EntityCommissionAmountInPreferredCurrency";
			public const string PaidEntityCommissionAmountInPreferredCurrency = "PaidEntityCommissionAmountInPreferredCurrency";
			public const string OutstandingEntityCommissionAmountInPreferredCurrency = "OutstandingEntityCommissionAmountInPreferredCurrency";

			public const string ApprovalRequestPk = "ApprovalRequestPk";
		}

		#endregion

		#region Constructors

		protected CommissionLineGrouping(BusinessObjectFactory factory, ViewCommissionLineGrouper<TLine>[] subGroupers)
			: base(factory)
		{
			this.commissionLineProviderCollection = new CommissionLineProviderCollection<TLine>(Factory);
			this.subGroupers = subGroupers;
		}

		public void Init(IEnumerable<TLine> commissionLineProviders)
		{
			if (!commissionLineProviders.Any())
			{
				throw new ArgumentException("commissionLineProviders cannot be empty");
			}

			var firstCommissionLine = commissionLineProviders.First().ViewCommissionLine;
			this.CompanyPk = firstCommissionLine.VCL_GC_Company;
			this.SourceId = firstCommissionLine.VCL_GroupingSourceID;
			this.SourceTableCode = firstCommissionLine.VCL_GroupingSourceTableCode;
			this.SourceUniqueId = firstCommissionLine.VCL_GroupingSourceUniqueId;
			this.StaffCode = firstCommissionLine.VCL_GS_NKStaff;
			this.JobNumber = firstCommissionLine.VCL_JobNumber;
			this.PartyPk = firstCommissionLine.VCL_OH_Party;

			this.TransactionCurrencyCode = firstCommissionLine.VCL_RX_NKTransactionCurrency;
			this.LocalCurrencyCode = firstCommissionLine.VCL_RX_NKLocalCurrency;
			this.PreferredPaymentCompanyPk = firstCommissionLine.VCL_GC_PreferredPaymentCompany;
			this.PreferredCurrencyCode = firstCommissionLine.VCL_RX_NKPreferredPaymentCurrency;
			this.LocalToPreferredExchangeRate = firstCommissionLine.VCL_LocalToPreferredExchangeRate;
			this.CommissionStatus = firstCommissionLine.CommissionStatus;
			this.IsCanceled = firstCommissionLine.IsCancelled;

			using (commissionLineProviderCollection.SuspendListChanged())
			{
				commissionLineProviderCollection.RemoveAll();
				commissionLineProviderCollection.AddRange(commissionLineProviders);
			}

			HookCommissionLineProviderEventHandlers();
		}

		#endregion

		#region Property

		#region CompanyPk

		[List("Lookups.Companies")]
		public override ZGuid CompanyPk
		{
			get { return base.CompanyPk; }
			set { base.CompanyPk = value; }
		}

		public GlbCompany Company
		{
			get { return Factory.Load<GlbCompany>(CompanyPk); }
		}

		#endregion

		#region Source

		public JobHeader Job
		{
			get
			{
				if (SourceTableCode == JobHeaderSchema.Constants.Prefix)
				{
					return Factory.Load<JobHeader>(SourceId);
				}

				return null;
			}
		}

		public ICommissionableTransaction Transaction
		{
			get
			{
				if (SourceTableCode == AccTransactionHeaderSchema.Constants.Prefix)
				{
					return Factory.Load<TransactionHeader>(SourceId) as ICommissionableTransaction;
				}

				return null;
			}
		}

		#endregion

		#region SourceNumber

		[ResourceStringData("CommissionLineGrouping|SourceNumber", Caption = "Job / Transaction")]
		public ZString SourceNumber
		{
			get
			{
				switch (SourceTableCode)
				{
					case JobHeaderSchema.Constants.Prefix:
						return JobNumber;

					case AccTransactionHeaderSchema.Constants.Prefix:
						var transactionHeader = Factory.Load<AccTransactionHeader>(SourceId);
						return transactionHeader != null ? transactionHeader.AH_TransactionNum : ZString.Empty;

					default:
						return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo SourceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.SourceNumber); }
		}

		#endregion

		#region SourceClientPk

		public OrgHeader SourceClient
		{
			get { return Factory.Load<OrgHeader>(SourceClientPk); }
		}

		[ResourceStringData("CommissionLineGrouping|SourceClientPk", Caption = "Client")]
		[List("Lookups.Organisations")]
		public ZGuid SourceClientPk
		{
			get
			{
				switch (SourceTableCode)
				{
					case JobHeaderSchema.Constants.Prefix:
						var job = Factory.Load<JobHeader>(SourceId);
						var localAddr = job != null ? job.LocalChargesAddr : null;
						return localAddr != null ? localAddr.OA_OH : ZGuid.Empty;

					case AccTransactionHeaderSchema.Constants.Prefix:
						var transactionHeader = Factory.Load<AccTransactionHeader>(SourceId);
						return transactionHeader != null ? transactionHeader.AH_OH : ZGuid.Empty;

					default:
						return ZGuid.Empty;
				}
			}
		}

		#endregion

		#region FirstRecognitionDate

		[ResourceStringData("CommissionLineGrouping|FirstRecognitionDate", Caption = "First Recognition Date", ShortCaption = "1st Recog. Date")]
		public ZDate FirstRecognitionDate
		{
			get
			{
				if (!firstRecognitionDate.HasValue)
				{
					if (IsLeaf)
					{
						firstRecognitionDate = CommissionLines.Any() ? CommissionLines.Min(x => x.RecognitionDate) : ZDate.Empty;
					}
					else
					{
						firstRecognitionDate = SubGroupings.Any() ? SubGroupings.Min(x => x.FirstRecognitionDate) : ZDate.Empty;
					}
				}

				return firstRecognitionDate.Value;
			}
		}
		ZDate? firstRecognitionDate;

		public ZPropertyInfo FirstRecognitionDateInfo
		{
			get { return GetZPropertyInfo(Schema.FirstRecognitionDate); }
		}

		#endregion

		#region HasARInvoiceFullyPaid

		[ResourceStringData("CommissionLineGrouping|HasARInvoicesFullyPaid", Caption = "AR Invoice(s) Fully Paid")]
		public ZBool HasARInvoiceFullyPaid
		{
			get { return !CommissionLines.Any(p => p.IsARInvoice && !p.HasFullyPaid); }
		}

		public ZPropertyInfo HasARInvoiceFullyPaidInfo
		{
			get { return GetZPropertyInfo(nameof(HasARInvoiceFullyPaid)); }
		}

		#endregion

		#region EntityCode

		[ResourceStringData("CommissionLineGrouping|EntityCode", Caption = "Entity")]
		public ZString EntityCode
		{
			get
			{
				if (!StaffCode.IsEmpty)
				{
					return StaffCode;
				}
				else if (PartyPk.IsValid)
				{
					var party = Factory.Load<OrgHeader>(PartyPk);
					return party != null ? party.OH_Code : ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		#region EntityName

		[ResourceStringData("CommissionLineGrouping|EntityName", Caption = "Entity Name")]
		public ZString EntityName
		{
			get
			{
				if (!StaffCode.IsEmpty)
				{
					return StaffName;
				}
				else if (PartyPk.IsValid)
				{
					return PartyName;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		#endregion

		#region StaffCode

		[List("Lookups.Staff")]
		public override ZString StaffCode
		{
			get { return base.StaffCode; }
			set { base.StaffCode = value; }
		}

		#endregion

		#region StaffName

		[ResourceStringData("CommissionLineGrouping|StaffName", Caption = "Staff Name")]
		public ZString StaffName
		{
			get
			{
				if (!StaffCode.IsEmpty)
				{
					var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, StaffCode);
					return staff != null ? staff.GS_FullName : ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region PartyPk

		[List("Lookups.Organisations")]
		public override ZGuid PartyPk
		{
			get { return base.PartyPk; }
			set { base.PartyPk = value; }
		}

		public OrgHeader Party
		{
			get { return Factory.Load<OrgHeader>(PartyPk); }
		}

		#endregion

		#region PartyName

		[ResourceStringData("CommissionLineGrouping|PartyName", Caption = "Organization Name")]
		public ZString PartyName
		{
			get
			{
				if (PartyPk.IsValid)
				{
					var party = Factory.Load<OrgHeader>(PartyPk);
					return party != null ? party.OH_FullName : ZString.Empty;
				}

				return ZString.Empty;
			}
		}

		#endregion

		#region TransactionCurrency

		public RefCurrency TransactionCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, TransactionCurrencyCode); }
		}

		public int TransactionCurrencyDecimalPlaces => TransactionCurrency?.Decimals ?? LocalCurrencyDecimalPlaces;

		#endregion

		#region LocalCurrency

		public RefCurrency LocalCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, LocalCurrencyCode); }
		}

		public int LocalCurrencyDecimalPlaces => LocalCurrency?.Decimals ?? Company.GetLocalDecimals();

		#endregion

		#region PreferredCurrency

		public RefCurrency PreferredCurrency
		{
			get { return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, PreferredCurrencyCode); }
		}

		public int PreferredCurrencyDecimalPlaces => PreferredCurrency?.Decimals ?? LocalCurrencyDecimalPlaces;

		#endregion

		#region PreferredPaymentCompanyPk

		[List("Lookups.Companies")]
		public override ZGuid PreferredPaymentCompanyPk
		{
			get { return base.PreferredPaymentCompanyPk; }
			set { base.PreferredPaymentCompanyPk = value; }
		}

		public GlbCompany PreferredPaymentCompany
		{
			get { return Factory.Load<GlbCompany>(PreferredPaymentCompanyPk); }
		}

		#endregion

		#region TotalCommissionableAmount

		#region TotalCommissionableAmountInTransactionCurrency

		[ResourceStringData("CommissionLineGrouping|TotalCommissionableAmountInTransactionCurrency", Caption = "Transaction Commissionable Amount", ShortCaption = "Trans Comm Amount")]
		[DecimalPlaces(nameof(TransactionCurrencyDecimalPlaces))]
		public ZDecimal TotalCommissionableAmountInTransactionCurrency
		{
			get
			{
				return IsLeaf ?
					CommissionLines.Sum(x => x.VCL_TransactionAmount) :
					SubGroupings.Sum(x => x.TotalCommissionableAmountInTransactionCurrency);
			}
		}

		public ZPropertyInfo TotalCommissionableAmountInTransactionCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.TotalCommissionableAmountInTransactionCurrency); }
		}

		#endregion

		#region TotalCommissionableAmountInLocalCurrency

		[ResourceStringData("CommissionLineGrouping|TotalCommissionableAmountInLocalCurrency", Caption = "Local Commissionable Amount", ShortCaption = "Local Comm Amount")]
		[DecimalPlaces(nameof(LocalCurrencyDecimalPlaces))]
		public ZDecimal TotalCommissionableAmountInLocalCurrency
		{
			get
			{
				return IsLeaf ?
					CommissionLines.Sum(x => x.VCL_TotalCommissionableAmountInLocalCurrency) :
					SubGroupings.Sum(x => x.TotalCommissionableAmountInLocalCurrency);
			}
		}

		public ZPropertyInfo TotalCommissionableAmountInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.TotalCommissionableAmountInLocalCurrency); }
		}

		#endregion

		#region TotalCommissionableAmountInPreferredCurrency

		[ResourceStringData("CommissionLineGrouping|TotalCommissionableAmountInPreferredCurrency", Caption = "Commissionable Amount (Preferred Currency)", MediumCaption = "Commissionable Amount", ShortCaption = "Comm Amount")]
		[DecimalPlaces(nameof(PreferredCurrencyDecimalPlaces))]
		public ZDecimal TotalCommissionableAmountInPreferredCurrency
		{
			get
			{
				return IsLeaf ?
					CommissionLines.Sum(x => x.VCL_TotalCommissionableAmountInPreferredCurrency) :
					SubGroupings.Sum(x => x.TotalCommissionableAmountInPreferredCurrency);
			}
		}

		public ZPropertyInfo TotalCommissionableAmountInPreferredCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.TotalCommissionableAmountInPreferredCurrency); }
		}

		#endregion

		#endregion

		#region ShareCommissionAmount

		#region ShareCommissionAmountInLocalCurrency

		[ResourceStringData("CommissionLineGrouping|ShareCommissionAmountInLocalCurrency", Caption = "Local Share Amount")]
		[DecimalPlaces(nameof(LocalCurrencyDecimalPlaces))]
		public ZDecimal ShareCommissionAmountInLocalCurrency
		{
			get
			{
				return IsLeaf ?
					CommissionLines.Sum(x => x.VCL_ShareCommissionAmountInLocalCurrency) :
					SubGroupings.Sum(x => x.ShareCommissionAmountInLocalCurrency);
			}
		}

		public ZPropertyInfo ShareCommissionAmountInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.ShareCommissionAmountInLocalCurrency); }
		}

		#endregion

		#region ShareCommissionAmountInPreferredCurrency

		[ResourceStringData("CommissionLineGrouping|ShareCommissionAmountInPreferredCurrency", Caption = "Share Amount (Preferred Currency)", ShortCaption = "Share Amount")]
		[DecimalPlaces(nameof(PreferredCurrencyDecimalPlaces))]
		public ZDecimal ShareCommissionAmountInPreferredCurrency
		{
			get
			{
				return IsLeaf ?
					CommissionLines.Sum(x => x.VCL_ShareCommissionAmountInPreferredCurrency) :
					SubGroupings.Sum(x => x.ShareCommissionAmountInPreferredCurrency);
			}
		}

		public ZPropertyInfo ShareCommissionAmountInPreferredCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.ShareCommissionAmountInPreferredCurrency); }
		}

		#endregion

		#endregion

		#region EntityCommissionAmount

		#region EntityCommissionAmountInLocalCurrency

		[ResourceStringData("CommissionLineGrouping|EntityCommissionAmountInLocalCurrency", Caption = "Local Entity Amount")]
		[DecimalPlaces(nameof(LocalCurrencyDecimalPlaces))]
		public ZDecimal EntityCommissionAmountInLocalCurrency
		{
			get
			{
				return IsLeaf ?
					CommissionLines.Sum(x => x.VCL_EntityCommissionAmountInLocalCurrency) :
					SubGroupings.Sum(x => x.EntityCommissionAmountInLocalCurrency);
			}
		}

		public ZPropertyInfo EntityCommissionAmountInLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.EntityCommissionAmountInLocalCurrency); }
		}

		#endregion

		#region EntityCommissionAmountInPreferredCurrency

		[ResourceStringData("CommissionLineGrouping|EntityCommissionAmountInPreferredCurrency", Caption = "Entity Amount (Preferred Currency)", ShortCaption = "Entity Amount")]
		[DecimalPlaces(nameof(PreferredCurrencyDecimalPlaces))]
		public ZDecimal EntityCommissionAmountInPreferredCurrency
		{
			get
			{
				return IsLeaf ?
					CommissionLines.Sum(x => x.VCL_EntityCommissionAmountInPreferredCurrency) :
					SubGroupings.Sum(x => x.EntityCommissionAmountInPreferredCurrency);
			}
		}

		public ZPropertyInfo EntityCommissionAmountInPreferredCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.EntityCommissionAmountInPreferredCurrency); }
		}

		#endregion

		#endregion

		#region PaidEntityCommissionAmount

		[ResourceStringData("CommissionLineGrouping|PaidEntityCommissionAmountInPreferredCurrency", Caption = "Paid Entity Amount (Preferred Currency)", ShortCaption = "Paid Amount")]
		[DecimalPlaces(nameof(PreferredCurrencyDecimalPlaces))]
		public ZDecimal PaidEntityCommissionAmountInPreferredCurrency
		{
			get
			{
				return IsLeaf ?
					CommissionLines.Where(x => x.IsPaid).Sum(x => x.VCL_EntityCommissionAmountInPreferredCurrency) :
					SubGroupings.Sum(x => x.PaidEntityCommissionAmountInPreferredCurrency);
			}
		}

		public ZPropertyInfo PaidEntityCommissionAmountInPreferredCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.PaidEntityCommissionAmountInPreferredCurrency); }
		}

		#endregion

		#region Customer

		[ResourceStringData("CommissionLineGrouping|CustomerCodes", Caption = "Customer Code", ShortCaption = "Code")]
		public ZString CustomerCodes
		{
			get { return Abbreviate(string.Join(", ", Customers.Select(c => c.OH_Code).OrderBy(c => c)), 100); }
		}

		public ZPropertyInfo CustomerCodesInfo
		{
			get { return GetZPropertyInfo(nameof(CustomerCodes)); }
		}

		[ResourceStringData("CommissionLineGrouping|CustomerNames", Caption = "Customer Name", ShortCaption = "Customer")]
		public ZString CustomerNames
		{
			get { return Abbreviate(string.Join(", ", Customers.Select(c => c.OH_FullNameTruncated).OrderBy(c => c)), 100); }
		}

		public ZPropertyInfo CustomerNamesInfo
		{
			get { return GetZPropertyInfo(nameof(CustomerNames)); }
		}

		static ZString Abbreviate(ZString val, int maxLength)
		{
			if (val.Length <= maxLength)
			{
				return val;
			}
			return val.Substring(0, maxLength - 3) + "...";
		}

		List<OrgHeader> Customers
		{
			get
			{
				if (IsLeaf)
				{
					return CommissionLines.Select(cl => cl?.CommissionHeader?.Customer).Distinct().Where(o => o != null).ToList();
				}
				else
				{
					return SubGroupings.SelectMany(sg => sg.Customers).Distinct().Where(o => o != null).ToList();
				}
			}
		}

		#endregion

		#region TransportMode

		[ResourceStringData("CommissionLineGrouping|TransportMode", Caption = "Transport Mode", ShortCaption = "Mode")]
		public ZString TransportMode
		{
			get
			{
				var job = Job;
				if (job == null)
				{
					return ZString.Empty;
				}
				job.InitializeParentFromGenericJobWithoutSettingDefaults();
				return JobRelatedTransactionCommissionCreator.GetCommissionableMode(job);
			}
		}

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(nameof(TransportMode)); }
		}

		#endregion

		#region Origin

		[ResourceStringData("CommissionLineGrouping|Origin", Caption = "Origin", ShortCaption = "Origin")]
		public ZString Origin
		{
			get
			{
				var job = Job;
				if (job == null)
				{
					return ZString.Empty;
				}
				job.InitializeParentFromGenericJobWithoutSettingDefaults();
				return JobRelatedTransactionCommissionCreator.GetCommissionableOrigin(job);
			}
		}

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(nameof(Origin)); }
		}

		#endregion

		#region Destination

		[ResourceStringData("CommissionLineGrouping|Destination", Caption = "Destination", ShortCaption = "Destination")]
		public ZString Destination
		{
			get
			{
				var job = Job;
				if (job == null)
				{
					return ZString.Empty;
				}
				job.InitializeParentFromGenericJobWithoutSettingDefaults();
				return JobRelatedTransactionCommissionCreator.GetCommissionableDestination(job);
			}
		}

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(nameof(Destination)); }
		}

		#endregion

		#region OutstandingEntityCommissionAmount

		[ResourceStringData("CommissionLineGrouping|OutstandingEntityCommissionAmountInPreferredCurrency", Caption = "Outstanding Entity Amount (Preferred Currency)", ShortCaption = "Outstanding Amount")]
		[DecimalPlaces(nameof(PreferredCurrencyDecimalPlaces))]
		public ZDecimal OutstandingEntityCommissionAmountInPreferredCurrency
		{
			get
			{
				return IsLeaf ?
					CommissionLines.Where(x => !x.IsPaid).Sum(x => x.VCL_EntityCommissionAmountInPreferredCurrency) :
					SubGroupings.Sum(x => x.OutstandingEntityCommissionAmountInPreferredCurrency);
			}
		}

		public ZPropertyInfo OutstandingEntityCommissionAmountInPreferredCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.OutstandingEntityCommissionAmountInPreferredCurrency); }
		}

		#endregion

		#region ApprovalRequestPk

		[ResourceStringData("CommissionLineGrouping|ApprovalRequestPk", Caption = "Approval Request")]
		[List("Lookups.CommissionApprovalRequests")]
		public ZGuid ApprovalRequestPk
		{
			get { return CommissionLines.First().ApprovalRequestPk; }
		}

		public ZPropertyInfo ApprovalRequestPkInfo
		{
			get { return GetZPropertyInfo(Schema.ApprovalRequestPk); }
		}

		public AccCommissionApprovalRequest ApprovalRequest
		{
			get { return Factory.Load<AccCommissionApprovalRequest>(ApprovalRequestPk); }
		}

		#endregion

		#region CommissionStatus

		[ResourceStringData("CommissionLineGrouping|CommissionStatusDescription", Caption = "Commission Approval Status", ShortCaption = "Approval Status")]
		public ZString CommissionStatusDescription
		{
			get { return Lookups.CommissionStatuses.GetDescriptionFromCode(CommissionStatus); }
		}

		#endregion

		#endregion

		#region HasChanges

		public override bool HasChanges
		{
			get { return false; }
			set
			{
				// Do nothing
			}
		}

		#endregion

		#region Fetch Strategy

		protected override IBusinessObjectFetchStrategy GetFetchStrategy()
		{
			return new CommissionLineGroupingFetchStrategy<TGrouping, TLine>(this);
		}

		#endregion

		#region Lookups

		public CommissionLineGroupingLookups<TGrouping, TLine> Lookups
		{
			get { return new CommissionLineGroupingLookups<TGrouping, TLine>(this); }
		}

		#endregion

		#region Validation

		public new CommissionLineGroupingValidation<TGrouping, TLine> Validation
		{
			get { return (CommissionLineGroupingValidation<TGrouping, TLine>)base.Validation; }
		}

		protected override CommissionLineGroupingValidation GetNewValidation()
		{
			return new CommissionLineGroupingValidation<TGrouping, TLine>(this);
		}

		public ZBool ShouldRollupCommissionLineNotifications = false;

		#endregion

		#region SubGroupings

		public ZBool IsLeaf
		{
			get { return subGroupers == null || !subGroupers.Any(); }
		}

		public CommissionLineGroupingCollection<TGrouping, TLine> SubGroupingCollection
		{
			get
			{
				if (subGroupingCollection == null)
				{
					this.subGroupingCollection = GetNewSubGroupingCollection(Factory);

					if (subGroupers != null && subGroupers.Any())
					{
						var currentSubGrouper = subGroupers.First();
						var subGroups = currentSubGrouper.GetGroupings(CommissionLineProviders);
						using (subGroupingCollection.SuspendListChanged())
						{
							foreach (var subGroup in subGroups)
							{
								subGroupingCollection.AddNew(subGroup, subGroupers.Skip(1).ToArray());
							}
						}

						HookSubGroupingEventHandlers();
					}
				}

				return subGroupingCollection;
			}
		}
		CommissionLineGroupingCollection<TGrouping, TLine> subGroupingCollection;

		public IEnumerable<TGrouping> SubGroupings
		{
			get { return SubGroupingCollection.Cast<TGrouping>(); }
		}

		protected abstract CommissionLineGroupingCollection<TGrouping, TLine> GetNewSubGroupingCollection(BusinessObjectFactory factory);

		protected virtual void HookSubGroupingEventHandlers()
		{
		}

		readonly IEnumerable<ViewCommissionLineGrouper<TLine>> subGroupers;

		#endregion

		#region CommissionLineProviders

		public CommissionLineProviderCollection<TLine> CommissionLineProviderCollection
		{
			get { return commissionLineProviderCollection; }
		}
		readonly CommissionLineProviderCollection<TLine> commissionLineProviderCollection;

		public IEnumerable<TLine> CommissionLineProviders
		{
			get { return CommissionLineProviderCollection.Cast<TLine>(); }
		}

		protected CommissionLineProviderCollection<TLine> GetNewCommissionLineProviderCollection(BusinessObjectFactory factory, IEnumerable<TLine> commissionLineProviders)
		{
			return new CommissionLineProviderCollection<TLine>(factory, commissionLineProviders);
		}

		protected virtual void HookCommissionLineProviderEventHandlers()
		{
		}

		#endregion

		#region CommissionLines

		public IEnumerable<ViewCommissionLine> CommissionLines
		{
			get { return CommissionLineProviders.Select(x => x.ViewCommissionLine); }
		}

		public IEnumerable<ViewCommissionLine> CommissionLinesIncludingSubGroupings
		{
			get
			{
				if (IsLeaf)
				{
					foreach (var commissionLine in CommissionLines)
					{
						yield return commissionLine;
					}
				}
				else
				{
					foreach (var subGrouping in SubGroupings)
					{
						foreach (var subGroupingLine in subGrouping.CommissionLinesIncludingSubGroupings)
						{
							yield return subGroupingLine;
						}
					}
				}
			}
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("3c61a9bc-6167-49a3-b221-75e8e999c55a", "Entity Commission"); }
		}

		#endregion
	}
}
