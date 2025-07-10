using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public interface IStmALogInternals
	{
		void CancelWithoutNotifications();
	}

	[DebuggerDisplay("Event: {SL_SE_NKEvent}, TableFriendlyName: {SL_TableFriendlyName}, Reference: {SL_Reference}, IsCancelled: {SL_IsCancelled}")]
	[IDontMindLoadingASubclassInstead]
	public class StmALog : BaseStmALog,
		IStmALog,
		IStmALogInternals,
		IUpdateFieldsLock,
		IUpdateFieldsLockReachAround,
		IExternalWorkflowTrigger,
		IEventUserContextSource
	{
		public StmALog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Constants

		public const char ReferenceDelimiter = '|';

		public const char EscapeChar = '\\';

		public const char CodeValuePairDelimiter = '=';

		public static char SpaceDelimiter => ' ';

		public static readonly char[] EventReferenceSpecialCharacters = new[] { ReferenceDelimiter, CodeValuePairDelimiter };

		#endregion

		#region Properties

		#region Overridden

		[BusinessObjectTestExclude] // ValidateSL_SE_NKEvent() reports a silent error when setting a crap code
		[List("EventsForBinding")]
		public override ZString SL_SE_NKEvent
		{
			get { return base.SL_SE_NKEvent; }
			set
			{
				CheckCanUpdateKeyFields();

				if (value != base.SL_SE_NKEvent)
				{
					base.SL_SE_NKEvent = value;

					ResetDisplayEventReference();
				}
			}
		}

		public override ZDateTime SL_EventTime
		{
			get { return base.SL_EventTime; }
			set
			{
				CheckCanUpdateKeyFields();
				if (!value.IsValid)
				{
					ErrorReporter.ReportOnceWithAdditionalInfo(null, LogMessages.InvalidEventTimeError(PK),
						ErrorReportKeys.Category.WorkflowGun,
						ErrorReportKeys.Category.DeletedStmALog);
				}
				base.SL_EventTime = value;
			}
		}

		public override ZString SL_Table
		{
			get { return base.SL_Table; }
			set
			{
				CheckCanUpdateKeyFields();
				base.SL_Table = value;
			}
		}

		public override ZGuid SL_Parent
		{
			get { return base.SL_Parent; }
			set
			{
				CheckCanUpdateKeyFields();
				base.SL_Parent = value;
			}
		}

		public override ZBool SL_IsEstimate
		{
			get { return base.SL_IsEstimate; }
			set
			{
				CheckCanUpdateKeyFields();
				base.SL_IsEstimate = value;
			}
		}

		public override ZString SL_Reference
		{
			get { return base.SL_Reference; }
			set
			{
				CheckCanUpdateKeyFields();
				base.SL_Reference = value;

				referenceFreeText = GetFreeTextFromReference(value);
				ReferenceFreeTextInfo.RefreshBinding();

				if (parameters != null)
				{
					parameters.CollectionChanged -= ParametersChanged;
				}

				try
				{
					parameters = GetParametersFromReference(value, ParseReferenceError.Exception);
				}
				catch (ArgumentException ex)
				{
					ErrorReporter.ReportOnce(StmALog.LogMessages.SL_ReferenceArgumentError(ex, this));
					parameters = GetParametersFromReference(value, ParseReferenceError.None);
				}

				parameters.CollectionChanged += ParametersChanged;
				ResetDisplayEventReference();
			}
		}

		protected override void OnDelete()
		{
			OnDeleteStmALog();
			base.OnDelete();
		}

		protected virtual void OnDeleteStmALog()
		{
			if (Master is IWorkflowProviderCore || LinkedPropagationState.InMemoryPropagatedLogs.Any())
			{
				using (EventRecursionHandler.WithEventRecursionDetection())
				{
					GetProcessTaskHandler().Withdraw();
				}
			}
		}

		public override bool HasChanges { get => !suppressHasChanges && base.HasChanges; set => base.HasChanges = value; }

		public static IDisposable SuppressHasChanges()
		{
			if (suppressHasChanges)
			{
				return null;
			}
			else
			{
				suppressHasChanges = true;
				return new DisposableAction(() => suppressHasChanges = false);
			}
		}

		[ThreadStatic]
		static bool suppressHasChanges;

		#endregion

		#region Parameters

		public override IDictionary<string, string> Parameters
		{
			get
			{
				if (parameters == null)
				{
					parameters = GetParametersFromReference(SL_Reference, ParseReferenceError.None);
					parameters.CollectionChanged += ParametersChanged;
				}

				return parameters;
			}
		}

		void ParametersChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			CheckCanUpdateKeyFields();

			base.SL_Reference = GenerateEventReferenceToFitInReferenceMaxLength(ReferenceFreeText, Parameters);

			ResetDisplayEventReference();
		}

		ObservableDictionary<string, string> parameters;

		#endregion

		#region ReferenceFreeText

		public override ZString ReferenceFreeText
		{
			get
			{
				if (!referenceFreeText.HasValue)
				{
					referenceFreeText = GetFreeTextFromReference(SL_Reference);
				}

				return referenceFreeText.Value;
			}

			set
			{
				CheckCanUpdateKeyFields();
				CheckMaximumLength(ReferenceFreeTextInfo, value);

				base.SL_Reference = GenerateEventReferenceToFitInReferenceMaxLength(value, Parameters);

				referenceFreeText = value;
				ReferenceFreeTextInfo.RefreshBinding();

				ResetDisplayEventReference();
			}
		}

		public ZString? referenceFreeText;

		public override ZString SL_ReferenceForBinding
		{
			get
			{
				if (Master is IStmALogOperationProvider stmALogOperationProvider)
				{
					return stmALogOperationProvider.GetHandledStringProperty(base.SL_ReferenceForBinding);
				}

				return base.SL_ReferenceForBinding;
			}
		}

		public static ZString GetReferenceForBinding(BusinessObject master, ZString reference)
		{
			if (master is IStmALogOperationProvider stmALogOperationProvider)
			{
				return stmALogOperationProvider.GetHandledStringProperty(GetReferenceForBinding(reference));
			}
			return GetReferenceForBinding(reference);
		}

		#endregion

		#region DisplayEventReference

		protected override ZString DisplayEventReferenceCore
		{
			get
			{
				if (Event == null)
				{
					return SL_Reference;
				}

				if (displayEventReference.HasValue)
				{
					return displayEventReference.Value;
				}

				var referenceFormat = Event.SE_IsRefernceFormatOverridden
					? Event.SE_OverriddenReferenceFormat
					: Event.SE_ReferenceFormat;

				var macro = referenceFormat.Quote().UnescapeXmlTags().ToString();
				displayEventReference = EventDataModel.GetDisplayEventReference(macro);

				if (Master is IStmALogOperationProvider stmALogOperationProvider)
				{
					displayEventReference = stmALogOperationProvider.GetHandledStringProperty(displayEventReference.Value);
				}

				return displayEventReference.Value;
			}
		}

		ZString? displayEventReference;

		public ZPropertyInfo DisplayEventReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.DisplayEventReference); }
		}

		void ResetDisplayEventReference()
		{
			displayEventReference = null;
			DisplayEventReferenceInfo.RefreshBinding();
		}

		IEventDataModel EventDataModel
		{
			get { return eventDataModel ?? (eventDataModel = new EventDataModel(this)); }
		}

		IEventDataModel eventDataModel;

		#endregion

		#region ErrorMessages

		public static class ErrorReportKeys
		{
			public static class Category
			{
				public static readonly string WorkflowGun = "WorkflowGun"; // ErrorReport category key
				public static readonly string DeletedStmALog = "DeletedLog"; // ErrorReport category key
			}

			public static class Flags
			{
				public static readonly string TrackDeletedStmALogs = "TrackDeletedStmALogs";
			}
		}

		public static class LogMessages
		{
			public static string DeletedStmALogError(ZGuid pk) => $@"Deleted StmALog [{pk}]"; // ErrorReport message

			public static string InvalidEventTimeError(ZGuid pk) => $@"Event time cannot be set to Empty or Invalid [{pk}]"; // ErrorReport message

			public static string DuplicatedKeyError(string key, string value, string oldValue) => string.Format(
				CultureInfo.InvariantCulture,
				"{0} {1}={2}|{1}={3}",
				DuplicatedKeyErrorPrefix(),
				key,
				value,
				oldValue
			); // Error message

			public static string InvalidEventTimeWithInfo(IStmALog stmALog) =>
				$"A StmALog with an empty EventTime is invalid and should not create a 'WTE' event.\nPK: {stmALog.PK}, Code: {stmALog.SL_SE_NKEvent}, Reference: {stmALog.SL_Reference}, Estimate: {stmALog.SL_IsEstimate}, EventTime: {stmALog.SL_EventTime}, Parent ID: {stmALog.ParentID}, SL_Parent: {stmALog.SL_Parent}";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Used for building error message")]
			public static string DuplicatedKeyErrorPrefix() => "The reference contains a parameter with a duplicated key. The duplicated pairs are:";

			public static string InvalidEventReferenceParametersError(List<string> invalidCodesForTest, string eventReference) => $@"
Event reference parameters must be from an enforced parameter list
If you wish to add a new parameter, add its description to public static class EventReferenceParameters in Shared\\CargoWise.EventReference
,its code to CargoWise.EventReference.Constants.EventReferenceParameters.Codes and then update
CargoWise.EventReference.Constants.EventReferenceParameters.Codes.All.
It is also possible to override certain codes when under a desired context: see GetParameterCodes(string eventCode) in ParameterLookupsProvider
The following parameter codes are not valid: {String.Join(",", invalidCodesForTest)}. The entered reference was: {eventReference}."; // Error message

			public static string SL_ReferenceArgumentError(ArgumentException ex, IStmALog log) => $"{ex.Message}, For StmALog: {log.Print()}";
		}

		#endregion

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			using (EnvProxy.Instance.SuspendBranchAccessError())
			{
				var currentBranch = EnvProxy.Instance.CurrentBranch;
				if (currentBranch != null)
				{
					SL_GB_NKBranch = currentBranch.Code;
				}
			}

			var currentDepartment = EnvProxy.Instance.CurrentDepartment;
			if (currentDepartment != null)
			{
				SL_GE_NKDepartment = currentDepartment.Code;
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			using (LockForUpdatingKeyFieldsForTesting())
			{
				base.FillWithValidTestDataCore(kind, propertyPath);
			}
		}
#endif

		#endregion

		#region Critical Date, Milestone and Trigger Update Management

		class KeyFieldSettingManager : IDisposable
		{
			internal KeyFieldSettingManager(StmALog parent, FireWorkflowMode mode)
			{
				this.parent = parent;
				parent.keyFieldSetCount++;
				this.mode = mode;
				if (mode != FireWorkflowMode.Suppress)
				{
					this.SL_SE_NKEvent = parent.SL_SE_NKEvent;
					this.SL_EventTime = parent.SL_EventTime;
					this.SL_Table = parent.SL_Table;
					this.SL_Parent = parent.SL_Parent;
					this.SL_IsEstimate = parent.SL_IsEstimate;
					this.SL_IsCancelled = parent.SL_IsCancelled;
					this.SL_Reference = parent.SL_Reference;
				}
			}
			readonly StmALog parent;
			readonly FireWorkflowMode mode;

			readonly ZString SL_SE_NKEvent;
			readonly ZDateTime SL_EventTime;
			readonly ZString SL_Table;
			readonly ZGuid SL_Parent;
			readonly ZBool SL_IsEstimate;
			readonly ZBool SL_IsCancelled;
			readonly ZString SL_Reference;

			ZBool DeferFiringWorkflow
			{
				get
				{
					return parent.SL_FireWorkflow;
				}
			}

			void IDisposable.Dispose()
			{
				parent.keyFieldSetCount--;

				if (parent.keyFieldSetCount == 0 && mode != FireWorkflowMode.Suppress && !DeferFiringWorkflow && HaveKeyFieldsChangedAfterConstruction())
				{
					parent.FireWorkflow(mode);
				}
			}

			bool HaveKeyFieldsChangedAfterConstruction()
			{
				return this.SL_SE_NKEvent != parent.SL_SE_NKEvent
					|| this.SL_EventTime != parent.SL_EventTime
					|| this.SL_Table != parent.SL_Table
					|| this.SL_Parent != parent.SL_Parent
					|| this.SL_IsEstimate != parent.SL_IsEstimate
					|| this.SL_IsCancelled != parent.SL_IsCancelled
					|| this.SL_Reference != parent.SL_Reference;
			}
		}

		[Conditional("DEBUG")]
		void CheckCanUpdateKeyFields()
		{
			if (keyFieldSetCount == 0)
			{
				throw new InvalidOperationException("Must wrap expressions setting key fields with 'using (log.LockForUpdatingKeyFieldsForTesting())' so that Event Management knows when you're finished updating and can fire Milestones, Triggers and Date Updates.");
			}
		}
		int keyFieldSetCount;

		void IExternalWorkflowTrigger.FireWorkflow()
		{
			FireWorkflow(FireWorkflowMode.UpdateAll);
		}

		void FireWorkflow(FireWorkflowMode mode) => new WorkflowGun(this, mode).TriggerWorkflow(Master);

		IDisposable IUpdateFieldsLock.LockForUpdatingKeyFields()
		{
			return LockForUpdatingKeyFields();
		}

		IDisposable IUpdateFieldsLockReachAround.LockForUpdatingKeyFields(bool shouldNotifyOnRelease)
		{
			return LockForUpdatingKeyFields(shouldNotifyOnRelease);
		}

#if DEBUG
		public IDisposable LockForUpdatingKeyFieldsForTesting()
		{
			return LockForUpdatingKeyFields();
		}
#endif

		internal IDisposable LockForUpdatingKeyFields(bool shouldNotifyOnRelease)
		{
			return LockForUpdatingKeyFields(shouldNotifyOnRelease ? FireWorkflowMode.UpdateAll : FireWorkflowMode.Suppress);
		}

		internal IDisposable LockForUpdatingKeyFields(FireWorkflowMode mode = FireWorkflowMode.UpdateAll)
		{
			return new KeyFieldSettingManager(this, mode);
		}

#if DEBUG
		internal
#endif
 IProcessTaskHandler GetProcessTaskHandler() => WorkflowGun.GetProcessTaskHandler(this, Master);

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static ZString GenerateEventReference(string referenceFreeText, IEnumerable<KeyValuePair<string, string>> parameters)
		{
			return EventLogReferenceBuilder.GenerateEventReference(referenceFreeText, parameters);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public static ZString GenerateEventReferenceToFitInReferenceMaxLength(ZString referenceFreeText, IEnumerable<KeyValuePair<string, string>> parametersInOrderOfPrecedence)
		{
			return EventLogReferenceBuilder.GenerateEventReferenceToFitInReferenceMaxLength(referenceFreeText, parametersInOrderOfPrecedence);
		}

		public static ZString GetFreeTextFromReference(string eventReference) => EventLogReferenceBuilder.GetFreeTextFromText(eventReference, "|", "=");

		public static string SerializeParameterValue(IZType value)
		{
			if (value == null)
			{
				return null;
			}

			if (value is ZDateTime)
			{
				return ((ZDateTime)value).ToISO8601String();
			}

			return value.ToString();
		}

		public static object ParseParameterValue(KeyValuePair<string, string> parameter, Type targetType)
		{
			if (targetType == typeof(ZDateTime))
			{
				DateTime dateTime;

				if (DateTime.TryParse(parameter.Value, out dateTime))
				{
					return new ZDateTime(dateTime);
				}

				return null;
			}

			if (targetType == typeof(ZInt))
			{
				int number;

				if (Int32.TryParse(parameter.Value, out number))
				{
					return new ZInt(number);
				}

				return null;
			}

			return (ZString)parameter.Value;
		}

		/// <summary>
		///     Retrieves parameters from the <paramref name="eventReference"/>.
		/// </summary>
		/// <param name="eventReference">
		///     The event reference with/without parameters in a format: {oprional free text}|ParamKey=ParamValue|ParamKey=ParamValue.
		///
		///		Here are some examples of valid event references:
		///		
		///		|LOC=UAIEV|FAC=CTO
		///		some free text
		///		some free text|LOC=UAIEV
		/// </param>
		/// <exception cref="ArgumentException">
		///     Thrown when <paramref name="eventReference"/> includes parameters with the same key.
		/// </exception>
		public static ObservableDictionary<string, string> GetParametersFromReference(string eventReference, ParseReferenceError throwOnDuplicates = ParseReferenceError.None)
		{
			return GetParametersFromText(eventReference, throwOnDuplicates: throwOnDuplicates);
		}

		internal static ObservableDictionary<string, string> GetParametersFromText(string eventReference, ParseReferenceError throwOnDuplicates = ParseReferenceError.None)
		{
			if (eventReference == null)
			{
				return [];
			}

			var result = EventReferenceProcessor.ParseAndValidateEventReference(eventReference, throwOnDuplicates);

			#if DEBUG
			if (result.InvalidCodesForTest.Count > 0)
			{
				ErrorReporter.ReportOnce(LogMessages.InvalidEventReferenceParametersError(result.InvalidCodesForTest, eventReference));
			}
			#endif
			return result.Parameters;
		}

		public enum ParseReferenceError
		{
			None,
			Exception,
		}

		public void UpdateReference(string newReference)
		{
			using (LockForUpdatingKeyFields(true))
			{
				SL_Reference = newReference;
			}
		}

		internal void CancelWithoutChangingAnything()
		{
			Cancel(FireWorkflowMode.Suppress);
		}

		public override void Cancel()
		{
			Cancel(FireWorkflowMode.UpdateAll);
		}

		void IStmALogInternals.CancelWithoutNotifications()
		{
			Cancel(FireWorkflowMode.Suppress);
		}

		internal void Cancel(FireWorkflowMode mode)
		{
			using (LockForUpdatingKeyFields(mode))
			{
				base.Cancel();
			}
		}

		public override void Cancel(ZDateTime eventTime)
		{
			using (LockForUpdatingKeyFields(true))
			{
				base.Cancel(eventTime);
			}
		}

		public override void Reactivate()
		{
			using (LockForUpdatingKeyFields(true))
			{
				base.Reactivate();
			}
		}

		public override void Reactivate(ZDateTime eventTime)
		{
			using (LockForUpdatingKeyFields(true))
			{
				base.Reactivate(eventTime);
			}
		}

		public MessageValueObject RelatedEDIMessage => relatedMessage ?? (relatedMessage = GetRelatedEDIMessage(this));
		MessageValueObject relatedMessage;

		public static MessageValueObject GetRelatedEDIMessage(IStmALog log)
		{
			var relatedMessagePivot = log.Factory.LoadTop1<IGenPivot>(GetMessagePivotQuery(log));
			if (relatedMessagePivot == null)
			{
				return null;
			}

			var message = log.Factory.Load<IEDIMessage>(relatedMessagePivot.XX_Relation2ID);
			if (message == null)
			{
				return null;
			}

			return new MessageValueObject(message);
		}

		static ZQuery GetMessagePivotQuery(IStmALog log)
		{
			var query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_RelationType, Enterprise.Core.Constants.GenPivotTypes.XmlEdiMessage);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, log.PK);
			return query;
		}

		KeyDataPairCollection sourceInfoItems;
		public override IKeyDataPairCollection SourceInfoItems
		{
			get { return sourceInfoItems ?? (sourceInfoItems = MessageSourceItemRetriever.GetSourceItems(Factory, RelatedEDIMessage)); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new StmALogFetchStrategy(this);
		}

		#region Types

		class StmALogFetchStrategy : EnterpriseBusinessObjectFetchStrategy
		{
			public StmALogFetchStrategy(StmALog log)
				: base(log)
			{
			}

			StmALog log => BusinessObject as StmALog;

			protected override void FetchForViewCore(TableColumn[] columns)
			{
				base.FetchForViewCore(columns);

				foreach (var column in columns)
				{
					if (string.IsNullOrEmpty(column.TableName))
					{
						if (column.ColumnName == nameof(SL_UserNameAndInitials))
						{
							Factory.AddFetchHint(ObjectFactory.GetType<IGlbStaff>(), GlbStaffSchema.GS_Code, log.SL_GS_NKUser);
						}
					}
				}
			}
		}

		#endregion

		ZString IEventUserContextSource.StaffCode => SL_GS_NKUser;

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			if (!this.IsInDatabase || this.HasChanges || this.HasNotifications())
			{
				base.RunPreSaveValidationCore();
			}
		}

		#endregion

		public bool CheckReferenceEquals(string expectedReference)
		{
			if (GetFreeTextFromReference(expectedReference ?? "") != GetFreeTextFromReference(SL_Reference))
			{
				return false;
			}
			var expectedParameters = GetParametersFromReference(expectedReference ?? "", ParseReferenceError.Exception);
			var actualParameters = GetParametersFromReference(SL_Reference, ParseReferenceError.Exception);
			if (!expectedParameters.Equals(actualParameters))
			{
				return false;
			}
			return true;
		}
	}

	static class Extensions
	{
		public static ZString InsertSpacesIntoPascalCasing(this ZString sourceKey)
		{
			var characters = new List<char>(sourceKey.ToString());

			for (int index = 1; index < characters.Count; index++)
			{
				if (!char.IsLower(characters[index]) && !char.IsWhiteSpace(characters[index]))
				{
					if (char.IsLower(characters[index - 1]) || index < (characters.Count - 1) && char.IsLower(characters[index + 1]))
					{
						characters.Insert(index, StmALog.SpaceDelimiter);
						index++;
					}
				}
			}

			return new string(characters.ToArray());
		}

		public static ZString Quote(this ZString st)
		{
			var result = st.StartsWith("\"")
				? st
				: ZString.Format("\"{0}\"", st);

			return result;
		}

		public static ZString UnescapeXmlTags(this ZString st)
		{
			var resultString = st;

			resultString = resultString.Replace("&gt;", ">");
			resultString = resultString.Replace("&lt;", "<");

			return resultString;
		}

		public static ZString IfEmptyStringUse(this object sourceString, ZString fallbackString)
		{
			var result = string.IsNullOrEmpty(sourceString as string)
				? fallbackString
				: new ZString(sourceString);

			return result;
		}

		public static KeyValuePair<string, string> AsKeyFor(this string key, string value)
		{
			return new KeyValuePair<string, string>(key, value);
		}
	}
}
