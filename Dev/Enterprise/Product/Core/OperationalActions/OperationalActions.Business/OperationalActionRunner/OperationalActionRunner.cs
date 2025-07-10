using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Services.OperationalActions.Business.OperationalActionRunnerLookups;

namespace Enterprise.Services.OperationalActions.Business
{
	public class OperationalActionRunner : AutoOperationalActionRunner, IObsoleteValidation
	{
		public const string SettingCacheName = "OperationalActionRunnerSettings";

		public OperationalActionRunner(OperationalAction action, Type bizObjType, ITargetRecordSelection selection)
			: this(action, bizObjType, selection.GetSelectedRecords())
		{
			Selection = selection;
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public OperationalActionRunner(OperationalAction action, Type bizObjType, ISelectedRecords selectedRecords)
			: base(action.Factory)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (bizObjType == null)
			{
				throw new ArgumentNullException(nameof(bizObjType));
			}

			if (selectedRecords == null)
			{
				throw new ArgumentNullException(nameof(selectedRecords));
			}

			if (action.Context == null)
			{
				throw new ArgumentException("action has no context");
			}

			if (!typeof(BusinessObject).IsAssignableFrom(bizObjType))
			{
				throw new ArgumentOutOfRangeException(nameof(bizObjType), bizObjType, "Not a business object type");
			}

			this.action = action;
			this.targets = selectedRecords.PrimaryKeys;
			AutoExportOfNonSelectedObjects = selectedRecords.AutoSelectedAllKeys;
			this.bizObjType = bizObjType;
		}

		public IOverrideSelectionCount OverrideSelectionCount { get; set; }

		#region Properties

		public bool IsFilterRecordsSelection => Selection is IFilterRecordsSelection;

		public override ZInt SelectedGridRowCount => OverrideSelectionCount?.SelectionCount ?? targets.Length;

		public int FilterRowCount
		{
			get
			{
				if (OverrideSelectionCount != null)
				{
					filterRowCount = OverrideSelectionCount.SelectionCount;
				}
				else if (filterRowCount == null)
				{
					filterRowCount = Selection?.FilterRowCount ?? 0;
				}
				return (int)filterRowCount;
			}
		}
		int? filterRowCount;

		public override ZString SelectedGridRowNoun => GetNoun(SelectedGridRowCount);
		public ZString FilterRowNoun => GetNoun(FilterRowCount);

		ZString GetNoun(int count)
		{
			return count == 1 ? action.Context.Supporter.SingularElementNoun : action.Context.Supporter.PluralElementNoun;
		}

		[List("Lookups.Printer_List")]
		public override ZGuid Printer
		{
			get { return Lookups.BulkDeliveryMethod_List.UsesPrinter(BulkDeliveryMethod) ? base.Printer : ZGuid.Empty; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.Printer = value; }
		}

		protected override bool Printer_ReadOnly
		{
			get { return !Lookups.BulkDeliveryMethod_List.UsesPrinter(BulkDeliveryMethod); }
		}

		[List("Lookups.BulkDeliveryMethod_List")]
		public override ZString BulkDeliveryMethod
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.BulkDeliveryMethod; }
			set
			{
				base.BulkDeliveryMethod = value;

				if (!Lookups.BulkDeliveryMethod_List.AllowCoverNote(value))
				{
					IncludeCoverNote = false;
				}

				if (!Lookups.BulkDeliveryMethod_List.AllowDeliverDocumentsInOneEmail(value))
				{
					DeliverDocumentsInOneEmail = false;
					AttachmentOptions = ZString.Empty;
				}
			}
		}

		[List("Languages")]
		[BusinessObjectMaxLengthTestExclude]
		[MaxLength(3)]
		public ZString DocumentPrintLanguage
		{
			get
			{
				if (!languageHasBeenSet)
				{
					return (Res.CurrentLanguage == Res.DefaultLanguage) ? (ZString)DataRegistry.Instance.EnglishSpelling : (ZString)Res.CurrentLanguage;
				}

				return documentPrintLanguage;
			}
			set
			{
				if (value == Res.DefaultLanguage)
				{
					value = DataRegistry.Instance.EnglishSpelling;
				}
				if ((documentPrintLanguage != value || !languageHasBeenSet) && Languages.ContainsCode(value))
				{
					languageHasBeenSet = true;
					documentPrintLanguage = value;
					UpdateLanguageWarning();
				}
				DocumentPrintLanguageInfo.RefreshBinding();
			}
		}
		ZString documentPrintLanguage;
		AvailableDocBuilderLanguageList languages;
		bool languageHasBeenSet;

		public ZPropertyInfo DocumentPrintLanguageInfo
		{
			get { return GetZPropertyInfo(nameof(DocumentPrintLanguage)); }
		}

		void UpdateLanguageWarning()
		{
			DocumentPrintLanguageInfo.ClearAllNotifications();
			if (!Res.IsEnglish(documentPrintLanguage))
			{
				LanguageLicenceCheckpoint licence;
				Env.Licence.LanguagePackLookup.TryGetValue(documentPrintLanguage, out licence);
				if (licence != null)
				{
					DocumentPrintLanguageInfo.AddWarning(Res.GetString("783eb73d-934b-4036-9c61-f897923b72dc", "You have selected to issue this document translated into {0}.", Languages.GetDescriptionFromCode(documentPrintLanguage)));
				}
			}
		}

		public AvailableDocBuilderLanguageList Languages
		{
			get { return languages ?? (languages = new AvailableDocBuilderLanguageList(Factory)); }
		}

		protected override bool CoverNoteText_ReadOnly
		{
			get { return !IncludeCoverNote; }
		}

		public override ZBool IncludeCoverNote
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.IncludeCoverNote; }
			set
			{
				base.IncludeCoverNote = value;

				if (!value)
				{
					CoverNoteText = "";
				}
			}
		}

		protected override bool IncludeCoverNote_ReadOnly
		{
			get { return !Lookups.BulkDeliveryMethod_List.AllowCoverNote(BulkDeliveryMethod); }
		}

		public override ZBool DeliverDocumentsInOneEmail
		{
			get => base.DeliverDocumentsInOneEmail;
			set
			{
				base.DeliverDocumentsInOneEmail = value;
				if (value)
				{
					AttachmentOptions = AttachmentOptionsCodes.SingleAttachment;
				}
				else
				{
					AttachmentOptions = ZString.Empty;
					OverrideRecipientEmail = false;
				}
			}
		}

		protected override bool DeliverDocumentsInOneEmail_ReadOnly => !Lookups.BulkDeliveryMethod_List.AllowDeliverDocumentsInOneEmail(BulkDeliveryMethod) ||
			Action.DocumentPivots.OfType<OperationalActionDocumentPivot>().Select(d => d.Outward?.SU_ContactType).Distinct().ToArray().Length != 1;

		[List("Lookups.AttachmentOptions_List")]
		public override ZString AttachmentOptions { get => base.AttachmentOptions; set => base.AttachmentOptions = value; }

		protected override bool AttachmentOptions_ReadOnly => DeliverDocumentsInOneEmail_ReadOnly || !DeliverDocumentsInOneEmail;

		public ZBool OverrideRecipientEmailEnabled
		{
			get
			{
				if (DeliverDocumentsInOneEmail)
				{
					if (!typeof(ISupportOverrideOperationalActionRecipientEmail).IsAssignableFrom(bizObjType))
					{
						return false;
					}

					var query = new ZQuery(BusinessObjectFactory.GetTableSchemaFromType(bizObjType).PK, SQLComparisonOperator.Equal, targets);
					var overrideRecipientEmailDecider = Factory.LoadTop1(bizObjType, query) as ISupportOverrideOperationalActionRecipientEmail;

					return overrideRecipientEmailDecider?.IsOverrideRecipientEmailEnabled(bizObjType, targets) ?? false;
				}
				else
				{
					return false;
				}
			}
		}

		public override ZBool OverrideRecipientEmail
		{
			get => base.OverrideRecipientEmail;
			set
			{
				base.OverrideRecipientEmail = value;
				if (!value)
				{
					Validation.ValidateRecipientEmail();
				}
			}
		}

		protected override bool RecipientEmail_ReadOnly => !OverrideRecipientEmail;

		#endregion

		#region Related BusinessObjects

		public OperationalAction Action
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return action; }
		}
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OperationalAction action;

		public ZGuid[] GetSelectedPrimaryKeys()
		{
			return targets;
		}

		public RunnerFieldCollection Fields
		{
			get
			{
				if (fields == null)
				{
					fields = NewFieldsCollection();
					RegisterEditableChildObject(fields);
				}
				return fields;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1099:ResGetStringDefaultTextMustBeStringLiteral", Justification = "Baseline")]
		RunnerFieldCollection NewFieldsCollection()
		{
			RunnerFieldCollection fields = new RunnerFieldCollection(Factory);
			action.FieldDescriptors.SortByOrder();

			foreach (OperationalActionFieldDescriptor descriptor in action.FieldDescriptors)
			{
				OperationalActionFieldSupporter supporter = action.Context.FieldSupporters[descriptor.FieldName];

				if (supporter == null)
				{
					if (!Action.SU_IsSystemDefined)
					{
						string messageFormat = Res.GetString(
							"OperationalActionRunner|InvalidField",
							"The field '{0}' does not exist, this field may have been removed or renamed.\r\n" +
							"To resolve this you will need to edit this action and correct the fields name.",
							descriptor.FieldName);

						fields.Add(new RunnerErrorField(Factory, descriptor, messageFormat));
					}
					else
					{
						string key = "OperationActionRunner.NewFieldsCollection:" + Action.SU_MenuName;
						string message = string.Format((NoResString)"Undefined field.\nField Name: {0}\n", descriptor.FieldName) + GetDebugInfo();

						ErrorReporter.ReportOnce(key, message);
					}
				}
				else if (supporter.ReadOnly)
				{
					if (!Action.SU_IsSystemDefined)
					{
						string messageFormat = Res.GetString(
							"OperationalActionRunner|ReadOnlyField",
							"The field '{0}' cannot be bulk updated.",
							descriptor.FieldName);

						fields.Add(new RunnerErrorField(Factory, descriptor, messageFormat));
					}
					else
					{
						string key = "OperationalActionRunner.NewFieldsCollection:" + Action.SU_MenuName;
						string message = string.Format((NoResString)"ReadOnly field.\nField Name: {0}\n", descriptor.FieldName) + GetDebugInfo();
					}
				}
				else
				{
					fields.Add(supporter.NewRunnerField(Factory, descriptor));
				}
			}

			return fields;
		}
		RunnerFieldCollection fields;

		public OperationalActionMethodApplicatorCollection MethodApplicators
		{
			get
			{
				if (methodApplicators == null)
				{
					methodApplicators = new OperationalActionMethodApplicatorCollection(this);
					methodApplicators.Load();
					RegisterEditableChildObject(methodApplicators);
				}
				return methodApplicators;
			}
		}
		OperationalActionMethodApplicatorCollection methodApplicators;

		#endregion

		#region Run

		public void Run(IOperationalActionLog log, BusinessObjectFactory factoryForChanges, OperationalActionMethodUIMode uiMode = OperationalActionMethodUIMode.All)
		{
			if (log == null)
			{
				throw new ArgumentNullException(nameof(log));
			}

			if (factoryForChanges == null)
			{
				throw new ArgumentNullException(nameof(factoryForChanges));
			}

			log.SetMasterProgressMax(MethodApplicators.Count);
			InitialiseBeforeAllBatchesRun(uiMode);
			Run(log, factoryForChanges, uiMode, targets);
		}

		void Run(IOperationalActionLog log, BusinessObjectFactory factoryForChanges, OperationalActionMethodUIMode uiMode, ZGuid[] pks)
		{
			try
			{
				InitialiseBeforeIndividiualBatchRun(uiMode);
				RunCore(log, factoryForChanges, uiMode, pks);
			}
			catch (ZCannotSaveException ex)
			{
				LogSaveExceptionErrorMessage(ex, log);
			}
			catch (ZSaveException ex)
			{
				LogSaveExceptionErrorMessage(ex, log);
			}
			catch (Exception ex)
			{
				if (ex is TargetInvocationException exception && exception.InnerException != null)
				{
					LogSaveExceptionErrorMessage(exception.InnerException, log);
				}
				else
				{
					LogSaveExceptionErrorMessage(ex, log);
				}

				var key = $"RunActionException_{bizObjType?.Name}_{ex.GetType().Name}";
				ErrorReporter.ReportOnce(key, GetDebugInfo(), ex);
			}
		}

		void InitialiseBeforeAllBatchesRun(OperationalActionMethodUIMode uiMode) => Applicators(uiMode).ForEach(a => a.InitialiseBeforeAllBatchesRun());

		void LogSaveExceptionErrorMessage(Exception ex, IOperationalActionLog log)
		{
			log.NotifyFormat(OperationalActionLogErrorLevel.Error, Res.GetString("OperationalActionRunner|SaveException", "An {0} occurred while saving: {1}", ex.GetType().ToString(), ex.Message));
		}

		public void BatchRun(IOperationalActionLog log, Action<IOperationalActionLog, BusinessObjectFactory> saveAndLogAction, OperationalActionMethodUIMode uiMode = OperationalActionMethodUIMode.All)
		{
			if (log == null)
			{
				throw new ArgumentNullException(nameof(log));
			}

			var filterRecordsSelection = Selection as IFilterRecordsSelection;
			var oldRowCount = FilterRowCount;
			filterRowCount = null;

			if (RunOnAllMatchingRecords && filterRecordsSelection != null)
			{
				var batches = (FilterRowCount / Env.Registry.OperationalActionsRecordBatchSize);
				if (FilterRowCount % Env.Registry.OperationalActionsRecordBatchSize != 0)
				{
					batches++;
				}
				var max = MethodApplicators.Count * batches;
				log.SetMasterProgressMax(max);
				int batch = 0;
				InitialiseBeforeAllBatchesRun(uiMode);
				foreach (var records in filterRecordsSelection.GetAllFilterRecords(bizObjType))
				{
					batch++;
					var factoryForChanges = new BusinessObjectFactory { NameForDebugging = "OperationalActions factory for changes", RefreshEnabled = false };
					using (factoryForChanges.AddDisposableService())
					{
						log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("OperationalActionRunner|BatchStart", "Starting Batch {0} of {1}: ...", batch, batches));
						Run(log, factoryForChanges, uiMode, records.PrimaryKeys);
						saveAndLogAction(log, factoryForChanges);
					}
				}
			}

			if (FilterRowCount != oldRowCount)
			{
				var errorToReport = Res.GetString("2ec8cc94-b917-490c-af5d-8f86d3544587",
				"There were {0} item(s) selected but {1} item(s) were found to process.", oldRowCount, FilterRowCount);
				log.Notify(OperationalActionLogErrorLevel.Warning, errorToReport);
			}
		}

		public void SummaryLog(IOperationalActionLog log)
		{
			if (log == null)
			{
				throw new ArgumentNullException(nameof(log));
			}

			try
			{
				foreach (OperationalActionMethodApplicator applicator in MethodApplicators)
				{
					if (applicator.SupportsSummary)
					{
						log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("OperationalActionRunner|SectionSummary", "{0} Summary:", applicator.Name));
						applicator.SummaryLog(log);
					}
				}
			}
			catch (Exception ex)
			{
				throw new RunActionException(GetDebugInfo(), ex);
			}
		}

		void InitialiseBeforeIndividiualBatchRun(OperationalActionMethodUIMode uiMode)
		{
			foreach (var applicator in Applicators(uiMode))
			{
				applicator.InitialiseBeforeIndividiualBatchRun();
			}
		}

		protected virtual void RunCore(IOperationalActionLog log, BusinessObjectFactory factoryForChanges, OperationalActionMethodUIMode uiMode, ZGuid[] pks)
		{
			BusinessObject[] bizObjs = null;

			foreach (var applicator in Applicators(uiMode))
			{
				log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("OperationalActionRunner|SectionStart", "Starting Section: {0} ...", applicator.Name));
				var delayedApplicator = applicator as IOperationalActionMethodApplicatorDelayingBizoLoading;
				if (delayedApplicator != null)
				{
					delayedApplicator.Apply(log, pks, bizObjType);
				}
				else
				{
					if (bizObjs == null)
					{
						bizObjs = LoadBusinessObjectsInOrder(factoryForChanges, pks);
					}
					applicator.Apply(log, bizObjs);
				}

				if (log.HighestErrorLevelEncountered > OperationalActionLogErrorLevel.Warning)
				{
					break;
				}

				log.BumpMasterProgress();
			}
		}

		#endregion

		#region Strategies

		public OperationalActionRunnerLookups Lookups
		{
			get { return lookups ?? (lookups = new OperationalActionRunnerLookups(this)); }
		}
		OperationalActionRunnerLookups lookups;

		#endregion

		#region Extract Unique Security/Licence Checkpoints

		public SecurityCheckpoint[] ExtractUniqueSecurityCheckpoints()
		{
			return ExtractUniqueCheckpoints((m) => m.GetRequiredSecurityCheckpoints());
		}
		public LicenceCheckpoint[] ExtractUniqueLicenceCheckpoints()
		{
			return ExtractUniqueCheckpoints((m) => m.GetRequiredLicenceCheckpoints());
		}

		T[] ExtractUniqueCheckpoints<T>(Converter<OperationalActionMethod, T[]> checkpointsGetter)
		{
			Dictionary<T, bool> requiredCheckpoints = new Dictionary<T, bool>();
			foreach (OperationalActionMethod method in MethodApplicators.GetMethods())
			{
				T[] checkpoints = checkpointsGetter(method);

				if (checkpoints != null)
				{
					foreach (T checkpoint in checkpoints)
					{
						if (checkpoint != null)
						{
							requiredCheckpoints[checkpoint] = true;
						}
					}
				}
			}

			T[] result = new T[requiredCheckpoints.Count];
			requiredCheckpoints.Keys.CopyTo(result, 0);
			return result;
		}

		#endregion

		#region Load / Save Settings

		public void LoadSettings()
		{
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				string value = Env.Registry.GetFilterCriteria(SettingCacheName);

				if (!string.IsNullOrEmpty(value))
				{
					using (StringReader reader = new StringReader(value))
					using (XmlTextReader xmlReader = new XmlTextReader(reader))
					{
						xmlReader.WhitespaceHandling = WhitespaceHandling.None;
						xmlReader.ReadToFollowing(SettingCacheName);
						((IXmlSerializable)this).ReadXml(xmlReader);
					}
				}
			}
		}

		public void SaveSettings()
		{
			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.WriteStartDocument();
				xmlWriter.WriteStartElement(SettingCacheName);
				((IXmlSerializable)this).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();
				xmlWriter.Flush();

				Env.Registry.SetFilterCriteria(SettingCacheName, writer.ToString());
			}
		}

		#endregion

		#region GetDebugInfo

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Diagnostic Information")]
		public string GetDebugInfo()
		{
			const string format =
				"Action Name: {0}\n" +
				"Business Context: {1}\n" +
				"Is System Defined: {2}\n" +
				"Action Supporter Type: {3}\n" +
				"BusinessObject Type: {4}\n" +
				"Fields:\n" +
				"{5}\n";

			return string.Format(format,
				Action.SU_MenuName,
				Action.SU_BusinessContext,
				Action.SU_IsSystemDefined,
				Action.Context.Supporter.GetType(),
				bizObjType.Name,
				string.Join("\r\n", Fields.Select(f => $"\tFieldCaption:{f.Descriptor?.FieldCaption}, ShouldApply:{f.ShouldApply}, " + (f.ShouldApply ? $"Field:{(f as IOperationalActionFieldValuePair).Field.Field}" : "")))
				);
		}

		#endregion

		#region Implementation

		protected override ZString HumanReadableNameCore
		{
			get { return action.SU_MenuNameMultilingual; }
		}

		public ITargetRecordSelection Selection { get; }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BulkDeliveryMethod = AutoBulkDeliveryMethod.CodeText;
			LoadSettings();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (!HasErrors)
			{
				SaveSettings();
			}
		}

		BusinessObject[] LoadBusinessObjectsInOrder(BusinessObjectFactory factoryForChanges, ZGuid[] pks)
		{
			ITableSchema schema = BusinessObjectFactory.GetTableSchemaFromType(bizObjType);
			if (!typeof(NonPersistentBusinessObject).IsAssignableFrom(bizObjType))
			{
				factoryForChanges.AddFetchHint(bizObjType, new ZQuery(schema.PK, pks));
			}

			BusinessObject[] bizObjs = (BusinessObject[])Array.CreateInstance(bizObjType, pks.Length);

			int write = 0;

			for (int i = 0; i < pks.Length; i++)
			{
				BusinessObject obj = factoryForChanges.Load(bizObjType, pks[i]);

				if (obj != null)
				{
					obj.IsTopLevel = true; //to enable autoAdminBusinessObjectLogger
					bizObjs[write] = obj;
					write++;
				}
			}

			if (write < bizObjs.Length)
			{
				Array.Resize(ref bizObjs, write);
			}

			return bizObjs;
		}

		IEnumerable<OperationalActionMethodApplicator> Applicators(OperationalActionMethodUIMode uiMode) => MethodApplicators.GetApplicators(uiMode);

		#endregion

		readonly ZGuid[] targets;
		readonly Type bizObjType;
	}
}
