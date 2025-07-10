using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.Business.AccChargeGLPostingOverrideLookups;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public abstract partial class TransactionLine : AccTransactionLines, AccountingSuspenders.IRunMethodSuspending, IDataExportBatchSource, ITransactionLine
	{
		public TransactionLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AL_PostToGL), ConcurrencyPolicy.Observe);
			MultipleReversingErrors = new List<ZString>();
		}

		public static readonly TransactionLineTypeDecider TypeDecider = new TransactionLineTypeDecider();

		public new abstract class Schema : AccTransactionLines.Schema
		{
			public const string AL_DBAH_OSExTaxAmount = "AL_DBAH_OSExTaxAmount";//TODO: remove
			public const string AL_DBAH_OSTaxAmount = "AL_DBAH_OSTaxAmount";
			public const string AL_LocalExTaxAmount = "AL_LocalExTaxAmount";
			public const string AL_LocalTaxAmount = "AL_LocalTaxAmount";
			public const string AL_LocalWHTAmount = "AL_LocalWHTAmount";
			public const string AL_LocalTotalAmount = "AL_LocalTotalAmount";

			public const string AL_OSExTaxAmount = "AL_OSExTaxAmount";
			public const string AL_OSTaxAmount = "AL_OSTaxAmount";
			public const string AL_OSWHTAmount = "AL_OSWHTAmount";
			public const string AL_OverseasTotal = "AL_OverseasTotal";

			public const string GenericCharge = "GenericCharge";
			public const string GenericJob = "GenericJob";

			public const string AL_Calc_LocalRXDecimals = "AL_Calc_LocalRXDecimals";
			public const string AL_Calc_RXDecimals = "AL_Calc_RXDecimals";

			public const string AL_Calc_InputGSTVATRecoverablePercentage = "AL_Calc_InputGSTVATRecoverablePercentage";

			public const string AL_OSTaxAmount_Recoverable = "AL_OSTaxAmount_Recoverable";
			public const string AL_OSTaxAmount_NotRecoverable = "AL_OSTaxAmount_NotRecoverable";
			public const string AL_LocalTaxAmount_Recoverable = "AL_LocalTaxAmount_Recoverable";
			public const string AL_LocalTaxAmount_NotRecoverable = "AL_LocalTaxAmount_NotRecoverable";
		}

		public override void OnSavingCore()
		{
			base.OnSavingCore();

			var prevIgnoreValidationSuspended = IgnoreValidationSuspended;
			using (new DisposableAction(() => IgnoreValidationSuspended = false, () => IgnoreValidationSuspended = prevIgnoreValidationSuspended))
			using (this.GetValidationSuspender())
			{
				if (AL_PostDate.IsEmpty)
				{
					AL_PostDate = ZDateTime.Now;
				}

				CalculateHighPrecisionExchangeRate();
			}
		}

		public override void Delete()
		{
			SubAccountHelper.DeleteSubAccounts(this as ISupportMultiSubAccounts);
			base.Delete();
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (AL_LineType.IsEmpty)
			{
				AL_LineType = LineType;
			}
			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new TransactionLinesFetchStrategy(this);
		}

		public List<ZString> MultipleReversingErrors { get; set; }

		public string ErrorMessageIfInvalidAL_AC()
		{
			var errorMessage = string.Empty;

			if (ChargeCode != null && ChargeCode.AC_ChargeType != Constants.ChargeType.Comment)
			{
				var glPostingAccounts = GetDefaultGLPostingAccounts();
				var expectedGLAccountPK = GetGLAccountPKFromChargeCodeAccounts(glPostingAccounts);
				if
				(
					(TransactionLineHeader == null || !TransactionLineHeader.IsReverseTransaction) &&
					(AL_AG == ZGuid.Empty || !expectedGLAccountPK.IsValid || expectedGLAccountPK != AL_AG)
				)
				{
					errorMessage = GetInvalidChargeCodeError(ChargeCode.AC_Code, AL_LineType);
				}
			}
			return errorMessage;
		}

		TransactionHeader TransactionLineHeader
		{
			get { return Factory.Load<TransactionHeader>(AL_AH); }
		}

		public string ErrorMessageIfInvalidAL_AC_AL_AG()
		{
			string errorMessage = string.Empty;

			if (!IsInDatabase)
			{
				errorMessage = ErrorMessageIfInvalidAL_AC();

				if (string.IsNullOrEmpty(errorMessage) &&
					(
						(AL_LineType == TransactionLineTypes.Revenue || AL_LineType == TransactionLineTypes.Cost)
						&& ChargeCode == null && GLHeader == null
					))
				{
					errorMessage = EmptyChargeCodeAndGLHeaderError;
				}
			}
			return errorMessage;
		}

		public static ZString GetInvalidChargeCodeError(ZString code, ZString lineType)
		{
			return Res.GetString("90ccfc59-a918-4ee6-9e44-0f37d7c7d970", "Charge Code '{0:G}' must have {1:G} GL Account entered.", code, lineType);
		}

		public static ZString EmptyChargeCodeAndGLHeaderError
		{
			get { return " " + Res.GetString("2576018e-f8cd-4609-a670-bc0519799049", "At least a Charge Code or GL Header must be entered."); }
		}

		public static ZString GetTransactionDetailsInErrorMessage(ZString ledger, ZString transactionType)
		{
			return " " + Res.GetString("f09bff79-1e6b-4bc8-8669-510a71a57a36", "Transaction details: Ledger: {0}, Transaction Type: {1}", ledger, transactionType);
		}

		public override JobHeader Job
		{
			get { return IsDeleted ? null : Factory.Load<Job>(AL_JH); }
		}

		public Job InvoicingJob
		{
			get { return Job as Job; }
		}

		public override GlbCompany Company
		{
			get
			{
				if (base.Company != null)
				{
					return base.Company;
				}
				else if (Branch != null && Branch.Company != null)
				{
					return Branch.Company;
				}
				else
				{
					return GlbCompany.CurrentCompany;
				}
			}
		}

		internal void UpdateAL_ReverseDate()
		{
			var revenueRecognitionDate = GetOriginalDateOrRevenueRecognitionDate(GetAL_ReverseDateForImmediateRevenueRecognition());
			if (revenueRecognitionDate > ZDateTime.Now)
			{
				AL_ReverseDate_FutureSystemCalculatedValue = revenueRecognitionDate;
			}

			using (GetValidationSuspender())
			{
				AL_ReverseDate = revenueRecognitionDate;
			}

			var notApplicableLineTypes = new[] { TransactionLineTypes.Accrual, TransactionLineTypes.WIP };
			if (!notApplicableLineTypes.Contains(AL_LineType.ToString()) &&
				!AL_ReverseDate.IsEmpty) // With 'OldJob' and 'JobClosure' Revenue Recognition Setup we can get empty Reverse Date. If there is already a WIP created and reversed with different Registry Setup it should not be un-reversed.
			{
				var relatedJobCharge = RelatedJobCharge;
				if (relatedJobCharge != null)
				{
					if (relatedJobCharge.JR_AL_ARLine == PK)
					{
						relatedJobCharge.UpdateLastWIPReverseDate(AL_ReverseDate);
					}
					if (relatedJobCharge.JR_AL_APLine == PK) //auto JRJ journal can link REV line to cost part so we can’t just check line types to define that line is linked to cost part
					{
						relatedJobCharge.UpdateLastAccrualReverseDate(AL_ReverseDate);
					}
				}
			}
		}

		protected virtual ZDateTime GetOriginalDateOrRevenueRecognitionDate(ZDateTime originalDate)
		{
			ZDateTime resultDate = originalDate;

			var invoicingJob = InvoicingJob;
			if (invoicingJob != null)
			{
				resultDate = ZDateTime.Empty;

				ZDateTime revenueRecognitionDate = invoicingJob.GetRevenueRecognitionDate(AL_RevRecognitionType);
				if (revenueRecognitionDate >= AccountingConstants.RevenueRecognitionDateConstants.Immediate &&
					 revenueRecognitionDate < AccountingConstants.RevenueRecognitionDateConstants.MinSpecialDate)
				{
					resultDate = originalDate;

					if (revenueRecognitionDate != AccountingConstants.RevenueRecognitionDateConstants.Immediate)
					{
						resultDate = ZDateTime.Empty;

						var periodCalculator = new AccountingPeriodCalculator(Factory, Company);
						AccPeriodManagement period = periodCalculator.GetPeriodManagementFromDate(revenueRecognitionDate);
						if (period != null)
						{
							if (period.AM_IsSubLedgerClosed)
							{
								if (AccountingConfigurationRegistry.Instance.PostIntoNextOpenPeriodWhenRecognitionPeriodClosed.Value)
								{
									AccPeriodManagement nextOpenPeriod = periodCalculator.GetNextSubLedgerOpenPeriodManagementFromDate(revenueRecognitionDate, GlbCompany.CurrentCompany.PK);
									if (nextOpenPeriod != null)
									{
										resultDate = nextOpenPeriod.AM_EndDate;
									}
								}
								else
								{
									resultDate = ZDateTime.Now;
								}
							}
							else
							{
								resultDate = revenueRecognitionDate;
							}
						}
					}
				}
			}
			return resultDate;
		}

		protected virtual ZDateTime GetAL_ReverseDateForImmediateRevenueRecognition()
		{
			return AL_PostDate;
		}

		public void CalculateHighPrecisionExchangeRate()
		{
			if (!IsInDatabase && AL_RX_NKTransactionCurrency != Company.GC_RX_NKLocalCurrency)
			{
				using (GetLocalAmountCalculationSuspender())
				{
					int decimals = AccTransactionLinesSchema.AL_ExchangeRate.Scale;
					ZDecimal highPrecisionExchangeRate = Company.GetExchangeRate().GetRate(AL_LocalExTaxAmount, AL_OSExTaxAmount, decimals);

					if (highPrecisionExchangeRate != ZDecimal.Zero)
					{
						using (ExchangeRateValidationSuspender.GetSuspender())
						{
							AL_ExchangeRate = highPrecisionExchangeRate;
						}
					}
				}
			}
		}

		internal ZDecimal GetHighPrecisionExchangeRate()
		{
			return Env.CurrentCompany.ExchangeRate.GetRate(AL_LineAmount + AL_GSTVAT, AL_OSAmount, AccTransactionLinesSchema.AL_ExchangeRate.Scale);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			shouldReCalculateOSTotalSplit = true;
			shouldSetAL_OSWHTAmount = true;
		}

		protected abstract ZString LineType { get; }

		#region Collections

		public RefCurrencyCollection CurrencyCollection
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}

		public AccChargeCodeCollection ChargeCodeCollection
		{
			get { return GetChargeCodeCollectionCore(); }
		}

		protected virtual AccChargeCodeCollection GetChargeCodeCollectionCore()
		{
			if (Department != null)
			{
				return FindboxLookupCollections.GetChargeCodeCollection(Factory, Department);
			}
			else
			{
				ZQuery result = new ZQuery();
				result.IsNoResultQuery = true;
				return new AccChargeCodeCollection(Factory, result); //NoResultQuery collection is safe to not cache as it never add any bizo and so won't sit it its parent collections
			}
		}

		public OrgHeaderCollection OrganisationsCollection
		{
			get { return GetOrganisationsCollection(); }
		}

		protected virtual OrgHeaderCollection GetOrganisationsCollection()
		{
			return FindboxLookupCollections.GetOrgHeaderCollection(Factory);
		}

		public JobCollection JobCollection
		{
			get { return GetJobCollection(); }
		}

		protected virtual JobCollection GetJobCollection()
		{
			return FindboxLookupCollections.GetJobCollection_CurrentCompanyOnly(Factory);
		}

		public GlbDepartmentCollection DepartmentCollection
		{
			get { return FindboxLookupCollections.GetDepartmentCollection_ActiveOnly(Factory); }
		}

		public AccGLHeaderCollection GLHeaderCollection
		{
			get
			{
				var glHeaderCollection = new AccGLHeaderCollection(Factory, this, ShowGLAccountsForImportAction);
				glHeaderCollection.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("ddac5068-6956-4e66-a954-b451f9f46f14", "You cannot directly post to these GL accounts."));
				return glHeaderCollection;
			}
		}

		public Action<AccGLHeaderCollection, List<AccGLHeader>> ShowGLAccountsForImportAction;

		#endregion

		#region AL_IsFinalCharge

		#endregion

		#region AL_ReverseDate_FutureSystemCalculatedValue

		public ZDateTime AL_ReverseDate_FutureSystemCalculatedValue
		{
			get
			{
				return aL_ReverseDate_FutureSystemCalculatedValue;
			}
			set
			{
				aL_ReverseDate_FutureSystemCalculatedValue = value;
			}
		}

		ZDateTime aL_ReverseDate_FutureSystemCalculatedValue = DateTime.MinValue;

		#endregion

		#region Current Log Record Fields

		public ZString CreatingUserID => CreatingUser?.GS_LoginName ?? string.Empty;

		public ZString AL_Calc_CreatingUserName => CreatingUser?.GS_FullName ?? string.Empty;

		GlbStaff CreatingUser => creatingUser ??= Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, AL_SystemCreateUser);
		GlbStaff creatingUser;

		public ZPropertyInfo AL_Calc_CreatingUserNameInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AL_Calc_CreatingUserName));
			}
		}

		public ZDateTime AL_Calc_CreatedDate => AL_SystemCreateTimeUtc.IsValid ? AL_SystemCreateTimeUtc.ToLocalBranchTime() : DateTime.MinValue;

		public ZPropertyInfo AL_Calc_CreatedDateInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AL_Calc_CreatedDate));
			}
		}

		#region AL_GC
		public ZGuid AL_Calc_GC_Company
		{
			get
			{
				if (!AL_GC.IsEmpty)
				{
					return AL_GC;
				}
				else if (Branch != null & !Branch.GB_GC.IsEmpty)
				{
					return Branch.GB_GC;
				}
				else
				{
					return GlbCompany.CurrentCompany.PK;
				}
			}
		}

		public ZPropertyInfo AL_Calc_GC_CompanyInfo
		{
			get { return GetZPropertyInfo(nameof(AL_Calc_GC_Company)); }
		}

		#endregion

		#endregion

		#region Overridden Properties

		public override ZGuid AL_AC
		{
			get
			{
				return base.AL_AC;
			}
			set
			{
				var hasChanged = AL_AC != value;

				base.AL_AC = value;

				SetAL_RevRecognitionType();

				if (hasChanged)
				{
					AL_GovtChargeCode = GetMatchedGovtChargeCode();
					if (SupportsInputTaxRecoverable)
					{
						SetVATRecoverablePercentage();
					}
				}
			}
		}

		public override ZGuid AL_AT
		{
			get { return base.AL_AT; }
			set
			{
				base.AL_AT = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateAL_InputGSTVATRecoverable();
					Validation.ValidateAL_GovtChargeCode();
				}
			}
		}

		[List("DepartmentCollection")]
		public override ZGuid AL_GE
		{
			get
			{
				return base.AL_GE;
			}
			set
			{
				base.AL_GE = value;

				AL_GEInfo.RefreshBinding();
				Validation.ValidateAL_AC();
			}
		}

		public override AccChargeCode.GLPostingAccounts GetDefaultGLPostingAccounts()
		{
			if (ChargeCode != null)
			{
				var jobType = InvoicingJob?.JobType?.Code ?? JobTypeAdditionalCodes.All;
				var direction = InvoicingJob?.Direction ?? Constants.FreightShipmentDirection.Code.All;
				var transportMode = InvoicingJob?.TransportMode ?? TransportModeAdditionalCodes.All;
				var consolContainerMode = ConsolContainerModeAdditionalCodes.All;
				var matserPaymentType = MasterPaymentTypeAdditionalCodes.All;
				var housePaymentType = HousePaymentTypeAdditionalCodes.All;

				if (jobType == JobInvoicingConsumerTypes.ShipmentCode)
				{
					var shipment = InvoicingJob.PlugInData as ForwardingShipment;
					if (shipment != null)
					{
						if (shipment.IsCollect)
						{
							housePaymentType = Constants.PaymentType.Collect;
						}
						else if (shipment.IsPrepaid)
						{
							housePaymentType = Constants.PaymentType.Prepaid;
						}
						var consol = shipment.Consols.Cast<ForwardingConsol>().OrderBy(x => x.JK_JX_JA_E_DEP).FirstOrDefault(y => y.TransportMode == InvoicingJob.TransportMode);
						if (consol != null)
						{
							consolContainerMode = consol.JK_ConsolMode;
							matserPaymentType = consol.JK_PrepaidCollect;
						}
					}
				}
				else if (jobType == JobInvoicingConsumerTypes.ForwardingConsolCode || jobType == JobInvoicingConsumerTypes.GatewayConsolCode)
				{
					var consol = InvoicingJob.PlugInData as ForwardingConsol;
					if (consol != null)
					{
						consolContainerMode = InvoicingJob.ContainerMode;
						matserPaymentType = consol.JK_PrepaidCollect;
					}
				}

				return ChargeCode.GetGLPostingAccounts(Department, TransactionHeader?.Header ?? this.Header, jobType, direction, transportMode, consolContainerMode, matserPaymentType, housePaymentType);
			}
			else
			{
				return default(AccChargeCode.GLPostingAccounts);
			}
		}

		public override ZGuid AL_AG
		{
			get
			{
				return base.AL_AG;
			}
			set
			{
				bool hasChanged = AL_AG != value;

				base.AL_AG = value;

				Validation.ValidateAL_AC();

				if (hasChanged && SupportsInputTaxRecoverable)
				{
					SetVATRecoverablePercentage();
				}
			}
		}

		protected virtual bool AL_AG_ReadOnly
		{
			get { return true; }
		}

		public override ZString AL_RX_NKTransactionCurrency
		{
			get { return base.AL_RX_NKTransactionCurrency; }
			set
			{
				base.AL_RX_NKTransactionCurrency = value;
				if (AL_OSExTaxAmount != 0m)
				{
					AL_OSExTaxAmount = RoundAmountToCurrencyDecimals(AL_OSExTaxAmount);
				}
			}
		}

		[ZUnbindableProperty()]
		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal AL_LineAmount
		{
			get { return base.AL_LineAmount; }
			set { base.AL_LineAmount = value; }
		}

		[List("JobCollection")]
		public override ZGuid AL_JH
		{
			get
			{
				return base.AL_JH;
			}
			set
			{
				var hasChanges = AL_JH != value;

				base.AL_JH = value;

				if (hasChanges)
				{
					AL_GovtChargeCode = GetMatchedGovtChargeCode();
				}

				SetAL_RevRecognitionType();
			}
		}

		#endregion

		#region ExchangeRate

		public virtual ZExchangeRate ExchangeRate
		{
			get
			{
				if (ExchangeRate_innerValue == null)
				{
					ExchangeRate_innerValue = GetNewExchangeRate();
				}
				return ExchangeRate_innerValue;
			}
		}
		protected ZExchangeRate ExchangeRate_innerValue;

		protected virtual ZExchangeRate GetNewExchangeRate()
		{
			var exRate = new ZAccExchangeRate(this, RateType, AL_ExchangeRateInfo, (ZPropertyInfoString)AL_RX_NKTransactionCurrencyInfo, AL_Calc_GC_CompanyInfo);
			exRate.IsCurrencyRequired = true;
			exRate.IsRateRequired = true;
			return exRate;
		}

		public virtual ExchangeRateType RateType =>
			LineType == TransactionLineTypes.Cost ||
			LineType == TransactionLineTypes.Accrual ||
			LineType == TransactionLineTypes.UnapprovedCost
				? ExchangeRateType.Buy : ExchangeRateType.Sell;

		public FunctionalitySuspender ExchangeRateValidationSuspender
		{
			get { return exchangeRateValidationSuspender ?? (exchangeRateValidationSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender exchangeRateValidationSuspender;

		#endregion

		#region Calculated Amount Properties

		#region Suspend Local Amount Recalculation

		protected bool IsLocalAmountRecalculationSuspended;

		public LocalAmountRecalculationSuspender GetLocalAmountCalculationSuspender()
		{
			return new LocalAmountRecalculationSuspender(this);
		}

		public class LocalAmountRecalculationSuspender : IDisposable
		{
			public LocalAmountRecalculationSuspender(TransactionLine parent)
			{
				parent.IsLocalAmountRecalculationSuspended = true;
				this.Parent = parent;
			}

			readonly TransactionLine Parent;

			void IDisposable.Dispose()
			{
				Parent.IsLocalAmountRecalculationSuspended = false;
			}
		}

		#endregion

		#region Suspend OS Amount Recalculation

		protected bool IsOSAmountRecalculationSuspended;

		public OSAmountRecalculationSuspender GetOSAmountCalculationSuspender()
		{
			return new OSAmountRecalculationSuspender(this);
		}

		public class OSAmountRecalculationSuspender : IDisposable
		{
			public OSAmountRecalculationSuspender(TransactionLine parent)
			{
				parent.IsOSAmountRecalculationSuspended = true;
				this.Parent = parent;
			}

			readonly TransactionLine Parent;

			void IDisposable.Dispose()
			{
				Parent.IsOSAmountRecalculationSuspended = false;
			}
		}

		#endregion

		#region RelatedJobCharge

		public BaseCharge RelatedJobCharge
		{
			get { return GetRelatedJobChargeCore(); }
		}
		protected BaseCharge relatedCharge_innerValue;

		protected virtual BaseCharge GetRelatedJobChargeCore()
		{
			if (JobChargeRelatedLineFilterField != null &&
				(relatedCharge_innerValue == null || relatedCharge_innerValue.IsDeleted ||
				 ((ZGuid)relatedCharge_innerValue[JobChargeRelatedLineFilterField.Name]) != PK))
			{
				if (Factory.ServiceContainer.GetService<ModifiedWIPAccrualRelatedChargeFetchHintService>() == null)
				{
					Factory.ServiceContainer.AddService(new ModifiedWIPAccrualRelatedChargeFetchHintService(Factory));
				}

				relatedCharge_innerValue = GetRelatedChargeWithCache(JobChargeRelatedLineFilterField);
			}
			return relatedCharge_innerValue != null && relatedCharge_innerValue.IsDeleted ? null : relatedCharge_innerValue;
		}

		protected BaseCharge GetRelatedChargeWithCache(SchemaColumn chargeFilterField)
		{
			var query = new ZQuery(chargeFilterField, PK);
			if (!IsInDatabase)
			{
				query.FetchOnlyFromLocalCache = true;
			}

			return Factory.LoadTop1<BaseCharge>(query);
		}

		#region Modified WIPs and Accruals RelatedCharge fetch hints Service to improve performance of the BaseWIPAccrualsCriticalValidation

		class ModifiedWIPAccrualRelatedChargeFetchHintService : IService
		{
			internal ModifiedWIPAccrualRelatedChargeFetchHintService(BusinessObjectFactory factory)
			{
				PreFetchRelatedCharges(factory);
			}

			void PreFetchRelatedCharges(BusinessObjectFactory factory)
			{
				var allLinePKs = from DataRow row in ((INeedDataSet)factory).Data.Tables[AccTransactionLinesSchema.Constants.TableName].Rows
								 where row.RowState == DataRowState.Modified
								 select new ZGuid(row[AccTransactionLinesSchema.Constants.PK]);

				if (allLinePKs.Any())
				{
					ZQuery cacheOnlyFilter = new ZQuery(AccTransactionLinesSchema.PK, allLinePKs.ToArray());
					cacheOnlyFilter.AddToFilter(AccTransactionLinesSchema.AL_LineType, new string[] { TransactionLineTypes.WIP, TransactionLineTypes.Accrual });
					cacheOnlyFilter.FetchOnlyFromLocalCache = true;
					var allLines = factory.Load<TransactionLine>(cacheOnlyFilter);

					var preFetchLines = (from TransactionLine line in allLines
										 where line.JobChargeRelatedLineFilterField != null &&
										 (line.relatedCharge_innerValue == null ||
											line.relatedCharge_innerValue.IsDeleted ||
											((ZGuid)line.relatedCharge_innerValue[line.JobChargeRelatedLineFilterField.Name] != line.PK))
										 select line);

					if (preFetchLines.Any())
					{
						foreach (SchemaColumn column in (from TransactionLine line in preFetchLines select line.JobChargeRelatedLineFilterField).Distinct())
						{
							var linePKs = (from TransactionLine line in preFetchLines where line.JobChargeRelatedLineFilterField == column select line.PK);
							if (linePKs.Any())
							{
								foreach (var linePK in linePKs)
								{
									factory.AddFetchHint(column, linePK);
								}
							}
						}
					}
				}
			}
		}

		#endregion

		protected virtual SchemaColumn JobChargeRelatedLineFilterField
		{
			get { return null; }
		}

		#endregion

		#region AL_DBAH_OSExTaxAmount
		//TODO: remove after refactoring of Scott's code
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_DBAH_OSExTaxAmount
		{
			get
			{
				int multiplier = (InvertSigns) ? 1 : -1;
				return AL_OSExTaxAmount * multiplier;
			}
		}

		public ZPropertyInfo AL_DBAH_OSExTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_DBAH_OSExTaxAmount); }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_DBAH_OSTaxAmount
		{
			get
			{
				int multiplier = this.InvertSigns ? 1 : -1;
				return (this.AL_OSTaxAmount * multiplier);
			}
		}

		public ZPropertyInfo AL_DBAH_OSTaxAmountInfo
		{
			get
			{
				return base.GetZPropertyInfo(nameof(AL_DBAH_OSTaxAmount));
			}
		}

		#endregion

		#region Local Ex Tax Amount

		[ZUnbindableProperty()]
		[DecimalPlaces(nameof(LocalDecimals))]
		public virtual ZDecimal AL_LocalExTaxAmount
		{
			get
			{
				return AL_LineAmount * Multiplier;
			}
			set
			{
				using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
				{
					if (!AL_RX_NKTransactionCurrency.IsEmpty)
					{
						if (!IsOSAmountRecalculationSuspended)
						{
							AL_OSExTaxAmount = RoundAmountToCurrencyDecimals(value, AL_ExchangeRate);
						}
					}
				}))
				{
					AL_LineAmount = (value * Multiplier);

					runMethodSuspender.RunMethod();
					UpdateAL_LocalTaxAmount();
					UpdateAL_LocalWHTAmount();

					AL_LocalExTaxAmountInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AL_LocalExTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_LocalExTaxAmount); }
		}

		#endregion

		#region Local GST Amount

		[ZUnbindableProperty()]
		[DecimalPlaces(nameof(LocalDecimals))]
		public virtual ZDecimal AL_LocalTaxAmount
		{
			get
			{
				return AL_GSTVAT * Multiplier;
			}
			set
			{
				using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
				{
					if (!AL_RX_NKTransactionCurrency.IsEmpty)
					{
						if (!IsOSAmountRecalculationSuspended)
						{
							AL_OSTaxAmount = TaxAmountCalculator.GetOSTaxAmount(Factory, AL_OSExTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), value, AL_ExchangeRate, TransactionCurrency, Company.PK);
						}
					}
				}))
				{
					AL_GSTVAT = value * Multiplier;

					runMethodSuspender.RunMethod();

					AL_LocalTaxAmountInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AL_LocalTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_LocalTaxAmount); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		internal void UpdateAL_LocalTaxAmount()
		{
			UpdateAL_LocalTaxAmountCore();
		}

		protected virtual void UpdateAL_LocalTaxAmountCore()
		{
			AL_LocalTaxAmount = CalculateLocalTaxAmount();
		}

		protected virtual ZDecimal CalculateLocalTaxAmount()
		{
			return TaxAmountCalculator.GetLocalTaxAmount(Factory, AL_GC, AL_LocalExTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), AL_OSTaxAmount, AL_ExchangeRate);
		}

		#endregion

		#region Local WHT Amount

		[ZUnbindableProperty()]
		[DecimalPlaces(nameof(LocalDecimals))]
		public virtual ZDecimal AL_LocalWHTAmount
		{
			get
			{
				return AL_WithholdingTax * Multiplier;
			}
			set
			{
				if (AL_WithholdingTax != value)
				{
					AL_WithholdingTax = value * Multiplier;
					AL_OSWHTAmount = TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalWithHoldingAmount(value, AL_ExchangeRate, TransactionCurrency);

					AL_LocalWHTAmountInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AL_LocalWHTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_LocalWHTAmount); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region Local Total Amount

		[DecimalPlaces(nameof(LocalDecimals))]
		public virtual ZDecimal AL_LocalTotalAmount
		{
			get { return AL_LocalExTaxAmount + AL_LocalTaxAmount; }
		}

		public ZPropertyInfo AL_LocalTotalAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_LocalTotalAmount); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#region ITransactionLine Members

		decimal ITransactionLine.LocalTotalAmount => AL_LocalTotalAmount;
		decimal ITransactionLine.LocalTaxAmount => AL_LocalTaxAmount;
		decimal ITransactionLine.LocalExTaxAmount => AL_LocalExTaxAmount;
		string ITransactionLine.TaxRateCode => TaxRate?.AT_Code ?? string.Empty;

		#endregion

		#endregion

		internal void UpdateAL_A9_VatClassFromTaxRate()
		{
			AL_A9_VATClass = (TaxRate != null) ? TaxRate.AT_A9_DefaultVatClass : ZGuid.Empty;
		}

		internal void UpdateAL_OSTaxAmount()
		{
			UpdateAL_OSTaxAmountCore();
		}

		protected virtual void UpdateAL_OSTaxAmountCore()
		{
			AL_OSTaxAmount = CalculateOSTaxAmount();
		}

		protected virtual ZDecimal CalculateOSTaxAmount()
		{
			return !AL_OSExTaxAmount.IsEmpty && TaxRate != null ? RoundAmountToCurrencyDecimals(TaxAmountCalculator.GetOSTaxAmount(Factory, AL_OSExTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), TransactionCurrency, Company.PK)) : ZDecimal.Zero;
		}

		protected void UpdateAL_LocalWHTAmount()
		{
			AL_LocalWHTAmount = TaxAmountCalculator.GetLocalWithholdingTaxAmountFromLocalExTaxAmount(AL_LocalExTaxAmount, Withholding);
		}

		#region OS Ex Tax Amount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSExTaxAmount
		{
			get { return AL_OSExTaxAmountCore; }
			set
			{
				UpdateAL_OSExTaxAmount(value);
			}
		}

		void UpdateAL_OSExTaxAmount(ZDecimal value)
		{
			ZDecimal roundedValue = value;
			if (RoundToZero)
			{
				roundedValue = roundedValue.Round(0);
			}
			if (value < Math.Pow(10, 15))
			{
				AL_OSExTaxAmountCore = roundedValue;
			}
			else
			{
				OSTotalSplitParts = (OSTotalSplitParts.fAL_OSTaxAmount, fAL_OSExTaxAmount: roundedValue);
			}
			if (!IsValidationSuspended)
			{
				if (LineValidation_MightBeNull != null)
				{
					LineValidation_MightBeNull.ValidateAL_OSExTaxAmount();
				}
			}
		}

		internal void RecalculatePropertiesBasedOnOSExTaxAmount()
		{
			UpdateAL_OSExTaxAmount(AL_OSExTaxAmount);
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSExTaxAmount_DBSigned => OSTotalSplitParts.fAL_OSExTaxAmount;

		protected virtual bool RoundToZero
		{
			get
			{
				return IsIcelandAndKronurOSSellCurrency &&
					AL_LineType != TransactionLineTypes.Accrual &&
					AL_LineType != TransactionLineTypes.Cost;
			}
		}

		bool IsIcelandAndKronurOSSellCurrency
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Iceland && TransactionCurrency != null && AL_RX_NKTransactionCurrency == Core.Constants.CurrencyCodes.Iceland; }
		}

		protected virtual ZDecimal AL_OSExTaxAmountCore
		{
			get { return OSTotalSplitParts.fAL_OSExTaxAmount * Multiplier; }
			set
			{
				using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
				{
					AL_LocalExTaxAmount = RoundAmountToLocalDecimals(value, AL_ExchangeRate);
				}))
				{
					AL_OverseasTotal = RoundAmountToCurrencyDecimals(value) + RoundAmountToCurrencyDecimals(AL_OSTaxAmount);
					OSTotalSplitParts = (OSTotalSplitParts.fAL_OSTaxAmount, fAL_OSExTaxAmount: value * Multiplier);

					UpdateAL_OSTaxAmount();

					OSTotalSplitParts = (OSTotalSplitParts.fAL_OSTaxAmount, fAL_OSExTaxAmount: RoundAmountToCurrencyDecimals(value * Multiplier));
					AL_OSExTaxAmountInfo.RefreshBinding();

					runMethodSuspender.RunMethod();
				}
			}
		}

		public ZPropertyInfo AL_OSExTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_OSExTaxAmount); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		public virtual bool IsCommentCharge
		{
			get { return (ChargeCode != null && ChargeCode.IsComment); }
		}

		#endregion

		#region OS Tax Amount

		bool shouldReCalculateOSTotalSplit { get; set; }

		(ZDecimal fAL_OSTaxAmount, ZDecimal fAL_OSExTaxAmount) oSTotalSplitParts { get; set; }

		(ZDecimal fAL_OSTaxAmount, ZDecimal fAL_OSExTaxAmount) OSTotalSplitParts
		{
			get
			{
				if (shouldReCalculateOSTotalSplit)
				{
					oSTotalSplitParts = TaxAmountCalculator
					  .SplitOSTotalToTaxAndExTaxAmounts(
						  AL_GSTVAT,
						  () => Company.GetExchangeRate().LocalToForeign(AL_LineAmount, AL_ExchangeRate, AL_RX_NKTransactionCurrency),
						  AL_OSAmount);

					shouldReCalculateOSTotalSplit = false;
				}
				return oSTotalSplitParts;
			}
			set => oSTotalSplitParts = value;
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public virtual ZDecimal AL_OSTaxAmount
		{
			get
			{
				return OSTotalSplitParts.fAL_OSTaxAmount * Multiplier;
			}
			set
			{
				using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this,
					delegate
					{
						var osTaxAmountFromExTaxAmount =
							TaxAmountCalculator.GetOSTaxAmount(Factory, AL_OSExTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), TransactionCurrency, Company.PK);

						AL_LocalTaxAmount = value == osTaxAmountFromExTaxAmount
							? TaxAmountCalculator.GetLocalTaxAmount(Factory, AL_GC, AL_LocalExTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), value, AL_ExchangeRate)
							: TaxAmountCalculator.GetLocalTaxAmountFromOSTaxAmount(Factory, AL_GC, value, AL_ExchangeRate);
					}))
				{
					if (value < Math.Pow(10, 15))
					{
						runMethodSuspender.RunMethod();

						AL_OverseasTotal = RoundAmountToCurrencyDecimals(value) + RoundAmountToCurrencyDecimals(AL_OSExTaxAmount);
					}
					OSTotalSplitParts = (fAL_OSTaxAmount: RoundAmountToCurrencyDecimals(value * Multiplier), OSTotalSplitParts.fAL_OSExTaxAmount);

					if (!IsValidationSuspended)
					{
						if (LineValidation_MightBeNull != null)
						{
							LineValidation_MightBeNull.ValidateAL_OSTaxAmount();
						}
					}

					AL_OSTaxAmountInfo.RefreshBinding();
				}
			}
		}

		public virtual ZPropertyInfo AL_OSTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_OSTaxAmount); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region Input VAT Recoverable

		void SetVATRecoverablePercentage()
		{
			AL_InputGSTVATRecoverable = GetInputGSTVATRecoverableDefaultValue();
		}

		internal ZDecimal GetInputGSTVATRecoverableDefaultValue()
		{
			return ChargeCode != null ? ChargeCode.AC_InputGSTVATRecoverable : new ZDecimal(1m);
		}

		[ZUnbindableProperty()]
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public override ZDecimal AL_InputGSTVATRecoverable
		{
			get { return base.AL_InputGSTVATRecoverable; }
			set
			{
				base.AL_InputGSTVATRecoverable = value;
				AL_Calc_InputGSTVATRecoverablePercentageInfo.RefreshBinding();
			}
		}

		[DecimalPlaces(nameof(PercentageDecimals))]
		public ZDecimal AL_Calc_InputGSTVATRecoverablePercentage
		{
			get { return AL_InputGSTVATRecoverable * 100m; }
			set
			{
				AL_InputGSTVATRecoverable = Enterprise.ZArchitecture.Core.Utilities.Round(value / 100m, AccTransactionLinesSchema.AL_InputGSTVATRecoverable.Scale);
				AL_Calc_InputGSTVATRecoverablePercentageInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AL_Calc_InputGSTVATRecoverablePercentageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.AL_Calc_InputGSTVATRecoverablePercentage, x => AL_InputGSTVATRecoverableInfo); }
		}

		protected bool AL_InputGSTVATRecoverable_ReadOnly
		{
			get { return !IsUserAllowedToOverrideVATRecoverablePercentage || IsTransactionHeaderReversing || IsCostWithNotOverheadChargeCode; }
		}

		internal bool IsCostWithNotOverheadChargeCode
		{
			get { return AL_LineType == TransactionLineTypes.Cost && (ChargeCode == null || ChargeCode.AC_ChargeType != Constants.ChargeType.Overhead); }
		}

		public bool SupportsInputTaxRecoverable
		{
			get { return SupportsInputVatRecoverableCore; }
		}

		internal bool IsUserAllowedToOverrideVATRecoverablePercentage
		{
			get { return OverrideInputVatRecoverableSecurityCheckPoint.IsAllowed; }
		}

		protected virtual bool SupportsInputVatRecoverableCore
		{
			get { return false; }
		}

		internal bool IsTransactionHeaderReversing
		{
			get { return IsTransactionHeaderReversingCore; }
		}

		protected virtual bool IsTransactionHeaderReversingCore
		{
			get { return false; }
		}

		protected virtual SecurityCheckpoint OverrideInputVatRecoverableSecurityCheckPoint
		{
			get { return Env.Security.None; }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSTaxAmount_Recoverable
		{
			get { return RoundAmountToCurrencyDecimals(AL_OSTaxAmount * AL_InputGSTVATRecoverable); }
		}

		public ZPropertyInfo AL_OSTaxAmount_RecoverableInfo
		{
			get { return GetZPropertyInfo(Schema.AL_OSTaxAmount_Recoverable, "OS Tax Amount Recoverable"); }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSTaxAmount_NotRecoverable
		{
			get { return AL_OSTaxAmount - AL_OSTaxAmount_Recoverable; }
		}

		public ZPropertyInfo AL_OSTaxAmount_NotRecoverableInfo
		{
			get { return GetZPropertyInfo(Schema.AL_OSTaxAmount_NotRecoverable, "OS Tax Amount Not Recoverable"); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AL_LocalTaxAmount_Recoverable
		{
			get { return Utilities.Round(AL_LocalTaxAmount * AL_InputGSTVATRecoverable, LocalDecimals); }
		}

		public ZPropertyInfo AL_LocalTaxAmount_RecoverableInfo
		{
			get { return GetZPropertyInfo(Schema.AL_LocalTaxAmount_Recoverable, "Local Tax Amount Recoverable"); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AL_LocalTaxAmount_NotRecoverable
		{
			get { return AL_LocalTaxAmount - AL_LocalTaxAmount_Recoverable; }
		}

		public ZPropertyInfo AL_LocalTaxAmount_NotRecoverableInfo
		{
			get { return GetZPropertyInfo(Schema.AL_LocalTaxAmount_NotRecoverable, "Local Tax Amount Not Recoverable"); }
		}

		#endregion

		#region OS Withholding Amount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		[ReadOnly(true)]
		public virtual ZDecimal AL_OSWHTAmount
		{
			get
			{
				if (shouldSetAL_OSWHTAmount && AL_LocalWHTAmount != 0)
				{
					aL_OSWHTAmount = TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalWithHoldingAmount(AL_LocalWHTAmount, AL_ExchangeRate, TransactionCurrency) * Multiplier;
				}
				return aL_OSWHTAmount * Multiplier;
			}
			set
			{
				if (aL_OSWHTAmount != value)
				{
					//HM 14-Jun-2018: As of current implementation AL_OSWHTAmount is readonly in GUI. That's why we do not need to propagate change to AL_LocalWHTAmount 
					//In the future it might change, this is why not deleting this code, rather commenting for easier tracking of changes
					//using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
					//	{
					//		AL_LocalWHTAmount = TaxAmountCalculator.GetLocalWithholdingTaxAmountFromOSWithHoldingAmount(value, AL_ExchangeRate);
					//	}))
					//{
					//	runMethodSuspender.RunMethod();

					aL_OSWHTAmount = RoundAmountToCurrencyDecimals(value * Multiplier);
					AL_OSWHTAmountInfo.RefreshBinding();
					//}
				}
			}
		}
		protected ZDecimal aL_OSWHTAmount;

		public ZPropertyInfo AL_OSWHTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_OSWHTAmount); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		bool shouldSetAL_OSWHTAmount;

		#endregion

		#region OS Total Amount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public virtual ZDecimal AL_OverseasTotal
		{
			get
			{
				return AL_OSAmount * Multiplier;
			}
			set
			{
				AL_OSAmount = value * Multiplier;
				AL_OverseasTotalInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo AL_OverseasTotalInfo
		{
			get { return GetZPropertyInfo(Schema.AL_OverseasTotal); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region AL_Calc_LocalRXDecimals

		public ZInt AL_Calc_LocalRXDecimals
		{
			get
			{
				RefCurrency companyCurrency = Company.LocalCurrency;
				return (companyCurrency != null ? companyCurrency.Decimals : 2);
			}
		}

		public ZPropertyInfo AL_Calc_LocalRXDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(AL_Calc_LocalRXDecimals)); }
		}

		#endregion

		#region AL_Calc_RXDecimals

		public ZInt AL_Calc_RXDecimals
		{
			get
			{
				return (TransactionCurrency != null ? TransactionCurrency.Decimals : 2);
			}
		}

		public ZPropertyInfo AL_Calc_RXDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(AL_Calc_RXDecimals)); }
		}

		#endregion

		#region AL_GovtChargeCode

		public virtual bool IsGovtChargeCodeApplicable
		{
			get { return false; }
		}

		ZString GetMatchedGovtChargeCode()
		{
			if (!IsGovtChargeCodeApplicable || !AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value || ChargeCode == null)
			{
				return ZString.Empty;
			}

			var matchedGovtChargeCode = ChargeCode.AC_GovtChargeCode;
			if (InvoicingJob != null)
			{
				if (CostLineTypes.Contains(AL_LineType))
				{
					matchedGovtChargeCode = ChargeCode.GetFallbackGovtChargeCode(InvoicingJob.GetConfigurationMatcherParameters(CostSell.Cost));
				}
				else if (RevenueLineTypes.Contains(AL_LineType))
				{
					matchedGovtChargeCode = ChargeCode.GetFallbackGovtChargeCode(InvoicingJob.GetConfigurationMatcherParameters(CostSell.Revenue));
				}
			}

			return matchedGovtChargeCode;
		}

		#endregion

		#endregion

		#region Validation - using New Validation

		protected override AccTransactionLinesValidation GetNewValidation()
		{
			if (IsValidationSuspended) //keep this IF the first always to avoid any any db hits and calculations for a case when validation will not be used.
			{
				return GetEmptyValidation();//Return empty validation as just some not null value as actual validation calls will be skipped anyway.
			}
			else if (IsInDatabase && (TransactionLineHeader != null &&
				(TransactionLineHeader.IsTransactionInDatabaseReadOnly
				|| TransactionLineHeader.PeriodCalculator.IsPeriodGLClosed(TransactionLineHeader.PostPeriod))))
			{
				return GetEmptyValidation();
			}
			else
			{
				return GetNewValidationCore();
			}
		}

		protected virtual AccTransactionLinesValidation GetNewValidationCore()
		{
			return new TransactionLineValidation(this);
		}

		protected virtual AccTransactionLinesValidation GetEmptyValidation()
		{
			return new TransactionLineEmptyValidation(this);
		}

		protected internal TransactionLineValidation LineValidation_MightBeNull
		{
			get { return Validation as TransactionLineValidation; }
		}

		#endregion

		#region Implementation

		protected abstract bool InvertSigns { get; }

		protected ZInt Multiplier
		{
			get { return InvertSigns ? -1 : 1; }
		}

		protected virtual ZDecimal RoundAmountToLocalDecimals(ZDecimal amount, ZDecimal exchangeRate)
		{
			return Env.CurrentCompany.ExchangeRate.ForeignToLocal(amount, exchangeRate);
		}

		protected virtual ZDecimal RoundAmountToCurrencyDecimals(ZDecimal amount, ZDecimal exchangeRate)
		{
			return Env.CurrentCompany.ExchangeRate.LocalToForeign(amount, exchangeRate, AL_RX_NKTransactionCurrency);
		}

		protected virtual ZDecimal RoundAmountToCurrencyDecimals(ZDecimal value)
		{
			if (value.IsEmpty)
			{
				return value;
			}

#if DEBUG
			if (Globals.IsTest)
			{
				RoundAmountToCurrencyDecimals_CallsCount_ForTestOnly++;
			}
#endif

			return (ZDecimal)Utilities.Round(value, CurrencyDecimals);
		}

#if DEBUG
		public int RoundAmountToCurrencyDecimals_CallsCount_ForTestOnly;
#endif

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AL_LineType = LineType;
			AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AL_GB = GlbBranch.CurrentBranch.PK;
			AL_GE = GlbDepartment.CurrentDepartment.PK;
			if (AL_ExchangeRate != 1m)
			{
				AL_ExchangeRate = 1m;
			}
			AL_RevRecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate;
		}

		internal void CalculateRevenueRecognitionTypeIfEmpty()
		{
			if (string.IsNullOrEmpty(AL_RevRecognitionType))
			{
				SetAL_RevRecognitionType();
			}
		}

		void SetAL_RevRecognitionType()
		{
			var oldType = AL_RevRecognitionType;
			AL_RevRecognitionType = ChargeCode == null || InvoicingJob == null ? RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate : InvoicingJob.GetRevenueRecognitionType(ChargeCode);

			if (oldType != AL_RevRecognitionType && AL_RevRecognitionType == string.Empty)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.RevenueRecognitionTypeSetFromValidToEmpty, () => System.Environment.StackTrace);
			}
		}

		public override ZString AL_RevRecognitionType
		{
			get => base.AL_RevRecognitionType;
			set
			{
				var oldValue = base.AL_RevRecognitionType;
				base.AL_RevRecognitionType = value;
				if (oldValue != value)
				{
					Validation.ValidateAL_JH();
				}
			}
		}

		#endregion

		#region posting groups

		public ZShort PostingGroupID
		{
			get { return TaxRate != null ? TaxRate.AT_PostingGroupId : (ZShort)AccTaxRate.DefaultPostingGroupID; }
		}

		#endregion

		#region IRunMethodSuspending Members

		bool AccountingSuspenders.IRunMethodSuspending.RunMethodSuspended
		{
			get;
			set;
		}

		#endregion

		#region IDataExportBatchSource Members

		ZBool IDataExportBatchSource.IsDataExportBatchSupported
		{
			get
			{
				ZBool result = false;

				if (AL_LineType == TransactionLineTypes.Accrual || AL_LineType == TransactionLineTypes.WIP)
				{
					result = true;
				}

				return result;
			}
		}

		public DataExportBatchDependentCollection DataExportBatchCollection
		{
			get
			{
				if (relatedBatchCollection == null)
				{
					relatedBatchCollection = new DataExportBatchDependentCollection(this);
					relatedBatchCollection.Load();
				}
				return relatedBatchCollection;
			}
		}

		DataExportBatchDependentCollection relatedBatchCollection;

		#endregion

		#region AL_Calc_DisplayAmount

		[DecimalPlaces(0)]
		public virtual ZDecimal DisplayMultiplier => 1m;

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_Calc_DisplayAmount
		{
			get
			{
				return AL_OSExTaxAmount * DisplayMultiplier;
			}
		}

		public ZPropertyInfo AL_Calc_DisplayAmountInfo
		{
			get { return GetZPropertyInfo(nameof(AL_Calc_DisplayAmount)); }
		}

		#endregion

		#region PlacesOfSupply

		[List(nameof(Lookups) + "." + nameof(AccTransactionLinesLookups.PlacesOfSupply))]
		[ResourceStringData("e8a82716-bff7-4430-a28f-ad6fad6d2c57", ShortCaption = "FPOS", Caption = "Place of Supply")]
		public override ZString AL_PlaceOfSupply
		{
			get => base.AL_PlaceOfSupply;
			set
			{
				if (AL_PlaceOfSupply != value)
				{
					base.AL_PlaceOfSupply = value;
					AL_PlaceOfSupplyType = PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(Company, AL_PlaceOfSupply);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AccTransactionLinesLookups.PlaceOfSupplyTypes))]
		[ResourceStringData("3133a1b8-d6b6-4c3a-95b8-bc16be1db164", ShortCaption = "FPOS Type", Caption = "Place of Supply Type")]
		public override ZString AL_PlaceOfSupplyType
		{
			get => base.AL_PlaceOfSupplyType;
			set
			{
				if (AL_PlaceOfSupplyType != value)
				{
					base.AL_PlaceOfSupplyType = value;
					Validation.ValidateAL_PlaceOfSupply();
				}
			}
		}

		public ILocation PlaceOfSupplyLocation => PlaceOfSupplyHelper.TryConvertToLocation(Company, AL_PlaceOfSupply);

		#endregion

		#region Supply Type

		[List(nameof(Lookups) + "." + nameof(AccTransactionLinesLookups.SupplyTypes))]
		public override ZString AL_SupplyType
		{
			get => base.AL_SupplyType;
			set
			{
				if (AL_SupplyType != value)
				{
					base.AL_SupplyType = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateAL_SupplyType();
					}
				}
			}
		}

		#endregion
	}
}
