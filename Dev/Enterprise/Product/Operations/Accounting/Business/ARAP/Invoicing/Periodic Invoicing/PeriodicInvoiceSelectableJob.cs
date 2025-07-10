using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing
{
	public interface IIncludeInThePeriodicInvoiceChangedObserver
	{
		void Notify(object sender);
	}

	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public class PeriodicInvoiceSelectableJob : NonPersistentBusinessObject, IObsoleteValidation, IWrapPersistentBizO
	{
		#region Schema
		public static class Schema
		{
			public const string TableName = Job.Schema.TableName;
			public const string JH_JobNum = Job.Schema.JH_JobNum;
			public const string JH_Status = Job.Schema.JH_Status;
			public const string JH_GB = Job.Schema.JH_GB;
			public const string JH_GE = Job.Schema.JH_GE;
			public const string JH_A_JOP = Job.Schema.JH_A_JOP;
			public const string JH_GS_NKRepOps = Job.Schema.JH_GS_NKRepOps;
			public const string JH_GS_NKRepSales = Job.Schema.JH_GS_NKRepSales;
			public const string JH_MasterBillNo = Job.Schema.JH_MasterBillNo;
			public const string JH_HouseBillNo = Job.Schema.JH_HouseBillNo;
			public const string JH_ConsolNo = Job.Schema.JH_ConsolNo;
			public const string JH_OSAmountForPeriodicBilling = Job.Schema.JH_OSAmountForPeriodicBilling;
			public const string JH_OSTaxAmountForPeriodicBilling = Job.Schema.JH_OSTaxAmountForPeriodicBilling;
			public const string JH_LocalAmountForPeriodicBilling = Job.Schema.JH_LocalAmountForPeriodicBilling;
			public const string JH_LocalTaxAmountForPeriodicBilling = Job.Schema.JH_LocalTaxAmountForPeriodicBilling;
			public const string JH_OSExtraTaxAmount = Job.Schema.JH_OSExtraTaxAmount;
			public const string JH_LocalExtraTaxAmount = Job.Schema.JH_LocalExtraTaxAmount;
			public const string JH_SystemCreateUser = Job.Schema.JH_SystemCreateUser;
			public const string JH_SystemCreateTimeUtc = Job.Schema.JH_SystemCreateTimeUtc;
			public const string JH_SystemLastEditUser = Job.Schema.JH_SystemLastEditUser;
			public const string JH_SystemLastEditTimeUtc = Job.Schema.JH_SystemLastEditTimeUtc;
			public const string IncludeInThePeriodicInvoice = "IncludeInThePeriodicInvoice";
			public const string JobReasonCode = "JobReasonCode";
			public const string LocalChargesPK = "LocalChargesPK";
			public const string AgentCollectPK = "AgentCollectPK";
			public const string RevenueRecognitionDates = "RevenueRecognitionDates";
			public const string Currency = "Currency";
			public const string JH_JS_OrderReferences = Job.Schema.JH_JS_OrderReferences;
			public const string AdditionalReferenceAsString = Job.Schema.AdditionalReferenceAsString;
			public const string JH_JS_JK_VoyageFlight = Job.Schema.JH_JS_JK_VoyageFlight;
			public const string JH_JS_JK_Vessel = Job.Schema.JH_JS_JK_Vessel;
			public const string JH_GB_TaxBranch = Job.Schema.JH_GB_TaxBranch;
			public const string ChargeTaxBranches = "ChargeTaxBranches";
		}
		#endregion

		readonly IIncludeInThePeriodicInvoiceChangedObserver includeInThePeriodicInvoiceChangedObserver;

		public PeriodicInvoiceSelectableJob(BusinessObjectFactory factory, IIncludeInThePeriodicInvoiceChangedObserver observer, Job job)
			: base(job.Factory, ((INeedRow)job).Row)
		{
			Argument.NotNull(observer, "observer");
			Argument.NotNull(job, "job");
			this.parent = job;
			this.includeInThePeriodicInvoiceChangedObserver = observer;
		}

		#region Readonly

		protected bool GetPropertyReadonlyness(PropertyDescriptor property)
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

		List<string> writableProperties;

		bool UseEditableFieldsForReadOnly;

		#endregion

		#region IncludeInThePeriodicInvoice Property

		ZBool includeInThePeriodicInvoice;
		public ZBool IncludeInThePeriodicInvoice
		{
			get
			{
				return includeInThePeriodicInvoice;
			}
			set
			{
				bool hasChanged = includeInThePeriodicInvoice != value;
				SetNonPersistentPropertyValue(IncludeInThePeriodicInvoiceInfo, ref includeInThePeriodicInvoice, value);
				if (hasChanged)
				{
					using (GetValidationSuspender())
					{
						includeInThePeriodicInvoiceChangedObserver.Notify(this);
					}
					IncludeRelativeValidation();
				}
			}
		}

		internal void SetDefaultForIncludeInThePeriodicInvoice()
		{
			using (GetValidationSuspender())
			{
				IncludeInThePeriodicInvoice = !IsOnHold && !IsReadyForFinancialClosureWithoutPostSecurity;
			}
		}

		bool IsOnHold
		{
			get
			{
				return Parent != null && (Parent.IsWorkOnHold || Parent.IsInvoiceOnHold);
			}
		}

		bool IsReadyForFinancialClosureWithoutPostSecurity
		{
			get
			{
				return Parent != null && Parent.IsReadyForFinancialClosureWithoutPostSecurity;
			}
		}

		public virtual ZPropertyInfo IncludeInThePeriodicInvoiceInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.IncludeInThePeriodicInvoice);
			}
		}

		#endregion

		#region JobReasonCode Property

		public ZString JobReasonCode
		{
			get
			{
				return Parent == null ? null : Parent.JH_ProfitLossReasonCode;
			}
		}

		public virtual ZPropertyInfo JobReasonCodeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.JobReasonCode);
			}
		}

		#endregion

		#region Properties Based On A Job Property

		public ZDateTime JH_A_JOP { get { return Parent.JH_A_JOP; } }
		public ZPropertyInfo JH_A_JOPInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_A_JOP, x => Parent.JH_A_JOPInfo); } }

		public ZString JH_ConsolNo { get { return Parent.JH_ConsolNo; } }
		public ZPropertyInfo JH_ConsolNoInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_ConsolNo, x => Parent.JH_ConsolNoInfo); } }

		[List("Lookups.Branches")]
		public ZGuid JH_GB { get { return Parent.JH_GB; } }
		public ZPropertyInfo JH_GBInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_GB, x => Parent.JH_GBInfo); } }

		[List("Lookups.Branches")]
		public ZGuid JH_GB_TaxBranch { get { return Parent.JH_GB_TaxBranch; } }
		public ZPropertyInfo JH_GB_TaxBranchInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_GB_TaxBranch, x => Parent.JH_GB_TaxBranchInfo); } }

		public ZString ChargeTaxBranches
		{
			get
			{
				return fChargeTaxBranches;
			}
			set
			{
				SetNonPersistentPropertyValue(ChargeTaxBranchesInfo, ref fChargeTaxBranches, value);
			}
		}
		ZString fChargeTaxBranches;

		public ZPropertyInfo ChargeTaxBranchesInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeTaxBranches)); }
		}

		[List("Lookups.Departments")]
		public ZGuid JH_GE { get { return Parent.JH_GE; } }
		public ZPropertyInfo JH_GEInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_GE, x => Parent.JH_GEInfo); } }

		[List("Lookups.RepOps")]
		public ZString JH_GS_NKRepOps { get { return Parent.JH_GS_NKRepOps; } }
		public ZPropertyInfo JH_GS_NKRepOpsInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_GS_NKRepOps, x => Parent.JH_GS_NKRepOpsInfo); } }

		[List("Lookups.RepSales")]
		public ZString JH_GS_NKRepSales { get { return Parent.JH_GS_NKRepSales; } }
		public ZPropertyInfo JH_GS_NKRepSalesInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_GS_NKRepSales, x => Parent.JH_GS_NKRepSalesInfo); } }

		public ZString JH_HouseBillNo { get { return Parent.JH_HouseBillNo; } }
		public ZPropertyInfo JH_HouseBillNoInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_HouseBillNo, x => Parent.JH_HouseBillNoInfo); } }

		public ZString JH_JobNum { get { return Parent.JH_JobNum; } }
		public ZPropertyInfo JH_JobNumInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_JobNum, x => Parent.JH_JobNumInfo); } }

		public ZString JH_JS_OrderReferences { get { return Parent.JH_JS_OrderReferences; } }
		public ZPropertyInfo JH_JS_OrderReferencesInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_JS_OrderReferences, x => Parent.JH_JS_OrderReferencesInfo); } }

		public ZString JH_JS_JK_Vessel { get { return Parent.JH_JS_JK_Vessel; } }
		public ZPropertyInfo JH_JS_JK_VesselInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_JS_JK_Vessel, x => Parent.JH_JS_JK_VesselInfo); } }

		public ZString JH_JS_JK_VoyageFlight { get { return Parent.JH_JS_JK_VoyageFlight; } }
		public ZPropertyInfo JH_JS_JK_VoyageFlightfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_JS_JK_VoyageFlight, x => Parent.JH_JS_JK_VoyageFlightInfo); } }

		public ZString AdditionalReferenceAsString { get { return Parent.AdditionalReferenceAsString; } }
		public ZPropertyInfo AdditionalReferenceAsStringInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.AdditionalReferenceAsString, x => Parent.AdditionalReferenceAsStringInfo); } }

		public ZString JH_MasterBillNo { get { return Parent.JH_MasterBillNo; } }
		public ZPropertyInfo JH_MasterBillNoInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_MasterBillNo, x => Parent.JH_MasterBillNoInfo); } }

		public ZString JH_Status { get { return Parent.JH_Status; } }
		public ZPropertyInfo JH_StatusInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_Status, x => Parent.JH_StatusInfo); } }

		[List("Lookups.LocalCharges")]
		public ZGuid LocalChargesPK { get { return Parent.LocalChargesPK; } }
		public ZPropertyInfo LocalChargesPKInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.LocalChargesPK, x => Parent.LocalChargesPKInfo); } }

		[List("Lookups.AgentCollects")]
		public ZGuid AgentCollectPK { get { return Parent.AgentCollectPK; } }
		public ZPropertyInfo AgentCollectPKInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.AgentCollectPK, x => Parent.AgentCollectPKInfo); } }

		public ZString RevenueRecognitionDates { get { return Parent.RevenueRecognitionDates; } }
		public ZPropertyInfo RevenueRecognitionDatesInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.RevenueRecognitionDates, x => Parent.RevenueRecognitionDatesInfo); } }

		public ZString JH_SystemCreateUser { get { return Parent.JH_SystemCreateUser; } }
		public ZPropertyInfo JH_SystemCreateUserInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_SystemCreateUser, x => Parent.JH_SystemCreateUserInfo); } }

		public ZDateTime JH_SystemCreateTimeUtc { get { return Parent.JH_SystemCreateTimeUtc; } }
		public ZPropertyInfo JH_SystemCreateTimeUtcInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_SystemCreateTimeUtc, x => Parent.JH_SystemCreateTimeUtcInfo); } }

		public ZString JH_SystemLastEditUser { get { return Parent.JH_SystemLastEditUser; } }
		public ZPropertyInfo JH_SystemLastEditUserInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_SystemLastEditUser, x => Parent.JH_SystemLastEditUserInfo); } }

		public ZDateTime JH_SystemLastEditTimeUtc { get { return Parent.JH_SystemLastEditTimeUtc; } }
		public ZPropertyInfo JH_SystemLastEditTimeUtcInfo { get { return Parent == null ? null : GetWrappedZPropertyInfo(Schema.JH_SystemLastEditTimeUtc, x => Parent.JH_SystemLastEditTimeUtcInfo); } }

		public ZString JobType { get { return Parent.JobType.Code; } }
		public ZString TransportMode { get { return Parent.TransportMode; } }
		public ZString ServiceDirection { get { return Parent.ServiceDirection; } }
		public ZString ServiceLevel => Parent.ServiceLevel;
		public ZString LayoutWhenPrintedInPeriodicInvoice { get { return Parent.LayoutWhenPrintedInPeriodicInvoice; } }
		public ZString SecondaryLayoutWhenPrintedInPeriodicInvoice { get { return Parent.SecondaryLayoutWhenPrintedInPeriodicInvoice; } }

		#endregion Properties Based On A Job Property

		#region Properties For Periodic Invoice Display 

		#region Currency Property

		ZString currency;

		[MaxLength(3)]
		public ZString Currency
		{
			get { return currency; }
			set { SetNonPersistentPropertyValue(CurrencyInfo, ref currency, value); }
		}

		public virtual ZPropertyInfo CurrencyInfo { get { return GetZPropertyInfo(Schema.Currency); } }

		public ZInt LocalDecimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		public ZPropertyInfo LocalDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(LocalDecimals)); }
		}

		public ZInt OSDecimals
		{
			get
			{
				if (!string.IsNullOrEmpty(currency))
				{
					RefCurrency oSCurrency = RefCurrency.LoadFromCurrencyCode(Factory, currency);
					return oSCurrency.Decimals;
				}
				return GlbCompany.CurrentCompany.LocalCurrency.Decimals;
			}
		}

		public ZPropertyInfo OSDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(OSDecimals)); }
		}

		#endregion
		#region JH_OSAmountForPeriodicBilling Property

		ZDecimal jh_OSAmountForPeriodicBilling;

		public ZDecimal JH_OSAmountForPeriodicBilling
		{
			get
			{
				return jh_OSAmountForPeriodicBilling;
			}
			set
			{
				SetNonPersistentPropertyValue(JH_OSAmountForPeriodicBillingInfo, ref jh_OSAmountForPeriodicBilling, value);
			}
		}

		public virtual ZPropertyInfo JH_OSAmountForPeriodicBillingInfo { get { return GetZPropertyInfo(Schema.JH_OSAmountForPeriodicBilling); } }

		#endregion
		#region JH_OSTaxAmountForPeriodicBilling Property

		ZDecimal jh_OSTaxAmountForPeriodicBilling;

		public ZDecimal JH_OSTaxAmountForPeriodicBilling
		{
			get
			{
				return jh_OSTaxAmountForPeriodicBilling;
			}
			set
			{
				SetNonPersistentPropertyValue(JH_OSTaxAmountForPeriodicBillingInfo, ref jh_OSTaxAmountForPeriodicBilling, value);
			}
		}

		public virtual ZPropertyInfo JH_OSTaxAmountForPeriodicBillingInfo { get { return GetZPropertyInfo(Schema.JH_OSTaxAmountForPeriodicBilling); } }

		#endregion
		#region JH_LocalAmountForPeriodicBilling Property

		ZDecimal jh_LocalAmountForPeriodicBilling;

		public ZDecimal JH_LocalAmountForPeriodicBilling
		{
			get
			{
				return jh_LocalAmountForPeriodicBilling;
			}
			set
			{
				SetNonPersistentPropertyValue(JH_LocalAmountForPeriodicBillingInfo, ref jh_LocalAmountForPeriodicBilling, value);
			}
		}

		public virtual ZPropertyInfo JH_LocalAmountForPeriodicBillingInfo { get { return GetZPropertyInfo(Schema.JH_LocalAmountForPeriodicBilling); } }

		#endregion
		#region JH_LocalTaxAmountForPeriodicBilling Property

		ZDecimal jh_LocalTaxAmountForPeriodicBilling;

		public ZDecimal JH_LocalTaxAmountForPeriodicBilling
		{
			get
			{
				return jh_LocalTaxAmountForPeriodicBilling;
			}
			set
			{
				SetNonPersistentPropertyValue(JH_LocalTaxAmountForPeriodicBillingInfo, ref jh_LocalTaxAmountForPeriodicBilling, value);
			}
		}

		public virtual ZPropertyInfo JH_LocalTaxAmountForPeriodicBillingInfo { get { return GetZPropertyInfo(Schema.JH_LocalTaxAmountForPeriodicBilling); } }

		#endregion
		#region JH_OSExtraTaxAmount Property

		ZDecimal jh_OSExtraTaxAmount;

		public ZDecimal JH_OSExtraTaxAmount
		{
			get
			{
				return jh_OSExtraTaxAmount;
			}
			set
			{
				SetNonPersistentPropertyValue(JH_OSExtraTaxAmountInfo, ref jh_OSExtraTaxAmount, value);
			}
		}

		public virtual ZPropertyInfo JH_OSExtraTaxAmountInfo { get { return GetZPropertyInfo(Schema.JH_OSExtraTaxAmount); } }

		#endregion
		#region JH_LocalExtraTaxAmount Property

		ZDecimal jh_LocalExtraTaxAmount;

		public ZDecimal JH_LocalExtraTaxAmount
		{
			get
			{
				return jh_LocalExtraTaxAmount;
			}
			set
			{
				SetNonPersistentPropertyValue(JH_LocalExtraTaxAmountInfo, ref jh_LocalExtraTaxAmount, value);
			}
		}
		public virtual ZPropertyInfo JH_LocalExtraTaxAmountInfo { get { return GetZPropertyInfo(Schema.JH_LocalExtraTaxAmount); } }

		#endregion

		#endregion Properties For Periodic Invoice Display

		#region Lookups Property

		public JobHeaderLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected virtual JobHeaderLookups GetNewLookups()
		{
			return new JobHeaderLookups(Parent);
		}

		JobHeaderLookups fLookups;

		#endregion

		#region Non Bound Properties

		public ZGuid JH_ParentID
		{
			get
			{
				return Parent.JH_ParentID;
			}
		}

		#region Parent Job Property

		readonly Job parent;

		public override CargoWise.Schema.SchemaGuidColumn PKSchemaColumn
		{
			get
			{
				return ZArchitecture.Schema.JobHeaderSchema.PK;
			}
		}

		BusinessObject IWrapPersistentBizO.Parent { get { return Parent; } }

		public Job Parent { get { return parent; } }

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			IncludeRelativeValidation();
		}

		void IncludeRelativeValidation()
		{
			ValidateIncludeInThePeriodicInvoice();
			ValidateJobReasonCode();
		}

		public event EventHandler ValidateIncludeInThePeriodicInvoiceEventHandler;
		void ValidateIncludeInThePeriodicInvoice()
		{
			if (currentValidationMode != ValidationMode.NoValidation && !IsValidationSuspended)
			{
				ValidateIncludeInThePeriodicInvoiceEventHandler?.Invoke(this, EventArgs.Empty);
				IncludeInThePeriodicInvoiceInfo.ClearAllNotifications();

				if (currentValidationMode == ValidationMode.EmptyValidation)
				{
					return;
				}

				if (IsOnHold)
				{
					var message = Res.GetString("7933FA05-B2A4-43E4-A93B-18E0EAA8D502", "Jobs with status 'Invoice on hold' or 'Work on hold' cannot be included in the invoice.");
					if (IncludeInThePeriodicInvoice)
					{
						if (currentValidationMode != ValidationMode.WarningsOnly)
						{
							IncludeInThePeriodicInvoiceInfo.AddError(message);
						}
					}
					else
					{
						IncludeInThePeriodicInvoiceInfo.AddWarning(message);
					}
				}

				if (IsReadyForFinancialClosureWithoutPostSecurity)
				{
					var message = AccountingConstants.JobIsReadyForFinancialClosureWithoutPostSecurityErrorMessage;
					if (IncludeInThePeriodicInvoice)
					{
						if (currentValidationMode != ValidationMode.WarningsOnly)
						{
							IncludeInThePeriodicInvoiceInfo.AddError(message);
						}
					}
					else
					{
						IncludeInThePeriodicInvoiceInfo.AddWarning(message);
					}
				}
			}
		}

		void ValidateJobReasonCode()
		{
			if (currentValidationMode == ValidationMode.NoValidation || IsValidationSuspended)
			{
				return;
			}

			JobReasonCodeInfo.ClearAllNotifications();

			if (currentValidationMode == ValidationMode.EmptyValidation)
			{
				return;
			}

			if (IncludeInThePeriodicInvoice)
			{
				if (Parent.JH_ProfitLossReasonCode.IsEmpty
				&& AccountingConfigurationRegistry.Instance.DoesJobStatusGetChangedToInvoicedWhenARInvoicePosted(Parent.JH_Status)
				&& ((JobValidation)Parent.Validation).IsProfitLossReasonCodeInvalidForThisJobStatus(JobHeaderStatus.JobInvoiced.Code))
				{
					var message = Res.GetString("CF2E5D52-C2DC-4b68-817F-79953FB7D548", "Job {0} status will be changed to INV after posting the first AR Invoice. The Profit/Loss threshold settings require Profit/Loss reason to be set on this job before posting any AR invoices. Posting is prohibited and Invoice preview not available until Profit / Loss reason has been entered. ", JH_JobNum);
					JobReasonCodeInfo.AddError(message);
				}
			}
		}

		public enum ValidationMode
		{
			FullValidation,
			WarningsOnly,
			EmptyValidation,
			NoValidation
		}

		ValidationMode currentValidationMode;

		internal void SetCurrentValidationMode(ValidationMode validationMode)
		{
			currentValidationMode = validationMode;
			if (validationMode == ValidationMode.FullValidation)
			{
				using (new DisposableAction(() => currentValidationMode = ValidationMode.WarningsOnly, () => currentValidationMode = ValidationMode.FullValidation))
				{
					RunPreSaveValidation();
				}
			}
			else if (validationMode != ValidationMode.NoValidation)
			{
				RunPreSaveValidation();
			}
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing.Testing
{
	public class DummyObserver : IIncludeInThePeriodicInvoiceChangedObserver
	{
		public void Notify(object sender) { }
	}
}

#endif
#endregion
