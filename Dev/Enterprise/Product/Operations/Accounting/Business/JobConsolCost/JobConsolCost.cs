using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;
using static System.FormattableString;
using AccGenericConsol = Enterprise.Accounting.Business.GenericConsol.GenericConsol;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	[DebuggerDisplay("{DebuggerDisplay}")]
	[ProvideMetaDataProperty("PropertiesReadOnlyState", MetaDataTypes.ReadOnly)]
	public partial class JobConsolCost : AutoJobConsolCost,
		IJobConsolCost,
		Enterprise.Integration.Rating.IRatingJobConsolCost,
		IQuickCalculatorCharge,
		IPaymentBasisViewCharge,
		ISupportCriticalValidation,
		IHaveConstructorStackTrace,
		IApportionedChargesHeader,
		IAutoRatingChargeInfo
	{
		string DebuggerDisplay => string.Join("-", ChargeCode?.AC_Code ?? "null", Consol?.JK_UniqueConsignRef ?? "null", Creditor?.OH_Code ?? "null");

		#region Schema

		public new abstract class Schema : AutoJobConsolCost.Schema
		{
			public const string E6_OSGSTRealAmount = "E6_OSGSTRealAmount";
			public const string E6_OSExtraTaxAmount = "E6_OSExtraTaxAmount";
			public const string E6_Calc_LocalGSTAmount = "E6_Calc_LocalGSTAmount";
			public const string E6_Calc_LocalTotalAmount = "E6_Calc_LocalTotalAmount";
			public const string GSTInclusiveAmount = "GSTInclusiveAmount";
			public const string E6_ApportionToRelatedShipments = "E6_ApportionToRelatedShipments";
			public const string IsApproved = "IsApproved";
			public const string ChargeCodeDescription = "ChargeCodeDescription";
			public const string InvoiceOSTotal = "InvoiceOSTotal";
			public const string InvoiceOSTax = "InvoiceOSTax";
			public const string InvoiceOSCurrency = "InvoiceOSCurrency";
			public const string InvoiceLocalTotal = "InvoiceLocalTotal";
			public const string InvoiceLocalTax = "InvoiceLocalTax";
			public const string E6_CostGovtChargeCode = "E6_CostGovtChargeCode";
			public const string E6_SellGovtChargeCode = "E6_SellGovtChargeCode";
			public const string E6_MasterBillNumber = "E6_MasterBillNumber";
			public const string E6_ConsolCostAccrual = "E6_ConsolCostAccrual";
			public const string E6_ConsolTotalAccrual = "E6_ConsolTotalAccrual";
			public const string E6_OSGSTAmount_Calc = "E6_OSGSTAmount_Calc";
			public const string E6_AW = "E6_AW";
			public const string E6_OSWHTAmount = "E6_OSWHTAmount";
			public const string CostTaxBranchName = "CostTaxBranchName";
			public const string ChargeGroup = "ChargeGroup";
			public const string ChargeCodeSubGroup = "ChargeCodeSubGroup";
		}

		#endregion

		public JobConsolCost(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			JobsWithMutexes = new List<Job>();

			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(E6_RX_NKCurrency), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(E6_AH_APInvoice), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(E6_AH_ARInvoice), ConcurrencyPolicy.Strict);

			this.SetConstructorStackTrace();

			Tracer = ObjectFactory.Get<ITracer>();
		}

		#region IHaveConstructorStackTrace member

		StackTrace IHaveConstructorStackTrace.ConstructorStackTrace { get; set; }

		#endregion

		public readonly List<Job> JobsWithMutexes;

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			E6_Sequence = 0;
		}

		#endregion

		#region Saving

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && !IsForIncompleteInvoice; }
		}

		public bool IsForIncompleteInvoice
		{
			get { return ParentAPInvoice != null && !ParentAPInvoice.IsDeleted && ParentAPInvoice.AH_Ledger == LedgerTypes.IncompleteTransactions; }
		}

		bool FactoryWithValidContext
		{
			get { return !Factory.HasContext(BusinessContext.IncompleteInvoiceSaving) && !Factory.HasContext(BusinessContext.PreviewInvoice); }
		}

		protected sealed override void OnFactorySaving()
		{
			if (FactoryWithValidContext)
			{
				if (IsInDatabase && Factory.RefreshEnabled)
				{
					var reloader = Factory.ServiceContainer.GetService<ChargeReloader>() ?? Factory.ServiceContainer.AddService(new ChargeReloader(Factory));
					reloader.Reload();
				}

				base.OnFactorySaving();

				OnFactorySavingCore();
			}
			else if (IsForIncompleteInvoice)
			{
				PaymentBases.DeleteAll();
			}
		}

		protected virtual void OnFactorySavingCore()
		{
		}

		protected sealed override void OnFactorySavingBeforeTransactionCore()
		{
			CollectApportionmentChargesInfoOnSaving();

			if (FactoryWithValidContext)
			{
				base.OnFactorySavingBeforeTransactionCore();
				var context = GetUserContextForCorrectCompany();
				if (context != Env.CurrentUserContext)
				{
					using (Env.SetTemporaryUserContext(context))
					{
						OnFactorySavingBeforeTransactionCoreInCompanyContext();
					}
				}
				else
				{
					OnFactorySavingBeforeTransactionCoreInCompanyContext();
				}
			}
		}

		void CollectApportionmentChargesInfoOnSaving()
		{
			var monitor = Factory.ServiceContainer.GetService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>();
			monitor?.CollectApportionmentChargesInfo(this);
		}

		void OnFactorySavingBeforeTransactionCoreInCompanyContext()
		{
			CalculationStrategy.PrepareForPosting();
			if (!IsInDatabase)
			{
				SynchroniseUnpostedInvoiceDetailsIfNecessary();
			}

			OnFactorySavingBeforeTransactionCore2();
		}

		protected virtual void OnFactorySavingBeforeTransactionCore2()
		{
		}

		protected virtual void OnSavingCore()
		{
		}

		protected sealed override void OnFactorySaved(bool saveSucceeded)
		{
			if (FactoryWithValidContext)
			{
				base.OnFactorySaved(saveSucceeded);
				if (saveSucceeded)
				{
					HasSaveBeenRun = false;
				}

				OnFactorySavedCore(saveSucceeded);

				if (Factory.ServiceContainer.GetService<ChargeReloader>() != null)
				{
					Factory.ServiceContainer.RemoveService<ChargeReloader>();
				}
			}
		}

#if DEBUG
		public void OnFactorySaved_ForTestOnly(bool saveSucceeded)
		{
			OnFactorySaved(saveSucceeded);
		}
#endif

		protected virtual void OnFactorySavedCore(bool saveSucceeded)
		{
		}

		internal bool HasSaveBeenRun;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		IUserContext GetUserContextForCorrectCompany()
		{
			var result = Env.CurrentUserContext;

			if (!IsDeleted && E6_GC.IsValid && E6_GC != GlbCompany.CurrentCompany.PK)
			{
				var branchPK = ZGuid.Empty;
				var departmentGuid = result.Department.PK;
				if (fApportionmentCharges == null || fApportionmentCharges.Count == 0)
				{
					branchPK = Factory.Load<GlbCompany>(E6_GC)?.FirstActiveBranch?.PK ?? ZGuid.Empty;
				}
				else
				{
					var apportionmentCharge = ApportionmentCharges.Cast<ApportionSplitCharge>().FirstOrDefault(x => x.Branch != null && x.Branch.IsInDatabase);
					if (apportionmentCharge != null)
					{
						branchPK = apportionmentCharge.JR_GB;
						if (apportionmentCharge.JR_GE.IsValid)
						{
							departmentGuid = apportionmentCharge.JR_GE.ToGuid();
						}
					}
				}

				if (branchPK.IsValid)
				{
					var strategy = new DefaultErrorReportStrategy() { IsSilentReport = true };
					result = new UserContext(result.User.LoginName, branchPK.ToGuid(), departmentGuid, strategy, factory: Factory);

					if (strategy.NotificationMessages.Count > 0)
					{
						throw new ZCannotSaveException(strategy.NotificationMessages[0], "Cannot save the consol cost.");
					}
				}
			}
			return result;
		}

		#endregion

		#region Delete

		/// <summary>
		/// Performs a deletion of the Job Consol Cost using the internal calcaulation strategy which may or may not cascade
		/// to charges.
		/// </summary>
		public override void Delete()
		{
			if (IsPostedCorrectly)
			{
				throw new CannotDeleteException("This consol cost is already posted and cannot be deleted.");
			}
			else
			{
				CalculationStrategy.HandleDelete();
			}

			Attributes.RemoveAndDeleteAll();
		}

		/// <summary>
		/// Performs a deletion of the Job Consol Cost and any associated charges.
		/// </summary>
		public void DeleteCostAndCharges()
		{
			if (IsPostedCorrectly)
			{
				throw new CannotDeleteException("This consol cost is already posted and cannot be deleted.");
			}
			else
			{
				new ConsolCostCalculationStrategy(this).HandleDelete();
			}
		}

		public override bool CanDelete
		{
			get
			{
				return (!IsPostedCorrectly) && ApportionmentCharges.All(x => x.CanDelete);
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var reasonForNotAbleToDeleteList = new List<string>();

				foreach (var charge in ApportionmentCharges)
				{
					var reason = charge.ReasonForNotAbleToDelete;
					if (!charge.CanDelete && !reasonForNotAbleToDeleteList.Contains(reason))
					{
						reasonForNotAbleToDeleteList.Add(reason);
					}
				}

				var reasonForNotAbleToDelete = new ZStringBuilder();
				reasonForNotAbleToDeleteList.ForEach(reason => reasonForNotAbleToDelete.Append(reason));
				return (NoResString)reasonForNotAbleToDelete.ToStringWithNewLineBetweenAppends();
			}
		}

		protected override void DeleteForDataRefresh()
		{
			base.DeleteForDataRefresh();

			Attributes.RemoveAndDeleteAll();
		}

		/// <summary>
		/// Deletes this cost only with no checks.
		/// </summary>
		void DeleteCostOnly()
		{
			base.Delete();
		}

		public bool IsPostedCorrectly
		{
			get
			{
				Lazy<bool> isApportionedCorrectly = new Lazy<bool>(() =>
				{
					var result = ApportionmentCharges.ArePostedToSameAPInvoiceAsConsolCost
						&& ApportionmentCharges.AreInvoiceDetailsInSyncWithConsolCost
						&& ApportionmentCharges.AreInvoiceDetailsInSyncWithPostedInvoiceLines
							|| ApportionmentCharges.ArePostedWithJobRevenueJournal;
					return result;
				});

				return IsPosted && isApportionedCorrectly.Value;
			}
		}

		#endregion

		#region Related Objects

		public IJobCostingPlugIn Consol
		{
			get
			{
				return AccGenericConsol.LoadConsolBOFromParentIdAndCode(Factory, E6_ParentID, E6_ParentTableCode);
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			E6_GC = GlbCompany.CurrentCompany.PK;
			if (!E6_ParentID.IsValid)
			{
				using (ReportSettingParentSuspender.GetSuspender())
				{
					SetE6_ParentIDAndE6_ParentTableCodeTogether(Factory.NewWithValidTestData<ForwardingConsol>().PK, JobConsolSchema.Constants.Prefix);
				}
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif

		public InvoicingBase ParentAPInvoice { get; set; }

		bool IsForeignCurrencyParentAPInvoice
		{
			get { return ParentAPInvoice != null && ParentAPInvoice.AH_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		[ChildEditable(false)]
		public ApportionmentSplitChargeCollection ApportionmentCharges
		{
			get
			{
				if (fApportionmentCharges == null)
				{
					fApportionmentCharges = new ApportionmentSplitChargeCollection(this);
					RegisterEditableChildObject(fApportionmentCharges);
					fApportionmentCharges.Load();
					UpdateApportionmentChargesListing();
				}
				return fApportionmentCharges;
			}
		}
		ApportionmentSplitChargeCollection fApportionmentCharges;

		[ChildEditable(false)]
		public ApportionmentSplitChargeFilteredCollection FilteredApportionmentCharges
		{
			get
			{
				if (filteredApportionmentCharges == null && ApportionmentCharges != null)
				{
					filteredApportionmentCharges = new ApportionmentSplitChargeFilteredCollection(ApportionmentCharges);
					RegisterEditableChildObject(filteredApportionmentCharges);
				}
				return filteredApportionmentCharges;
			}
		}
		ApportionmentSplitChargeFilteredCollection filteredApportionmentCharges;

		[ChildEditable(false)]
		public CostPaymentBasisCollection PaymentBases
		{
			get
			{
				if (paymentBases == null)
				{
					paymentBases = new CostPaymentBasisCollection(this);
					paymentBases.Load();
					RegisterEditableChildObject(paymentBases);
				}

				return paymentBases;
			}
		}
		CostPaymentBasisCollection paymentBases;

		#region Attributes

		[ChildEditable(true)]
		public JobConsolCostAttribCollection Attributes
		{
			get
			{
				if (attributes == null)
				{
					attributes = new JobConsolCostAttribCollection(this);
					attributes.Load();
					RegisterEditableChildObject(attributes);
				}

				return attributes;
			}
		}
		JobConsolCostAttribCollection attributes;

		#endregion

		#endregion

		#region Properties

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AgentDeclaredOSAmount { get; set; }

		public int ExchangeRateDecimalPlaces => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		public int LocalCurrencyDecimals => Company.GetLocalDecimals();

		public int CurrencyDecimals => Currency != null ? Currency.Decimals : LocalCurrencyDecimals;

		public bool IsPosting { get; set; }

		internal ZGuid RelatedJobPkFromIntercompanyInvoiceImport { get; set; }

		public Charge SellChargeFromSellToCostSynchronisation { get; set; }

		public JobConsolCostCalculationStrategyBase CalculationStrategy
		{
			get
			{
				if (this.HasContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithoutCalculations))
				{
					fCalculationStrategy = new ConsolCostCalculationStrategyWithoutCalculations(this);
				}
				else if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					BusinessObjectCollection collection = ((IBusinessObjectInternals)this).ParentCollections[0];
					if (collection is JobConsolCostCollection || collection is JobConsolCostFilteredCollection)
					{
						if (IsPosting && !this.HasContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting))
						{
							fCalculationStrategy = new ConsolCostCalculationStrategyWithoutCalculations(this);
						}
						else
						{
							fCalculationStrategy = new ConsolCostCalculationStrategy(this);
						}
					}
					else if (collection is APInvoiceConsolCostCollection)
					{
						fCalculationStrategy = new InvoicingBaseConsolCostCalculationStrategy(this);
					}
					else if (collection is InvoicingBaseConsolCostCollectionForImporting)
					{
						fCalculationStrategy = new ConsolCostCalculationStrategyWithoutCalculations(this);
					}
				}

				if (fCalculationStrategy == null)
				{
					fCalculationStrategy = new ConsolCostCalculationStrategyWithoutCalculations(this);
				}
				return fCalculationStrategy;
			}
		}

		JobConsolCostCalculationStrategyBase fCalculationStrategy;

		public void SetCalculationDescription(AutoRateInfo rateInfo)
		{
			var rateDescription = rateInfo.Description;
			var blobDescription = ZBlob.FromUTF8(rateDescription);
			CostCalculationDescription = blobDescription;

			if (!string.IsNullOrEmpty(rateDescription))
			{
				var combinedDescriptionBuilder = new StringBuilder();
				combinedDescriptionBuilder.Append(Res.GetString(
						"abae164d-43c0-4005-808f-d85eb0e8004f",
						"This cost was autocosted and apportioned from Consol {0}. Details listed are Consol details.",
						Consol?.JK_UniqueConsignRef ?? ZString.Empty));
				combinedDescriptionBuilder.Append(System.Environment.NewLine);
				combinedDescriptionBuilder.Append(System.Environment.NewLine);
				combinedDescriptionBuilder.Append(rateDescription);
				blobDescription = ZBlob.FromUTF8(combinedDescriptionBuilder.ToString());
			}

			foreach (var charge in ApportionmentCharges.Cast<ApportionSplitCharge>())
			{
				charge.CostCalculationDescription = blobDescription;
			}
		}

		void EmptyCostCalculationDescription()
		{
			if (!CostCalculationDescription.IsEmpty)
			{
				CostCalculationDescription = ZBlob.FromUTF8(Res.GetString("d5e1ea9b-7f95-46c3-9246-0adec2caf0da", "Cost was autocosted but cost amount was subsequently changed."));
				ZString consolNumber = Consol != null ? Consol.JK_UniqueConsignRef : ZString.Empty;
				ZBlob blob = ZBlob.FromUTF8(Res.GetString("429c60ba-6551-4b00-8ec7-8dd7804eb304", "Cost was autocosted but cost amount was subsequently changed from Consol {0}.", consolNumber));
				foreach (ApportionSplitCharge charge in ApportionmentCharges)
				{
					charge.CostCalculationDescription = blob;
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("F24F5449-F6AB-4B5A-B0AA-37278CE634D1", Caption = "Description")]
		public ZBlob CostCalculationDescription
		{
			get { return GetCalculationStmNote(CostCalculationNote); }
			set
			{
				if (ChargeCode != null)
				{
					costCalculationNote = this.CreateOrUpdateCostCalculationNote(CostCalculationNote, value);
				}
				CostCalculationDescriptionInfo.RefreshBinding();
			}
		}

		public ZString CostCalculationDescriptionString
		{
			get
			{
				var description = CostCalculationDescription;
				return ORtfTextUtil.IsRtf(description) ? ORtfTextUtil.RtfToText(description) : description.ToUTF8();
			}
		}

		public ZBlob CostCalculationDescription_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(CostCalculationDescription);
			}
			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				CostCalculationDescription = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public ZPropertyInfo CostCalculationDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CostCalculationDescription)); }
		}

		ZBlob GetCalculationStmNote(StmNote note)
		{
			if (ChargeCode == null || note == null)
			{
				return ZBlob.Empty;
			}

			return note.ST_NoteData;
		}

		StmNote CostCalculationNote => costCalculationNote ?? (costCalculationNote = this.GetCostCalculationNote());
		StmNote costCalculationNote;

		public ZBool IsChequeNumberAutoAllocated
		{
			get
			{
				if (IsCheque && ChequeBook != null)
				{
					return ChequeBook.IsAutoPrint;
				}
				else
				{
					return ZBool.False;
				}
			}
		}

		public ZString Calc_ChequeNumberIsAutoAllocatedLabel
		{
			get { return IsChequeNumberAutoAllocated && E6_ChequeOrReference.IsEmpty ? AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel : ""; }
		}

		public ZPropertyInfo Calc_ChequeNumberIsAutoAllocatedLabelInfo
		{
			get { return GetZPropertyInfo(nameof(Calc_ChequeNumberIsAutoAllocatedLabel)); }
		}

		public override AccTransactionHeader APInvoice
		{
			get
			{
				var result = base.APInvoice;
				if (result != null
					&& result.AH_Ledger == LedgerTypes.JobCosting
					&& result.AH_TransactionType == TransactionTypes.JobRevenueJournal)
				{
					result = null;
				}
				return result;
			}
		}

		APInvoice ConcreteAPInvoice
		{
			get
			{
				if (APInvoice != null && APInvoice.IsAPInvoice)
				{
					return Factory.Load<APInvoice>(E6_AH_APInvoice);
				}
				return null;
			}
		}

		IEnumerable<APInvoiceLine> APInvoiceLines
		{
			get
			{
				return ConcreteAPInvoice?.Lines.Cast<APInvoiceLine>()
						.Where(x => !x.ConsolIDFromApportionedCharge.IsEmpty && x.GetConsolID().Item1 == Consol.PK);
			}
		}

		/// <summary>
		/// In addition to APInvoice, AutoJRJ may be linked here. See ChargeWithCost::CreateAutoJobRevenueJournal()
		/// </summary>
		public override ZGuid E6_AH_APInvoice
		{
			get { return base.E6_AH_APInvoice; }
			set
			{
				ZGuid oldValue = base.E6_AH_APInvoice;
				base.E6_AH_APInvoice = value;
				if (oldValue != value)
				{
					if (value.IsEmpty)
					{
						E6_IsTaxAmountOverridden = false;
						E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge;
					}
					else
					{
						if (IsPosted)
						{
							E6_RatingBehaviour = RatingBehaviours.StopFromAutorating;
						}
						if (!E6_IsTaxAmountOverridden)
						{
							E6_IsTaxAmountOverridden = true;
						}
					}

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost,
() => Invariant(
$@"Original AP invoice '{oldValue}' New AP invoice '{value}'.
Call stack:
{new StackTrace().ToString()}"));
				}
			}
		}

		public override ZBool E6_IsTaxAmountOverridden
		{
			get { return base.E6_IsTaxAmountOverridden; }
			set
			{
				if (base.E6_IsTaxAmountOverridden != value)
				{
					base.E6_IsTaxAmountOverridden = value;
					ApportionmentCharges.Cast<ApportionSplitCharge>().ForEach(x => x.JR_IsCostTaxAmountOverridden = value);
					GSTCalculationStrategy.ResetGSTAmount();
				}
			}
		}

		protected override JobConsolCostValidation GetNewValidation()
		{
			JobConsolCostValidation result = null;
			if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
			{
				BusinessObjectCollection collection = ((IBusinessObjectInternals)this).ParentCollections[0];
				if (collection is JobConsolCostCollection && (!IsPosted || IsApprovingPosting))
				{
					result = new ForwardingConsolCostingValidation(this, (JobConsolCostCollection)collection);
				}
				else if (collection is JobConsolCostFilteredCollection && (!IsPosted || IsApprovingPosting))
				{
					result = new ForwardingConsolCostingValidation(this, (JobConsolCostCollection)((JobConsolCostFilteredCollection)collection).CollectionToFilter);
				}
				else if (collection is APInvoiceConsolCostCollection)
				{
					result = new APInvoiceConsolCostValidation(this);
				}
				else
				{
					result = new EmptyConsolCostValidation(this);
				}
			}

			if (result == null)
			{
				result = new EmptyConsolCostValidation(this);
			}
			return result;
		}

#if DEBUG
		public JobConsolCostValidation GetNewValidation_ForTestOnly()
		{
			return GetNewValidation();
		}
#endif

		public void RemoveNonApplicableCharges()
		{
			using (ApportionmentCharges.SuspendListChanged())
			{
				var apportionCharges = ApportionmentCharges.Cast<ApportionSplitCharge>().ToArray();

				foreach (var charge in apportionCharges)
				{
					if (charge.JR_OSCostAmt == 0m)
					{
						RemoveNonApplicableChargeSafe(charge);
					}
				}
			}
		}

		internal void RemoveNonApplicableChargeSafe(ApportionSplitCharge apportionSplitCharge)
		{
			if (apportionSplitCharge.IsInDatabase && apportionSplitCharge.JR_OSSellAmt != 0m)
			{
				apportionSplitCharge.ClearCostSide();
				ApportionmentCharges.Remove(apportionSplitCharge);
			}
			else
			{
#if DEBUG
				var shouldSkipDeletingForTest = Globals.IsTest && IsForceToSkipDeletingApportionSplitCharge_ForTestOnly;
				if (!shouldSkipDeletingForTest)
				{
#endif
					var charge = Factory.Load<Charge>(apportionSplitCharge.PK);
					charge.BeforeSuccessfulDeleting?.Invoke(charge, EventArgs.Empty);
					ApportionmentCharges.RemoveAndDelete(apportionSplitCharge);
#if DEBUG
				}
#endif
				if (!apportionSplitCharge.IsDeleted)
				{
					ErrorReporter.ReportOnce("NonApplicableApportionSplitChargeNotDeleted", apportionSplitCharge.GetJobChargeInfo());
				}
			}
		}

		public bool E6_Calc_IncludeOnAgentInvoice
		{
			get { return IsCreditorOverseasAgent && E6_IsForCollectInvoice; }
		}

		public void OverrideIsCreditorOverseasAgentCheck(bool @override)
		{
			fOverriddenIsCreditOverseasAgent = @override;
		}

		bool? fOverriddenIsCreditOverseasAgent;

		internal bool IsCreditorOverseasAgent
		{
			get
			{
				bool result = false;

				if (fOverriddenIsCreditOverseasAgent.HasValue)
				{
					result = fOverriddenIsCreditOverseasAgent.Value;
				}
				else if (Consol != null)
				{
					IJobCostingPlugIn plugIn = Consol;
					if (plugIn.IsLoadPortLocal())
					{
						if (plugIn.ReceivingAgent != null)
						{
							if (E6_OH_Creditor == plugIn.ReceivingAgent.PK)
							{
								result = plugIn.ReceivingAgentAPInvoicingParty.PK == plugIn.ReceivingAgent.PK &&
										 plugIn.ReceivingAgentARInvoicingParty.PK == plugIn.ReceivingAgent.PK;
							}
							else if (E6_OH_Creditor == plugIn.ReceivingAgentAPInvoicingParty.PK)
							{
								result = true;
							}
							else if (E6_OH_Creditor == plugIn.ReceivingAgentARInvoicingParty.PK)
							{
								result = true;
							}
						}
					}
					else if (plugIn.SendingAgent != null)
					{
						if (E6_OH_Creditor == plugIn.SendingAgent.PK)
						{
							result = plugIn.SendingAgentAPInvoicingParty.PK == plugIn.SendingAgent.PK &&
									 plugIn.SendingAgentARInvoicingParty.PK == plugIn.SendingAgent.PK;
						}
						else if (E6_OH_Creditor == plugIn.SendingAgentAPInvoicingParty.PK)
						{
							result = true;
						}
						else if (E6_OH_Creditor == plugIn.SendingAgentARInvoicingParty.PK)
						{
							result = true;
						}
					}
				}

				return result;
			}
		}

		[List("Lookups.Currencies")]
		public ZString LocalCurrency
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		public ZPropertyInfo LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(LocalCurrency)); }
		}

		public ZString MatchedWithTNFJournalNum
		{
			get
			{
				var result = ZString.Empty;
				if (E6_OH_Creditor.IsValid && Creditor != null && !E6_RX_NKCurrency.IsEmpty && (!E6_InvoiceNum.IsEmpty || !E6_CostReference.IsEmpty))
				{
					result = Factory.GetCachedValue(GetCachingKey(), GetMatchedWithTNFJournalNum);
				}
				return result;
			}
		}

		string GetCachingKey()
		{
			return "MatchedTNFJNL:" + Creditor.OH_Code + E6_InvoiceNum + E6_CostReference + E6_RX_NKCurrency;
		}

		ZString GetMatchedWithTNFJournalNum()
		{
			var query = new ZQuery();

			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, E6_GC);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OH, E6_OH_Creditor);
			query.AddToFilter(AccTransactionHeaderSchema.AH_RX_NKTransactionCurrency, E6_RX_NKCurrency);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, Constants.TransactionCategory.Codes.TransactionNotFound);
			query.AddToFilter(AccTransactionHeaderSchema.AH_ChequeOrReference, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.Equal, null);
			query.AddToFilter(AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.GreaterThan, 0);

			var referenceFilter = new ZQuery(AccTransactionHeaderSchema.AH_ChequeOrReference, E6_InvoiceNum);
			referenceFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_ChequeOrReference, E6_CostReference);
			query.AddToFilter(referenceFilter);

			var matchedJournal = Factory.LoadTop1<APJournal>(query);
			return matchedJournal != null ? matchedJournal.AH_TransactionNum : ZString.Empty;
		}

		#region IsFinal

		ZBool fIsFinal;
		[ReadOnlyMember(nameof(IsApprovingPosting))]
		public ZBool IsFinal
		{
			get { return fIsFinal; }
			set
			{
				fIsFinal = value;
				foreach (ApportionSplitCharge charge in ApportionmentCharges)
				{
					charge.IsFinal = value;
				}
				IsFinalInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsFinal();
				}
			}
		}

		public ZPropertyInfo IsFinalInfo
		{
			get { return GetZPropertyInfo(nameof(IsFinal)); }
		}

		#endregion

		#region IsPosted

		public ZBool IsPosted
		{
			get { return !IsDeleted && E6_AH_APInvoice.IsValid; }
		}

		public ZPropertyInfo IsPostedInfo
		{
			get { return GetZPropertyInfo(nameof(IsPosted)); }
		}

		public bool IsApprovingPosting
		{
			get { return ParentAPInvoice != null && ParentAPInvoice.IsInvoiceApproving; }
		}

		public ZBool IsRevenuePosted
		{
			get { return ApportionmentCharges.Cast<ApportionSplitCharge>().Any(charge => charge.IsRevenuePosted); }
		}
		#endregion

		#region IsApproved

		public ZBool IsApproved
		{
			get
			{
				return APInvoice != null
					&& (APInvoice.AH_TransactionType == TransactionTypes.Invoice || APInvoice.AH_TransactionType == TransactionTypes.CreditNote);
			}
		}

		public ZPropertyInfo IsApprovedInfo
		{
			get { return GetZPropertyInfo(Schema.IsApproved); }
		}

		#endregion

		[List("Lookups.ChargeCodes")]
		public ZGuid E6_AC_ChargeCodeReadOnly
		{
			get { return E6_AC_ChargeCode; }
		}

		public ZPropertyInfo E6_AC_ChargeCodeReadOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(E6_AC_ChargeCodeReadOnly)); }
		}

		[List("Lookups.ChargeCodes")]
		[ReadOnlyMember(nameof(IsChargeCodeReadOnly))]
		public override ZGuid E6_AC_ChargeCode
		{
			get { return base.E6_AC_ChargeCode; }
			set
			{
				ZGuid previousValue = base.E6_AC_ChargeCode;
				base.E6_AC_ChargeCode = value;
				if (previousValue != value && value.IsValid)
				{
					SetRelatedPropertiesForNewChargeCode(value, previousValue);
				}
			}
		}

		void SetRelatedPropertiesForNewChargeCode(ZGuid newChargeCodePk, ZGuid oldChargeCodePk, bool setCreditor = true)
		{
			if (ApportionmentCharges.OfType<ApportionSplitCharge>().Any(charge => charge.IsRevenuePosted))
			{
				return;
			}

			if (newChargeCodePk.IsValid && E6_RatingBehaviour == RatingBehaviours.Default)
			{
				E6_RatingBehaviour = RatingBehaviours.CreateNewCharge;
			}

			foreach (ApportionSplitCharge charge in ApportionmentCharges)
			{
				if (ImportCostSuspensionCounter > 0)
				{
					using (charge.GetSuspenderForConsolCostImporter())
					{
						charge.JR_AC = newChargeCodePk;
					}
				}
				else
				{
					charge.JR_AC = newChargeCodePk;

					//Setting all tax related values here looks strange as they should be set in related consol cost setter anyway.
					//We add tax date here only because this code was here before. Ideally it should be just removed if no reason of its existence found.
					charge.JR_AT_CostGSTRate = E6_AT_TaxRate;
					charge.SetCostTaxDateSafe(E6_TaxDate);
					charge.JR_A9_CostVATClass = E6_A9_VATClass;
				}
			}

			UpdateGovtChargeCode();
			UpdateApportionmentMethod();
			TryToDefaultSupplyType();
			TryToDefaultCostPlaceOfSupply();
			SetGSTRateAndTaxMessage();
			if (setCreditor)
			{
				SetCreditor();
				SetWHTRate();
			}

			//Set Container Service Apportion Method
			if (newChargeCodePk.IsValid && (!this.IsInDatabase || (this.IsInDatabase && oldChargeCodePk.IsValid)) && IsContainerService)
			{
				E6_PPDCLT = PrepaidCollectList.Codes.CTS;
				SetIsUsedForApportionment();
			}
			else
			{
				E6_PPDCLT = (E6_PPDCLT == PrepaidCollectList.Codes.CTS) ? (ZString)"ALL" : E6_PPDCLT;
			}

			if (oldChargeCodePk.IsEmpty && newChargeCodePk.IsValid && E6_ParentID.IsValid && (ImportCostSuspensionCounter == 0 && ConvertTransactionSuspensionCounter == 0))
			{
				CalculationStrategy.OnE6_ParentIDConsolSet();
			}
		}

		public void UpdateApportionmentMethod()
		{
			if (Consol == null || ChargeCode == null)
			{
				return;
			}

			E6_ApportionmentMethod = Consol.GetApportionmentMethod(ChargeCode);
		}

		#region Government Charge Code

		public void UpdateGovtChargeCode()
		{
			if (!IsValidForGovtChargeCode)
			{
				return;
			}

			e6_CostGovtChargeCode = GetMatchedGovtChargeCode(CostSell.Cost);
			e6_SellGovtChargeCode = GetMatchedGovtChargeCode(CostSell.Revenue);

			foreach (ApportionSplitCharge charge in ApportionmentCharges)
			{
				charge.JR_CostGovtChargeCode = e6_CostGovtChargeCode;
				charge.JR_SellGovtChargeCode = e6_SellGovtChargeCode;
			}
		}

		public ZString GetMatchedGovtChargeCode(CostSell costOrSell)
		{
			if (!IsValidForGovtChargeCode)
			{
				return ZString.Empty;
			}

			var result = Consol != null
				? ChargeCode?.GetFallbackGovtChargeCode(Consol.GetConfigurationMatcherParameters(costOrSell))
				: ChargeCode?.AC_GovtChargeCode;

			return result ?? ZString.Empty;
		}

		public bool IsValidForGovtChargeCode => !E6_AC_ChargeCode.IsEmpty && E6_AC_ChargeCode.IsValid && AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value;

		#endregion

		[SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public ZString ChargeCodeDescription
		{
			get
			{
				return ChargeCode != null ? ChargeCode.AC_Desc : ZString.Empty;
			}
		}

		public ZPropertyInfo ChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodeDescription); }
		}
		public ZString ChargeGroup
		{
			get { return ChargeCode?.AC_ChargeGroup ?? ZString.Empty; }
		}

		public ZPropertyInfo ChargeGroupInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeGroup); }
		}

		public ZString ChargeCodeSubGroup
		{
			get { return ChargeCode?.AC_ChargeSubGroup ?? ZString.Empty; }
		}

		public ZPropertyInfo ChargeCodeSubGroupInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodeSubGroup); }
		}

		bool IsChargeCodeReadOnly
		{
			get { return IsImportedConsolCost || IsApprovingPosting || IsGatewayConsolCost; }
		}

		public bool IsGatewayConsolCost
		{
			get
			{
				return Consol is IGateway && ApportionmentCharges
					.Cast<BaseCharge>()
					.Any(x => x.JR_E6_GatewaySellHeader == PK);
			}
		}

		/// <summary>
		/// Set the charge code and creditor for a new autorated instance.
		/// The given rate provider will be used as the creditor if:
		/// - there is no creditor override on the charge code
		/// - the default creditor from the consol is not a carrier agent of the provider
		/// - the charge is penalty
		/// </summary>
		internal void SetChargeCodeAndCreditorForNewAutoRated(AccChargeCode chargeCode, ZGuid providerOrgPk)
		{
			Argument.NotNull(chargeCode, nameof(chargeCode));

			// calling base setter to avoid the overridden setter updating E6_OH_Creditor
			base.E6_AC_ChargeCode = chargeCode.PK;

			var consol = Consol;
			if (consol != null
				&& !SetCreditorFromOverrideIfExists(consol, chargeCode))
			{
				var creditorPk = ZGuid.Empty;

				if (IsPenaltyCharge(chargeCode))
				{
					if (providerOrgPk.IsValid)
					{
						creditorPk = providerOrgPk;
					}
				}
				else
				{
					creditorPk = CalculateCreditorPkFromCostSupporter(consol, chargeCode, providerOrgPk);

					if (!creditorPk.IsValid && providerOrgPk.IsValid)
					{
						creditorPk = providerOrgPk;
					}
				}

				if (!creditorPk.IsEmpty)
				{
					E6_OH_Creditor = creditorPk;
				}
			}

			SetRelatedPropertiesForNewChargeCode(chargeCode.PK, ZGuid.Empty, setCreditor: false);
		}

		bool SetCreditorFromOverrideIfExists(IJobCostingPlugIn consol, AccChargeCode chargeCode)
		{
			var costSupporter = consol.CostSupporter;
			var creditorOverride = chargeCode.CreditorOverrides.GetCreditorOverride(
									GetConsumerType(consol),
									costSupporter.Direction,
									costSupporter.TransportMode,
									paymentTerm: consol.IsMasterCollect ? Constants.PaymentType.Collect : Constants.PaymentType.Prepaid,
									department: ZGuid.Empty,
									overseasAgentIsApplicable: true,
									overseasCreditor: consol.AgentToInvoice()?.PK);
			if (creditorOverride != null)
			{
				E6_OH_Creditor = creditorOverride.Value;
				return true;
			}

			return false;
		}

		bool IsPenaltyCharge(AccChargeCode chargeCode)
		{
			if (chargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.ContainerDetention
				|| chargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.Storage
				|| chargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.CarrierStorage
				|| chargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.CartageDemurrageTotal
				|| chargeCode.AC_ChargeSubGroup == ChargeCodeSubGroupList.MergedDemurrageDetention)
			{
				return true;
			}

			return false;
		}

		void SetCreditor()
		{
			IJobCostingPlugIn consol;
			AccChargeCode chargeCode;

			if (E6_OH_Creditor.IsValid ||
				(consol = Consol) == null ||
				(chargeCode = ChargeCode) == null)
			{
				return;
			}

			if (!SetCreditorFromOverrideIfExists(consol, chargeCode))
			{
				SetCreditorFromCostSupporter(consol, chargeCode);
			}
		}

		void SetCreditorFromCostSupporter(IJobCostingPlugIn consol, AccChargeCode chargeCode)
		{
			var pk = CalculateCreditorPkFromCostSupporter(consol, chargeCode, ZGuid.Empty);
			if (!pk.IsEmpty)
			{
				E6_OH_Creditor = pk;
			}
		}

		/// <param name="rateProviderOrgPK">when it is ZGuid.Empty, it is a manually entered charge or a charge in Standard Costing, otherwise it was created due to autorating.</param>
		static ZGuid CalculateCreditorPkFromCostSupporter(IJobCostingPlugIn consol, AccChargeCode chargeCode, ZGuid rateProviderOrgPK)
		{
			return chargeCode.AC_IsGroupageCharge
				? consol.CostSupporter.GetCreditorPK(chargeCode.AC_ChargeGroup, rateProviderOrgPK)
				: ZGuid.Empty;
		}

		JobInvoicingConsumerType GetConsumerType(IJobCostingPlugIn consol)
		{
			if (consol is IGateway gatewayConsol && (gatewayConsol.GatewayBillingSupporter?.IsGatewayBillingEnabled() ?? false))
			{
				return IsGatewayConsolCost ? JobInvoicingConsumerTypes.GatewayConsol : JobInvoicingConsumerTypes.ForwardingConsol;
			}
			return (consol as IJobInvoicingPlugIn)?.InvoicingSupporter?.ConsumerType;
		}

		void SetWHTRate()
		{
			if (IsCostWHTApplicable && ChargeCode != null)
			{
				E6_AW = ChargeCode.AC_AW_WithholdingTaxRate;
			}
			else
			{
				E6_AW = ZGuid.Empty;
			}
		}

		public IDisposable GetSuspenderForUnapprovedTransactonConverter()
		{
			return new SuspenderForUnapprovedTransactonConverter(this);
		}

		int ConvertTransactionSuspensionCounter;

		class SuspenderForUnapprovedTransactonConverter : IDisposable
		{
			public SuspenderForUnapprovedTransactonConverter(JobConsolCost consolCost)
			{
				this.consolCost = consolCost;
				consolCost.ConvertTransactionSuspensionCounter++;
			}

			readonly JobConsolCost consolCost;

			public void Dispose()
			{
				consolCost.ConvertTransactionSuspensionCounter--;
			}
		}

		public IDisposable GetSuspenderForConsolCostImporter()
		{
			return new SuspenderForConsolCostImporter(this);
		}

		int ImportCostSuspensionCounter;

		class SuspenderForConsolCostImporter : IDisposable
		{
			public SuspenderForConsolCostImporter(JobConsolCost consolCost)
			{
				this.consolCost = consolCost;
				consolCost.ImportCostSuspensionCounter++;
			}

			readonly JobConsolCost consolCost;

			public void Dispose()
			{
				consolCost.ImportCostSuspensionCounter--;
			}
		}

		public void SetGSTRateAndTaxMessage()
		{
			SetTaxRateAndMessage(false);
		}

		ZGuid GetCostGSTId(out ZGuid overrideInvTaxMsg)
		{
			ZGuid result = ZGuid.Empty;
			overrideInvTaxMsg = ZGuid.Empty;

			if (Creditor != null && IsCostGSTApplicable && ChargeCode != null)
			{
				if (Consol != null)
				{
					var parameters = Consol.GetTaxCalculationParameters();
					parameters.Organisation = Creditor;
					parameters.FixedPlaceOfSupply = PlaceOfSupplyLocation;
					parameters.SupplyType = E6_SupplyType;

					if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
					{
						parameters.Branch = Factory.Load<GlbBranch>(E6_GB_CostTaxBranch);
					}

					AccTaxRate taxRate = ChargeCode.GetGSTRate(parameters, out overrideInvTaxMsg);
					if (taxRate != null)
					{
						result = taxRate.PK;
					}
				}
				if (!result.IsValid)
				{
					result = ChargeCode.AC_AT_GSTRate;
				}
			}

			return result;
		}

		internal bool IsCostGSTApplicable
		{
			get { return E6_OH_Creditor.IsValid && IsCostAccountGSTRegistered && GlbCompany.CurrentCompany.GC_IsGSTRegistered; }
		}

		bool IsCostAccountGSTRegistered
		{
			get
			{
				if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled())
				{
					if (Creditor == null || Creditor.CompanyData == null)
					{
						return ZBool.False;
					}

					if (Creditor.IsProxyOrg(Company) &&
						(!AutoJRJRegistryStatusHelper.IsAutoJRJWithTaxRegistrationNumberEnabled() ||
						ApportionmentCharges.Cast<ApportionSplitCharge>().Any(x => x.JR_IsUsedForApportionment && !x.IsExcludedFromAutoJRJ(Creditor))))
					{
						return ZBool.False;
					}

					return Creditor.CompanyData.IsAPTaxApplicable;
				}
				else
				{
					return (Creditor != null && Creditor.CompanyData != null) ? Creditor.CompanyData.IsAPTaxApplicable : ZBool.False;
				}
			}
		}

		[List("Lookups.BankAccounts")]
		[ReadOnlyMember(nameof(IsGatewayConsolCost))]
		public override ZGuid E6_AB_BankAccount
		{
			get { return base.E6_AB_BankAccount; }
			set
			{
				base.E6_AB_BankAccount = value;
				foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
				{
					splitCharge.JR_AB = value;
				}

				if (BankAccount?.IsCashAccount ?? false)
				{
					E6_PaymentType = ReceiptTypes.Cash;
				}
			}
		}

		[List("Lookups.ChequeBooks")]
		public override ZGuid E6_AK_ChequeBook
		{
			get { return base.E6_AK_ChequeBook; }
			set
			{
				base.E6_AK_ChequeBook = value;
				CalculationStrategy.OnE6_AK_ChequeBookSet();
				foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
				{
					splitCharge.JR_AK = value;
				}
				CheckNumberIsAutoAllocated();
			}
		}

		public bool E6_AK_ChequeBook_ReadOnly
		{
			get { return !IsCheque || IsGatewayConsolCost; }
		}

		[ReadOnlyMember(nameof(E6_InvoiceNum_ReadOnly))]
		public override ZString E6_InvoiceNum
		{
			get { return base.E6_InvoiceNum; }
			set
			{
				base.E6_InvoiceNum = value;
				foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
				{
					splitCharge.JR_APInvoiceNum = value;
				}
			}
		}

		protected virtual bool E6_InvoiceNum_ReadOnly
		{
			get { return (Creditor != null && Creditor.CompanyData.OB_APCostsSelfBilled) || IsGatewayConsolCost; }
		}

		public override ZString E6_CostReference
		{
			get { return base.E6_CostReference; }
			set
			{
				base.E6_CostReference = value;
				foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
				{
					splitCharge.JR_CostReference = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobConsolCostLookups.PlacesOfSupply))]
		[ResourceStringData("af39b151-db82-456b-8ca6-1ec0d6e3909c", ShortCaption = "FPOS", Caption = "Fixed Place of Supply")]
		public override ZString E6_PlaceOfSupply
		{
			get { return base.E6_PlaceOfSupply; }
			set
			{
				if (E6_PlaceOfSupply != value)
				{
					base.E6_PlaceOfSupply = value;

					foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
					{
						using (splitCharge.SuspendCostTaxCalulation(false))
						{
							splitCharge.JR_CostPlaceOfSupply = value;
						}
					}

					E6_PlaceOfSupplyType = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(Company ?? GlbCompany.CurrentCompany, E6_PlaceOfSupply);
					SetGSTRateAndTaxMessage();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobConsolCostLookups.PlaceOfSupplyTypes))]
		public override ZString E6_PlaceOfSupplyType
		{
			get { return base.E6_PlaceOfSupplyType; }
			set
			{
				if (E6_PlaceOfSupplyType != value)
				{
					base.E6_PlaceOfSupplyType = value;
					foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
					{
						splitCharge.JR_CostPlaceOfSupplyType = value;
					}

					Validation.ValidateE6_PlaceOfSupply();
				}
			}
		}

		public ILocation PlaceOfSupplyLocation => PlaceOfSupplyHelper.TryConvertToLocation(Company, E6_PlaceOfSupply);

		bool TryToDefaultCostPlaceOfSupply()
		{
			var result = false;

			if (!IsPosted)
			{
				(var posType, var posCode) = GetDefaultPlaceOfSupply();
				if (E6_PlaceOfSupply != posCode)
				{
					if (!posType.IsEmpty)
					{
						E6_PlaceOfSupply = posCode;
					}
					else if (posCode.IsEmpty)
					{
						E6_PlaceOfSupply = posCode;
					}
					result = true;
				}
			}

			return result;
		}

		void TryToDefaultSupplyType()
		{
			E6_SupplyType = Consol.GetSupplyType(ChargeCode);
		}

		(ZString posType, ZString posCode) GetDefaultPlaceOfSupply()
		{
			var result = (posType: ZString.Empty, posCode: ZString.Empty);

			if (Consol != null && ChargeCode != null && Creditor != null)
			{
				result = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(Consol, ChargeCode, Creditor, E6_SupplyType);
			}

			return result;
		}

		[List("Lookups.TaxRates")]
		[ReadOnlyMember(nameof(IsTaxRateReadOnly))]
		public override ZGuid E6_AT_TaxRate
		{
			get { return base.E6_AT_TaxRate; }
			set
			{
				bool hasChanged = value != base.E6_AT_TaxRate;
				base.E6_AT_TaxRate = value;
				if (hasChanged)
				{
					var suspendList = ApportionmentCharges.OfType<ApportionSplitCharge>()
						.SelectMany(splitCharge => new IDisposable[] {
							ImportCostSuspensionCounter != 0 ? null : splitCharge.ApportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender.GetSuspender(),
							splitCharge.SuspendCostTaxCalulation(true)
						}.WhereNotNull())
						.ToArray();
					using (var delayedListForCheckingDatesMismatch = new DisposableList(suspendList))
					{
						if (!E6_AT_TaxRate.IsValid)
						{
							E6_TaxDate = ZDate.Empty;
						}
					}

					CalculateTax();
					SetTaxRateAndMessage(true);

					if (!IsValidationSuspended)
					{
						Validation.ValidateE6_CostGovtChargeCode();
						Validation.ValidateE6_SellGovtChargeCode();
						Validation.ValidateE6_A9_VATClass();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(IsTaxRateReadOnly))]
		public override ZDate E6_TaxDate
		{
			get => base.E6_TaxDate;
			set
			{
				var hasChanged = value != base.E6_TaxDate;
				base.E6_TaxDate = value;
				if (hasChanged)
				{
					foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
					{
						using (TaxRecalculationSuspender.IsSuspended ? splitCharge.CostTaxRecalculationSuspender.GetSuspender() : DisposableAction.NoAction)
						{
							splitCharge.JR_CostTaxDate = value;
						}
					}
					CalculateTax();
				}
			}
		}

		#region Tax Branch

		[ReadOnlyMember(nameof(IsTaxBranchReadOnly))]
		public override ZGuid E6_GB_CostTaxBranch
		{
			get { return base.E6_GB_CostTaxBranch; }
			set
			{
				if (E6_GB_CostTaxBranch != value)
				{
					using (GetValidationSuspender())
					{
						base.E6_GB_CostTaxBranch = value;

						if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
						{
							SetGSTRateAndTaxMessage();

							foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
							{
								splitCharge.JR_GB_CostTaxBranch = value;
							}
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateE6_GB_CostTaxBranch();
					}
				}
			}
		}

		bool IsTaxBranchReadOnly
		{
			get
			{
				return IsPosted
					|| !IsCostGSTApplicable
					|| !Env.Security.MaintainConsolJobInvoicingAllowOverrideCostTaxBranch.IsAllowedWithConstraint()
					|| !AccountingMasterFilesUtils.IsTaxBranchApplicable;
			}
		}

		public void ResetCostTaxInfo()
		{
			if (!IsCostTaxBranchActual)
			{
				SetTaxBranchDefault();
			}

			if (!IsCostGSTRateActual)
			{
				SetGSTRateAndTaxMessage();
			}
		}

		public bool IsCostGSTRateActual => ChargeCode != null && ChargeCode.IsComment || IsCostGSTApplicable == (TaxRate != null);

		public bool IsCostTaxBranchActual => ChargeCode != null && ChargeCode.IsComment || IsCostTaxBranchApplicable == (CostTaxBranch != null);

		void SetTaxBranchDefault()
		{
			E6_GB_CostTaxBranch = AccountingMasterFilesUtils.GetTaxBranchResetValue(IsCostGSTApplicable);
		}

		bool IsCostTaxBranchApplicable => AccountingMasterFilesUtils.IsTaxBranchApplicable && IsCostGSTApplicable;

		#region CostTaxBranchName

		public ZString CostTaxBranchName
		{
			get { return CostTaxBranch != null ? CostTaxBranch.GB_BranchName : ZString.Empty; }
		}

		public ZPropertyInfo CostTaxBranchNameInfo
		{
			get { return GetZPropertyInfo(Schema.CostTaxBranchName); }
		}

		#endregion

		#endregion

		public void UpdateCostTaxDateBasedOnRegistry()
		{
			if (!E6_AT_TaxRate.IsValid || !E6_ParentID.IsValid)
			{
				E6_TaxDate = ZDate.Empty;
			}
			else if (E6_TaxDate.IsEmpty)
			{
				var costSupporter = Consol?.CostSupporter;
				if (costSupporter != null)
				{
					var taxDateOption = AccountingUtils.GetTaxDateDefaultingOptionForCostSupporter(costSupporter, LedgerTypes.AccountsPayable, JobInvoicingConsumerTypes.ForwardingConsolCode);
					if (taxDateOption != null)
					{
						if (taxDateOption.TaxDateOption == TaxDateDefaultingOption.Code.InvoiceDate)
						{
							E6_TaxDate = ParentAPInvoice?.AH_InvoiceDate.Date ?? ZDate.Empty;
						}
						else if (taxDateOption.TaxDateOption == TaxDateDefaultingOption.Code.Today)
						{
							E6_TaxDate = ZDate.Today;
						}
					}
				}
			}
		}

		internal FunctionalitySuspender TaxRecalculationSuspender => taxRecalculationSuspender ?? (taxRecalculationSuspender = new FunctionalitySuspender());
		FunctionalitySuspender taxRecalculationSuspender;

		public void SetTaxDateSafe(ZDate value) => E6_TaxDate = (E6_AT_TaxRate.IsValid ? value : ZDate.Empty);

		void CalculateTax()
		{
			if (TaxRecalculationSuspender.IsSuspended)
			{
				return;
			}

			E6_OSCostAmount = GSTCalculationStrategy.GetOSCostAmountFromGSTInclusiveAmount(GSTInclusiveAmount);
			GSTCalculationStrategy.ResetGSTAmountOverriddenFlag();
			if (E6_IsTaxAmountOverridden && IsGSTInclusiveAmount)
			{
				E6_OSGSTAmount_Calc = GSTInclusiveAmount - E6_OSCostAmount;
			}
			else
			{
				GSTCalculationStrategy.ResetGSTAmount();
			}
		}

		internal bool IsTaxRateReadOnly
		{
			get
			{
				return !IsCostGSTApplicable
					|| ShouldBeReadOnlyWhenPosted
					|| IsGatewayConsolCost
					|| !AccountingUtils.IsUserCanChangeGST(LedgerTypes.AccountsPayable)
					|| !Env.Security.MaintainConsolJobInvoicingAllowOverrideCostTaxId.IsAllowed;
			}
		}

		[List("Lookups.VATClasses")]
		[ReadOnlyMember(nameof(IsVATClassReadOnly))]
		public override ZGuid E6_A9_VATClass
		{
			get { return base.E6_A9_VATClass; }
			set
			{
				bool hasChanged = value != base.E6_A9_VATClass;
				base.E6_A9_VATClass = value;
				if (hasChanged)
				{
					foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
					{
						splitCharge.JR_A9_CostVATClass = value;
					}
				}
			}
		}

		bool IsVATClassReadOnly
		{
			get
			{
				return E6_AT_TaxRate == ZGuid.Empty
					|| IsGatewayConsolCost
					|| !Env.Security.MaintainConsolJobInvoicingAllowOverrideCostTaxMsg.IsAllowed
					|| !AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			}
		}

		internal FunctionalitySuspender UpdateE6_A9_VATClassSuspender => updateE6_A9_VATClassSuspender ?? (updateE6_A9_VATClassSuspender = new FunctionalitySuspender());
		FunctionalitySuspender updateE6_A9_VATClassSuspender;

		void SetTaxRateAndMessage(bool skipRateSetting)
		{
			if (UpdateE6_A9_VATClassSuspender.IsSuspended)
			{
				return;
			}

			ZGuid overrideInvTaxMsg;
			ZGuid overrideTaxRate = GetCostGSTId(out overrideInvTaxMsg);
			if (!skipRateSetting)
			{
				using (UpdateE6_A9_VATClassSuspender.GetSuspender())
				{
					E6_AT_TaxRate = overrideTaxRate;
				}
			}

			if (overrideTaxRate != ZGuid.Empty && overrideTaxRate == E6_AT_TaxRate && overrideInvTaxMsg != ZGuid.Empty)
			{
				E6_A9_VATClass = overrideInvTaxMsg;
			}
			else
			{
				E6_A9_VATClass = (TaxRate != null) ? TaxRate.AT_A9_DefaultVatClass : ZGuid.Empty;
			}
		}

		[ReadOnlyMember(nameof(E6_DocumentReceivedDate_ReadOnly))]
		public override ZDateTime E6_DocumentReceivedDate
		{
			get { return base.E6_DocumentReceivedDate; }
			set
			{
				base.E6_DocumentReceivedDate = value;

				foreach (ApportionSplitCharge aCharge in ApportionmentCharges)
				{
					aCharge.JR_APDocumentReceivedDate = value;
				}
				CalculateDueDate();
			}
		}

		protected virtual bool E6_DocumentReceivedDate_ReadOnly
		{
			get { return (Creditor != null && Creditor.CompanyData.OB_APCostsSelfBilled) || IsGatewayConsolCost; }
		}

		[ReadOnlyMember(nameof(E6_InvoiceDate_ReadOnly))]
		public override ZDateTime E6_InvoiceDate
		{
			get { return base.E6_InvoiceDate; }
			set
			{
				base.E6_InvoiceDate = value;

				SetDefaultDocumentReceivedDate();

				foreach (ApportionSplitCharge aCharge in ApportionmentCharges)
				{
					aCharge.JR_APInvoiceDate = value;
				}
				CalculateDueDate();
			}
		}

		bool E6_InvoiceDate_ReadOnly
		{
			get { return (Creditor != null && Creditor.CompanyData.OB_APCostsSelfBilled) || IsGatewayConsolCost; }
		}

		void SetDefaultDocumentReceivedDate()
		{
			if (E6_DocumentReceivedDate.IsEmpty)
			{
				var defaultLogic = AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.Value;
				if (defaultLogic == AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.CreateDate)
				{
					E6_DocumentReceivedDate = ZDateTime.Now;
				}
				else if (defaultLogic == AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.InvoiceDate)
				{
					E6_DocumentReceivedDate = E6_InvoiceDate;
				}
			}
		}

		void CalculateDueDate()
		{
			if (Creditor != null)
			{
				var calculateDate = DueDateCalculation.GetCalculateDate(Creditor.CompanyData.GetAPTerm(), E6_InvoiceDate, E6_DocumentReceivedDate);
				if (calculateDate.IsValid)
				{
					E6_PaymentDate = new InvoiceAndDueDateCalculator(null, calculateDate, Creditor).DueDate;
				}
			}
		}

		[ReadOnlyMember(nameof(E6_PaymentDate_ReadOnly))]
		public override ZDateTime E6_PaymentDate
		{
			get { return base.E6_PaymentDate; }
			set
			{
				base.E6_PaymentDate = value;
				foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
				{
					splitCharge.JR_PaymentDate = value;
				}
			}
		}

		bool E6_PaymentDate_ReadOnly
		{
			get { return (Creditor != null && Creditor.CompanyData.OB_APCostsSelfBilled) || IsGatewayConsolCost; }
		}

		#region E6_ChequeOrReference

		public override ZString E6_ChequeOrReference
		{
			get { return base.E6_ChequeOrReference; }
			set
			{
				ZString paddedChequeNo = CalculationStrategy.GetPaddedChequeNo(AccValidationHelper, value);
				base.E6_ChequeOrReference = paddedChequeNo;
				CalculationStrategy.OnE6_ChequeOrReferenceSet();
				foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
				{
					splitCharge.JR_ChequeNo = paddedChequeNo;
				}
			}
		}

		public bool E6_ChequeOrReference_ReadOnly
		{
			get { return IsChequeNumberAutoAllocated || IsGatewayConsolCost; }
		}

		AccValidationHelper AccValidationHelper
		{
			get
			{
				if (fAccValidationHelper == null)
				{
					fAccValidationHelper = new AccValidationHelper();
				}
				return fAccValidationHelper;
			}
		}

		AccValidationHelper fAccValidationHelper;

		void CheckNumberIsAutoAllocated()
		{
			Calc_ChequeNumberIsAutoAllocatedLabelInfo.RefreshBinding();
			if (IsChequeNumberAutoAllocated)
			{
				E6_ChequeOrReference = ZString.Empty;
			}
		}

		#endregion

		#region E6_RatingBehaviour

		[List("Lookups.RatingBehaviourList")]
		[ResourceStringData("830135A4-8E89-49CA-9A26-735FB97823D2", Caption = "Rating Behavior")]
		[ReadOnlyMember(nameof(IsGatewayConsolCost))]
		public override ZString E6_RatingBehaviour
		{
			get => IsGatewayConsolCost ? (ZString)RatingBehaviours.Default : base.E6_RatingBehaviour;
			set => base.E6_RatingBehaviour = value;
		}

		#endregion

		#region E6_RX_NKCurrency

		[List("Lookups.Currencies")]
		[ReadOnlyMember(nameof(IsCurrencyReadOnly))]
		public override ZString E6_RX_NKCurrency
		{
			get
			{
				return base.E6_RX_NKCurrency;
			}
			set
			{
				if (E6_RX_NKCurrency != value)
				{
					var oldCurrency = E6_RX_NKCurrency;
					using (AccountingValuesRoundingHelper.GetActionForChangeInDecimalPlaces(this, OSPropertiesRequiringRounding(), oldCurrency, value, E6_OSCostAmountRoundingErrorReproterFunctionalitySuspender))
					{
						base.E6_RX_NKCurrency = value;

						foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
						{
							splitCharge.JR_RX_NKCostCurrency = E6_RX_NKCurrency;
						}

						CalculationStrategy.HandleCurrencyChanged();

						if (Currency != null)
						{
							SetGSTInclusiveAmountCore(GSTInclusiveAmount, false);
						}

						UntickSurplusApportionmentChargesWhenLocalAmountIsTooSmall();

						if (!IsValidationSuspended)
						{
							Validation.ValidateE6_RX_NKCurrency();
							Validation.ValidateE6_InvoiceDate();
						}

						foreach (ApportionSplitCharge charge in ApportionmentCharges)
						{
							AccountingValuesRoundingHelper.ReportErrorIfPropertiesNotRounded(charge, AdditionalOSPropertiesRequiringRoundingApportionSplitCharge().Append(charge.CostOSPropertiesRequiringRounding()), oldCurrency, E6_RX_NKCurrency, E6_OSCostAmountRoundingErrorReproterFunctionalitySuspender);
						}
					}
				}
			}
		}

		internal FunctionalitySuspender E6_OSCostAmountRoundingErrorReproterFunctionalitySuspender
		{
			get
			{
				if (e6_OSCostAmountRoundingErrorReproterFunctionalitySuspender == null)
				{
					e6_OSCostAmountRoundingErrorReproterFunctionalitySuspender = new FunctionalitySuspender(
						() =>
						{
							foreach (ApportionSplitCharge charge in ApportionmentCharges)
							{
								AccountingValuesRoundingHelper.ReportErrorIfPropertiesNotRounded(charge, AdditionalOSPropertiesRequiringRoundingApportionSplitCharge().Append(charge.CostOSPropertiesRequiringRounding()), originalCurrencyForRounding, E6_RX_NKCurrency, null);
							}
							AccountingValuesRoundingHelper.ReportErrorIfPropertiesNotRounded(this, OSPropertiesRequiringRounding(), originalCurrencyForRounding, E6_RX_NKCurrency, null);
						},
						true,
						() => originalCurrencyForRounding = E6_RX_NKCurrency);
				}

				return e6_OSCostAmountRoundingErrorReproterFunctionalitySuspender;
			}
		}

		FunctionalitySuspender e6_OSCostAmountRoundingErrorReproterFunctionalitySuspender;

		ZString originalCurrencyForRounding;

		#endregion

		internal string[] OSPropertiesRequiringRounding() => new[]
		{
			nameof(E6_OSCostAmount),
			nameof(E6_OSGSTAmount),
			nameof(E6_OSGSTAmount_Calc),
			nameof(GSTInclusiveAmount),
			nameof(E6_OSWHTAmount)
		};

		internal string[] AdditionalOSPropertiesRequiringRoundingApportionSplitCharge()
		{
			return new[]
			{
				nameof(ApportionSplitCharge.JR_OSCostAmt),
				nameof(ApportionSplitCharge.JR_OSCostGSTAmt),
				nameof(ApportionSplitCharge.JR_OSCostGSTAmt_Calc),
				nameof(ApportionSplitCharge.JR_OSCostWHTAmt),
			}.ToArray();
		}

		bool IsCurrencyReadOnly
		{
			get { return IsForeignCurrencyParentAPInvoice || IsGatewayConsolCost; }
		}

		[List("Lookups.Currencies")]
		public ZString E6_RX_NKCurrencyReadOnly
		{
			get { return E6_RX_NKCurrency; }
		}

		public virtual ZPropertyInfo E6_RX_NKCurrencyReadOnlyInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(E6_RX_NKCurrencyReadOnly)); }
		}

		internal ZDecimal GetExchangeRateBasedOnJobBillingExchangeRateConfiguration(RefCurrency currency, OrgHeader creditor = null)
		{
			return CalculationStrategy.CalculateExchangeRateBasedOnToJobBillingExchangeRateConfig(currency, creditor);
		}

		public void RedeaultExchangeRate()
		{
			if (!IsPosted)
			{
				E6_ExchangeRate = GetExchangeRateBasedOnJobBillingExchangeRateConfiguration(Currency);
			}
		}

		#region E6_ExchangeRate

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		[ReadOnlyMember(nameof(IsExchangeRateReadOnly))]
		public override ZDecimal E6_ExchangeRate
		{
			get { return base.E6_ExchangeRate; }
			set
			{
				ZBool isChanged = E6_ExchangeRate != value;
				base.E6_ExchangeRate = value;

				SetExchangeRateOnApportionedCharges(isChanged);

				foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
				{
					splitCharge.JR_OSCostExRate = value;
				}
				CalculationStrategy.HandleExchangeRateChanged();
				PushLocalAmountExchangeRateDifferencesToGreatestCharge();
				PushUnApportionedGSTAmountBasedOnRepresentation();

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobConsolCostExchangeRateChangeToZero, () =>
				{
					if ((isChanged || !IsInDatabase) && E6_ExchangeRate.IsEmpty && !E6_RX_NKCurrency.IsEmpty && Factory.NameForDebugging == Enterprise.MasterFiles.Integration.MasterFilesIntegrationConstants.PublishUniversalXmlInternallyFactoryName)
					{
						return Invariant($@"[{E6_RX_NKCurrency}]{JobConsolCostSchema.E6_ExchangeRate.Name} had been changed to zero
{System.Environment.StackTrace}");
					}
					else
					{
						return null;
					}
				});
			}
		}

		void SetExchangeRateOnApportionedCharges(ZBool isChanged)
		{
			if (!isChanged
				|| base.E6_ExchangeRate.IsEmpty
				|| Consol == null
				|| Currency == null
				|| !ApportionmentCharges.Any()
				|| E6_RX_NKCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
				|| E6_AC_ChargeCode != Env.Registry.FreightChargeCode)
			{
				return;
			}

			var exchangeRateConsumer = ApportionmentCharges.Cast<ApportionSplitCharge>().First().InvoicingJob?.ExchangeRateConfigurationRateConsumer;
			if (!AccExchangeRateConfigurationRateFinder.IsPreferenceConsolExchangeRateAtCompanyLevelForJobWithConsolDirectionAndConsolTransportMode(exchangeRateConsumer, ExchangeRateConfigurationRateConsumer, E6_RX_NKCurrency))
			{
				return;
			}

			var costQuery = new ZQuery(JobConsolCostSchema.E6_AC_ChargeCode, Env.Registry.FreightChargeCode);
			costQuery.AddToFilter(JobConsolCostSchema.E6_GC, GlbCompany.CurrentCompany.PK);
			costQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, Consol.CostSupporter.PK);
			costQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, Consol.CostSupporter.Type);
			costQuery.AddToFilter(JobConsolCostSchema.PK, SQLComparisonOperator.NotEqual, PK);
			if (Factory.LoadTop1<JobConsolCost>(costQuery) != null)
			{
				return;
			}

			var parentCollections = ((IBusinessObjectInternals)this).ParentCollections;
			if (parentCollections.Length > 0
				&& parentCollections[0] is JobConsolCostCollection jobConsolCostCollection
				&& jobConsolCostCollection.DoesReplaceShipmentExchangeRate())
			{
				foreach (ApportionSplitCharge charge in ApportionmentCharges)
				{
					if (charge.InvoicingJob != null)
					{
						var ratesCollection = charge.InvoicingJob.ExchangeRates;
						var exchangeRate = ratesCollection.FindByRefCurrency(Currency);
						if (exchangeRate == null)
						{
							exchangeRate = ratesCollection.AddNew();
							exchangeRate.JF_RX_NKRateCurrency = E6_RX_NKCurrency;
						}
						exchangeRate.JF_BaseRate = base.E6_ExchangeRate;
					}
				}
			}
		}

		bool IsExchangeRateReadOnly
		{
			get
			{
				if (ParentAPInvoice != null)
				{
					if (ParentAPInvoice.IsSettingLineExchangeRateSupported)
					{
						var consolCostExchangeRateOverrideSecurity = ParentAPInvoice is APInvoice ?
							Env.Security.AllowAPInvoiceConsolCostExchangeRateOverride :
							Env.Security.AllowAPCreditNoteConsolCostExchangeRateOverride;
						if (!consolCostExchangeRateOverrideSecurity.IsAllowed)
						{
							return true;
						}
					}
					if (IsForeignCurrencyParentAPInvoice && !ParentAPInvoice.AH_PostedToEFT)
					{
						return true;
					}
				}

				return IsGatewayConsolCost;
			}
		}

		#endregion

		ZAccExchangeRate fCostExchangeRate;
		public ZAccExchangeRate CostExchangeRate
		{
			get
			{
				if (fCostExchangeRate == null)
				{
					fCostExchangeRate = new ZAccExchangeRate(this, ExchangeRateType.Buy, E6_ExchangeRateInfo, (ZPropertyInfoString)E6_RX_NKCurrencyInfo, E6_GCInfo);

					fCostExchangeRate.IsRateRequired = true;
					fCostExchangeRate.IsCurrencyRequired = true;
				}
				return fCostExchangeRate;
			}
		}

		[List("Lookups.Creditors")]
		[ReadOnlyMember(nameof(IsGatewayConsolCost))]
		public override ZGuid E6_OH_Creditor
		{
			get { return base.E6_OH_Creditor; }
			set
			{
				var isChanged = (base.E6_OH_Creditor != value);

				if (isChanged)
				{
					base.E6_OH_Creditor = value;

					CalculationStrategy.OnE6_OH_CreditorSet();

					foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
					{
						splitCharge.JR_OH_CostAccount = value;
					}
					CalculationStrategy.HandleCreditorChanged();
					SetTaxBranchDefault();
					SetGSTRateAndTaxMessage();
					SetWHTRate();
					if (Creditor != null && Creditor.CompanyData.OB_APCostsSelfBilled)
					{
						E6_InvoiceNum = ZString.Empty;
						E6_InvoiceDate = ZDateTime.Empty;
						E6_DocumentReceivedDate = ZDateTime.Empty;
						E6_PaymentDate = ZDateTime.Empty;
					}

					Validation.ValidateE6_AC_ChargeCode();
				}
			}
		}

		[List("Lookups.PaymentMethodList")]
		[ReadOnlyMember(nameof(IsGatewayConsolCost))]
		public override ZString E6_PaymentType
		{
			get { return base.E6_PaymentType; }
			set
			{
				base.E6_PaymentType = value;

				if (!IsCheque)
				{
					E6_AK_ChequeBook = ZGuid.Empty;
				}

				foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
				{
					splitCharge.JR_PaymentType = value;
				}
				CheckNumberIsAutoAllocated();
				if (!IsValidationSuspended)
				{
					Validation.ValidateE6_PaymentType();
					Validation.ValidateE6_OH_Creditor();
				}
			}
		}

		bool IsCheque
		{
			get { return E6_PaymentType == ReceiptTypes.Cheque; }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		[ReadOnlyMember(nameof(IsGatewayConsolCost))]
		public override ZDecimal E6_OSCostAmount
		{
			get { return base.E6_OSCostAmount; }
			set
			{
				bool suspendApportionment = E6_ApportionmentMethod == AllocationMethod.Manual && UnApportionedAmount != ZDecimal.Zero;
				var roundedValue = AccountingUtils.Round(value, Currency);
				if (E6_OSCostAmount == 0M && ApportionmentCharges.JR_OSCostAmtSum == 0M)
				{
					SetIsUsedForApportionment();
				}

				if (AccountingValuesRoundingHelper.PropertyHasChanges(this, E6_OSCostAmount != roundedValue))
				{
					var oldValue = E6_OSCostAmount;
					base.E6_OSCostAmount = AccountingUtils.Round(roundedValue, Currency);

					if (!this.HasContext(BusinessContext.SuppressAutoRatingChange) && E6_RatingBehaviour == RatingBehaviours.ReAutorateCharge)
					{
						E6_RatingBehaviour = RatingBehaviours.CreateNewCharge;
					}

					if (PaymentBases.Any())
					{
						PaymentBases.DeleteAll();
					}

					if (!IsAutoratingInProcess)
					{
						EmptyCostCalculationDescription();
					}

					CalculationStrategy.HandleForeignCostAmountChanged();
					if (!suspendApportionment)
					{
						SplitApportionAmount();
						if (!IsValidationSuspended)
						{
							Validation.ValidateE6_LocalCostAmount();
						}
					}
					GSTCalculationStrategy.ResetGSTAmount();
					SetWHTAmount();
					if (!suspendApportionment)
					{
						ApportionGSTCharges();
					}

					if (AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
					{
						MarkAsNeedingValidationIncludingChildren();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateUnApportionedAmount();
					}

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobConsolCostOSCostAmountChanged, () =>
					{
						if (E6_OSCostAmount != oldValue)
						{
							return Invariant($@"{JobConsolCostSchema.E6_OSCostAmount.Name} (Old: {oldValue}, New: {E6_OSCostAmount}), Unapportioned: {UnApportionedAmount}

{System.Environment.StackTrace}");
						}
						return null;
					});
				}
			}
		}

		ZBool IsAutoratingInProcess;

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		[ReadOnlyMember(nameof(IsLocalCostAmountReadOnly))]
		public override ZDecimal E6_LocalCostAmount
		{
			get { return base.E6_LocalCostAmount; }
			set
			{
				var hasChanges = AccountingValuesRoundingHelper.PropertyHasChanges(this, E6_LocalCostAmount != value);
				base.E6_LocalCostAmount = value;

				CalculationStrategy.HandleLocalCostAmountChanged();
				PushLocalAmountExchangeRateDifferencesToGreatestCharge();
				PushUnApportionedGSTAmountBasedOnRepresentation();

				if (hasChanges)
				{
					UntickSurplusApportionmentChargesWhenLocalAmountIsTooSmall();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateE6_LocalCostAmount();
				}

				if (E6_LocalCostAmount.DecimalPlaces > LocalCurrencyDecimals)
				{
					JobConsolCostInvalidLocalCurrencyDecimalsStackTrace = Invariant(@$"
You have entered {E6_LocalCostAmount.DecimalPlaces} decimal places for the local cost amount. The {GlbCompany.CurrentCompany.LocalCurrency.RX_Code} currency only allows entering amounts up to {LocalCurrencyDecimals} decimal places.
{System.Environment.StackTrace}");
				}
			}
		}

		/// <summary>
		/// As we sort by JR_OSCostAmt we need to have initial apportionment done before calling this method.
		/// </summary>
		void UntickSurplusApportionmentChargesWhenLocalAmountIsTooSmall()
		{
			if (!E6_LocalCostAmount.IsEmpty && E6_RX_NKCurrency != GlbCompany.CurrentCompany.LocalCurrency.RX_Code && E6_ApportionmentMethod != AllocationMethod.Manual)
			{
				var usedCharges = ApportionmentCharges.Cast<ApportionSplitCharge>().Where(x => x.JR_IsUsedForApportionment).ToArray();
				var minLocalAmount = LocalCurrencyMinAmount * usedCharges.Length;
				var minLocalAmountSurplus = minLocalAmount - Math.Abs(E6_LocalCostAmount);
				if (minLocalAmountSurplus > 0)
				{
					var numberOfChargesToUntick = (int)(minLocalAmountSurplus / LocalCurrencyMinAmount);
					var chargesToUntick = usedCharges.OrderBy(x => x.JR_OSCostAmt).Take(numberOfChargesToUntick);
					foreach (var charge in chargesToUntick)
					{
						charge.JR_IsUsedForApportionment = false;
					}

					SplitApportionAmount();
				}
			}
		}

		internal string JobConsolCostInvalidLocalCurrencyDecimalsStackTrace { get; private set; }

		bool IsLocalCostAmountReadOnly
		{
			get { return IsForeignCurrencyParentAPInvoice || IsGatewayConsolCost; }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal E6_Calc_LocalExTaxAmount
		{
			get { return E6_LocalCostAmount; }
		}

		public ZPropertyInfo E6_Calc_LocalExTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(E6_Calc_LocalExTaxAmount)); }
		}

		#region E6_OSGSTAmount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		[ReadOnlyMember(nameof(IsOSGSTAmountReadOnly))]
		public override ZDecimal E6_OSGSTAmount
		{
			get { return base.E6_OSGSTAmount; }
			set { base.E6_OSGSTAmount = value; }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		[ReadOnlyMember(nameof(IsOSGSTAmountReadOnly))]
		public ZDecimal E6_OSGSTAmount_Calc
		{
			get { return GSTCalculationStrategy.GetE6_OSGSTAmount(); }
			set
			{
				GSTCalculationStrategy.SetE6_OSGSTAmount((ZDecimal)Utilities.Round(value, CurrencyDecimals));
			}
		}

		public ZPropertyInfo E6_OSGSTAmount_CalcInfo
		{
			get { return GetZPropertyInfo(nameof(E6_OSGSTAmount_Calc)); }
		}

		JobConsolCostTaxCalculationStrategy GSTCalculationStrategy
		{
			get
			{
				JobConsolCostTaxCalculationStrategy gstCalculationStrategy = null;

				if (E6_IsTaxAmountOverridden)
				{
					gstCalculationStrategy = new JobConsolCostTaxCalculationWhenOverridden(this);
				}
				else
				{
					gstCalculationStrategy = new JobConsolCostTaxCalculationWhenNotOverridden(this);
				}

				return gstCalculationStrategy;
			}
		}

		internal bool IsOSGSTAmountReadOnly
		{
			get
			{
				return !E6_IsTaxAmountOverridden
					|| !(IsCostGSTApplicable && GSTCalculationStrategy.TaxRateAmount > 0m)
					|| !AccountingUtils.IsUserCanChangeGST(LedgerTypes.AccountsPayable)
					|| ShouldBeReadOnlyWhenPosted
					|| IsGatewayConsolCost;
			}
		}

		internal bool IsGSTAmountEqualToCalculatedGST
		{
			get { return E6_OSGSTAmount_Calc == new JobConsolCostTaxCalculationWhenNotOverridden(this).GetE6_OSGSTAmount(); }
		}

		internal bool IsAPInvoiceConsolCost => GSTCalculationStrategy.IsAPInvoiceConsolCost;

		#endregion

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal E6_Calc_LocalGSTAmount
		{
			get
			{
				var taxRateFromDate = TaxRate?.GetRate(E6_TaxDate);
				var taxRateFromEffectiveDate = TaxRate?.GetEffectiveExtraRate(E6_TaxDate);
				var osTaxAmountFromExTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, E6_OSCostAmount, TaxRate, taxRateFromDate, taxRateFromEffectiveDate, Currency, Company.PK);

				var result = E6_OSGSTAmount_Calc == osTaxAmountFromExTaxAmount
					? TaxAmountCalculator.GetLocalTaxAmount(Factory, E6_GC, E6_LocalCostAmount, TaxRate, taxRateFromDate, taxRateFromEffectiveDate, E6_OSGSTAmount_Calc, E6_ExchangeRate)
					: TaxAmountCalculator.GetLocalTaxAmountFromOSTaxAmount(Factory, E6_GC, E6_OSGSTAmount_Calc, E6_ExchangeRate);
				return result;
			}
		}

		public ZPropertyInfo E6_Calc_LocalGSTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.E6_Calc_LocalGSTAmount); }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal E6_Calc_OSTotalAmount
		{
			get { return E6_OSCostAmount + E6_OSGSTAmount_Calc; }
		}

		public ZPropertyInfo E6_Calc_OSTotalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(E6_Calc_OSTotalAmount)); }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal E6_Calc_LocalTotalAmount
		{
			get { return E6_Calc_LocalExTaxAmount + E6_Calc_LocalGSTAmount; }
		}

		public ZPropertyInfo E6_Calc_LocalTotalAmountInfo
		{
			get { return GetZPropertyInfo(Schema.E6_Calc_LocalTotalAmount); }
		}

		[List("Lookups.ApportionmentMethodList")]
		public override ZString E6_ApportionmentMethod
		{
			get { return base.E6_ApportionmentMethod; }
			set
			{
				var hasChanges = base.E6_ApportionmentMethod != value;
				base.E6_ApportionmentMethod = value;

				if (hasChanges)
				{
					if (value != ZString.Empty)
					{
						SplitApportionAmount();
						ApportionGSTCharges();
					}

					UntickSurplusApportionmentChargesWhenLocalAmountIsTooSmall();
				}
			}
		}

		protected bool E6_ApportionmentMethod_ReadOnly
		{
			get
			{
				var result = IsApprovingPosting
					|| (CalculationStrategy is ConsolCostCalculationStrategy && !Env.Security.MaintainConsolJobInvoicingEditMethod.IsAllowed)
					|| (CalculationStrategy is InvoicingBaseConsolCostCalculationStrategy && !Env.Security.AllowAPInvoiceConsolCostApportionmentMethodOverride.IsAllowed)
					|| (!IsGatewayConsolCost && !Env.Security.MaintainConsolJobInvoicingEditMethod.IsAllowed)
					|| (IsGatewayConsolCost && !Env.Security.GatewayConsolJobInvoicingEditMethod.IsAllowed);
				return result;
			}
		}

#if DEBUG
		public bool E6_ApportionmentMethod_ReadOnlyForTest => E6_ApportionmentMethod_ReadOnly;
#endif
		public void SetIsUsedForApportionment()
		{
			bool hasChanges = false;
			using (this.SuspendSplittingApportionAmount())
			{
				foreach (ApportionSplitCharge charge in ApportionmentCharges)
				{
					bool newValue = charge.ShouldIncludeInApportionment;

					hasChanges |= charge.JR_IsUsedForApportionment != newValue;

					charge.JR_IsUsedForApportionment = newValue;
				}
			}
			if (hasChanges)
			{
				SplitApportionAmount();
			}
		}

		[ReadOnlyMember(nameof(IsApprovingPosting))]
		[List("Lookups.PrepaidCollectList")]
		public override ZString E6_PPDCLT
		{
			get { return base.E6_PPDCLT; }
			set
			{
				bool hasChanges = E6_PPDCLT != value;
				base.E6_PPDCLT = value;
				if (hasChanges)
				{
					SetIsUsedForApportionment();
					SplitApportionAmount();
				}
			}
		}

		#region IAutoRatingChargeInfo

		public bool CanReautorate(CostSell costOrSell, params ZString[] operationalJobCodes)
		{
			// This has to be revised. Check how the method in BaseCharge is realized. Perhaps we need to check payment bases as well.
			// Must be addressed in:
			// WI00583188 - [CW1] [RFF] CanReautorate for JobConsolCost
			return E6_RatingBehaviour == JobChargeLookups.ReAutorateCharge;
		}

		public ZBool IsApportioned => false;
		public ICurrency CostCurrency => InvoiceCurrency;
		public ICurrency SellCurrency => InvoiceCurrency;
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal CostAmount => E6_OSCostAmount;
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AgentDeclaredCostAmount => 0;
		public ZDecimal SellAmount => 0;
		public ZDecimal LocalCostAmount => E6_LocalCostAmount;
		public ZDecimal LocalSellAmount => 0;
		public ZString CostReference => E6_CostReference;
		public ZString SellReference => ZString.Empty;
		public ZString CostRatingBehavior => E6_RatingBehaviour;
		public ZString SellRatingBehavior => ZString.Empty;
		public ZGuid CostAccountPK { get; }
		public IAutoRatingChargeInfo ParentConsolCost => null;

		/// <summary>
		///		Use this collection for read-only only, otherwise changes won't be saved to the DB.
		///		For managing attributes for the cost please use JobConsolCostAttribCollection, i.e. <see cref="Attributes"/> property.
		///
		///		Later it will be refactored to return some read-only collection. Can't do it now as it is being used by JobCharge to manage attributes.
		/// </summary>
		RateAttributeSet IAutoRatingChargeInfo.RateAttributes
		{
			get
			{
				var set = new RateAttributeSet();

				foreach (JobConsolCostAttrib attr in Attributes)
				{
					set.Add(attr.E6A_Name, attr.E6A_Value, attr.E6A_Amount);
				}

				return set;
			}
		}

		#endregion

		#region Calculated Properties

		JobConsolCost[] ConsolCostsWithSameInvoice
		{
			get
			{
				JobConsolCost[] result = new JobConsolCost[] { this };

				if (!E6_InvoiceNum.IsEmpty && E6_OH_Creditor.IsValid)
				{
					var collection = ((IBusinessObjectInternals)this).ParentCollections.FirstOrDefault(x => x is JobConsolCostCollection) as JobConsolCostCollection;

					if (collection != null)
					{
						if (E6_AH_APInvoice.IsValid)
						{
							result = collection.Cast<JobConsolCost>().Where(x => x.E6_AH_APInvoice == E6_AH_APInvoice).ToArray();
						}
						else
						{
							result = collection.Cast<JobConsolCost>().Where(x => x.E6_InvoiceNum == E6_InvoiceNum && x.E6_OH_Creditor == E6_OH_Creditor).ToArray();
						}
					}
				}

				return result;
			}
		}

		[DecimalPlaces(nameof(InvoiceCurrencyDecimals))]
		public ZDecimal InvoiceOSTotal
		{
			get
			{
				if (InvoiceOSCurrency == LocalCurrency)
				{
					return InvoiceLocalTotal;
				}
				if (ConcreteAPInvoice != null)
				{
					return Math.Abs(APInvoiceLines.Sum(x => x.AL_OSAmount));
				}
				return ConsolCostsWithSameInvoice.Sum(x => x.E6_Calc_OSTotalAmount);
			}
		}

		public ZPropertyInfo InvoiceOSTotalInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceOSTotal); }
		}

		[DecimalPlaces(nameof(InvoiceCurrencyDecimals))]
		public ZDecimal InvoiceOSTax
		{
			get
			{
				if (InvoiceOSCurrency == LocalCurrency)
				{
					return InvoiceLocalTax;
				}
				if (ConcreteAPInvoice != null)
				{
					return Math.Abs(APInvoiceLines.Sum(x => x.AL_OSTaxAmount));
				}
				return ConsolCostsWithSameInvoice.Sum(x => x.E6_OSGSTAmount_Calc);
			}
		}

		public ZPropertyInfo InvoiceOSTaxInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceOSTax); }
		}

		public ZString InvoiceOSCurrency
		{
			get
			{
				return ConsolCostsWithSameInvoice.Where(x => x.E6_RX_NKCurrency != E6_RX_NKCurrency).Any() ? LocalCurrency : E6_RX_NKCurrency;
			}
		}

		RefCurrency InvoiceCurrency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, InvoiceOSCurrency);

		public int InvoiceCurrencyDecimals => InvoiceCurrency?.Decimals ?? LocalCurrencyDecimals;

		public ZPropertyInfo InvoiceOSCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceOSCurrency); }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal InvoiceLocalTotal
		{
			get
			{
				if (ConcreteAPInvoice != null)
				{
					return Math.Abs(APInvoiceLines.Sum(x => x.AL_LocalTotalAmount));
				}
				return ConsolCostsWithSameInvoice.Sum(x => x.E6_Calc_LocalTotalAmount);
			}
		}

		public ZPropertyInfo InvoiceLocalTotalInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceLocalTotal); }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal InvoiceLocalTax
		{
			get
			{
				if (ConcreteAPInvoice != null)
				{
					return Math.Abs(APInvoiceLines.Sum(x => x.AL_LocalTaxAmount));
				}
				return ConsolCostsWithSameInvoice.Sum(x => x.E6_Calc_LocalGSTAmount);
			}
		}

		public ZPropertyInfo InvoiceLocalTaxInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceLocalTax); }
		}

		#region CostGovtChargeCode

		[MaxLength(nameof(CostGovtChargeCodeMaxLength))]
		public ZString E6_CostGovtChargeCode
		{
			get
			{
				var apportionmentCharges = ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Where(x => !x.IsApplyingParentGovtChargeCode);

				if (!apportionmentCharges.Any())
				{
					return e6_CostGovtChargeCode;
				}
				else if (apportionmentCharges.AllSame(x => x.JR_CostGovtChargeCode))
				{
					return apportionmentCharges.First().JR_CostGovtChargeCode;
				}

				return ZString.Empty;
			}
			set
			{
				if (value != e6_CostGovtChargeCode)
				{
					CheckMaximumLength(E6_CostGovtChargeCodeInfo, value);
					e6_CostGovtChargeCode = value;

					foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
					{
						splitCharge.JR_CostGovtChargeCode = e6_CostGovtChargeCode;
					}

					E6_CostGovtChargeCodeInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateE6_CostGovtChargeCode();
					}
				}
			}
		}
		ZString e6_CostGovtChargeCode;

		public ZPropertyInfo E6_CostGovtChargeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.E6_CostGovtChargeCode); }
		}

		public int CostGovtChargeCodeMaxLength
		{
			get { return JobChargeSchema.JR_CostGovtChargeCode.MaxLength; }
		}

		protected bool E6_CostGovtChargeCode_ReadOnly
		{
			get { return !Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ConsolInvAllowOverrideCostGovtCrgCode).IsAllowed; }
		}

		#endregion

		#region SellGovtChargeCode

		[MaxLength(nameof(SellGovtChargeCodeMaxLength))]
		public ZString E6_SellGovtChargeCode
		{
			get
			{
				var apportionmentCharges = ApportionmentCharges
					.Cast<ApportionSplitCharge>()
					.Where(x => !x.IsApplyingParentGovtChargeCode);

				if (!apportionmentCharges.Any())
				{
					return e6_SellGovtChargeCode;
				}
				else if (apportionmentCharges.AllSame(x => x.JR_SellGovtChargeCode))
				{
					return apportionmentCharges.First().JR_SellGovtChargeCode;
				}

				return ZString.Empty;
			}
			set
			{
				if (value != e6_SellGovtChargeCode)
				{
					CheckMaximumLength(E6_SellGovtChargeCodeInfo, value);
					e6_SellGovtChargeCode = value;
					E6_SellGovtChargeCodeInfo.RefreshBinding();

					foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
					{
						splitCharge.JR_SellGovtChargeCode = e6_SellGovtChargeCode;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateE6_SellGovtChargeCode();
					}
				}
			}
		}
		ZString e6_SellGovtChargeCode;

		public ZPropertyInfo E6_SellGovtChargeCodeInfo
		{
			get { return GetZPropertyInfo(Schema.E6_SellGovtChargeCode); }
		}

		public int SellGovtChargeCodeMaxLength
		{
			get { return JobChargeSchema.JR_SellGovtChargeCode.MaxLength; }
		}

		protected bool E6_SellGovtChargeCode_ReadOnly
		{
			get { return !Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ConsolInvAllowOverrideSellGovtCrgCode).IsAllowed; }
		}

		#endregion

		#region E6_MasterBillNumber

		public ZString E6_MasterBillNumber
		{
			get { return GenericConsolBizO != null ? GenericConsolBizO.VX_SecondaryCode : ZString.Empty; }
		}

		public ZPropertyInfo E6_MasterBillNumberInfo
		{
			get { return GetZPropertyInfo(Schema.E6_MasterBillNumber); }
		}

		#endregion

		#region Withholding Tax
		[List("Lookups.WithholdingTaxes")]
		[RelatedBusinessObject("WithholdingTax")]
		public ZGuid E6_AW
		{
			get
			{
				if (ApportionmentCharges.Count == 0)
				{
					return e6_AW;
				}
				else
				{
					return ApportionmentCharges[0].JR_AW_CostWHTRate;
				}
			}
			set
			{
				if (e6_AW != value)
				{
					if (ApportionmentCharges.Count == 0)
					{
						e6_AW = value;
					}
					else
					{
						foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
						{
							splitCharge.JR_AW_CostWHTRate = value;
						}

						e6_AW = ApportionmentCharges[0].JR_AW_CostWHTRate;
					}

					SetWHTAmount();

					E6_AWInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateE6_AW();
					}
				}
			}
		}
		ZGuid e6_AW;

		public ZPropertyInfo E6_AWInfo
		{
			get { return GetZPropertyInfo(Schema.E6_AW); }
		}

		public bool E6_AW_ReadOnly
		{
			get
			{
				return !IsCostWHTApplicable
					   || !AccountingUtils.IsUserCanChangeWHT(LedgerTypes.AccountsPayable)
					   || ShouldBeReadOnlyWhenPosted
					   || IsGatewayConsolCost;
			}
		}

		ZBool IsCostWHTApplicable
		{
			get
			{
				return E6_OH_Creditor.IsValid && GlbCompany.CurrentCompany.GC_IsWHTRegistered && Creditor.MiscServ.OM_APWHTApplicable;
			}
		}

		public AccWithholding WithholdingTax => E6_AW.IsValid ? Factory.Load<AccWithholding>(E6_AW) : null;

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal E6_OSWHTAmount
		{
			get
			{
				Recalculate_e6_OSWHTAmount();
				return e6_OSWHTAmount;
			}
			private set
			{
				if (AccountingValuesRoundingHelper.PropertyHasChanges(this, e6_OSWHTAmount != AccountingUtils.Round(value, Currency)))
				{
					e6_OSWHTAmount = AccountingUtils.Round(value, Currency);
				}

				E6_OSWHTAmountInfo.RefreshBinding();
			}
		}
		ZDecimal e6_OSWHTAmount;

		public ZPropertyInfo E6_OSWHTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.E6_OSWHTAmount); }
		}

		void SetWHTAmount()
		{
			E6_OSWHTAmount = GetOSWHTAmount();
		}

		ZDecimal GetOSWHTAmount()
		{
			return TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalExTaxAmount(E6_LocalCostAmount, WithholdingTax, E6_ExchangeRate, Currency);
		}

		void Recalculate_e6_OSWHTAmount()
		{
			if (shouldRecalculate_e6_OSWHTAmount && E6_AW != ZGuid.Empty)
			{
				e6_OSWHTAmount = GetOSWHTAmount();
			}
		}

		bool shouldRecalculate_e6_OSWHTAmount;
		#endregion

		ConsolAndCostAccrualCalculator AccrualCalculator
		{
			get
			{
				if (accrualCalculator == null)
				{
					accrualCalculator = Factory.GetCachedValue(ConsolAndCostAccrualCalculator.ConsolAndCostAccrualCalculatorKey,
						() => { return new ConsolAndCostAccrualCalculator(Factory, E6_OH_Creditor); });
				}
				return accrualCalculator;
			}
		}
		ConsolAndCostAccrualCalculator accrualCalculator;

		#region E6_ConsolCostAccrual

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal E6_ConsolCostAccrual
		{
			get
			{
				ZDecimal result = 0;
				if (E6_OH_Creditor.IsValid && E6_AC_ChargeCode.IsValid && E6_ParentID.IsValid && E6_GC.IsValid)
				{
					result = AccrualCalculator.GetAccrualBasedOnConsolCost(this);
				}
				return result;
			}
		}

		public ZPropertyInfo E6_ConsolCostAccrualInfo
		{
			get { return GetZPropertyInfo(Schema.E6_ConsolCostAccrual); }
		}

		#endregion

		#region E6_ConsolTotalAccrual

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal E6_ConsolTotalAccrual
		{
			get
			{
				ZDecimal result = 0;
				if (E6_OH_Creditor.IsValid && E6_AC_ChargeCode.IsValid && E6_ParentID.IsValid && E6_GC.IsValid)
				{
					result = AccrualCalculator.GetAccrualBasedOnConsolShipmentAndConsolCost(this);
				}
				return result;
			}
		}

		public ZPropertyInfo E6_ConsolTotalAccrualInfo
		{
			get { return GetZPropertyInfo(Schema.E6_ConsolCostAccrual); }
		}

		#endregion

		[DecimalPlaces(nameof(CurrencyDecimals))]
		[ResourceStringData("10bff613-055d-4eff-b6c0-31afa9065a5c", Caption = "Unapportioned Amount")]
		public ZDecimal UnApportionedAmount
		{
			get
			{
				var apportionedChargesTotal = ApportionmentCharges.Cast<ApportionSplitCharge>().Sum(x => x.JR_OSCostAmt);

				return E6_OSCostAmount - apportionedChargesTotal;
			}
		}

		public ZPropertyInfo UnApportionedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(UnApportionedAmount)); }
		}

		#endregion

		#region Extra Tax and "real" GST Amounts

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal E6_OSGSTRealAmount
		{
			get { return GSTCalculationStrategy.GetE6_OSGSTRealAmount(); }
		}

		public ZPropertyInfo E6_OSGSTRealAmountInfo
		{
			get { return GetZPropertyInfo(nameof(E6_OSGSTRealAmount)); }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		[ReadOnly(true)]
		public ZDecimal E6_OSExtraTaxAmount
		{
			get { return GSTCalculationStrategy.GetE6_OSExtraTaxAmount(); }
		}

		public ZPropertyInfo E6_OSExtraTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(E6_OSExtraTaxAmount)); }
		}

		#endregion

		#region GSTInclusiveAmount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal GSTInclusiveAmount
		{
			get
			{
				return IsGSTInclusiveAmount ? gstInclusiveAmount : (ZDecimal)(E6_OSCostAmount + E6_OSGSTAmount_Calc);
			}
			set
			{
				SetGSTInclusiveAmountCore(value, true);
			}
		}
		ZDecimal gstInclusiveAmount;

		public void SetGSTInclusiveAmountCore(ZDecimal value, bool isRecalculationOsAmt)
		{
			var roundedValue = Utilities.Round(value, (Currency != null ? Currency.Decimals : GlbCompany.CurrentCompany.LocalCurrency.Decimals));
			if (gstInclusiveAmount != roundedValue)
			{
				if (IsGSTInclusiveAmount)
				{
					gstInclusiveAmount = roundedValue;
					if (isRecalculationOsAmt)
					{
						E6_OSCostAmount = GSTCalculationStrategy.GetOSCostAmountFromGSTInclusiveAmount(gstInclusiveAmount);
					}
				}
				GSTInclusiveAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo GSTInclusiveAmountInfo
		{
			get { return GetZPropertyInfo(Schema.GSTInclusiveAmount); }
		}

		public bool GSTInclusiveAmount_ReadOnly
		{
			get { return !IsGSTInclusiveAmount; }
		}

		internal ZBool IsGSTInclusiveAmount
		{
			get
			{
				APInvoiceConsolCostCollection collection = null;
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					collection = ((IBusinessObjectInternals)this).ParentCollections[0] as APInvoiceConsolCostCollection;
				}
				var resultWithOutNeedUpdate = collection != null && collection.ParentAPInvoice != null && collection.ParentAPInvoice.GSTInclusiveAmounts;
				var isValueChanged = isGSTInclusiveAmount != resultWithOutNeedUpdate;

				isGSTInclusiveAmount = resultWithOutNeedUpdate;
				if (isValueChanged && !resultWithOutNeedUpdate)
				{
					Validation.ValidateGSTInclusiveAmount();
				}
				return resultWithOutNeedUpdate && collection != null && collection.ParentAPInvoice != null && !collection.ParentAPInvoice.GSTInclusiveAmountNeedUpdate;
			}
		}
		ZBool isGSTInclusiveAmount;

		internal ZBool GSTInclusiveAmountNeedUpdate
		{
			get
			{
				APInvoiceConsolCostCollection collection = null;
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					collection = ((IBusinessObjectInternals)this).ParentCollections[0] as APInvoiceConsolCostCollection;
				}
				return collection != null && collection.ParentAPInvoice != null && collection.ParentAPInvoice.GSTInclusiveAmountNeedUpdate;
			}
			set
			{
				APInvoiceConsolCostCollection collection = null;
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					collection = ((IBusinessObjectInternals)this).ParentCollections[0] as APInvoiceConsolCostCollection;
				}
				if (collection != null && collection.ParentAPInvoice != null)
				{
					collection.ParentAPInvoice.GSTInclusiveAmountNeedUpdate = value;
				}
			}
		}

		#endregion

		#region E6_SupplyType

		[List("Lookups.SupplyTypes")]
		public override ZString E6_SupplyType
		{
			get { return base.E6_SupplyType; }
			set
			{
				if (E6_SupplyType != value)
				{
					using (GetValidationSuspender())
					{
						base.E6_SupplyType = value;

						foreach (ApportionSplitCharge splitCharge in ApportionmentCharges)
						{
							splitCharge.JR_CostSupplyType = value;
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateE6_SupplyType();
					}

					SetGSTRateAndTaxMessage();
				}
			}
		}

		#endregion

		#endregion

		#region ReadOnly

		internal bool ShouldBeReadOnlyWhenPosted => IsPosted && !IsApprovingPosting;

		protected bool GetPropertiesReadOnlyState(PropertyDescriptor property) => GetPropertiesReadOnlyStateCore(property);

		bool GetPropertiesReadOnlyStateCore(PropertyDescriptor property)
		{
			var shouldBeReadOnlyWhenPosted = ShouldBeReadOnlyWhenPosted && !FieldsNotReadOnlyWhenPosted.Contains(property.Name);
			var shouldPropertyBeReadOnly = shouldBeReadOnlyWhenPosted || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
			return shouldPropertyBeReadOnly;
		}

		/// <summary>
		/// The fields below should be NOT read-only when cost is posted.
		/// This set should be kept as minimum as possible by adding "ReadOnly" handlers for properties, don't add them to the set.
		/// </summary>
		HashSet<string> FieldsNotReadOnlyWhenPosted => fieldsNotReadOnlyWhenPosted ??
			(
				fieldsNotReadOnlyWhenPosted = new HashSet<string>
				{
					// special UI fields that can still be editable when charge is posted
					nameof(E6_RatingBehaviour)
				}
			);
		HashSet<string> fieldsNotReadOnlyWhenPosted;

		#endregion

		#region Apportionment

		public IJobInvoicingPlugIn[] ShipmentsToApportion
		{
			get
			{
				var result = new List<IJobInvoicingPlugIn>();

				if (Consol != null)
				{
					foreach (IJobInvoicingPlugIn shipment in Consol.CostSupporter.ShipmentsList)
					{
						if (shipment.InvoicingSupporter.IncludeInConsolCosting(E6_ApportionToRelatedShipments))
						{
							result.Add(shipment);
						}
					}

					//Will collect information to see which shipments are considered for apportion
					Tracer.TraceInformation(AccountingTraceSourceCodes.ConsolCost, BuildTraceMessage);
				}

				return result.ToArray();

				#region Trace Message

				string BuildTraceMessage()
				{
					var allShipments = string.Join(", ", Consol.CostSupporter.ShipmentsList.Select(r => Invariant($"{r.JobNumber} of type {r.InvoicingSupporter.GetType()}")).ToArray());
					var apportionedToShipments = string.Join(", ", result.Select(r => r.JobNumber).ToArray());
					var message = Invariant($"Traced @{this.GetType().FullName}.{nameof(ShipmentsToApportion)} ->\r\n{nameof(E6_ApportionToRelatedShipments)}={E6_ApportionToRelatedShipments.ToYesNoString()}\r\n All Shipments: {allShipments}\r\nShipments toApportion: {apportionedToShipments}\r\n{nameof(Consol.CostSupporter)} type: {Consol.CostSupporter.GetType()}");
					return message;
				}

				#endregion
			}
		}

		[ReadOnlyMember(nameof(IsImportedConsolCost))]
		public ZBool E6_ApportionToRelatedShipments
		{
			get
			{
				if (!fE6_ApportionToRelatedShipments.HasValue)
				{
					fE6_ApportionToRelatedShipments = AccountingConfigurationRegistry.Instance.ConsolCostDefaultRelatedShipmentsApportionment.Value;
				}
				return fE6_ApportionToRelatedShipments.Value;
			}
			set
			{
				bool hasChanges = !fE6_ApportionToRelatedShipments.HasValue || fE6_ApportionToRelatedShipments != value;
				fE6_ApportionToRelatedShipments = value;
				if (hasChanges)
				{
					UpdateApportionmentChargesListing();
					if (E6_PPDCLT == PrepaidCollectList.Codes.CTS)
					{
						SetIsUsedForApportionment();
					}
				}
				E6_ApportionToRelatedShipmentsInfo.RefreshBinding();
			}
		}
		bool? fE6_ApportionToRelatedShipments;

		public ZPropertyInfo E6_ApportionToRelatedShipmentsInfo
		{
			get { return GetZPropertyInfo(Schema.E6_ApportionToRelatedShipments); }
		}

		public void PrepareForPosting()
		{
			if (HasSaveBeenRun)
			{
				return;
			}

			HasSaveBeenRun = true;
			var chargesToDelete = new List<BaseCharge>();
			var chargesToAdd = new List<BaseCharge>();

			foreach (var charge in ApportionmentCharges.ToArray<ApportionSplitCharge>())
			{
				if (!charge.IsInDatabase)
				{
					var chargeToUpdate = GetMatchingCharge(charge);
					if (chargeToUpdate != null)
					{
						UpdateMatchingCharge(chargeToUpdate, charge);
						chargesToAdd.Add(chargeToUpdate);
					}
					else if (charge.InvoicingJob != null)
					{
						var newCharge = charge.InvoicingJob.Charges.AddNew();
						PopulateNewCharge(newCharge, charge);
						chargesToAdd.Add(newCharge);
					}
					else
					{
						var jobDeltedCallStack = CriticalValidationInfoCollectorService.GetService(charge.Factory)?.GetInfo(charge.JR_JH, CriticalValidationInfoCollectorServiceKeyType.DeleteJobHeader);
						var errorMessage = $@"ApportionSplitCharge without InvoicingJob. New Apportionment Charge will be deleted without finding a matching Job Charge or creating new one.
Charge JR_JH:{charge.JR_JH}
Job Deleted:{jobDeltedCallStack}
Charge Details:{charge.GetJobChargeInfo()}
JobConsolCostInfo: {this.GetJobConsolCostInfo()}";
						ErrorReporter.ReportOnce("ApportionSplitCharge without InvoicingJob", errorMessage);
					}
					chargesToDelete.Add(charge);
				}
				else if (charge.IsDisbursementCharge &&
					(charge.JR_RX_NKCostCurrencyInfo.HasChanges
					|| charge.JR_OSCostAmtInfo.HasChanges
					|| charge.JR_LocalSellAmtInfo.HasChanges
					)
					&& !charge.IsRevenuePosted)
				{
					var loadedCharge = Factory.Load<Charge>(charge.PK);
					TryToSetEmptyRevenueExRateFromConsolCostExRate(loadedCharge);
					loadedCharge.Calculations.UpdateRevenueBasedOnCost();
				}
			}

			foreach (var chargeToDelete in chargesToDelete)
			{
				chargeToDelete.Delete();
			}

			foreach (var chargeToAdd in chargesToAdd)
			{
				var apportionmentCharge = Factory.Load<ApportionSplitCharge>(chargeToAdd.PK);
				apportionmentCharge.CostCalculationDescription = chargeToAdd.CostCalculationDescription;
				apportionmentCharge.RevenueCalculationDescription = chargeToAdd.RevenueCalculationDescription;
				apportionmentCharge.SetShipmentInfo(chargeToAdd.ShipmentInfo);
				ApportionmentCharges.Add(apportionmentCharge);

				CollectApportionmentChargesInfoOnSaving();
			}

			(chargesToAdd.FirstOrDefault() as ChargeWithCost)?.CreateAutoJobRevenueJournal();
			SynchroniseUnpostedInvoiceDetailsIfNecessary();
		}

		Charge GetMatchingCharge(ApportionSplitCharge charge)
		{
			var matchingCharges = Factory.Load<Charge>(GetMatchingChargesQuery(charge)).ToList();

			var chargeRelatedJobNumber = charge.JR_Calc_RelatedJobNumber;
			if (!chargeRelatedJobNumber.IsEmpty)
			{
				matchingCharges.RemoveAll(x => x.JR_Calc_RelatedJobNumber != chargeRelatedJobNumber);
			}

			if (!charge.JR_E6_GatewaySellHeader.IsEmpty)
			{
				matchingCharges.RemoveAll(x => x.SellAccountIsOrgProxy);
			}

			// Further filtering, find best matches based on the presented charge attributes
			var bestMatches = charge.FindBestMatchesWithAttributes(matchingCharges);

			return bestMatches.Cast<Charge>()
				.OrderByDescending(x => x.IsInDatabase)
				.FirstOrDefault(x => !x.IsCostPosted && !x.IsRevenuePostedWithManualJobRevenueJournal);
		}

		ZQuery GetMatchingChargesQuery(ApportionSplitCharge charge)
		{
			var result = new ZQuery(JobChargeSchema.JR_AC, E6_AC_ChargeCode);
			result.AddToFilter(JobChargeSchema.JR_GB, charge.JR_GB);
			result.AddToFilter(JobChargeSchema.JR_GE, charge.JR_GE);
			result.AddToFilter(JobChargeSchema.JR_JH, charge.JR_JH);
			result.AddToFilter(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, charge.PK);

			var jobConsolCostQuery = new ZQuery(JobChargeSchema.JR_E6, SQLComparisonOperator.Equal, null);
			jobConsolCostQuery.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_E6, PK);
			result.AddToFilter(jobConsolCostQuery);

			var creditorQuery = new ZQuery(JobChargeSchema.JR_OH_CostAccount, SQLComparisonOperator.Equal, null);
			if (E6_OH_Creditor.IsValid)
			{
				creditorQuery.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_OH_CostAccount, E6_OH_Creditor);
			}
			result.AddToFilter(creditorQuery);

			return result;
		}

		void UpdateMatchingCharge(Charge chargeToUpdate, ApportionSplitCharge charge)
		{
			using (chargeToUpdate.GetValidationSuspender())
			{
				CopyValues(charge, chargeToUpdate);
				using (chargeToUpdate.SuppressAutoRatingOverride())
				{
					chargeToUpdate.JR_OSCostAmt = charge.JR_OSCostAmt;
					if (charge.JR_IsCostTaxAmountOverridden)
					{
						chargeToUpdate.JR_OSCostGSTAmt_Calc = charge.JR_OSCostGSTAmt_Calc;
					}
					chargeToUpdate.JR_LocalCostAmt = charge.JR_LocalCostAmt;
					TryToSetEmptyRevenueExRateFromConsolCostExRate(chargeToUpdate);
					chargeToUpdate.SetEstimatedCost(charge.JR_EstimatedCost);
				}
			}
		}

		void PopulateNewCharge(Charge newCharge, ApportionSplitCharge charge)
		{
			using (newCharge.GetValidationSuspender())
			using (newCharge.SuppressAutoRatingOverride())
			{
				using (newCharge.Calculations.SuspendCalculations())
				{
					CopyValues(charge, newCharge);
				}
				UpdateRevenueAmount(newCharge, charge);
				newCharge.SetEstimatedCost(charge.JR_EstimatedCost);
			}
		}

		public bool HasSynchronisedAPInvoiceDetails
		{
			get
			{
				return ApportionmentCharges.AreInvoiceDetailsInSyncWithConsolCost
					&& (!IsPosted
						|| (ApportionmentCharges.ArePostedToSameAPInvoiceAsConsolCost
							&& ApportionmentCharges.AreInvoiceDetailsInSyncWithPostedInvoiceLines));
			}
		}
		bool IsPostedConsolCostToSync => IsPosted || ApportionmentCharges.Cast<ApportionSplitCharge>().Any(x => x.JR_IsCostPosted);

		[SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		public bool SynchroniseUnpostedInvoiceDetailsIfNecessary()
		{
			var hasToSync = !IsPostedConsolCostToSync && !HasSynchronisedAPInvoiceDetails;
			if (hasToSync)
			{
				var chargesToSync = ApportionmentCharges.ToArray<ApportionSplitCharge>();
				var chargesWithTaxIssues = chargesToSync.Where(x => x.JR_AT_CostGSTRate != E6_AT_TaxRate).ToArray();
				var chargesWithTaxDateIssues = chargesToSync.Where(x => x.JR_CostTaxDate != E6_TaxDate).ToArray();

				foreach (var charge in chargesToSync.Where(x => x.JR_APInvoiceNum != E6_InvoiceNum))
				{
					charge.JR_APInvoiceNum = E6_InvoiceNum;
				}
				foreach (var charge in chargesToSync.Where(x => x.JR_APInvoiceDate != E6_InvoiceDate))
				{
					charge.JR_APInvoiceDate = E6_InvoiceDate;
				}
				foreach (var charge in chargesToSync.Where(x => x.JR_APDocumentReceivedDate != E6_DocumentReceivedDate))
				{
					charge.JR_APDocumentReceivedDate = E6_DocumentReceivedDate;
				}
				foreach (var charge in chargesToSync.Where(x => x.JR_PaymentDate != E6_PaymentDate))
				{
					charge.JR_PaymentDate = E6_PaymentDate;
				}
				foreach (var charge in chargesToSync.Where(x => x.JR_OH_CostAccount != E6_OH_Creditor))
				{
					charge.JR_OH_CostAccount = E6_OH_Creditor;
				}
				foreach (var charge in chargesToSync.Where(x => x.JR_CostReference != E6_CostReference))
				{
					charge.JR_CostReference = E6_CostReference;
				}
				foreach (var charge in chargesWithTaxIssues)
				{
					charge.JR_AT_CostGSTRate = E6_AT_TaxRate;
				}
				foreach (var charge in chargesWithTaxDateIssues)
				{
					charge.JR_CostTaxDate = E6_TaxDate;
				}
				foreach (var charge in chargesToSync.Where(x => x.JR_A9_CostVATClass != E6_A9_VATClass))
				{
					charge.JR_A9_CostVATClass = E6_A9_VATClass;
				}
				foreach (var charge in chargesToSync.Where(x => x.JR_CostPlaceOfSupply != E6_PlaceOfSupply))
				{
					charge.JR_CostPlaceOfSupply = E6_PlaceOfSupply;
				}
				foreach (var charge in chargesToSync.Where(x => x.JR_CostPlaceOfSupplyType != E6_PlaceOfSupplyType))
				{
					charge.JR_CostPlaceOfSupplyType = E6_PlaceOfSupplyType;
				}
				foreach (var charge in chargesToSync.Where(x => x.JR_GB_CostTaxBranch != E6_GB_CostTaxBranch))
				{
					charge.JR_GB_CostTaxBranch = E6_GB_CostTaxBranch;
				}

				if (chargesWithTaxIssues.Any() || chargesWithTaxDateIssues.Any())
				{
					PushUnApportionedGSTAmountBasedOnRepresentation();
				}
			}
			return hasToSync;
		}

		AccTransactionHeader APInvoiceUsedInSync
		{
			get
			{
				if (APInvoice != null)
				{
					return APInvoice;
				}
				foreach (ApportionSplitCharge apportionSplitCharge in ApportionmentCharges)
				{
					if (apportionSplitCharge.IsCostPosted && apportionSplitCharge.APLine?.TransactionHeader is AccTransactionHeader transactionHeader)
					{
						return transactionHeader;
					}
				}
				return null;
			}
		}

		public bool SynchronisePostedInvoiceDetailsIfPossible(out string errorMessage)
		{
			var hasToSync = false;
			errorMessage = string.Empty;

			if (IsPostedConsolCostToSync && APInvoiceUsedInSync != null && ApportionmentCharges.Count > 0)
			{
				if (!ApportionmentCharges.ArePostedToSameAPInvoiceAsConsolCost)
				{
					if (ApportionmentCharges.GetPostedAPInvoices.Count() == 1
						&& ApportionmentCharges.ArePostedWithSameTaxRateAndClass)
					{
						E6_AH_APInvoice = ApportionmentCharges[0].APLine.AL_AH;
					}
					else
					{
						errorMessage = Res.GetString("ad643378-fa48-488b-bc5b-9bebb1eeba71", "- Has Charge(s) not posted or posted to different Invoice.");
					}
				}

				if (!ApportionmentCharges.ArePostedWithSameTaxRateAndClass)
				{
					errorMessage += System.Environment.NewLine;
					errorMessage += Res.GetString("a6649780-25df-4539-9497-399f297b241e", "- Has Charge(s) posted with different Tax Rate.");
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					hasToSync |= SyncConsolCostAndInvoice();
					hasToSync |= SyncChargeAndInvoice();
				}
				else
				{
					errorMessage = Res.GetString("a7f97a5c-478b-40bb-88a4-e2b347798c8c", "Consol Cost with Charge Code {0}, Invoice # {1} for Creditor {2}", ChargeCode.AC_Code, E6_InvoiceNum, Creditor.OH_Code) + System.Environment.NewLine + errorMessage;
				}
			}
			return hasToSync;
		}

		bool SyncChargeAndInvoice()
		{
			bool result = false;

			var postedTaxDate = ApportionmentCharges.GetPostedTaxDates.First();
			if (postedTaxDate != ZDate.Empty)
			{
				foreach (ApportionSplitCharge charge in ApportionmentCharges.OfType<ApportionSplitCharge>().Where(x => x.JR_CostTaxDate.IsEmpty && x.JR_CostTaxDate != postedTaxDate))
				{
					using (charge.CostTaxRecalculationSuspender.GetSuspender())
					{
						result = true;
						charge.JR_CostTaxDate = postedTaxDate;
					}
				}
			}

			return result;
		}

		bool SyncConsolCostAndInvoice()
		{
			bool result = E6_InvoiceNum != APInvoiceUsedInSync.AH_TransactionNum || E6_InvoiceDate != APInvoiceUsedInSync.AH_InvoiceDate || E6_DocumentReceivedDate != APInvoiceUsedInSync.AH_DocumentReceivedDate
						  || E6_PaymentDate != APInvoiceUsedInSync.AH_DueDate || E6_OH_Creditor != APInvoiceUsedInSync.AH_OH || E6_CostReference != APInvoiceUsedInSync.AH_TransactionReference
						  || E6_AT_TaxRate != ApportionmentCharges.GetPostedTaxRates.First() || E6_TaxDate != ApportionmentCharges.GetPostedTaxDates.First()
						  || E6_A9_VATClass != ApportionmentCharges.GetPostedTaxClasses.First() || E6_PlaceOfSupply != APInvoiceUsedInSync.AH_PlaceOfSupply
						  || E6_PlaceOfSupplyType != APInvoiceUsedInSync.AH_PlaceOfSupplyType || E6_GB_CostTaxBranch != ApportionmentCharges.GetPostedTaxBranch.First();

			if (result)
			{
				if (E6_OH_Creditor != APInvoiceUsedInSync.AH_OH)
				{
					E6_OH_Creditor = APInvoiceUsedInSync.AH_OH;
				}
				if (E6_InvoiceNum != APInvoiceUsedInSync.AH_TransactionNum)
				{
					E6_InvoiceNum = APInvoiceUsedInSync.AH_TransactionNum;
				}
				if (E6_InvoiceDate != APInvoiceUsedInSync.AH_InvoiceDate)
				{
					E6_InvoiceDate = APInvoiceUsedInSync.AH_InvoiceDate;
				}
				if (E6_DocumentReceivedDate != APInvoiceUsedInSync.AH_DocumentReceivedDate)
				{
					E6_DocumentReceivedDate = APInvoiceUsedInSync.AH_DocumentReceivedDate;
				}
				if (E6_PaymentDate != APInvoiceUsedInSync.AH_DueDate)
				{
					E6_PaymentDate = APInvoiceUsedInSync.AH_DueDate;
				}
				if (E6_CostReference != APInvoiceUsedInSync.AH_TransactionReference)
				{
					E6_CostReference = APInvoiceUsedInSync.AH_TransactionReference;
				}
				if (E6_AT_TaxRate != ApportionmentCharges.GetPostedTaxRates.First())
				{
					E6_AT_TaxRate = ApportionmentCharges.GetPostedTaxRates.First();
				}
				if (E6_TaxDate != ApportionmentCharges.GetPostedTaxDates.First())
				{
					E6_TaxDate = ApportionmentCharges.GetPostedTaxDates.First();
				}
				if (E6_A9_VATClass != ApportionmentCharges.GetPostedTaxClasses.First())
				{
					E6_A9_VATClass = ApportionmentCharges.GetPostedTaxClasses.First();
				}
				if (E6_PlaceOfSupply != APInvoiceUsedInSync.AH_PlaceOfSupply)
				{
					E6_PlaceOfSupply = APInvoiceUsedInSync.AH_PlaceOfSupply;
				}
				if (E6_PlaceOfSupplyType != APInvoiceUsedInSync.AH_PlaceOfSupplyType)
				{
					E6_PlaceOfSupplyType = APInvoiceUsedInSync.AH_PlaceOfSupplyType;
				}
				if (E6_GB_CostTaxBranch != ApportionmentCharges.GetPostedTaxBranch.First())
				{
					E6_GB_CostTaxBranch = ApportionmentCharges.GetPostedTaxBranch.First();
				}
			}

			return result;
		}

		void UpdateRevenueAmount(Charge chargeToUpdate, ApportionSplitCharge apportionCharge)
		{
			// Don't fire any calculators if:
			//		a) It's not a DSB charge with unposted revenue
			//		b) It's a Non-DSB charge, but it has a revenue value
			// otherwise, let the system calculate the revenue field when ACharge was not rated sell

			if ((!chargeToUpdate.IsDisbursementCharge || chargeToUpdate.IsRevenuePosted) && chargeToUpdate.JR_OSSellAmt != 0)
			{
				chargeToUpdate.Calculations.SwitchOffCalculations();
			}

			chargeToUpdate.JR_OSCostAmt = apportionCharge.JR_OSCostAmt;
			chargeToUpdate.JR_LocalCostAmt = apportionCharge.JR_LocalCostAmt;
			if (chargeToUpdate.JR_IsCostTaxAmountOverridden)
			{
				chargeToUpdate.JR_OSCostGSTAmt_Calc = apportionCharge.JR_OSCostGSTAmt_Calc;
			}
			chargeToUpdate.JR_OSCostWHTAmt = apportionCharge.JR_OSCostWHTAmt;

			TryToSetEmptyRevenueExRateFromConsolCostExRate(chargeToUpdate);
		}

		void TryToSetEmptyRevenueExRateFromConsolCostExRate(Charge charge)
		{
			if (charge.JR_OSSellExRate <= 0m
				&& charge.JR_RX_NKSellCurrency == E6_RX_NKCurrency)
			{
				charge.RevenueExchangeRate?.SetBaseRate(E6_ExchangeRate);
			}
		}

		void CopyValues(ApportionSplitCharge fromCharge, BaseCharge toCharge)
		{
			if (fromCharge.HasContext(BusinessContext.AutoJobRevenueJournal))
			{
				toCharge.SetContext(BusinessContext.AutoJobRevenueJournal);
			}

			toCharge.JR_E6 = fromCharge.JR_E6;
			toCharge.OrgNotDebtorCheckEnabled = fromCharge.OrgNotDebtorCheckEnabled;
			if (!toCharge.IsRevenuePosted && !toCharge.JR_OH_SellAccount.IsValid)
			{
				toCharge.JR_OH_SellAccount = fromCharge.JR_OH_SellAccount;
			}
			toCharge.JR_AC = fromCharge.JR_AC;
			toCharge.JR_IsCostTaxAmountOverridden = fromCharge.JR_IsCostTaxAmountOverridden;
			toCharge.JR_OH_CostAccount = fromCharge.JR_OH_CostAccount;
			toCharge.JR_APInvoiceNum = fromCharge.JR_APInvoiceNum;
			toCharge.JR_APInvoiceDate = fromCharge.JR_APInvoiceDate;
			toCharge.JR_APDocumentReceivedDate = fromCharge.JR_APDocumentReceivedDate;
			toCharge.JR_CostReference = fromCharge.JR_CostReference;
			toCharge.JR_PaymentDate = fromCharge.JR_PaymentDate;
			toCharge.JR_PaymentType = fromCharge.JR_PaymentType;
			toCharge.JR_AB = fromCharge.JR_AB;
			toCharge.JR_GE = fromCharge.JR_GE;
			toCharge.JR_GB = fromCharge.JR_GB;
			toCharge.JR_CostPlaceOfSupply = fromCharge.JR_CostPlaceOfSupply;
			toCharge.UpdateChequeBook(fromCharge.JR_AK);
			toCharge.UpdateChequeNumberWithoutUpdatingChequeBook(fromCharge.JR_ChequeNo);
			toCharge.JR_AT_CostGSTRate = fromCharge.JR_AT_CostGSTRate;
			toCharge.SetCostTaxDateSafe(fromCharge.JR_CostTaxDate);
			toCharge.JR_A9_CostVATClass = fromCharge.JR_A9_CostVATClass;
			toCharge.JR_AW_CostWHTRate = fromCharge.JR_AW_CostWHTRate;
			toCharge.JR_RX_NKCostCurrency = fromCharge.JR_RX_NKCostCurrency;
			toCharge.JR_OSCostExRate = fromCharge.JR_OSCostExRate;
			toCharge.JR_E6_GatewaySellHeader = fromCharge.JR_E6_GatewaySellHeader;
			if (!toCharge.IsInternalJobInfoDisabled)
			{
				toCharge.JR_JH_InternalJob = fromCharge.JR_JH_InternalJob;
				toCharge.JR_GB_InternalBranch = fromCharge.JR_GB_InternalBranch;
				toCharge.JR_GE_InternalDept = fromCharge.JR_GE_InternalDept;
			}
			toCharge.JR_CostGovtChargeCode = fromCharge.JR_CostGovtChargeCode;
			toCharge.JR_SellGovtChargeCode = fromCharge.JR_SellGovtChargeCode;
			toCharge.JR_CostSupplyType = fromCharge.JR_CostSupplyType;
			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				toCharge.JR_GB_CostTaxBranch = fromCharge.JR_GB_CostTaxBranch;
			}
			if (!toCharge.IsInDatabase ||
				(toCharge.IsInDatabase && ((ZBool)toCharge.JR_IsIncludedInProfitShareInfo.OriginalValue) == fromCharge.JR_IsIncludedInProfitShare))
			{
				toCharge.JR_IsIncludedInProfitShare = fromCharge.JR_IsIncludedInProfitShare;
				toCharge.JR_AgentDeclaredCostAmt = fromCharge.JR_AgentDeclaredCostAmt;
			}
			toCharge.CostCalculationDescription = fromCharge.CostCalculationDescription;

			if (!toCharge.IsInDatabase && fromCharge.JR_DisplaySequence > 0)
			{
				toCharge.JR_DisplaySequence = fromCharge.JR_DisplaySequence;
			}

			if (fromCharge.IsDisbursementCharge && toCharge.IsDisbursementCharge)
			{
				toCharge.JR_Desc = fromCharge.JR_Desc;
				toCharge.RevenueCalculationDescription = fromCharge.CostCalculationDescription;
			}

			if (fromCharge.InvoicingJob.Parent.IsGatewayBillingEnabled())
			{
				toCharge.JR_Calc_RelatedJobNumber = fromCharge.JR_Calc_RelatedJobNumber;
			}

			if (fromCharge.JobChargeAttributes.Any())
			{
				toCharge.ReplaceAttributes(fromCharge.JobChargeAttributes);
			}
			else
			{
				toCharge.AddAttributes(((IAutoRatingChargeInfo)this).RateAttributes);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				UpdateApportionmentChargesListing();
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			shouldRecalculate_e6_OSWHTAmount = true;
		}
		public bool ApportionAgentDeclaredCost { get; set; }

		public void SplitApportionAmount()
		{
			if (!IsSplitApportionAmountSuspended)
			{
				using (ApportionmentCharges.GetApportionmentSplitChargeCollectionValidationSuspender())
				{
					SchemaColumn agentDeclaredCostAmtColumn = null;

					if (ApportionAgentDeclaredCost)
					{
						agentDeclaredCostAmtColumn = JobChargeSchema.JR_AgentDeclaredCostAmt;
					}

					if (AgentDeclaredOSAmount == 0m && !RatingDataRegistry.Instance.UnspecifiedCostShouldForceZeroToBePulledThrough.Value)
					{
						AgentDeclaredOSAmount = E6_OSCostAmount;
					}

					ApportionmentCreator.Apportion(this, JobChargeSchema.JR_OSCostAmt, E6_OSCostAmount, agentDeclaredCostAmtColumn, AgentDeclaredOSAmount);
					PushLocalAmountExchangeRateDifferencesToGreatestCharge();
					PushUnApportionedGSTAmountBasedOnRepresentation();
					ApportionAgentDeclaredCost = false;
				}
			}
		}

		public void ApportionGSTCharges()
		{
			if (!IsSplitApportionAmountSuspended)
			{
				if (E6_OSCostAmount != 0m && Currency != null && E6_IsTaxAmountOverridden)
				{
					foreach (ApportionSplitCharge charge in ApportionmentCharges)
					{
						ZString refCurrencyNK = charge.CostCurrency != null ? charge.JR_RX_NKCostCurrency : ZString.Empty;
						charge.JR_OSCostGSTAmt_Calc = (AccountingUtils.Round(E6_OSGSTAmount_Calc * (charge.JR_OSCostAmt / E6_OSCostAmount), refCurrencyNK));
					}
				}

				PushUnApportionedGSTAmountBasedOnRepresentation();
			}
		}

		internal ZDecimal LocalCurrencyMinAmount
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.CurrencyMinAmount(); }
		}

		internal void PushLocalAmountExchangeRateDifferencesToGreatestCharge()
		{
			if (!IsSplitApportionAmountSuspended && UnApportionedAmount == ZDecimal.Zero)
			{
				ApportionSplitCharge[] sortedCharges = (from ApportionSplitCharge charge in ApportionmentCharges
														where charge.JR_IsUsedForApportionment
														orderby Math.Abs(charge.JR_OSCostAmt) descending
														select charge
														).ToArray();

				if (sortedCharges.Any())
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.PushToGreatestChargeExchangeRateDifferences_BeforeMethodCall, GetInfoBeforePush);

					if (E6_LocalCostAmount == ZDecimal.Zero)
					{
						foreach (ApportionSplitCharge charge in ApportionmentCharges)
						{
							charge.JR_LocalCostAmt = ZDecimal.Zero;
						}
					}
					else if (E6_RX_NKCurrency != GlbCompany.CurrentCompany.LocalCurrency.RX_Code)
					{
						var totalLocalCostOnSplitCharges = sortedCharges.Sum(x => x.JR_LocalCostAmt);
						var roundingError = E6_LocalCostAmount - totalLocalCostOnSplitCharges;

						if (roundingError != 0m)
						{
							var lastCharge = sortedCharges.Last();
							var completed = false;

							foreach (var charge in sortedCharges)
							{
								completed = ApplyMaxPossibleAdjustment(charge, ref roundingError);

								if (completed)
								{
									break;
								}
								else if (charge == lastCharge)
								{
									charge.JR_LocalCostAmt = 0m; // That should not happen but it will cause a Critical Validation Validation error
								}
							}
						}
					}

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.PushToGreatestChargeExchangeRateDifferences_AfterMethodCall, GetInfoAfterPush);
				}
			}

			string GetInfoBeforePush()
			{
				var info = new ZStringBuilder();
				info.AppendLine(Invariant($"E6_OSCostAmount: {E6_OSCostAmount}, E6_LocalCostAmount: {E6_LocalCostAmount}, currency: {E6_RX_NKCurrency}, local currency: {LocalCurrency}, exchange rate: {E6_ExchangeRate}"));
				info.AppendLine("Amounts on charges (JR_IsUsedForApportionment, JR_OSCostAmt, JR_LocalCostAmt):");
				info.AppendLine(Invariant($"Before push: {GetAmountsInfo()}"));

				return info.ToString();
			}

			string GetInfoAfterPush()
			{
				var info = new ZStringBuilder();
				info.AppendLine(Invariant($"After  push: {GetAmountsInfo()}"));
				info.AppendLine(new StackTrace().ToString());

				return info.ToString();
			}

			string GetAmountsInfo()
			{
				return new ZStringBuilder(ApportionmentCharges.Cast<ApportionSplitCharge>().Select(x => Invariant($"({x.JR_IsUsedForApportionment}, {x.JR_OSCostAmt}, {x.JR_LocalCostAmt})"))).ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		internal void PushUnApportionedGSTAmountBasedOnRepresentation()
		{
			if (!IsSplitApportionAmountSuspended && E6_IsTaxAmountOverridden && UnApportionedAmount.IsEmpty)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.PushUnApportionedGSTAmountBasedOnRepresentation_BeforeMethodCall, GetInfoBeforePush);

				this.PushUnApportionedAmountBasedOnRepresentation(E6_OSGSTAmount_Calc, JobCharge.Schema.JR_OSCostGSTAmt_Calc);

				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.PushUnApportionedGSTAmountBasedOnRepresentation_AfterMethodCall, GetInfoAfterPush);
			}

			string GetInfoBeforePush()
			{
				var info = new ZStringBuilder();
				info.AppendLine(Invariant($"E6_OSCostAmount: {E6_OSCostAmount}, E6_OSGSTAmountOverride: {E6_OSGSTAmount}"));
				info.AppendLine("Amounts on charges (JR_OSCostAmt, JR_OSCostGSTAmtOverride):");
				info.AppendLine(Invariant($"Before push: {GetAmountsInfo()}"));

				return info.ToString();
			}

			string GetInfoAfterPush()
			{
				var info = new ZStringBuilder();
				info.AppendLine(Invariant($"After  push: {GetAmountsInfo()}"));
				info.AppendLine(new StackTrace().ToString());

				return info.ToString();
			}

			string GetAmountsInfo()
			{
				return new ZStringBuilder(ApportionmentCharges.Cast<ApportionSplitCharge>().Select(x => Invariant($"({x.JR_OSCostAmt}, {x.JR_OSCostGSTAmt})"))).ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		internal bool HasUnApportionedGSTAmount() => this.HasUnApportionedAmount(E6_OSGSTAmount_Calc, JobCharge.Schema.JR_OSCostGSTAmt_Calc);

		bool ApplyMaxPossibleAdjustment(ApportionSplitCharge charge, ref decimal adjustment)
		{
			const decimal maxAllowedDifference = 5m;

			bool result = false;
			var maxAdjustment = maxAllowedDifference * Math.Sign(adjustment);   // We can update Amount no more than by 5 with correct sign
			if (Math.Sign(charge.JR_LocalCostAmt) != Math.Sign(adjustment))
			{
				var maxNegativeAdjustment = -(charge.JR_LocalCostAmt - LocalCurrencyMinAmount * Math.Sign(charge.JR_LocalCostAmt)); // For Amount and Adjustment with different sign we can update Amount to be not less than 0.01 or 1 with the same sign.
																																	// That is why We substract and reverse sign to get largest Adjustment that can be applied to Amount
				maxAdjustment = maxAllowedDifference < Math.Abs(maxNegativeAdjustment) ? maxAdjustment : maxNegativeAdjustment;         // We should not exceed maxAllowedDifference
			}

			if (maxAdjustment != 0m)
			{
				if (Math.Abs(adjustment) <= Math.Abs(maxAdjustment))
				{
					charge.JR_LocalCostAmt += adjustment;
					adjustment = ZDecimal.Zero;
					result = true;
				}
				else
				{
					charge.JR_LocalCostAmt += maxAdjustment;
					adjustment -= maxAdjustment;
				}
			}

			return result;
		}

		public bool IsContainerService
		{
			get
			{
				bool result = false;
				var forwardingConsol = Consol as ForwardingConsol;
				if (forwardingConsol != null && this.ChargeCode != null)
				{
					result = forwardingConsol.Containers.Cast<ForwardingContainer>().Any(c => c.Services.Cast<JobService>().Any(x => x.ES_ServiceCode == this.ChargeCode.AC_ChargeSubGroup));
				}
				return result;
			}
		}
		#endregion

		#region Light Validation

		protected override bool EnableLightValidationIfAvailable
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value;
			}
		}

#if DEBUG
		public bool EnableLightValidationIfAvailable_ForTestOnly()
		{
			return EnableLightValidationIfAvailable;
		}
#endif

		#endregion

		internal void RecalculateCostAmountWithUnApportionedAmount()
		{
			if (UnApportionedAmount != 0m)
			{
				using (SuspendSplittingApportionAmount())
				{
					ZDecimal localCostAmtSum = 0m;
					ZDecimal osCostAmtSum = 0m;
					ZDecimal osCostGSTAmtSum = 0m;

					foreach (ApportionSplitCharge aCharge in ApportionmentCharges)
					{
						if (!aCharge.JR_E6.IsEmpty)
						{
							localCostAmtSum += aCharge.JR_LocalCostAmt;
							osCostAmtSum += aCharge.JR_OSCostAmt;
							osCostGSTAmtSum += aCharge.JR_OSCostGSTAmt_Calc;
						}
					}

					if (E6_LocalCostAmount != localCostAmtSum)
					{
						E6_LocalCostAmount = localCostAmtSum;
					}

					if (E6_OSCostAmount != osCostAmtSum)
					{
						E6_OSCostAmount = osCostAmtSum;
					}

					if (E6_OSGSTAmount_Calc != osCostGSTAmtSum)
					{
						E6_IsTaxAmountOverridden = true;
						E6_OSGSTAmount_Calc = osCostGSTAmtSum;
					}
				}
			}
		}

		public static (bool isDeleted, string message) DeleteUnpostedConsolCostsLinkedWithJobs(IEnumerable<Job> jobs)
		{
#if DEBUG
			if (Globals.IsTest && IsForceToMakeConsolCostDeleteFail_ForTestOnly)
			{
				return (false, "for test only fail message");
			}
#endif
			var result = false;
			var msgBuilder = new ZStringBuilder();
			try
			{
				if (jobs.Any())
				{
					var allUnpostedConsolCosts = new HashSet<JobConsolCost>();

					foreach (Job job in jobs)
					{
						foreach (JobConsolCost cost in job.UnpostedConsolCostsLinkedToThisJob)
						{
							allUnpostedConsolCosts.Add(cost);
						}
					}

					if (allUnpostedConsolCosts.Any())
					{
						foreach (var consolCost in allUnpostedConsolCosts)
						{
							if (consolCost.CanDelete)
							{
								consolCost.DeleteCostAndCharges();
							}
						}

						var jobsWhoseunpostedConsolCostCouldNotBeDeleted = jobs.Where(x => x.UnpostedConsolCostsLinkedToThisJob.Any());
						if (jobsWhoseunpostedConsolCostCouldNotBeDeleted.Any())
						{
							msgBuilder.Append(Res.GetString("abfa459c-0436-47e3-8c87-535fba7c6c95", "Failed to delete unposted consol cost(s) that are associated with jobs: {0}", JobClosureHelper.GetJobNumbers(jobsWhoseunpostedConsolCostCouldNotBeDeleted)));
						}
						else
						{
							result = true;
							msgBuilder.Append(Res.GetString("92f1a305-a323-474e-94d5-62cc14f8b484", "Successfully deleted unposted consol cost(s) associated with jobs: {0}", JobClosureHelper.GetJobNumbers(jobsWhoseunpostedConsolCostCouldNotBeDeleted)));
						}
					}
				}
				else
				{
					msgBuilder.Append(Res.GetString("04ce861e-f2a6-440c-ae93-89c6cb7b4958", "Empty Job List"));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				msgBuilder.Append(Res.GetString("85a80eea-8a55-4ea2-b199-035d6c8b9b60", "An error occurred. Couldn't delete unposted consol costs. \r\n {0}", ex.Message));
				throw;
			}
			return (result, msgBuilder.ToStringWithNewLineBetweenAppends());
		}

#if DEBUG
		[ThreadStatic]
		public static bool IsForceToMakeConsolCostDeleteFail_ForTestOnly;

		[ThreadStatic]
		public static bool IsForceToSkipDeletingApportionSplitCharge_ForTestOnly;
#endif

		#region IApportionedChargesHeader

		IApportionedCharge[] IApportionedChargesHeader.Charges
		{
			get { return ApportionmentCharges.ToArray<ApportionSplitCharge>(); }
		}

		ZString IApportionedChargesHeader.ApportionmentMethod
		{
			get { return E6_ApportionmentMethod; }
		}

		bool IApportionedChargesHeader.IsChargeReadyToPost(IApportionedCharge charge)
		{
			var appCharge = charge as ApportionSplitCharge;
			return charge.IsUsedForApportionment && appCharge != null && appCharge.JR_OSCostAmt != 0M && !appCharge.IsCostPosted;
		}

		ZDecimal IApportionedChargesHeader.FreeSpace => Consol != null ? Consol.CostSupporter.FreeSpace : 0;

		#endregion

		#region Implementation

		public void UpdateApportionmentChargesListing()
		{
			if (!IsDeleted)
			{
				CalculationStrategy.UpdateApportionmentChargesListing();
			}
		}

		public void UpdateShipmentInfosOnCharges()
		{
			new ConsolCostCalculationStrategy(this).UpdateShipmentInfosOnCharges();
		}

		public IDisposable SuspendSplittingApportionAmount() => ApportionAmountSplitterSuspender.GetSuspender();

		bool IsSplitApportionAmountSuspended => ApportionAmountSplitterSuspender.IsSuspended;

		FunctionalitySuspender ApportionAmountSplitterSuspender => apportionAmountSplitterSuspender ?? (apportionAmountSplitterSuspender = new FunctionalitySuspender());
		FunctionalitySuspender apportionAmountSplitterSuspender;

#if DEBUG
		public
#else
		internal
#endif
			FunctionalitySuspender ReportSettingParentSuspender
		{
			get { return reportSettingParentSuspender ?? (reportSettingParentSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender reportSettingParentSuspender;

		public Job GetJob(IJobInvoicingPlugIn jobParent)
		{
			var jobQuery = new ZQuery(JobHeaderSchema.JH_ParentID, jobParent.PK);
			jobQuery.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			return Factory.LoadTop1<Job>(jobQuery);
		}

		public ZGuid RelatedConsolCostPK { get; set; }

		public JobConsolCost RelatedConsolCostFromDatabase
		{
			get { return Factory.Load<JobConsolCost>(RelatedConsolCostPK); }
		}

		public bool IsImportedConsolCost
		{
			get { return RelatedConsolCostPK.IsValid; }
		}

		bool IsConsolReadOnly
		{
			get { return IsImportedConsolCost || IsApprovingPosting; }
		}

		public bool ValidateIfRelatedConsolCostCanBeMarkedAsImported(JobConsolCost relatedCost)
		{
			return relatedCost != null && relatedCost.E6_AC_ChargeCode == E6_AC_ChargeCode;
		}

		[List("ConsolList")]
		public ZGuid E6_ParentIDReadOnly
		{
			get { return E6_ParentID; }
		}

		public ZPropertyInfo E6_ParentIDReadOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(E6_ParentIDReadOnly)); }
		}

		public AccGenericConsol GenericConsolBizO
		{
			get { return Factory.LoadTop1<AccGenericConsol>(new ZQuery(ViewGenericConsolSchema.PK, E6_ParentID)); }
		}

		[List("ConsolList")]
		[ReadOnlyMember(nameof(IsConsolReadOnly))]
		[RelatedBusinessObject("GenericConsolBizO")]
		public override ZGuid E6_ParentID
		{
			get { return base.E6_ParentID; }
			set
			{
				ReportDeveloperExceptionIfSettingParentWithoutSuspender();
				using (ReportSettingParentSuspender.GetSuspender())
				{
					CollectE6_ParentIDChangedInfoIfRequired(base.E6_ParentID, value);

					base.E6_ParentID = value;
					if (E6_ParentID.IsValid)
					{
						E6_ParentTableCode = AccGenericConsol.GetParentTableCodeFromParentId(Factory, E6_ParentID);
					}
				}
				CalculationStrategy.OnE6_ParentIDConsolSet();
			}
		}

		public override ZString E6_ParentTableCode
		{
			get { return base.E6_ParentTableCode; }
			set
			{
				ReportDeveloperExceptionIfSettingParentWithoutSuspender();
				base.E6_ParentTableCode = value;
			}
		}

		public void SetE6_ParentIDAndE6_ParentTableCodeTogether(ZGuid parentId, ZString parentTableCode)
		{
			ReportDeveloperExceptionIfSettingParentWithoutSuspender();

			CollectE6_ParentIDChangedInfoIfRequired(base.E6_ParentID, parentId);

			base.E6_ParentID = parentId;

			using (ReportSettingParentSuspender.GetSuspender())
			{
				E6_ParentTableCode = parentTableCode;
			}
			CalculationStrategy.OnE6_ParentIDConsolSet();
		}

		void CollectE6_ParentIDChangedInfoIfRequired(ZGuid currentParentId, ZGuid newParentId)
		{
			if (!currentParentId.IsEmpty && newParentId.IsEmpty)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory)
					.AddLastInfoWhenAllowed(PK,
					CriticalValidationInfoCollectorServiceKeyType.ConsolCostParentChangedFromNonEmptyToEmpty,
					() => Invariant($"\r\nE6_ParentId has been changed from {currentParentId} to {newParentId}.\r\n{System.Environment.StackTrace}"));
			}
		}

		/// <summary>
		/// Will report Developer Exception if E6_ParentID or E6_ParentTableCode are set in any way but via calling JobConsolCollection.TryAddNew() method which checks if Consol allows adding Consol Costs
		/// </summary>
		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		void ReportDeveloperExceptionIfSettingParentWithoutSuspender()
		{
			if (!ReportSettingParentSuspender.IsSuspended && !this.HasContext(BusinessContext.EnableDirectSettingConsolCostParent))
			{
				var devErrorMessage = @"Use JobConsolCostCollection.TryAddNew() method to add a new JobConsolCost to a Consol.
Use APInvoiceConsolCostCollection.TryAddNewForConsol(Consol) method to add a new Consol Cost.
They will check if adding Consol Cost is allowed by Consol and return null when it is not (e.g. Gateway Consol).
The JobConsolCost.ReportSettingParentSuspender can be used to wrap later setting of E6_ParentID and E6_ParentTablecode in cases when JobConsolCost parent is not set at its creation.
Setting BusinessContext.EnableDirectSettingConsolCostParent on the Consol BusinessObject level should be used to allow adding new JobConsolCost to JobConsolCostCollection in GUI.
Setting same BusinessContext on AP Invoice should be used for GUI when we have Consol Costs linked to AP Invoice in GUI.
Make sure there is a validation to prevent saving illegitimate Consol Costs for Gateway Consols.";

				ErrorReporter.ReportOnce(devErrorMessage + System.Environment.NewLine + System.Environment.NewLine + new StackTrace().ToString());
			}
		}

		public GenericConsolCollection ConsolList
		{
			get
			{
				if (fConsolList == null)
				{
					fConsolList = new GenericConsolCollection(Factory);
				}
				return fConsolList;
			}
		}
		GenericConsolCollection fConsolList;

		public event EventHandler<APInvoiceCostingJobCreationErrorEventArgs> APInvoiceConsolCostingJobCreationError;

		public class APInvoiceCostingJobCreationErrorEventArgs : EventArgs
		{
			public APInvoiceCostingJobCreationErrorEventArgs(string errorMessage)
			{
				this.ErrorMessage = errorMessage;
			}

			public readonly string ErrorMessage;
		}

		void RaiseOnConsolChanged(InvoicingBaseConsolCostImporter costImporter)
		{
			if (!IsConsolChangedSuspended)
			{
				foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					if (collection is APInvoiceConsolCostCollection consolCostCollection)
					{
						if (consolCostCollection.IsConsolCostChangedHooked && !consolCostCollection.IsConsolCostSelectionOnConsolChangedSuspended)
						{
							((APInvoiceConsolCostCollection)collection).RaiseOnConsolChanged(this, new ConsolChangedEventArgs(costImporter));
						}
					}
				}
			}
		}

		bool IsCalculatingForeignAndLocalAmountsSuspended;

		class ForeignLocalAmountSuspender : IDisposable
		{
			public ForeignLocalAmountSuspender(JobConsolCost parent)
			{
				this.Parent = parent;
				parent.IsCalculatingForeignAndLocalAmountsSuspended = true;
			}

			readonly JobConsolCost Parent;

			#region IDisposable Members

			void IDisposable.Dispose()
			{
				Parent.IsCalculatingForeignAndLocalAmountsSuspended = false;
			}

			#endregion
		}

		internal bool IsConsolChangedSuspended;

		public ConsolChangedSuspender GetConsolChangedSuspender()
		{
			return new ConsolChangedSuspender(this);
		}

		public class ConsolChangedSuspender : IDisposable
		{
			public ConsolChangedSuspender(JobConsolCost parent)
			{
				parent.IsConsolChangedSuspended = true;
				this.Parent = parent;
			}

			readonly JobConsolCost Parent;

			void IDisposable.Dispose()
			{
				Parent.IsConsolChangedSuspended = false;
			}
		}

		public class ConsolChangedEventArgs : EventArgs
		{
			public ConsolChangedEventArgs(InvoicingBaseConsolCostImporter costImporter)
			{
				this.CostImporter = costImporter;
			}

			public readonly InvoicingBaseConsolCostImporter CostImporter;
		}

		public void PopulateMissingApportionments(JobConsolCost consolCost)
		{
			foreach (ApportionSplitCharge apportionSplitCharge in consolCost.ApportionmentCharges)
			{
				bool found = false;

				foreach (ApportionSplitCharge thisSplitCharge in ApportionmentCharges)
				{
					if (apportionSplitCharge.JR_JH == thisSplitCharge.JR_JH)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					ApportionSplitCharge newCharge = ApportionmentCharges.AddNew();
					using (newCharge.SuspendSettingHasChanges())
					{
						newCharge.JR_JH = apportionSplitCharge.JR_JH;
						newCharge.JR_GB = apportionSplitCharge.JR_GB;
						newCharge.JR_GE = apportionSplitCharge.JR_GE;
						newCharge.JR_AC = E6_AC_ChargeCode;
						RefCurrency currency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, E6_RX_NKCurrency);
						if (currency != null)
						{
							newCharge.JR_RX_NKCostCurrency = currency.RX_Code;
						}
						newCharge.JR_OSCostExRate = E6_ExchangeRate;
						newCharge.JR_E6 = PK;
						newCharge.JR_OH_CostAccount = E6_OH_Creditor;
						newCharge.JR_APInvoiceNum = E6_InvoiceNum;
						newCharge.JR_APInvoiceDate = E6_InvoiceDate;
						newCharge.JR_APDocumentReceivedDate = E6_DocumentReceivedDate;
						newCharge.JR_CostReference = E6_CostReference;
						newCharge.JR_CostPlaceOfSupply = E6_PlaceOfSupply;
						newCharge.JR_AT_CostGSTRate = E6_AT_TaxRate;
						newCharge.SetCostTaxDateSafe(E6_TaxDate);
						newCharge.JR_A9_CostVATClass = E6_A9_VATClass;
						newCharge.JR_PaymentDate = E6_PaymentDate;
						newCharge.JR_PaymentType = E6_PaymentType;
						newCharge.JR_AB = E6_AB_BankAccount;
						newCharge.JR_AK = E6_AK_ChequeBook;
						newCharge.JR_ChequeNo = E6_ChequeOrReference;
						newCharge.JR_CostGovtChargeCode = E6_CostGovtChargeCode;
						newCharge.JR_SellGovtChargeCode = E6_SellGovtChargeCode;
						newCharge.JR_CostSupplyType = E6_SupplyType;
						if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
						{
							newCharge.JR_GB_CostTaxBranch = E6_GB_CostTaxBranch;
						}
					}
				}
			}
		}

		#endregion

		#region IQuickCalculatorCharge Members

		Job IQuickCalculatorCharge.InvoicingJob
		{
			get { return null; }
		}

		bool IQuickCalculatorCharge.CanUpdateSell
		{
			get { return false; }
		}

		bool IQuickCalculatorCharge.CanUpdateCost
		{
			get { return !IsPosted || IsApprovingPosting; }
		}

		void IQuickCalculatorCharge.SetAmount(CostSell costSell, AutoRateInfo result, OrgHeader org, bool recalculateOverridden)
		{
			if (costSell == CostSell.Cost)
			{
				IsAutoratingInProcess = ZBool.True;
				E6_OSCostAmount = result.Amount;
				PaymentBases.DeleteAll();
				result.Bases.ConvertToJobPaymentBases(costSell == CostSell.Cost, PaymentBases.AddNew);
				IsAutoratingInProcess = ZBool.False;
				SetCalculationDescription(result);
			}
		}

		bool IQuickCalculatorCharge.IsCalculationDescriptionRelevant
		{
			get { return false; }
		}

		string IQuickCalculatorCharge.SellCurrencyCode
		{
			get { return ZString.Empty; }
		}

		string IQuickCalculatorCharge.CostCurrencyCode
		{
			get { return Currency != null ? Currency.RX_Code : ZString.Empty; }
		}

		string IQuickCalculatorCharge.RatingBehaviour
		{
			get { return E6_RatingBehaviour; }
		}

		#endregion

		#region IPaymentBasisViewCharge

		BusinessObjectCollection<JobPaymentBasis> IPaymentBasisViewCharge.CostPaymentBasesView
		{
			get
			{
				var collection = new JobPaymentBasisViewCollection(Factory);
				collection.AddRange(PaymentBases);
				return collection;
			}
		}

		BusinessObjectCollection<JobPaymentBasis> IPaymentBasisViewCharge.SellPaymentBasesView => new JobPaymentBasisViewCollection(Factory);

		#endregion

		#region ISupportCriticalValidation

		ICriticalValidation ISupportCriticalValidation.CriticalValidation
		{
			get { return new JobConsolCostCriticalValidation(this); }
		}

		public sealed override void OnSaving()
		{
			base.OnSaving();
			OnSavingCore();
			((ISupportCriticalValidation)this).CriticalValidation.RegisterOnSavingCheck();
		}

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(this);
		}

		#endregion

		#region IJobConsolCost Members

		string IJobConsolCost.GetJobConsolCostInfo()
		{
			return Enterprise.Accounting.Business.CriticalValidation.CriticalValidationInfoExtensions.GetJobConsolCostInfo(this);
		}

		bool IJobConsolCost.ShouldBeReadOnlyWhenPosted => ShouldBeReadOnlyWhenPosted;

		string IChargeWithChargeCode.AC_Code => ChargeCode?.AC_Code;
		[SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Baseline")]
		string IChargeWithChargeCode.AC_Desc => ChargeCode?.AC_Desc;

		#endregion

		#region IRatingJobConsolCost Members

		string Enterprise.Integration.Rating.IRatingJobConsolCost.RatingBehaviour => E6_RatingBehaviour;
		decimal Enterprise.Integration.Rating.IRatingJobConsolCost.OSCostAmount => E6_OSCostAmount;

		#endregion

		#region ExchangeRateConfigurationRateConsumer

		public IAccExchangeRateConfigurationRateConsumer ExchangeRateConfigurationRateConsumer => ExchangeRateConfigurationRateConsumerCreator.CreateExchangeRateConfigurationRateConsumerForConsolCost(Consol, this, Company);

		#endregion

		internal InvoicingBase GetTheImportingIncompleteInvoiceIfAny()
		{
			var attribs = Attributes.OfType<JobConsolCostAttrib>()
									.Where(a => a.E6A_Name == JobChargeAttribTypeList.Codes.LinkedToIncompleteInvoice)
									.ToList();
			if (attribs.Count == 1 && ZGuid.TryParse(attribs[0].E6A_Value, out var invoicePK))
			{
				return Factory.Load<InvoicingBase>(invoicePK);
			}
			return null;
		}

		public ZShort E6_Calc_PostingGroupId
		{
			get { return TaxRate != null ? TaxRate.AT_PostingGroupId : (ZShort)AccTaxRate.DefaultPostingGroupID; }
		}

		public string BuildTraceMessageForApportionedCharges(string checkPoint)
		{
			var chargeDetails = string.Join("\r\n", ApportionmentCharges.OfType<ApportionSplitCharge>().Select(ap => ap.GetJobChargeInfo()));
			var message = Invariant($"{checkPoint} -> E6_PK: {PK}\r\nApportioned Charges: {chargeDetails}");
			return message;
		}

		readonly ITracer Tracer;
	}
}
