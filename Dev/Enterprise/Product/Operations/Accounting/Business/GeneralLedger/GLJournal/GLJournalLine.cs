using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournal;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals.FetchStrategies;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public partial class GLJournalLine : DependentTransactionLine, IDebitCreditAmounts, ISupportMultiSubAccounts
	{
		public GLJournalLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			OSAmount = new DebitCreditDataEntry(() => AL_OSExTaxAmount, x => AL_OSExTaxAmount = x);
			LocalAmount = new DebitCreditDataEntry(() => AL_LocalExTaxAmount, x => AL_LocalExTaxAmount = x);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AL_AG), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AL_GB), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AL_GE), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AL_Desc), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AL_LineAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AL_OH), ConcurrencyPolicy.Strict);
		}

		#region Schema

		public new abstract class Schema : DependentTransactionLine.Schema
		{
			public const string UnsignedOSLineAmount = "UnsignedOSLineAmount";
			public const string UnsignedLocalLineAmount = "UnsignedLocalLineAmount";
			public const string Units = "Units";
			public const string UnitQuantity = "UnitQuantity";
		}

		#endregion

		public GLJournal JournalHeader
		{
			get { return (GLJournal)MasterTransactionHeader; }
		}

		protected override bool InvertSigns
		{
			get { return false; }
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			using (SuspendSettingHasChanges())
			{
				OSAmount.OnLoaded();
				LocalAmount.OnLoaded();
			}
			if (HasAssignedExportBatchNumber)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				ResetJournalCachedRequests();
			}

			if (JournalHeader != null)
			{
				JournalHeader.OnGLJournalLineSaved(saveSucceeded);
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			ResetJournalCachedRequests();
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new GLJournalLineFetchStrategy(this);
		}

		public CodeDescriptionPairList DebitCreditSign_List
		{
			get { return LocalAmount.List; }
		}

		protected override ZString LineType
		{
			get { return ""; }
		}

		public override AccTransactionHeader TransactionHeader => JournalHeader ?? base.TransactionHeader;

		protected override AccTransactionLinesLookups GetNewLookups()
		{
			return new GLJournalLineLookups(this);
		}

		public virtual void CopyValuesFrom(GLJournalLine line)
		{
			AL_RX_NKTransactionCurrency = line.AL_RX_NKTransactionCurrency;
			UnsignedOSLineAmount = line.UnsignedOSLineAmount;
			DebitCreditSign = line.DebitCreditSign;
			AL_LineType = line.AL_LineType;
			AL_GB = line.AL_GB;
			AL_GE = line.AL_GE;
			AL_AG = line.AL_AG;
			AL_Desc = line.AL_Desc;
			AL_PostDate = line.AL_PostDate;
			AL_ReverseDate = line.AL_ReverseDate;
			AL_ExchangeRate = line.AL_ExchangeRate;
			SubAccountHelper.CopySubAccounts(this, line);
			CopyGLAttributesValue(line);
		}

		void CopyGLAttributesValue(GLJournalLine line)
		{
			var transactionLineDissectionAttributes = line.AccTransactionLineDissectionAttributes;
			foreach (var dissectionAttribute in AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>())
			{
				var sourceDissectionAttribute = transactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>().FirstOrDefault(x => x.ALD_Attribute == dissectionAttribute.ALD_Attribute);
				if (sourceDissectionAttribute != null)
				{
					dissectionAttribute.ALD_AttributeValue = sourceDissectionAttribute.ALD_AttributeValue;
					if (dissectionAttribute.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG)
					{
						dissectionAttribute.ALD_AttributeValueID = sourceDissectionAttribute.ALD_AttributeValueID;
					}
				}
			}
		}

		#region Validation - using New Validation

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			if (JournalHeader != null && (JournalHeader.IsPeriodExists || JournalHeader.IsLinkedWithDSBJobCloseBatch_Cached))
			{
				return GetEmptyValidation();
			}

			return new GLJournalLineValidation(this);
		}

		protected GLJournalLineValidation GLLineValidation
		{
			get { return Validation as GLJournalLineValidation; }
		}

		#endregion

		#region Overriden Properties

		[List("GLHeaderCollection")]
		public override ZGuid AL_AG
		{
			get { return base.AL_AG; }
			set
			{
				var original = base.AL_AG;
				base.AL_AG = value;
				fGLLocalCNAccountDescription = null;
				fGLLocalCNAccountCode = null;

				RunAmountAuthorisationValidation();

				if (original != base.AL_AG && JournalHeader != null && GLHeader != null && AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					AL_ExchangeRate = AccountingUtils.GetGLJournalExchangeRate(Factory, GLHeader.AG_AccountType, AL_RX_NKTransactionCurrency, JournalHeader.PostPeriod, JournalHeader.AH_PostDate);
				}
				PopulateTransactionLineDissectionAttribute();
			}
		}

		#region reporting book

		void PopulateTransactionLineDissectionAttribute()
		{
			var multiPeriodApportionmentFromHeader = Factory.Load<AccTransactionHeader>(TransactionHeader?.AH_TransactionBelongsToGroup ?? ZGuid.Empty);
			if (multiPeriodApportionmentFromHeader != null)
			{
				foreach (var dissectionAttribute in AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>())
				{
					var addr = multiPeriodApportionmentFromHeader.Header?.Addresses.Cast<OrgAddress>().FirstOrDefault(x => x.AddressCapability.GetCapabilityEnabled(OrgAddressType.Office.Code) && x.AddressCapability.GetIsMainAddress(OrgAddressType.Office.Code));
					switch (dissectionAttribute.ALD_Attribute)
					{
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG:

							dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.Cast<CodeDescriptionWithGroup>().FirstOrDefault(x => x.Code == (multiPeriodApportionmentFromHeader.Header?.CompanyData.OB_ARConsolidatedAccountingCategory ?? ZString.Empty))?.Group.ToString() ?? AccountingMasterFilesConstants.NAV.Code;
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO:
							if (addr == null)
							{
								dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.NAV.Code;
							}
							else if (addr.OA_RN_NKCountryCode == GlbCompany.CurrentCompany.Country.Code)
							{
								dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.LFOCodes.LOC;
							}
							else
							{
								dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.LFOCodes.FOR;
							}
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE:
							if (addr == null)
							{
								dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.NAV.Code;
							}
							else if (addr.OA_RN_NKCountryCode == GlbCompany.CurrentCompany.Country.Code)
							{
								dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.LFECodes.LOC;
							}
							else if (Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, addr.OA_RN_NKCountryCode).RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion)
							{
								dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.LFECodes.WEU;
							}
							else
							{
								dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.LFECodes.OEU;
							}
							break;
						case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR:

							if (multiPeriodApportionmentFromHeader.AH_TransactionType == TransactionTypes.CreditNote)
							{
								dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.SPRCodes.SPR;
							}
							else if (multiPeriodApportionmentFromHeader.AH_TransactionType == TransactionTypes.Invoice)
							{
								dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.SPRCodes.SPS;
							}
							else
							{
								dissectionAttribute.ALD_AttributeValue = AccountingMasterFilesConstants.NAV.Code;
							}
							break;
					}
				}
			}
		}

		#endregion

		protected override bool AL_AG_ReadOnly
		{
			get { return false; }
		}

		protected bool AL_OH_ReadOnly
		{
			get
			{
				return IsNoteJournal || (JournalHeader == null || !JournalHeader.IsEliminationJournal);
			}
		}

		protected bool AL_RX_NKTransactionCurrency_ReadOnly => IsNoteJournal;

		protected override bool AL_ExchangeRate_ReadOnly => IsNoteJournal || base.AL_ExchangeRate_ReadOnly;

		public override bool AL_Calc_FirstSubClassParentId_ReadOnly => IsNoteJournal || base.AL_Calc_FirstSubClassParentId_ReadOnly;

		public override bool AL_Calc_SecondSubClassParentId_ReadOnly => IsNoteJournal || base.AL_Calc_SecondSubClassParentId_ReadOnly;

		public bool IsNoteJournal => JournalHeader?.IsNoteJournal ?? false;

		[List("DepartmentCollection")]
		public override ZGuid AL_GE
		{
			get { return base.AL_GE; }
			set { base.AL_GE = value; }
		}

		public override ZString AL_RX_NKTransactionCurrency
		{
			get
			{
				return base.AL_RX_NKTransactionCurrency;
			}
			set
			{
				base.AL_RX_NKTransactionCurrency = value;

				if (AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					if (JournalHeader != null && GLHeader != null)
					{
						AL_ExchangeRate = AccountingUtils.GetGLJournalExchangeRate(Factory, GLHeader.AG_AccountType, AL_RX_NKTransactionCurrency, JournalHeader.PostPeriod, JournalHeader.AH_PostDate);
					}
				}
				else
				{
					AL_ExchangeRate = 1M;
				}

				if (GLLineValidation != null)
				{
					GLLineValidation.ValidateAL_AG();
					GLLineValidation.ValidateAL_ExchangeRate();
				}
			}
		}

		#region LineCurrencyDecimals

		public ZInt LineCurrencyDecimals
		{
			get
			{
				return IsNoteJournal ? UnitQuantityDecimals : (TransactionCurrency != null ? TransactionCurrency.Decimals : 2);
			}
		}

		public ZPropertyInfo LineCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo(nameof(LineCurrencyDecimals)); }
		}

		#endregion

		void RunAmountAuthorisationValidation()
		{
			if (JournalHeader != null)
			{
				var glJournalValidation = JournalHeader.Validation as GLJournalValidation;
				if (glJournalValidation != null)
				{
					glJournalValidation.ValidateAH_OSExTaxAmount();
				}
			}

			if (GLLineValidation != null)
			{
				GLLineValidation.ValidateUnsignedLocalLineAmount();
			}
		}

		#endregion

		#region New Properties

		#region Description Generation

		public bool IsMasterJournalLine
		{
			get; set;
		}

		public PeriodApportionmentJournalDescBuilder PeriodApportionmentJournalDescBuilder
		{
			get
			{
				if (fPeriodApportionmentJournalDescBuilder == null)
				{
					fPeriodApportionmentJournalDescBuilder = new PeriodApportionmentJournalDescBuilder(IsMasterJournalLine);
				}
				return fPeriodApportionmentJournalDescBuilder;
			}
		}
		PeriodApportionmentJournalDescBuilder fPeriodApportionmentJournalDescBuilder;

		public void UpdatePeriodApportionmentJournalLineDescription()
		{
			base.AL_Desc = PeriodApportionmentJournalDescBuilder.BuildLineDescription();
		}

		#endregion

		#region Debit/Credit

		[MaxLength(2)]
		[List("DebitCreditSign_List")]
		public ZString DebitCreditSign
		{
			get { return LocalAmount.DebitCreditSign; }
			set
			{
				if (LocalAmount.DebitCreditSign != value)
				{
					CheckMaximumLength(DebitCreditSignInfo, value);
					LocalAmount.DebitCreditSign = value;
					OSAmount.DebitCreditSign = LocalAmount.DebitCreditSign;
					if (GLLineValidation != null)
					{
						GLLineValidation.ValidateDebitCreditSign();
					}
				}
				DebitCreditSignInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DebitCreditSignInfo
		{
			get { return GetZPropertyInfo(nameof(DebitCreditSign)); }
		}

		#endregion

		#region UnsignedLineAmount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal UnsignedOSLineAmount
		{
			get { return OSAmount.UnsignedAmount; }
			set
			{
				OSAmount.UnsignedAmount = value;
				if (GLLineValidation != null)
				{
					GLLineValidation.ValidateUnsignedOSLineAmount();
					GLLineValidation.ValidateUnsignedLocalLineAmount();
				}
				UnsignedOSLineAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnsignedOSLineAmountInfo
		{
			get { return GetZPropertyInfo(nameof(UnsignedOSLineAmount)); }
		}

		[ResourceStringData("GLJournalLine|UnsignedLocalLineAmount", Caption = "Local Amount")]
		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal UnsignedLocalLineAmount
		{
			get { return LocalAmount.UnsignedAmount; }
			set
			{
				LocalAmount.UnsignedAmount = value;
				if (GLLineValidation != null)
				{
					GLLineValidation.ValidateUnsignedLocalLineAmount();
					GLLineValidation.ValidateUnsignedOSLineAmount();
				}
				UnsignedLocalLineAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo UnsignedLocalLineAmountInfo
		{
			get { return GetZPropertyInfo(nameof(UnsignedLocalLineAmount)); }
		}

		#endregion

		#region Unit Quantity

		[ResourceStringData("GLJournalLine|UnitQuantity", Caption = "Unit Quantity")]
		[DecimalPlaces(nameof(UnitQuantityDecimals))]
		public ZDecimal UnitQuantity
		{
			get { return UnsignedLocalLineAmount; }
			set
			{
				using (var runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
				{
					UnsignedOSLineAmount = value;
					UnsignedLocalLineAmount = value;
				}))
				{
					runMethodSuspender.RunMethod();
					if (GLLineValidation != null)
					{
						GLLineValidation.ValidateUnitQuantity();
					}
					UnitQuantityInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo UnitQuantityInfo => GetZPropertyInfo(nameof(UnitQuantity));

		public int UnitQuantityDecimals => AccountingConstants.CurrencyDefaultValues.NoteJournalCurrencyDecimal;

		#endregion

		#region Units

		[ResourceStringData("GLJournalLine|Units", Caption = "Unit")]
		public ZString Units => GLHeader != null && GLHeader.AG_AccountType == AccountType.Note ? GLHeader.AG_StatisticalUnits : ZString.Empty;

		public ZPropertyInfo UnitsInfo => GetZPropertyInfo(nameof(Units));

		#endregion

		#region GLAccountDescription

		public ZString GLAccountDescription
		{
			get { return GLHeader != null ? GLHeader.AG_DescriptionMultilingual : ZString.Empty; }
		}

		public ZPropertyInfo GLAccountDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(GLAccountDescription)); }
		}

		#endregion

		#region GLLocalCNAccountDescription

		public ZString GLLocalCNAccountDescription
		{
			get
			{
				if (fGLLocalCNAccountDescription == null)
				{
					if (GLHeader != null)
					{
						ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
						query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
						ZDBOnlySubQuery accGLDescriptorPivotSubQuery = new ZDBOnlySubQuery(typeof(AutoAccGLDescriptorPivot), AccGLDescriptorPivotSchema.YJ_AJ);

						accGLDescriptorPivotSubQuery.AddToFilter(AccGLDescriptorPivotSchema.YJ_AG, GLHeader.PK);
						query.AddSubQuery(AccGLAccountDescriptorSchema.PK, accGLDescriptorPivotSubQuery, JoinCondition.And);
						query.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, Constants.Languages.ChineseSimplified);
						AccGLAccountDescriptor accGLAccountDescriptor = Factory.LoadTop1<AccGLAccountDescriptor>(query);
						fGLLocalCNAccountDescription = (accGLAccountDescriptor != null) ? accGLAccountDescriptor.AJ_AccountDescription : ZString.Empty;
					}
				}
				return (fGLLocalCNAccountDescription != null ? (ZString)fGLLocalCNAccountDescription : ZString.Empty);
			}
		}
		string fGLLocalCNAccountDescription;

		public ZPropertyInfo GLLocalCNAccountDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(GLLocalCNAccountDescription)); }
		}

		#endregion

		#region GLLocalCNAccountCode

		public ZString GLLocalCNAccountCode
		{
			get
			{
				if (fGLLocalCNAccountCode == null)
				{
					if (GLHeader != null)
					{
						ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
						query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
						ZDBOnlySubQuery accGLDescriptorPivotSubQuery = new ZDBOnlySubQuery(typeof(AutoAccGLDescriptorPivot), AccGLDescriptorPivotSchema.YJ_AJ);

						accGLDescriptorPivotSubQuery.AddToFilter(AccGLDescriptorPivotSchema.YJ_AG, GLHeader.PK);
						query.AddSubQuery(AccGLAccountDescriptorSchema.PK, accGLDescriptorPivotSubQuery, JoinCondition.And);
						query.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, Constants.Languages.ChineseSimplified);
						AccGLAccountDescriptor accGLAccountDescriptor = Factory.LoadTop1<AccGLAccountDescriptor>(query);
						fGLLocalCNAccountCode = (accGLAccountDescriptor != null) ? accGLAccountDescriptor.AJ_LocalAccountNumber : ZString.Empty;
					}
				}
				return (fGLLocalCNAccountCode != null ? (ZString)fGLLocalCNAccountCode : ZString.Empty);
			}
		}
		string fGLLocalCNAccountCode;

		public ZPropertyInfo GLLocalCNAccountCodeInfo
		{
			get { return GetZPropertyInfo(nameof(GLLocalCNAccountCode)); }
		}

		#endregion

		public bool HasAssignedExportBatchNumber
		{
			get { return IsInDatabase && Factory.LoadTop1<GenExportBatchSequence>(new ZQuery(GenExportBatchSequenceSchema.XB_ParentID, PK)) != null; }
		}

		internal GLJournal.AuthorisationRequiredType AuthorizationRequiredType
		{
			get
			{
				var result = GLJournal.AuthorisationRequiredType.NoAuthorisationRequired;
				if (JournalHeader != null)
				{
					result = JournalHeader.GetLineAuthorisationRequiredType(this);
				}

				return result;
			}
		}

		public ZInt AL_PostDatePeriod
		{
			get
			{
				if (JournalHeader != null && JournalHeader.AH_PostDate.Date == AL_PostDate.Date)
				{
					return JournalHeader.AH_PostDatePeriod;
				}

				var periodCalculator = JournalHeader?.PeriodCalculator ?? GetLinePeriodCaclulator();
				return periodCalculator.GetPeriodFromDate(AL_PostDate);
			}
		}

		public int AL_ReverseDatePeriod
		{
			get
			{
				if (JournalHeader != null && JournalHeader.AH_DueDate.Date == AL_ReverseDate.Date)
				{
					return JournalHeader.AH_DueDatePeriod;
				}

				var periodCalculator = JournalHeader?.PeriodCalculator ?? GetLinePeriodCaclulator();
				return periodCalculator.GetPeriodFromDate(AL_ReverseDate);
			}
		}

		AccountingPeriodCalculator GetLinePeriodCaclulator()
		{
			return new AccountingPeriodCalculator(Factory, Company);
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			if (IsInDatabase)
			{
				JournalHeader.GLJournalLineGLDDeleter.AddDeletedGLJournalLinePK(PK);
			}

			base.Delete();
		}

		#endregion

		#region Saving

		public override void OnSavingCore()
		{
			base.OnSavingCore();

			if (JournalHeader != null)
			{
				var prevIgnoreValidationSuspended = IgnoreValidationSuspended;
				using (new DisposableAction(() => IgnoreValidationSuspended = false, () => IgnoreValidationSuspended = prevIgnoreValidationSuspended))
				using (GetValidationSuspender())
				{
					AL_PostDate = JournalHeader.AH_PostDate;
					AL_LineType = JournalHeader.AH_TransactionType;
					AL_ReverseDate = JournalHeader.AH_DueDate;
				}
				JournalHeader.OnGLJournalLineSaving();
			}
		}

		#endregion

		#region Implementation

		protected static readonly ZString CR = DebitCreditDataEntry.CR;
		protected static readonly ZString DR = DebitCreditDataEntry.DR;

		protected readonly DebitCreditDataEntry OSAmount;
		protected readonly DebitCreditDataEntry LocalAmount;

		void ResetJournalCachedRequests()
		{
			if (JournalHeader != null)
			{
				JournalHeader.ResetAllCachedRequests();
			}
		}

		public void ResetLineDefaultValuesForNoteJournal()
		{
			AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AL_OH = ZGuid.Empty;
		}

		public override int CurrencyDecimals => IsNoteJournal ? UnitQuantityDecimals : base.CurrencyDecimals;

		public override int LocalDecimals => IsNoteJournal ? UnitQuantityDecimals : base.LocalDecimals;

		protected override ZDecimal RoundAmountToLocalDecimals(ZDecimal value)
		{
			return IsNoteJournal ? value : base.RoundAmountToLocalDecimals(value);
		}

		protected override ZDecimal RoundAmountToLocalDecimals(ZDecimal amount, ZDecimal exchangeRate)
		{
			return IsNoteJournal ? amount : base.RoundAmountToLocalDecimals(amount, exchangeRate);
		}

		protected override ZDecimal RoundAmountToCurrencyDecimals(ZDecimal value)
		{
			return IsNoteJournal ? value : base.RoundAmountToCurrencyDecimals(value);
		}

		protected override ZDecimal RoundAmountToCurrencyDecimals(ZDecimal amount, ZDecimal exchangeRate)
		{
			return IsNoteJournal ? amount : base.RoundAmountToCurrencyDecimals(amount, exchangeRate);
		}

		#endregion

		#region IDebitCreditAmounts Members

		ZDecimal IDebitCreditAmounts.OSUnsignedLineAmount
		{
			get { return UnsignedOSLineAmount; }
			set { UnsignedOSLineAmount = value; }
		}

		ZDecimal IDebitCreditAmounts.LocalUnsignedLineAmount
		{
			get { return UnsignedLocalLineAmount; }
			set { UnsignedLocalLineAmount = value; }
		}

		#endregion

		#region ISupportMultiSubAccounts

		protected override bool IsMultiSubAccountsSupportedCore => true;

		ISupportSubAccountCollection ISupportMultiSubAccounts.SubAccounts => SubAccounts;

		#endregion
	}
}
