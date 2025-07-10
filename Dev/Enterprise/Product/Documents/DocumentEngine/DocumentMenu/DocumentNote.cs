using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine
{
	public class DocumentNote : HiddenStmNote, IDocumentNote, IDisposable
	{
		/// <summary>
		/// DON'T USE THIS CONSTRUCTOR. Call DocumentNote.LoadNote instead.
		/// This constructor is only here for compatibility with the BusinessObjectFactory
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DocumentNote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			mutexes = new DisposableList(5);
		}

#if DEBUG
		internal
#endif
 readonly DisposableList mutexes;

		/// <summary>
		/// Retrieves the note for this businessobject, and if not found, a new note is returned.
		/// If the DocumentNote is in use in another session then null is returned.
		/// </summary>
		public static DocumentNote LoadNoteWithExclusiveMutex(IStmNoteParent mainBusinessObject)
		{
			var mutex = GetDatabaseMutex(mainBusinessObject);
			if (mutex.HasLock || mutex.Lock())
			{
				var note = LoadNoteCore(mainBusinessObject);
				note.mutexes.Add(mutex);
				return note;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Retrieves the note for this businessobject, and if not found, a new note is returned.
		/// </summary>
		public static DocumentNote LoadNote(IStmNoteParent mainBusinessObject)
		{
			return LoadNoteCore(mainBusinessObject);
		}

		static DocumentNote LoadNoteCore(IStmNoteParent mainBusinessObject)
		{
			var note = RetrieveNote(mainBusinessObject);
			if (note == null)
			{
				note = mainBusinessObject.NotesFactory.New<DocumentNote>();
				note.ST_ParentID = mainBusinessObject.NotesParentPK;
				note.ST_Table = mainBusinessObject.NotesParentTableName;

				note.MainBusinessObject = mainBusinessObject;
				note.Master = mainBusinessObject;
				note.HasChanges = false;
			}
			return note;
		}

		static ZGlobalMutex GetDatabaseMutex(IStmNoteParent mainBusinessObject)
		{
			var mutexKey = GetMutexKey(mainBusinessObject);
			return new ZGlobalMutex(new MutexID("DocumentNote", (NoResString)"Only one user at a time is allowed to edit a Document Note"), mutexKey);
		}

		static string GetMutexKey(IStmNoteParent mainBusinessObject)
		{
			return string.Format("DocumentNote.LoadNote-{0}-{1}",
				mainBusinessObject.NotesParentTableName,
				mainBusinessObject.NotesParentPK);
		}

		/// <summary>
		/// Retrieves the note for this businessobject or return null if note is not found.
		/// </summary>
		public static DocumentNote RetrieveNote(IStmNoteParent mainBusinessObject)
		{
			if (mainBusinessObject == null)
			{
				throw new ArgumentNullException("MainBusinessObject");
			}
			if (!(mainBusinessObject is IDocumentSupportable))
			{
				throw new ArgumentException("MainBusinessObject provided for synchronisation must implement IDocumentSupportable. But it's currently a: " + mainBusinessObject.GetType().ToString());
			}

			var query = new ZQuery(StmNoteSchema.ST_ParentID, SQLComparisonOperator.Equal, mainBusinessObject.NotesParentPK);
			query.AddToFilter(StmNoteSchema.ST_NoteType, SQLComparisonOperator.Equal, "DOC");
			query.AddToFilter(StmNoteSchema.ST_Description, ZString.Empty);
			query.AddToFilter(new ZQuery(StmNoteSchema.ST_Table, mainBusinessObject.NotesParentTableName).AddToFilter(JoinCondition.Or, StmNoteSchema.ST_Table, ""));
			query.FetchOnlyFromLocalCache = !mainBusinessObject.IsInDatabase;

			DocumentNote result = null;
			foreach (var documentNote in mainBusinessObject.NotesFactory.Load<DocumentNote>(query))
			{
				if (!documentNote.ST_NoteData.ToAscii().StartsWith(@"{\rtf1"))
				{
					//See Issue00800712/WI00037279. Rich text note masquerades as document note via unknown mechanism. It never has interesting information in it, so this is OK to do.
					result = documentNote;
					using (result.SuspendSettingHasChanges())
					{
						result.MainBusinessObject = mainBusinessObject;
						result.Master = mainBusinessObject;
					}
					break;
				}
			}
			return result;
		}

		protected override StmNoteValidation GetNewValidation()
		{
			return new DocumentNoteValidation(this);
		}

		public bool UpdateSDFsAndUDFsOnFactorySaving;

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (UpdateSDFsAndUDFsOnFactorySaving || HasChanges)
			{
				UpdateUDFsFromMainBusinessObjectIfFactoryContentsChangedSinceLastUpdate();
				SaveSDFsAndUDFsToXML();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is being used for issue reporting")]
		public override void OnSaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{
				ClearUserDefinedFieldHasChanges();

				if (systemDefinedFieldWrappers != null)
				{
					if (SystemDefinedFieldWrappers.HasChanges)
					{
						foreach (var wrapper in SystemDefinedFieldWrappers)
						{
							if (wrapper.HasChanges)
							{
								wrapper.HasChanges = false;
							}
						}
						SystemDefinedFieldWrappers.HasChanges = false;
					}
				}

				if (this.HasChanges)
				{
					var message = "OnSaving should remove all outstanding changes flags, yet some still exist:\r\n";
					if (this.DoChildrenHaveChanges())
					{
						foreach (var child in ((IBusiness)this).Children)
						{
							if (child.HasChanges)
							{
								message += string.Format("A child. HumanReadableName: {0} Identifier: {1} ToString(): {2} TableName: {3}\r\n", child.HumanReadableName, child.Identifier, child.ToString(), child.TableName);
							}
						}
					}
					else
					{
						message += string.Format("The DocumentNote itself. PK: {0}", this.PK);
					}

					Globals.Message.ShowDeveloperErrorOnce("DocumentNoteOnSavingStillHasChanges", message, "Document Note still has changes");
				}
			}
		}

		public void Dispose()
		{
			mutexes.Dispose();
			mutexes.Clear();
			fDocumentCommandCollection = null;
			fUserDefinedFieldList = null;
			fAllSystemDefinedFields = null;
			foreach (var child in ((IBusiness)this).Children)
			{
				UnRegisterEditableChildObject(child);
			}
		}

		public override bool IsSavedByFactory
		{
			get
			{
				return base.IsSavedByFactory && (IsInDatabase || IsNotEmpty);
			}
		}

		internal bool IsNotEmpty
		{
			get
			{
				return
					systemDefinedFieldWrappers != null &&
					SystemDefinedFieldWrappers.Cast<StmSystemDefinedFieldWrapper>().Any(field => field.S1_Value != field.S1_DefaultValue)
					||
					fUserDefinedFieldList != null &&
					UserDefinedFieldList.Select(field => field as FilterFieldValueSerialisable).Where(field => field != null).Any(field => field.HasSerialisableValueChanged);
			}
		}

		public override bool ShouldNoteContextBeChecked
		{
			get
			{
				return false;
			}
		}

		protected override void SetDefaultValuesForOrg()
		{
			// We shouldn't SetDefaultValuesForOrg in DocumentNote.
		}

		/// <summary>
		/// Tries to set a UDF value.
		/// Returns true if it succeeds and false if it fails for any reason.
		/// </summary>
		/// <param name="fieldName">The name of the field to set</param>
		/// <param name="text">The text to set as the field's value</param>
		/// <returns></returns>
		public virtual bool SetFieldValue(string fieldName, string text)
		{
			foreach (FilterField userDefinedField in UserDefinedFieldList)
			{
				if (userDefinedField is FilterFieldValueSerialisable && userDefinedField.DisplayName == fieldName)
				{
					((FilterFieldValueSerialisable)userDefinedField).ValueAsStringForSerialisation = text;
				}
			}

			return false;
		}

		public string GetFieldValueAsString(string fieldName)
		{
			foreach (FilterField userDefinedField in UserDefinedFieldList)
			{
				if (userDefinedField is FilterFieldValueSerialisable && userDefinedField.DisplayName == fieldName)
				{
					return ((FilterFieldValueSerialisable)userDefinedField).ValueAsStringForSerialisation;
				}
			}

			return null;
		}

		public string GetSystemDefinedFieldValue(string fieldName)
		{
			foreach (StmSystemDefinedFieldWrapper fieldWrapper in SystemDefinedFieldWrappers)
			{
				if (fieldWrapper.S1_Name == fieldName)
				{
					return fieldWrapper.S1_Value;
				}
			}
			return null;
		}

		public void SetSystemDefinedFieldValue(string fieldName, string text)
		{
			foreach (StmSystemDefinedFieldWrapper fieldWrapper in SystemDefinedFieldWrappers)
			{
				if (fieldWrapper.S1_Name == fieldName)
				{
					fieldWrapper.S1_Value = text;
					break;
				}
			}
		}

		public StmSystemDefinedFieldWrapperCollection FilteredSystemDefinedFieldWrappers
		{
			get
			{
				if (filteredSystemDefinedFieldWrappers == null)
				{
					filteredSystemDefinedFieldWrappers = new StmSystemDefinedFieldWrapperCollection();
					if (MainBusinessObject == null)
					{
						throw new InvalidOperationException("Cannot Load the FilteredSystemDefinedFieldWrappers collection until the MainBusinesObject is set.");
					}
					LoadFilteredSystemDefinedFieldWrappersInto(filteredSystemDefinedFieldWrappers, SystemDefinedFieldWrappers);
				}

				return filteredSystemDefinedFieldWrappers;
			}
		}
		StmSystemDefinedFieldWrapperCollection filteredSystemDefinedFieldWrappers;

		void LoadFilteredSystemDefinedFieldWrappersInto(StmSystemDefinedFieldWrapperCollection filteredCollection, StmSystemDefinedFieldWrapperCollection unfilteredCollection)
		{
			if (MainBusinessObject == null)
			{
				throw new InvalidOperationException("Cannot Load the FilteredSystemDefinedFieldWrappers collection until the MainBusinesObject is set.");
			}
			filteredCollection.RemoveAll();

			foreach (StmSystemDefinedFieldWrapper sdFieldWrapper in unfilteredCollection)
			{
				if ((MatchesDocumentType(sdFieldWrapper) || ShowCommonFields(sdFieldWrapper) || DocumentType == AllCategories) && !HideFieldWhenValueIsEmpty(sdFieldWrapper))
				{
					filteredCollection.Add(sdFieldWrapper);
				}
			}
			filteredCollection.Sort(StmSystemDefinedFieldWrapper.Schema.S1_Order, ListSortDirection.Ascending);
		}

		void UpdateFilteredSystemDefinedFieldWrappers()
		{
			if (filteredSystemDefinedFieldWrappers != null)
			{
				LoadFilteredSystemDefinedFieldWrappersInto(filteredSystemDefinedFieldWrappers, SystemDefinedFieldWrappers);
			}
		}

		[ChildEditable]
		public StmSystemDefinedFieldWrapperCollection SystemDefinedFieldWrappers
		{
			get
			{
				if (systemDefinedFieldWrappers == null)
				{
					systemDefinedFieldWrappers = GetSystemDefinedFieldWrappers(AllSystemDefinedFields);
					RegisterEditableChildObject(systemDefinedFieldWrappers);
				}

				return systemDefinedFieldWrappers;
			}
		}
		internal StmSystemDefinedFieldWrapperCollection systemDefinedFieldWrappers;

		internal StmSystemDefinedFieldWrapperCollection GetSystemDefinedFieldWrappers(StmSystemDefinedFieldCollection allSystemDefinedFields)
		{
			StmSystemDefinedFieldWrapperCollection wrappers;
			if (MainBusinessObject == null)
			{
				throw new InvalidOperationException("Cannot Load SystemDefinedFieldWrappers collection until MainBusinessObject is set.");
			}
			wrappers = new StmSystemDefinedFieldWrapperCollection();
			foreach (StmSystemDefinedField sdField in allSystemDefinedFields)
			{
				wrappers.Add(new StmSystemDefinedFieldWrapper(sdField));
			}
			LoadSDFsFromXML(wrappers);
			return wrappers;
		}

		public UserControlProviderList GetCompleteFieldList(DocumentNoteCache noteCache)
		{
			Argument.NotNull(noteCache, "noteCache");

			var businessContext = ((IDocumentSupportable)MainBusinessObject).DocumentSupporter.BusinessContext;
			UserControlProviderListCacheItem cacheItem;
			UserControlProviderList systemDefinedFieldList;

			if (noteCache.TryGetValue(businessContext, out cacheItem))
			{
				cacheItem.UserDefined.ClearFieldValues();
				LoadUDFsFromXML(cacheItem.UserDefined);

				systemDefinedFieldList = GetSystemDefinedFieldList(cacheItem.SystemDefinedFieldCollection);
			}
			else
			{
				systemDefinedFieldList = SystemDefinedFieldList;
				cacheItem = new UserControlProviderListCacheItem(AllSystemDefinedFields, UserDefinedFieldList);
				noteCache.Add(businessContext, cacheItem);
			}

			return GetCompleteFieldList(systemDefinedFieldList, cacheItem.UserDefined);
		}

		public UserControlProviderList GetCompleteFieldList()
		{
			return GetCompleteFieldList(SystemDefinedFieldList, UserDefinedFieldList);
		}

		UserControlProviderList GetCompleteFieldList(UserControlProviderList systemDefinedFieldList, UserControlProviderList userDefinedFieldList)
		{
			var result = new UserControlProviderList();
			result.AddRange(systemDefinedFieldList);

			foreach (FilterField userDefinedField in userDefinedFieldList)
			{
				if (!result.ContainsName(userDefinedField.DisplayName))
				{
					result.Add(userDefinedField);
				}
			}

			return result;
		}

		public UserControlProviderList GetSystemDefinedFieldList() // BG: I think this should be coming from the FULL list. Check it.
		{
			var wrappers = FilteredSystemDefinedFieldWrappers;
			return GetSystemDefinedFieldList(wrappers);
		}

		internal UserControlProviderList GetSystemDefinedFieldList(StmSystemDefinedFieldCollection allSystemDefinedFieldCollection)
		{
			var filteredWrappers = new StmSystemDefinedFieldWrapperCollection();
			var unfilteredWrappers = GetSystemDefinedFieldWrappers(allSystemDefinedFieldCollection);
			LoadFilteredSystemDefinedFieldWrappersInto(filteredWrappers, unfilteredWrappers);
			return GetSystemDefinedFieldList(filteredWrappers);
		}

		UserControlProviderList GetSystemDefinedFieldList(StmSystemDefinedFieldWrapperCollection systemDefinedFieldWrapperCollection)
		{
			var result = new UserControlProviderList();
			foreach (StmSystemDefinedFieldWrapper field in systemDefinedFieldWrapperCollection)
			{
				var textFieldProvider = new TextField(Factory);
				textFieldProvider.DisplayName = field.S1_NameFromDatabase;
				textFieldProvider.ValueAsStringForSerialisation = field.S1_Value;
				result.Add(textFieldProvider);
			}
			return result;
		}

		List<UserControlProviderList> GetListOfUDFListsFromAllDocumentCommands(IDocumentSupportable mainBusinessObject)
		{
			var result = new List<UserControlProviderList>();

			fDocumentCommandCollection = new DocumentCommandCollection(mainBusinessObject, new BusinessObjectFactory());
			fDocumentCommandCollection.Load();
			if (DocumentCommandCollection.Count > 0)
			{
				var pivotPKs = new List<ZGuid>();

				foreach (DocumentCommand documentCommand in DocumentCommandCollection.GetApplicableDocumentCommands())
				{
					if (!pivotPKs.Contains(documentCommand.PK))
					{
						pivotPKs.Add(documentCommand.PK);
					}
				}

#if DEBUG
				IsGetApplicalbeDocumentsCommandsCalled = DocumentCommandCollection.FilterEvaluatedResult.Keys.Count > 0;
#endif

				var pivotQuery = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, pivotPKs);
				var pivots = Factory.Load<StmMenuTemplatePivotBase>(pivotQuery);

				var templatePKs = new List<string>();
				foreach (var pivot in pivots)
				{
					var pk = pivot.SI_SO.ToString();
					if (!templatePKs.Contains(pk))
					{
						templatePKs.Add(pk);
					}
				}

				if (templatePKs.Count > 0)
				{
					var templates = GetTemplates(templatePKs);

					foreach (DynamicBusinessObject template in templates)
					{
						var cachedUDF = (ZBlob)template[StmTemplateSchema.SO_UDFFieldCache];
						if (cachedUDF == null || cachedUDF.IsEmpty)
						{
							cachedUDF = Factory.Load<StmTemplateBase>((ZGuid)template[StmTemplateSchema.PK]).GetUpToDateUDFFieldCacheValue();
						}

						var rootNode = StringTreeNode.Deserialise(cachedUDF);

						var udfCollectionBuilder =
							new UserDefinedFieldCollectionBuilder(rootNode, (ZString)template[StmTemplateSchema.SO_DataContext], new ValidatorPack(), DummyEvaluator);
						udfCollectionBuilder.Build();

						udfCollectionBuilder.UserDefinedFields.OfType<FilterField>().ForEach(f => f.TemplateName = template[StmTemplateSchema.SO_Name].ToString());

						result.Add(udfCollectionBuilder.UserDefinedFields);
					}
				}
			}

			return result;
		}

#if DEBUG
		internal bool IsGetApplicalbeDocumentsCommandsCalled;
#endif

		DynamicBusinessObjectCollection GetTemplates(List<string> templatePKs)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var templatePKsParam = string.Join("', '", templatePKs.ToArray());
			var sql = string.Format(
@"select {1}, {2}, {3}, {4}
from {0}
where {1} in ('{5}')
and {4} not like @SystemDocBuilderTemplate
and {4} not like @UserDocBuilderTemplate",
				StmTemplateSchema.Constants.TableName,
				StmTemplateSchema.PK.Name, StmTemplateSchema.SO_UDFFieldCache.Name, StmTemplateSchema.SO_DataContext.Name, StmTemplateSchema.SO_Name.Name,
				templatePKsParam);

			result.Load(sql, new[]
			{
				ZSqlParameter.New("@SystemDocBuilderTemplate", SectionRepositoryTemplateNames.System + "%", StmTemplateSchema.SO_Name),
				ZSqlParameter.New("@UserDocBuilderTemplate", SectionRepositoryTemplateNames.User + "%", StmTemplateSchema.SO_Name)
			});

			return result;
		}

		public virtual string DummyEvaluator(Match match)
		{
			return match.Value;
		}

		object IDocumentNote.MainBusinessObject
		{
			get { return MainBusinessObject; }
			set { MainBusinessObject = value as IStmNoteParent; }
		}

		internal IStmNoteParent MainBusinessObject
		{
			get { return fMainBusinessObject; }
			set
			{
				fMainBusinessObject = value;
				if (value != null)
				{
					ST_Table = value.NotesParentTableName;
				}
			}
		}
		IStmNoteParent fMainBusinessObject;

		internal UserControlProviderList SystemDefinedFieldList
		{
			get { return fSystemDefinedFieldList ?? (fSystemDefinedFieldList = GetSystemDefinedFieldList()); }
		}
		internal UserControlProviderList fSystemDefinedFieldList;

		public UserControlProviderList UserDefinedFieldList
		{
			get { return fUserDefinedFieldList ?? (fUserDefinedFieldList = GetNewUserDefinedFieldList(MainBusinessObject)); }
		}
		internal UserControlProviderList fUserDefinedFieldList;

		UserControlProviderList GetNewUserDefinedFieldList(IStmNoteParent mainBusinessObject)
		{
			if (mainBusinessObject == null)
			{
				throw new InvalidOperationException("Cannot Load the FieldList collection until the MainBusinessObject is set.");
			}

			var userDefinedFieldListForACommands = GetListOfUDFListsFromAllDocumentCommands((IDocumentSupportable)mainBusinessObject);

			var result = new UserControlProviderList();
			foreach (var userDefinedFieldListForACommand in userDefinedFieldListForACommands)
			{
				foreach (FilterField userDefinedField in userDefinedFieldListForACommand)
				{
					if (!result.ContainsName(userDefinedField.DisplayName))
					{
						result.Add(userDefinedField);
						RegisterEditableChildObject(userDefinedField);
					}
				}
			}
			LoadUDFsFromXML(result);

			result.OfType<FilterField>().ForEach(f => f.Validators.Add(FieldNameCaseValidator));

			return result;
		}

		FieldNameCaseValidator FieldNameCaseValidator => fieldNameCaseValidator ?? (fieldNameCaseValidator = new FieldNameCaseValidator(this));
		FieldNameCaseValidator fieldNameCaseValidator;

		public void RemoveUnnecessaryUDFValidators()
		{
			UserDefinedFieldList.OfType<FilterField>().Where(f => !f.HasErrors).ForEach(f => f.Validators.Remove(FieldNameCaseValidator));
		}

		internal StmSystemDefinedFieldCollection AllSystemDefinedFields
		{
			get
			{
				if (MainBusinessObject == null)
				{
					throw new InvalidOperationException("Cannot Load AllSystemDefinedFields collection until MainBusinessObject is set.");
				}
				return fAllSystemDefinedFields ?? (fAllSystemDefinedFields = GetAllSystemDefinedFields(((IDocumentSupportable)MainBusinessObject).DocumentSupporter.BusinessContext, MainBusinessObject.NotesFactory));
			}
		}
		StmSystemDefinedFieldCollection fAllSystemDefinedFields;
		StmSystemDefinedFieldCollection GetAllSystemDefinedFields(BusinessContext businessContext, BusinessObjectFactory factory)
		{
			var result = new StmSystemDefinedFieldCollection(factory);
			result.Load(businessContext, GlbCompany.CurrentCompany.Country);
			return result;
		}

		bool MatchesDocumentType(StmSystemDefinedFieldWrapper field)
		{
			return fDocumentType.IsEmpty || fDocumentType.EqualsIgnoringCase(field.S1_Category);
		}

		bool ShowCommonFields(StmSystemDefinedFieldWrapper field)
		{
			return IsCommonCategory(field.S1_CategoryFromDatabase) && fAlwaysShowCommonDocuments;
		}

		bool IsCommonCategory(ZString category)
		{
			return category.EqualsIgnoringCase("COMMON");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant String")]
		const string LetterOfCreditNumber = "Letter of Credit Number";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant String")]
		const string LetterOfCreditDate = "Letter of Credit Date";

		bool HideFieldWhenValueIsEmpty(StmSystemDefinedFieldWrapper field)
		{
			//The two fields LetterOfCreditNumber & LetterOfCreditDate can be removed after '1 jan 2024'.
			var fieldName = field.S1_NameFromDatabase;
			return (fieldName == LetterOfCreditNumber || fieldName == LetterOfCreditDate) && field.S1_Value.IsEmpty && IsCommonCategory(field.S1_Category);
		}

		public CodeDescriptionPairList DocumentTypes
		{
			get { return fDocumentTypes ?? (fDocumentTypes = GetNewDocumentTypesList()); }
		}
		CodeDescriptionPairList fDocumentTypes;
		CodeDescriptionPairList GetNewDocumentTypesList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(ResString.GetMultilingualString("166f2b6e-3637-4fad-9c83-fe3329901dd1", "ALL"));
			foreach (StmSystemDefinedFieldWrapper wrapper in SystemDefinedFieldWrappers)
			{
				if (!result.ContainsCode(wrapper.S1_Category) && !IsCommonCategory(wrapper.S1_CategoryFromDatabase))
				{
					result.AddPair(wrapper.S1_Category);
				}
			}
			return result;
		}
		const string AllCategories = "ALL";

		#region DocumentType Field
		[MaxLength(StmSystemDefinedField.Schema.S1_CategoryMaxLength)]
		public ZString DocumentType
		{
			get { return fDocumentType; }
			set
			{
				CheckMaximumLength(DocumentTypeInfo, value);
				fDocumentType = value;
				UpdateFilteredSystemDefinedFieldWrappers();
				DocumentTypeInfo.RefreshBinding();
			}
		}
		ZString fDocumentType = AllCategories;

		public DocumentCommandCollection DocumentCommandCollection
		{
			get
			{
				if (fDocumentCommandCollection == null)
				{
					GetNewUserDefinedFieldList(MainBusinessObject);
				}
				return fDocumentCommandCollection;
			}
		}
		DocumentCommandCollection fDocumentCommandCollection;

		public ZPropertyInfo DocumentTypeInfo
		{
			get { return GetZPropertyInfo(nameof(DocumentType)); }
		}
		#endregion

		#region AlwaysShowCommonDocuments Field
		public ZBool AlwaysShowCommonDocuments
		{
			get { return fAlwaysShowCommonDocuments; }
			set
			{
				fAlwaysShowCommonDocuments = value;
				UpdateFilteredSystemDefinedFieldWrappers();
				AlwaysShowCommonDocumentsInfo.RefreshBinding();
			}
		}
		ZBool fAlwaysShowCommonDocuments = true;

		public ZPropertyInfo AlwaysShowCommonDocumentsInfo
		{
			get { return GetZPropertyInfo(nameof(AlwaysShowCommonDocuments)); }
		}
		#endregion

		internal void SaveSDFsAndUDFsToXML()
		{
			using (var data = new DataSet())
			{
				var table = new DataTable((NoResString)"Doc");
				var nameColumn = new DataColumn((NoResString)"Name", typeof(string));
				var valueColumn = new DataColumn((NoResString)"Value", typeof(string));
				table.Columns.Add(nameColumn);
				table.Columns.Add(valueColumn);
				table.PrimaryKey = new DataColumn[] { nameColumn };
				data.Tables.Add(table);

				foreach (FilterField userDefinedField in UserDefinedFieldList)
				{
					var serialisableField = userDefinedField as FilterFieldValueSerialisable;
					if (serialisableField != null && serialisableField.HasSerialisableValueChanged)
					{
						var newRow = table.NewRow();
						newRow[nameColumn] = serialisableField.DisplayName;
						newRow[valueColumn] = serialisableField.ValueAsStringForSerialisation;
						userDefinedField.IsOverriddenInDocData = true;
						table.Rows.Add(newRow);
					}
				}

				foreach (StmSystemDefinedFieldWrapper field in SystemDefinedFieldWrappers)
				{
					if (field.S1_Value != field.S1_DefaultValue)
					{
						var foundRow = data.Tables[0].Rows.Find(field.S1_NameFromDatabase);
						if (foundRow != null && foundRow["Value"] != null && foundRow["Value"] != DBNull.Value)
						{
							foundRow[valueColumn] = field.S1_Value;
						}
						else
						{
							var newRow = table.NewRow();
							newRow[nameColumn] = field.S1_NameFromDatabase;
							newRow[valueColumn] = field.S1_Value;
							table.Rows.Add(newRow);
						}
					}
					field.HasChanges = false;
				}

				using (var xmlStream = new MemoryStream())
				{
					data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);

					var newNoteData = new ZBlob(xmlStream.ToArray());
					if (ST_NoteData.Equals(newNoteData))
					{
						ClearUserDefinedFieldHasChanges();
					}
					else
					{
						ST_NoteData = newNoteData;
					}
				}
			}
		}

		void ClearUserDefinedFieldHasChanges()
		{
			if (fUserDefinedFieldList != null)
			{
				foreach (var userDefinedField in UserDefinedFieldList)
				{
					var serialisableField = userDefinedField as FilterFieldValueSerialisable;
					if (serialisableField != null)
					{
						serialisableField.HasChanges = false;
					}
				}
			}
		}

		internal void LoadSDFsFromXML(StmSystemDefinedFieldWrapperCollection sdFieldWrapperCollection)
		{
			if (!ST_NoteData.IsEmpty)
			{
				using (var xmlStream = new MemoryStream(ST_NoteData))
				using (var sdfData = new DataSet())
				{
					try
					{
						sdfData.ReadXml(xmlStream, XmlReadMode.Auto);
					}
					catch (XmlException exception)
					{
						var message = string.Format(
							(NoResString)"Read XML failed when load SDFs from XML for document note: PK = [{0}], Note Data = [{1}]",
							PK, ST_NoteData.ToUTF8().ToString().ShrinkToMaxLength(1024));
						ErrorReporter.ReportOnce("DocumentNote.LoadSDFsFromXML_XmlException", message, exception);
					}

					if (sdfData.Tables.Count == 1)
					{
						var sdfTable = sdfData.Tables[0];
						sdfTable.PrimaryKey = new DataColumn[] { sdfTable.Columns["Name"] };

						foreach (StmSystemDefinedFieldWrapper sdFieldWrapper in sdFieldWrapperCollection)
						{
							var rowWithMatchingName = sdfTable.Rows.Find(sdFieldWrapper.S1_NameFromDatabase);
							if (rowWithMatchingName != null)
							{
								var matchingRowValue = rowWithMatchingName["Value"];
								if (matchingRowValue != null && matchingRowValue != DBNull.Value)
								{
									using (sdFieldWrapper.SuspendSettingHasChanges())
									{
										sdFieldWrapper.S1_Value = matchingRowValue.ToString();
									}
								}
							}
						}
					}
				}
			}
		}

		void ReloadUDFsFromXMLIfNeeded()
		{
			if (fUserDefinedFieldList != null)
			{
				LoadUDFsFromXML(fUserDefinedFieldList);
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			ReloadUDFsFromXMLIfNeeded();
		}

		internal void LoadUDFsFromXML(UserControlProviderList userDefinedFieldList)
		{
			if (!ST_NoteData.IsEmpty)
			{
				using (var xmlStream = new MemoryStream(ST_NoteData))
				using (var sdfData = new DataSet())
				{
					try
					{
						sdfData.ReadXml(xmlStream, XmlReadMode.Auto);
					}
					catch (XmlException exception)
					{
						var message = string.Format(
							(NoResString)"Read XML failed when load UDFs from XML for document note: PK = [{0}], Note Data = [{1}]",
							PK, ST_NoteData.ToUTF8().ToString().ShrinkToMaxLength(1024));
						ErrorReporter.ReportOnce("DocumentNote.LoadUDFsFromXML_XmlException", message, exception);
					}

					if (sdfData.Tables.Count == 1)
					{
						var sdfTable = sdfData.Tables[0];

						if (!sdfTable.Columns.Contains((NoResString)"Name") && userDefinedFieldList.Any())
						{
							var message =
								string.Format(CultureInfo.InvariantCulture,
									(NoResString)"There's no column \"Name\" in loaded DataTable for document note: PK = [{0}], Note Data = [{1}], Parent Table = [{2}], Description = [{3}], Note Text = [{4}], Note Type = [{5}], Parent BO = [{6}]",
									PK, ST_NoteData.ToUTF8(), ST_Table, ST_Description, ST_NoteText, ST_NoteType,
									(MainBusinessObject as BusinessObject)?.HumanReadableName);
							ErrorReporter.ReportOnce("NoColumnNameInLoadedDataTableForDocumentNote", message);
							return;
						}

						sdfTable.PrimaryKey = new[] { sdfTable.Columns["Name"] };

						foreach (FilterField userDefinedField in userDefinedFieldList)
						{
							var serialisableField = userDefinedField as FilterFieldValueSerialisable;
							if (serialisableField != null)
							{
								var rowWithMatchingName = sdfTable.Rows.Find(serialisableField.DisplayName);
								if (rowWithMatchingName != null)
								{
									var matchingRowValue = rowWithMatchingName["Value"];
									if (matchingRowValue != null && matchingRowValue != DBNull.Value)
									{
										serialisableField.SetValueFromXML(matchingRowValue.ToString());
										serialisableField.IsOverriddenInDocData = true;
									}
								}
							}
						}
					}
				}
			}
		}

		public void UpdateUDFsFromMainBusinessObjectIfFactoryContentsChangedSinceLastUpdate()
		{
			if (MainBusinessObject != null && MainBusinessObject.NotesFactory.CacheVersion != factoryVersion)
			{
				factoryVersion = MainBusinessObject.NotesFactory.CacheVersion;
				UpdateUDFsFromMainBusinessObjectInternal();
			}
		}
		int factoryVersion;

		internal void UpdateUDFsFromMainBusinessObjectInternal()
		{
			var documentSupportableBO = MainBusinessObject as IDocumentSupportable ?? throw new InvalidOperationException("Cannot UpdateUDFsFromMainBusinessObject() until MainBusinessObject is set. It must also be IDocumentSupportable.");
			var dummyReportForGettingDefaultsCache = new Dictionary<string, Report>();
			using (Factory.GetDocWrapperContextManager().SuspendContextSetCheck())
			using (Factory.GetDocWrapperContextManager().ClearMenuContextValuesTemporarily())
			{
				try
				{
					foreach (FilterField filterField in UserDefinedFieldList)
					{
						var serialisedField = filterField as FilterFieldValueSerialisable;
						if (serialisedField != null &&
							!serialisedField.HasSerialisableValueChanged &&
							serialisedField.DataContextValue != null &&
							serialisedField.DataContextValue.DataContext != Core.Constants.DataContext.ERA &&
							serialisedField.DataContextValue.DataContext != Core.Constants.DataContext.IMO)
						{
							var dummyReportCacheKey = serialisedField.DataContextValue.FullDataContext;
							Report dummyReportForGettingDefaults;
							if (!dummyReportForGettingDefaultsCache.TryGetValue(dummyReportCacheKey, out dummyReportForGettingDefaults))
							{
								var dataProviders = documentSupportableBO.DocumentSupporter.GetBODocDataProviders(serialisedField.DataContextValue, null);
								if (dataProviders != null && dataProviders.Length > 0)
								{
									var dataProvider = dataProviders[0];
									var tempPack = new DocumentPack();
									dummyReportForGettingDefaults = new Report(tempPack, null, dataProvider, "", UserDefinedFieldList, DocumentDirection.ANY, false);
									dummyReportForGettingDefaults.ResetDataProvider();
									dummyReportForGettingDefaults.RegisterDocumentAndReportRelatedMacroProviders();
								}
								dummyReportForGettingDefaultsCache[dummyReportCacheKey] = dummyReportForGettingDefaults;
							}
							if (dummyReportForGettingDefaults != null)
							{
								serialisedField.UpdateValueIfNotOverriddenByUser(dummyReportForGettingDefaults);
							}
						}
					}
				}
				finally
				{
					foreach (var report in dummyReportForGettingDefaultsCache.Values)
					{
						if (report != null)
						{
							report.Dispose();
						}
					}
				}
			}
		}
	}
}
