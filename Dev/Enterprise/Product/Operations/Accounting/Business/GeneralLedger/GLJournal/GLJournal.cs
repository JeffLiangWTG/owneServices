using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Database.Shared;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.Accounting.Business.GeneralLedger.GLJournal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	[SystemDefinedValues]
	[GlowDataDefinition("IGLJournalHeader")]
	public partial class GLJournal : TransactionHeaderWithLines, ITemplateCopyable, IDocumentSupportable, IGeneralLedger, IDocManagerSupport, IDisposable, IAutoCurrencyAdjustmentGLJournal, IEDocsParsingSupport
	{
		public new abstract class Schema : TransactionHeaderWithLines.Schema
		{
			public const string ApprovalRequestStatus = "ApprovalRequestStatus";
			public const string PeriodPK = "PeriodPK";
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are reviewed. All possible values are expected and handled.")]
		public GLJournal(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(AH_InvoiceDate), ConcurrencyPolicy.Ignore);
			InitEDocRelatedFactories();
		}

		#region Overridden Members

		protected override AccTransactionHeaderLookups GetNewLookups()
		{
			return new GLJournalLookups(this);
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (IsLinkedToConsolidationBatch)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AH_InvoiceDate = Env.Time.CurrentLocalDateTime;
			AH_TransactionType = TransactionTypes.GLStandardJournal;
			AH_GB = GlbBranch.CurrentBranch.PK;
			AH_GE = GlbDepartment.CurrentDepartment.PK;

			if (IsPostDateEnabled)
			{
				AH_PostDate = PeriodCalculator.GetLastDayForPeriod(AH_PostDate);
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (Lines.Count > AccountingUtils.ObjectCountThresholdToDisableDataRefreshBus)
			{
				Factory.RefreshEnabled = false;
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				RefreshBindingIncludingChildren();
				ResetAllCachedRequests();
			}

			HandleCreatedEDocOnSaved(saveSucceeded);
		}

		protected override void OnSavingCore()
		{
			base.OnSavingCore();

			CreateApprovedJournalApprovalRequest();

			var prevIgnoreValidationSuspended = IgnoreValidationSuspended;
			using (new DisposableAction(() => IgnoreValidationSuspended = false, () => IgnoreValidationSuspended = prevIgnoreValidationSuspended))
			using (this.GetValidationSuspender())
			{
				var shouldSetTransactionNumber = !IsInDatabase;
				if (shouldSetTransactionNumber
#if DEBUG
					&& (!Globals.IsTest || !IsManuallySetTransactionNumber_ForTestOnly)
#endif
					)
				{
					using (Branch.SetAsTemporaryContext())
					{
						AH_TransactionNum = AccountingNumberFountainWrapperFactory.Instance.GLJournal.Generate(this);
					}
				}

				Factory.ClearQueryCache(GLJournalApprovalRequest.Schema.TableName);
				var request = this.GetLatestLinkedApprovalRequestInDB();
				if (request != null)
				{
					if (request.IsInDatabase)
					{
						request.Reload(); //to get latest version of a request as it saved in first factory for SaveTogether.
					}

					if (shouldSetTransactionNumber)
					{
						request.SetJournalTransactionNumber(this);

						if (OriginalTransaction != null && IsJournalCanBeReversed)
						{
							OriginalTransaction.IsCancelled = IsCancelled = true;
							OriginalTransaction.AH_TransactionBelongsToGroup = AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
						}
					}

					ResetAllCachedRequests();
				}
				AH_InvoiceDate = ZDateTime.Now;
			}

			CreateEDocOnSaving();
		}

		void CreateApprovedJournalApprovalRequest()
		{
			if (!IsInDatabase && IsPeriodExists)
			{
				var request = Factory.New<GLJournalApprovalRequest>();
				request.Initialize(this);
				request.XP_ParentID = PK;
				request.XP_ReasonDescription = AH_Desc;
				request.XP_GB_RequestingBranch = AH_GB;
				request.XP_GS_NKApprovingUser1 = GlbStaff.CurrentUser.GS_Code;
				request.XP_ApprovalDate = ZDateTime.Now;
				request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
			}
		}

		public ZString GetLockMessage(ZGlobalMutex mutex, ZString formAction)
		{
			var lockInfo = mutex.GetLockInfo();
			var who = lockInfo != null ? lockInfo.UserWithLock.GS_FullName.ToString() : Res.GetString("AAF27344-0C69-415F-A7B4-37E7BB53690F", "*unknown user*");
			var when = lockInfo != null ? lockInfo.LockStartTime.ToDateTime().ToLocalTime().ToLongTimeString() : Res.GetString("A6D7338E-4CEC-4EB8-A614-963FCEE135B3", "*unknown time*");

			string message = Res.GetString("GLJournalForm|UserActionHeader", "The GL Journal is currently being edited by user '{0}' since {1}, you cannot {2} it before the other user closes the form.{3}{3}", who, when, formAction, System.Environment.NewLine);
			return message;
		}

		protected override bool SupportsCloneCore() => true;

		protected override ZString HumanReadableNameCore => Res.GetString("ec22b03e-aba7-4004-9032-a845132641c8", "General Ledger Journal");

		protected override bool InvertSigns => false;

		protected override ZString Ledger => LedgerTypes.General;

		protected override ZString TransactionType => TransactionTypes.GLStandardJournal;

		public override Type DependentTransactionLineType => typeof(GLJournalLine);

		protected override DependentTransactionLineCollection GetDependentLinesCollection()
		{
			ZQuery orderByQuery = new ZQuery();
			orderByQuery.OrderBy = AccTransactionLinesSchema.Constants.AL_Sequence;
			GLJournalLineCollection journalLines = GetDependentLinesCollectionCore(orderByQuery);

			bool journalLinesReadOnlyState = GetPropertiesReadOnlyState(null);
			if (journalLinesReadOnlyState)
			{
				journalLines.SetReadOnlyIncludingChildren(true);
			}

			return journalLines;
		}

		protected virtual GLJournalLineCollection GetDependentLinesCollectionCore(ZQuery orderByQuery)
		{
			return new GLJournalLineCollection(this, orderByQuery);
		}

		protected override void GenerateReverseTransactionCore(bool mustTransform)
		{
			fIsReversing = true;

			GLJournal reversingJournal = (GLJournal)Factory.New(GetType());
			using (reversingJournal.GetValidationSuspender())
			{
				reversingJournal.CopyHeaderValuesFrom(this);
				reversingJournal.AH_PostDate = ZDateTime.Today;

				foreach (GLJournalLine line in Lines)
				{
					GLJournalLine reversingLine = (GLJournalLine)reversingJournal.Lines.AddNew();
					using (reversingLine.GetValidationSuspender())
					{
						reversingLine.CopyValuesFrom(line);
						reversingLine.DebitCreditSign = (line.DebitCreditSign == DebitCreditDataEntry.DR ? DebitCreditDataEntry.CR : DebitCreditDataEntry.DR);
						reversingLine.UnsignedLocalLineAmount = line.UnsignedLocalLineAmount;
					}
				}
			}

			reversingJournal.Lines.SetReadOnlyIncludingChildren(true);
			fReverseTransaction = reversingJournal;
			fReverseTransaction.IsReverseTransaction = true;
			fReverseTransaction.OriginalTransaction = this;
		}

		#region AH_TransactionType

		[List("TransactionType_List")]
		public override ZString AH_TransactionType
		{
			get { return base.AH_TransactionType; }
			set
			{
				var oldValue = base.AH_TransactionType;
				base.AH_TransactionType = value;
				ClearAgePeriod();
				SetDescription();
				GLJournalLines.SetLineTypeForAllLines();

				if (oldValue != base.AH_TransactionType)
				{
					if (IsNoteJournal)
					{
						GLJournalLines.Cast<GLJournalLine>().ForEach(x => x.ResetLineDefaultValuesForNoteJournal());
					}

					var lastDay = PeriodCalculator.GetLastDayForPeriod(AH_PostDate);
					if (lastDay.IsValid)
					{
						AH_PostDate = lastDay;
					}
				}
			}
		}

		protected void ClearAgePeriod()
		{
			if (IsStdJournal || IsNoteJournal)
			{
				AgePeriod = 0;
			}
		}

		protected void SetDescription()
		{
			AH_Desc = AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AH_Ledger + AH_TransactionType,
				AH_Ledger + " " + new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(AH_TransactionType));
		}

		#endregion

		public override ZInt AgePeriod
		{
			get { return base.AgePeriod; }
			set
			{
				base.AgePeriod = value;

				if (PeriodCalculator.IsPeriodValid(value))
				{
					if (IsRevJournal)
					{
						AH_DueDate = PeriodCalculator.GetFirstDayForPeriod(value);
					}
					else if (IsAutoJournal)
					{
						AH_DueDate = PeriodCalculator.GetLastDayForPeriod(value);
					}
				}
			}
		}

		public override ZDateTime AH_PostDate
		{
			get { return base.AH_PostDate; }
			set
			{
				var oldValue = base.AH_PostDate;
				bool lastDaySet = false;
				if (!IsPostDateEnabled)
				{
					var lastDay = PeriodCalculator.GetLastDayForPeriod(value);
					if (lastDaySet = lastDay.IsValid)
					{
						base.AH_PostDate = lastDay;
					}
				}
				if (!lastDaySet)
				{
					base.AH_PostDate = value;
				}

				GLJournalLines.SetPostDateForAllLines();

				if (AH_PostDate != oldValue)
				{
					foreach (GLJournalLine line in Lines)
					{
						if (line.AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							if (!line.IsInDatabase && line.GLHeader != null)
							{
								var previousDefaultValue = AccountingUtils.GetGLJournalExchangeRate(Factory, line.GLHeader.AG_AccountType, line.AL_RX_NKTransactionCurrency, PostPeriod, oldValue);
								if (line.AL_ExchangeRate == previousDefaultValue)
								{
									line.AL_ExchangeRate = AccountingUtils.GetGLJournalExchangeRate(Factory, line.GLHeader.AG_AccountType, line.AL_RX_NKTransactionCurrency, PostPeriod, AH_PostDate);
								}
							}
						}
						else
						{
							line.AL_ExchangeRate = 1M;
						}
					}
				}
			}
		}

		protected override bool AH_PostDate_ReadOnly => IsPostDateEnabled ? PostPeriodInfo.ReadOnly : base.AH_PostDate_ReadOnly;

		public override ZDateTime AH_DueDate
		{
			get { return base.AH_DueDate; }
			set
			{
				var day = ZDateTime.Invalid;
				if (IsRevJournal && !IsDueDateEnabled)
				{
					day = PeriodCalculator.GetFirstDayForPeriod(value);
				}
				else if (IsAutoJournal)
				{
					day = PeriodCalculator.GetLastDayForPeriod(value);
				}

				base.AH_DueDate = day.IsValid ? day : value;

				GLJournalLines.SetReverseDateForAllLines();
			}
		}

		protected override bool AH_DueDate_ReadOnly => IsDueDateEnabled ? AgePeriodInfo.ReadOnly : base.AH_DueDate_ReadOnly;

		public ZInt AH_PostDatePeriod
		{
			get
			{
				if (!ah_PostDatePeriod.HasValue || ah_PostDateForPeriod != AH_PostDate.Date)
				{
					ah_PostDateForPeriod = AH_PostDate.Date;
					ah_PostDatePeriod = PeriodCalculator.GetPeriodFromDate(ah_PostDateForPeriod);
				}

				return ah_PostDatePeriod.Value;
			}
		}
		ZInt? ah_PostDatePeriod;
		ZDate ah_PostDateForPeriod;

		public ZInt AH_DueDatePeriod
		{
			get
			{
				if (!ah_DueDatePeriod.HasValue || ah_DueDateForPeriod != AH_DueDate.Date)
				{
					ah_DueDateForPeriod = AH_DueDate.Date;
					ah_DueDatePeriod = PeriodCalculator.GetPeriodFromDate(ah_DueDateForPeriod);
				}

				return ah_DueDatePeriod.Value;
			}
		}
		ZInt? ah_DueDatePeriod;
		ZDate ah_DueDateForPeriod;

		public virtual bool AH_TransactionCategory_ReadOnly => HasNotPostedApprovalRequest ? !Env.Security.GLJournalApprovalModifyPresentationField.IsAllowed :
														!Env.Security.GeneralLedgerModifyPresentationField.IsAllowed;

		[List("TransactionCategory_List")]
		public override ZString AH_TransactionCategory
		{
			get { return base.AH_TransactionCategory; }
			set { base.AH_TransactionCategory = value; }
		}

		protected override AccountingNumberFountainWrapper NumberFountainForTransactionNumber => null;

		public override ZInt PostPeriod
		{
			get { return base.PostPeriod; }
			set
			{
				if (PostPeriod != value)
				{
					foreach (GLJournalLine line in Lines)
					{
						if (line.AL_RX_NKTransactionCurrency != GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
						{
							if (!line.IsInDatabase && line.GLHeader != null)
							{
								var accountType = line.GLHeader.AG_AccountType;
								var previousDefaultValue = AccountingUtils.GetGLJournalExchangeRate(Factory, accountType, line.AL_RX_NKTransactionCurrency, PostPeriod, AH_PostDate);
								if (line.AL_ExchangeRate == previousDefaultValue)
								{
									line.AL_ExchangeRate = AccountingUtils.GetGLJournalExchangeRate(Factory, accountType, line.AL_RX_NKTransactionCurrency, value, AH_PostDate);
								}
							}
						}
						else
						{
							line.AL_ExchangeRate = 1M;
						}
					}
				}

				base.PostPeriod = value;
			}
		}

		protected override bool AH_GB_TaxBranch_ReadOnly => false;

		public ZGuid PeriodPK
		{
			get { return this.GetSystemDefinedValue<ZGuid>(Schema.PeriodPK); }
			set { this.SetSystemDefinedValue(Schema.PeriodPK, AddOnColumnDataType.Codes.Guid, value); }
		}

		public bool IsPeriodExists
		{
			get
			{
				var periodPK = PeriodPK;
				return periodPK.IsValid && Factory.Exists(typeof(AccPeriodManagement), new ZQuery(AccPeriodManagementSchema.PK, periodPK));
			}
		}

		bool IAutoCurrencyAdjustmentGLJournal.IsAutoCurrencyAdjustmentGLJournal => IsPeriodExists;

		#region Validation

		protected override TransactionHeaderValidation GetNewReversalValidation()
		{
			return new GLJournalValidation(this);
		}

		protected override TransactionHeaderValidation GetNewValidationCore()
		{
			return new GLJournalValidation(this);
		}

		#endregion

		#endregion

		#region Load With Mutex

		public static LoadedJournalWithMutex LoadWithMutex(ZGuid journalPk, BusinessObjectFactory factory)
		{
			ZGlobalMutex mutex = null;
			var disposeMutex = true;
			try
			{
				mutex = new ZGlobalMutex(MutexIDs.GLJournalForm, GetMutexKey(journalPk));
				var result = new LoadedJournalWithMutex();
				if (mutex.Lock())
				{
					result.journal = factory.Load<GLJournal>(journalPk);
					if (result.journal != null)
					{
						result.journal.mutex = mutex;
					}
					else
					{
						return result;
					}
				}
				else
				{
					var lockInfo = mutex.GetLockInfo();
					var who = lockInfo != null ? lockInfo.UserWithLock.GS_FullName.ToString() : Res.GetString("e6045845-aa1b-4983-baa9-59a93f86345e", "*unknown user*");
					var when = lockInfo != null ? lockInfo.LockStartTime.ToDateTime().ToLocalTime().ToLongTimeString() : Res.GetString("a506b74b-5f8c-40cd-83bd-887357965491", "*unknown time*");

					result.errorMessage = Res.GetString("9425b8c1-548e-49ac-8cc1-0db588d7cd48", "The GL Journal is currently being changed by user '{0}' since {1}, you cannot change it before the other user closes the form.", who, when);
				}
				disposeMutex = false;
				return result;
			}
			finally
			{
				if (disposeMutex)
				{
					((IDisposable)mutex)?.Dispose();
					mutex = null;
				}
			}
		}

		public static ZString GetMutexKey(ZGuid pk)
		{
			return pk + "_" + GlbCompany.CurrentCompany.GC_Code;
		}

		ZGlobalMutex mutex;

		[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Used for parameter passing")]
		public struct LoadedJournalWithMutex
		{
			public GLJournal journal;
			public ZString errorMessage;
		}

		#endregion

		#region IDisposable Members

		bool disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		~GLJournal()
		{
			// Do not need to dispose the mutex if it is null.
			// DisposableActionForDbConnection() might spin up an entirely new DB connection; this is pointless if the mutex is null.
			// And extremely dangerous on the finalizer thread.
			// And, if the app is shutting down, it is not possible to create a new DB connection.
			if (mutex == null || System.Environment.HasShutdownStarted)
			{
				return;
			}

			using (CargoWise.Data.Db.DisposableActionForDbConnection())
			{
				Dispose(false);
			}
		}

		void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				//for managed Resource
			}
			if (!disposed)
			{
				try
				{
					((IDisposable)mutex)?.Dispose();
				}
				catch (Exception exc)
				{
					if (exc.IsCriticalException())
					{
						throw;
					}
				}
				disposed = true;
			}
		}

		#endregion

		#region Type Decider

		//public static new readonly TypeDecider TypeDecider = new TransactionHeaderTypeDecider();
		public static new readonly TypeDecider TypeDecider = new GLJournalTypeDecider();

		#endregion

		#region ReadOnly State

		protected override bool IsTransactionInDatabaseReadOnlyCore => false;

		protected override bool GetPropertiesReadOnlyState(PropertyDescriptor property)
		{
			if (IsReverseTransaction && !IsInDatabase)
			{
				if (property != null)
				{
					switch (property.Name)
					{
						case Schema.AH_NumberOfSupportingDocuments:
						case Schema.AH_Desc:
						case Schema.PostPeriod:
						case Schema.AH_PostDate:
							return false;
						case Schema.AgePeriod:
						case Schema.AH_DueDate:
							return !(IsRevJournal || IsAutoJournal);
					}
				}
				return true;
			}

			bool result = false;

			if (IsInDatabase)
			{
				if (IsReverseTransaction)
				{
					var request = GetLatestLinkedApprovalRequestInDB();
					if (request != null && request.XP_ApprovalStatus == Constants.GenApprovalRequestApprovalStatus.Posted)
					{
						return true;
					}
				}

				if (periodBasedReadonlyStateForSavedJournal == null)
				{
					IsPeriodClosedDelegate isPeriodClosed = AH_TransactionCategory.IsEmpty ? PeriodCalculator.IsPeriodGLClosed : PeriodCalculator.IsPeriodSubledgerClosedForAdjustments;
					switch (AH_TransactionType)
					{
						case TransactionTypes.GLStandardJournal:
						case TransactionTypes.GLNoteJournal:
							periodBasedReadonlyStateForSavedJournal = isPeriodClosed(PostPeriod);
							break;
						case TransactionTypes.GLReversingJournal:
							periodBasedReadonlyStateForSavedJournal = isPeriodClosed(PostPeriod) || isPeriodClosed(AgePeriod);
							break;
						case TransactionTypes.GLAutoJournal:
							{
								periodBasedReadonlyStateForSavedJournal = isPeriodClosed(PostPeriod) || isPeriodClosed(AgePeriod);
								if (!periodBasedReadonlyStateForSavedJournal.Value)
								{
									int tempPeriod = PeriodCalculator.GetNextPeriod(PostPeriod);
									while (tempPeriod != 0 && tempPeriod < AgePeriod)
									{
										periodBasedReadonlyStateForSavedJournal = isPeriodClosed(tempPeriod);
										if (periodBasedReadonlyStateForSavedJournal.Value)
										{
											break;
										}
										tempPeriod = PeriodCalculator.GetNextPeriod(tempPeriod);
									}
								}
							}
							break;
					}
				}
				if (periodBasedReadonlyStateForSavedJournal != null)
				{
					result = periodBasedReadonlyStateForSavedJournal.Value;
				}
			}
			if (!result && property != null)
			{
				result = MetaData.GetReadOnlyExcludingMethodProvider(this, property);
				result |= HasAssignedExportBatchNumberLines_CachedByTime;
				if (!result)
				{
					bool membersReadOnlyForChina = IsInDatabase && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Constants.CountryCodes.China;
					switch (property.Name)
					{
						case Schema.AH_OSExTaxAmount:
						case Schema.AH_TransactionNum:
							result = true;
							break;
						case Schema.AH_TransactionCategory:
						case Schema.AH_TransactionType:
							result = IsInDatabase;
							break;
						case Schema.PostPeriod:
						case Schema.AH_PostDate:
							result = membersReadOnlyForChina;
							break;
						case Schema.AgePeriod:
							result = membersReadOnlyForChina || IsStdJournal || IsNoteJournal;
							break;
						case Schema.AH_DueDate:
							result = membersReadOnlyForChina || !IsRevJournal;
							break;
					}
				}
			}
			return result;
		}

		bool? periodBasedReadonlyStateForSavedJournal;

		delegate ZBool IsPeriodClosedDelegate(ZInt period);

		bool HasAssignedExportBatchNumberLines
		{
			get
			{
				bool result = false;
				foreach (GLJournalLine line in Lines)
				{
					if (line.HasAssignedExportBatchNumber)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		bool HasAssignedExportBatchNumberLines_CachedByTime
		{
			get
			{
				if (last_HasAssignedExportBatchNumberLines_ReadTime.IsEmpty ||
					(ZDateTime.Now - last_HasAssignedExportBatchNumberLines_ReadTime).TotalSeconds > cacheIntervalInSec)
				{
					last_HasAssignedExportBatchNumberLines_Value = HasAssignedExportBatchNumberLines;
					last_HasAssignedExportBatchNumberLines_ReadTime = ZDateTime.Now;
				}

				return last_HasAssignedExportBatchNumberLines_Value;
			}
		}

		bool last_HasAssignedExportBatchNumberLines_Value;
		ZDateTime last_HasAssignedExportBatchNumberLines_ReadTime = ZDateTime.Empty;
		int cacheIntervalInSec = 30;

#if DEBUG
		[SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public void SetCacheIntervalForHasAssignedExportBatchNumberLines_ForTestOnly(int seconds)
		{
			cacheIntervalInSec = seconds;
		}
#endif

		#endregion

		#region Lists

		CodeDescriptionPairList fTransactionType_List;
		public CodeDescriptionPairList TransactionType_List
		{
			get
			{
				if (fTransactionType_List == null)
				{
					var provider = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(GlbCompany.CurrentCompany.GC_RN_NKCountryCode) as IInstanceProvider<IGLJournalTypesProvider>;
					fTransactionType_List = provider?.Get()?.GetGLJournalTypes() ?? new CodeDescriptionPairList(OLookUpEditType.GLJournalTypes);
				}
				return fTransactionType_List;
			}
		}

		ReadOnlyCodeDescriptionPairList fTransactionCategory_List;
		public ReadOnlyCodeDescriptionPairList TransactionCategory_List
		{
			get
			{
				if (fTransactionCategory_List == null)
				{
					CodeDescriptionPairList list = new CodeDescriptionPairList(AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.
							GetFallBackValueAtAllLevels(AH_GC.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList());

					if (IsInDatabase && !AH_TransactionCategory.IsEmpty && !list.ContainsCode(AH_TransactionCategory))
					{
						list.AddPair(AH_TransactionCategory, ResString.GetMultilingualString("6a4f913d-5acc-49d8-b756-0e5147ef9c68", "Obsolete Journal Category"));
					}

					fTransactionCategory_List = list;
				}
				return fTransactionCategory_List;
			}
		}

		#endregion

		[ChildEditable(true)]
		public virtual GLJournalLineCollection GLJournalLines => (GLJournalLineCollection)Lines;

		public GLJournalLineGLDDeleter GLJournalLineGLDDeleter => fGLJournalLineGLDDeleter ?? (fGLJournalLineGLDDeleter = new GLJournalLineGLDDeleter(AH_GC));
		GLJournalLineGLDDeleter fGLJournalLineGLDDeleter;

		public GLJournalGLDComplianceReportAction GLJournalGLDComplianceReportAction => fGLJournalGLDComplianceReportAction ?? (fGLJournalGLDComplianceReportAction = new GLJournalGLDComplianceReportAction(this));
		GLJournalGLDComplianceReportAction fGLJournalGLDComplianceReportAction;

		public GLJournalAllTransactionComplianceReportAction GLJournalAllTransactionComplianceReportAction => fGLJournalAllTransactionComplianceReportAction ?? (fGLJournalAllTransactionComplianceReportAction = new GLJournalAllTransactionComplianceReportAction(this));
		GLJournalAllTransactionComplianceReportAction fGLJournalAllTransactionComplianceReportAction;

		public ZInt CurrencySubUnitRatio
		{
			get
			{
				RefCurrency currentCurrency = RefCurrency.LoadFromCurrencyCode(Factory, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				return (ZInt)Math.Log10(currentCurrency.RX_SubUnitRatio);
			}
		}

		#region Balancing

		public bool IsBalanced => GetSumOfLines(GLJournalLine.Schema.AL_LocalExTaxAmount) == 0m;

		public GLJournalLine Balance()
		{
			GLJournalLine balancingLine = GLJournalLines.AddNew();
			// This balances the journal automatically, i.e. no need to explicitly detail the balancing amounts
			balancingLine.AL_AG = (Guid)BalancingAccount.Value;

			return balancingLine;
		}

		public virtual IRegistryItem BalancingAccount => AccountingConfigurationRegistry.Instance.GLJournalClearingAccount;

		#endregion

		public bool IsEliminationJournal
		{
			get
			{
				var glPresentationJournalCaterogyList = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
				return glPresentationJournalCaterogyList.EliminationCategory?.Code == AH_TransactionCategory;
			}
		}

		bool IsLinkedToConsolidationBatch => IsInDatabase && Factory.LoadTop1<AccConsolidationBatch>(new ZQuery(AccConsolidationBatchSchema.YB_AH_EliminationJournal, PK)) != null;

		public bool DoesHeaderAndLinesHavePersistentChanges
		{
			get
			{
				var result = !IsInDatabase;
				if (!result)
				{
					result = ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(info => info.IsPersistent && info.HasChanges);
				}
				if (!result)
				{
					result = GLJournalLines.Cast<GLJournalLine>().Any(line => !line.IsInDatabase || line.ZPropertyInfoHash.Cast<ZPropertyInfo>().Any(info => info.IsPersistent && info.HasChanges));
				}
				return result;
			}
		}

		#region Description Generation

		public bool IsMasterJournal
		{
			get; set;
		}

		public PeriodApportionmentJournalDescBuilder PeriodApportionmentJournalDescBuilder {
			get
			{
				if (fPeriodApportionmentJournalDescBuilder == null)
				{
					fPeriodApportionmentJournalDescBuilder = new PeriodApportionmentJournalDescBuilder(IsMasterJournal);
				}
				return fPeriodApportionmentJournalDescBuilder;
			}
		}
		PeriodApportionmentJournalDescBuilder fPeriodApportionmentJournalDescBuilder;

		public void UpdatePeriodApportionmentJournalHeaderDescription()
		{
			AH_Desc = PeriodApportionmentJournalDescBuilder.BuildHeaderDescription();
		}

		#endregion

		public bool IsNoteJournal => AH_TransactionType == TransactionTypes.GLNoteJournal;

		public bool IsStdJournal => AH_TransactionType == TransactionTypes.GLStandardJournal;

		public bool IsRevJournal => AH_TransactionType == TransactionTypes.GLReversingJournal;

		public bool IsAutoJournal => AH_TransactionType == TransactionTypes.GLAutoJournal;

		public bool IsPostDateEnabled => (IsStdJournal || IsNoteJournal || IsRevJournal) && ArePostAndDueDatesEnabledInRegistry;

		public bool IsDueDateEnabled => IsRevJournal && ArePostAndDueDatesEnabledInRegistry;

		bool ArePostAndDueDatesEnabledInRegistry => AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.GetValueWithoutFallback(Company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		protected override ZDecimal RoundAmountToLocalDecimals(ZDecimal amount)
		{
			return IsNoteJournal ? amount : base.RoundAmountToLocalDecimals(amount);
		}

		public override int OSCurrencyDecimals => IsNoteJournal ? AccountingConstants.CurrencyDefaultValues.NoteJournalCurrencyDecimal : base.OSCurrencyDecimals;

		public override int LocalCurrencyDecimals => IsNoteJournal ? AccountingConstants.CurrencyDefaultValues.NoteJournalCurrencyDecimal : base.LocalCurrencyDecimals;

		public GLJournalApprovalRequest GetPendingApprovalRequestByTransactionBelongsToGroup()
		{
			if (AH_TransactionBelongsToGroup.IsValid)
			{
				var query = new ZQuery(GenApprovalRequestSchema.PK, AH_TransactionBelongsToGroup);
				query.AddToFilter(GenApprovalRequestSchema.XP_ApprovalStatus, new[] { Constants.GenApprovalRequestApprovalStatus.Requested, Constants.GenApprovalRequestApprovalStatus.Approved });
				return Factory.LoadTop1<GLJournalApprovalRequest>(query);
			}
			return null;
		}

		public void LinkOriginalJournalToApprovalRequest(BusinessObjectFactory factory)
		{
			if (IsJournalCanBeReversed)
			{
				var originalTransaction = factory.Load<GLJournal>(OriginalTransaction.PK);
				originalTransaction.AH_TransactionBelongsToGroup = latestLinkedApprovalRequestPK;
			}
		}

		public bool IsJournalCanBeReversed => !IsInDatabase && IsReverseTransaction && !IsPeriodExists;

		public bool IsLinkedWithDSBJobCloseBatch_Cached
			=> Factory.GetCachedValue(
				$"{PK.ToStringKey()}|IsLinkedWithDSBJobCloseBatch_Cached",
				() => CargoWise.Data.Db.Connection.Exists($"FROM dbo.DsbJobCloseBatch WHERE JBB_AH_Journal = @journalPK",
					(x) => x.AddParameter(ZSqlParameter.New("@journalPK", PK, DsbJobCloseBatchSchema.JBB_AH_Journal))),
				CacheStalenessPolicy.StaleOnFactorySave
			);

		public IEnumerable<string> GetComplianceReportTypesByTablePrefixAndStatus(string tablePrefix, string[] statusList)
		{
			var gldComplianceReportCodeList = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value
					.Cast<ComplianceReportConfiguration>()
					.Where(x => x.ReportBaseTablePrefix == tablePrefix)
					.Select(x => x.ReportCode);
			if (gldComplianceReportCodeList.IsNullOrEmpty())
			{
				return Enumerable.Empty<string>();
			}

			var query = new ZDBOnlyQuery(typeof(AccComplianceReport));
			query.AddToFilter(AccComplianceReportSchema.ACR_ReportType, gldComplianceReportCodeList);
			query.AddToFilter(AccComplianceReportSchema.ACR_Status, statusList);
			query.AddToFilter(AccComplianceReportSchema.ACR_GC_Company, GlbCompany.CurrentCompany.PK);
			if (!Factory.Exists(typeof(AccComplianceReport), query))
			{
				return Enumerable.Empty<string>();
			}

			return GetComplianceReportTypesCore(tablePrefix, statusList);
		}

		IEnumerable<string> GetComplianceReportTypesCore(string tablePrefix, string[] statusList)
		{
			string queryComplianceReportSql;
			switch (tablePrefix)
			{
				case ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.GeneralLedgerData:
					queryComplianceReportSql = GetGLDTransactionComplianceReportSql;
					break;
				case ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes.AllTransactions:
					queryComplianceReportSql = GetAllTransactionComplianceReportSql(statusList);
					break;
				default:
					return Enumerable.Empty<string>();
			}

			var complianceReportTypes = new List<string>();
			CargoWise.Data.Db.Connection.ExecuteReader(
				queryComplianceReportSql,
				command =>
				{
					command.AddParameter("@PK", SqlDbType.UniqueIdentifier, PK.ToGuid());
					command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
					command.AddTableValuedParameter("@StatusList", TVPHelper.TVP_varchar, statusList);
					command.AddTableValuedParameter("@LinePKs", TVPHelper.TVP_uniqueidentifier, GLJournalLines.Select(line => line.PK.ToGuid()));
				},
				data =>
				{
					complianceReportTypes.Add((string)data["ACR_ReportType"]);
				});

			return complianceReportTypes;
		}

		string GetGLDTransactionComplianceReportSql => @"
SELECT DISTINCT ACR_ReportType
FROM
	dbo.AccComplianceReportTransactionPivot
INNER JOIN dbo.AccGeneralLedgerData
	ON ACL_ParentID = GLD_PK
INNER JOIN dbo.AccComplianceReport
	ON ACL_ACR_Report = ACR_PK
WHERE GLD_AH_TransactionHeader = @PK
	AND ACR_GC_Company = @CompanyPK
	AND ACR_Status IN (SELECT Value FROM @StatusList)
ORDER BY ACR_ReportType ASC
";

		string GetAllTransactionComplianceReportSql(string[] statusList)
		{
			var queryReportTypeFromPivotSql = @"
SELECT DISTINCT ACR_ReportType
FROM
	dbo.AccComplianceReportTransactionPivot
INNER JOIN dbo.AccTransactionLines
	ON ACL_ParentID = AL_PK
INNER JOIN dbo.AccComplianceReport
	ON ACL_ACR_Report = ACR_PK
WHERE ACL_ParentID IN (SELECT Value FROM @LinePKs)
	AND ACR_GC_Company = @CompanyPK
	AND ACR_Status IN (SELECT Value FROM @StatusList)
{0}
ORDER BY ACR_ReportType ASC
";
			var queryReportCodeFromQueueSql = @"
UNION ALL

SELECT DISTINCT ACR_ReportType FROM
	dbo.AccTransactionComplianceReportQueue
INNER JOIN dbo.AccTransactionLines
	ON ACQ_ParentID = AL_PK
INNER JOIN dbo.AccComplianceReport
	ON ACQ_ReportType = ACR_ReportType
WHERE ACQ_ParentID IN (SELECT Value FROM @LinePKs)
	AND ACQ_GC_Company = @CompanyPK
	AND ACR_GC_Company = @CompanyPK
	AND ACR_Status = 'QUE'
	AND (ACR_GB_Branch IS NULL OR ACQ_GB_Branch = ACR_GB_Branch)
	AND ACQ_Date >= ACR_DateFrom
	AND ACQ_Date < DATEADD(day, 1, ACR_DateTo)
";
			return string.Format(queryReportTypeFromPivotSql, statusList.Contains(AccComplianceReport.Status.ReportDataQueued) ? queryReportCodeFromQueueSql : string.Empty);
		}

		#region ITemplateCopyable Members

		protected override TransactionHeader CopyTransaction()
		{
			GLJournal copyOfCurrent = (GLJournal)Factory.New(GetType());

			copyOfCurrent.CopyHeaderValuesFrom(this);

			foreach (GLJournalLine line in this.GLJournalLines)
			{
				GLJournalLine newLine = copyOfCurrent.GLJournalLines.AddNew();
				newLine.CopyValuesFrom(line);
			}
			if (!IsPostDateEnabled)
			{
				copyOfCurrent.PostPeriod = copyOfCurrent.PeriodCalculator.GetPeriodFromDate(ZDateTime.Now);
			}
			copyOfCurrent.AgePeriod = 0;
			return copyOfCurrent;
		}

		void CopyHeaderValuesFrom(GLJournal journal)
		{
			AH_Ledger = LedgerTypes.General;
			AH_TransactionType = journal.AH_TransactionType;
			AH_Desc = journal.AH_Desc;
			AH_InvoiceDate = ZDateTime.Now;
			AH_PostDate = journal.AH_PostDate;

			if (journal.AH_DueDate.IsValid)
			{
				AH_DueDate = journal.AH_DueDate;
			}

			AH_TransactionCategory = journal.AH_TransactionCategory;
			AH_OSExTaxAmount = journal.AH_OSExTaxAmount;
			AH_RX_NKTransactionCurrency = journal.AH_RX_NKTransactionCurrency;
			AH_ExchangeRate = journal.AH_ExchangeRate;
			AH_GB = journal.AH_GB;
			AH_GE = journal.AH_GE;
		}

		#endregion

		#region IDocumentSupportable Members

		public override DocumentSupporter DocumentSupporter => new GLJournalDocumentSupporter(this);

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new AccountingDocManagerInfo(this, Constants.DocManagerCodes.GLJournal));
		AccountingDocManagerInfo docManagerInfo;

		#endregion

		#region IEDocsParsingSupport Members

		string IEDocsParsingSupport.UtilityData => throw new NotImplementedException();

		bool IEDocsParsingSupport.DenySendForParsing(Guid docPK, string docType, string fileName)
		{
			return true;
		}

		#endregion

		#region CreateEDocOnSaving

		ZGuid EDocCreatedOnSavingUniqueKey { get; set; }

		[SuppressMessage("Decrutification", "WTG3012:AvoidBoolLiteralsInLargerBoolExpressions", Justification = "#if directive in expression")]
		protected virtual bool ShouldCreateEDocOnSaving => true
#if DEBUG
			&& !(Globals.IsTest && SuspendShouldCreateEDocOnSaving_ForTestOnly > 0)
#endif
			;

#if DEBUG
		public static IDisposable SuspendCreateEDocOnSaving() => new DisposableAction(() => SuspendShouldCreateEDocOnSaving_ForTestOnly++, () => SuspendShouldCreateEDocOnSaving_ForTestOnly--);

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static int SuspendShouldCreateEDocOnSaving_ForTestOnly = 0;
#endif

		void InitEDocRelatedFactories()
		{
			if (ShouldCreateEDocOnSaving)
			{
				var docInfo = DocManagerInfo;
				docInfo.UseBusinessEntityFactoryAsInternal = true;
				var docFactory = (docInfo.MasterFactory as BusinessObjectFactory);
				var allEDocs = docInfo.AllEDocs;

				if (allEDocs != null)
				{
					if (docFactory.ChildFactories.Contains(Factory))
					{
						docFactory.ChildFactories.Remove(Factory);
					}

					if (!Factory.ChildFactories.Contains(docFactory))
					{
						Factory.ChildFactories.Add(docFactory);
					}
				}
			}
		}

		bool IsCreatingEDocOnSaving;

		void CreateEDocOnSaving()
		{
			if (ShouldCreateEDocOnSaving && EDocCreatedOnSavingUniqueKey.IsEmpty)
			{
				var fileName = ZString.Format((NoResString)"GL {0}_{1}_{2}_{3}.pdf", this.AH_TransactionNum, Env.CurrentBranch.Code, Env.CurrentUser.Initials, Env.Time.CurrentLocalDateTime.ToString("dMMyyyy hmmss tt", CultureInfo.InvariantCulture));
				using (var utility = new EDocAttachementCreator<GLJournal>(this, fileName, Constants.DocManagerCodes.GLJournal, true, false))
				{
					try
					{
						IsCreatingEDocOnSaving = true;
						utility.PrintDocument(this, (NoResString)"General Ledger Journal", DocumentEngine.AllowedDeliveryOptions.All);
					}
					finally
					{
						IsCreatingEDocOnSaving = false;
					}
					EDocCreatedOnSavingUniqueKey = utility.EDocCreatedUniqueKey;
				}
			}
		}

		protected override bool ShouldUseAddedLogNotInDatabase => (IsCreatingEDocOnSaving && !IsInDatabase);

		void HandleCreatedEDocOnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded && !EDocCreatedOnSavingUniqueKey.IsEmpty)
			{
				var edoc = DocManagerInfo.AllEDocs.GetFromUniqueKey(EDocCreatedOnSavingUniqueKey.ToGuid());
				if (edoc != null)
				{
					DocManagerInfo.AllEDocs.Remove(edoc);
				}
			}

			EDocCreatedOnSavingUniqueKey = ZGuid.Empty;
		}

		public void OnGLJournalLineSaving()
		{
			if (IsInDatabase)
			{
				CreateEDocOnSaving();
			}
		}

		public void OnGLJournalLineSaved(bool saveSucceeded)
		{
			HandleCreatedEDocOnSaved(saveSucceeded);
		}

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges => base.ShouldCreateAutoLogIfOnlyChildrenHaveChanges & DoesHeaderAndLinesHavePersistentChanges;
		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges => base.ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges && DoesHeaderAndLinesHavePersistentChanges;

		#endregion

		public override ZBool CanApplyTaxBranch => false;
	}

	public class GLJournalDocumentSupporter : TransactionHeader.TransactionHeaderDocumentSupporter
	{
		public GLJournalDocumentSupporter(GLJournal gLJournal)
			: base(gLJournal)
		{
		}

		protected GLJournal GLJournal => (GLJournal)BusinessObject;

		#region Overrides

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			var dataContexts = new List<Constants.DataContext>
			{
				Constants.DataContext.GLJournal,
				Constants.DataContext.GenericFreightJob,
				Constants.DataContext.ChinaJournalListing,
			};

			if (ShouldSupportAccountingVoucher())
			{
				dataContexts.Add(Constants.DataContext.AccountingVoucher);
			}

			return dataContexts.ToArray();
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			if (dataContext == Constants.DataContext.GLJournal)
			{
				return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.GLJournal, GLJournal) };
			}
			else
			{
				return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
			}
		}

		#endregion
	}

	public class GLJournalTypeDecider : TypeDecider
	{
		public override Type GetTypeForBinding()
		{
			return typeof(GLJournal);
		}

		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string receiptType = row[TransactionHeader.Schema.AH_ReceiptType].ToString();

			if (receiptType == ReceiptTypes.ForeignCurrencyBalance)
			{
				return typeof(FCBAdjustmentJournal);
			}
			else
			{
				return typeof(GLJournal);
			}
		}

		public override Type GetTypeForNew()
		{
			return typeof(GLJournal);
		}
	}

	public class NoteJournalCurrency : RefCurrency
	{
		public NoteJournalCurrency(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ZInt RX_SubUnitRatio => (int)Math.Pow(10, AccountingConstants.CurrencyDefaultValues.NoteJournalCurrencyDecimal);
	}
}
