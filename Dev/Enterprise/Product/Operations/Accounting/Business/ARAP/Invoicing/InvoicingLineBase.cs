using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing.FetchStrategies;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComponentModel;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.Accounting.Business.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;
using AccGenericChargeCollection = Enterprise.Accounting.Business.GenericCharge.GenericChargeCollection;
using AccGenericJobHeader = Enterprise.Accounting.Business.GenericJob.GenericJob;
using AccGenericJobHeaderCollection = Enterprise.Accounting.Business.GenericJob.GenericJobCollection;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	[PropertyDescriptorCollection(typeof(LineMatchingPropertyDescriptorCollection))]
	public abstract partial class InvoicingLineBase : DependentTransactionLine,
		IDisposable,
		IGenericChargeCollectionRequired,
		IReceivablesTaxAmountCalculation,
		ISecurityOverrideProviderSource,
		ISupportDataImporting,
		ILineMatching,
		IComplianceDocumentHeaderDetail,
		IDataVersionLoggingSupported,
		IComplianceInfoToImport,
		IDescriptionSetter,
		ITransactionLineTaxDate
	{
		#region Schema

		public new abstract class Schema : DependentTransactionLine.Schema
		{
			public const string ChargeTypeWithOverride = "ChargeTypeWithOverride";
			public const string GSTInclusiveAmount = "GSTInclusiveAmount";
			public const string JobLocalClient = "JobLocalClient";
			public const string JobOverseasAgent = "JobOverseasAgent";
			public const string ComplianceDocumentNumber = "ComplianceDocumentNumber";
			public const string ComplianceSubType = "ComplianceSubType";
			public const string ComplianceDocumentOrganization = "ComplianceDocumentOrganization";
			public const string ComplianceDocumentVATRegistrationNum = "ComplianceDocumentVATRegistrationNum";
			public const string ComplianceDocumentDate = "ComplianceDocumentDate";
			public const string ComplianceDocumentReportingPeriod = "ComplianceDocumentReportingPeriod";
			public const string ComplianceDocumentSupportingReason = "ComplianceDocumentSupportingReason";
			public const string ComplianceSupportingDocumentType = "ComplianceSupportingDocumentType";
			public const string ComplianceSupportingDocumentNumber = "ComplianceSupportingDocumentNumber";
			public const string CreateComplianceDocumentRecordOnPosting = "CreateComplianceDocumentRecordOnPosting";
			public const string PeriodApportionmentMethod = "PeriodApportionmentMethod";
			public const string PeriodStartDate = "PeriodStartDate";
			public const string PeriodEndDate = "PeriodEndDate";
			public const string PeriodClearingGLAccountPK = "PeriodClearingGLAccountPK";
		}

		#endregion

		public InvoicingLineBase(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			InvoicingLineTaxDateCacheProvider = new InvoicingLineTaxDateCacheProvider(this, () => InvoiceBase?.InvoiceTaxDateCacheProvider);
		}

		public ZGuid CopiedFromPK { get; set; }

		public void CopyValuesFrom(InvoicingLineBase line)
		{
			SuspendAL_JHSettingDefaults();

			try
			{
				base.CopyPersistentValuesFrom(line);
				AL_GB = line.AL_GB;
				AL_GE = line.AL_GE;
			}
			finally
			{
				ResumeAL_JHSettingDefaults();
			}
		}

		public bool MarkForWarningAsCreditorOrLoginCompanyIsNotTaxRegisteredForTaxedTransaction { get; set; }

		public (ZString TaxAmountErroeMessage, ZGuid TaxRatePK, ZDecimal OSTaxAmount, ZDecimal TaxRateCalc) TaxAmountErrorDetailForInterCompanyInvoiceImport { get; set; }

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new InvoicingLineBaseFetchStrategy(this);
		}

		#region IComplianceDocumentHeaderDetail

		ZString IComplianceDocumentHeaderDetail.DocumentSubType => ComplianceSubType;

		ZString IComplianceDocumentHeaderDetail.DocumentNumber => ComplianceDocumentNumber;

		ZDateTime IComplianceDocumentHeaderDetail.DocumentDate => ComplianceDocumentDate;

		ZInt IComplianceDocumentHeaderDetail.DocumentReportingPeriod => ComplianceDocumentReportingPeriod;

		ZString IComplianceDocumentHeaderDetail.DocumentLedger => InvoiceBase.AH_Ledger;

		ZGuid IComplianceDocumentHeaderDetail.DocumentPK => InvoiceBase.GetTransactionGeneratedComplianceDocument()?.PK ?? ZGuid.Empty;

		ZGuid IComplianceDocumentHeaderDetail.DocumentCompany =>
			InvoiceBase.Company.PK;

		ZString IComplianceDocumentHeaderDetail.DocumentTransactionType => InvoiceBase.AH_TransactionType;

		#endregion

		#region Compliance

		AccComplianceDocumentHeader fComplianceDocumentHeader;
		public AccComplianceDocumentHeader ComplianceDocumentHeader
		{
			get
			{
				if (fComplianceDocumentHeader == null)
				{
					var query = new ZDBOnlyQuery(typeof(AccComplianceDocumentHeader));
					var subQuery = new ZDBOnlySubQuery(typeof(AccComplianceDocumentLine), AccComplianceDocumentLineSchema.ADL_ADH);
					var subQuery1 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_ADL);
					subQuery1.AddToFilter(AccComplianceDocumentPivotSchema.ADP_AL, PK);

					subQuery.AddSubQuery(AccComplianceDocumentLineSchema.PK, subQuery1, JoinCondition.And);
					query.AddSubQuery(AccComplianceDocumentHeaderSchema.PK, subQuery, JoinCondition.And);
					query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_DocumentStatus, SQLComparisonOperator.NotEqual, ComplianceDocumentStatus.Voided);
					fComplianceDocumentHeader = Factory.LoadTop1<AccComplianceDocumentHeader>(query);
				}

				return fComplianceDocumentHeader;
			}
		}

		[ReadOnlyMember(nameof(IsComplianceDocumentRelatedPropertiesReadOnly))]
		[MaxLength(AccComplianceDocumentHeader.Schema.ADH_DocumentNumberMaxLength)]
		public ZString ComplianceDocumentNumber
		{
			get
			{
				if (fComplianceDocumentNumber.IsEmpty)
				{
					fComplianceDocumentNumber = ComplianceDocumentHeader != null ? ComplianceDocumentHeader.ADH_DocumentNumber : ZString.Empty;
				}
				return fComplianceDocumentNumber;
			}
			set
			{
				CheckMaximumLength(ComplianceDocumentNumberInfo, value);
				fComplianceDocumentNumber = value;
				ComplianceDocumentNumberInfo.RefreshBinding();

				if ((InvoiceBase?.HasPCDSettingForAP ?? false) && CreateComplianceDocumentRecordOnPosting)
				{
					var validation = this.Validation as InvoicingLineBaseValidation;
					validation?.ValidateComplianceDocumentVATRegistrationNum();
					validation?.ValidateComplianceDocumentDate();
					validation?.ValidateComplianceSupportingDocumentType();
					validation?.ValidateComplianceDocumentSupportingReason();
					validation?.ValidateComplianceSupportingDocumentNumber();
					validation?.ValidateComplianceDocumentReportingPeriod();
					validation?.ValidateComplianceSubType();
					validation?.ValidateComplianceDocumentOrganization();
				}
			}
		}
		ZString fComplianceDocumentNumber;

		public ZPropertyInfo ComplianceDocumentNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceDocumentNumber); }
		}

		[ReadOnlyMember(nameof(IsComplianceDocumentRelatedPropertiesReadOnly))]
		[MaxLength(AccComplianceDocumentHeader.Schema.ADH_ComplianceSubTypeMaxLength)]
		[List("ComplianceSubTypeInLocalLanguageList")]
		public ZString ComplianceSubType
		{
			get
			{
				if (fComplianceSubType.IsEmpty)
				{
					fComplianceSubType = ComplianceDocumentHeader != null ? ComplianceDocumentHeader.ADH_ComplianceSubType : ZString.Empty;
				}
				return fComplianceSubType;
			}
			set
			{
				CheckMaximumLength(ComplianceSubTypeInfo, value);
				fComplianceSubType = value;
				ComplianceSubTypeInfo.RefreshBinding();
			}
		}
		ZString fComplianceSubType;

		public ZPropertyInfo ComplianceSubTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceSubType); }
		}

		public ICodeDescriptionPairList ComplianceSubTypeInLocalLanguageList
		{
			get
			{
				return AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCodeInLocalLanguage(GlbCompany.CurrentCompany.Country.Code);
			}
		}

		[ReadOnlyMember(nameof(IsComplianceDocumentRelatedPropertiesReadOnly))]
		[List("ComplianceOrganization")]
		public ZGuid ComplianceDocumentOrganization
		{
			get
			{
				return fComplianceDocumentOrganization;
			}
			set
			{
				if (fComplianceDocumentOrganization != value)
				{
					fComplianceDocumentOrganization = value;
					var organisation = Factory.Load<OrgHeader>(fComplianceDocumentOrganization);
					var orgVATRegistrationNum = organisation?.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == Country.GetConsumptionTaxRegistrationOrgCusCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) && x.OK_RN_NKCodeCountry == GlbCompany.CurrentCompany.GC_RN_NKCountryCode)?.OK_CustomsRegNo ?? ZString.Empty;
					ComplianceDocumentVATRegistrationNum = orgVATRegistrationNum;
					ComplianceDocumentOrganizationInfo.RefreshBinding();
				}
			}
		}
		ZGuid fComplianceDocumentOrganization;

		public ZPropertyInfo ComplianceDocumentOrganizationInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceDocumentOrganization, "Compliance Document Organization"); }
		}

		[ReadOnlyMember(nameof(IsComplianceDocumentRelatedPropertiesReadOnly))]
		[MaxLength(AccComplianceDocumentHeader.Schema.ADH_VATRegistrationNumberOverrideMaxLength)]
		public ZString ComplianceDocumentVATRegistrationNum
		{
			get
			{
				return fComplianceDocumentVATRegistrationNum;
			}
			set
			{
				CheckMaximumLength(ComplianceDocumentVATRegistrationNumInfo, value);
				fComplianceDocumentVATRegistrationNum = value;
				ComplianceDocumentVATRegistrationNumInfo.RefreshBinding();
			}
		}
		ZString fComplianceDocumentVATRegistrationNum;

		public ZPropertyInfo ComplianceDocumentVATRegistrationNumInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceDocumentVATRegistrationNum, "VAT Registration Number"); }
		}

		[ReadOnlyMember(nameof(IsComplianceDocumentRelatedPropertiesReadOnly))]
		public ZDateTime ComplianceDocumentDate
		{
			get
			{
				return fComplianceDocumentDate;
			}
			set
			{
				fComplianceDocumentDate = value;
				ComplianceDocumentDateInfo.RefreshBinding();
			}
		}
		ZDateTime fComplianceDocumentDate;

		public ZPropertyInfo ComplianceDocumentDateInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceDocumentDate, "Document Date"); }
		}

		[ReadOnlyMember(nameof(IsComplianceDocumentRelatedPropertiesReadOnly))]
		public ZInt ComplianceDocumentReportingPeriod
		{
			get
			{
				return fComplianceDocumentReportingPeriod;
			}
			set
			{
				fComplianceDocumentReportingPeriod = value;
				ComplianceDocumentReportingPeriodInfo.RefreshBinding();
			}
		}
		ZInt fComplianceDocumentReportingPeriod;

		public ZPropertyInfo ComplianceDocumentReportingPeriodInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceDocumentReportingPeriod, "Reporting Period"); }
		}

		[ReadOnlyMember(nameof(IsComplianceDocumentRelatedPropertiesReadOnly))]
		[MaxLength(AccComplianceDocumentHeader.Schema.ADH_SupportingReasonMaxLength)]
		[List("SupportingReasonCodesList")]
		public ZString ComplianceDocumentSupportingReason
		{
			get
			{
				return fComplianceDocumentSupportingReason;
			}
			set
			{
				CheckMaximumLength(ComplianceDocumentSupportingReasonInfo, value);
				fComplianceDocumentSupportingReason = value;
				ComplianceDocumentSupportingReasonInfo.RefreshBinding();
			}
		}
		ZString fComplianceDocumentSupportingReason;

		public ZPropertyInfo ComplianceDocumentSupportingReasonInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceDocumentSupportingReason, "Supporting Reason"); }
		}

		[ReadOnlyMember(nameof(IsComplianceDocumentRelatedPropertiesReadOnly))]
		[MaxLength(AccComplianceDocumentHeader.Schema.ADH_SupportingDocumentTypeMaxLength)]
		[List("SupportingDocumentType")]
		public ZString ComplianceSupportingDocumentType
		{
			get
			{
				return fComplianceSupportingDocumentType;
			}
			set
			{
				CheckMaximumLength(ComplianceSupportingDocumentTypeInfo, value);
				fComplianceSupportingDocumentType = value;
				ComplianceSupportingDocumentTypeInfo.RefreshBinding();
			}
		}
		ZString fComplianceSupportingDocumentType;

		public ZPropertyInfo ComplianceSupportingDocumentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceSupportingDocumentType, "Supporting Doc Type"); }
		}

		[ReadOnlyMember(nameof(IsComplianceDocumentRelatedPropertiesReadOnly))]
		[MaxLength(AccComplianceDocumentHeader.Schema.ADH_SupportingDocumentNumberMaxLength)]
		public ZString ComplianceSupportingDocumentNumber
		{
			get
			{
				return fComplianceSupportingDocumentNumber;
			}
			set
			{
				CheckMaximumLength(ComplianceSupportingDocumentNumberInfo, value);
				fComplianceSupportingDocumentNumber = value;
				ComplianceSupportingDocumentNumberInfo.RefreshBinding();
			}
		}
		ZString fComplianceSupportingDocumentNumber;

		public ZPropertyInfo ComplianceSupportingDocumentNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceSupportingDocumentNumber, "Supporting Doc Number"); }
		}

		public ZBool CreateComplianceDocumentRecordOnPosting
		{
			get
			{
				return fCreateComplianceDocumentRecordOnPosting;
			}
			set
			{
				fCreateComplianceDocumentRecordOnPosting = value;
				if (!fCreateComplianceDocumentRecordOnPosting)
				{
					ClearComplianceDocumentDetailData();
				}
				CreateComplianceDocumentRecordOnPostingInfo.RefreshBinding();
			}
		}
		ZBool fCreateComplianceDocumentRecordOnPosting;

		void ClearComplianceDocumentDetailData()
		{
			ComplianceDocumentNumber = ZString.Empty;
			ComplianceSupportingDocumentNumber = ZString.Empty;
			ComplianceSubType = ZString.Empty;
			ComplianceDocumentOrganization = ZGuid.Empty;
			ComplianceDocumentVATRegistrationNum = ZString.Empty;
			ComplianceDocumentDate = ZDateTime.Empty;
			ComplianceDocumentReportingPeriod = ZInt.Zero;
			ComplianceDocumentSupportingReason = ZString.Empty;
			ComplianceSupportingDocumentType = ZString.Empty;
			var validation = this.Validation as InvoicingLineBaseValidation;
			ComplianceDocumentNumber = ZString.Empty;
			ComplianceSubType = ZString.Empty;
			validation?.ValidateComplianceDocumentNumber();
			validation?.ValidateComplianceSubType();
		}

		public bool IsComplianceDocumentRelatedPropertiesReadOnly => !CreateComplianceDocumentRecordOnPosting;

		public ZPropertyInfo CreateComplianceDocumentRecordOnPostingInfo
		{
			get { return GetZPropertyInfo(Schema.CreateComplianceDocumentRecordOnPosting, "Create Compliance Document Record On Posting"); }
		}

		#endregion

		public ICodeDescriptionPairList SupportingReasonCodesList
		{
			get
			{
				return AccComplianceDocumentHeaderLookups.GetSupportingReasonCodesList(LedgerTypes.AccountsPayable);
			}
		}
		public ICodeDescriptionPairList SupportingDocumentType => AccComplianceDocumentHeaderLookups.SupportingDocumentTypeList;

		#region JobChargeTarget fields

		public ZString AL_Calc_RelatedJobNumber => AL_Calc_RelatedJob.JobNumber;

		public ZPropertyInfo AL_Calc_RelatedJobNumberInfo => GetZPropertyInfo(nameof(AL_Calc_RelatedJobNumber));

		internal ZGuid AL_Calc_RelatedJobPK => AL_Calc_RelatedJob.JobPK;

		(ZGuid JobPK, ZString JobNumber) AL_Calc_RelatedJob
		{
			get
			{
				(ZGuid, ZString) relatedJob = default;
				if (IsInDatabase)
				{
					relatedJob = ThisAsILineMatching.Charge != null ? (ThisAsILineMatching.Charge.RelatedJobID, ThisAsILineMatching.Charge.JR_Calc_RelatedJobNumber) : default;
				}
				else if (IsPopulatedFromImportedJobCharge)
				{
					relatedJob = OriginalJobCharge is BaseCharge baseCharge ? (baseCharge.RelatedJobID, baseCharge.JR_Calc_RelatedJobNumber) : default;
				}
				else if (IsConvertedFromARInvoice)
				{
					relatedJob = RelatedJobFromIntercompanyInvoiceImport;
				}
				return relatedJob;
			}
		}

		public ZGuid TargetJobIDFromIntercompanyInvoiceImport
		{
			get => targetJobIDFromIntercompanyInvoiceImport;
			set
			{
				if (IsConvertedFromARInvoice)
				{
					targetJobIDFromIntercompanyInvoiceImport = value;
				}
			}
		}
		ZGuid targetJobIDFromIntercompanyInvoiceImport;

		public (ZGuid JobPk, ZString JobNumber) RelatedJobFromIntercompanyInvoiceImport
		{
			get => relatedJobFromIntercompanyInvoiceImport;
			set
			{
				if (IsConvertedFromARInvoice)
				{
					relatedJobFromIntercompanyInvoiceImport.JobPk = value.JobPk;
					relatedJobFromIntercompanyInvoiceImport.JobNumber = value.JobNumber;
					OriginalInvoicingJobAndRelatedJob = (AL_JH, relatedJobFromIntercompanyInvoiceImport.JobPk, relatedJobFromIntercompanyInvoiceImport.JobNumber);
					AL_Calc_RelatedJobNumberInfo.RefreshBinding();
				}
			}
		}
		(ZGuid JobPk, ZString JobNumber) relatedJobFromIntercompanyInvoiceImport;

		(ZGuid InvoicingJobPk, ZGuid RelatedJobPk, ZString RelatedJobNumber) OriginalInvoicingJobAndRelatedJob
		{
			get => originalInvoicingJobAndRelatedJob;
			set
			{
				if (!isOriginalInvoicingJobAndRelatedJobSet)
				{
					originalInvoicingJobAndRelatedJob.InvoicingJobPk = value.InvoicingJobPk;
					originalInvoicingJobAndRelatedJob.RelatedJobPk = value.RelatedJobPk;
					originalInvoicingJobAndRelatedJob.RelatedJobNumber = value.RelatedJobNumber;
					isOriginalInvoicingJobAndRelatedJobSet = true;
				}
			}
		}
		(ZGuid InvoicingJobPk, ZGuid RelatedJobPk, ZString RelatedJobNumber) originalInvoicingJobAndRelatedJob;
		bool isOriginalInvoicingJobAndRelatedJobSet;

		void HandleInvoicingJobChangeForRelatedJob()
		{
			if (isOriginalInvoicingJobAndRelatedJobSet)
			{
				if (OriginalInvoicingJobAndRelatedJob.InvoicingJobPk == AL_JH)
				{
					RelatedJobFromIntercompanyInvoiceImport = (OriginalInvoicingJobAndRelatedJob.RelatedJobPk, OriginalInvoicingJobAndRelatedJob.RelatedJobNumber);
				}
				else
				{
					RelatedJobFromIntercompanyInvoiceImport = default;
				}
			}
		}

		#endregion

		#region Consol

		internal void SetConsolID(JobConsolCost jobConsolCost)
		{
			this.consolID = jobConsolCost != null ? Tuple.Create(jobConsolCost.E6_ParentID, jobConsolCost.E6_ParentTableCode) : null;
		}
		Tuple<ZGuid, ZString> consolID;

		public Tuple<ZGuid, ZString> GetConsolID()
		{
			return this.consolID;
		}

		public IJobCostingPlugIn Consol
		{
			get
			{
				IJobCostingPlugIn consol = null;

				if (consolID != null)
				{
					consol = Factory.Load(consolID.Item2, consolID.Item1) as IJobCostingPlugIn;
				}

				return consol;
			}
		}

		[MaxLength(10)]
		public ZString ConsolIDFromApportionedCharge
		{
			get
			{
				var consolIDFromApportionedCharge = ZString.Empty;
				if (IsInDatabase && consolID == null)
				{
					var relatedCharge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, PK));
					if (relatedCharge != null && relatedCharge.ParentConsolCost != null)
					{
						SetConsolID(relatedCharge.ParentConsolCost);
					}
				}
				var lineConsol = Consol;
				if (lineConsol != null)
				{
					consolIDFromApportionedCharge = lineConsol.JK_UniqueConsignRef;
				}

				return consolIDFromApportionedCharge;
			}
		}

		public ZPropertyInfo ConsolIDFromApportionedChargeInfo
		{
			get { return GetZPropertyInfo(nameof(ConsolIDFromApportionedCharge)); }
		}

		#endregion

		#region Overridden Properties & Methods

		protected override void DefaultGLAccounts()
		{
			base.DefaultGLAccounts();
			PeriodApportionment.DefaultClearingAccount();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			PeriodApportionment.DefaultClearingAccount();
		}

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (InvoiceBase == null || InvoiceBase.IsDeleted || (InvoiceBase.AH_Ledger != LedgerTypes.IncompleteTransactions && InvoiceBase.AH_Ledger != LedgerTypes.TransactionsPendingAllocation)); }
		}

		protected override void OnFactorySavingBeforeTransactionCore2()
		{
			base.OnFactorySavingBeforeTransactionCore2();
			if ((ZString)AL_DescInfo.OriginalValue != AL_Desc)
			{
				LogLineDescriptionChanged(Job != null ? Job.JH_JobNum.ToString() : (NoResString)"(none)", GenericChargeBizO != null ? GenericChargeBizO.VC_Code.ToString() : "");
			}

			var enableGovernmentChargeCode = AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.GetFallBackValueAtAllLevels(AL_GC.ToGuid(), Guid.Empty, Guid.Empty);
			if (enableGovernmentChargeCode && ChargeCode != null)
			{
				if ((!IsInDatabase || AL_GovtChargeCodeInfo.HasChanges) && ChargeCode.AC_GovtChargeCode != AL_GovtChargeCode)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					InvoiceBase.Logs.AddNew(Events.EditedARecord, string.Format(CultureInfo.InvariantCulture, "Government Charge Code changed from Default value: '{0}'. New Value: '{1}'", ChargeCode.AC_GovtChargeCode, AL_GovtChargeCode));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		public virtual void LogLineDescriptionChanged(string jobNumber, string chargeCode)
		{
		}

		#region SecurityOverrideProviderCore

		ISecurityOverrideProvider ISecurityOverrideProviderSource.Provider
		{
			get { return provider ?? SecurityOverrideProviderSource.Get(InvoiceBase).Provider; }
			set { provider = value; }
		}
		ISecurityOverrideProvider provider;

		#endregion

		#region Branch

		public override ZGuid AL_GB
		{
			get { return base.AL_GB; }
			set
			{
				if (CanChangeLineValues)
				{
					base.AL_GB = value;

					if (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value && IsGSTMandatory)
					{
						SetDefaultPlaceOfSupply();
						if (!AccountingMasterFilesUtils.IsTaxBranchApplicable)
						{
							SetGSTRate();
						}
					}
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		protected virtual bool AL_GB_ReadOnly
		{
			get { return IsPopulatedFromImportedJobCharge || IsInvoicingBaseApproving; }
		}

		#endregion

		#region AL_GB_TaxBranch

		public override ZGuid AL_GB_TaxBranch
		{
			get { return base.AL_GB_TaxBranch; }
			set
			{
				base.AL_GB_TaxBranch = value;

				if (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value
					&& IsGSTMandatory
					&& AccountingMasterFilesUtils.IsTaxBranchApplicable)
				{
					SetGSTRate();
				}
			}
		}

		#endregion

		#region AL_ExchangeRate

		public override ZDecimal AL_ExchangeRate
		{
			get { return base.AL_ExchangeRate; }
			set
			{
				if (base.AL_ExchangeRate != value)
				{
					if (CanChangeLineValues)
					{
						base.AL_ExchangeRate = value;
						PeriodApportionment.Recalculate();
					}
					else
					{
						RaiseApportionedLineModified();
					}
				}
			}
		}

		public ZDecimal AL_ExchangeRate_Decimals => Company.ExchangeRateDecimalPlaces;

		protected override bool AL_ExchangeRate_ReadOnly
		{
			get
			{
				bool isAPInvoicePostedToEFT = InvoiceBase != null && InvoiceBase.IsAPInvoiceOrCreditNote &&
					InvoiceBase.AH_PostedToEFT && InvoiceBase.AH_RX_NKTransactionCurrency != InvoiceBase.AH_Calc_LocalRXCode;

				return InvoiceBase == null || (!InvoiceBase.IsEditingLocalAmountsSupported && !isAPInvoicePostedToEFT) || base.AL_ExchangeRate_ReadOnly;
			}
		}

		#endregion

		#region AL_A9_VATClass

		public override ZGuid AL_A9_VATClass
		{
			get { return base.AL_A9_VATClass; }
			set
			{
				if ((!base.AL_A9_VATClass.IsEmpty || !this.HasContext(Context.GenericChargeChangingTaxIdIsEmpty)) && PreserveLinkedValuesOnGenericChargeChange)
				{
					return;
				}

				if (CanChangeLineValues)
				{
					base.AL_A9_VATClass = value;
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		public bool AL_A9_VATClass_ReadOnly
		{
			get
			{
				bool result = false;
				if (AL_AT == ZGuid.Empty)
				{
					result = true;
				}
				else
				{
					if ((InvoiceBase != null && InvoiceBase.AH_Ledger == LedgerTypes.AccountsReceivable) || AL_LineType == TransactionLineTypes.Revenue)
					{
						result =
							!Env.Security.NewReceivablesOverrideTaxMessageAllows.IsAllowed
							||
							!AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
					}
					else if ((InvoiceBase != null && InvoiceBase.AH_Ledger == LedgerTypes.AccountsPayable) || AL_LineType == TransactionLineTypes.Cost)
					{
						result = !(AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty) && Env.Security.NewPayablesOverrideTaxMessageAllows.IsAllowed);
					}
				}
				return result;
			}
		}

		#endregion

		#region AL_Desc

		public override ZString AL_Desc
		{
			get { return base.AL_Desc; }
			set
			{
				if (!string.IsNullOrWhiteSpace(base.AL_Desc) && PreserveLinkedValuesOnGenericChargeChange)
				{
					return;
				}

				base.AL_Desc = value;
			}
		}

		#endregion

		#region AL_AT

		public override ZGuid AL_AT
		{
			get { return base.AL_AT; }
			set
			{
				if (!base.AL_AT.IsEmpty && PreserveLinkedValuesOnGenericChargeChange)
				{
					return;
				}

				if (CanChangeLineValues)
				{
					if (AL_AT != value)
					{
						ResetCachedCalculatedTaxAmount();
					}

					var oldValue = AL_AT;
					base.AL_AT = value;
					if (AL_AT != oldValue)
					{
						InvoicingLineTaxDateCacheProvider.StaleCache();
						SetAL_GSTVATBasis();
						if (!AL_JH.IsEmpty)
						{
							UpdateAPTaxDateForOperationalJob();
						}
					}
					UpdateAL_A9_VatClass();
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		void UpdateAL_A9_VatClass()
		{
			if (AL_AT == ZGuid.Empty)
			{
				AL_A9_VATClass = ZGuid.Empty;
			}
			else
			{
				ZGuid overrideInvTaxMsg = ZGuid.Empty;
				AccTaxRate overrideTaxRate = GetFallbackTaxRate(out overrideInvTaxMsg);
				if (overrideTaxRate != null && overrideTaxRate.PK == AL_AT && overrideInvTaxMsg != ZGuid.Empty)
				{
					AL_A9_VATClass = overrideInvTaxMsg;
				}
				else
				{
					UpdateAL_A9_VatClassFromTaxRate();
				}
			}
		}

		protected virtual bool AL_AT_ReadOnly
		{
			get
			{
				return ChargeCode != null && ChargeCode.IsComment ||
					!(IsGSTMandatory && (AllowUserGSTOverride || IsCurrentChargeGLAccount));
			}
		}

		#endregion

		[ResourceStringData("InvoicingLineBase|AL_TaxDate", Caption = "Tax Date", MediumCaption = "Tax Date", ShortCaption = "Tax Date",
			FullDescription = @"Tax Date is used to determine the applicable Tax Rate, based on the selected Tax ID. Tax Date defaults based on the job type, transport mode and direction of the selected job, according to the configuration in the following registry Accounting > Tax Configurations > Tax Date Defaulting Option. Applies to Invoice Line > Tax Date (AR and AP, Invoice, Credit Note and Adjustment), Apportion to Consol tab on AP Invoice > Tax Date")]
		public override ZDate AL_TaxDate
		{
			get => base.AL_TaxDate;
			set
			{
				if (CanChangeLineValues)
				{
					var oldValue = AL_TaxDate;
					base.AL_TaxDate = value;
					if (AL_TaxDate != oldValue)
					{
						InvoicingLineTaxDateCacheProvider.StaleCache();
						UpdateExchangeRateWhenExRateOptionIsEIT();
					}
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		void UpdateExchangeRateWhenExRateOptionIsEIT()
		{
			if (InvoiceBase == null || Factory.HasContext(BusinessContext.NotUpdateExchangeRateWhenExRateOptionIsEITFromBulkConsolCostImport))
			{
				return;
			}

			InvoiceBase.SetExchangeRateForInvoicePostingExchangeRateOption(true, AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code);
		}

		protected override void SetBaseAL_TaxDate(ZDate value)
		{
			base.SetBaseAL_TaxDate(value);
			if (value.IsValid)
			{
				UpdateExchangeRateWhenExRateOptionIsEIT();
			}
		}

		public override ZInt AL_TaxRateNumerator
		{
			get => base.AL_TaxRateNumerator;
			set
			{
				if (CanChangeLineValues)
				{
					base.AL_TaxRateNumerator = value;
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		public override ZInt AL_TaxRateDenominator
		{
			get => base.AL_TaxRateDenominator;
			set
			{
				if (CanChangeLineValues)
				{
					base.AL_TaxRateDenominator = value;
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		public override ZInt AL_TaxExtraRateNumerator
		{
			get => base.AL_TaxExtraRateNumerator;
			set
			{
				if (CanChangeLineValues)
				{
					base.AL_TaxExtraRateNumerator = value;
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		public override ZInt AL_TaxExtraRateDenominator
		{
			get => base.AL_TaxExtraRateDenominator;
			set
			{
				if (CanChangeLineValues)
				{
					base.AL_TaxExtraRateDenominator = value;
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		protected override void OnRateChangedCore()
		{
			UpdateAL_OSTaxAmount();
			if (GSTInclusiveAmounts)
			{
				SplitGSTInclusiveAmount();
			}
		}

		void RaiseApportionedLineModified()
		{
			if (ParentLinesCollection != null)
			{
				ApportionedLineModifiedStackTrace.Add(new StackTrace());
				ParentLinesCollection.OnApportionedInvoiceLineModified(this);
			}
		}

		public List<StackTrace> ApportionedLineModifiedStackTrace
		{
			get
			{
				return apportionedLineModifiedStackTrace ?? (apportionedLineModifiedStackTrace = new List<StackTrace>());
			}
		}

		List<StackTrace> apportionedLineModifiedStackTrace;

		#region AL_AW

		public override ZGuid AL_AW
		{
			get { return base.AL_AW; }
			set
			{
				base.AL_AW = value;
				UpdateAL_LocalWHTAmount();
			}
		}

		protected bool AL_AW_ReadOnly
		{
			get
			{
				return ChargeCode != null && ChargeCode.IsComment
				  || !(IsWHTMandatory && (AllowUserWHTOverride || IsCurrentChargeGLAccount))
				  || IsPopulatedFromImportedApportionment;
			}
		}

		bool IsWHTMandatory => GlbCompany.CurrentCompany.GC_IsWHTRegistered && IsCurrentOrganisationWHTApplicable;

		public void RecalculateWHTRate()
		{
			if (IsWHTMandatory)
			{
				if (ChargeCode != null && !ChargeCode.IsComment)
				{
					AL_AW = ChargeCode.AC_AW_WithholdingTaxRate;
				}
			}
			else
			{
				AL_AW = ZGuid.Empty;
			}
		}

		#endregion

		#region AL_RX_NKTransactionCurrency

		public override ZString AL_RX_NKTransactionCurrency
		{
			get { return base.AL_RX_NKTransactionCurrency; }
			set
			{
				if (CanChangeLineValues)
				{
					if (AL_RX_NKTransactionCurrency != value)
					{
						var oldValue = AL_RX_NKTransactionCurrency;
						var oldExRate = AL_ExchangeRate;
						var oldLocalAmount = AL_LocalExTaxAmount;
						var oldOSAmount = AL_OSExTaxAmount;

						ResetCachedCalculatedTaxAmount();
						base.AL_RX_NKTransactionCurrency = value;

						if (TransactionHeader != null && AL_RX_NKTransactionCurrency != Company.GC_RX_NKLocalCurrency && ExchangeRateCalculator.IsExRateOptionApplicable(this.GetExRateLedger(), TransactionHeader.AH_RX_NKTransactionCurrency == Company.GC_RX_NKLocalCurrency, AL_GC))
						{
							SetExchangeRate();
						}

						RecalculateLocalAmounts();
						RecalculateTaxAmounts();

						if (!IsInDatabase &&
							(TransactionHeader?.AH_TransactionType.ToString() == TransactionTypes.Invoice || TransactionHeader?.AH_TransactionType.ToString() == TransactionTypes.CreditNote || TransactionHeader?.AH_TransactionType.ToString() == TransactionTypes.AdjustmentNote) &&
							GlbCompany.CurrentCompany.LocalCurrency.Code == AL_RX_NKTransactionCurrency &&
							AL_OSExTaxAmount != AL_LocalExTaxAmount &&
							AL_ExchangeRate == 1m)
						{
							CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK,
								CriticalValidationInfoCollectorServiceKeyType.TransactionLineLocalAmountInconsistentWithOSAmountWhenExRateIsOne,
								() =>
								{
									return FormattableString.Invariant($@"Transaction line local amount is inconsistent with OS amount while exchange rate is 1.
Currency changed from {oldValue} to {value}.
Exchange rate changed from {oldExRate} to {AL_ExchangeRate}.
Line local exclude tax amount changed from {oldLocalAmount} to {AL_LocalExTaxAmount}.
Line OS exclude tax amount changed from {oldOSAmount} to {AL_OSExTaxAmount}.
Global Company Currency is {GlbCompany.CurrentCompany.LocalCurrency.Code}.
Line RunMethodSuspended is {((AccountingSuspenders.IRunMethodSuspending)this).RunMethodSuspended}.
Call Stack when line currency changed:
{new System.Diagnostics.StackTrace()}");
								},
								CriticalValidationInfoCollectorService.CollectionFrequency.CollectOnlyAfterErrorReportForCurrentUserSession);
						}
					}
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		protected bool AL_RX_NKTransactionCurrency_ReadOnly
		{
			get { return InvoiceBase == null || !InvoiceBase.IsEditingLocalAmountsSupported; }
		}

		#endregion

		#region AL_AH

		public override ZGuid AL_AH
		{
			get { return base.AL_AH; }
			set
			{
				base.AL_AH = value;
				if (InvoiceBase != null)
				{
					UpdateGSTReadOnlyState();
					UpdateWHTReadOnlyState();
					CreateComplianceDocumentRecordOnPosting = InvoiceBase.HasPCDSettingForAP || InvoiceBase.HasPCDSettingForIN;
				}
			}
		}

		#endregion

		#region AL_JH

		public void SetExchangeRate()
		{
			if (InvoiceBase != null && !InvoiceBase.IsReverseTransaction)
			{
				if (IsPopulatedFromImportedApportionment)
				{
					AL_ExchangeRate = ApportionmentChargeImportedFrom.JR_OSCostExRate;
					if (InvoiceBase.UseJobExchangeRate)
					{
						InvoiceBase.ExchangeRate.DontSetTodaysRateOnCurrencyChange = true;
						ExchangeRate.DontSetTodaysRateOnCurrencyChange = true;
					}
				}
				else if (InvoiceBase.UseJobExchangeRate)
				{
					AL_ExchangeRate = InvoiceBase.GetExchangeRateFromJobExRateConfig(TransactionJob);
					InvoiceBase.ExchangeRate.DontSetTodaysRateOnCurrencyChange = true;
					ExchangeRate.DontSetTodaysRateOnCurrencyChange = true;
				}
				else if (InvoiceBase.AH_RX_NKTransactionCurrency != AL_RX_NKTransactionCurrency)
				{
					if (AL_RX_NKTransactionCurrency != Company.GC_RX_NKLocalCurrency && ExchangeRateCalculator.IsExRateOptionApplicable(this.GetExRateLedger(), TransactionHeader.AH_RX_NKTransactionCurrency == Company.GC_RX_NKLocalCurrency, AL_GC))
					{
						AL_ExchangeRate = ExchangeRateCalculator.GetOverrideExchangeRate(AL_RX_NKTransactionCurrency, InvoiceBase.AH_RX_NKTransactionCurrency == Company.GC_RX_NKLocalCurrency,
							AL_GC, RateType, this.GetExRateLedger(), InvoiceBase.AH_InvoiceDate, InvoiceBase.AH_PostDate, InvoiceBase.InvoiceTaxDate);
						ExchangeRate.DontSetTodaysRateOnCurrencyChange = true;
					}
					else
					{
						AL_ExchangeRate = Company.GetExchangeRate().TodaysRate(AL_RX_NKTransactionCurrency, RateType);
					}
				}
				else
				{
					AL_ExchangeRate = InvoiceBase.AH_ExchangeRate;
				}
				InvoiceBase.TransactionLinesDefaultExRates[PK] = AL_ExchangeRate;
			}
		}

		public override ZGuid AL_JH
		{
			get { return base.AL_JH; }
			set
			{
				if (CanChangeLineValues)
				{
					if (value != base.AL_JH)
					{
						base.AL_JH = value;
						Job job = Factory.Load<Job>(AL_JH);
						if (job != null)
						{
							job.InitializeParentFromGenericJobWithSettingDefaults();
							if (!job.IsInDatabase)
							{
								InvoiceBase?.LineJobsWithMutex.Add(job);
							}
							PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Default;
						}
						if (!AL_JHSettingDefaultsSuspended)
						{
							CalculateBranchAndDepartment();
							SetAL_SupplyType();
							CalculateGST(IsGSTMandatory);
							UpdateAPTaxDateForOperationalJob();
							ReloadLineCharges();
							LoadTransactionJob(value);
							SetExchangeRate();
						}
						HandleInvoicingJobChangeForRelatedJob();

						if (value.IsValid && InvoiceBase != null && !InvoiceBase.IsImportingJobCharges && !InvoiceBase.IsImportingConsolApportionment
							&& fIsChargePopupEnabled && AccountingConfigurationRegistry.Instance.PopupImportAccrualsScreenOnAPInvoice.Value && !AL_JHInfo.HasErrors())
						{
							RaiseShowJobChargesForImportEvent();
						}
						if (value.IsValid && InvoiceBase != null && InvoiceBase.OnJobChangedIsSubscribed)
						{
							if (job != null)
							{
								InvoiceBase.RaiseOnJobChanged(this, job);
								if (!IsValidationSuspended)
								{
									Validation.ValidateAL_JH();
								}
							}
						}
					}
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		protected virtual bool AL_JH_ReadOnly => IsPopulatedFromImportedJobCharge || !IsJobApplicable;

		public bool IsJobApplicable => !((fMasterTransactionHeader != null && fMasterTransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable) || IsInvoicingBaseApproving) || (IsAmendingOriginal && !InvoiceBase.IsBadDebtWritingOff);

		public bool IsRelatedJobReadyForFinancialClosureWithoutPostSecurity => Job?.IsReadyForFinancialClosureWithoutPostSecurity ?? false;

		public bool ShouldValidateJob => IsJobApplicable || (InvoiceBase != null && (InvoiceBase.IsConvertedUAInvoiceOrCRD || InvoiceBase.IsConvertedFromARInvoice));

		protected internal bool IsAmendingOriginal
		{
			get
			{
				IAmending amending = this.InvoiceBase as IAmending;
				return amending != null && amending.IsAmendingTransaction;
			}
		}

		public bool IsAmendingOriginalViaStrongReference => InvoiceBase != null && InvoiceBase.IsAmendingTransaction_StrongReference;

		#endregion

		#region Calculate Branch and Department

		void CalculateBranchAndDepartment()
		{
			CalculateBranch();
			CalculateDepartment();
		}

		void CalculateBranch()
		{
			if (InvoicingJob != null)
			{
				ZGuid branchPK = ZGuid.Empty;
				var job = InvoicingJob;
				if ((SubmittedFromInvoicingForm || IsConvertedFromARInvoice) && ChargeCode != null)
				{
					job.InitializeParentFromGenericJobWithSettingDefaults();
					var branchToOverride = ChargeCode.GetOverriddenBranch(job.JobType, job.Direction, job.TransportMode,
						defaultingRule => job.PlugInData == null ? null : job.PlugInData.InvoicingSupporter.GetOrganisationByBranchDefaultingRule(defaultingRule));
					if (branchToOverride != null)
					{
						branchPK = branchToOverride.PK;
					}
				}
				if (!branchPK.IsValid)
				{
					branchPK = job.JH_GB;
				}
				if (branchPK.IsValid)
				{
					AL_GB = branchPK;
				}
			}
		}

		void CalculateDepartment()
		{
			ZGuid departmentPK = ZGuid.Empty;
			if (GenericTransactionCharge != null)
			{
				var department = CalculateDepartmentFromChargeCode();
				if (department != null)
				{
					departmentPK = department.PK;
				}
			}
			if (Job != null && !departmentPK.IsValid)
			{
				departmentPK = Job.JH_GE;
			}
			if (departmentPK.IsValid)
			{
				AL_GE = departmentPK;
			}
		}

		GlbDepartment CalculateDepartmentFromChargeCode()
		{
			GlbDepartment resultDepartment = null;
			if (InvoicingJob != null && InvoicingJob.PlugInData is ForwardingShipment && InvoicingJob.Department != null && ChargeCode != null && ChargeCode.IsCustomsCharge)
			{
				string mappedDepartmentCode = DepartmentMappings.GetMapping(InvoicingJob.Department.GE_Code);
				GlbDepartment mappedDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, mappedDepartmentCode));

				if (mappedDepartment != null && mappedDepartment.GE_IsActive)
				{
					resultDepartment = mappedDepartment;
				}
			}
			else
			{
				if (GenericTransactionCharge != null)
				{
					ZString deptFillterList = GenericTransactionCharge.VC_DepartmentFilterList;
					if (((deptFillterList.Split(',').Length == 1) &&
						(deptFillterList != "ALL")))
					{
						GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, deptFillterList));
						if (department != null && department.GE_IsActive)
						{
							resultDepartment = department;
						}
					}
				}
			}

			return resultDepartment;
		}

		DepartmentMappingCollection fDepartmentMappings;
		DepartmentMappingCollection DepartmentMappings
		{
			get
			{
				if (fDepartmentMappings == null)
				{
					fDepartmentMappings = new DepartmentMappingCollection(AccountingConfigurationRegistry.Instance.InvoicingDepartmentMapping.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty));
				}
				return fDepartmentMappings;
			}
		}

		#endregion

		#region AL_OSExTaxAmount

		protected bool AL_OSExTaxAmount_ReadOnly
		{
			get { return (ChargeCode != null && ChargeCode.IsComment) || (InvoiceBase == null || InvoiceBase.IsConvertedFromARInvoice); }
		}

		protected override ZDecimal AL_OSExTaxAmountCore
		{
			get { return base.AL_OSExTaxAmountCore; }
			set
			{
				if (CanChangeLineValues)
				{
					if (value != AL_OSExTaxAmountCore)
					{
						ResetCachedCalculatedTaxAmount();
					}

					base.AL_OSExTaxAmountCore = value;
					PeriodApportionment.Recalculate();
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		protected override bool RoundToZero => InvoiceBase != null ? base.RoundToZero && !InvoiceBase.IsReverseTransaction : base.RoundToZero;

		void ResetCachedCalculatedTaxAmount()
		{
			IsCachedTaxAmountDirty = true;
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal CachedCalculatedTaxAmount
		{
			get
			{
				if (IsCachedTaxAmountDirty)
				{
					fCachedCalculatedTaxAmount = (TaxRate != null && TransactionCurrency != null) ? Utilities.Round(AL_OSExTaxAmount * (AL_TaxRateCalc / 100), TransactionCurrency.Decimals) : 0M;
					IsCachedTaxAmountDirty = false;
				}
				return fCachedCalculatedTaxAmount;
			}
		}

		bool IsCachedTaxAmountDirty = true;
		ZDecimal fCachedCalculatedTaxAmount;

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

					if (IsGSTMandatory)
					{
						SetGSTRate();
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateAL_SupplyType();
					}
				}
			}
		}

		protected virtual bool AL_SupplyType_ReadOnly => IsPopulatedFromImportedApportionment;

		#endregion

		protected bool AL_Desc_ReadOnly => IsInvoicingBaseApproving || (LineType == ZArchitecture.Core.TransactionLineTypes.Revenue && ChargeCode != null && !(ChargeCode.AC_AllowDescriptionOvertype && IsUserAllowedToModifyDefaultChargeCodeDescription));

		protected override SchemaColumn JobChargeRelatedLineFilterField
		{
			get
			{
				SchemaColumn column = null;
				if (TransactionHeader != null && !TransactionHeader.AH_IsCancelled)
				{
					if (AL_LineType == ZArchitecture.Core.TransactionLineTypes.Revenue)
					{
						column = JobChargeSchema.JR_AL_ARLine;
					}
					else
					{
						column = JobChargeSchema.JR_AL_APLine;
					}
				}
				return column;
			}
		}

		[ReadOnlyMember(nameof(IsInvoicingBaseApproving))]
		public override ZBool AL_IsFinalCharge
		{
			get { return base.AL_IsFinalCharge; }
			set
			{
				if (CanChangeLineValues)
				{
					base.AL_IsFinalCharge = value;
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		public override void OnSavingCore()
		{
			base.OnSavingCore();

			if (AL_AC.IsValid
				&& new[] { TransactionLineTypes.Revenue, TransactionLineTypes.Cost }.Contains<string>(AL_LineType)
				&& (ChargeCode?.IsComment ?? false))
			{
				AL_PostToGL = AL_ReverseToGL = "Y";
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				ReleaseMutex();
			}
		}

		public override bool IsGovtChargeCodeApplicable
		{
			get { return true; }
		}

		protected virtual bool AL_GovtChargeCode_ReadOnly
		{
			get
			{
				if (GLHeader != null
					&& (GenericTransactionCharge != null && !GenericTransactionCharge.IsDeleted && GenericTransactionCharge.VC_IsGLAccount))
				{
					return false;
				}
				else
				{
					if (TransactionHeader != null)
					{
						if (TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable
							|| TransactionHeader.AH_Ledger == LedgerTypes.IncompleteTransactions
							|| TransactionHeader.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
						{
							return !Env.Security.NewPayablesOverrideGovtCCodeAllows.IsAllowed;
						}
						else if (TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
						{
							return !Env.Security.NewReceivablesOverrideGovtCCodeAllows.IsAllowed;
						}
					}
				}

				return true;
			}
		}

		public override bool IsEnforceBranchLevelPostingValidationApplicable
		{
			get
			{
				var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent((InvoicingBase)MasterTransactionHeader);
				bool isTaxRecoveryLine = taxRecordParent.IsTaxRecoveryLine(this);
				return !isTaxRecoveryLine;
			}
		}

		#endregion

		#region Validation

		protected override sealed AccTransactionLinesValidation GetNewValidation()
		{
			if (IsValidationSuspended) //keep this IF the first always to avoid any any db hits and calculations for a case when validation will not be used.
			{
				return GetEmptyValidation();//Return empty validation as just some not null value as actual validation calls will be skipped anyway.
			}

			AccTransactionLinesValidation result = null;

			if (InvoiceBase != null)
			{
				if (InvoiceBase.IsDeleted || InvoiceBase.IsInPreviewingInvoicesContext || ((IReversing)InvoiceBase).IsReversed)
				{
					result = GetEmptyValidation();
				}
				else if (InvoiceBase.IsInMatchingContext)
				{
					result = new LineMatchingValidation(this);
				}
				else if (!InvoiceBase.IsBeingCreatedPostedAllocatedApprovedOrIncomplete)
				{
					result = GetEmptyValidation();
				}
			}

			if (result == null)
			{
				result = GetNewValidationCore();
			}

			return result;
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			return new InvoicingLineBaseValidation(this);
		}

		#endregion

		#region GenericJob

		public void SuspendAL_JHSettingDefaults()
		{
			AL_JHSettingDefaultsSuspended = true;
		}

		public void ResumeAL_JHSettingDefaults()
		{
			AL_JHSettingDefaultsSuspended = false;
		}

		bool AL_JHSettingDefaultsSuspended;
		
		protected InvoicingLineBaseCollection ParentLinesCollection
		{
			get
			{
				InvoicingLineBaseCollection result = null;

				foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					if (!(parentCollection is JobRelatedInvoicingLineBaseCollection))
					{
						InvoicingLineBaseCollection foundCollection = parentCollection as InvoicingLineBaseCollection;
						if (foundCollection != null)
						{
							if (result != null)
							{
								throw new NotSupportedException("Invoice line should only have one parent invoice lines collection.");
							}
							result = foundCollection;
						}
					}
				}

				return result;
			}
		}

		InvoicingLineBaseCollection ParentFilteredLinesCollection
		{
			get
			{
				InvoicingLineBaseCollection result = null;

				foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					if (!(parentCollection is JobRelatedInvoicingLineBaseCollection))
					{
						FilteredInvoicingLineBaseCollectionView foundCollectionView = parentCollection as FilteredInvoicingLineBaseCollectionView;

						if (foundCollectionView != null)
						{
							if (result != null)
							{
								throw new NotSupportedException("Invoice line should only have one parent filtered invoice lines collection.");
							}
							result = (InvoicingLineBaseCollection)foundCollectionView.CollectionToFilter;
						}
					}
				}

				return result;
			}
		}
		
		#endregion

		#region GenericCharge

		public void SuspendGenericChargeSettingDefaults()
		{
			GenericChargeSettingDefaultsSuspended = true;
		}

		public void ResumeGenericChargeSettingDefaults()
		{
			GenericChargeSettingDefaultsSuspended = false;
		}

		bool GenericChargeSettingDefaultsSuspended;

		public AccGenericCharge GenericChargeBizO
		{
			get
			{
				var query = new ZQuery(ViewGenericChargeSchema.PK, (AL_AC.IsValid ? AL_AC : AL_AG));
				var charge = Factory.LoadTop1<AccGenericCharge>(query);
				if (ChargeCode != null && charge != null && ChargeCode.AC_ChargeType != charge.VC_Type)
				{
					charge.Reload();
				}
				return charge;
			}
		}

		[RelatedBusinessObject("GenericChargeBizO")]
		[List("ChargeList")]
		public ZGuid GenericCharge
		{
			get
			{
				if (IsInDatabase && fGenericCharge.IsEmpty)
				{
					AccGenericCharge result = GenericChargeBizO;
					fGenericCharge = (result != null) ? result.PK : ZGuid.Empty;
				}
				return fGenericCharge;
			}
			set
			{
				using (this.SetTempContext(Context.GenericChargeChanging))
				using (this.SetTempContext(AL_AT.IsEmpty ? Context.GenericChargeChangingTaxIdIsEmpty : Context.GenericChargeChangingTaxIdHasValue))
				{
					InvoicingLineBaseValidation invoicingLineValidation = Validation as InvoicingLineBaseValidation;
					if (GenericChargeSettingDefaultsSuspended)
					{
						if (value.IsValid)
						{
							LoadGenericCharge(value);
						}
						SetNonPersistentPropertyValue(GenericChargeInfo, ref fGenericCharge, value);
						if (invoicingLineValidation != null && !IsValidationSuspended)
						{
							invoicingLineValidation.ValidateGenericCharge();
							invoicingLineValidation.ValidateAL_JH();
						}
					}
					else
					{
						if (value != fGenericCharge)
						{
							if (CanChangeLineValues)
							{
								ZGuid chargeGuid = value;
								ResetGenericChargeDependantValues();

								if (chargeGuid.IsValid)
								{
									LoadGenericCharge(chargeGuid);
									if (GenericTransactionCharge != null)
									{
										AL_Desc = GenericTransactionCharge.VC_Description;

										if (GenericTransactionCharge.VC_IsGLAccount)
										{
											AL_AG = GenericTransactionCharge.PK;
										}
										else
										{
											AL_AC = GenericTransactionCharge.PK;

											if (InvoiceBase != null)
											{
												if (InvoiceBase.IsMiscServTaxApplicable)
												{
													CalculateGST(IsGSTMandatory);
												}

												if (InvoiceBase.IsMiscServWHTApplicable)
												{
													AL_AW = GenericTransactionCharge.VC_WHTRate;
												}
											}
										}

										CalculateBranchAndDepartment();
									}
									else
									{
										SetNonPersistentPropertyValue(GenericChargeInfo, ref fGenericCharge, ZGuid.Empty);
										if (invoicingLineValidation != null && !IsValidationSuspended)
										{
											invoicingLineValidation.ValidateGenericCharge();
											invoicingLineValidation.ValidateAL_JH();
										}
										GenericTransactionCharge = null;
									}
									AL_ATInfo.RefreshBinding();
									AL_AWInfo.RefreshBinding();
								}

								SetNonPersistentPropertyValue(GenericChargeInfo, ref fGenericCharge, value);
								if (invoicingLineValidation != null && !IsValidationSuspended)
								{
									invoicingLineValidation.ValidateGenericCharge();
									invoicingLineValidation.ValidateAL_JH();
								}
								UpdateGSTReadOnlyState();
								ReloadLineCharges();
							}
							else
							{
								RaiseApportionedLineModified();
							}
						}
					}
					GenericChargeInfo.RefreshBinding();
				}
			}
		}

		ZGuid fGenericCharge;

		public virtual bool GenericCharge_ReadOnly
		{
			get { return IsPopulatedFromImportedJobCharge || IsInvoicingBaseApproving; }
		}

		public ZPropertyInfo GenericChargeInfo
		{
			get { return GetZPropertyInfo(Schema.GenericCharge); }
		}

		public ZString ChargeTypeWithOverride
		{
			get
			{
				ZString chargeType = ZString.Empty;
				if (ChargeCode != null)
				{
					chargeType = JobInvoicing.Job.GetChargeTypeInformation(ChargeCode, InvoicingJob).AN_ChargeType;
				}

				return chargeType;
			}
		}

		public ZPropertyInfo ChargeTypeWithOverrideInfo
		{
			get { return GetZPropertyInfo(InvoicingLineBase.Schema.ChargeTypeWithOverride); }
		}

		#endregion

		#region Enable Multi-Period Apportionment of Costs and Revenues

		public PeriodApportionmentLinesCollection PeriodApportionmentLines => PeriodApportionment.Lines;
		public PeriodApportionmentManager PeriodApportionment => periodApportionment ?? (periodApportionment = new PeriodApportionmentManager(this));
		PeriodApportionmentManager periodApportionment;

		[ResourceStringData("InvoicingLineBase|PeriodApportionmentMethod", Caption = "Period Apportionment Method", MediumCaption = "Apportionment Method", ShortCaption = "Apportionment",
			FullDescription = "To split this charge posting over multiple GL Periods, select the preferred apportionment method to split the amount. " +
			"You will also be required to enter the Service Period Start and End Dates for this charge and the GL Clearing Account for the posting journals. " +
			"Multi-period apportionment is available only for non-job related charges.")]
		[List(nameof(PeriodApportionmentMethodsList))]
		public ZString PeriodApportionmentMethod
		{
			get
			{
				return fPeriodApportionmentMethod;
			}
			set
			{
				if (fPeriodApportionmentMethod != value)
				{
					bool isDefault(string val) => string.IsNullOrWhiteSpace(val) || val == PeriodApportionmentMethods.Codes.Default;
					var changingDEFtoNonDEF = isDefault(fPeriodApportionmentMethod) && !isDefault(value);

					fPeriodApportionmentMethod = value;
					if (isDefault(fPeriodApportionmentMethod))
					{
						PeriodClearingGLAccountPK = ZGuid.Empty;
						PeriodStartDate = ZDate.Empty;
						PeriodEndDate = ZDate.Empty;
						PeriodClearingGLAccountPKInfo.RefreshBinding();
						PeriodStartDateInfo.RefreshBinding();
						PeriodEndDateInfo.RefreshBinding();
					}
					else
					{
						PeriodApportionment.Recalculate();
						if (changingDEFtoNonDEF)
						{
							PeriodApportionment.DefaultClearingAccount();
						}
					}

					var invoicingLineValidation = Validation as InvoicingLineBaseValidation;
					if (invoicingLineValidation != null && !IsValidationSuspended)
					{
						invoicingLineValidation.ValidatePeriodApportionmentMethod();
					}

					PeriodApportionmentMethodInfo.RefreshBinding();
				}
			}
		}
		ZString fPeriodApportionmentMethod;

		public bool PeriodApportionmentMethod_ReadOnly
		{
			get { return !AL_JH.IsEmpty && AL_JH.IsValid; }
		}

		bool IsPeriodApportionmentMethodDefault => PeriodApportionmentMethod == PeriodApportionmentMethods.Codes.Default;

		public ZPropertyInfo PeriodApportionmentMethodInfo
		{
			get { return GetZPropertyInfo(Schema.PeriodApportionmentMethod, "Period Apportionment Method"); }
		}

		public ICodeDescriptionPairList PeriodApportionmentMethodsList => Factory.GetCachedValue("InvoicingLineBase.PeriodApportionmentMethodTypeList", () => new CodeDescriptionPairList(OLookUpEditType.PeriodApportionmentMethods));

		[ResourceStringData("InvoicingLineBase|PeriodStartDate", Caption = "Service Period Start Date", MediumCaption = "Period Start Date", ShortCaption = "Period Start",
			FullDescription = "Enter the service period this invoice covers. The Service Period dates are used to calculate the General Ledger periods for multi-period apportionment posting.")]
		public ZDate PeriodStartDate
		{
			get
			{
				return fPeriodStartDate;
			}
			set
			{
				if (fPeriodStartDate != value)
				{
					fPeriodStartDate = value;
					PeriodApportionment.Recalculate();

					var invoicingLineValidation = Validation as InvoicingLineBaseValidation;
					if (invoicingLineValidation != null && !IsValidationSuspended)
					{
						invoicingLineValidation.ValidatePeriodStartDate();
					}

					PeriodStartDateInfo.RefreshBinding();
				}
			}
		}
		ZDate fPeriodStartDate;

		public bool PeriodStartDate_ReadOnly => IsPeriodApportionmentMethodDefault;

		public ZPropertyInfo PeriodStartDateInfo
		{
			get { return GetZPropertyInfo(Schema.PeriodStartDate, "Service Period Start Date"); }
		}

		[ResourceStringData("InvoicingLineBase|PeriodEndDate", Caption = "Service Period End Date", MediumCaption = "Period End Date", ShortCaption = "Period End",
			FullDescription = "Enter the service period this invoice covers. The Service Period dates are used to calculate the General Ledger periods for multi-period apportionment posting.")]
		public ZDate PeriodEndDate
		{
			get
			{
				return fPeriodEndDate;
			}
			set
			{
				if (fPeriodEndDate != value)
				{
					fPeriodEndDate = value;
					PeriodApportionment.Recalculate();

					var invoicingLineValidation = Validation as InvoicingLineBaseValidation;
					if (invoicingLineValidation != null && !IsValidationSuspended)
					{
						invoicingLineValidation.ValidatePeriodEndDate();
					}

					PeriodEndDateInfo.RefreshBinding();
				}
			}
		}
		ZDate fPeriodEndDate;

		public bool PeriodEndDate_ReadOnly => IsPeriodApportionmentMethodDefault;

		public ZPropertyInfo PeriodEndDateInfo
		{
			get { return GetZPropertyInfo(Schema.PeriodEndDate, "Service Period End Date"); }
		}

		bool isDefaultPeriodApportionmentMethod(string val) => string.IsNullOrWhiteSpace(val) || val == PeriodApportionmentMethods.Codes.Default;

		public override ZDecimal AL_InputGSTVATRecoverable
		{
			get { return base.AL_InputGSTVATRecoverable; }
			set
			{
				var hasChange = AL_InputGSTVATRecoverable != value;
				base.AL_InputGSTVATRecoverable = value;
				if (hasChange && !isDefaultPeriodApportionmentMethod(fPeriodApportionmentMethod))
				{
					PeriodApportionment.Recalculate();
				}
			}
		}

		[ResourceStringData("InvoicingLineBase|PeriodClearingGLAccountPK", Caption = "Period Clearing GL Account", MediumCaption = "Clearing GL Account", ShortCaption = "Clearing Account",
			FullDescription = "Enter or select a General Ledger Account for multi-period apportionment posting. The total charge amount is posted to the Clearing account in the invoice posting period. " +
			"Additionally, a separate journal is posted into each of the Service Period GL Accounts, reducing the balance of the Clearing account and recording the actual.")]
		[RelatedBusinessObject("PeriodClearingGLAccount")]
		[List("Lookups.BSHGLHeaders")]
		public ZGuid PeriodClearingGLAccountPK
		{
			get
			{
				return fPeriodClearingGLAccountPK;
			}
			set
			{
				if (fPeriodClearingGLAccountPK != value)
				{
					fPeriodClearingGLAccountPK = value;

					var invoicingLineValidation = Validation as InvoicingLineBaseValidation;
					if (invoicingLineValidation != null && !IsValidationSuspended)
					{
						invoicingLineValidation.ValidatePeriodClearingGLAccountPK();
					}

					PeriodClearingGLAccountPKInfo.RefreshBinding();
				}
			}
		}
		ZGuid fPeriodClearingGLAccountPK;

		public bool PeriodClearingGLAccountPK_ReadOnly => IsPeriodApportionmentMethodDefault;

		public ZPropertyInfo PeriodClearingGLAccountPKInfo
		{
			get { return GetZPropertyInfo(Schema.PeriodClearingGLAccountPK); }
		}

		public AccGLHeader PeriodClearingGLAccount
		{
			get { return Factory.Load<AccGLHeader>(PeriodClearingGLAccountPK); }
		}

		#endregion

		public override ZDecimal AL_OSTaxAmount
		{
			get { return base.AL_OSTaxAmount; }
			set
			{
				if (CanChangeLineValues)
				{
					var hasChange = AL_OSTaxAmount != value;
					base.AL_OSTaxAmount = value;
					if (hasChange && !isDefaultPeriodApportionmentMethod(fPeriodApportionmentMethod))
					{
						PeriodApportionment.Recalculate();
					}
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		protected override ZExchangeRate GetNewExchangeRate()
		{
			var exRate = base.GetNewExchangeRate();
			exRate.ShouldSetExchangeRateOnCurrencySetting = () => CanChangeLineValues;
			return exRate;
		}

		protected virtual bool CanChangeLineValues
		{
			get
			{
				return !IsPopulatedFromImportedApportionment || (InvoiceBase != null && (InvoiceBase.IsImportingConsolApportionment || InvoiceBase.IsProxyingHeaderValues));
			}
		}

		public override ZGuid AL_AG
		{
			get { return base.AL_AG; }
			set
			{
				var oldValue = AL_AG;
				base.AL_AG = value;
				if (AL_AG != oldValue)
				{
					SetAL_GSTVATBasis();
				}
			}
		}

		public override ZString AL_GovtChargeCode
		{
			get => base.AL_GovtChargeCode;
			set
			{
				if (AL_GovtChargeCode != value)
				{
					if (CanChangeLineValues)
					{
						base.AL_GovtChargeCode = value;
					}
					else
					{
						RaiseApportionedLineModified();
					}
				}
			}
		}

		#region AL_AC

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public override ZGuid AL_AC
		{
			get { return base.AL_AC; }
			set
			{
				var oldValue = AL_AC;
				using (InvoicingLineTaxDateCacheProvider.BindChargeEvents())
				{
					base.AL_AC = value;
				}

				if (AL_AC != oldValue)
				{
					InvoicingLineTaxDateCacheProvider.StaleCache();
					SetAL_SupplyType();
					SetAL_GSTVATBasis();
				}

				if (ChargeCode != null)
				{
					if (ChargeCode.IsComment)
					{
						AL_AT = ZGuid.Empty;
						AL_AW = ZGuid.Empty;
						AL_OSExTaxAmount = 0M;
						AL_OSTaxAmount = 0M;
					}
					else
					{
						if (IsGSTMandatory)
						{
							SetDefaultPlaceOfSupply();
							SetGSTRate();
						}

						if (IsWHTMandatory)
						{
							AL_AW = ChargeCode.AC_AW_WithholdingTaxRate;
						}

						if (AL_Desc.IsEmpty || AL_Desc == fDefaultDescription)
						{
							AL_Desc = ChargeCode.AC_Desc;
							fDefaultDescription = AL_Desc;
						}

						if (InvoiceBase != null)
						{
							UpdateGSTReadOnlyState();
							UpdateWHTReadOnlyState();
						}
					}
				}

				AL_ATInfo.RefreshBinding();
				AL_AWInfo.RefreshBinding();
				AL_OSExTaxAmountInfo.RefreshBinding();
			}
		}

		public IEnumerable<GlobalChargeCodeMap> ARGlobalChargeCodes
		{
			get
			{
				return InvoiceBase != null ? InvoiceBase.GetGlobalChargeCodes(AL_AC) : Array.Empty<GlobalChargeCodeMap>();
			}
		}

		ZString fDefaultDescription;

		protected void SetGSTRate()
		{
			if (InvoiceBase != null && !InvoiceBase.IsSettingGSTOnLinesSuspended)
			{
				ZGuid overrideInvTaxMsg = ZGuid.Empty;
				AccTaxRate rate = GetFallbackTaxRate(out overrideInvTaxMsg);
				if (rate != null)
				{
					AL_AT = rate.PK;
					AL_A9_VATClass = overrideInvTaxMsg;
				}
			}
		}

		internal void SetDefaultPlaceOfSupply()
		{
			if ((CostLineTypes.Contains(AL_LineType) || RevenueLineTypes.Contains(AL_LineType))
				&& (InvoiceBase?.NeedPlaceOfSupplyAtLineLevel ?? false)
				&& ChargeCode != null
				&& (Header ?? InvoiceBase?.Header) != null
				&& InvoicingJob == null)
			{
				var costSell = CostLineTypes.Contains(AL_LineType)
					? CostSell.Cost
					: CostSell.Revenue;

				var posDetails = AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(ChargeCode, costSell, Header ?? InvoiceBase?.Header, AL_SupplyType, Branch);
				if (!posDetails.posType.IsEmpty)
				{
					AL_PlaceOfSupply = posDetails.posCode;
				}
				else if (posDetails.posCode.IsEmpty)
				{
					AL_PlaceOfSupply = posDetails.posCode;
				}
			}
		}

		#region FallbackTaxRate

		protected virtual AccTaxRate GetFallbackTaxRate(out ZGuid overrideInvTaxMsg)
		{
			AccTaxRate rate = null;
			overrideInvTaxMsg = ZGuid.Empty;

			var chargeCode = ChargeCode;
			if (chargeCode != null)
			{
				AccTaxRate rateOverride = GetChargeCodeTaxRateOverride(chargeCode, out overrideInvTaxMsg);
				if (rateOverride != null)
				{
					rate = rateOverride;
				}
				else
				{
					rate = chargeCode.GSTRate;
				}
			}
			return rate;
		}

		protected override TaxRateOverrideCalculator CreateTaxRateOverrideCalculator()
		{
			return new TaxRateOverrideCalculator(Factory, () => GetChargeCodeTaxTuple(ChargeCode).TaxOverride);
		}

		AccTaxRate GetChargeCodeTaxRateOverride(AccChargeCode chargeCode, out ZGuid overrideInvTaxMsg)
		{
			var taxRateTuple = GetChargeCodeTaxTuple(chargeCode);
			overrideInvTaxMsg = taxRateTuple.OverrideInvTaxMsg;

			return taxRateTuple.TaxRate;
		}

		(AccTaxRate TaxRate, ZGuid OverrideInvTaxMsg, AccChargeTaxOverride TaxOverride) GetChargeCodeTaxTuple(AccChargeCode chargeCode)
		{
			AccChargeTaxOverrideMatcher.TaxCalculationParameters parameters = GetTaxCalculationParameters();

			return parameters != null && chargeCode != null ? chargeCode.GetGSTRateTuple(parameters) : (null, ZGuid.Empty, null);
		}

		internal AccChargeTaxOverrideMatcher.TaxCalculationParameters GetTaxCalculationParameters()
		{
			// This function is also used by tax framework to fetch tax calculation parameters when calculating other taxes. So, upon modification of this function,
			// it is neccesary to add/modify unit tests for all child classes of InvoicingLineBaseTaxableGetTest_TaxCalculationParametersTest class as well.

			var invoiceBase = InvoiceBase;
			var organisation = invoiceBase?.Header;
			if (invoiceBase == null || organisation == null)
			{
				return null;
			}

			IJobCostingPlugIn consol = null;
			if (invoiceBase.HasContext(BusinessContext.InterCompanyInvoiceImportedFromGatewayConsol) && invoiceBase.IsConsolInvoice)
			{
				consol = invoiceBase.Consol;
			}
			else
			{
				consol = Consol;  //For AP Invoice individual lines will be linked to Consol Cost it has been imported from
			}

			var parameters = consol?.GetTaxCalculationParameters();
			if (parameters != null)
			{
				parameters.Organisation = organisation;
			}
			else
			{
				parameters = InvoicingJob?.GetTaxCalculationParameters() ?? new AccChargeTaxOverrideMatcher.TaxCalculationParameters();
				parameters.CostOrSell = TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable ? CostSell.Revenue : CostSell.Cost;
				parameters.Organisation = organisation;
				parameters.Branch = Branch;
			}

			//If any line level FPOS exists, it will override FPOS already set in parameters.FixedPlaceOfSupply
			if (PlaceOfSupplyLocation != null && parameters != null)
			{
				parameters.FixedPlaceOfSupply = PlaceOfSupplyLocation;
			}

			if (TaxRateOverrideCalculator.GetIsUseTaxOverrideForInterCompanyInvoiceImport(Factory))
			{
				parameters.TransactionContext = TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			}

			if (!AL_SupplyType.IsEmpty && parameters != null)
			{
				parameters.SupplyType = AL_SupplyType;
			}

			if (!AL_GB_TaxBranch.IsEmpty)
			{
				parameters.Branch = Factory.Load<GlbBranch>(AL_GB_TaxBranch);
			}

			return parameters;
		}

		#endregion

		public AccGenericJobHeader GenericJobObject
		{
			get
			{
				var job = Job;
				if (job != null)
				{
					return job.LoadGenericJob<AccGenericJobHeader>();
				}
				else
				{
					return null;
				}
			}
		}

		public void UpdateAPTaxDateForOperationalJob()
		{
			if (InvoiceBase != null
				&& (InvoiceBase.AH_Ledger == LedgerTypes.AccountsPayable
						|| InvoiceBase.AH_Ledger == LedgerTypes.IncompleteTransactions
						|| TransactionHeader.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions))
			{
				var result = ZDate.Today;
				if (!AL_JH.IsValid || !AL_AT.IsValid)
				{
					result = ZDate.Empty;
				}
				else
				{
					var cachedKey = string.Format("UpdateAPTaxDateForJob_{0}", AL_JH);
					result = Factory.GetCachedValue(cachedKey, () =>
					{
						var taxDate = ZDate.Today;
						var operationalJob = InvoicingJob;
						if (operationalJob != null)
						{
							var taxDateOption = ((IPostingJob)operationalJob).GetTaxDateDefaultingOptionForJob(Factory, LedgerTypes.AccountsPayable);
							if (taxDateOption != null)
							{
								var option = taxDateOption.TaxDateOption;
								if (option == TaxDateDefaultingOption.Code.InvoiceDate)
								{
									taxDate = InvoiceBase.AH_InvoiceDate.Date;
								}
								else if (option != TaxDateDefaultingOption.Code.Today)
								{
									var plugIn = operationalJob.GetInvoicingSupporter();
									if (plugIn != null)
									{
										var operationalDate = ZDate.Empty;
										var taxDateDescription = ZString.Empty;
										(operationalDate, taxDateDescription) = ((IPostingJob)operationalJob).GetTaxDateBasedOnRegistryDefaultingOption(plugIn, option, ZDate.Empty);
										taxDate = operationalDate;
									}
								}
							}
						}
						return taxDate;
					});
				}
				AL_TaxDate = result;
			}
		}

		public void CalculateGST(ZBool isGSTRequired)
		{
			ZGuid result = ZGuid.Empty;
			if (isGSTRequired && InvoiceBase != null && !InvoiceBase.IsSettingGSTOnLinesSuspended)
			{
				if (GenericTransactionCharge != null)
				{
					ZBool isGLAccount = ZBool.False;
					try
					{
						isGLAccount = new ZBool(GenericTransactionCharge.VC_IsGLAccount);
					}
					catch (Exception ex) when (!ex.IsCriticalException()) // This is a Hack by DK. We need to fix BizO Regen so VC_IsGLAccount is treated as ZBool, not as ZString
					{
					}

					bool foundFallBackTaxRate = false;
					ZGuid overrideInvTaxMsg = ZGuid.Empty;
					AccTaxRate rate = GetFallbackTaxRate(out overrideInvTaxMsg);
					if (!isGLAccount && rate != null)
					{
						result = rate.PK;
						foundFallBackTaxRate = true;
					}
					else
					{
						result = AL_AT;
					}
					AL_AT = result;
					if (foundFallBackTaxRate)
					{
						AL_A9_VATClass = overrideInvTaxMsg;
					}
				}
			}
			else
			{
				AL_AT = ZGuid.Empty;
			}
		}

		#endregion

		#region AL_GE

		public override ZGuid AL_GE
		{
			get { return base.AL_GE; }
			set
			{
				if (AL_GE != value)
				{
					if (CanChangeLineValues)
					{
						base.AL_GE = value;

						SetAL_SupplyType();
					}
					else
					{
						RaiseApportionedLineModified();
					}
				}
			}
		}

		protected virtual bool AL_GE_ReadOnly
		{
			get { return IsPopulatedFromImportedJobCharge || IsInvoicingBaseApproving; }
		}

		#endregion

		#region AL_OverseasTotal

		public override ZDecimal AL_OverseasTotal
		{
			get { return base.AL_OverseasTotal; }
			set
			{
				base.AL_OverseasTotal = value;
				UpdateGSTInclusiveAmount();
			}
		}

		protected override bool AL_OverseasTotal_ReadOnly => !InvoiceBase?.IsSourceReferenceUsed ?? true;

		#endregion

		#region AL_LineAmount

		public override ZDecimal AL_LineAmount
		{
			get { return base.AL_LineAmount; }

			set
			{
				var lineAmountChanged = AL_LineAmount != value;
				base.AL_LineAmount = value;

				if (lineAmountChanged)
				{
					OnLineAmountChanged();
				}
			}
		}

		#endregion

		#region AL_Sequence_ReadOnly

		internal protected virtual bool AL_Sequence_ReadOnly
		{
			get
			{
				return !(!IsInDatabase ||
					(TransactionHeader != null &&
					 (TransactionHeader.AH_Ledger == LedgerTypes.IncompleteTransactions || TransactionHeader.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
					));
			}
		}

		#endregion

		#region JobCollection

		protected override JobCollection GetJobCollection()
		{
			var result = base.GetJobCollection();
			if (IsAmendingOriginalViaStrongReference)
			{
				if (jobCollectionForAmmending == null)
				{
					var query = new ZQuery(result.CompleteFilter);
					ZGuid[] jobPKs = ((IAmending)this.InvoiceBase).OriginalTransactionJobPKs;
					query.AddToFilter(JobHeaderSchema.PK, jobPKs);
					jobCollectionForAmmending = new JobCollection(Factory, query);
				}
				result = jobCollectionForAmmending;
			}

			return result;
		}
		JobCollection jobCollectionForAmmending;

		#endregion

		#region Cash Advance

		public ICashAdvanceRequirement RelatedCashAdvanceRequirement
		{
			get
			{
				var charge = GetCashAdvanceRelatedCharge();
				if (charge != null)
				{
					if (relatedCashAdvanceRequirement == null || relatedCashAdvanceRequirement.JobCharge.PK != charge.PK)
					{
						relatedCashAdvanceRequirement = GetCashAdvanceRequirement();
					}
				}
				else
				{
					if (relatedCashAdvanceRequirement != null)
					{
						relatedCashAdvanceRequirement = null;
					}
				}
				return relatedCashAdvanceRequirement;
			}
		}
		ICashAdvanceRequirement relatedCashAdvanceRequirement;

		ICashAdvanceRequirement GetCashAdvanceRequirement()
		{
			ICashAdvanceRequirement car = null;
			var charge = GetCashAdvanceRelatedCharge();
			if (TransactionHeader != null && !TransactionHeader.AH_IsCancelled)
			{
				if (AL_LineType == TransactionLineTypes.Revenue && (charge?.JR_IsARCashAdvance ?? false))
				{
					car = charge.ARCashAdvanceRequirement;
				}
				else if (AL_LineType == TransactionLineTypes.Cost && (charge?.JR_IsAPCashAdvance ?? false))
				{
					car = charge.APCashAdvanceRequirement;
				}
			}
			return car;
		}

		internal BaseCharge GetCashAdvanceRelatedCharge()
		{
			if (!IsInDatabase &&
				 TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable &&
				 !InvoiceBase.HasJobChargeTransformerBeenRun &&
				 IsPopulatedFromImportedApportionment)
			{
				return ApportionmentChargeImportedFrom;
			}
			else if (!IsInDatabase &&
					TransactionHeader.AH_Ledger == LedgerTypes.AccountsPayable &&
					!InvoiceBase.HasJobChargeTransformerBeenRun &&
					IsPopulatedFromImportedJobCharge)
			{
				return OriginalJobCharge as BaseCharge;
			}
			else
			{
				return RelatedJobCharge;
			}
		}
		#endregion

		protected virtual bool GetPropertyReadonlyness(PropertyDescriptor property)
		{
			bool result = false;
			if (UseEditableFieldsForReadOnly && property.HasSetter())
			{
				result = !WritableProperties.Contains(property.Name);
			}
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		List<string> WritableProperties
		{
			get
			{
				if (writableProperties == null)
				{
					writableProperties = new List<string>();
				}
				return writableProperties;
			}
		}

		public void AddWritableProperties(string[] list)
		{
			foreach (string line in list)
			{
				WritableProperties.Add(line);
			}
			UseEditableFieldsForReadOnly = true;
			RefreshBinding();
		}

		public void ClearWritableProperties() => WritableProperties.Clear();

		List<string> writableProperties;

		bool UseEditableFieldsForReadOnly;

		protected override void UpdateAL_OSTaxAmountCore()
		{
			base.UpdateAL_OSTaxAmountCore();
			NZCustomsEntryFeeTaxCalculator.UpdateEntryFeeGST(this);
		}

		#region Lists

		#region JobList

		public AccGenericJobHeaderCollection JobList
		{
			get
			{
				if (fJobList == null)
				{
					fJobList = new AccGenericJobHeaderCollection(Factory, JobListFilter);
				}
				return fJobList;
			}
		}

		protected virtual ZQuery JobListFilter
		{
			get
			{
				return new ZQuery(ViewGenericJobSchema.VJ_JobType, SQLComparisonOperator.NotEqual, "FCN");
			}
		}

		AccGenericJobHeaderCollection fJobList;

		#endregion

		#region ChargeList

		public AccGenericChargeCollection ChargeList
		{
			get
			{
				var chargeCollectionBuilder = GetGenericChargeCollectionBuilder();
				chargeCollectionBuilder.SetJobInfo(this);
				return !IsInDatabase ? chargeCollectionBuilder.GetBuiltButNotLoadedCollection(ShowGLAccountsForImportAction) : new AccGenericChargeCollection(Factory);
			}
		}

		#endregion

		#endregion

		#region MasterTransactionHeader

		public InvoicingBase InvoiceBase
		{
			get { return (InvoicingBase)MasterTransactionHeader; }
		}

		#endregion

		#region Other Public Property & Method

		public ZInt Decimals => TransactionCurrency != null ? TransactionCurrency.Decimals : GlbCompany.CurrentCompany.GetLocalDecimals();

		public ZPropertyInfo DecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(Decimals)); }
		}

		#region GSTInclusiveAmount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal GSTInclusiveAmount
		{
			get
			{
				return fGSTInclusiveAmount;
			}
			set
			{
				if (fGSTInclusiveAmount != value)
				{
					if (CanChangeLineValues)
					{
						fGSTInclusiveAmount = RoundAmountToCurrencyDecimals(value);
						if (GSTInclusiveAmounts)
						{
							SplitGSTInclusiveAmount(value);
						}
						GSTInclusiveAmountInfo.RefreshBinding();
					}
					else
					{
						RaiseApportionedLineModified();
					}
				}
			}
		}

		public ZPropertyInfo GSTInclusiveAmountInfo
		{
			get { return GetZPropertyInfo(Schema.GSTInclusiveAmount); }
		}

		protected bool GSTInclusiveAmount_ReadOnly
		{
			get { return !GSTInclusiveAmounts || InvoiceBase.IsConvertedFromARInvoice; }
		}

		ZBool GSTInclusiveAmounts
		{
			get
			{
				return InvoiceBase != null && InvoiceBase.GSTInclusiveAmounts && !InvoiceBase.GSTInclusiveAmountNeedUpdate;
			}
		}

		ZDecimal fGSTInclusiveAmount;

		void UpdateGSTInclusiveAmount()
		{
			if (!GSTInclusiveAmounts)
			{
				GSTInclusiveAmount = AL_OverseasTotal;
			}
		}

		void SplitGSTInclusiveAmount(ZDecimal value)
		{
			ZDecimal taxRate = TaxRate == null ? 0M : AL_TaxRateCalc + GetEffectiveExtraRate();
			AL_OSExTaxAmount = 100 * value / (100 + taxRate);
		}

		internal void SplitGSTInclusiveAmount()
		{
			SplitGSTInclusiveAmount(GSTInclusiveAmount);
		}
		
		#endregion

		public void UpdateGSTReadOnlyState()
		{
			AL_ATInfo.RefreshBinding();
		}

		public void UpdateWHTReadOnlyState()
		{
			AL_AWInfo.RefreshBinding();
		}

		public void SetDefaultAL_JH(ZGuid jobHeaderGuid)
		{
			if (ShouldDefaultAL_JH)
			{
				SetDefaultAL_JHCore(jobHeaderGuid);
			}
		}

		public void AppendCostReferenceToTheLineDescription(Charge charge)
		{
			if (charge != null && !string.IsNullOrEmpty(charge.JR_CostReference))
			{
				AL_Desc = Res.GetString("23b452dd-b6c5-4b35-a82d-fa2d6954a7c9", "{0} {{{1}}}", AL_Desc, charge.JR_CostReference);
			}
		}

		public string GetCostReferenceFromDescriptionIfAny()
		{
			var result = string.Empty;
			if (Regex.IsMatch(AL_Desc, ".*\\s*{.*}"))
			{
				var texts = Regex.Split(AL_Desc, @"({)|(})");
				result = texts[texts.Length - 3];
			}
			return result;
		}

		protected virtual bool ShouldDefaultAL_JH
		{
			get { return InvoiceBase != null && InvoiceBase.SubmittedFromInvoicingForm && !InvoiceBase.IsReversing; }
		}

		protected virtual void SetDefaultAL_JHCore(ZGuid jobHeaderGuid)
		{
		}

		public ZString TaxReportingBasisHumanReadableName
		{
			get
			{
				var result = ZString.Empty;

				if (AL_GSTVATBasis == AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code)
				{
					result = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Description;
				}
				else if (AL_GSTVATBasis == AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Code)
				{
					result = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Accrual.Description;
				}

				return result;
			}
		}

		[List("JobOrganisations")]
		public ZGuid JobLocalClient
		{
			get { return InvoicingJob != null ? InvoicingJob.LocalChargesPK : ZGuid.Empty; }
		}

		public ZPropertyInfo JobLocalClientInfo
		{
			get { return GetZPropertyInfo(Schema.JobLocalClient); }
		}

		[List("JobOrganisations")]
		public ZGuid JobOverseasAgent
		{
			get { return InvoicingJob != null ? InvoicingJob.AgentCollectPK : ZGuid.Empty; }
		}

		public CreditorCollection ComplianceOrganization => new CreditorCollection(Factory);

		public ZPropertyInfo JobOverseasAgentInfo
		{
			get { return GetZPropertyInfo(Schema.JobOverseasAgent); }
		}

		public OrganisationsFindBoxCollection JobOrganisations
		{
			get { return jobOrganisations ?? (jobOrganisations = new OrganisationsFindBoxCollection(Factory)); }
		}
		OrganisationsFindBoxCollection jobOrganisations;

		#endregion

		#region Line Job Charges

		struct LineJobCharges
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Value is accessed by default reflection-based .Equals(...) implementation of structs.")]
			readonly ZGuid JobPK;
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Value is accessed by default reflection-based .Equals(...) implementation of structs.")]
			readonly ZGuid ChargePK;			

			public LineJobCharges(ZGuid jobPK, ZGuid chargePK)
			{
				this.JobPK = jobPK;
				this.ChargePK = chargePK;
			}
		}

		LineJobCharges fLineJobCharges;

		AccTransactionLinesCollection fLineCharges;

		public virtual AccTransactionLinesCollection LineCharges
		{
			get
			{
				if (fLineCharges == null)
				{
					fLineCharges = new AccTransactionLinesCollection(Factory, LineChargesFilter);
					fLineJobCharges = new LineJobCharges(AL_JH, AL_AC);
					if (AL_JH.IsValid && AL_AC.IsValid)
					{
						fLineCharges.Load();

						RemoveCancelledLine(fLineCharges);
					}
				}
				return fLineCharges;
			}
		}

		void RemoveCancelledLine(AccTransactionLinesCollection lines)
		{
			for (int i = lines.Count - 1; i > -1; i--)
			{
				if (lines[i].TransactionHeader != null)
				{
					if (lines[i].TransactionHeader.AH_IsCancelled)
					{
						lines.Remove(lines[i]);
					}
				}
			}
		}

		public virtual void ReloadLineCharges()
		{
			if (fLineCharges != null && !fLineJobCharges.Equals(new LineJobCharges(AL_JH, AL_AC)))
			{
				fLineJobCharges = new LineJobCharges(AL_JH, AL_AC);
				LineCharges.Load(LineChargesFilter);

				RemoveCancelledLine(fLineCharges);
			}
		}

		ZQuery LineChargesFilter
		{
			get
			{
				ZQuery filter = new ZQuery();
				if (AL_AC.IsValid && AL_JH.IsValid)
				{
					filter.AddToFilter(AccTransactionLinesSchema.AL_JH, AL_JH);
					filter.AddToFilter(AccTransactionLinesSchema.AL_AC, AL_AC);

					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionLines));
					ZDBOnlySubQuery aPSubQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_AL_APLine);
					ZDBOnlySubQuery aRSubQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_AL_ARLine);

					query.AddSubQuery(aPSubQuery, JoinCondition.And);
					query.AddSubQuery(aRSubQuery, JoinCondition.Or);
					filter.AddToFilter(query);

					filter.AddToFilter(AccTransactionLinesSchema.PK, SQLComparisonOperator.NotEqual, PK);
					filter.AddToFilter(AccTransactionLinesSchema.AL_GC, AL_GC);
				}
				else
				{
					filter.IsNoResultQuery = true;
				}
				return filter;
			}
		}

		#endregion

		#region Implementation

		public void ReleaseMutex()
		{
			if (TransactionJob != null)
			{
				TransactionJob.Dispose();
			}
		}

		void LoadGenericCharge(ZGuid chargePK)
		{
			ZQuery filter = new ZQuery(ViewGenericChargeSchema.PK, chargePK);
			GenericTransactionCharge = Factory.LoadTop1<AccGenericCharge>(filter);
		}

		void ResetGenericChargeDependantValues()
		{
			AL_AC = ZGuid.Empty;
			AL_AT = ZGuid.Empty;
			AL_AW = ZGuid.Empty;
			AL_AG = ZGuid.Empty;
		}

		protected override bool AL_OSTaxAmount_ReadOnly
		{
			get { return !(IsGSTMandatory && (IsTaxRateValidForTaxAmountCalculation || (InvoiceBase?.IsSourceReferenceUsed ?? false)) && (AllowUserGSTOverride || IsCurrentChargeGLAccount)); }
		}

		protected bool AL_LocalTaxAmount_ReadOnly
			=> AL_OSTaxAmount_ReadOnly
			|| InvoiceBase == null
			|| (
				!InvoiceBase.IsEditingLocalAmountsSupported
				&& (
					!InvoiceBase.IsSourceReferenceUsed
					|| Company.GC_RX_NKLocalCurrency == AL_RX_NKTransactionCurrency
				)
			);

		protected internal bool IsCurrentOrganisationWHTApplicable
		{
			get
			{
				bool result = false;
				if (MasterTransactionHeader != null && MasterTransactionHeader.Header != null && MasterTransactionHeader.Header.MiscServ != null)
				{
					result = ((InvoicingBase)MasterTransactionHeader).IsMiscServWHTApplicable;
				}
				return result;
			}
		}

		protected bool AllowUserWHTOverride
		{
			get
			{
				bool result = false;

				if (MasterTransactionHeader != null)
				{
					if (MasterTransactionHeader.Header != null && MasterTransactionHeader.Header.MiscServ != null)
					{
						result = MasterTransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable
									? AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyWHTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)
										: AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyWHTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
					}
				}

				return result;
			}
		}

		public void LoadTransactionJob(ZGuid jobHeaderPK)
		{
			TransactionJob = Factory.Load<Job>(jobHeaderPK);
		}

		internal Job TransactionJob;

		public bool IsInvoicingBaseApproving
		{
			get { return InvoiceBase != null && InvoiceBase.IsInvoiceApproving && RelatedJobChargeExists; }
		}

		bool RelatedJobChargeExists
		{
			get { return Factory.LoadTop1<Charge>(new ZQuery(JobChargeSchema.JR_AL_APLine, PK)) != null; }
		}

		bool SubmittedFromInvoicingForm
		{
			get { return InvoiceBase != null && InvoiceBase.SubmittedFromInvoicingForm; }
		}

		internal bool IsConvertedFromARInvoice
		{
			get { return InvoiceBase != null && InvoiceBase.IsConvertedFromARInvoice; }
		}

		public bool IsPopulatedFromImportedJobCharge
		{
			get { return OriginalJobCharge != null; }
		}

		public bool ValidateIfLineCanBeMarkedAsImported(JobCharge originalCharge)
		{
			return originalCharge != null && originalCharge.JR_JH == AL_JH && originalCharge.JR_AC == AL_AC && originalCharge.JR_GB == AL_GB && originalCharge.JR_GE == AL_GE && !originalCharge.JR_IsApportioned;
		}

		void SetAL_GSTVATBasis()
		{
			if (InvoiceBase != null)
			{
				InvoiceBase.SetLinesAL_GSTVATBasis(this);
			}
		}

		void SetAL_SupplyType()
		{
			var result = InvoicingJob?.GetSupplyType(ChargeCode, ChargeTypeWithOverride, AL_GE) ?? ZString.Empty;

			if (!result.IsEmpty)
			{
				AL_SupplyType = result;
			}
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			ReleaseMutex();
		}

		#endregion

		#region IGenericChargeCollectionRequired Members

		bool IGenericChargeCollectionRequired.IsJobRelated
		{
			get { return TransactionJob != null; }
		}

		GlbDepartment IGenericChargeCollectionRequired.Department
		{
			get { return Department; }
		}

		BusinessObjectFactory IGenericChargeCollectionRequired.Factory
		{
			get { return Factory; }
		}

		protected abstract GenericChargeCollectionBuilder GetGenericChargeCollectionBuilder();

		#endregion

		#region Apportionment

		public bool IsPopulatedFromImportedApportionment
		{
			get { return ApportionmentChargeImportedFrom != null; }
		}

		public bool IsLinkedChargeDeleted
		{
			get
			{
				if (!IsInDatabase)
				{
					_ = ApportionmentChargeImportedFrom;
				}
				return isLinkedChargeDeleted;
			}
			set
			{
				isLinkedChargeDeleted = value;
			}
		}
		bool isLinkedChargeDeleted;

		public ApportionSplitCharge ApportionmentChargeImportedFrom
		{
			get
			{
				if (fApportionmentChargeImportedFrom == null && IsInDatabase)
				{
					ZQuery splitChargeFilter = new ZQuery(JobChargeSchema.JR_AL_APLine, SQLComparisonOperator.Equal, PK);
					splitChargeFilter.AddToFilter(JoinCondition.And, JobChargeSchema.JR_E6, SQLComparisonOperator.NotEqual, null);
					fApportionmentChargeImportedFrom = Factory.LoadTop1<ApportionSplitCharge>(splitChargeFilter);
				}
				else if (fApportionmentChargeImportedFrom != null && fApportionmentChargeImportedFrom.IsDeleted)
				{
					if (!InvoiceBase.IsReportingDeletedApportionmentChargesSuspended)
					{
						IsLinkedChargeDeleted = true;
						var deletionCallstack = CriticalValidationInfoCollectorService.GetService(Factory)?.GetInfo(fApportionmentChargeImportedFrom.PK, CriticalValidationInfoCollectorServiceKeyType.ApportionmentChargeLinkedToInvoiceLineDeleted);
						ErrorReporter.ReportOnce("ApportionmentChargeImportedFromIsDeleted_4", deletionCallstack ?? (NoResString)"Charge deletion call stack was not collected");
					}
					fApportionmentChargeImportedFrom = null;
				}

				return fApportionmentChargeImportedFrom;
			}
			set
			{
				fApportionmentChargeImportedFrom = value;
			}
		}

		ApportionSplitCharge fApportionmentChargeImportedFrom;

		internal ZGuid ApportionmentChargeImportedFrom_PKForErrorReporting => fApportionmentChargeImportedFrom?.PK ?? ZGuid.Empty; //field is used to prevent property call and running its logic

#if DEBUG
		internal
#endif
 protected ZGuid ConsolIDBeingApportioned = ZGuid.Empty;

		public ZGuid ImportedApportionmentID
		{
			get
			{
				ZGuid resultGuid = fImportedApportionmentID;
				if (fImportedApportionmentID.IsEmpty && !IsDeleted && ApportionmentChargeImportedFrom != null)
				{
					resultGuid = ApportionmentChargeImportedFrom.JR_E6;
				}
				return resultGuid;
			}
		}

		protected ZGuid fImportedApportionmentID;

		protected void RaiseShowJobChargesForImportEvent()
		{
			if (ShowJobChargesForImportEvent != null && !((ISupportDataImporting)this).IsImportingData
				&& ParentFilteredLinesCollection != null && ParentFilteredLinesCollection.ShouldShowJobChargesForImport)
			{
				ShowJobChargesForImportEvent(this, EventArgs.Empty);
			}
		}

		public event EventHandler ShowJobChargesForImportEvent;

		JobCharge fOriginalJobCharges;
		public JobCharge OriginalJobCharge
		{
			get
			{
				if (fOriginalJobCharges != null && fOriginalJobCharges.IsDeleted)
				{
					fOriginalJobCharges = null;
				}

				return fOriginalJobCharges;
			}
			set
			{
				fOriginalJobCharges = value;
			}
		}

		public ZGuid OriginalJobChargePK => OriginalJobCharge != null ? OriginalJobCharge.PK : ZGuid.Empty;

		public ZPropertyInfo OriginalJobChargePKInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalJobChargePK)); }
		}

		Accrual fOriginalAccrual;
		public Accrual OriginalAccrual
		{
			get { return fOriginalAccrual; }
			set { fOriginalAccrual = value; }
		}

		protected bool AL_LocalExTaxAmount_ReadOnly
			=> AL_OSExTaxAmount_ReadOnly
				|| InvoiceBase == null
				|| (
					!InvoiceBase.IsEditingLocalAmountsSupported
					&& (!InvoiceBase.IsSourceReferenceUsed || Company.GC_RX_NKLocalCurrency == AL_RX_NKTransactionCurrency));

		public override ZDecimal AL_LocalExTaxAmount
		{
			get { return base.AL_LocalExTaxAmount; }
			set
			{
				if (CanChangeLineValues)
				{
					var hasChange = AL_LocalExTaxAmount != value;
					base.AL_LocalExTaxAmount = value;
					if (hasChange && !isDefaultPeriodApportionmentMethod(fPeriodApportionmentMethod))
					{
						PeriodApportionment.Recalculate();
					}
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		public override ZString AL_PlaceOfSupply
		{
			get => base.AL_PlaceOfSupply;
			set
			{
				if (CanChangeLineValues)
				{
					base.AL_PlaceOfSupply = value;
				}
				else
				{
					RaiseApportionedLineModified();
				}
			}
		}

		public virtual void ImportFromApportionSplitCharge(ApportionSplitCharge chargeToImportFrom)
		{
			if (!IsInvoicingBaseApproving)
			{
				if (chargeToImportFrom.Job != null)
				{
					AL_JH = chargeToImportFrom.Job.PK;
				}
				SetConsolID(chargeToImportFrom.ParentConsolCost);
				AL_RX_NKTransactionCurrency = chargeToImportFrom.JR_RX_NKCostCurrency;
				AL_ExchangeRate = chargeToImportFrom.JR_OSCostExRate;
				try
				{
					SuspendGenericChargeSettingDefaults();
					GenericCharge = chargeToImportFrom.JR_AC;
					AL_AC = chargeToImportFrom.JR_AC;
				}
				finally
				{
					ResumeGenericChargeSettingDefaults();
				}
				AL_Desc = chargeToImportFrom.JR_Desc;
				AL_GB = chargeToImportFrom.JR_GB;
				AL_GE = chargeToImportFrom.JR_GE;
			}
			AL_OSExTaxAmount = chargeToImportFrom.JR_OSCostAmt;
			try
			{
				((AccountingSuspenders.IRunMethodSuspending)this).RunMethodSuspended = true;
				AL_LocalExTaxAmount = chargeToImportFrom.JR_LocalCostAmt;
			}
			finally
			{
				((AccountingSuspenders.IRunMethodSuspending)this).RunMethodSuspended = false;
			}
			AL_PlaceOfSupply = chargeToImportFrom.JR_CostPlaceOfSupply;
			AL_SupplyType = chargeToImportFrom.JR_CostSupplyType;

			using (new DisposableAction(() => Factory.SetContext(BusinessContext.NotUpdateExchangeRateWhenExRateOptionIsEITFromBulkConsolCostImport), () => Factory.RemoveContext(BusinessContext.NotUpdateExchangeRateWhenExRateOptionIsEITFromBulkConsolCostImport)))
			{
				AL_AT = chargeToImportFrom.JR_AT_CostGSTRate;

				// this line set AL_TaxDate to today's date, if AL_AT is valid and JR_CostTaxDate is empty.
				SetTaxDateSafe(chargeToImportFrom.JR_CostTaxDate);
			}

			AL_A9_VATClass = chargeToImportFrom.JR_A9_CostVATClass;
			AL_OSTaxAmount = chargeToImportFrom.JR_OSCostGSTAmt_Calc;
			AL_GovtChargeCode = chargeToImportFrom.JR_CostGovtChargeCode;
			AL_AW = chargeToImportFrom.JR_AW_CostWHTRate;

			if (InvoiceBase != null && InvoiceBase.IsInDatabase)
			{
				if (CreateComplianceDocumentRecordOnPosting && chargeToImportFrom.CreateComplianceDocumentRecordOnPosting)
				{
					CreateComplianceDocumentRecordOnPosting = chargeToImportFrom.CreateComplianceDocumentRecordOnPosting;
					ComplianceDocumentNumber = chargeToImportFrom.ComplianceDocumentNumber;
					ComplianceSubType = chargeToImportFrom.ComplianceSubType;
					ComplianceDocumentOrganization = chargeToImportFrom.ComplianceDocumentOrganization;
					ComplianceDocumentVATRegistrationNum = chargeToImportFrom.ComplianceDocumentVATRegistrationNum;
					ComplianceDocumentDate = chargeToImportFrom.ComplianceDocumentDate;
					ComplianceDocumentReportingPeriod = chargeToImportFrom.ComplianceDocumentReportingPeriod;
					ComplianceDocumentSupportingReason = chargeToImportFrom.ComplianceDocumentSupportingReason;
					ComplianceSupportingDocumentType = chargeToImportFrom.ComplianceSupportingDocumentType;
					ComplianceSupportingDocumentNumber = chargeToImportFrom.ComplianceSupportingDocumentNumber;
				}
				else
				{
					CreateComplianceDocumentRecordOnPosting = false;
				}
			}

			if (!IsInvoicingBaseApproving)
			{
				AL_IsFinalCharge = chargeToImportFrom.IsFinal;

				ApportionmentChargeImportedFrom = chargeToImportFrom;
				OriginalJobCharge = chargeToImportFrom.RelatedApportionChargeFromDB;
				fImportedApportionmentID = chargeToImportFrom.JR_E6;

				if (chargeToImportFrom.ParentConsolCost != null && chargeToImportFrom.ParentConsolCost.E6_TaxDate.IsEmpty)
				{
					using (chargeToImportFrom.ParentConsolCost.TaxRecalculationSuspender.GetSuspender())
					{
						chargeToImportFrom.ParentConsolCost.E6_TaxDate = AL_TaxDate;
					}
				}

				InvoicingLineBaseValidation invoicingLineValidation = Validation as InvoicingLineBaseValidation;
				if (invoicingLineValidation != null)
				{
					invoicingLineValidation.ValidateGenericCharge();
				}
			}
		}
#if DEBUG
		public
#else
	protected
#endif
 void SuspendChargePopup()
		{
			fIsChargePopupEnabled = false;
		}

#if DEBUG
		public
#else
	protected
#endif
 void ResumeChargePopup()
		{
			fIsChargePopupEnabled = true;
		}

		protected bool fIsChargePopupEnabled = true;

		#endregion

		#region Related Pay Lines

		public DependentAccTransLinePayCollection TransLinePays
		{
			get
			{
				if (fTransLinePays == null)
				{
					fTransLinePays = new DependentAccTransLinePayCollection(this);
				}
				return fTransLinePays;
			}
		}
		DependentAccTransLinePayCollection fTransLinePays;

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal TransLinePaysTotalAmount
		{
			get
			{
				ZDecimal result = 0m;
				foreach (AccTransLinePay linePay in TransLinePays)
				{
					result += linePay.A7_Amount;
				}

				return result;
			}
		}

		#endregion

		#region IReceivablesTaxAmountCalculation Members

		AccTaxRate IReceivablesTaxAmountCalculation.GSTRate
		{
			get { return TaxRate; }
		}

		ZDecimal IReceivablesTaxAmountCalculation.Rate => AL_TaxRateCalc;

		ZDecimal IReceivablesTaxAmountCalculation.EffectiveExtraRate => GetEffectiveExtraRate();

		ZDecimal IReceivablesTaxAmountCalculation.OsExTaxAmount
		{
			get { return AL_OSExTaxAmount; }
		}

		ZDecimal IReceivablesTaxAmountCalculation.LocalExTaxAmount
		{
			get { return AL_LocalExTaxAmount; }
		}

		ZDecimal IReceivablesTaxAmountCalculation.OsTaxAmount
		{
			get { return AL_OSTaxAmount; }
		}

		ZDecimal IReceivablesTaxAmountCalculation.LocalTaxAmount
		{
			get { return AL_LocalTaxAmount; }
		}

		ZDecimal IReceivablesTaxAmountCalculation.OsGSTAmount
		{
			get { return AL_OSGSTAmount; }
		}

		ZDecimal IReceivablesTaxAmountCalculation.OsExtraTaxAmount
		{
			get { return AL_OSExtraTaxAmount; }
		}

		ZDecimal IReceivablesTaxAmountCalculation.LocalGSTAmount
		{
			get { return AL_LocalGSTAmount; }
		}

		ZDecimal IReceivablesTaxAmountCalculation.LocalExtraTaxAmount
		{
			get { return AL_LocalExtraTaxAmount; }
		}

		bool IReceivablesTaxAmountCalculation.ShouldExclude
		{
			get { return NZCustomsEntryFeeTaxCalculator.IsEntryFeeChargeWithCorrectAmount(this); }
		}

		void IReceivablesTaxAmountCalculation.RecalculateOsTaxAmount()
		{
			try
			{
				this.SetContext(BusinessContext.RecalculatingLineTaxAmountForAdjustingTaxAtHeaderLevel);
				UpdateAL_OSTaxAmount();
			}
			finally
			{
				this.RemoveContext(BusinessContext.RecalculatingLineTaxAmountForAdjustingTaxAtHeaderLevel);
			}

			if (TaxAmountCalculator.UseLocalExTaxAmountToCalculateLocalTax(AL_GC))
			{
				UpdateAL_LocalTaxAmount();
			}
		}

		void IReceivablesTaxAmountCalculation.AdjustOsTaxAmount(ZDecimal amountToAdjust, bool adjustOSTaxOnly)
		{
			if (amountToAdjust.IsEmpty)
			{
				return;
			}
			if (TaxAmountCalculator.UseLocalExTaxAmountToCalculateLocalTax(AL_GC) || adjustOSTaxOnly)
			{
				using (var runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this,
					() => base.AL_OSTaxAmount += amountToAdjust
				))
				{
					runMethodSuspender.RunMethod();
				}
			}
			else
			{
				base.AL_OSTaxAmount += amountToAdjust;
			}
		}

		void IReceivablesTaxAmountCalculation.AdjustLocalTaxAmount(ZDecimal amountToAdjust)
		{
			using (GetOSAmountCalculationSuspender())
			{
				AL_LocalTaxAmount += amountToAdjust;
			}
		}

		void IReceivablesTaxAmountCalculation.AdjustOsExtraTaxAmount(ZDecimal amountToAdjust)
		{
			if (amountToAdjust.IsEmpty)
			{
				return;
			}
			else
			{
				AL_OSExtraTaxAmount += amountToAdjust;
				if (ShouldTaxAmountBeAdjustedForSplitPayments)
				{
					AL_OSGSTAmount = -AL_OSExtraTaxAmount;
				}
				else
				{
					CalculateAndSetAL_LocalExtraTaxAmount(AL_OSExtraTaxAmount, alwaysRecalculate: true);
				}
			}
		}

		void IReceivablesTaxAmountCalculation.AdjustLocalExtraTaxAmount(ZDecimal amountToAdjust)
		{
			if (amountToAdjust.IsEmpty)
			{
				return;
			}
			else
			{
				AL_LocalExtraTaxAmount += amountToAdjust;
				if (ShouldTaxAmountBeAdjustedForSplitPayments)
				{
					AL_LocalGSTAmount = -AL_LocalExtraTaxAmount;
				}
			}
		}

		bool ShouldTaxAmountBeAdjustedForSplitPayments => TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.VATRemittedByCustomer
			&& TaxAmountCalculator.UseLocalExTaxAmountToCalculateLocalTax(AL_GC);

		#endregion

		#region ILineMatching Members

		ILineMatching ThisAsILineMatching
		{
			get { return this; }
		}

		ZDecimal CalculateRelatedPaidAmount(ZDecimal sourceAmount, ZDecimal localSourceAmount, ZString sourceCurrency, ZDecimal sourceExchangeRate, ZString targetCurrency, ZDecimal targetExchangeRate)
		{
			ZDecimal result;

			if (sourceCurrency == targetCurrency)
			{
				result = sourceAmount;
			}
			else if (sourceCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				result = Env.CurrentCompany.ExchangeRate.LocalToForeign(localSourceAmount, targetExchangeRate, targetCurrency);
			}
			else if (targetCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				result = Env.CurrentCompany.ExchangeRate.ForeignToLocal(sourceAmount, sourceExchangeRate);
			}
			else
			{
				result = Env.CurrentCompany.ExchangeRate.LocalToForeign(Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(sourceAmount, sourceExchangeRate), targetExchangeRate, targetCurrency);
			}

			return result;
		}

		void ILineMatching.SetDefaultValues()
		{
			fILineMatching_AL_RX_NKTransactionCurrency = TransactionHeader.AH_RX_NKTransactionCurrency;
			fILineMatching_AL_ExchangeRate = TransactionHeader.AH_ExchangeRate;

			var isHeaderUseLocalCurrency = TransactionHeader.AH_RX_NKTransactionCurrency != AL_RX_NKTransactionCurrency;
			if (isHeaderUseLocalCurrency)
			{
				fILineMatching_AL_OSAmount = AL_LocalTotalAmount * Multiplier;
				fILineMatching_AL_OSExTaxAmount = AL_LocalExTaxAmount;
				fILineMatching_AL_OSTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, AL_GC, AL_LocalExTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), AL_OSTaxAmount, AL_ExchangeRate);
			}
			else
			{
				fILineMatching_AL_OSAmount = AL_OSAmount;
				fILineMatching_AL_OSExTaxAmount = AL_OSExTaxAmount;
				fILineMatching_AL_OSTaxAmount = AL_OSTaxAmount;
			}

			var newLocalOutstandingAmount = (AL_LocalTotalAmount * Multiplier) - TransLinePaysTotalAmount;
			var newOutstandingAmount = isHeaderUseLocalCurrency
				? (ZDecimal)newLocalOutstandingAmount
				: GetCurrentOSOutStandingAmount(newLocalOutstandingAmount == 0);

			fOriginalPaidAmount = fPaidAmount = fOutstandingAmount = newOutstandingAmount;
			fLocalOutstandingAmount = newLocalOutstandingAmount;
			fIsFullyPay = true;

			if (ThisAsILineMatching.Charge == null)
			{
				fPaidAmountInChargeCurrency = ZDecimal.Zero;
			}
			else if (ThisAsILineMatching.PaidAmount == ThisAsILineMatching.AL_OSAmount)
			{
				fPaidAmountInChargeCurrency = ThisAsILineMatching.ChargeAmount;
			}
			else
			{
				fPaidAmountInChargeCurrency = ThisAsILineMatching.PaidAmount.IsEmpty ? ZDecimal.Zero : CalculateRelatedPaidAmount(ThisAsILineMatching.PaidAmount, ThisAsILineMatching.LocalPaidAmount, ThisAsILineMatching.AL_RX_NKTransactionCurrency, ThisAsILineMatching.AL_ExchangeRate, ThisAsILineMatching.ChargeCurrency, ThisAsILineMatching.ChargeExRate);
			}
		}

		ZDecimal GetCurrentOSOutStandingAmount(bool isFullyPaid)
		{
			return isFullyPaid
				? 0m
				: AL_OSAmount - TransactionLinePayOSOutstandingAmountProvider.GetTransLinePaysTotalOSAmount(this);
		}

		void ILineMatching.UpdateOriginalAmounts()
		{
			fOriginalPaidAmount = fPaidAmount;
		}

		void ILineMatching.ResetAmounts()
		{
			fPaidAmount = ZDecimal.Zero;
			fPaidAmountInChargeCurrency = ZDecimal.Zero;
		}

		Charge fCharge;
		Charge ILineMatching.Charge
		{
			get
			{
				if (fCharge == null)
				{
					ZQuery query = new ZQuery();
					query.AddToFilter(JobChargeSchema.JR_JH, AL_JH);
					query.AddToFilter(JobChargeSchema.JR_AL_APLine, PK);
					query.AddToFilter(JoinCondition.Or, JobChargeSchema.JR_AL_ARLine, PK);

					fCharge = Factory.LoadTop1<Charge>(query);
				}

				return fCharge;
			}
		}

		ZDecimal fILineMatching_AL_OSAmount;
		ZDecimal ILineMatching.AL_OSAmount
		{
			get { return fILineMatching_AL_OSAmount; }
		}

		ZDecimal fILineMatching_AL_OSExTaxAmount;
		ZDecimal ILineMatching.AL_OSExTaxAmount
		{
			get { return fILineMatching_AL_OSExTaxAmount; }
		}

		ZDecimal fILineMatching_AL_OSTaxAmount;
		ZDecimal ILineMatching.AL_OSTaxAmount
		{
			get { return fILineMatching_AL_OSTaxAmount; }
		}

		ZDecimal fILineMatching_AL_ExchangeRate;
		ZDecimal ILineMatching.AL_ExchangeRate
		{
			get { return fILineMatching_AL_ExchangeRate; }
		}

		ZString fILineMatching_AL_RX_NKTransactionCurrency;
		ZString ILineMatching.AL_RX_NKTransactionCurrency
		{
			get { return fILineMatching_AL_RX_NKTransactionCurrency; }
		}

		ZDecimal fOutstandingAmount;
		ZDecimal ILineMatching.OutstandingAmount
		{
			get { return fOutstandingAmount; }
		}

		ZPropertyInfo ILineMatching.OutstandingAmountInfo
		{
			get { return GetZPropertyInfo("OutstandingAmount"); }
		}

		ZDecimal fLocalOutstandingAmount;
		ZDecimal ILineMatching.LocalOutstandingAmount
		{
			get { return fLocalOutstandingAmount; }
		}

		ZPropertyInfo ILineMatching.LocalOutstandingAmountInfo
		{
			get { return GetZPropertyInfo("LocalOutstandingAmount"); }
		}

		ZDecimal fPaidAmount;
		public ZDecimal PaidAmount
		{
			get { return fPaidAmount; }
			set
			{
				SetNonPersistentPropertyValue(ThisAsILineMatching.PaidAmountInfo, ref fPaidAmount, value);

				if (fPaidAmount != fOutstandingAmount)
				{
					SetNonPersistentPropertyValue(ThisAsILineMatching.IsFullyPayInfo, ref fIsFullyPay, ZBool.False);
				}
				else if (!fPaidAmount.IsEmpty)
				{
					SetNonPersistentPropertyValue(ThisAsILineMatching.IsFullyPayInfo, ref fIsFullyPay, ZBool.True);
				}

				if (ThisAsILineMatching.Charge != null)
				{
					if (value == ThisAsILineMatching.AL_OSAmount)
					{
						fPaidAmountInChargeCurrency = ThisAsILineMatching.ChargeAmount;
					}
					else
					{
						fPaidAmountInChargeCurrency = CalculateRelatedPaidAmount(ThisAsILineMatching.PaidAmount, ThisAsILineMatching.LocalPaidAmount, ThisAsILineMatching.AL_RX_NKTransactionCurrency, ThisAsILineMatching.AL_ExchangeRate, ThisAsILineMatching.ChargeCurrency, ThisAsILineMatching.ChargeExRate);
					}
				}

				if (!IsValidationSuspended && Validation is LineMatchingValidation)
				{
					((LineMatchingValidation)Validation).ValidatePaidAmount();
				}
			}
		}

		public ZPropertyInfo PaidAmountInfo
		{
			get { return GetZPropertyInfo(nameof(PaidAmount)); }
		}

		ZDecimal ILineMatching.LocalPaidAmount
		{
			get
			{
				if (PaidAmount == ThisAsILineMatching.OutstandingAmount)
				{
					return ThisAsILineMatching.LocalOutstandingAmount;
				}
				return Env.CurrentCompany.ExchangeRate.ForeignToLocal(ThisAsILineMatching.PaidAmount, ThisAsILineMatching.AL_ExchangeRate);
			}
		}

		ZPropertyInfo ILineMatching.LocalPaidAmountInfo
		{
			get { return GetZPropertyInfo("LocalPaidAmount"); }
		}

		ZDecimal fOriginalPaidAmount;
		ZDecimal ILineMatching.OriginalPaidAmount
		{
			get { return fOriginalPaidAmount; }
		}

		public ZString TransactionNumber
		{
			get { return InvoiceBase.AH_TransactionNum; }
		}

		public ZPropertyInfo TransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionNumber)); }
		}

		public ZString ConsolidatedInvoiceRef
		{
			get { return InvoiceBase.AH_ConsolidatedInvoiceRef; }
		}

		public ZPropertyInfo ConsolidatedInvoiceRefInfo
		{
			get { return GetZPropertyInfo(nameof(ConsolidatedInvoiceRef)); }
		}

		public ZString ChargeType
		{
			get { return ThisAsILineMatching.Charge == null ? ZString.Empty : ThisAsILineMatching.Charge.ChargeType; }
		}

		public ZPropertyInfo ChargeTypeInfo
		{
			get { return GetZPropertyInfo(BaseCharge.Schema.ChargeType); }
		}

		[List("Currencies")]
		public ZString ChargeCurrency
		{
			get
			{
				ZString result = ZString.Empty;

				if (ThisAsILineMatching.Charge != null)
				{
					switch (AL_LineType)
					{
						case TransactionLineTypes.Revenue:
							result = ThisAsILineMatching.Charge.JR_RX_NKSellCurrency;
							break;
						case TransactionLineTypes.Cost:
							result = ThisAsILineMatching.Charge.JR_RX_NKCostCurrency;
							break;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo ChargeCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeCurrency)); }
		}

		public ZDecimal ChargeExRate
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (ThisAsILineMatching.Charge != null)
				{
					switch (AL_LineType)
					{
						case TransactionLineTypes.Revenue:
							result = ThisAsILineMatching.Charge.JR_OSSellExRate;
							break;
						case TransactionLineTypes.Cost:
							result = ThisAsILineMatching.Charge.JR_OSCostExRate;
							break;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo ChargeExRateInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeExRate)); }
		}

		public ZDecimal ChargeAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;

				if (ThisAsILineMatching.Charge != null)
				{
					switch (AL_LineType)
					{
						case TransactionLineTypes.Revenue:
							result = ThisAsILineMatching.Charge.JR_OSSellAmt + ThisAsILineMatching.Charge.JR_OSSellGSTAmt_Calc;
							break;
						case TransactionLineTypes.Cost:
							result = ThisAsILineMatching.Charge.JR_OSCostAmt + ThisAsILineMatching.Charge.JR_OSCostGSTAmt_Calc;
							break;
					}
				}
				if (InvoiceBase != null)
				{
					ZDecimal creditNoteMultiplier = InvoiceBase.AH_TransactionType == TransactionTypes.CreditNote ? -1.0m : 1.0m;
					result *= creditNoteMultiplier;
				}

				return result * Multiplier;
			}
		}

		public ZPropertyInfo ChargeAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeAmount)); }
		}

		ZBool fIsFullyPay;
		public ZBool IsFullyPay
		{
			get { return fIsFullyPay; }
			set
			{
				SetNonPersistentPropertyValue(ThisAsILineMatching.IsFullyPayInfo, ref fIsFullyPay, value);
				ThisAsILineMatching.PaidAmount = value ? ThisAsILineMatching.OutstandingAmount : ZDecimal.Zero;
			}
		}

		public ZPropertyInfo IsFullyPayInfo
		{
			get { return GetZPropertyInfo(nameof(IsFullyPay)); }
		}

		ZDecimal fPaidAmountInChargeCurrency;
		public ZDecimal PaidAmountInChargeCurrency
		{
			get { return fPaidAmountInChargeCurrency; }
			set
			{
				SetNonPersistentPropertyValue(ThisAsILineMatching.PaidAmountInChargeCurrencyInfo, ref fPaidAmountInChargeCurrency, value);

				if (value == ThisAsILineMatching.ChargeAmount)
				{
					ThisAsILineMatching.PaidAmount = ThisAsILineMatching.AL_OSAmount;
				}
				else
				{
					ThisAsILineMatching.PaidAmount = CalculateRelatedPaidAmount(ThisAsILineMatching.PaidAmountInChargeCurrency, ThisAsILineMatching.PaidAmountInChargeCurrency, ThisAsILineMatching.ChargeCurrency, ThisAsILineMatching.ChargeExRate, ThisAsILineMatching.AL_RX_NKTransactionCurrency, ThisAsILineMatching.AL_ExchangeRate);
				}

				if (fPaidAmount != fOutstandingAmount)
				{
					SetNonPersistentPropertyValue(ThisAsILineMatching.IsFullyPayInfo, ref fIsFullyPay, ZBool.False);
				}
				else
				{
					SetNonPersistentPropertyValue(ThisAsILineMatching.IsFullyPayInfo, ref fIsFullyPay, ZBool.True);
				}
			}
		}

		public ZPropertyInfo PaidAmountInChargeCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(PaidAmountInChargeCurrency)); }
		}

		public RefCurrencyCollection Currencies
		{
			get { return Lookups.TransactionCurrencies; }
		}

		#endregion

		#region ISupportDataImporting Members

		bool ISupportDataImporting.IsImportingData { get; set; }

		#endregion

		#region Data Logging
		
		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged
		{
			get { return IsInDatabase; }
		}
		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => this.GetDefaultDataVersionLogFormatter();

		#endregion

		#region Check Creating Job Header Error

		public string CreatingJobHeaderErrorMessage { get; private set; }

		public void SetCreatingJobHeaderError(string creatingJobHeaderError)
		{
			CreatingJobHeaderErrorMessage = creatingJobHeaderError;
			AddRowError(creatingJobHeaderError);
		}

		#endregion

		protected virtual SecurityCheckpoint ModifyDefaultChargeCodeDescription
		{
			get { return Env.Security.None; }
		}

		bool IsUserAllowedToModifyDefaultChargeCodeDescription
		{
			get { return ModifyDefaultChargeCodeDescription.IsAllowed; }
		}

		void OnLineAmountChanged()
		{
			var filteredLines = InvoiceBase?.FilteredLines;

			if (filteredLines != null && filteredLines.IsNonCommittedCollectionElement(this) && !filteredLines.CollectionToFilter.Contains(this))
			{
				((ICancelAddNew)filteredLines).EndNew(((IList)filteredLines).IndexOf(this));
			}
		}

		protected override bool IsMultiSubAccountsSupportedCore => true;

		ZString IDescriptionSetter.Description { set => AL_Desc = value; }

		bool PreserveLinkedValuesOnGenericChargeChange => this.HasContext(Context.GenericChargeChanging) && (TransactionHeader?.HasContext(TransactionAllocationConverter.Context.InConversion) ?? false);

		readonly InvoicingLineTaxDateCacheProvider InvoicingLineTaxDateCacheProvider;

		enum Context
		{
			GenericChargeChanging,
			GenericChargeChangingTaxIdIsEmpty,
			GenericChargeChangingTaxIdHasValue
		}
	}
}
