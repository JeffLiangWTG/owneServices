using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.Business.EventManagement.Interfaces;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;

namespace Enterprise.ZArchitecture.Business
{
	public interface IStmNoteInternals
	{
		bool IsRelatedInParentView { get; }
		bool IsNoteRead { get; set; }
	}

	[CodeProperty(StmNote.Schema.ST_Description), DescriptionProperty(StmNote.Schema.ST_Description)]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class StmNote : AutoStmNote, IStmNoteInternals, ICustomTextTemplateContext, ICustomTextTemplateFallbackContext, ICrudEventProvider, IForceLogsOnParentForNewWithNoHasChanges
	{
		#region Temporary Type Decider for the Order Update History note

		/// to be removed in favour or collection support for loading the existing type from the cache - see Geoff

		public static readonly TypeDecider TypeDecider = new StmNoteTypeDecider();

		[ImmutableObject(true)]
		class StmNoteTypeDecider : TypeDecider
		{
			public override Type GetTypeForBinding()
			{
				return typeof(StmNote);
			}

			public override Type GetTypeForNew()
			{
				return typeof(StmNote);
			}

			public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
			{
				Type result;

				if ((string)row[StmNoteSchema.Constants.ST_Description] == PredefinedNoteTypes.Instance.OrderUpdateHistory.Code &&
					!(new ZBool(row[StmNoteSchema.Constants.ST_IsCustomDescription])))
				{
					result = ObjectFactory.GetType<Enterprise.Integration.Freight.IOrderUpdateHistoryStmNote>();
				}
				else
				{
					result = typeof(StmNote);
				}

				return result;
			}
		}

		#endregion

		#region Schema

		public new abstract class Schema : AutoStmNote.Schema
		{
			public const string ST_CreatedByUserInitials = "ST_CreatedByUserInitials";
			public const string ST_CreatedByUserName = "ST_CreatedByUserName";
			public const string ST_CreatedDateUtc = "ST_CreatedDateUtc";
			public const string ST_IsPopupLog = "ST_IsPopupLog";
			public const string ST_IsTextOnly = "ST_IsTextOnly";
			public const string ST_LastModifiedByUserName = "ST_LastModifiedByUserName";
			public const string ST_LastModifiedDate = "ST_LastModifiedDate";
			public const string ST_NoteDataAsText = "ST_NoteDataAsText";
			public const string ST_NoteDataOrTextForBinding = "ST_NoteDataOrTextForBinding";
			public const string ST_NoteSource = "ST_NoteSource";

			public const string ST_NoteDataAsTextConcatenatedAndTrimmed = "ST_NoteDataAsTextConcatenatedAndTrimmed";

			public const string ST_NoteContextModule = "SUBSTRING(ST_NoteContext, 1, 1)";
			public const string ST_NoteContextDirection = "SUBSTRING(ST_NoteContext, 2, 1)";
			public const string ST_NoteContextFreightMode = "SUBSTRING(ST_NoteContext, 3, 1)";
		}

		#endregion

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public StmNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			UpdateIsPopupLog();
		}

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public ZString GetUnSerializeNoteText()
		{
			return (ST_IsSerializable) ? base.ST_NoteText : ST_NoteText;
		}

		public bool IsInternalOrClientVisibleOrAgentVisible
		{
			get { return ST_NoteType == nameof(StmNoteVisibility.PUB) || ST_NoteType == nameof(StmNoteVisibility.INT) || ST_NoteType == nameof(StmNoteVisibility.AGV); }
		}

		internal bool HasMaster
		{
			get { return Master != null && !Master.IsDeleted; }
		}

		#region CopyPersistentValuesFrom & Clone

		public void CopyPersistentValuesFrom(StmNote note)
		{
			CopyPersistentValuesFrom((EnterpriseBusinessObject)note);
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (StmNote)base.CloneInternal(args);
			clone.ST_Description = this.ST_DescriptionInDatabase;
			return clone;
		}

		#endregion

		#region On Saving

		public override void OnSaving()
		{
			base.OnSaving();
			CreateCrudStmALog();
			UpdateNoteReadOnlyAfterAdd();
			CheckSTTableFilled();
			CheckNotRichTextAsDOCNote();
		}

		protected override void OnSavingForDelete()
		{
			base.OnSavingForDelete();
			CreateCrudStmALog();
		}

		public void CreateCrudStmALog()
		{
			if (Master is IWorkflowProviderCore && Master is IStmALogProvider logProvider)
			{
				using (((IBusinessObjectInternals)this).SuppressReportRowDeletedError())
				{
					var parameters = new Dictionary<string, string> { { "DES", ST_Description } };
					new CrudEventWrapper<StmNote>(this, logProvider).CreateLog(_ => GetNoteEventReference(), parameters);
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearUnusedNoteTextOrData();
			RefreshBinding();
			base.RunPreSaveValidationCore();
		}

		[Conditional("DEBUG")]
		void CheckSTTableFilled()
		{
			if (string.IsNullOrWhiteSpace(ST_Table) && ((INeedRow)this).Row.RowState == DataRowState.Added)
			{
				ErrorReporter.ReportOnce("EmptySTTableInNewStmNote", GetErrorReportMessage("All notes should be saved with a non-blank ST_Table."));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message for logging")]
		void CheckNotRichTextAsDOCNote()
		{
			if (IsRichTextAsDOCNote)
			{
				Globals.Message.ShowDeveloperErrorOnce("RichTextNoteMasqueradingAsDocumentNote", GetErrorReportMessage("An StmNote is being saved that would be confused for a DocumentNote (DOC and no description) but has a rich text payload."), "Rich Text Note Masquerading as Document Note");
				this.Delete();
			}
		}

		public bool IsRichTextAsDOCNote
		{
			get { return (ST_NoteType == nameof(StmNoteVisibility.DOC) && string.IsNullOrWhiteSpace(ST_Description) && ST_NoteData.ToAscii().StartsWith(@"{\rtf1", StringComparison.Ordinal)); }
		}

		string GetErrorReportMessage(string message)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} PK = {1}, ST_ParentID = {2}, ST_Description = {3}, ST_NoteText = {4}",
					message, //{0}
					PK, //{1}
					ST_ParentID, //{2}
					ST_Description, //{3}
					ST_NoteText); //{4}
		}

		protected virtual void ClearUnusedNoteTextOrData()
		{
			if (!ST_Description.IsEmpty && HasChanges)
			{
				if (ST_IsTextOnly)
				{
					if (!ST_NoteData.IsEmpty)
					{
						ST_NoteData = ZBlob.Empty;
					}
				}
				else
				{
					if (!ST_NoteText.IsEmpty)
					{
						ST_NoteText = ZString.Empty;
					}
				}
			}
		}
		#endregion

		#region Read Only

		public override bool ReadOnly
		{
			get { return (IsReadOnlyAfterAdd) || base.ReadOnly; }
			set { base.ReadOnly = (IsReadOnlyAfterAdd) || value; }
		}

		public bool IsReadOnlyAfterAdd
		{
			get { return fIsReadOnlyAfterAdd; }
			set { fIsReadOnlyAfterAdd = value; }
		}

		void UpdateNoteReadOnlyAfterAdd()
		{
			PredefinedNoteType noteType = null;
			IStmNoteParentWithSystemNote stmNoteParentWithSystemNote = null;
			if (HasMaster)
			{
				noteType = PredefinedNoteTypes.Instance.NoteTypeByDescription(ST_DescriptionInDatabase);
				stmNoteParentWithSystemNote = Master as IStmNoteParentWithSystemNote;
			}
			IsReadOnlyAfterAdd = (noteType != null && noteType.IsReadOnlyAfterAdd) || (stmNoteParentWithSystemNote != null && stmNoteParentWithSystemNote.IsSystemNote(this));
			//the second part is for the unit test, or otherwise if a read-only note type is only loaded locally maybe??
			if ((noteType != null && noteType.IsReadOnlyAfterAdd)
				|| (HasDescriptionList && (ST_Description_List.NoteTypeByDescription(ST_Description)?.IsReadOnlyAfterAdd ?? false)))
			{
				ST_IsCustomDescription = false; //to prevent unsaveable note
			}
		}

		bool fIsReadOnlyAfterAdd;

		#endregion

		#region Overridden Properties

		#region HumanReadableNameCore

		protected override ZString HumanReadableNameCore
		{
			get
			{
				string humanReadableName = Res.GetString("11044606-2fa7-4860-9d90-a99821c85536", "Note");
				if (!ST_Description.IsEmpty)
				{
					humanReadableName += " - " + ST_Description;
				}

				return humanReadableName;
			}
		}

		#endregion

		#region ST_Description

		public override ZString ST_Description
		{
			get
			{
				var description = base.ST_Description;
				if (HasDescriptionList)
				{
					var matchingItem = ST_Description_List.NoteTypeByDescription(description);
					if (matchingItem != null)
					{
						description = matchingItem.Description;
					}
				}
				return description;
			}
			set
			{
				var description = value;
				PredefinedNoteType matchingItem = null;
				if (HasDescriptionList)
				{
					matchingItem = ST_Description_List.NoteTypeByDescription(description);
					if (matchingItem != null)
					{
						description = matchingItem.MultilingualDescription.GetUnresolvedString();
					}
				}
				if (description != ST_Description)
				{
					var oldIsTextOnly = ST_IsTextOnly;
					if (matchingItem != null && base.ST_IsCustomDescription)
					{
						base.ST_IsCustomDescription = false;
					}

					base.ST_Description = description;

					if (HasMaster && !ST_IsCustomDescription)
					{
						ZString visibility = ST_Description_List.DefaultVisibilityForDescription(base.ST_Description);
						if (!visibility.IsEmpty)
						{
							ST_NoteType = visibility;
						}
						UpdateNoteTextMaxLength();
						UpdateIsPopupLog();
						UpdateIsReadOnly();
					}
					if (matchingItem != null)
					{
						if (ST_NoteType != nameof(StmNoteVisibility.DOC))
						{
							if (matchingItem.IsTextOnly && !oldIsTextOnly && !ST_NoteData.IsEmpty)
							{
								var oldNoteData = ST_NoteData;
								ST_NoteData = ZBlob.Empty;
								ST_NoteText = ORtfTextUtil.RtfToText(oldNoteData);
							}
							else if (!matchingItem.IsTextOnly && oldIsTextOnly && !ST_NoteText.IsEmpty)
							{
								ST_NoteData = ORtfTextUtil.TextToRtfBytes(ST_NoteText);
							}
						}
					}
				}
			}
		}

		public ZString ST_DescriptionInDatabase
		{
			get { return base.ST_Description; }
		}

		public virtual ZPropertyInfo ST_DescriptionInDatabaseInfo
		{
			get { return GetZPropertyInfo(nameof(ST_DescriptionInDatabase)); }
		}

		#endregion

		void UpdateIsReadOnly()
		{
			if (ST_IsSerializable)
			{ ReadOnly = true; }
		}

		#region ST_IsCustomDescription

		public override ZBool ST_IsCustomDescription
		{
			get { return base.ST_IsCustomDescription; }
			set
			{
				if (value != base.ST_IsCustomDescription)
				{
					base.ST_IsCustomDescription = value;

					if (ST_IsCustomDescription && ST_NoteData.IsEmpty && !ST_NoteText.IsEmpty && ST_NoteType != nameof(StmNoteVisibility.DOC))
					{
						ST_NoteData = ORtfTextUtil.TextToRtfBytes(ST_NoteText);
					}
					else if (ST_IsTextOnly && !ST_IsCustomDescription && string.IsNullOrEmpty((string)((INeedRow)this).Row[nameof(ST_NoteText)]) && !ST_NoteData.IsEmpty)
					{
						//have to write it like this because otherwise it grabs the value of ST_NoteData
						var temp = ORtfTextUtil.RtfToText(ST_NoteData);
						ST_NoteData = ZBlob.Empty;
						ST_NoteText = temp;
					}

					if (!ST_IsCustomDescription)
					{
						ZString visibility = ST_Description_List.DefaultVisibilityForDescription(ST_Description);
						if (!visibility.IsEmpty)
						{
							ST_NoteType = visibility;
						}
					}
					UpdateIsPopupLog();
				}
			}
		}

		protected bool ST_NoteType_ReadOnly
		{
			get { return !ST_IsCustomDescription; }
		}

		#endregion

		#region ST_NoteText & ST_NoteData

		[BusinessObjectTestExclude] // setting this property should report an exception if the note is not text-only
		public override ZString ST_NoteText
		{
			get
			{
				ZString result;
				if (ST_NoteType == nameof(StmNoteVisibility.DOC))
				{
					result = (!ST_IsSerializable) ? base.ST_NoteText : SerializableNoteText.HumanReadableText(this, SerializableNoteType);
				}
				else
				{
					result = (!ST_IsSerializable) ? base.ST_NoteText : SerializableNoteText.HumanReadableText(this, SerializableNoteType);

					if (result.IsEmpty && !base.ST_NoteData.IsEmpty)
					{
						result = ORtfTextUtil.RtfToText(base.ST_NoteData);
					}
				}

				return result;
			}
			set
			{
				if (!value.IsEmpty)
				{
					ReportDeveloperIfNoteTextShouldNotBeSet(value);
				}
				base.ST_NoteText = value;
			}
		}

		public override ZBlob ST_NoteData
		{
			get
			{
				if (!base.ST_NoteData.IsEmpty)
				{
					return base.ST_NoteData;
				}
				else
				{
					if (!base.ST_NoteText.IsEmpty && ST_NoteType != nameof(StmNoteVisibility.DOC))
					{
						return ZBlob.FromUTF8(base.ST_NoteText);
					}
					return ZBlob.Empty;
				}
			}
			set
			{
				if (!value.IsEmpty)
				{
					ReportDeveloperIfNoteDataShouldNotBeSet(value);
				}
				if (ST_NoteType == nameof(StmNoteVisibility.DOC) && string.IsNullOrWhiteSpace(ST_Description) && value.ToAscii().StartsWith(@"{\rtf1", StringComparison.OrdinalIgnoreCase))
				{
					base.ST_NoteData = ZBlob.FromAscii(ORtfTextUtil.RtfToText(value.ToAscii()));
				}
				else
				{
					base.ST_NoteData = value;
				}
			}
		}

		bool fST_NoteData_ReadOnly;

		public bool ST_NoteData_ReadOnly
		{
			get => fST_NoteData_ReadOnly;
			set
			{
				fST_NoteData_ReadOnly = value;
				ST_NoteDataInfo.RefreshBinding();
			}
		}

		protected bool ST_NoteText_ReadOnly
		{
			get { return ST_IsPopupLog; }
		}

		protected int ST_NoteText_MaxLength
		{
			get { return NoteTextMaxLength; }
		}

		public int NoteTextMaxLength
		{
			get { return noteTextMaxLength ?? -1; }
			set { noteTextMaxLength = value; }
		}
		int? noteTextMaxLength;

		protected virtual void ReportDeveloperIfNoteTextShouldNotBeSet(ZString value)
		{
			if (!IsCopying && !ST_Description.IsEmpty && !ST_IsTextOnly && value != ST_NoteText)
			{
				throw new InvalidOperationException(TextOnNonTextNoteExceptionMessage + string.Format(CultureInfo.InvariantCulture, " {0}: {1}", ST_Description, GetCannotSetST_NoteTextErrorMessage(value))); // Column name used in error message, not key
			}
		}
		public const string TextOnNonTextNoteExceptionMessage = "TextOnNonTextNote";

		protected virtual void ReportDeveloperIfNoteDataShouldNotBeSet(ZBlob value)
		{
			if (!IsCopying && !ST_Description.IsEmpty && ST_IsTextOnly && value != ST_NoteData)
			{
				ErrorReporter.ReportOnce("DataOnTextNote", string.Format(CultureInfo.InvariantCulture, "{0}: {1}", ST_Description, GetCannotSetST_NoteDataErrorMessage(value))); // Column name used in error message, not key
			}
		}

		#region SuppressResourceStringsCheckRegion

		string GetCannotSetST_NoteDataErrorMessage(ZBlob value)
		{
			var text = (ZString)ORtfTextUtil.RtfToText(value);
			return "You should not set ST_NoteData on a text note." + System.Environment.NewLine + GetIncorrectNoteFieldErrorDetails(text);
		}

		string GetCannotSetST_NoteTextErrorMessage(ZString value)
		{
			return "You should not set ST_NoteText on a non-text note." + System.Environment.NewLine + GetIncorrectNoteFieldErrorDetails(value);
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		string GetIncorrectNoteFieldErrorDetails(ZString value)
		{
			const int maxNoteLength = 200;
			var parentBizO = HasMaster ? this.Master.NotesParentTableName : "unknown (Master was null)";
			var parentNoteTypes = ZString.Empty;

			foreach (PredefinedNoteType noteType in ST_Description_List)
			{
				parentNoteTypes += "   \"" + noteType.Code + "\"" + " - IsTextOnly = " + noteType.IsTextOnly + " IsCustom = " + noteType.IsCustomNoteType + System.Environment.NewLine;
			}

			var noteData = (ZString)ORtfTextUtil.RtfToText(ST_NoteData);
			var message = string.Format(
				System.Environment.NewLine +
				"ST_Description = \"{0}\"" + System.Environment.NewLine +
				"ST_IsCustomDecription = {1}" + System.Environment.NewLine +
				"ST_IsTextOnly = {2}" + System.Environment.NewLine +
				"ST_NoteData (first {3} characters) = {4}" + System.Environment.NewLine +
				"ST_NoteText (first {3} characters) = {5}" + System.Environment.NewLine +
				"ST_PK  = {14}" + System.Environment.NewLine +
				"value (first {3} characters) = {6}" + System.Environment.NewLine +
				"ST_NoteType = {10}" + System.Environment.NewLine +
				"ST_IsSerializable = {11}" + System.Environment.NewLine +
				"base.ST_NoteData.IsEmpty ? {12}" + System.Environment.NewLine +
				"base.ST_NoteText (first {3} characters) : {13}" + System.Environment.NewLine +
				"Parent BizObject = \"{7}\"" + System.Environment.NewLine +
				"Parent BizObject's Predefined Notes = " + System.Environment.NewLine +
				"{8}" + System.Environment.NewLine +
				"{9}" + System.Environment.NewLine + System.Environment.NewLine +
				"Call stack of updating IsPopupLog to true:" + System.Environment.NewLine +
				"{15}", ST_Description, ST_IsCustomDescription, ST_IsTextOnly, maxNoteLength, noteData.SubstringSafe(0, maxNoteLength), ST_NoteText.SubstringSafe(0, maxNoteLength), value.SubstringSafe(0, maxNoteLength), parentBizO, parentNoteTypes, GetActiveControlInfo(), ST_NoteType, ST_IsSerializable, base.ST_NoteData.IsEmpty, base.ST_NoteText.SubstringSafe(0, maxNoteLength), PK, updateIsPopupLogToTrueStackTraces); // Exception Text}

			return message;
		}

		string GetActiveControlInfo() => ObjectFactory.Get<IStmNoteControl>().GetActiveControlInfo();

		#endregion
		#endregion

		#region ST_Table

		public override ZString ST_Table
		{
			get { return base.ST_Table; }
			set
			{
				base.ST_Table = ZDataUtils.GetTableNameFromDbAndTableName(value);

				if (IsNewOrgNote)
				{
					SetDefaultValuesForOrg();
				}
			}
		}

		bool IsNewOrgNote =>
			ST_Table == OrgHeaderSchema.Constants.TableName && !IsInDatabase;

		#endregion

		#region ST_NoteData_HTML

		public ZBlob ST_NoteData_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(ST_NoteData);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				base.ST_NoteData = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
				ST_NoteData_HTMLInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ST_NoteData_HTMLInfo
		{
			get { return GetZPropertyInfo(nameof(ST_NoteData_HTML)); }
		}

		#endregion

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			DataRow row = ((IBusinessObjectInternals)this).Row;
			row[StmNoteSchema.Constants.ST_NoteContext] = "AAA";
		}

		protected virtual void SetDefaultValuesForOrg()
		{
			if (DataRegistry.Instance.DefaultNoteContextFromCurrentlyLoggedInDepartment)
			{
				var department = EnvProxy.Instance.CurrentDepartment;

				if (department != null)
				{
					ST_NoteContextModule = GetValueFromDepartment(department, "GE_Activity", StmNoteContextModule.A).ToString();
					ST_NoteContextDirection = GetValueFromDepartment(department, "GE_Direction", StmNoteContextDirection.A).ToString();
					ST_NoteContextFreightMode = GetValueFromDepartment(department, "GE_Mode", StmNoteContextFreightMode.A).ToString();
				}
			}

			if (ST_GC_RelatedCompany.IsEmpty && DataRegistry.Instance.DefaultNoteCompanyFromCurrentlyLoggedInCompany)
			{
				ST_GC_RelatedCompany = EnvProxy.Instance.CurrentBranch.CompanyPK;
			}
		}

		T GetValueFromDepartment<T>(IDepartment department, string propertyName, T fallback)
		{
			var propertyInfo = ((BusinessObject)department).FindPropertyInfo(propertyName);
			if (propertyInfo == null)
			{
				ErrorReporter.ReportOnce("Could not find property " + propertyName + " on GlbDepartment");
				return fallback;
			}

			return GetPKOfCodeDescription(propertyInfo, fallback);
		}

		T GetPKOfCodeDescription<T>(ZPropertyInfo info, T fallback)
		{
			var probablyList = ZMetaData.GetMetaData(
								info.BizObj,
								info.PropertyDescriptor,
								MetaDataTypes.ListDataSource);

			if (probablyList == null)
			{
				ErrorReporter.ReportOnce("ListAttributeMissingOnDepartmentProp", "The list attribute is missing on " + info.Name);
				return fallback;
			}

			var list = probablyList as CodeDescriptionPairList;
			if (list == null)
			{
				ErrorReporter.ReportOnce("ListAttributeHasBadTypeOnDepartmentProp", "The list attribute for " + info.Name + " should be a CodeDescriptionPairList");
				return fallback;
			}

			var code = list.GetCodeFromDescription((ZString)info.Value);
			if (code != null)
			{
				var value = list[code, StringComparison.CurrentCulture]?.PK;
				if (value != null && value is T)
				{
					return (T)value;
				}
			}

			return fallback;
		}

		#endregion

		#region New Properties

		#region IsLoggingEnabled
		public bool IsLoggingEnabled { get; set; } = true;

		#endregion

		#region ST_NoteType_DescriptiveText

		[BusinessObjectTestExclude()]
		public ZString ST_NoteType_DescriptiveText
		{
			get
			{
				switch (ST_NoteType)
				{
					case StmNoteDescription.Pub:
						return StmNoteDescription.PubDescriptive;
					case StmNoteDescription.Int:
						return StmNoteDescription.IntDescriptive;
					case StmNoteDescription.Prv:
						return StmNoteDescription.PrvDescriptive;
					case StmNoteDescription.Agv:
						return StmNoteDescription.AgvDescriptive;
					default:
						return base.ST_NoteType;
				}
			}
			set
			{
				if (value.EqualsIgnoringCase(StmNoteDescription.PubDescriptive))
				{
					ST_NoteType = StmNoteDescription.Pub;
				}
				else if (value.EqualsIgnoringCase(StmNoteDescription.IntDescriptive))
				{
					ST_NoteType = StmNoteDescription.Int;
				}
				else if (value.EqualsIgnoringCase(StmNoteDescription.PrvDescriptive))
				{
					ST_NoteType = StmNoteDescription.Prv;
				}
				else if (value.EqualsIgnoringCase(StmNoteDescription.AgvDescriptive))
				{
					ST_NoteType = StmNoteDescription.Agv;
				}
				else
				{
					ST_NoteType = value.SubstringSafe(0, Schema.ST_NoteTypeMaxLength);
				}
				ST_NoteType_DescriptiveTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ST_NoteType_DescriptiveTextInfo
		{
			get { return GetZPropertyInfo(nameof(ST_NoteType_DescriptiveText)); }
		}

		protected bool ST_NoteType_DescriptiveText_ReadOnly
		{
			get { return !ST_IsCustomDescription; }
		}

		#endregion

		#region ST_NoteDataAsText

		[BusinessObjectTestExclude()] // Max Length is not known for the binary data converted to text
		public ZString ST_NoteDataAsText
		{
			get { return ST_IsTextOnly ? ST_NoteText : (ZString)ORtfTextUtil.RtfToText(ST_NoteData); }
			set
			{
				if (ST_IsTextOnly)
				{
					ST_NoteText = value;
				}
				else
				{
					ST_NoteData = ORtfTextUtil.TextToRtfBytes(value);
				}
				ST_NoteDataAsTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ST_NoteDataAsTextInfo
		{
			get { return GetZPropertyInfo(Schema.ST_NoteDataAsText); }
		}

		#endregion

		#region ST_NoteDataAsTextConcatenatedAndTrimmed

		[BusinessObjectTestExclude()] // Max Length is not known for the binary data converted to text
		[MaxLength(80)]
		public ZString ST_NoteDataAsTextConcatenatedAndTrimmed
		{
			get { return ST_NoteDataAsText.SubstringSafe(0, 80).Replace(System.Environment.NewLine, " "); }
		}

		public ZPropertyInfo ST_NoteDataAsTextConcatenatedAndTrimmedInfo
		{
			get { return GetZPropertyInfo(Schema.ST_NoteDataAsTextConcatenatedAndTrimmed); }
		}

		#endregion

		#region ST_NoteSource

		[EditorBrowsable(EditorBrowsableState.Never)]
		[BusinessObjectTestExclude()] // Max Length is not known for the note source
		public virtual ZString ST_NoteSource
		{
			get
			{
				ZString result;

				if (Master is INoteSource && IsRelatedInParentView)
				{
					result = ((INoteSource)Master).NoteSourceName;
				}
				else
				{
					if (!ST_Table.IsEmpty)
					{
						result = DataBoundResourceStrings.GetTableDescriptiveName(ST_Table);
					}
					else
					{
						result = ZString.Empty;
					}
				}

				if (!IsRelatedInParentView)
				{
					result = Res.GetString("935fbfe8-d47a-4f58-94b9-dfcd8dc33f56", "This {0}", result);
				}

				return result;
			}
		}

		public ZPropertyInfo ST_NoteSourceInfo
		{
			get { return GetZPropertyInfo(Schema.ST_NoteSource); }
		}

		#endregion

		#region ST_CreatedDateUtc

		public ZDateTime ST_CreatedDateUtc
		{
			get { return IsInDatabase ? ST_SystemCreateTimeUtc : ZDateTime.Empty; }
		}

		public ZPropertyInfo ST_CreatedDateUtcInfo
		{
			get { return GetZPropertyInfo(Schema.ST_CreatedDateUtc); }
		}

		public ZDateTime ST_CreatedDateLocal
		{
			get
			{
				ZDateTime createdDateUtc = ST_CreatedDateUtc;
				return createdDateUtc.IsValid ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(createdDateUtc.ToDateTime()) : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo ST_CreatedDateLocalInfo
		{
			get { return GetZPropertyInfo(nameof(ST_CreatedDateLocal)); }
		}

		#endregion

		#region ST_CreatedByUserInitials + ST_CreatedByUserName + ST_LastModifiedUserName

		[MaxLength(3)]
		public ZString ST_CreatedByUserInitials
		{
			get { return ST_SystemCreateUser; }
		}

		[MaxLength(256)]
		public ZString ST_CreatedByUserName
		{
			get
			{
				if (ST_SystemCreateUser.Length > 0)
				{
					return GetUserFromUserCode(ST_SystemCreateUser)?.GS_FullName ?? ST_SystemCreateUser;
				}

				return ZString.Empty;
			}
		}

		[MaxLength(3)]
		public ZString ST_LastModifiedByUserName
		{
			get { return ST_SystemLastEditUser; }
		}

		public ZPropertyInfo ST_CreatedByUserInitialsInfo
		{
			get { return GetZPropertyInfo(Schema.ST_CreatedByUserInitials); }
		}

		public ZPropertyInfo ST_CreatedByUserNameInfo
		{
			get { return GetZPropertyInfo(Schema.ST_CreatedByUserName); }
		}

		public ZPropertyInfo ST_LastModifiedByUserNameInfo
		{
			get { return GetZPropertyInfo(Schema.ST_LastModifiedByUserName); }
		}

		public IGlbStaff GetUserFromUserCode(string userCode)
		{
			return Factory.LoadFromNaturalKey<IGlbStaff>(GlbStaffSchema.GS_Code, userCode);
		}

		#endregion

		#region ST_LastModifiedDate

		public ZDateTime ST_LastModifiedDate
		{
			get { return ST_SystemLastEditTimeUtc; }
		}

		public ZPropertyInfo ST_LastModifiedDateInfo
		{
			get { return GetZPropertyInfo(Schema.ST_LastModifiedDate); }
		}

		public ZDateTime ST_LastModifiedDateLocal
		{
			get
			{
				ZDateTime lastModifiedDateUtc = ST_LastModifiedDate;
				return lastModifiedDateUtc.IsValid ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(lastModifiedDateUtc.ToDateTime()) : ZDateTime.Empty;
			}
		}

		public ZPropertyInfo ST_LastModifiedDateLocalInfo
		{
			get { return GetZPropertyInfo(nameof(ST_LastModifiedDateLocal)); }
		}

		#endregion

		#region ST_IsSerializable

		bool ST_IsSerializable
		{
			get
			{
				return SerializableNoteType != null;
			}
		}

		#endregion

		Type SerializableNoteType
		{
			get
			{
				PredefinedNoteType noteType = (HasMaster && Master.NoteTypes != null) ? Master.NoteTypes.NoteTypeByDescription(ST_Description) : null;
				return (noteType != null) ? noteType.SerializableNoteType : null;
			}
		}

		#region ST_IsTextOnly

		public ZBool ST_IsTextOnly
		{
			get // this may need caching / refreshing if it is hit too often
			{
				ZBool result = false;

				if (!ST_IsCustomDescription)
				{
					if (HasDescriptionList)
					{
						var matchingItem = ST_Description_List.NoteTypeByDescription(ST_DescriptionInDatabase);
						if (matchingItem != null)
						{
							return matchingItem.IsTextOnly;
						}
					}
					PredefinedNoteType noteType = PredefinedNoteTypes.Instance.NoteTypeByDescription(ST_DescriptionInDatabase);
					if (noteType != null)
					{
						result = noteType.IsTextOnly;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo ST_IsTextOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.ST_IsTextOnly); }
		}

		#endregion

		#region ST_IsPopupLog

		public ZBool ST_IsPopupLog
		{
			get
			{
				if (!ST_IsTextOnly)
				{
					isPopupLog = false;
				}

				return isPopupLog;
			}
		}

		public ZPropertyInfo ST_IsPopupLogInfo
		{
			get { return GetZPropertyInfo(Schema.ST_IsPopupLog); }
		}

		ZBool isPopupLog;

		#endregion

		#region ST_NoteContext "wrappers"

		readonly ZString ErrorMessage = Res.GetString("ba6ba887-0459-4866-ba06-104f7ff21698", "wrong entry");

		#region ST_NoteContextModule

		[BusinessObjectTestExclude]
		[MaxLength(1)]
		public virtual ZString ST_NoteContextModule
		{
			get { return ST_NoteContext.Length == 3 ? new ZString(this.ST_NoteContext.Substring(0, 1)) : (ZString)"?"; }
			set
			{
				if ((ST_NoteContextModule == nameof(StmNoteContextModule.W) && value != nameof(StmNoteContextModule.W))
					|| (ST_NoteContextModule != nameof(StmNoteContextModule.W) && value == nameof(StmNoteContextModule.W)))
				{
					ST_NoteContextDirection = nameof(StmNoteContextDirection.A);
					ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
				}
				value = value.TrimEnd(' ');
				CheckMaximumLength(ST_NoteContextModuleInfo, value);
				this.ST_NoteContext = value + ((this.ST_NoteContext.Length == 3) ? this.ST_NoteContext.Substring(1, 2) : (ZString)"??");
				ST_NoteContextModuleInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ST_NoteContextModuleInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ST_NoteContextModule)); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(90)]
		public virtual ZString ST_NoteContextModuleCaption
		{
			get
			{
				return StmNoteCaptions.Module.ContainsKey(ST_NoteContextModule) ? StmNoteCaptions.Module[ST_NoteContextModule] : ErrorMessage;
			}
			set
			{
				var match = StmNoteCaptions.Module.SingleOrDefault(pair => pair.Value.ToString().Equals(value, StringComparison.OrdinalIgnoreCase));
				ST_NoteContextModule = match.Key.IsEmpty ? (ZString)"?" : match.Key;
				ST_NoteContextModuleCaptionInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ST_NoteContextModuleCaptionInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ST_NoteContextModuleCaption)); }
		}

		#endregion

		#region ST_NoteContextDirection

		[BusinessObjectTestExclude]
		[MaxLength(1)]
		public virtual ZString ST_NoteContextDirection
		{
			get { return ST_NoteContext.Length == 3 ? new ZString(this.ST_NoteContext.Substring(1, 1)) : (ZString)"?"; }
			set
			{
				if (ST_NoteContextDirection != value)
				{
					ST_NoteContextFreightMode = nameof(StmNoteContextFreightMode.A);
				}
				value = value.TrimEnd(' ');
				CheckMaximumLength(ST_NoteContextDirectionInfo, value);
				this.ST_NoteContext = this.ST_NoteContext.Substring(0, 1) + value + this.ST_NoteContext.Substring(2, 1);
				ST_NoteContextDirectionInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ST_NoteContextDirectionInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ST_NoteContextDirection)); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(90)]
		public virtual ZString ST_NoteContextDirectionCaption
		{
			get
			{
				ZString result = ErrorMessage;
				if (ST_NoteContextModule == nameof(StmNoteContextModule.W))
				{
					if (StmNoteCaptions.DirectionWarehouse.ContainsKey(ST_NoteContextDirection))
					{
						result = StmNoteCaptions.DirectionWarehouse[ST_NoteContextDirection];
					}
				}
				else
				{
					if (StmNoteCaptions.Direction.ContainsKey(ST_NoteContextDirection))
					{
						result = StmNoteCaptions.Direction[ST_NoteContextDirection];
					}
				}
				return result;
			}
			set
			{
				ST_NoteContextDirection = "?";
				if (ST_NoteContextModule == nameof(StmNoteContextModule.W))
				{
					var match = StmNoteCaptions.DirectionWarehouse.SingleOrDefault(pair => pair.Value.ToString().Equals(value, StringComparison.OrdinalIgnoreCase));
					if (!match.Key.IsEmpty)
					{
						ST_NoteContextDirection = match.Key;
					}
				}
				else
				{
					var match = StmNoteCaptions.Direction.SingleOrDefault(pair => pair.Value.ToString().Equals(value, StringComparison.OrdinalIgnoreCase));
					if (!match.Key.IsEmpty)
					{
						ST_NoteContextDirection = match.Key;
					}
				}
				ST_NoteContextDirectionCaptionInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ST_NoteContextDirectionCaptionInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ST_NoteContextDirectionCaption)); }
		}

		#endregion

		#region ST_NoteContextFreightMode

		[BusinessObjectTestExclude]
		[MaxLength(1)]
		public virtual ZString ST_NoteContextFreightMode
		{
			get { return ST_NoteContext.Length == 3 ? new ZString(this.ST_NoteContext.Substring(2, 1)) : (ZString)"?"; }
			set
			{
				value = value.TrimEnd(' ');
				CheckMaximumLength(ST_NoteContextFreightModeInfo, value);
				this.ST_NoteContext = this.ST_NoteContext.Substring(0, 2) + value;
				ST_NoteContextFreightModeInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ST_NoteContextFreightModeInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ST_NoteContextFreightMode)); }
		}

		[BusinessObjectTestExclude]
		[MaxLength(90)]
		public virtual ZString ST_NoteContextFreightModeCaption
		{
			get
			{
				ZString result = ErrorMessage;
				if (ST_NoteContextModule == nameof(StmNoteContextModule.W))
				{
					if (ST_NoteContextDirection == nameof(StmNoteContextWarehouseDirection.R))
					{
						if (StmNoteCaptions.FreightModeWarehouseR.ContainsKey(ST_NoteContextFreightMode))
						{
							result = StmNoteCaptions.FreightModeWarehouseR[ST_NoteContextFreightMode];
						}
					}
					else if (ST_NoteContextDirection == nameof(StmNoteContextWarehouseDirection.O))
					{
						if (StmNoteCaptions.FreightModeWarehouseO.ContainsKey(ST_NoteContextFreightMode))
						{
							result = StmNoteCaptions.FreightModeWarehouseO[ST_NoteContextFreightMode];
						}
					}
					else if (ST_NoteContextDirection == nameof(StmNoteContextWarehouseDirection.I))
					{
						if (StmNoteCaptions.FreightModeWarehouseI.ContainsKey(ST_NoteContextFreightMode))
						{
							result = StmNoteCaptions.FreightModeWarehouseI[ST_NoteContextFreightMode];
						}
					}
					else if (ST_NoteContextFreightMode == "A")
					{
						result = StmNoteCaptions.FreightMode[ST_NoteContextFreightMode];
					}
				}
				else
				{
					if (StmNoteCaptions.FreightMode.ContainsKey(ST_NoteContextFreightMode))
					{
						result = StmNoteCaptions.FreightMode[ST_NoteContextFreightMode];
					}
				}
				return result;
			}
			set
			{
				ST_NoteContextFreightMode = "?";
				Dictionary<ZString, MultilingualString> captionLookup = null;
				if (ST_NoteContextModule == nameof(StmNoteContextModule.W))
				{
					if (ST_NoteContextDirection == nameof(StmNoteContextWarehouseDirection.R))
					{
						captionLookup = StmNoteCaptions.FreightModeWarehouseR;
					}
					else if (ST_NoteContextDirection == nameof(StmNoteContextWarehouseDirection.O))
					{
						captionLookup = StmNoteCaptions.FreightModeWarehouseO;
					}
					else if (ST_NoteContextDirection == nameof(StmNoteContextWarehouseDirection.I))
					{
						captionLookup = StmNoteCaptions.FreightModeWarehouseI;
					}
					else if (value == Res.GetString("2b5bc3d0-15c4-46e6-8c67-7d2c5027747b", "A - All"))
					{
						ST_NoteContextFreightMode = "A"; // Code Value for Comparison
					}
				}
				else
				{
					captionLookup = StmNoteCaptions.FreightMode;
				}

				if (captionLookup != null)
				{
					var match = captionLookup.SingleOrDefault(pair => pair.Value.ToString().Equals(value, StringComparison.OrdinalIgnoreCase));
					if (!match.Key.IsEmpty)
					{
						ST_NoteContextFreightMode = match.Key;
					}
				}
				ST_NoteContextFreightModeCaptionInfo.RefreshBinding();
			}
		}

		public virtual ZPropertyInfo ST_NoteContextFreightModeCaptionInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(ST_NoteContextFreightModeCaption)); }
		}

		#endregion

		#region ST_NoteContext

		public override ZString ST_NoteContext
		{
			get { return base.ST_NoteContext; }
			set
			{
				if (value.Length < 3)
				{
					ErrorReporter.ReportOnce("ST_NoteShouldNotBeLessThanThreeLetters", $"ST_Note should not be less than three letters, new value is '{value}', current value is '{base.ST_NoteContext}'.");
				}
				base.ST_NoteContext = value;
			}
		}

		#endregion

		#endregion

		public bool ST_IsClonedForEdit
		{
			get { return fIsClonedForEdit; }
			set { fIsClonedForEdit = value; }
		}

		#region ST_GC_RelatedCompany

		[List("GlbCompanyList")]
		public override ZGuid ST_GC_RelatedCompany
		{
			get { return base.ST_GC_RelatedCompany; }
			set { base.ST_GC_RelatedCompany = value; }
		}

		public bool IsBelongingToCurrentLoginCompany
		{
			get
			{
				return ST_GC_RelatedCompany == ZGuid.Empty || ST_GC_RelatedCompany == EnvProxy.Instance.CurrentCompany.PK;
			}
		}

		#endregion

		bool fIsClonedForEdit;

		public virtual bool ShouldNoteContextBeChecked
		{
			get
			{
				return !ST_NoteContext_ReadOnly;
			}
		}

		#endregion

		#region List Properties

		#region GlbCompanyList

		public IActiveBusinessObjectCollection GlbCompanyList
		{
			get
			{
				if (glbCompanyList == null)
				{
					glbCompanyList = (IActiveBusinessObjectCollection)ObjectFactory.Get<IGlbCompanyCollection>("IGlbCompanyCollection", Factory);
				}

				return glbCompanyList;
			}
		}

		IActiveBusinessObjectCollection glbCompanyList;

		#endregion

		#region ST_NoteType_List

		[List("ST_NoteTypeCore_List")]
		public override ZString ST_NoteType
		{
			get => base.ST_NoteType;
			set
			{
				base.ST_NoteType = value;

				if (base.ST_NoteType == nameof(StmNoteVisibility.DOC))
				{
					ST_NoteData = ST_NoteData;
				}
			}
		}

		public virtual CodeDescriptionPairList ST_NoteType_List
		{
			get
			{
				if (fST_NoteType_List == null)
				{
					fST_NoteType_List = new CodeDescriptionPairList();
					fST_NoteType_List.AddPair(StmNoteDescription.PubDescriptive, ZString.Empty);
					fST_NoteType_List.AddPair(StmNoteDescription.IntDescriptive, ZString.Empty);
					fST_NoteType_List.AddPair(StmNoteDescription.PrvDescriptive, ZString.Empty);
					fST_NoteType_List.AddPair(StmNoteDescription.AgvDescriptive, ZString.Empty);
				}

				return fST_NoteType_List;
			}
		}
		CodeDescriptionPairList fST_NoteType_List;

		public virtual CodeDescriptionPairList ST_NoteTypeCore_List
		{
			get
			{
				if (fST_NoteTypeCore_List == null)
				{
					fST_NoteTypeCore_List = new CodeDescriptionPairList();
					fST_NoteTypeCore_List.AddPair(StmNoteDescription.Pub, StmNoteDescription.PubDescriptive);
					fST_NoteTypeCore_List.AddPair(StmNoteDescription.Int, StmNoteDescription.IntDescriptive);
					fST_NoteTypeCore_List.AddPair(StmNoteDescription.Prv, StmNoteDescription.PrvDescriptive);
					fST_NoteTypeCore_List.AddPair(StmNoteDescription.Agv, StmNoteDescription.AgvDescriptive);
				}

				return fST_NoteTypeCore_List;
			}
		}
		CodeDescriptionPairList fST_NoteTypeCore_List;

		#endregion

		#region ST_Description_List

		public virtual NoteTypeCollection ST_Description_List
		{
			get
			{
				if (descriptionListOverride != null)
				{
					return descriptionListOverride;
				}
				if (!HasMaster)
				{
					ErrorReporter.ReportOnce("Note must have parent object.");
					return new NoteTypeCollection();
				}
				return Master.NoteTypes;
			}
			protected set
			{
				descriptionListOverride = value;
			}
		}

		NoteTypeCollection descriptionListOverride;

		public ZBool HasDescriptionList
		{
			get { return HasMaster || descriptionListOverride != null; }
		}

		#endregion

		#region ST_NoteContextModule_List

		protected CodeDescriptionPairList fST_NoteContextModule_List;
		public CodeDescriptionPairList ST_NoteContextModule_List
		{
			get
			{
				if (fST_NoteContextModule_List == null)
				{
					fST_NoteContextModule_List = new CodeDescriptionPairList();
					fST_NoteContextModule_List.AddPair(StmNoteCaptions.Module["A"], Res.GetString("StmNote|ContextModule|EveryRelatedEntitySeesTheseNotes", "Every related entity sees these notes"));
					fST_NoteContextModule_List.AddPair(StmNoteCaptions.Module["F"], Res.GetString("StmNote|ContextModule|VisibleOnForwardingSystemOnly", "Visible on Forwarding system only"));
					fST_NoteContextModule_List.AddPair(StmNoteCaptions.Module["C"], Res.GetString("StmNote|ContextModule|VisibleOnCFSSystemOnly", "Visible on CFS system only"));
					fST_NoteContextModule_List.AddPair(StmNoteCaptions.Module["D"], Res.GetString("StmNote|ContextModule|VisibleOnCustomsDeclarationsSystemOnly", "Visible on Customs/Declarations system only"));
					fST_NoteContextModule_List.AddPair(StmNoteCaptions.Module["E"], Res.GetString("StmNote|ContextModule|VisibleOnCustomsDeclarationsSystemAndShipments", "Visible on Customs/Declarations system and Shipments"));
					fST_NoteContextModule_List.AddPair(StmNoteCaptions.Module["O"], Res.GetString("StmNote|ContextModule|VisibleOnOrdersSystemOnly", "Visible on Orders system only"));
					fST_NoteContextModule_List.AddPair(StmNoteCaptions.Module["I"], Res.GetString("StmNote|ContextModule|VisibleOnForwardingBrokerageCFSAndOrdersSystems", "Visible on Forwarding, Brokerage, CFS and Orders systems"));
					fST_NoteContextModule_List.AddPair(StmNoteCaptions.Module["T"], Res.GetString("StmNote|ContextModule|VisibleOnTransportSystemOnly", "Visible on Transport system only"));
					fST_NoteContextModule_List.AddPair(StmNoteCaptions.Module["W"], Res.GetString("StmNote|ContextModule|VisibleOnWarehouseSystemOnly", "Visible on Warehouse system only"));
					fST_NoteContextModule_List.AddPair(StmNoteCaptions.Module["S"], Res.GetString("StmNote|ContextModule|VisibleOnShipsAgencySystemOnly", "Visible on Ships Agency system only"));
				}

				return fST_NoteContextModule_List;
			}
		}

		#endregion

		#region ST_NoteContextDirection_List

		public CodeDescriptionPairList ST_NoteContextDirection_List
		{
			get
			{
				CodeDescriptionPairList fST_NoteContextDirection_List = new CodeDescriptionPairList();

				if (ST_NoteContextModule == nameof(StmNoteContextModule.W))
				{
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.DirectionWarehouse["A"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForAnyWarehouseTransaction", "These notes are shown for any warehouse transaction"));
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.DirectionWarehouse["R"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForInboundWarehouseTransactions", "These notes are shown for Inbound warehouse transactions"));
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.DirectionWarehouse["O"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForOutboundWarehouseTransactions", "These notes are shown for Outbound warehouse transactions"));
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.DirectionWarehouse["I"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForInternalWarehouseTransactions", "These notes are shown for Internal warehouse transactions"));
				}

				else
				{
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.Direction["A"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForAnyDirection", "These notes are shown for any direction"));
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.Direction["I"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForImportDirectionOnly", "These notes are shown for Import direction only"));
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.Direction["E"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForExportDirectionOnly", "These notes are shown for Export direction only"));
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.Direction["B"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForImportAndExportDirections", "These notes are shown for Import and Export directions"));
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.Direction["D"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForDomesticDirectonOnly", "These notes are shown for Domestic direction only"));
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.Direction["X"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForCrossTradeDirectionOnly", "These notes are shown for Cross Trade direction only"));
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.Direction["F"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForAllForwardingDirections", "These notes are shown for All Forwarding directions"));
					fST_NoteContextDirection_List.AddPair(StmNoteCaptions.Direction["O"], Res.GetString("StmNote|ContextDirection|TheseNotesAreShownForOtherDirections", "These notes are shown for Other directions"));
				}

				return fST_NoteContextDirection_List;
			}
		}

		#endregion

		#region ST_NoteContextModule_List

		public CodeDescriptionPairList ST_NoteContextFreightMode_List
		{
			get
			{
				CodeDescriptionPairList fST_NoteContextFreightMode_List = new CodeDescriptionPairList();

				if (ST_NoteContextModule == nameof(StmNoteContextModule.W))
				{
					if (ST_NoteContextDirection == nameof(StmNoteContextWarehouseDirection.R))
					{
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseR["A"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForAnyWarehouseTransaction", "These notes are shown for any warehouse transaction"));
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseR["R"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForWarehouseReceiveTransactions", "These notes are shown for warehouse Receive transactions"));
					}

					else if (ST_NoteContextDirection == nameof(StmNoteContextWarehouseDirection.O))
					{
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseO["A"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForAnyWarehouseTransaction", "These notes are shown for any warehouse transaction"));
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseO["O"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForWarehouseOrderTransactions", "These notes are shown for warehouse Order transactions"));
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseO["R"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForWarehouseReleaseTransactions", "These notes are shown for warehouse Release transactions"));
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseO["T"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForInterWarehouseTransferTransactions", "These notes are shown for Inter Warehouse Transfer transactions"));
					}

					else if (ST_NoteContextDirection == nameof(StmNoteContextWarehouseDirection.I))
					{
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseI["A"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForAnyWarehouseTransaction", "These notes are shown for any warehouse transaction"));
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseI["T"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForWarehouseTransferTransactions", "These notes are shown for warehouse Transfer transactions"));
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseI["D"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForWarehouseAdjustmentTransactions", "These notes are shown for warehouse Adjustment transactions"));
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseI["S"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForWarehouseStocktakeCyclicCounts", "These notes are shown for warehouse Stocktake / Cyclic counts"));
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightModeWarehouseI["P"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForWarehousePeriodicBillingInvoices", "These notes are shown for warehouse Periodic Billing invoices"));
					}

					else
					{
						fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightMode["A"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForAnyWarehouseTransaction", "These notes are shown for any warehouse transaction"));
					}
				}

				else
				{
					fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightMode["A"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForAnyFreightMode", "These notes are shown for any freight mode"));
					fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightMode["S"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForSeaFreightModeOnly", "These notes are shown for Sea freight mode only"));
					fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightMode["F"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForFullContainerLoadFCLOnly", "These notes are shown for Full Container Load (FCL) only"));
					fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightMode["L"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForLessContainerLoadLCLOnly", "These notes are shown for Less Container Load (LCL) only"));
					fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightMode["I"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForAirFreightModeOnly", "These notes are shown for Air freight mode only"));
					fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightMode["R"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForRoadFreightModeOnly", "These notes are shown for Road freight mode only"));
					fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightMode["W"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForRailFreightModeOnly", "These notes are shown for Rail freight mode only"));
					fST_NoteContextFreightMode_List.AddPair(StmNoteCaptions.FreightMode["B"], Res.GetString("StmNote|ContextFreightMode|TheseNotesAreShownForAirAndSeaFreightModes", "These notes are shown for FSA and FAS freight modes only"));
				}

				return fST_NoteContextFreightMode_List;
			}
		}

		#endregion

		#endregion

		#region Master

		public virtual IStmNoteParent Master
		{
			get { return master; }
			set
			{
				if (master != value)
				{
					master = value;
					ST_Table = value != null ? new ZString(value.NotesParentTableName) : ZString.Empty;

					if (HasMaster && Master.NoteTypes.Count == 0 && !ST_IsCustomDescription)
					{
						if (!CustomNotesProvider.Instance.NoteTypeExistsByName(ST_DescriptionInDatabase))
						{
							ST_IsCustomDescription = true; // developer might have removed a previously existing list
						}
					}

					UpdateNoteTextMaxLength();
					if (!IsReadOnlyAfterAdd)
					{
						UpdateNoteReadOnlyAfterAdd();
					}
				}
			}
		}
		IStmNoteParent master;

		public virtual ICollection<IStmNoteParent> OverrideValidationMasters
		{
			get { return new List<IStmNoteParent>() { Master }; }
		}

		protected bool ST_NoteContext_ReadOnly
		{
			get { return !(Master is IOrgHeader); }
		}

		public virtual bool IsParentTemplateRecord => (Master as ITemplateRecordProvider)?.IsTemplateRecord ?? false;

		#endregion

		#region Popup

		public StmNote GetCloneForPopup()
		{
			StmNote result = (StmNote)Clone();
			if (ST_IsPopupLog)
			{
				result.ST_NoteText = "";
			}

			if (IsInDatabase)
			{
				result.ST_IsClonedForEdit = true;
			}

			if (this.HasDescriptionList)
			{
				result.descriptionListOverride = this.ST_Description_List;
			}
			result.ReadOnly = ReadOnly;
			result.parentTextTemplateContext = GetTextTemplateContextBusinessObject();
			result.NoteTextMaxLength = ST_NoteText_MaxLength;
			return result;
		}

		public void CopyChangesFromPopup(StmNote clone)
		{
			if (ST_IsPopupLog)
			{
				CopyTextFromPopup(clone);
			}
			else
			{
				if (ST_IsTextOnly)
				{
					ST_NoteText = clone.ST_NoteText.Length > ST_NoteTextInfo.MaxLength ? clone.ST_NoteText.SubstringSafe(0, ST_NoteTextInfo.MaxLength) : clone.ST_NoteText;
				}
				else
				{
					ST_NoteData = clone.ST_NoteData;
				}
			}
		}

		void CopyTextFromPopup(StmNote clone)
		{
			var builder = new StringBuilder();
			builder.Append(EnvProxy.Instance.CurrentUser.InitialsAndDateTime);

			var cloneText = clone.ST_NoteText.Trim();
			builder.Append(cloneText);

			if (!ST_NoteText.IsEmpty)
			{
				builder.AppendLine();
				builder.AppendLine(new string('-', 125));
				builder.Append(ST_NoteText);
			}

			ST_NoteDataAsText = new ZString(builder.ToString()).SubstringSafe(0, ST_NoteTextInfo.MaxLength);
		}

		readonly StringBuilder updateIsPopupLogToTrueStackTraces = new StringBuilder();

		void UpdateIsPopupLog()
		{
			if (!ST_IsCustomDescription)
			{
				PredefinedNoteType noteType = PredefinedNoteTypes.Instance.NoteTypeByDescription(ST_DescriptionInDatabase);
				isPopupLog = (noteType != null) && noteType.IsPopupLog;

				if (isPopupLog)
				{
					updateIsPopupLogToTrueStackTraces.AppendLine("ST_DescriptionInDatabase: " + ST_DescriptionInDatabase); // Log Information
					updateIsPopupLogToTrueStackTraces.AppendLine(new StackTrace().ToString());
				}
			}
			else
			{
				isPopupLog = false;
			}

			ST_IsPopupLogInfo.RefreshBinding();
		}

		#endregion

		#region IStmNoteInternals

		bool IStmNoteInternals.IsRelatedInParentView
		{
			get { return IsRelatedInParentView; }
		}

		protected virtual bool IsRelatedInParentView
		{
			get
			{
				StmNoteCollectionView view = ((IBusinessObjectInternals)this).ParentCollections.OfType<StmNoteCollectionView>().FirstOrDefault();
				return view != null && IsRelatedInView(view);
			}
		}

		protected virtual bool IsRelatedInView(StmNoteCollectionView view)
		{
			return ST_ParentID != view.Parent.NotesParentPK;
		}

		bool IStmNoteInternals.IsNoteRead
		{
			get { return IsNoteRead; }
			set { IsNoteRead = value; }
		}

		bool IsNoteRead
		{
			get { return fIsNoteRead; }
			set { fIsNoteRead = value; }
		}
		bool fIsNoteRead;

		#endregion

		#region Implementation

		void UpdateNoteTextMaxLength()
		{
			if (HasMaster && !ST_IsCustomDescription)
			{
				PredefinedNoteType noteType = PredefinedNoteTypes.Instance.NoteTypeByDescription(ST_DescriptionInDatabase);
				if (noteType != null)
				{
					noteTextMaxLength = noteType.TextOnlyMaxLength;
					if (ST_IsTextOnly && ST_NoteText.Length > ST_NoteTextInfo.MaxLength)
					{
						ST_NoteText = ST_NoteText.SubstringSafe(0, ST_NoteTextInfo.MaxLength);
					}
				}
			}
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = false;
			ISecurityProxy security = EnvProxy.Instance.Security;

			if (!IsInDatabase && !ST_IsClonedForEdit)
			{
				result = !security.NotesNew.IsAllowed;
				ZPropertyInfo info = ZPropertyInfoHash[property.Name];
				if (info.Name == StmNoteSchema.ST_IsCustomDescription.Name)
				{
					result = !security.NotesNewCustomNote.IsAllowed || !security.NotesNew.IsAllowed;
				}
			}
			else
			{
				result = !security.NotesEdit.IsAllowed;
			}

			return result || CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion

		#region ICustomTextTemplateContext Members

		public string GetTextTemplateContextID(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return GetTextTemplateContextBusinessObject().TableName + ".StmNote" + (!ST_IsCustomDescription ? "." + ST_DescriptionInDatabase : string.Empty);
		}

		string ICustomTextTemplateFallbackContext.GetFallbackTextTemplateContextID(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return GetTextTemplateContextBusinessObject().TableName + ".StmNote" + (ST_IsCustomDescription ? "." + ST_DescriptionInDatabase : string.Empty);
		}

		public BusinessObject[] GetTextTemplateContextBusinessObject(object dataSource, KBindingMemberInfo bindingMemberInfo)
		{
			return new BusinessObject[] { GetTextTemplateContextBusinessObject() };
		}

		BusinessObject GetTextTemplateContextBusinessObject()
		{
			if (parentTextTemplateContext != null)
			{
				return parentTextTemplateContext;
			}
			else if (HasMaster && Master is BusinessObject)
			{
				return (BusinessObject)Master;
			}
			else
			{
				return this;
			}
		}

		BusinessObject parentTextTemplateContext;

		#endregion

		#region NoteEventReference

		ZString GetNoteEventReference()
		{
			return EventLogReferenceBuilder
				.New()
				.AddGuid(PK)
				.Build();
		}

		#endregion

		public Event AddEvent { get => AutoEvents.NoteAdded; }
		public Event DeleteEvent { get => AutoEvents.NoteDeleted; }
		public Event ModifiedEvent { get => AutoEvents.NoteModified; }
		public Event ReadEvent { get => null; }
	}

	#region StmNote Description Class

	public static class StmNoteDescription
	{
		public const string Pub = "PUB";
		public const string Int = "INT";
		public const string Prv = "PRV";
		public const string Agv = "AGV";

		public static string PubDescriptive
		{
			get { return Res.GetString("5f8800b4-a98d-49b7-b678-d5238edf69df", "CLIENT-VISIBLE"); }
		}
		public static string IntDescriptive
		{
			get { return Res.GetString("8c6f7f22-91d9-4c5d-b33c-c813f17f5e8f", "INTERNAL"); }
		}
		public static string PrvDescriptive
		{
			get { return Res.GetString("98ef0734-60ff-45c1-82d5-dbd5593e09ef", "PRIVATE"); }
		}
		public static string AgvDescriptive
		{
			get { return Res.GetString("c03da96c-163e-490e-b927-0bf5678d5116", "AGENT-VISIBLE"); }
		}
	}

	#endregion
}
