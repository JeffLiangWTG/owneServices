using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.WIPAccrual
{
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public abstract class BaseWIPAccrual : Base.Transaction.TransactionLine, IHandleDeleteError
	{
		public static readonly new TypeDecider TypeDecider = new WIPAccrualTypeDecider();

		public BaseWIPAccrual(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AL_ReverseDate), ConcurrencyPolicy.Strict);
		}

		#region Readonly properties

		[ReadOnly(true)]
		public override ZString AL_RX_NKTransactionCurrency
		{
			get { return base.AL_RX_NKTransactionCurrency; }
			set
			{
				base.AL_RX_NKTransactionCurrency = value;

				if (!IsInDatabase && !IsReversed && AL_JH.IsValid && value != Job.Company.GC_RX_NKLocalCurrency)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoOnceWhenAllowed(
						PK,
						CriticalValidationInfoCollectorServiceKeyType.ACRWIPCurrencyNotSameWithLocalCurrency, () =>
						{
							var stringBuilder = new ZStringBuilder();
							var localCurrencyFromJobCompany = Invariant($"Local currency from job: company PK: {Job.Company.PK} currency: {Job.Company.GC_RX_NKLocalCurrency}.");
							stringBuilder.AppendLine(localCurrencyFromJobCompany);
							var userContextCompany = Env.CurrentUserContext.Company;
							var currentUserInfo = Invariant($"Current user context in AL_RX_NKTransactionCurrency setter: company PK: {userContextCompany.PK}, currency: {userContextCompany.LocalCurrency.Code}.");
							stringBuilder.AppendLine(currentUserInfo);
							var stackTrace = (NoResString)"Stacktrace:" + System.Environment.NewLine + System.Environment.StackTrace;
							stringBuilder.AppendLine(stackTrace);
							return stringBuilder.ToString();
						},
						CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlwaysInUnitTest_OtherwiseCollectAfterErrorReport);
				}
			}
		}

		protected bool AL_PostDate_ReadOnly
		{
			get { return !(AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value && Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod.IsAllowed); }
		}

		#region ReadOnly

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

		List<string> writableProperties;

		bool UseEditableFieldsForReadOnly;

		#endregion

		#endregion

		public ZString AlreadyReversedErrorMessage => Res.GetString("6b2a1ec6-7fd1-40c9-ba08-4a6a2d9228eb", "This transaction has already been reversed");

		public bool IsReversed
		{
			get { return !(AL_ReverseDate.IsEmpty || !AL_ReverseDate.IsValid); }
		}

		public bool CanReverseWhenRelatedJobStatusIsJFC => !(Job?.IsReadyForFinancialClosureWithoutModifySecurity ?? false);

		public bool Reverse()
		{
			return Reverse(true);
		}

		public bool Reverse(bool processApportionedCharge)
		{
			bool wasReversed = false;
			if (!IsReversed)
			{
				wasReversed = UpdateJobChargeReference(processApportionedCharge);
				if (wasReversed || ShouldReverseWhenChargeIsPartOfApportionedCost(processApportionedCharge))
				{
					UpdateAL_ReverseDate();
					wasReversed = true;
				}
			}
			return wasReversed;
		}

		public bool IsApportioned
		{
			get
			{
				bool result = false;

				if (AL_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual &&
					RelatedJobCharge != null && !RelatedJobCharge.JR_E6.IsEmpty)
				{
					ZQuery filter = new ZQuery(JobChargeSchema.JR_E6, RelatedJobCharge.JR_E6);
					filter.AddToFilter(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, RelatedJobCharge.PK);
					result = (Factory.LoadTop1<JobCharge>(filter) != null);
				}
				return result;
			}
		}

		public override void Delete()
		{
			if (!IsInDatabase)
			{
				base.Delete();
			}
			else
			{
				if (Factory.ExistsInDatabase(Schema.TableName, new ZQuery(AccTransactionLinesSchema.PK, PK)))
				{
					var errorMessageBuilder = new ZStringBuilder();

					errorMessageBuilder.AppendLine((NoResString)"Line is in db.");

					var chargeInfo = $"This WIP/ACR is not related to any job charge.";
					if (RelatedJobCharge != null)
					{
						chargeInfo = $@"ChargeInfo:
{RelatedJobCharge.GetAllPropertyValues()}";
					}
					errorMessageBuilder.AppendLine(chargeInfo);

					var lineInfo = $@"LineInfo:
{this.GetAllPropertyValues()}";
					errorMessageBuilder.AppendLine(lineInfo);

					ErrorReporter.Instance.Report((NoResString)"Cannot delete WIP/Accrual that are already in the database.", Invariant($"Line PK: {PK}, {errorMessageBuilder}"), null);
				}
			}
		}

		public ZDateTime ReversalEventDate
		{
			get
			{
				var reversalEventTime = Logs.Find(x => x.SL_SE_NKEvent == Events.TransactionReversed.Code).FirstOrDefault();
				return reversalEventTime != null ? reversalEventTime.SL_EventTime : ZDateTime.Empty;
			}
		}

		void RemoveUnsavedReverseEventsIfLineIsNotReversed()
		{
			if (!IsReversed)
			{
				List<StmALog> logsToDelete = new List<StmALog>();
				bool sendDeveloperNotification = false;
				foreach (StmALog log in Logs.LogsNotInDB)
				{
					if (log.SL_SE_NKEvent == Events.TransactionReversed.Code)
					{
						logsToDelete.Add(log);
						sendDeveloperNotification = true;
					}
				}
				foreach (StmALog log in logsToDelete)
				{
					log.Delete();
				}
				if (sendDeveloperNotification)
				{
					string additionalDetails = " PK = " + PK.ToString();
					Enterprise.ZArchitecture.Environment.Globals.Message.ShowDeveloperException(ReverseEventRemovedKey, ReverseEventRemovedKey + additionalDetails, new ZException(ReverseEventRemovedKey + additionalDetails));
				}
			}
		}

		static string ReverseEventRemovedKey => Res.GetString("bef6bda7-86bd-42e8-9cf0-1c777a3f9ca2", "Unsaved Reverse Event Removed on non-reversed WIP/ACR");

		public virtual void SetModeToReversing()
		{
			this.IsReversing = true;

			Factory.SetContext(BusinessContext.SkipJobHeaderRefreshParentDuringWIPAccrualReversing);

			using (SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
			{
				UpdateAL_ReverseDate();
				UpdateJobChargeReference();
			}
		}

		#region IHandleDeleteError

		bool IHandleDeleteError.RollbackAfterDeleteError
		{
			get { return false; }
		}

		bool IHandleDeleteError.RebindAfterDeleteError
		{
			get { return false; }
		}

		bool IHandleDeleteError.DisableFormOnDeleteConcurrencyError
		{
			get { return true; }
		}

		#endregion

		public virtual void SetValues(Job job, BaseCharge charge)
		{
			AL_Desc = charge.JR_Desc;
			AL_AC = charge.JR_AC;
			AL_ExchangeRate = 1;
			AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AL_GSTVAT = 0M;
			AL_WithholdingTax = 0M;
			AL_UnitQty = 0;
			AL_UnitPrice = 0M;
			AL_OSUnitPrice = 0M;
			AL_JH = job.PK;
			AL_GB = charge.JR_GB;
			AL_GE = charge.JR_GE;
		}

		#region Collection Filters

		protected override AccChargeCodeCollection GetChargeCodeCollectionCore()
		{
			if (Department != null)
			{
				return Factory.GetCachedValue(FindboxLookupCollections.CachingKey + "BaseWIPAccrual" + Department.GE_Code, () =>
					{
						ZQuery result = new ZQuery(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Disbursement);
						ZQuery marginTypeFilter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Margin);
						ZQuery marginPercentageFilter = new ZQuery(AccChargeCodeSchema.AC_MarginPercentage, SQLComparisonOperator.GreaterThan, 0m);
						ZQuery combinedMarginFilter = new ZQuery(marginTypeFilter, JoinCondition.And, marginPercentageFilter);
						result.AddToFilter(combinedMarginFilter, JoinCondition.Or);
						result.AddToFilter(new ZQuery(AccChargeCodeSchema.AC_IsActive, SQLComparisonOperator.Equal, true));

						var query = new ZQuery(base.GetChargeCodeCollectionCore().CompleteFilter);
						query.AddToFilter(result, JoinCondition.And);

						var collection = new AccChargeCodeCollection(Factory, query);
						return collection;
					});
			}
			else
			{
				return base.GetChargeCodeCollectionCore();
			}
		}

		#endregion

		#region Overridden Properties

		public override ZGuid AL_JH
		{
			get
			{
				return base.AL_JH;
			}
			set
			{
				base.AL_JH = value;
				if (value.IsValid)
				{
					SetBranchAndDepartment();
				}
			}
		}

		[ZUnbindableProperty()]
		public override ZDecimal AL_LocalExTaxAmount
		{
			get
			{
				return base.AL_LocalExTaxAmount;
			}
			set
			{
				base.AL_LocalExTaxAmount = value;
			}
		}

		#endregion

		#region Export Batch Transaction Reference Fields

		#region GenExportBatchSequence

		public GenExportBatchSequence ExportBatchSequencePostedObject
		{
			get
			{
				return GetExportedExportBatchSequence(Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionPostLineExport);
			}
		}

		public GenExportBatchSequence ExportBatchSequenceReversedObject
		{
			get
			{
				return GetExportedExportBatchSequence(Core.Constants.DataExportBatchSubTypes.Codes.AccountingTransactionReverseLineExport);
			}
		}

		GenExportBatchSequence GetExportedExportBatchSequence(string xbType)
		{
			ZQuery filter = new ZQuery(GenExportBatchSequenceSchema.XB_Type, xbType);
			filter.AddToFilter(GenExportBatchSequenceSchema.XB_ParentTableCode, AccTransactionLinesSchema.Constants.Prefix);
			filter.AddToFilter(GenExportBatchSequenceSchema.XB_ParentID, PK);

			return Factory.LoadTop1<GenExportBatchSequence>(filter);
		}

		#endregion

		public ZString ExportBatchTransactionReference
		{
			get
			{
				if (ExportBatchSequencePostedObject != null)
				{
					return ExportBatchSequencePostedObject.XB_BatchNumber.ToString().PadLeft(7, '0') + ExportBatchSequencePostedObject.XB_Sequence.ToString().PadLeft(5, '0');
				}
				return "";
			}
		}

		public ZPropertyInfo ExportBatchTransactionReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(ExportBatchTransactionReference)); }
		}

		public ZString ExportReverseBatchTransactionReference
		{
			get
			{
				if (ExportBatchSequenceReversedObject != null)
				{
					return ExportBatchSequenceReversedObject.XB_BatchNumber.ToString().PadLeft(7, '0') + ExportBatchSequenceReversedObject.XB_Sequence.ToString().PadLeft(5, '0');
				}
				return "";
			}
		}

		public ZPropertyInfo ExportReverseBatchTransactionReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(ExportReverseBatchTransactionReference)); }
		}

		#endregion

		#region Module Grids Binding Fields

		#region RelatedJobCharge

		internal void SetRelatedJobCharge(BaseCharge value)
		{
			relatedCharge_innerValue = value;
		}

		#endregion

		public abstract ZString RelatedCurrencyCodeValue { get; }

		public ZPropertyInfo RelatedCurrencyCodeValueInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedCurrencyCodeValue)); }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public abstract ZDecimal ForeignAmountValue { get; }

		public ZPropertyInfo ForeignAmountValueInfo
		{
			get { return GetZPropertyInfo(nameof(ForeignAmountValue)); }
		}

		public ZString JK_UniqueConsignRef
		{
			get
			{
				return Consol != null ? Consol.JK_UniqueConsignRef : ShipmentConsol != null ? ShipmentConsol.JK_UniqueConsignRef : ZString.Empty;
			}
		}

		public ZPropertyInfo JK_UniqueConsignRefInfo
		{
			get { return GetZPropertyInfo(nameof(JK_UniqueConsignRef)); }
		}

		public ZString JK_MasterBillNum
		{
			get
			{
				return Consol != null ? Consol.CostSupporter.MasterBillNum : ShipmentConsol != null ? ShipmentConsol.JK_MasterBillNum : ZString.Empty;
			}
		}

		public ZPropertyInfo JK_MasterBillNumInfo
		{
			get { return GetZPropertyInfo(nameof(JK_MasterBillNum)); }
		}

		public ZString JK_CoLoadMasterBill
		{
			get
			{
				return ShipmentConsol?.JK_CoLoadMasterBill ?? ZString.Empty;
			}
		}

		public ZPropertyInfo JK_CoLoadMasterBillInfo
		{
			get { return GetZPropertyInfo(nameof(JK_CoLoadMasterBill)); }
		}

		public ZString JS_HouseBill
		{
			get
			{
				return Shipment != null ? Shipment.JS_HouseBill : ZString.Empty;
			}
		}

		public ZPropertyInfo JS_HouseBillInfo
		{
			get { return GetZPropertyInfo(nameof(JS_HouseBill)); }
		}

		public ZString ConsignorCode
		{
			get
			{
				return Shipment != null && Shipment.ConsignorDocumentaryAddress != null && Shipment.ConsignorDocumentaryAddress.Organisation != null ?
					Shipment.ConsignorDocumentaryAddress.Organisation.OH_Code : ZString.Empty;
			}
		}

		public ZPropertyInfo ConsignorCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ConsignorCode)); }
		}

		public ZString ConsigneeCode
		{
			get
			{
				return Shipment != null && Shipment.ConsigneeDocumentaryAddress != null && Shipment.ConsigneeDocumentaryAddress.Organisation != null ?
					Shipment.ConsigneeDocumentaryAddress.Organisation.OH_Code : ZString.Empty;
			}
		}

		public ZPropertyInfo ConsigneeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ConsigneeCode)); }
		}

		#endregion

		#region Validation - using New Validation

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			if (IsReversing)
			{
				return new BaseWIPAccrualReverseValidation(this);
			}
			else if (IsInDatabase)
			{
				return GetEmptyValidation();
			}
			else
			{
				return new BaseWIPAccrualValidation(this);
			}
		}

		#endregion

		#region Implementation

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AL_PostDate = ZDateTime.Now;
		}

		#endregion

		public bool IsReversing
		{
			get { return this.HasContext(BusinessContext.WipAccrualReversing); }
			set
			{
				if (value)
				{
					this.SetContext(BusinessContext.WipAccrualReversing);
				}
				else
				{
					this.RemoveContext(BusinessContext.WipAccrualReversing);
				}
			}
		}

		protected override ZDateTime GetOriginalDateOrRevenueRecognitionDate(ZDateTime originalDate)
		{
			ZDateTime resultDate = base.GetOriginalDateOrRevenueRecognitionDate(originalDate);
			return resultDate.IsEmpty ? originalDate : resultDate;
		}

		public void UpdateAL_PostDate()
		{
			var relatedJobCharge = RelatedJobCharge;
			AL_PostDate = GetOriginalDateOrRevenueRecognitionDate(
							relatedJobCharge != null && !relatedJobCharge.WIPAccrualCreationDate.IsEmpty ?
							relatedJobCharge.WIPAccrualCreationDate : ZDateTime.Now);
		}

		protected bool UpdateJobChargeReference()
		{
			return UpdateJobChargeReference(true);
		}

		protected bool UpdateJobChargeReference(bool processApportionedCharge)
		{
			bool wasUpdated = false;
			Charge charge = Factory.LoadTop1<Charge>(new ZQuery(JobChargeRelatedLineFilterField, PK));
			if (charge != null && (ShouldReverseWhenChargeIsPartOfApportionedCost(processApportionedCharge) || !charge.JR_IsApportioned))
			{
				UpdateJobChargeLink(charge);
				wasUpdated = true;
			}
			return wasUpdated;
		}

		protected virtual bool ShouldReverseWhenChargeIsPartOfApportionedCost(bool processApportionedCharge)
		{
			return processApportionedCharge;
		}

		internal protected abstract void UpdateJobChargeLink(Charge charge);

		protected void SetBranchAndDepartment()
		{
			if (Job != null)
			{
				AL_GB = Job.JH_GB;
				AL_GE = Job.JH_GE;
			}
		}

		protected virtual SecurityCheckpoint FutureOrPreviousPeriodPostingCheckPoint
		{
			get { return Env.Security.JobCostingPostToPreviousOrFutureOpenPeriod; }
		}

		public bool IsEditingReverseDateAllowed
		{
			get { return FutureOrPreviousPeriodPostingCheckPoint.IsAllowed; }
		}

		IJobCostingPlugIn Consol
		{
			get { return RelatedJobCharge != null && RelatedJobCharge.ParentConsolCost != null ? RelatedJobCharge.ParentConsolCost.Consol : null; }
		}

		CommonShipment Shipment
		{
			get
			{
				if (!IsShipmentInitialaised)
				{
					ZDBOnlyQuery shipmentQuery = new ZDBOnlyQuery(typeof(CommonShipment));
					ZDBOnlySubQuery jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);

					jobHeaderQuery.AddToFilter(JobHeaderSchema.PK, AL_JH);
					shipmentQuery.AddSubQuery(jobHeaderQuery, JoinCondition.And);

					fShipment = Factory.LoadTop1<CommonShipment>(shipmentQuery);
					IsShipmentInitialaised = true;
				}
				return fShipment;
			}
		}
		CommonShipment fShipment;
		bool IsShipmentInitialaised;

		CommonConsol ShipmentConsol
		{
			get { return Shipment != null && Shipment.Consols.Count > 0 ? Shipment.Consols[0] : null; }
		}

		#endregion

		protected override ZDateTime GetAL_ReverseDateForImmediateRevenueRecognition()
		{
			ZDateTime reverseDate = ZDateTime.Now;
			return RelatedJobCharge != null && !RelatedJobCharge.WIPAccrualReverseDate.IsEmpty ?
					RelatedJobCharge.WIPAccrualReverseDate :
					(AL_PostDate > reverseDate ? AL_PostDate : reverseDate);
		}

		#region ISupportCriticalValidation

		protected override ICriticalValidation GetCriticalValidation()
		{
			return new BaseWIPAccrualCriticalValidation(this);
		}

		#endregion

		public override void OnSavingCore()
		{
			base.OnSavingCore();
			if (IsReversed && (AL_ReverseDateInfo.OriginalValue.IsEmpty || !AL_ReverseDateInfo.OriginalValue.IsValid))
			{
				Logs.AddNew(Events.TransactionReversed);
			}
			RemoveUnsavedReverseEventsIfLineIsNotReversed();
		}
	}

	#region Type Decider

	public class WIPAccrualTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			Type result = null;
			if (row[AccTransactionLinesSchema.Constants.AL_LineType].ToString() == ZArchitecture.Core.TransactionLineTypes.WIP)
			{
				result = typeof(WIP);
			}
			else if (row[AccTransactionLinesSchema.Constants.AL_LineType].ToString() == ZArchitecture.Core.TransactionLineTypes.Accrual)
			{
				result = typeof(Accrual);
			}
			else
			{
				throw new NotSupportedException("Only WIP Rows and Accrual Rows can be passed into this Type Decider");
			}
			return result;
		}

		public override Type GetTypeForNew()
		{
			throw new NotSupportedException("No default concrete type for WIP/Accrual");
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public static bool IsApplicableTo(DataRow row)
		{
			string lineType = row[AccTransactionLinesSchema.Constants.AL_LineType].ToString();
			return lineType == ZArchitecture.Core.TransactionLineTypes.WIP || lineType == ZArchitecture.Core.TransactionLineTypes.Accrual;
		}
		#endregion

	}
}
