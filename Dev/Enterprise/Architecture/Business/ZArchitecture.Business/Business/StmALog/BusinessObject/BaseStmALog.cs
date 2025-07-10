using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Workflow;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class BaseStmALog : AutoStmALog, IBusinessObjectReload, ICanBeSavedByDocumentFactory, IStmALog, IStmALogInMemoryIdentifier
	{
		#region Schema

		public abstract new class Schema : AutoStmALog.Schema
		{
			public const string SL_EventDescription = "SL_EventDescription";
			public const string SL_TableFriendlyName = "SL_TableFriendlyName";
			public const string SL_TableFriendlyNameForBinding = "SL_TableFriendlyNameForBinding";
			public const string SL_UserNameAndInitials = "SL_UserNameAndInitials";
			public const string SL_ContextDescription = "SL_ContextDescription";
			public const string DisplayEventReference = "DisplayEventReference";
			public const string ReferenceFreeText = "ReferenceFreeText";
			public static class Indexes
			{
				public const string NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime = "NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime";
			}
		}

		#endregion

		public BaseStmALog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SL_PostedTimeUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SL_IsCancelled), ConcurrencyPolicy.Ignore);
		}

		#region Cancellation

		public bool CanBeCancelledByUser
		{
			get
			{
				return
					Master != null &&
					!Events.EventsThatCannotBeCancelled.Contains(Events.All[SL_SE_NKEvent]) &&
					!Master.Logs.EventsThatCannotBeCancelled.Contains(SL_SE_NKEvent);
			}
		}

		public virtual void Cancel()
		{
			if (!SL_IsCancelled)
			{
				SL_IsCancelled = true;
			}
		}

		public virtual void Cancel(ZDateTime eventTime)
		{
			if (!SL_IsCancelled)
			{
				SL_IsCancelled = true;
				SL_EventTime = eventTime;
			}
		}

		public override ZPropertyInfo SL_IsCancelledInfo
		{
			get
			{
				var result = base.SL_IsCancelledInfo;
				return result;
			}
		}

		#endregion

		public virtual void Reactivate()
		{
			if (SL_IsCancelled)
			{
				SL_IsCancelled = false;
			}
		}

		public virtual void Reactivate(ZDateTime eventTime)
		{
			if (SL_IsCancelled)
			{
				SL_IsCancelled = false;
				SL_EventTime = eventTime;
			}
		}

		#region Business Object Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			SL_EventTimeOffset = new ZDateTimeOffset(ZDateTime.Now, DateTimeKind.Local);
			base.SL_PostedTimeUtc = ZDateTime.Empty;

			IGlbStaff currentUser = StaticCurrentFetcher.Instance.CurrentUser;
			SL_GS_NKUser = currentUser != null
				? currentUser.GS_Code
				: Globals.IsUserInteractive
					? ZString.Empty
					: new ZString(Environment.User.ServiceUserCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a developer error")]
		public override void Delete()
		{
			if (IsInDatabase)
			{
				var callStack = new System.Diagnostics.StackTrace().ToString();
				string message = "Disallowed attempt to delete an StmALog object already in database. \r\nCallStack: \r\n" + callStack;
#if DEBUG
				throw new InvalidOperationException(message);
#else
				ErrorReporter.ReportOnce(callStack.GetHashCode().ToString(), message);
#endif
			}
			else
			{
				if (!IsDeleted)
				{
					var categoryKey = StmALog.ErrorReportKeys.Category.DeletedStmALog;
					ErrorReporter.SetAdditionalInfo(categoryKey, "deletedLog.PK", PK.ToString());
					ErrorReporter.SetAdditionalInfo(categoryKey, "deletedLog.SL_EventTime", SL_EventTime.ToBestReadableDateTimeString());
					ErrorReporter.SetAdditionalInfo(categoryKey, "deletedLog.SL_SE_NKEvent", SL_SE_NKEvent.ToString());
					ErrorReporter.SetAdditionalInfo(categoryKey, "deletedLog.SL_Parent", SL_Parent.ToString());
					ErrorReporter.SetAdditionalInfo(categoryKey, "deletedLog.SL_Reference", SL_Reference.ToString());
					ErrorReporter.SetAdditionalInfo(categoryKey, "deletedLog.SL_IsCancelled", SL_IsCancelled.ToString());
					ErrorReporter.SetAdditionalInfo(categoryKey, "deletedLog.GetType()", GetType().ToString());

					ErrorReporter.AddStackTraceToAdditionalInfoIfFlagSet(categoryKey, "deletedLog.StackTrace", StmALog.ErrorReportKeys.Flags.TrackDeletedStmALogs);
				}
				/*
				 * Typically we would throw errors when deleting objects that are already deleted.
				 * However. In this case, in order to avoid refactoring/creating a brand new synchronization mechanism of StmALogs and
				 * a selection of business object properties, I have chosen to embrace the spagghetti. That is:
				 ** It is supported for OnDelete to try and delete this bizo (since it will update Milestones/arbitrary properties on any job that will attempt to delete this StmALog)
				 */
				if (deletingCount < 1)
				{
					try
					{
						++deletingCount;
						if (!IsDeleted)
						{
							OnDelete();
						}
						base.Delete();
					}
					finally
					{
						--deletingCount;
					}
				}
			}
		}

		int deletingCount;

		protected virtual void OnDelete() { }

		public override void OnSaving()
		{
			base.OnSaving();

			if (!SL_Table.IsEmpty && Factory.CanLoadFromTable(SL_Table) && Master != null && !string.IsNullOrEmpty(Master.LogsParentTableName) && Master.LogsParentTableName != SL_Table)
			{
				ErrorReporter.ReportOnce(AppendServiceTaskCodeForErrorReporter("ChangingSL_Table"), string.Format(CultureInfo.InvariantCulture, "SL_Table is different to Master table in OnSaving. Master table '{0}', SL_Table '{1}', Event code '{2}'.", // Column names are in a string, which is okay
					/*0*/ master.LogsParentTableName,
					/*1*/ SL_Table,
					/*2*/ SL_SE_NKEvent));
			}

			if (!IsInDatabase)
			{
				InitializePostedTime();
			}
		}

		string AppendServiceTaskCodeForErrorReporter(string errorKey)
		{
			var serviceTaskCode = ObjectFactory.Get<IEnv>().Instance.ServiceTaskCode;
			return string.IsNullOrEmpty(serviceTaskCode) ? errorKey : string.Format(CultureInfo.InvariantCulture, "{0}_{1}", errorKey, serviceTaskCode);
		}

		internal void InitializePostedTime()
		{
			if (!SL_PostedTimeUtc.IsValid)
			{
				base.SL_PostedTimeUtc = ZDateTime.UtcNow;
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (!saveSucceeded)
			{
				if (!IsInDatabase)
				{
					base.SL_PostedTimeUtc = ZDateTime.Empty;
				}
			}

			base.OnSaved(saveSucceeded);
		}

		public override bool ReadOnly
		{
			get { return base.ReadOnly && !IsEditing; }
			set { base.ReadOnly = value; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (Master == null)
			{
				Master = this;
			}
		}
#endif

		#endregion

		public bool IsEditing
		{
			get;
			set;
		}

		#region New Properties

		#region CompanyCode

		public ZString CompanyCode => Factory.LoadFromNaturalKey<IGlbBranch>(GlbBranchSchema.GB_Code, SL_GB_NKBranch)?.Company.GC_Code ?? ZString.Empty;

		public ZPropertyInfo CompanyCodeInfo => GetZPropertyInfo(nameof(CompanyCode));

		#endregion

		#region SL_EventDescription

		public ZString SL_EventDescription
		{
			get { return Event == null ? GetLegacyEventDescription() : Event.SE_DescMultilingual; }
		}

		public ZPropertyInfo SL_EventDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.SL_EventDescription); }
		}

		ZString GetLegacyEventDescription()
		{
			switch (SL_SE_NKEvent)
			{
				case "AVC":
					return Res.GetString("e17c3943-9595-40ad-9383-193454ede31d", "Advised Cartage");
				case "DTC":
					return Res.GetString("d2ed1e7c-5e4c-4bb9-8418-42dabdacca96", "Documents to Cartage");
				case "CAP":
					return Res.GetString("beb032b9-eacd-4ee9-83ec-93fd6b065660", "Cartage Advise Printed");
			}
			return ZString.Empty;
		}

		#endregion

		#region SL_TableFriendlyName

		public string SuffixForTableFriendlyName { get; set; }

		public ZString SL_TableFriendlyName => GetTableFriendlyName(this, Master as BusinessObject, Factory, false);

		public ZString SL_TableFriendlyNameForBinding => GetTableFriendlyName(this, Master as BusinessObject, Factory, true);

		static ZString GetEnglishHumanReadableName(BusinessObject master)
		{
			var userLanguage = EnvProxy.Instance.CurrentUser.Language;
			if (userLanguage == Enterprise.Core.SharedConstants.Languages.English)
			{
				return master.HumanReadableName;
			}

			var resourceStrings = ObjectFactory.Get<IResourceStrings>();
			resourceStrings.CurrentLanguage = Enterprise.Core.SharedConstants.Languages.English;
			var humanReadableName = master.HumanReadableName;
			resourceStrings.CurrentLanguage = userLanguage;

			return humanReadableName;
		}
		public static ZString GetTableFriendlyName(IStmALog log, BusinessObject master, BusinessObjectFactory factory, bool forBinding = false)
		{
			if (log.SL_SE_NKEvent == Events.LoginCode || log.SL_SE_NKEvent == Events.LogoutCode)
			{
				var branch = factory.LoadFromNaturalKey<IGlbBranch>(GlbBranchSchema.GB_Code, log.SL_GB_NKBranch);

				if (branch != null)
				{
					return branch.Company.GC_Name;
				}
			}
			ZString result;
			if (master != null)
			{
				result = forBinding ? master.HumanReadableName : GetEnglishHumanReadableName(master);
			}
			else
			{
				var slTable = log.SL_Table;
				result = !slTable.IsEmpty
					? (ZString)DataBoundResourceStrings.GetTableDescriptiveName(slTable)
					: ZString.Empty;
			}

			if (!result.IsEmpty && log is BaseStmALog baseLog && !baseLog.IsRelatedInParentView)
			{
				var suffix = !string.IsNullOrEmpty(baseLog.SuffixForTableFriendlyName) ? " " + baseLog.SuffixForTableFriendlyName : string.Empty;
				result = StmALogCollectionView.ThisPrefix(result + suffix, forBinding);
			}
			return result;
		}

		public ZPropertyInfo SL_TableFriendlyNameInfo
		{
			get { return GetZPropertyInfo(Schema.SL_TableFriendlyName); }
		}

		bool IsRelatedInParentView
		{
			get
			{
				var mainMaster = MainMaster;
				return mainMaster != null && SL_Parent != mainMaster.LogsParentPK;
			}
		}

		IStmALogParent MainMaster
		{
			get
			{
				foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)this).ParentCollections)
				{
					StmALogCollectionView collectionView = collection as StmALogCollectionView;
					if (collectionView != null && collectionView.Parent != null)
					{
						return collectionView.Parent;
					}

					StmALogCollectionWithMaster collectionWithMaster = collection as StmALogCollectionWithMaster;
					if (collectionWithMaster != null && collectionWithMaster.Master != null)
					{
						return collectionWithMaster.Master;
					}
				}

				return null;
			}
		}

		#endregion

		#region Context Description

		public ZString SL_ContextDescription
		{
			get
			{
				var mainMaster = MainMaster;
				if (mainMaster != null && Master != null && SL_Parent != mainMaster.LogsParentPK)
				{
					var relatedItemsNameProvider = mainMaster as IRelatedItemsNameProvider;
					if (relatedItemsNameProvider != null)
					{
						return relatedItemsNameProvider.GetNameOfRelatedItem(Master);
					}
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo SL_ContextDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(SL_ContextDescription)); }
		}

		#endregion

		#region SL_InitialiseAndUserName

		public ZString SL_UserNameAndInitials => User == null ? ZString.Empty : FormatStaffName(User.GS_FullName, User.GS_Code);

		public static ZString FormatStaffName(ZString fullName, ZString staffCode) => ZString.Format("{0} ({1})", fullName, staffCode);

		public ZPropertyInfo SL_UserNameAndInitialsInfo
		{
			get { return GetZPropertyInfo(Schema.SL_UserNameAndInitials); }
		}

		#endregion

		#region SL_EventTimeOffset

		public ZDateTimeOffset EventTimeOffset => SL_EventTimeOffset;

		public ZDateTimeOffset SL_EventTimeOffset
		{
			get
			{
				return ToDateTimeOffset(Factory, SL_EventTime, SL_EventTimeUtc, SL_GB_NKBranch);
			}
			set
			{
				base.SL_EventTime = value.ToZDateTime();
				base.SL_EventTimeUtc = value.ToUtcZDateTime();
				SL_EventTimeOffsetInfo.RefreshBinding();
			}
		}

		public static ZDateTimeOffset ToDateTimeOffset(BusinessObjectFactory factory, ZDateTime eventTime, ZDateTime eventTimeUtc, ZString branch)
		{
			if (eventTimeUtc.IsValid)
			{
				return ZDateTimeOffset.GetFromLocalAndUtc(eventTime, eventTimeUtc);
			}

			return ToDateTimeOffset(factory, eventTime, branch);
		}

		public static ZDateTimeOffset ToDateTimeOffset(BusinessObjectFactory factory, ZDateTime eventTime, ZString branch)
		{
			var timeZone = GetTimeZone(factory, branch);
			if (timeZone != null && eventTime.IsValid)
			{
				return new ZDateTimeOffset(eventTime, timeZone.GetUtcOffsetBasedOnLocal(eventTime.ToDateTime()));
			}

			return new ZDateTimeOffset(eventTime);
		}

		static ITimeZone GetTimeZone(BusinessObjectFactory factory, string branch)
		{
			var envBranch = EnvProxy.Instance.CurrentBranch;
			if (envBranch != null && branch == envBranch.Code && envBranch is IGlbBranch b)
			{
				return b.HomeTimeZone;
			}
			return factory.LoadFromNaturalKey<IGlbBranch>(GlbBranchSchema.GB_Code, branch)?.HomeTimeZone;
		}

		public ZPropertyInfo SL_EventTimeOffsetInfo => GetZPropertyInfo(nameof(SL_EventTimeOffset));

		#endregion

		#region SL_Reference extensions

		public virtual IDictionary<string, string> Parameters
		{
			get
			{
				return new Dictionary<string, string>();
			}
		}

		[MaxLength(AutoStmALog.Schema.SL_ReferenceMaxLength)]
		public virtual ZString ReferenceFreeText
		{
			get
			{
				return SL_Reference;
			}

			set
			{
				CheckMaximumLength(ReferenceFreeTextInfo, value);
				SL_Reference = value;

				ReferenceFreeTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ReferenceFreeTextInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.ReferenceFreeText);
			}
		}

		public virtual ZString SL_ReferenceForBinding
		{
			get
			{
				return GetReferenceForBinding(SL_Reference);
			}
			set
			{
				SL_Reference = value;
			}
		}

		public static ZString GetReferenceForBinding(ZString reference)
		{
			var freeTextBits = new List<string>();
			var paramBits = new List<(string key, string value)>();
			EventLogReferenceBuilder.New().ParseReference(reference, (text) => freeTextBits.Add(text), (key, value) => paramBits.Add((key, value)));

			var sb = new StringBuilder();
			sb.Append(string.Join(StmALog.ReferenceDelimiter.ToString(), freeTextBits.Where(text => !ZGuid.IsGuid(text))));

			if (paramBits.Count > 0)
			{
				sb.Append(StmALog.ReferenceDelimiter);
			}

			sb.Append(string.Join(StmALog.ReferenceDelimiter.ToString(), paramBits.Select(paramBit => paramBit.key + StmALog.CodeValuePairDelimiter + paramBit.value)));
			return sb.ToString();
		}

		public static ZGuid GetGuid(ZString reference)
		{
			var freeTextBits = new List<string>();
			EventLogReferenceBuilder.New().ParseReference(reference, (text) => freeTextBits.Add(text), (key, value) => { });

			foreach (var freeTextBit in freeTextBits)
			{
				ZGuid result;

				if (ZGuid.TryParse(freeTextBit, out result))
				{
					return result;
				}
			}

			return ZGuid.Empty;
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region Master

		IStmALogParent master;
		public IStmALogParent Master
		{
			get
			{
				if (master == null)
				{
					master = GetMaster(SL_Parent, SL_Table, Factory);
				}
				return master;
			}
			set
			{
				master = value;
				CheckNotEqualParentAndMaster();
				SL_Table = value != null ? value.LogsParentTableName : string.Empty;
				SL_Parent = value != null ? value.LogsParentPK : ZGuid.Empty;
			}
		}

		public static IStmALogParent GetMaster(ZGuid parentGUID, ZString parentTable, BusinessObjectFactory factory)
		{
			if (parentGUID.IsValid)
			{
				var master = factory.GetBizOsForPK(parentGUID.ToGuid())
					.OfType<IStmALogParent>()
					.FirstOrDefault(parent => !parent.IsDeleted && parentTable == parent.LogsParentTableName);

				if (master == null)
				{
					var prefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(parentTable);
					master = LoadMaster(factory, parentGUID, prefix) as IStmALogParent;
				}

				return master;
			}

			return null;
		}

		public static BusinessObject LoadMaster(BusinessObjectFactory factory, ZGuid parentGUID, string prefix)
		{
			var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(prefix, false);
			if (type != null)
			{
				return factory.Load(type, parentGUID);
			}
			return null;
		}

		protected void ResetMaster()
		{
			CheckNotEqualParentAndMaster();
			if (master != null)
			{
				var bizoMaster = master as BusinessObject;
				if (bizoMaster == null || bizoMaster.IsDeleted || bizoMaster.IsInDatabase || bizoMaster.PK != SL_Parent || bizoMaster.TableName != SL_Table)
				{
					master = null;
				}
			}
		}

		void CheckNotEqualParentAndMaster()
		{
			if (master != null && !(Master is NonPersistentBusinessObject) && !SL_Parent.IsEmpty && SL_Parent.IsValid && Master.LogsParentPK != SL_Parent)
			{
				ErrorReporter.ReportOnce("ChangingSL_Parent", string.Format(@"Trying to set different SL_Parent and Master.
Values are:
PK:          {0}
SL_Parent:   {1}
SL_Table:    {2}
MasterType:  {3}
MasterPK:    {4}
MasterTable: {5}", PK.ToString(), SL_Parent.ToString(), SL_Table, master.GetType(), master.LogsParentPK, master.LogsParentTableName));
			}
		}

		internal bool HasMaster
		{
			get { return Master != null && !Master.IsDeleted; }
		}

		#endregion

		[BusinessObjectTestExclude]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a developer error")]
		public override ZGuid SL_Parent
		{
			get { return base.SL_Parent; }
			set
			{
				if (SL_Parent != value)
				{
					if (IsInDatabase && !SL_Parent.IsEmpty && !value.IsEmpty)
					{
						string errorMessage = string.Format(
							"SL_Parent may not be changed once a log record has been saved. Changed from '{0}' to '{1}', Event code '{2}'",
							base.SL_Parent, value, SL_SE_NKEvent);
						ErrorReporter.ReportOnce("ChangingSL_Parent", errorMessage);
					}

					// TODO: M.K Remove when Work Item WI00057109 is done.
					// Also remove IsRemovingFromRelationship flag from BusinessObject.
					if (!value.IsValid && !IsRemovingFromRelationship)
					{
						ErrorReporter.ReportOnce("EmptyParentInNewStmALog",
							"SL_Parent of event " + SL_SE_NKEvent + " for table " + SL_Table + " was changed from {" + SL_Parent + "} to " +
							(value.IsEmpty ? "empty" : "invalid") + " by user " + SL_GS_NKUser);
					}

					base.SL_Parent = value;
					ResetMaster();
				}
			}
		}

		public IGlbStaff User
		{
			get { return (IGlbStaff)Factory.LoadFromNaturalKey(ObjectFactory.GetType<IGlbStaff>(), GlbStaffSchema.GS_Code, SL_GS_NKUser); }
		}

		#endregion

		#region Overridden Properties

		[BusinessObjectTestExclude] // ValidateSL_SE_NKEvent() reports a silent error when setting a crap code
		[List("EventsForBinding")]
		[ActionField(CollectionType = typeof(StmEventCodeDescriptionPairList), FieldType = ActionFieldType.Code)]
		public override ZString SL_SE_NKEvent
		{
			get { return base.SL_SE_NKEvent; }
			set { base.SL_SE_NKEvent = value; }
		}

		public override ZDateTime SL_EventTime
		{
			get { return base.SL_EventTime; }
			set
			{
				if (SL_EventTime != value)
				{
					base.SL_EventTime = value;
					base.SL_EventTimeUtc = value.IsValid ? TimeFactory.Instance.GetUtcFromLocalTime(value.ToDateTime()) : ZDateTime.Empty;
				}
			}
		}

		public override ZDateTime SL_EventTimeUtc
		{
			get { return base.SL_EventTimeUtc; }
			set
			{
				if (SL_EventTimeUtc != value)
				{
					base.SL_EventTimeUtc = value;
					base.SL_EventTime = value.IsValid ? TimeFactory.Instance.GetLocalTimeFromUtc(value.ToDateTime()) : ZDateTime.Empty;
				}
			}
		}

		public ZDateTime EventLocalBranchTime
		{
			get
			{
				return (SL_EventTimeUtc.IsValid) ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(SL_EventTimeUtc.ToDateTime()) : ZDateTime.Empty;
			}
		}
		public ZPropertyInfo EventLocalBranchTimeInfo
		{
			get { return GetZPropertyInfo(nameof(EventLocalBranchTime)); }
		}

		public override ZString SL_Table
		{
			get { return base.SL_Table; }
			set
			{
				ZString newValue = ZDataUtils.GetTableNameFromDbAndTableName(value);
				if (newValue.Length == 2)
				{
					ErrorReporter.ReportOnce(AppendServiceTaskCodeForErrorReporter("ShortSL_Table"), "Setting SL_Table to a short 'non tablename' value - " + newValue);
				}
				if (HasMaster && !SL_Table.IsEmpty && !string.IsNullOrEmpty(Master.LogsParentTableName) && Master.LogsParentTableName != newValue)
				{
					ErrorReporter.ReportOnce(AppendServiceTaskCodeForErrorReporter("ChangingSL_Table_NotMatchMaster"), "Setting SL_Table to '" + newValue + "' which does not match the master TableName '" + Master.LogsParentTableName + "'");
				}
				if (IsInDatabase)
				{
					if (!SL_Table.IsEmpty && newValue != SL_Table && SL_PostedTimeUtc > ZDateTime.UtcNow.AddYears(-1))
					{
						if (HasMaster && value == Master.LogsParentTableName)
						{
							// New value is correct. Where'd the old value come from? 
							string errorMessage = string.Format("SL_Table for a saved log record does not match master TableName: changing '{0}' to '{1}', Event code '{2}', Posted {3}.", // This is a developer error
																SL_Table, newValue, SL_SE_NKEvent, SL_PostedTimeUtc.ToLongTimeString());
							ErrorReporter.ReportOnce(AppendServiceTaskCodeForErrorReporter("ChangingSL_Table_OldValueExists"), errorMessage);
						}
						else
						{
							string errorMessage = string.Format("SL_Table may not be changed once a log record has been saved. Changed from '{0}' to '{1}', Event code '{2}', Posted {3}.", // This is a developer error
																SL_Table, newValue, SL_SE_NKEvent, SL_PostedTimeUtc.ToLongTimeString());
							ErrorReporter.ReportOnce(AppendServiceTaskCodeForErrorReporter("ChangingSL_Table_TryingToSetAfterSaved"), errorMessage);
						}
					}
				}

				base.SL_Table = newValue;
			}
		}

		public new ZBool SL_IsCancelled
		{
			get { return base.SL_IsCancelled; }
			private set
			{
				if (SL_IsCancelled != value)
				{
					base.SL_IsCancelled = value;
				}
			}
		}

		public override ZBool SL_IsEstimate
		{
			get { return base.SL_IsEstimate; }
			set
			{
				if (SL_IsEstimate != value)
				{
					base.SL_IsEstimate = value;
				}
			}
		}

		public override ZString SL_Reference
		{
			get { return base.SL_Reference; }
			set
			{
				if (SL_Reference != value)
				{
					base.SL_Reference = value.StripNonWesternEuropeanCharacters();
				}
			}
		}

		#endregion

		public StmEventCodeDescriptionPairList EventsForBinding
		{
			get { return eventsForBinding ?? (eventsForBinding = new StmEventCodeDescriptionPairList()); }
		}
		StmEventCodeDescriptionPairList eventsForBinding;

		#region SL_PostedTimeUtc(Local and Server)

		public ZDateTime PostedLocalBranchTime
		{
			get
			{
				ZDateTime result = (SL_PostedTimeUtc.IsValid) ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(SL_PostedTimeUtc.ToDateTime()) : ZDateTime.Empty;
				return result;
			}
		}
		public ZPropertyInfo PostedLocalBranchTimeInfo
		{
			get { return GetZPropertyInfo(nameof(PostedLocalBranchTime)); }
		}

		[BusinessObjectTestExclude] // Setting ST_PostedTime directly reports a silent error
		public sealed override ZDateTime SL_PostedTimeUtc
		{
			get => base.SL_PostedTimeUtc;
			set => throw new NotSupportedException("Setting SL_PostedTimeUtc is not supported");
		}

		#endregion

		#region IBusinessObjectReload

		BusinessObject IBusinessObjectReload.Reload(BusinessObjectFactory loadingFactory)
		{
			var query = GetQueryForReload(loadingFactory);
			query.AddToFilter(StmALogSchema.PK, PK);
			query.AddToFilter(StmALogSchema.SL_Parent, SL_Parent);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, SL_SE_NKEvent);
			query.AddToFilter(StmALogSchema.SL_EventTime, SL_EventTime);
			var type = GetType();
			BusinessObject result = loadingFactory.LoadTop1(type, query);
			if (result == null && SL_PostedTimeUtc.IsValid)
			{
				query = GetQueryForReload(loadingFactory);
				query.AddToFilter(StmALogSchema.PK, PK);
				query.AddToFilter(StmALogSchema.SL_Parent, SL_Parent);
				query.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SL_PostedTimeUtc);
				result = loadingFactory.LoadTop1(type, query);
				if (result == null)
				{
					query = GetQueryForReload(loadingFactory);
					query.AddToFilter(StmALogSchema.PK, PK);
					query.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SL_PostedTimeUtc);
					result = loadingFactory.LoadTop1(type, query);
				}
			}
			return result;
		}

		ZQuery GetQueryForReload(BusinessObjectFactory loadingFactory)
		{
			if (loadingFactory != Factory || !IsInDatabase)
			{
				return new ZQuery();
			}

			return new ZDBOnlyQuery(GetType());
		}

		#endregion
		public ZDateTime EventTime => SL_EventTime;
		public ZString Reference => SL_Reference;
		public ZBool IsEstimate => SL_IsEstimate;
		public ZString UserCode => SL_GS_NKUser;
		public ZDateTime PostedTimeUtc => SL_PostedTimeUtc;
		public ZString DepartmentCode => SL_GE_NKDepartment;
		public ZString BranchCode => SL_GB_NKBranch;
		public ZString Source => SL_TableFriendlyName;
		public ZDateTime PostedTimeLocal => EventLocalBranchTime;
		public ZDateTime EventTimeUtc => SL_EventTimeUtc;

		#region IWorkflowTriggerSource

		ZGuid IIdentified.Identifier => PK;
		ZGuid IWorkflowTriggerSource.ParentID => SL_Parent;
		ZString IWorkflowTriggerSource.SourceType => ZString.Empty; // In order to preserve backwards compatability with existing logs, StmALog is the default source type.
		ZString IWorkflowTriggerSource.CompanyCode => CompanyCode;
		ZString IWorkflowTriggerSource.FriendlyTableName => SL_TableFriendlyName;
		ZBool IWorkflowTriggerSource.IsCancelled => SL_IsCancelled;
		BusinessObject IStmALog.Master => (BusinessObject)Master;

		public SchemaColumn ReloadPerformanceIncreaseColumn => StmALogSchema.SL_PostedTimeUtc;

		public object ReloadPerformanceIncreaseColumnValue => SL_PostedTimeUtc.IsValidSqlDateTime ? SL_PostedTimeUtc : (!IsInDatabase && SL_PostedTimeUtc.IsEmpty ? ZDateTime.UtcNow : throw new InvalidOperationException(FormattableString.Invariant($"SL_PostedTimeUtc is not a valid SQL DateTime value - {SL_PostedTimeUtc}, Business Object is in database ({IsInDatabase}), Event Code ({SL_SE_NKEvent})")));

		#endregion

		#region ILinkedPropagation

		ZGuid IStmALogInMemoryIdentifier.InMemoryIdentifier => LazyLinkedPropagationState.InMemoryIdentifier;

		public void SetInMemoryIdentifier(ZGuid inMemoryIdentifier)
		{
			LazyLinkedPropagationState.AddLogInMemoryUniqueIdentifier(this, inMemoryIdentifier);
		}

		IStmALog IStmALog.WeakCopy()
		{
			return new LogCopy(this);
		}

		public Enterprise.Integration.IPropagationSettings PropagationSettings => propagationSettings = propagationSettings ?? new DefaultPropagationSettings();

		internal void SetPropagationConfig(Enterprise.Integration.IPropagationSettings propagationSettings)
		{
			this.propagationSettings = propagationSettings;
		}

		protected ILinkedPropagationState LinkedPropagationState => LazyLinkedPropagationState;
		LinkedPropagationState LazyLinkedPropagationState => linkedPropagation = linkedPropagation ?? new LinkedPropagationState(this.Factory, this);

		LinkedPropagationState linkedPropagation;
		Enterprise.Integration.IPropagationSettings propagationSettings;

		#endregion

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return new[] { StmALogSchema.Constants.SL_PostedTimeUtc };
		}

		IStaff IStmALog.User => (IStaff)Factory.LoadFromNaturalKey<IGlbStaff>(GlbStaffSchema.GS_Code, SL_GS_NKUser);

		ZString IEventUserContextSource.StaffCode => SL_GS_NKUser;

		IKeyDataPairCollection sourceInfoItems;
		public virtual IKeyDataPairCollection SourceInfoItems => sourceInfoItems ?? (sourceInfoItems = new KeyDataPairCollection(Factory));

		public ZString DisplayEventReference => DisplayEventReferenceCore;
		protected virtual ZString DisplayEventReferenceCore { get; }
		public virtual ILogsParams Params => new LogsParams(SL_Reference);
	}
}
