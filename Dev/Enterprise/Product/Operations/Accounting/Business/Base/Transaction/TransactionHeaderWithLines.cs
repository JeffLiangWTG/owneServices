using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public abstract partial class TransactionHeaderWithLines : TransactionHeader, ITransactionHeaderWithLines, IAuditParent
	{
		public new abstract class Schema : TransactionHeader.Schema
		{
			public const string AH_OSExtraTaxAmount = "AH_OSExtraTaxAmount";
			public const string AH_OSEDUPrimaryAmount = "AH_OSEDUPrimaryAmount";
			public const string AH_OSEDUSecondaryAmount = "AH_OSEDUSecondaryAmount";
			public const string AH_LocalExtraTaxAmount = "AH_LocalExtraTaxAmount";
			public const string AH_LocalEDUPrimaryAmount = "AH_LocalEDUPrimaryAmount";
			public const string AH_LocalEDUSecondaryAmount = "AH_LocalEDUSecondaryAmount";
			public const string AH_Calc_TaxBranchName = "AH_Calc_TaxBranchName";
		}

		public TransactionHeaderWithLines(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override AccTransactionHeaderCriticalValidation GetCriticalValidation()
		{
			return new TransactionHeaderWithLinesCriticalValidation(this);
		}

		#region DependentTransactionLineCollection

		public bool LinesHaveBeenLoaded
		{
			get { return fLines != null; }
		}

		[ChildEditable(true)]
		public DependentTransactionLineCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					CreateAndLoadLines();
				}
				return fLines;
			}
		}
		DependentTransactionLineCollection fLines;

		public DependentTransactionLineCollection GetComplianceRelatedLines(AccComplianceDocumentHeader compliance)
		{
			var complianceRelatedLines = GetDependentLinesCollection();
			complianceRelatedLines.Load(ComplianceRelatedLineCollectionLoadQuery(compliance));
			return complianceRelatedLines;
		}

		public ZQuery ComplianceRelatedLineCollectionLoadQuery(AccComplianceDocumentHeader compliance)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionLines));
			var subQuery = new ZDBOnlySubQuery(typeof(AccComplianceDocumentPivot), AccComplianceDocumentPivotSchema.ADP_AL);
			var subQuery1 = new ZDBOnlySubQuery(typeof(AccComplianceDocumentLine), AccComplianceDocumentLineSchema.PK);
			subQuery1.AddToFilter(AccComplianceDocumentLineSchema.ADL_ADH, compliance.PK);
			subQuery.AddSubQuery(AccComplianceDocumentPivotSchema.ADP_ADL, subQuery1, JoinCondition.And);
			query.AddSubQuery(AccTransactionLinesSchema.PK, subQuery, JoinCondition.And);
			return query;
		}

		public DependentTransactionLineCollection GetComplianceRelatedLines(AccTransactionLinesCollection lines)
		{
			var complianceRelatedLines = GetDependentLinesCollection();
			complianceRelatedLines.Load(new ZQuery(AccTransactionLinesSchema.PK, lines.Select(x => x.PK)));
			return complianceRelatedLines;
		}

		protected abstract DependentTransactionLineCollection GetDependentLinesCollection();

		#endregion

		public abstract Type DependentTransactionLineType { get; }

		#region ValidateAH_OSTotalAmountSuspender

		internal FunctionalitySuspender ValidateAH_OSTotalAmountSuspender
		{
			get
			{
				return validateAH_OSTotalAmountSuspender ?? (validateAH_OSTotalAmountSuspender =
					new FunctionalitySuspender(() =>
					{
						var validataion = Validation as TransactionHeaderWithLinesValidation;
						if (validataion != null)
						{
							validataion.ValidateAH_OSTotalAmount();
						}
					},
					true));
			}
		}
		FunctionalitySuspender validateAH_OSTotalAmountSuspender;

#if DEBUG
		public int ValidateAH_OSTotalAmountCallCount_ForTestOnly
		{
			get;
			set;
		}
#endif
		#endregion

		#region Header Amounts Update Suspender

		public IDisposable GetHeaderAmountsUpdateSuspender()
		{
			return new HeaderAmountsUpdateSuspender(this);
		}

		internal protected bool IsHeaderAmountsUpdateSuspended
		{
			get { return HeaderAmountsUpdateSuspenderCount > 0; }
		}

		class HeaderAmountsUpdateSuspender : IDisposable
		{
			internal HeaderAmountsUpdateSuspender(TransactionHeaderWithLines parent)
			{
				Parent = parent;
				Parent.HeaderAmountsUpdateSuspenderCount++;
			}

			void IDisposable.Dispose()
			{
				Parent.HeaderAmountsUpdateSuspenderCount--;

				if (Parent.HeaderAmountsUpdateSuspenderCount == 0)
				{
					using (Parent.ValidateAH_OSTotalAmountSuspender.GetSuspender())
					{
						if (Parent.pendingUpdateAH_OSExTaxAmount)
						{
							Parent.UpdateAH_OSExTaxAmount();
						}
						if (Parent.pendingUpdateAH_LocalExTaxAmount)
						{
							Parent.UpdateAH_LocalExTaxAmount();
						}
						if (Parent.pendingUpdateAH_OSTaxAmount)
						{
							Parent.UpdateAH_OSTaxAmount();
						}
						if (Parent.pendingUpdateAH_LocalTaxAmount)
						{
							Parent.UpdateAH_LocalTaxAmount();
						}
						if (Parent.pendingUpdateAH_OSTotalAmount)
						{
							Parent.UpdateAH_OSTotalAmount();
						}
						if (Parent.pendingUpdateAH_OSWHTAmount)
						{
							Parent.UpdateAH_OSWHTAmount();
						}
						if (Parent.pendingUpdateAH_LocalWHTAmount)
						{
							Parent.UpdateAH_LocalWHTAmount();
						}
						if (Parent.pendingUpdateAH_OSExtraTaxAmount)
						{
							Parent.UpdateAH_OSExtraTaxAmount();
						}
						if (Parent.pendingUpdateAH_LocalExtraTaxAmount)
						{
							Parent.UpdateAH_LocalExtraTaxAmount();
						}
					}
				}
			}

			readonly TransactionHeaderWithLines Parent;
		}

		int HeaderAmountsUpdateSuspenderCount;

		#endregion

		#region Update AH_LocalExTaxAmount

		public void UpdateAH_LocalExTaxAmount()
		{
			if (!IsHeaderAmountsUpdateSuspended)
			{
				UpdateAH_LocalExTaxAmount(0M);
				pendingUpdateAH_LocalExTaxAmount = false;
			}
			else
			{
				pendingUpdateAH_LocalExTaxAmount = true;
			}
		}

		bool pendingUpdateAH_LocalExTaxAmount;

		public void UpdateAH_LocalExTaxAmount(ZDecimal defaultNewAmount)
		{
			AH_LocalExTaxAmount = GetSumOfLines(DependentTransactionLine.Schema.AL_LocalExTaxAmount) + defaultNewAmount;
			AH_LocalExTaxAmountInfo.RefreshBinding();

			if (IsLocalCurrencyTransaction)
			{
				UpdateAH_OSExTaxAmount(defaultNewAmount);
			}
		}

		#endregion

		#region Update AH_LocalTaxAmount

		public void UpdateAH_LocalTaxAmount()
		{
			if (!IsHeaderAmountsUpdateSuspended)
			{
				UpdateAH_LocalTaxAmount(0m);
				pendingUpdateAH_LocalTaxAmount = false;
			}
			else
			{
				pendingUpdateAH_LocalTaxAmount = true;
			}
		}

		bool pendingUpdateAH_LocalTaxAmount;

		public void UpdateAH_LocalTaxAmount(ZDecimal defaultNewAmount)
		{
			AH_LocalTaxAmount = GetSumOfLines(DependentTransactionLine.Schema.AL_LocalTaxAmount) + defaultNewAmount;
			AH_LocalTaxAmountInfo.RefreshBinding();

			if (IsLocalCurrencyTransaction)
			{
				UpdateAH_OSTaxAmount(defaultNewAmount);
			}
		}

		#endregion

		#region Update AH_LocalWHTAmount

		public void UpdateAH_LocalWHTAmount()
		{
			if (!IsHeaderAmountsUpdateSuspended)
			{
				UpdateAH_LocalWHTAmount(0m);
				pendingUpdateAH_LocalWHTAmount = false;
			}
			else
			{
				pendingUpdateAH_LocalWHTAmount = true;
			}
		}

		bool pendingUpdateAH_LocalWHTAmount;

		public void UpdateAH_LocalWHTAmount(ZDecimal defaultNewAmount)
		{
			AH_LocalWHTAmount = GetSumOfLines(DependentTransactionLine.Schema.AL_LocalWHTAmount) + defaultNewAmount;
			AH_LocalWHTAmountInfo.RefreshBinding();

			if (IsLocalCurrencyTransaction)
			{
				UpdateAH_OSWHTAmount(defaultNewAmount);
			}
		}

		#endregion

		#region Update AH_OSExTaxAmount

		public void UpdateAH_OSExTaxAmount()
		{
			if (!IsHeaderAmountsUpdateSuspended)
			{
				UpdateAH_OSExTaxAmount(0m);
				pendingUpdateAH_OSExTaxAmount = false;
			}
			else
			{
				pendingUpdateAH_OSExTaxAmount = true;
			}
		}

		bool pendingUpdateAH_OSExTaxAmount;

		public virtual ZBool IsTaxed
		{
			get { return false; }
		}

		public void UpdateAH_OSExTaxAmount(ZDecimal defaultNewAmount)
		{
			AH_OSExTaxAmount = GetSumOfLines(IsLocalCurrencyTransaction ?
				DependentTransactionLine.Schema.AL_LocalExTaxAmount :
				DependentTransactionLine.Schema.AL_OSExTaxAmount) + defaultNewAmount;
			AH_OSExTaxAmountInfo.RefreshBinding();
		}

		#endregion

		#region Update AH_OSTaxAmount

		public void UpdateAH_OSTaxAmount()
		{
			if (!IsHeaderAmountsUpdateSuspended)
			{
				UpdateAH_OSTaxAmount(0m);
				pendingUpdateAH_OSTaxAmount = false;
			}
			else
			{
				pendingUpdateAH_OSTaxAmount = true;
			}
		}

		bool pendingUpdateAH_OSTaxAmount;

		public void UpdateAH_OSTaxAmount(ZDecimal defaultNewAmount)
		{
			UpdateAH_OSTaxAmountCore(defaultNewAmount);
		}

		protected virtual void UpdateAH_OSTaxAmountCore(ZDecimal defaultNewAmount)
		{
			AH_OSTaxAmount = GetSumOfLines(IsLocalCurrencyTransaction ?
				DependentTransactionLine.Schema.AL_LocalTaxAmount :
				DependentTransactionLine.Schema.AL_OSTaxAmount) + defaultNewAmount;
			AH_OSTaxAmountInfo.RefreshBinding();
		}

		#endregion

		#region Update AH_OSWHTAmount

		public void UpdateAH_OSWHTAmount()
		{
			if (!IsHeaderAmountsUpdateSuspended)
			{
				UpdateAH_OSWHTAmount(0m);
				pendingUpdateAH_OSWHTAmount = false;
			}
			else
			{
				pendingUpdateAH_OSWHTAmount = true;
			}
		}

		bool pendingUpdateAH_OSWHTAmount;

		public void UpdateAH_OSWHTAmount(ZDecimal defaultNewAmount)
		{
			AH_OSWHTAmount = GetSumOfLines(IsLocalCurrencyTransaction ?
				DependentTransactionLine.Schema.AL_LocalWHTAmount :
				DependentTransactionLine.Schema.AL_OSWHTAmount) + defaultNewAmount;
			AH_OSWHTAmountInfo.RefreshBinding();
		}

		#endregion

		#region Update AH_OSTotalAmount

		public void UpdateAH_OSTotalAmount(ZDecimal defaultNewAmount)
		{
			AH_OSTotalAmount = GetSumOfLines(IsLocalCurrencyTransaction ?
				DependentTransactionLine.Schema.AL_LocalTotalAmount :
				DependentTransactionLine.Schema.AL_OverseasTotal) + AH_OSTaxAmountOtherTaxes_ForDisplay + defaultNewAmount;
			AH_OSTotalAmountInfo.RefreshBinding();
		}

		public void UpdateAH_OSTotalAmount()
		{
			if (!IsHeaderAmountsUpdateSuspended)
			{
				UpdateAH_OSTotalAmount(0m);
				pendingUpdateAH_OSTotalAmount = false;
			}
			else
			{
				pendingUpdateAH_OSTotalAmount = true;
			}
		}

		bool pendingUpdateAH_OSTotalAmount;

		#endregion

		#region Extra Tax Amount

		protected bool IsTaxReadOnly
		{
			get { return !AH_OH.IsValid || Header == null || !Header.CompanyData.IsAPTaxApplicable; }
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_OSExtraTax
		{
			get { return AH_OSExtraTaxAmount * Multiplier; }
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_OSExtraTaxAmount
		{
			get
			{
				if (isUpdateAH_OSExtraTaxAmountNeeded)
				{
					isUpdateAH_OSExtraTaxAmountNeeded = false;
					UpdateAH_OSExtraTaxAmount();
				}
				return fAH_OSExtraTaxAmount;
			}
			set
			{
				isUpdateAH_OSExtraTaxAmountNeeded = false;
				fAH_OSExtraTaxAmount = value;
				AH_OSExtraTaxAmountInfo.RefreshBinding();
			}
		}
		ZDecimal fAH_OSExtraTaxAmount;

		public ZPropertyInfo AH_OSExtraTaxAmountInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.AH_OSExtraTaxAmount);
				return result;
			}
		}

		protected virtual bool AH_OSExtraTaxAmount_ReadOnly
		{
			get { return IsTaxReadOnly; }
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_OSEDUPrimaryAmount
		{
			get
			{
				return RoundAmountToCurrencyDecimals(AH_OSExtraTaxAmount * EDUPrimaryPart / (EDUPrimaryPart + EDUSecondaryPart));
			}
		}

		public ZPropertyInfo AH_OSEDUPrimaryAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_OSEDUPrimaryAmount); }
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public ZDecimal AH_OSEDUSecondaryAmount
		{
			get
			{
				return RoundAmountToCurrencyDecimals(AH_OSExtraTaxAmount * EDUSecondaryPart / (EDUPrimaryPart + EDUSecondaryPart));
			}
		}

		public ZPropertyInfo AH_OSEDUSecondaryAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_OSEDUSecondaryAmount); }
		}

		readonly ZDecimal EDUPrimaryPart = 2;
		readonly ZDecimal EDUSecondaryPart = 1;

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_LocalExtraTax
		{
			get { return AH_LocalExtraTaxAmount * Multiplier; }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_LocalExtraTaxAmount
		{
			get
			{
				if (isUpdateAH_LocalExtraTaxAmountNeeded)
				{
					isUpdateAH_LocalExtraTaxAmountNeeded = false;
					UpdateAH_LocalExtraTaxAmount();
				}
				return fAH_LocalExtraTaxAmount;
			}
			set
			{
				isUpdateAH_LocalExtraTaxAmountNeeded = false;
				fAH_LocalExtraTaxAmount = value;
				AH_LocalExtraTaxAmountInfo.RefreshBinding();
			}
		}
		ZDecimal fAH_LocalExtraTaxAmount;

		public ZPropertyInfo AH_LocalExtraTaxAmountInfo
		{
			get
			{
				ZPropertyInfo result = GetZPropertyInfo(Schema.AH_LocalExtraTaxAmount);
				return result;
			}
		}

		protected virtual bool AH_LocalExtraTaxAmount_ReadOnly
		{
			get { return IsTaxReadOnly; }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_LocalEDUPrimaryAmount
		{
			get { return RoundAmountToCurrencyDecimals(AH_LocalExtraTaxAmount * EDUPrimaryPart / (EDUPrimaryPart + EDUSecondaryPart)); }
		}

		public ZPropertyInfo AH_LocalEDUPrimaryAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_LocalEDUPrimaryAmount); }
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal AH_LocalEDUSecondaryAmount
		{
			get { return RoundAmountToCurrencyDecimals(AH_LocalExtraTaxAmount * EDUSecondaryPart / (EDUPrimaryPart + EDUSecondaryPart)); }
		}

		public ZPropertyInfo AH_LocalEDUSecondaryAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_LocalEDUSecondaryAmount); }
		}

		public void UpdateAH_LocalExtraTaxAmount()
		{
			if (!IsHeaderAmountsUpdateSuspended)
			{
				AH_LocalExtraTaxAmount = GetSumOfLines(DependentTransactionLine.Schema.AL_LocalExtraTaxAmount);
				AH_LocalExtraTaxAmountInfo.RefreshBinding();
				pendingUpdateAH_LocalExtraTaxAmount = false;

				if (IsLocalCurrencyTransaction)
				{
					UpdateAH_OSExtraTaxAmount();
				}
			}
			else
			{
				pendingUpdateAH_LocalExtraTaxAmount = true;
			}
		}

		bool pendingUpdateAH_LocalExtraTaxAmount;

		public virtual void UpdateAH_OSExtraTaxAmount()
		{
			if (!IsHeaderAmountsUpdateSuspended)
			{
				AH_OSExtraTaxAmount = GetSumOfLines(IsLocalCurrencyTransaction ?
					DependentTransactionLine.Schema.AL_LocalExtraTaxAmount :
					DependentTransactionLine.Schema.AL_OSExtraTaxAmount);
				AH_OSExtraTaxAmountInfo.RefreshBinding();
				pendingUpdateAH_OSExtraTaxAmount = false;
			}
			else
			{
				pendingUpdateAH_OSExtraTaxAmount = true;
			}
		}

		bool pendingUpdateAH_OSExtraTaxAmount;

		bool isUpdateAH_LocalExtraTaxAmountNeeded;
		bool isUpdateAH_OSExtraTaxAmountNeeded;

		#endregion

		#region AH_OSTaxAmount

		bool isNeedAH_OSTaxAmountLoadFromDB = true;
		public override ZDecimal AH_OSTaxAmount
		{
			get
			{
				if (isNeedAH_OSTaxAmountLoadFromDB && IsInDatabase)
				{
					vw_AccTransactionHeaderTax result = Factory.Load<vw_AccTransactionHeaderTax>(new ZQuery(vw_AccTransactionHeaderTaxSchema.PK, this.PK)).FirstOrDefault();
					if (result != null)
					{
						if (IsLocalCurrencyTransaction)
						{
							return result.AH_LocalTaxAmount * Multiplier;
						}
						else
						{
							if (Company.GC_IsReciprocal)
							{
								return result.AH_OSTaxAmountWithReciprocal * Multiplier;
							}
							else
							{
								return result.AH_OSTaxAmountWithNoReciprocal * Multiplier;
							}
						}
					}
				}
				return base.AH_OSTaxAmount;
			}
			set
			{
				base.AH_OSTaxAmount = value;
				isNeedAH_OSTaxAmountLoadFromDB = false;
			}
		}
		#endregion

		#region Tax Branch

		public override ZGuid AH_GB_TaxBranch
		{
			get => base.AH_GB_TaxBranch;
			set
			{
				base.AH_GB_TaxBranch = value;

				foreach (DependentTransactionLine line in Lines)
				{
					line.AL_GB_TaxBranch = value;
				}
			}
		}

		public ZString AH_Calc_TaxBranchName
		{
			get { return base.TaxBranch != null ? TaxBranch.GB_BranchName : ZString.Empty; }
		}

		protected bool AH_Calc_TaxBranchName_ReadOnly { get; } = true;

		public ZPropertyInfo AH_Calc_TaxBranchNameInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_TaxBranchName); }
		}

		protected virtual bool AH_GB_TaxBranch_ReadOnly => !IsOverrideTaxBranchSecurityAllowed;

		protected virtual bool IsOverrideTaxBranchSecurityAllowed
		{
			get
			{
				return AH_Ledger == LedgerTypes.AccountsReceivable ?
					Env.Security.NewReceivablesOverrideTaxBranchAllows.IsAllowedWithConstraint() :
					Env.Security.NewPayablesOverrideTaxBranchAllows.IsAllowedWithConstraint();
			}
		}

		#endregion

		#region Implementation

		protected override bool IsTaxReportableCore
		{
			get
			{
				foreach (TransactionLine line in Lines)
				{
					if (line.TaxRate != null && !line.TaxRate.IsNonReportable)
					{
						return true;
					}
				}

				return false;
			}
		}

		public bool IsMiscServTaxApplicable
		{
			get
			{
				if (Header != null && Header.MiscServ != null)
				{
					return (AH_Ledger == LedgerTypes.AccountsReceivable ?
						Header.CompanyData.IsARTaxApplicable :
						Header.CompanyData.IsAPTaxApplicable);
				}
				else
				{
					return false;
				}
			}
		}

		protected virtual void SetTransactionLinesExchangeRate(ZDecimal exchangeRate)
		{
			if (exchangeRate.IsValid)
			{
				using (Lines.SuspendListChanged())
				{
					foreach (DependentTransactionLine line in Lines)
					{
						line.AL_ExchangeRate = exchangeRate;
					}
				}
			}
		}

		protected void SetTransactionLinesCurrency(ZString invoiceCurrencyNK)
		{
			if (!invoiceCurrencyNK.IsEmpty)
			{
				using (Lines.SuspendListChanged())
				{
					foreach (TransactionLine line in Lines)
					{
						line.AL_RX_NKTransactionCurrency = invoiceCurrencyNK;
					}
				}
			}
		}

		internal protected ZDecimal GetSumOfLines(ZString fieldName)
		{
			ZDecimal sumOfLines = 0m;
			foreach (DependentTransactionLine line in Lines)
			{
				sumOfLines += (ZDecimal)line[fieldName];
			}
#if DEBUG
			GetSumOfLinesCallAmount_ForTestOnly++;
#endif
			return sumOfLines;
		}

#if DEBUG
		public int GetSumOfLinesCallAmount_ForTestOnly
		{
			get;
			set;
		}
#endif

		internal protected ZDecimal GetSumOfLines(ZString fieldName1, ZString fieldName2)
		{
			ZDecimal sumOfLines = 0m;
			foreach (DependentTransactionLine line in Lines)
			{
				sumOfLines += (ZDecimal)line[fieldName1] + (ZDecimal)line[fieldName2];
			}
			return sumOfLines;
		}

		protected virtual void CreateAndLoadLines()
		{
			fLines = GetDependentLinesCollection();
			RegisterEditableChildObject(fLines);
			fLines.Load(LineCollectionLoadQuery);

			if (IsInDatabase && IsTransactionInDatabaseReadOnly)
			{
				fLines.SetReadOnlyIncludingChildren(true);
				if (IsInMatchingContext)
				{
					if (IsValidationSuspended)
					{
						ResumeValidation();
					}
				}
				else
				{
					SuspendValidationOnChild(fLines);
				}
			}
		}

		protected virtual ZQuery LineCollectionLoadQuery
		{
			get
			{
				ZQuery orderByQuery = new ZQuery();
				orderByQuery.AddToFilter(AccTransactionLinesSchema.AL_GC, AH_GC);
				orderByQuery.OrderBy = AccTransactionLinesSchema.AL_Sequence.Name;
				return orderByQuery;
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			fLines?.Cast<TransactionLine>().ForEach(line => line.CalculateRevenueRecognitionTypeIfEmpty());
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();
			if (!IsInDatabase)
			{
				var prevIgnoreValidationSuspended = IgnoreValidationSuspended;
				using (new DisposableAction(() => IgnoreValidationSuspended = false, () => IgnoreValidationSuspended = prevIgnoreValidationSuspended))
				{
					try
					{
						Lines.SuspendValidation();
						foreach (DependentTransactionLine line in Lines)
						{
							line.AL_PostDate = AH_PostDate;
						}
					}
					finally
					{
						Lines.ResumeValidation();
					}
				}
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			isUpdateAH_LocalExtraTaxAmountNeeded = isUpdateAH_OSExtraTaxAmountNeeded = true;
			isNeedAH_OSTaxAmountLoadFromDB = true;
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded && IsTransactionInDatabaseReadOnly)
			{
				if (LinesHaveBeenLoaded)
				{
					Lines.SetReadOnlyIncludingChildren(true);
				}

				RefreshBindingIncludingChildren();
			}
		}

		public override void DeleteFromDB()
		{
			Lines.RemoveAndDeleteAll();
			base.DeleteFromDB();
		}

		public bool IsLocalCurrencyTransaction
		{
			get { return AH_RX_NKTransactionCurrency == AH_Calc_LocalRXCode; }
		}

		#endregion

		#region ITransactionHeaderWithLines

		ITransactionLine[] ITransactionHeaderWithLines.Lines => Lines.Cast<ITransactionLine>().ToArray();

		#endregion

		#region IAuditParent

		IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren
		{
			get
			{
				yield return new AuditChildInfo(AccTransactionLinesSchema.AL_AH, null);
			}
		}

		#endregion

		internal protected virtual BranchLevelPostingConfigurationRegistryItem EnforceBranchLevelPostingRegistryItem
		{
			get { return null; }
		}

		public bool LinesHaveSameBranch
		{
			get
			{
				if (Lines.Any())
				{
					ZGuid branchPK = Lines[0].AL_GB;
					if (Lines.Cast<DependentTransactionLine>().Any(x => x.AL_GB != branchPK))
					{
						return false;
					}

					return true;
				}

				return false;
			}
		}

		public virtual bool NeedPlaceOfSupplyAtLineLevel => false;

		public abstract ZBool CanApplyTaxBranch { get; }

		#region DEBUG Methods
#if DEBUG

		public TransactionLine FindTransactionLine(string transactionLineType, AccChargeCode chargeCode, ZGuid jobHeaderPK)
		{
			foreach (TransactionLine line in Lines)
			{
				if (line.AL_LineType == transactionLineType && line.AL_AC == chargeCode.PK && line.AL_JH == jobHeaderPK)
				{
					return line;
				}
			}
			return null;
		}

#endif
#endregion
	}
}
