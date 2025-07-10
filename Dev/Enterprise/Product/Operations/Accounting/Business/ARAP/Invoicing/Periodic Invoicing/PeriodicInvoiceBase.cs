using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business.PublicInterfaceClasses;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public abstract class PeriodicInvoiceBase : NonPersistentBusinessObject, IObsoleteValidation, IJobTypePickerSupporter
	{
		public abstract class Schema
		{
			public const string InvoiceDate = "InvoiceDate";
		}

		public PeriodicInvoiceBase(BusinessObjectFactory factory)
			: base(factory)
		{
			Factory.RefreshEnabled = false;
			Factory.SetContext(BusinessContext.PeriodicInvoicePosting);
		}

		#region Events

		public event EventHandler OnChangeJobs;

		void RaiseOnChangeJobs()
		{
			if (OnChangeJobs != null)
			{
				OnChangeJobs(this, EventArgs.Empty);
			}
		}

		public event EventHandler OnChangeMiscInvoices;

		void RaiseOnChangeMiscInvoices()
		{
			if (OnChangeMiscInvoices != null)
			{
				OnChangeMiscInvoices(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PostDate = ZDateTime.Now;
			InvoiceDate = ZDateTime.Now;
		}

		#endregion

		#region Filter Business Object

		public PeriodicInvoiceBaseJobFilterBusinessObject JobsFilter
		{
			get { return FilterJobs_internal ?? (FilterJobs_internal = GetJobFilterBusinessObject()); }
		}
		PeriodicInvoiceBaseJobFilterBusinessObject FilterJobs_internal;

		protected abstract PeriodicInvoiceBaseJobFilterBusinessObject GetJobFilterBusinessObject();

		public PeriodicInvoiceMiscInvoicesFilterBusinessObject MiscInvoicesFilter
		{
			get
			{
				if (FilterMiscInvoices_internal == null)
				{
					FilterMiscInvoices_internal = new PeriodicInvoiceMiscInvoicesFilterBusinessObject(this);
					FilterMiscInvoices_internal.ModuleFilters.SupportsQueryCaching = false;
				}

				return FilterMiscInvoices_internal;
			}
		}
		PeriodicInvoiceMiscInvoicesFilterBusinessObject FilterMiscInvoices_internal;

		#endregion

		#region Lookups

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get { return Currencies_internal ?? (Currencies_internal = new RefCurrencyCollection(Factory)); }
		}
		RefCurrencyCollection Currencies_internal;

		#endregion

		#region Job Types

		public CodeDescriptionPairList JobTypeLookUp
		{
			get
			{
				if (jobTypeLookUp_internal == null)
				{
					jobTypeLookUp_internal = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
					jobTypeLookUp_internal.RemoveCode(JobInvoicingConsumerTypes.ForwardingConsol);
					jobTypeLookUp_internal.RemoveCode(JobInvoicingConsumerTypes.TransportBooking);

					if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != Core.Constants.CountryCodes.UnitedStates)
					{
						jobTypeLookUp_internal.RemoveCode(JobInvoicingConsumerTypes.ImporterSecurityFiling);
					}

					jobTypeLookUp_internal.Sort();
				}
				return jobTypeLookUp_internal;
			}
		}
		CodeDescriptionPairList jobTypeLookUp_internal;

		#endregion

		#endregion

		#region Properties

		#region Currency

		[MaxLength(3)]
		[RelatedBusinessObject("Currency")]
		[List("Currencies")]
		public ZString CurrencyNK
		{
			get { return CurrencyNK_internal; }
			set
			{
				if (CurrencyNK != value)
				{
					ClearJobs();
					ClearMiscInvoices();

					SetNonPersistentPropertyValue(CurrencyNKInfo, ref CurrencyNK_internal, value);

					if (!IsValidationSuspended)
					{
						ValidateCurrencyNK();
					}
				}
			}
		}
		ZString CurrencyNK_internal;

		public ZPropertyInfo CurrencyNKInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyNK)); }
		}

		public RefCurrency Currency
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, CurrencyNK); }
		}

		#region CurrencyNK_Decimals

		public ZInt CurrencyNK_Decimals
		{
			get
			{
				RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyNK);
				return currency != null ? currency.Decimals : GlbCompany.CurrentCompany.LocalCurrency.Decimals;
			}
		}

		public ZPropertyInfo CurrencyNK_DecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyNK_Decimals)); }
		}

		#endregion

		#region LocalCurrency_Decimals

		public ZInt LocalCurrency_Decimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		public ZPropertyInfo LocalCurrency_DecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(LocalCurrency_Decimals)); }
		}

		#endregion

		[List("Currencies")]
		public ZString CurrencyReadonlyNK
		{
			get { return CurrencyNK; }
		}

		public ZPropertyInfo CurrencyReadonlyPKInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyReadonlyNK)); }
		}

		[List("Currencies")]
		public ZString CurrencyReadonlyLocalNK
		{
			get { return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		public ZPropertyInfo CurrencyReadonlyLocalPKInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyReadonlyLocalNK)); }
		}

		#endregion

		#region Invoice Date

		public virtual ZDateTime InvoiceDate
		{
			get { return InvoiceDate_internal; }
			set
			{
				var defaultDate = value;
				defaultDate = ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(defaultDate);
				SetNonPersistentPropertyValue(InvoiceDateInfo, ref InvoiceDate_internal, defaultDate);
				if (!IsValidationSuspended)
				{
					ValidateInvoiceDate();
				}

				if (ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate())
				{
					PostDate = InvoiceDate;
				}

				if (!IsValidationSuspended
					&& !IsLocalCurrency
					&& ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, IsLocalCurrency, GlbCompany.CurrentCompany.PK,
						AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code,
						AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code))
				{
					ValidateCurrencyNK();
				}
			}
		}
		ZDateTime InvoiceDate_internal;

		public ZPropertyInfo InvoiceDateInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceDate); }
		}

		protected virtual bool InvoiceDate_ReadOnly
		{
			get
			{
				return ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate();
			}
		}

		#endregion

		#region Post Date

		public virtual ZDateTime PostDate
		{
			get { return PostDate_internal; }
			set
			{
				var defaultDate = value;
				defaultDate = ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(defaultDate);

				SetNonPersistentPropertyValue(PostDateInfo, ref PostDate_internal, defaultDate);
				if (!IsValidationSuspended)
				{
					ValidatePostDate();

					if (!IsLocalCurrency && ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AR, IsLocalCurrency,
						GlbCompany.CurrentCompany.PK, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code))
					{
						ValidateCurrencyNK();
					}
				}
			}
		}
		ZDateTime PostDate_internal;

		public ZPropertyInfo PostDateInfo
		{
			get { return GetZPropertyInfo(nameof(PostDate)); }
		}

		internal bool PostDate_ReadOnly
		{
			get
			{
				return (!(AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value && Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed))
					|| ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate();
			}
		}

		#endregion

		#region Job Type

		public ZBoolDescriptionPairList JobTypeList
		{
			get
			{
				if (fJobTypeList == null)
				{
					fJobTypeList = new ZBoolDescriptionPairList();

					foreach (CodeDescriptionPair pair in JobTypeLookUp)
					{
						fJobTypeList.AddNew(GetDescriptionForJobTypeList(pair.Code), false);
					}

					fJobTypeList.OnPairChanged += new ZBoolDescriptionPairChangedEventHandler(JobTypeList_OnPairChanged);
				}

				return fJobTypeList;
			}
		}
		ZBoolDescriptionPairList fJobTypeList;

		public List<ZString> SelectedJobTypeCodes
		{
			get
			{
				List<ZString> result = new List<ZString>();
				foreach (ZBoolDescriptionPair jobType in JobTypesPicker.SelectedJobTypeList)
				{
					result.Add(GetJobTypeCodeFromJobTypeListDescription(jobType.Description));
				}
				return result;
			}
		}

		public string GetDescriptionForJobTypeList(string jobTypeCode)
		{
			return "[" + jobTypeCode + "]" + JobTypeLookUp.GetDescriptionFromCode(jobTypeCode);
		}

		public string GetJobTypeCodeFromJobTypeListDescription(string description)
		{
			return JobTypeLookUp.GetCodeFromDescription(description.Substring(5).Trim());
		}

		void JobTypeList_OnPairChanged(ZBoolDescriptionPairChangedEventArgs e)
		{
			if (!JobTypeList_OnPairChangedSuspender.IsSuspended)
			{
				ClearJobs();
				ClearMiscInvoices();

				JobTypeList_OnPairChangedCore(e);
			}
		}

		protected virtual void JobTypeList_OnPairChangedCore(ZBoolDescriptionPairChangedEventArgs e)
		{
			InitializeFilterForJobs();
		}

		protected void ResetJobTypeList()
		{
			using (JobTypeList_OnPairChangedSuspender.GetSuspender())
			{
				JobTypesPicker.Reset();
			}
		}

		protected FunctionalitySuspender JobTypeList_OnPairChangedSuspender
		{
			get { return jobTypeList_OnPairChangedSuspender ?? (jobTypeList_OnPairChangedSuspender = new FunctionalitySuspender(() => JobTypeList_OnPairChanged(null))); }
		}
		FunctionalitySuspender jobTypeList_OnPairChangedSuspender;

		#endregion

		#region Lines

		PeriodicInvoiceSelectableJobCollection fJobs;
		public PeriodicInvoiceSelectableJobCollection Jobs
		{
			get
			{
				if (fJobs == null)
				{
					fJobs = new PeriodicInvoiceSelectableJobCollection(Factory);
					fJobs.IncludeInThePeriodicInvoiceChanged += Jobs_IncludeInThePeriodicInvoiceChanged;

					if (ShouldRegisterEditableChildObject)
					{
						fJobs.UseDefaultingLogic();
						RegisterEditableChildObject(fJobs);
					}
				}

				return fJobs;
			}
		}

		public IEnumerable<Job> SelectedJobs
		{
			get
			{
				var selectedJobs = from PeriodicInvoiceSelectableJob job in Jobs
								   where job.IncludeInThePeriodicInvoice
								   select job.Parent;

				return selectedJobs;
			}
		}

		public PeriodicInvoiceMiscInvoiceCollection MiscInvoices
		{
			get
			{
				if (MiscInvoices_internal == null)
				{
					MiscInvoices_internal = new PeriodicInvoiceMiscInvoiceCollection(Factory);
					MiscInvoices_internal.OnIncludingInThePeriodicInvoiceChanged += new EventHandler<ChangedBizoEventArgs>(MiscInvoices_OnIncludingInThePeriodicInvoiceChanged);

					RegisterEditableChildObject(MiscInvoices_internal);
				}

				return MiscInvoices_internal;
			}
		}
		PeriodicInvoiceMiscInvoiceCollection MiscInvoices_internal;

		public IEnumerable<InvoicingBase> SelectedMiscInvoices
		{
			get
			{
				var selectedMiscInvoices = from InvoicingBase miscInvoice in MiscInvoices
										   where miscInvoice.IncludeInThePeriodicInvoice
										   select miscInvoice;

				return selectedMiscInvoices;
			}
		}

		public List<Charge> Charges
		{
			get { return Charges_internal ?? (Charges_internal = new List<Charge>()); }
		}
		List<Charge> Charges_internal;

		#endregion

		#region Totals

		public ZDecimal OSExTaxAmount
		{
			get
			{
				if (OSExTaxAmount_internal == null)
				{
					CalculateTotals();
				}
				return OSExTaxAmount_internal.Value;
			}
		}
		ZDecimal? OSExTaxAmount_internal;

		public ZPropertyInfo OSExTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OSExTaxAmount)); }
		}

		public ZDecimal OSTaxAmount
		{
			get
			{
				if (OSTaxAmount_internal == null)
				{
					CalculateTotals();
				}
				return OSTaxAmount_internal.Value;
			}
		}
		ZDecimal? OSTaxAmount_internal;

		public ZPropertyInfo OSTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OSTaxAmount)); }
		}

		public ZDecimal OSTotalAmount
		{
			get
			{
				if (OSTotalAmount_internal == null)
				{
					CalculateTotals();
				}
				return OSTotalAmount_internal.Value;
			}
		}
		ZDecimal? OSTotalAmount_internal;

		public ZPropertyInfo OSTotalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OSTotalAmount)); }
		}

		public ZDecimal LocalExTaxAmount
		{
			get
			{
				if (LocalExTaxAmount_internal == null)
				{
					CalculateTotals();
				}
				return LocalExTaxAmount_internal.Value;
			}
		}
		ZDecimal? LocalExTaxAmount_internal;

		public ZPropertyInfo LocalExTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalExTaxAmount)); }
		}

		public ZDecimal LocalTaxAmount
		{
			get
			{
				if (LocalTaxAmount_internal == null)
				{
					CalculateTotals();
				}
				return LocalTaxAmount_internal.Value;
			}
		}
		ZDecimal? LocalTaxAmount_internal;

		public ZPropertyInfo LocalTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalTaxAmount)); }
		}

		public ZDecimal LocalTotalAmount
		{
			get
			{
				if (LocalTotalAmount_internal == null)
				{
					CalculateTotals();
				}
				return LocalTotalAmount_internal.Value;
			}
		}
		ZDecimal? LocalTotalAmount_internal;

		public ZPropertyInfo LocalTotalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalTotalAmount)); }
		}

		public ZDecimal OSExtraTaxAmount
		{
			get
			{
				if (OSExtraTaxAmount_internal == null)
				{
					CalculateTotals();
				}
				return OSExtraTaxAmount_internal.Value;
			}
		}
		ZDecimal? OSExtraTaxAmount_internal;

		public ZPropertyInfo OSExtraTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(OSExtraTaxAmount)); }
		}

		public ZDecimal LocalExtraTaxAmount
		{
			get
			{
				if (LocalExtraTaxAmount_internal == null)
				{
					CalculateTotals();
				}
				return LocalExtraTaxAmount_internal.Value;
			}
		}
		ZDecimal? LocalExtraTaxAmount_internal;

		public ZPropertyInfo LocalExtraTaxAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalExtraTaxAmount)); }
		}

		protected virtual IEnumerable<Charge> SelectedCharges
		{
			get { return Charges.ToArray<Charge>(); }
		}

		class JobAmountsForPeriodicInvoiceDisplay
		{
			public ZDecimal OsExTaxAmount;
			public ZDecimal OsTaxAmount;
			public ZDecimal OSExtraTaxAmount;
			public ZDecimal LocalExTaxAmount;
			public ZDecimal LocalTaxAmount;
			public ZDecimal LocalExtraTaxAmount;
		}

		protected void CalculateTotals()
		{
			OSExTaxAmount_internal = 0m;
			OSTaxAmount_internal = 0m;
			OSTotalAmount_internal = 0m;
			OSExtraTaxAmount_internal = 0;

			LocalExTaxAmount_internal = 0m;
			LocalTaxAmount_internal = 0m;
			LocalTotalAmount_internal = 0m;
			LocalExtraTaxAmount_internal = 0;

			var jobTotals = new Dictionary<ZGuid, JobAmountsForPeriodicInvoiceDisplay>();

			foreach (Charge charge in SelectedCharges)
			{
				if (!jobTotals.ContainsKey(charge.JR_JH))
				{
					jobTotals.Add(charge.JR_JH, new JobAmountsForPeriodicInvoiceDisplay());
				}

				var jobtotal = jobTotals[charge.JR_JH];

				var canBillInLocalOrInvoiceCurrency = charge.BillInInvoiceCurrency || charge.BillInLocalCurrency;
				var billInThisInvoiceCurrency = charge.BillInInvoiceCurrency && charge.JR_RX_NKSellInvoiceCurrency == this.CurrencyNK;

				jobtotal.OsExTaxAmount += canBillInLocalOrInvoiceCurrency ? billInThisInvoiceCurrency ? charge.JR_OSSellInvoiceAmt : charge.JR_LocalSellInvoiceAmt : charge.JR_OSSellAmt;
				jobtotal.OsTaxAmount += canBillInLocalOrInvoiceCurrency ? billInThisInvoiceCurrency ? charge.JR_OSSellInvoiceGSTAmount : charge.JR_Sell_LocalGSTAmount : charge.JR_OSSellGSTAmt_Calc;
				jobtotal.OSExtraTaxAmount += canBillInLocalOrInvoiceCurrency ? billInThisInvoiceCurrency ? charge.JR_Calc_OSSellInvoiceExtraTaxAmt : charge.JR_Calc_LocalSellExtraTaxAmt : charge.JR_Calc_OSSellExtraTaxAmt;
				jobtotal.LocalExTaxAmount += charge.JR_LocalSellInvoiceAmt;
				jobtotal.LocalTaxAmount += billInThisInvoiceCurrency ? charge.JR_SellInvoice_LocalGSTAmount : charge.JR_Sell_LocalGSTAmount;
				jobtotal.LocalExtraTaxAmount += billInThisInvoiceCurrency ? charge.JR_Calc_LocalSellInvoiceExtraTaxAmt : charge.JR_Calc_LocalSellExtraTaxAmt;
			}

			foreach (PeriodicInvoiceSelectableJob jobForDisplay in Jobs)
			{
				JobAmountsForPeriodicInvoiceDisplay jobtotal = null;

				if (!jobTotals.TryGetValue(jobForDisplay.Parent.PK, out jobtotal))
				{
					jobtotal = new JobAmountsForPeriodicInvoiceDisplay();
				}

				jobForDisplay.JH_OSAmountForPeriodicBilling = jobtotal.OsExTaxAmount;
				jobForDisplay.JH_OSTaxAmountForPeriodicBilling = jobtotal.OsTaxAmount;
				jobForDisplay.JH_OSExtraTaxAmount = jobtotal.OSExtraTaxAmount;
				jobForDisplay.JH_LocalAmountForPeriodicBilling = jobtotal.LocalExTaxAmount;
				jobForDisplay.JH_LocalTaxAmountForPeriodicBilling = jobtotal.LocalTaxAmount;
				jobForDisplay.JH_LocalExtraTaxAmount = jobtotal.LocalExtraTaxAmount;

				var chargeTaxBranches = string.Join(",", Charges.Cast<Charge>().Where(x => x.JR_JH == jobForDisplay.Parent.PK).Select(x => x.SellTaxBranch?.GB_Code).Distinct());
				jobForDisplay.ChargeTaxBranches = chargeTaxBranches;

				OSExTaxAmount_internal += jobtotal.OsExTaxAmount;
				OSTaxAmount_internal += jobtotal.OsTaxAmount;
				OSExtraTaxAmount_internal += jobtotal.OSExtraTaxAmount;

				LocalExTaxAmount_internal += jobtotal.LocalExTaxAmount;
				LocalTaxAmount_internal += jobtotal.LocalTaxAmount;
				LocalExtraTaxAmount_internal += jobtotal.LocalExtraTaxAmount;
			}

			foreach (InvoicingBase transaction in MiscInvoices)
			{
				if (transaction.IncludeInThePeriodicInvoice)
				{
					OSExtraTaxAmount_internal += transaction.AH_OSExtraTax;
					OSExTaxAmount_internal += transaction.AH_OSExTax;
					OSTaxAmount_internal += transaction.AH_OSTax + transaction.AH_OSExtraTax;
					LocalExTaxAmount_internal += transaction.AH_InvoiceAmount;
					LocalTaxAmount_internal += transaction.AH_GSTAmount + transaction.AH_LocalExtraTax;
					LocalExtraTaxAmount_internal += transaction.AH_LocalExtraTax;
				}
			}

			OSTotalAmount_internal = OSExTaxAmount_internal + OSTaxAmount_internal;
			LocalTotalAmount_internal = LocalExTaxAmount_internal + LocalTaxAmount_internal;

			OSExTaxAmountInfo.RefreshBinding();
			OSTaxAmountInfo.RefreshBinding();
			OSTotalAmountInfo.RefreshBinding();

			LocalExTaxAmountInfo.RefreshBinding();
			LocalTaxAmountInfo.RefreshBinding();
			LocalTotalAmountInfo.RefreshBinding();

			LocalExtraTaxAmountInfo.RefreshBinding();
			OSExtraTaxAmountInfo.RefreshBinding();

			UpdateCurrency();
			ValidateOSTaxAmount();
		}

		protected void ClearTotals()
		{
			OSExTaxAmount_internal = null;
			OSTaxAmount_internal = null;
			OSTotalAmount_internal = null;

			LocalExTaxAmount_internal = null;
			LocalTaxAmount_internal = null;
			LocalTotalAmount_internal = null;

			LocalExtraTaxAmount_internal = null;
			OSExtraTaxAmount_internal = null;

			OSExTaxAmountInfo.RefreshBinding();
			OSTaxAmountInfo.RefreshBinding();
			OSTotalAmountInfo.RefreshBinding();

			LocalExTaxAmountInfo.RefreshBinding();
			LocalTaxAmountInfo.RefreshBinding();
			LocalTotalAmountInfo.RefreshBinding();

			LocalExtraTaxAmountInfo.RefreshBinding();
			OSExtraTaxAmountInfo.RefreshBinding();
		}

		#endregion

		#endregion

		#region Lines Methods

		#region Load Jobs

		protected void InitializeFilterForJobs()
		{
			JobsFilter.SetupFilter(this);
			CachedChargesQuery = null;
		}

		protected void InitializeFilterForMiscInvoices()
		{
			MiscInvoicesFilter.SetupFilter(this);
		}

		public void LoadJobs()
		{
			ClearJobs();

			InitializeFilterForJobs();

			using (ReloadChargesSuspender.GetSuspender())
			{
				LoadJobsCore();

				foreach (PeriodicInvoiceSelectableJob job in Jobs)
				{
					Factory.AddGenericJobQueryHint(typeof(GenericJob.GenericJob), job.Parent.JH_ParentID, job.Parent.JH_ParentTableCode);

					var jobParent = job.Parent.Parent as IAdditionalFetchHintsForJobParent;

					if (jobParent != null)
					{
						jobParent.LoadAdditionalFetchHints();
					}
				}
			}

			var castJobs = Jobs.Cast<PeriodicInvoiceSelectableJob>().Select(x => x.Parent);
			var loadedCharges = Factory.Load<Charge>(new ZQuery(JobChargeSchema.JR_JH, castJobs.Select(x => x.PK).ToArray()) { FetchOnlyFromLocalCache = true });
			loadedCharges.Where(x => !x.JR_AL_APLine.IsEmpty).ForEach(x => Factory.AddFetchHint(AccTransactionLinesSchema.Constants.TableName, x.JR_AL_APLine));
			loadedCharges.Where(x => !x.JR_AL_ARLine.IsEmpty).ForEach(x => Factory.AddFetchHint(AccTransactionLinesSchema.Constants.TableName, x.JR_AL_ARLine));

			var autoJRJChargePKs = RemoveJobsWithOnlyAutoJRJCharges(loadedCharges);
			castJobs.ForEach(x => Job.RefreshChargeLinesExchangeRateBinding(
				loadedCharges.Where(
					y => y.JR_JH == x.PK && !autoJRJChargePKs.Contains(y.PK))));

			ValidationHasRun = false;
			OnAfterChangeLines();
			RaiseOnChangeJobs();
		}

		protected virtual void LoadJobsCore()
		{
			Jobs.Load(GetJobsQuery);
		}

		protected void UpdateCurrency()
		{
			foreach (PeriodicInvoiceSelectableJob job in Jobs)
			{
				job.Currency = CurrencyNK;
			}
		}

		public void ClearJobs()
		{
			if (!ClearJobsSuspender.IsSuspended)
			{
				using (GetValidationSuspender())
				{
					Jobs.RemoveAll();

					ReloadCharges();

					OnAfterChangeLines();
					RaiseOnChangeJobs();
				}
			}
		}

		ZQuery GetJobsQuery
		{
			get
			{
				return JobsFilter.Filter;
			}
		}

		protected virtual ZQuery AdditionalChargeFilter
		{
			get { return new ZQuery(); }
		}

		protected void ReloadCharges()
		{
			if (!IsReloadChargesSuspended)
			{
				Charges.Clear();
				ReloadChargesCore();
				ClearTotals();
			}
		}

		protected virtual void ReloadChargesCore()
		{
			var jobPKs = (from PeriodicInvoiceSelectableJob job in Jobs where job.IncludeInThePeriodicInvoice select job.Parent.PK).Distinct();
			if (jobPKs.Any())
			{
				if (CachedChargesQuery == null)
				{
					CachedChargesQuery = JobsFilter.GetChargeQuery();
				}

				ZQuery chargesQuery = new ZQuery(CachedChargesQuery) { AllowTableValuedParameters = true };
				chargesQuery.AddToFilter(JobChargeSchema.JR_JH, jobPKs);
				AddNonAutoJRJCharge(chargesQuery);
			}
		}

		ZQuery CachedChargesQuery;

		public virtual void ReloadChargesByJob(ZGuid jobPK)
		{
			var oldCharges = Charges.Where(x => x.Job.PK == jobPK);
			foreach (var charge in oldCharges.ToList())
			{
				Charges.Remove(charge);
			}

			if (CachedChargesQuery == null)
			{
				CachedChargesQuery = JobsFilter.GetChargeQuery();
			}
			var chargesQuery = new ZQuery(CachedChargesQuery);
			chargesQuery.AddToFilter(JobChargeSchema.JR_JH, jobPK);
			chargesQuery.ReLoadExistingRows = true;
			AddNonAutoJRJCharge(chargesQuery);
			ClearTotals();
		}

		void AddNonAutoJRJCharge(ZQuery chargesQuery)
		{
			var chargesToAdd = Factory.Load<Charge>(chargesQuery);
			var chargesShouldNotAutoJRJ = chargesToAdd.Where(x => !x.IsValidForAutoRevenuePosting);
			Charges.AddRange(chargesShouldNotAutoJRJ);
		}

		void Jobs_IncludeInThePeriodicInvoiceChanged(object sender, EventArgs e)
		{
			ReloadCharges();

			bool isCalledByUserAction = sender != null;
			if (isCalledByUserAction)
			{
				Jobs_IncludeInThePeriodicInvoiceChangedCore(sender, e);
			}
			ValidationHasRun = false;
		}

		protected virtual void Jobs_IncludeInThePeriodicInvoiceChangedCore(object sender, EventArgs e)
		{
		}

		HashSet<ZGuid> RemoveJobsWithOnlyAutoJRJCharges(Charge[] charges)
		{
			var autoJRJChargePKs = new HashSet<ZGuid>();
			var jobPKs = new HashSet<ZGuid>();
			var jobPKsWithNonAutoJRJCharges = new HashSet<ZGuid>();

			foreach (var charge in charges)
			{
				jobPKs.Add(charge.JR_JH);

				if (!charge.IsValidForAutoRevenuePosting)
				{
					jobPKsWithNonAutoJRJCharges.Add(charge.JR_JH);
				}
				else
				{
					autoJRJChargePKs.Add(charge.PK);
				}
			}

			using (Jobs.SuspendListChanged())
			{
				jobPKs.ExceptWith(jobPKsWithNonAutoJRJCharges);

				foreach (var jobPKWithOnlyAutoJRJCharges in jobPKs)
				{
					Jobs.Remove(jobPKWithOnlyAutoJRJCharges);
				}
			}
			Jobs_IncludeInThePeriodicInvoiceChanged(this, EventArgs.Empty);

			return autoJRJChargePKs;
		}

		#endregion

		#region Load Miscellaneous Transactions

		public void LoadMiscInvoices()
		{
			ClearMiscInvoices();
			InitializeFilterForMiscInvoices();
			LoadMiscInvoicesCore();

			ClearTotals();

			ValidationHasRun = false;
			OnAfterChangeLines();
			RaiseOnChangeMiscInvoices();
		}

		protected virtual void LoadMiscInvoicesCore()
		{
			MiscInvoices.Load(GetMiscInvoicesQuery);
		}

		public void ClearMiscInvoices()
		{
			if (!ClearMiscInvoicesSuspender.IsSuspended)
			{
				using (GetValidationSuspender())
				{
					MiscInvoices.RemoveAll();

					ClearTotals();

					OnAfterChangeLines();
					RaiseOnChangeMiscInvoices();
				}
			}
		}

		public ZQuery GetMiscInvoicesQuery
		{
			get
			{
				ZQuery filter = MiscInvoicesFilter.Filter;

				filter.AddToFilter(TransactionTypeFilter);
				filter.AddToFilter(PaymentStatusFilter);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_AH_InvoiceStatement, null);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_JH, null);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, false);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef, string.Empty);
				filter.AddToFilter(AdditionalMiscTransactionFilter);
				filter.AddToFilter(TaxFrameworkRelatedZQueries.FilterForInvoicesWithoutTaxTranscation());
				return filter;
			}
		}

		protected virtual ZQuery AdditionalMiscTransactionFilter
		{
			get { return new ZQuery(AccTransactionHeaderSchema.AH_TransactionCategory, new ZString[] { InvoiceTypesList.Codes.FinalInvoice, InvoiceTypesList.Codes.DisbursementInvoice, ZString.Empty }); }
		}

		ZQuery TransactionTypeFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, new string[] { TransactionTypes.Invoice, TransactionTypes.AdjustmentNote, TransactionTypes.CreditNote });

				return filter;
			}
		}

		ZQuery PaymentStatusFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				filter.AddToFilter(UnpaidFilter);
				filter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_OSTotal, SQLComparisonOperator.Equal, 0m);
				return filter;
			}
		}

		ZQuery UnpaidFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				string queryText = AccTransactionHeader.AH_LocalTotalSQLFormula + " = " + AccTransactionHeaderSchema.AH_OutstandingAmount.Name;
				filter.AddFilterAndZSQLParameterCollection(queryText, null);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, null);
				return filter;
			}
		}

		void MiscInvoices_OnIncludingInThePeriodicInvoiceChanged(object sender, ChangedBizoEventArgs e)
		{
			InvoicingBase invoice = e.NewBusinessObject as InvoicingBase;
			if (invoice != null)
			{
				ZDecimal multiplier = invoice.IncludeInThePeriodicInvoice ? 1.0m : -1.0m;

				OSExTaxAmount_internal += invoice.AH_OSExTax * multiplier;
				OSTaxAmount_internal += invoice.AH_OSTax * multiplier;
				OSTotalAmount_internal += invoice.AH_OSTotal * multiplier;

				LocalExTaxAmount_internal += invoice.AH_InvoiceAmount * multiplier;
				LocalTaxAmount_internal += invoice.AH_GSTAmount * multiplier;
				LocalTotalAmount_internal += invoice.AH_LocalTotalAmount * multiplier;

				OSExTaxAmountInfo.RefreshBinding();
				OSTaxAmountInfo.RefreshBinding();
				OSTotalAmountInfo.RefreshBinding();

				LocalExTaxAmountInfo.RefreshBinding();
				LocalTaxAmountInfo.RefreshBinding();
				LocalTotalAmountInfo.RefreshBinding();
			}
			ValidationHasRun = false;
		}

		#endregion

		protected virtual void OnAfterChangeLines()
		{
		}

		#region ReloadCharges Suspenders

		protected bool IsReloadChargesSuspended
		{
			get { return ReloadChargesSuspender.IsSuspended || ReloadChargesSuspenderWithoutReloadOnResume.IsSuspended; }
		}

		protected FunctionalitySuspender ReloadChargesSuspender
		{
			get { return reloadChargesSuspender ?? (reloadChargesSuspender = new FunctionalitySuspender(() => ReloadCharges())); }
		}
		FunctionalitySuspender reloadChargesSuspender;

		internal FunctionalitySuspender ReloadChargesSuspenderWithoutReloadOnResume
		{
			get { return reloadChargesSuspenderWithoutReloadOnResume ?? (reloadChargesSuspenderWithoutReloadOnResume = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender reloadChargesSuspenderWithoutReloadOnResume;

		#endregion

		protected FunctionalitySuspender ClearJobsSuspender
		{
			get { return clearJobsSuspender ?? (clearJobsSuspender = new FunctionalitySuspender(() => ClearJobs())); }
		}
		FunctionalitySuspender clearJobsSuspender;

		protected FunctionalitySuspender ClearMiscInvoicesSuspender
		{
			get { return clearMiscInvoicesSuspender ?? (clearMiscInvoicesSuspender = new FunctionalitySuspender(() => ClearMiscInvoices())); }
		}
		FunctionalitySuspender clearMiscInvoicesSuspender;

		#endregion

		#region Validation

		public bool ValidationHasRun { get; private set; }

		protected override void RunPreSaveValidationCore()
		{
			if (fJobs != null)
			{
				foreach (PeriodicInvoiceSelectableJob job in Jobs)
				{
					if (job.IncludeInThePeriodicInvoice)
					{
						job.Parent.Reload();
					}
				}
			}

			base.RunPreSaveValidationCore();

			ValidateCurrencyNK();
			ValidateInvoiceDate();
			ValidateTotalAmount();
			ValidatePostDate();
			ValidateOSTaxAmount();
			ValidateLocalTotalAmount();

			ValidationHasRun = true;
		}

		public void ValidateOSTaxAmount()
		{
			OSTaxAmountInfo.ClearAllNotifications();

			if (OSTaxAmount != 0 && AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.Value)
			{
				OSTaxAmountInfo.AddWarning(Res.GetString("01549025-37bb-4e58-ad7b-4262c36f5c76", "This value is not precise and is for reference only. It will be recalculated during posting with higher precision due to Calculate Tax at Header Level rules."));
			}
		}

		public virtual void ValidateCurrencyNK()
		{
			CurrencyNKInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidCode(CurrencyNKInfo, Currencies);

			if (CurrencyNKInfo.Value.IsEmpty)
			{
				CurrencyNKInfo.AddError(Res.GetString("5eff82d1-8db4-46f0-8fe8-9dbab41c0ee2", "Please choose the Sell Currency to be invoiced. You have selected an invoice type that issues foreign currency invoices. You must now nominate that currency before invoicing can proceed."));
			}

			if (!CurrencyNKInfo.HasErrors())
			{
				var message = ExchangeRateCalculator.CheckInvoiceExchangeRate(this);
				if (!string.IsNullOrWhiteSpace(message))
				{
					CurrencyNKInfo.AddWarning(message);
				}
			}
		}

		public virtual void ValidateTotalAmount()
		{
			OSTotalAmountInfo.ClearAllNotifications();

			if (OSTotalAmount == 0 && ZeroBalanceValidationHelper.IsZeroOSTotalAmountInvalidForPeriodicInvoice(out string errorMessage))
			{
				OSTotalAmountInfo.AddError(errorMessage);
			}

			if (!OSTotalAmountInfo.HasErrors() && ((OSTotalAmount < 0 && LocalTotalAmount > 0) || (OSTotalAmount > 0 && LocalTotalAmount < 0)))
			{
				OSTotalAmountInfo.AddError(Res.GetString("A65B05BC-FA7B-4225-8481-FA57C2B7DB58", "OS Total Amount and Local Total Amount must be in the same sign."));
			}

			if (!OSTotalAmountInfo.HasErrors() && OSTotalAmount.IsEmpty && !LocalTotalAmount.IsEmpty)
			{
				OSTotalAmountInfo.AddError(Res.GetString("7CE39D5C-456C-4B13-9D90-E1A5B4FB05A5", "OS Total Amount cannot be zero when Local Total Amount is not zero."));
			}
		}

		public virtual void ValidateLocalTotalAmount()
		{
			LocalTotalAmountInfo.ClearAllNotifications();

			if (LocalTotalAmount.IsEmpty && !OSTotalAmount.IsEmpty)
			{
				LocalTotalAmountInfo.AddError(Res.GetString("8D0EFFCF-E8F7-478B-A715-CA351081C896", "Local Total Amount cannot be zero when OS Total Amount is not zero."));
			}
		}

		public void ValidateInvoiceDate()
		{
			InvoiceDateInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(InvoiceDateInfo);
			TypeValidation.CheckValidZDateTimeRange(InvoiceDateInfo);
			TypeValidation.CheckValidZDateTimeWithoutRange(InvoiceDateInfo);

			if (ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate())
			{
				InvoiceDateInfo.AddWarning(ARDefaultInvoiceAndPostDateCalculator.DefaultInvoiceDateReadOnlyWarningText);
			}
			CountrySpecificValidationHelper.AddWarningIfDateIsInTheFuture(InvoiceDateInfo);
			if (InvoicingPreSaveHelper.ShouldAddErrorIfInvoiceDateIsInTheFuture(GlbCompany.CurrentCompany.PK.ToGuid(), InvoiceDateInfo, typeof(PeriodicInvoiceBase)))
			{
				InvoiceDateInfo.AddError(AccountingConstants.InvoiceDateIsInTheFutureErrorMessage);
			}

			var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IInvoiceDateValidation>;
			var error = provider?.Get()?.ValidateInvoiceDate(InvoiceDate);
			if (error != null)
			{
				InvoiceDateInfo.AddError(error);
			}
		}

		public void ValidatePostDate()
		{
			PostDateInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(PostDateInfo);
			TypeValidation.CheckValidZDateTimeRange(PostDateInfo);
			TypeValidation.CheckValidZDateTimeWithoutRange(PostDateInfo);

			PeriodValidation.CheckDateFallsIntoValidPeriod(PostDateInfo);
			if (!PostDateInfo.HasErrors())
			{
				if (PostDate.Date > ZDateTime.Today)
				{
					PostDateInfo.AddError(TransactionHeaderValidation.FuturePostDateError);
				}
				else if (PostDate.Date < ZDateTime.Today)
				{
					PostDateInfo.AddWarning(TransactionHeaderValidation.PreviousPostDateWarning);
				}
			}

			if (ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate())
			{
				PostDateInfo.AddWarning(ARDefaultInvoiceAndPostDateCalculator.DefaultPostDateReadOnlyWarningText);
			}
		}

		void ValidateSelectedJobTypeCodes()
		{
			if (SelectedJobTypeCodes.Count == 0)
			{
				PostDateInfo.AddError(Res.GetString("ed9bdef8-a072-459f-84ae-51c501904c42", "At least one Job Type should be selected."));
			}
		}

		public virtual void ValidateBeforeFindingJobs()
		{
			ClearAllNotificationsIncludingChildren();
			ValidateCurrencyNK();
			ValidateInvoiceDate();
			ValidatePostDate();
			ValidateSelectedJobTypeCodes();
		}

		public virtual void ValidateBeforeFindingMiscInvoices()
		{
			ClearAllNotificationsIncludingChildren();
			ValidateCurrencyNK();
		}

		public PeriodValidationProvider PeriodValidation
		{
			get
			{
				if (PeriodValidation_innerValue == null)
				{
					PeriodValidation_innerValue = new PeriodValidationProvider(Factory);
				}

				return PeriodValidation_innerValue;
			}
		}
		PeriodValidationProvider PeriodValidation_innerValue;

		protected void ClearAllNotificationsIncludingChildren()
		{
			ClearAllNotifications();
			ClearJobs();
			ClearMiscInvoices();
		}

		#endregion

		#region Implementation

		protected ZString GetLayout(OrgHeader debtor, Job job)
		{
			var invoiceType = GetInvoiceType(debtor, job);
			return invoiceType != null ? invoiceType.PI_Type : ZString.Empty;
		}

		protected ZString GetSecondaryLayout(OrgHeader debtor, Job job)
		{
			var invoiceType = GetInvoiceType(debtor, job);
			return invoiceType != null ? invoiceType.PI_SecondaryType : ZString.Empty;
		}

		OrgInvoiceType GetInvoiceType(OrgHeader debtor, Job job)
		{
			var jobType = job?.JobType.Code ?? ZString.Empty;
			var transportMode = job?.TransportMode ?? ZString.Empty;
			var serviceDirection = job?.ServiceDirection ?? ZString.Empty;
			var serviceLevel = job?.ServiceLevel ?? ZString.Empty;

			OrgInvoiceType result = null;
			if (debtor != null && !string.IsNullOrEmpty(jobType) && debtor.CompanyData != null)
			{
				Dictionary<ZString, OrgInvoiceType> invoiceTypes = debtor.CompanyData.GetApplicableInvoiceTypes(transportMode, serviceDirection, serviceLevel, true, jobType);

				if (invoiceTypes != null && invoiceTypes.Any())
				{
					result = invoiceTypes.Values.First();
				}
			}
			return result;
		}

		#endregion

		public bool IsLocalCurrency => CurrencyNK == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

		protected abstract bool ShouldRegisterEditableChildObject { get; }

		protected abstract IEnumerable<ZString> GetJobTypesWhichHaveConfiguration();

		#region JobTypePicker
		public JobTypePicker JobTypesPicker
		{
			get { return jobTypesPicker ?? (jobTypesPicker = new JobTypePicker(this)); }
		}
		JobTypePicker jobTypesPicker;

		public ZString GetSelectedJobTypeCode(ZString selectedJobTypeDescription)
		{
			return GetJobTypeCodeFromJobTypeListDescription(selectedJobTypeDescription);
		}

		public ZString GetSelectedJobTypeDescription(ZString jobTypeCode)
		{
			return GetDescriptionForJobTypeList(jobTypeCode);
		}

		public void ValidateJobTypes()
		{
			JobTypeList_OnPairChangedCore(null);
		}

		public IEnumerable<ZString> GetInitialSelectedJobTypeCodeList()
		{
			return GetJobTypesWhichHaveConfiguration();
		}
		#endregion

	}
}
