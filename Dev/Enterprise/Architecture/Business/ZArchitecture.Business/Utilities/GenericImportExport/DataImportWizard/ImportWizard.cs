using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Utilities;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ImportWizard : ImportExportWizard, IDisposable
	{
		public ImportWizard(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
			: base(GetFactory(collectionInfo), settingsStorage, fileMapper)
		{
			this.collectionInfo = collectionInfo;
			if (collectionInfo.ValidateAndSave)
			{
				RegisterEditableChildObject(collectionInfo.Collection);
			}
		}

		static BusinessObjectFactory GetFactory(IImportCollectionInfo collectionInfo)
		{
			return collectionInfo.ValidateAndSave ? collectionInfo.Collection.Factory : null;
		}

		#region Properties

		#region CollectionInfo

		public override IEnumerable<IImportPropertyInfo> CollectionInfoProperties { get { return CollectionInfo.Properties; } }

		public IImportCollectionInfo CollectionInfo
		{
			get { return collectionInfo; }
		}
		readonly IImportCollectionInfo collectionInfo;

		#endregion

		#region FileContent

		public List<string[]> FileContent
		{
			get { return _fileContent; }
			protected set { _fileContent = value; }
		}

		List<string[]> _fileContent = new List<string[]>();

		protected override void RefreshFileContent(bool force)
		{
			if ((force || FileContent.Count > 0) && !HasErrors)
			{
				RefreshFileContentCore(force);
			}
		}

		void RefreshFileContentCore(bool force)
		{
			FileContent = LoadFile(1, Math.Max(MaxPreviewLines, StartingRow + 20), force);
			FileColumns.Clear();
			foreach (string[] values in FileContent)
			{
				for (int i = 0; i < values.Length; i++)
				{
					if (i >= FileColumns.Count)
					{
						FileColumns.Add(0);
					}
					FileColumns[i] = Math.Max(FileColumns[i], values[i].Length);
				}
			}

			RefreshBinding();
		}

		#endregion

		#region FileColumns

		[BusinessObjectTestExclude]
		public List<int> FileColumns
		{
			get { return fileColumns; }
		}

		readonly List<int> fileColumns = new List<int>();

		#endregion

		#region FileName

		protected override void ValidateFileName()
		{
			base.ValidateFileName();

			if (loadFileException != null)
			{
				FileNameInfo.AddError(loadFileException);
				loadFileException = null;
			}
			else if (!FileName.IsEmpty && !(fileMapper.IsRemote || File.Exists(FileName)))
			{
				FileNameInfo.AddError(Res.GetString("1c05afdf-2a09-4bdc-bcda-8031c82d66c3", "File doesn't exist."));
			}
		}

		#endregion

		#region StartingRow

		public ZInt StartingRow
		{
			get { return startingRow; }
			set
			{
				if (startingRow != value)
				{
					SetNonPersistentPropertyValue(StartingRowInfo, ref startingRow, value);

					if (!IsValidationSuspended)
					{
						ValidateStartingRow();
						RefreshFileContent(false);
					}
				}
			}
		}

		ZInt startingRow = 1;

		public ZPropertyInfo StartingRowInfo
		{
			get { return GetZPropertyInfo(nameof(StartingRow)); }
		}

		void ValidateStartingRow()
		{
			StartingRowInfo.ClearAllNotifications();
		}

		#endregion

		#region UseCurrentCountryNumberFormatting 

		public ZBool UseCurrentCountryNumberFormatting
		{
			get; set;
		}

		#endregion

		#region Mapping

		public ImportWizardMappingCollection Mapping
		{
			get
			{
				if (mapping == null)
				{
					mapping = new ImportWizardMappingCollection(this);
					mapping.Load();
					RegisterEditableChildObject(mapping);
				}

				return mapping;
			}
		}

		ImportWizardMappingCollection mapping;

		#endregion

		#region Preview

		public void LoadPreview()
		{
			ValidateMappings();
			Preview.SetCountedReadOnlyIncludingChildren(false);
			try
			{
				ImportIntoCollection(Preview, MaxPreviewLines);
			}
			finally
			{
				Preview.SetCountedReadOnlyIncludingChildren(true);
			}
		}

		public ImportWizardPreviewLineCollection Preview
		{
			get
			{
				if (preview == null)
				{
					preview = new ImportWizardPreviewLineCollection(CollectionInfo.Properties, BindToLists);
					preview.SetCountedReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(preview);
				}

				return preview;
			}
		}
		ImportWizardPreviewLineCollection preview;

		public Dictionary<string, IList> BindToLists
		{
			get
			{
				if (bindToLists == null)
				{
					IBindingList list = CollectionInfo.Collection;
					BusinessObject uncommittedBO = null;
					if (list != null)
					{
						uncommittedBO = (list.Count > 0 ? list[0] : list.AddNew()) as BusinessObject;
					}
					try
					{
						Dictionary<string, IList> lists = new Dictionary<string, IList>();
						foreach (IImportPropertyInfo property in CollectionInfo.Properties)
						{
							IList bindToList = property.GetBindToList(uncommittedBO);
							if (bindToList != null)
							{
								lists.Add(property.MappingName, bindToList);
							}
						}
						bindToLists = lists;
					}
					finally
					{
						if (list != null)
						{
							((ICancelAddNew)list).CancelNew(CollectionInfo.Collection.Count - 1);
						}
					}
				}

				return bindToLists;
			}
		}
		Dictionary<string, IList> bindToLists;

		#endregion

		#region CurrentFileContentLine

		public int CurrentFileContentLine
		{
			get { return currentFileContentLine; }
			set { currentFileContentLine = value; }
		}

		int currentFileContentLine;

		public string GetFileContentValue(int index)
		{
			string result = "";
			if (CurrentFileContentLine < FileContent.Count)
			{
				string[] values = FileContent[CurrentFileContentLine];
				if (index < values.Length)
				{
					result = values[index];
				}
			}

			return result;
		}

		#endregion

		#region ProperCase

		readonly ProperCaseConverter properCaseConverter = new ProperCaseConverter();
		public string ToProperCase(string text)
		{
			return properCaseConverter.Convert(text, ProperCaseExcludeListCache, StringComparer.OrdinalIgnoreCase);
		}

		ProperCaseExcludeWordCollection properCaseExcludeList;
		public ProperCaseExcludeWordCollection ProperCaseExcludeList
		{
			get
			{
				if (properCaseExcludeList == null)
				{
					properCaseExcludeList = new ProperCaseExcludeWordCollection(Factory, ProperCaseExcludeListCache);
					properCaseExcludeList.CountChanged += properCaseExcludeList_CountChanged;
				}
				return properCaseExcludeList;
			}
		}

		string[] properCaseExcludeListCache;
		string[] ProperCaseExcludeListCache
		{
			get { return properCaseExcludeListCache ?? (properCaseExcludeListCache = RawDataRegistry.Instance.ProperCaseExcludeList.Value); }
			set
			{
				properCaseExcludeListCache = value;
				RawDataRegistry.Instance.ProperCaseExcludeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
		}

		void properCaseExcludeList_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			ProperCaseExcludeListCache = ProperCaseExcludeList.Select(x => x.Word.ToString()).ToArray();
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			CleanUpPreview(); //just so we don't trigger validation errors on existing rows spuriously

			base.RunPreSaveValidationCore();

			ValidateStartingRow();
			ValidateMappings();
		}

		void ValidateMappings()
		{
			Mapping.RunPreSaveValidation();
		}

		#endregion

		#region LoadFile

#if DEBUG
		public virtual
#endif
		List<string[]> LoadFile(int startingRow, int maximumRows, bool forceFileLoad = false)
		{
			loadFileException = null;
			try
			{
				Stream stream = GetFileStream(forceFileLoad);

				if (string.Compare(Path.GetExtension(FileName), ".xls", true) == 0)
				{
					return LoadXls(stream, startingRow, maximumRows);
				}
				else
				{
					return LoadCsv(stream, startingRow, maximumRows);
				}
			}
			catch (Exception ex) when (ex is IOException || ex is SystemException || ex is ApplicationException)
			{
				loadFileException = ex.Message;
				if (ex.InnerException != null)
				{
					loadFileException += System.Environment.NewLine + ex.InnerException.Message;
				}

				ValidateFileName();
			}

			return new List<string[]>();
		}

		Stream currentFileStream;
#if DEBUG
		public Stream CurrentFileStream
		{
			get { return currentFileStream; }
			set { currentFileStream = value; }
		}

		public
#endif
		Stream GetFileStream(bool forceFileLoad)
		{
			if (forceFileLoad || currentFileStream == null)
			{
				using (Stream stream = fileMapper.OpenRead(FileName))
				{
					if (currentFileStream != null)
					{
						currentFileStream.Dispose();
					}
					currentFileStream = new VirtualMemoryStream();
					stream.CopyTo(currentFileStream);
				}
			}

			currentFileStream.Position = 0;
			return currentFileStream;
		}

		#region Xls

		List<string[]> LoadXls(Stream stream, int startingRow, int maximumRows)
		{
			// Needed to remove literals ("Text") and format fields ([Red]) from an Excel format string
			Regex dateFormatRe = new Regex(@"(?> (?:\[[^\[\]]*\]) | (?:""[^""]*"") )", RegexOptions.IgnorePatternWhitespace);

			List<string[]> result = new List<string[]>();
			using (IExcelInterface excelDoc = ExcelInterfaceFactory.New())
			{
				excelDoc.LoadExcelFile(stream);
				IExcelWorkSheet worksheet = excelDoc.WorkSheets[0];

				int maxRow = worksheet.RowCount;
				int maxCol = worksheet.ColumnCount;

				startingRow = startingRow > 0 ? startingRow - 1 : 0;
				if (maximumRows > 0)
				{
					maximumRows = Math.Min(maxRow, startingRow + maximumRows);
				}
				else
				{
					maximumRows = maxRow;
				}

				for (int i = startingRow; i < maximumRows; i++)
				{
					string[] values = new string[maxCol];
					for (int j = 0; j < maxCol; j++)
					{
						object cellValue = worksheet[i, j];
						if (cellValue is double)
						{
							CellFormat format = worksheet.GetCellFormat(i, j);
							if (!string.IsNullOrEmpty(format.FormatPattern))
							{
								string[] formatStrings = dateFormatRe.Split(format.FormatPattern);
								if (Array.Exists(formatStrings, s => s.IndexOfAny(new char[] { 'y', 'm', 'd', 'h', 's', 'Y', 'M', 'D', 'H', 'S' }) >= 0))
								{
									cellValue = DateTime.FromOADate((double)cellValue);
								}
							}
						}

						if (cellValue is double)
						{
							values[j] = ((double)cellValue).ToString("0.######", CultureInfo.InvariantCulture);
						}
						else
						{
							values[j] = cellValue.ToString().Trim();
						}
					}
					if (Array.Exists(values, value => !string.IsNullOrEmpty(value)))
					{
						result.Add(values);
					}
				}
			}

			return result;
		}

		#endregion

		#region Csv

		List<string[]> LoadCsv(Stream stream, int startingRow, int maximumRows)
		{
			Regex re = null;
			List<string[]> result = new List<string[]>();

			var interner = new StringInterner();
			StreamReader reader = new StreamReader(stream, true);
			startingRow = startingRow > 0 ? startingRow - 1 : 0;
			if (maximumRows > 0)
			{
				maximumRows = startingRow + maximumRows;
			}

			for (int i = 0; i < maximumRows || maximumRows < 0; i++)
			{
				string line = GetNextLine(reader);

				if (line == null)
				{
					break;
				}

				if (re == null)
				{
					if (!DelimiterWasSet)
					{
						Delimiter = OCsvLine.AutoDetectDelimiter(line).ToString();
					}
					string escapedDelimiter;
					switch (Delimiter.ToLower())
					{
						case SupportedDelimiter.Tab:
							escapedDelimiter = @"\t";
							break;

						case SupportedDelimiter.Space:
							escapedDelimiter = @"\x20";
							break;

						default:
							escapedDelimiter = Regex.Escape(Delimiter);
							break;
					}

					string pattern = string.Format("{0}(?=([^{1}]*{1}[^{1}]*{1})*(?![^{1}]*{1}))", escapedDelimiter, Regex.Escape(TextQualifier));
					re = new Regex(pattern, RegexOptions.Compiled);
				}

				if (i < startingRow)
				{
					continue;
				}

				MatchCollection matches = re.Matches(line);
				string[] values = new string[matches.Count + 1];
				bool emptyRow = true;
				int v = 0;
				foreach (string value in GetValues(line, matches))
				{
					values[v++] = interner.InternValue(value);
					if (emptyRow && !string.IsNullOrWhiteSpace(value))
					{
						emptyRow = false;
					}
				}

				if (!emptyRow)
				{
					result.Add(values);
				}
			}

			return result;
		}

		IEnumerable GetValues(string s, MatchCollection matches)
		{
			int prevIndex = 0;
			foreach (Match m in matches)
			{
				yield return SubstringWithTextQualifier(s, prevIndex, m.Index - prevIndex);
				prevIndex = m.Index + m.Length;
			}
			yield return SubstringWithTextQualifier(s, prevIndex, s.Length - prevIndex);
		}

		string SubstringWithTextQualifier(string s, int start, int length)
		{
			if (length >= 2 && s[start] == TextQualifier[0] && s[start + length - 1] == TextQualifier[0])
			{
				start++;
				length -= 2;
			}

			string result = s.Substring(start, length);
			return result.Replace(TextQualifier + TextQualifier, TextQualifier);
		}

		public static string GetNextLine(StreamReader sr)
		{
			string currentLine;
			do
			{
				currentLine = sr.ReadLine();
			}
			while (string.IsNullOrWhiteSpace(currentLine) && !sr.EndOfStream);

			if (!string.IsNullOrWhiteSpace(currentLine))
			{
				bool openQuote = currentLine.CountMatches('"') % 2 != 0;
				if (openQuote)
				{
					StringBuilder sb = new StringBuilder(currentLine);
					while (openQuote)
					{
						if (!sr.EndOfStream)
						{
							string nextLine = sr.ReadLine();
							sb.Append(System.Environment.NewLine).Append(nextLine);
							openQuote = sb.ToString().CountMatches('"') % 2 != 0;
						}
						else
						{
							// we made it to the end of the file without closing the quotes!! Invalid file.
							openQuote = false;
						}
					}
					currentLine = sb.ToString();
				}
			}
			else
			{
				// return null if it was whitespace
				currentLine = null;
			}

			return currentLine;
		}

		#endregion

		#endregion

		#region ImportIntoCollection

		void CleanUpPreview()
		{
			if (preview == null)
			{ return; }
			foreach (var bizo in Preview.ToArray())
			{
				if (CollectionInfo.Collection.Contains(bizo))
				{
					Preview.Remove(bizo);
					((IBusinessObjectState)bizo).DecrementReadOnlyIncludingChildren(); //have to do this manually because when we added it to the readonly collection Preview it got incremented once
				}
				else
				{
					Preview.RemoveAndDelete(bizo);
				}
			}
		}

		public IEnumerable<ImportWizardMapping> MatchForUpdateColumns => Mapping.OfType<ImportWizardMapping>().Where(x => x.IsMapped() && x.UpdateExisting);

		public void ImportIntoCollection(IBusinessObjectCollection collection, int maximumRows = -1)
		{
			CleanUpPreview();

			List<string[]> valuesList = LoadFile(StartingRow, maximumRows);

			if (collection.Factory != null)
			{
				collection.Factory.SuspendValidation();
			}
			collection.SuspendValidation();

			var count = valuesList.Count;

			using (collection.SuspendAdditionallyForImport())
			using (collection.SuspendListChanged())
			{
				BusinessObject dummyBizoForUpdateExisting = null;
				bool isNew = false;
				try
				{
					var mappedRecords = Mapping.OfType<ImportWizardMapping>().Where(x => x.IsMapped()).ToArray();
					mappedRecords = GetSortedMappedRecordsIfNeeded(mappedRecords);
					var updateExistingMappedRecords = mappedRecords.Where(x => x.UpdateExisting);
					for (int r = 0; r < count; r++)
					{
						if (count != valuesList.Count)
						{
							ErrorReporter.ReportOnce("Length of valueList should not be changed", string.Format("Length of valuesList should not be changed. count = {0}, valuesList.Count = {1}", count, valuesList.Count));
						}

						int currentReadable = r + 1;
						if (!OnProgressChanged(currentReadable * 100 / count, Res.GetString("e0712534-00f8-4d11-b262-a74534962226", "Importing ({0} of {1}) ...", currentReadable, count)))
						{
							break;
						}

						var values = valuesList[r];
						var bizObjs = new List<BusinessObject>();

						if (updateExistingMappedRecords.Any())
						{
							var matched = FindMatchedBizos(collection, values, updateExistingMappedRecords, ref dummyBizoForUpdateExisting, ref isNew);
							bizObjs.AddRange(matched);
						}
						else if (CollectionInfo.Collection.AllowNew)
						{
							bizObjs.Add(collection.AddNew());
						}

						foreach (var bizObj in bizObjs)
						{
							using (bizObj.GetValidationSuspender())
							{
								ImportIntoBizObj(bizObj, values, mappedRecords);
							}
						}
					}
				}
				finally
				{
					if (isNew)
					{
						collection.Delete(dummyBizoForUpdateExisting);
					}
					collection.ResumeValidation();

					if (collection.Factory != null)
					{
						collection.Factory.ResumeValidation();
					}
				}
			}
		}

		IEnumerable<BusinessObject> FindMatchedBizos(IBusinessObjectCollection collection, string[] values,
			IEnumerable<ImportWizardMapping> mappedRecords, ref BusinessObject dummyBizoForUpdateExisting, ref bool isNew)
		{
			//general algorithm: we return all bizos that match all filters.
			var resultsPerFilter = new List<List<BusinessObject>>();

			if (dummyBizoForUpdateExisting == null)
			{
				if (collection.Count == 0)
				{
					dummyBizoForUpdateExisting = collection.AddNew();
					isNew = true;
				}
				else
				{
					dummyBizoForUpdateExisting = collection[0] as BusinessObject;
				}
			}

			foreach (var record in mappedRecords)
			{
				IZType value;
				Exception exception;
				record.TryParse(dummyBizoForUpdateExisting, values, out value, out exception, record.DefaultValue);
				if (exception != null)
				{ continue; }

				var bizOsInGrid = this.CollectionInfo.Collection.Children.Cast<BusinessObject>().Where(x =>
				(x[record.MappingName] == null && value == null) || x[record.MappingName].Equals(value)
				);
				resultsPerFilter.Add(bizOsInGrid.ToList());
			}

			//condense resultsPerFilter down into results
			var result = resultsPerFilter.Any() ? resultsPerFilter.Aggregate((x, y) => x.Intersect(y).ToList()) : new List<BusinessObject>();

			foreach (var resultBizo in result)
			{
				if (!collection.Contains(resultBizo))
				{
					collection.Add(resultBizo);
				}
			}

			return result;
		}

		protected virtual ImportWizardMapping[] GetSortedMappedRecordsIfNeeded(ImportWizardMapping[] mappedRecords) => mappedRecords;

		void ImportIntoBizObj(BusinessObject bizObj, string[] values, ImportWizardMapping[] mappedRecords)
		{
			ISupportDataImporting importingSupport = bizObj as ISupportDataImporting;
			bool oldIsImportingData = true;
			if (importingSupport != null && !importingSupport.IsImportingData)
			{
				oldIsImportingData = false;
				importingSupport.IsImportingData = true;
			}

			try
			{
				bizObj.PrepareDataImport();
				using (var suspender = new SetterValueWithSuspender(bizObj, mappedRecords.Select(x => x.MappingName)))
				{
					ImportIntoBizObjCore(suspender.SetValue, mappedRecords, bizObj, values);
				}
			}
			finally
			{
				if (importingSupport != null && !oldIsImportingData)
				{
					importingSupport.IsImportingData = false;
				}
			}
		}

		static readonly char[] dotAndCommaSeparator = new char[] { '.', '+' };

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected virtual void ImportIntoBizObjCore(Action<BusinessObject, string, object> setValue, IEnumerable<ImportWizardMapping> mappedRecords, BusinessObject bizObj, string[] values)
		{
			var bizObjOld = bizObj;
			var customFieldProvider = bizObj as ICustomFieldProvider;
			const string customiseKeyIdentifier = "__prop__"; // Programmatic Constant
			var referToError = Res.GetString("E9430FFB-6495-4C5C-AB9A-9A2527AC244A", "Please refer to online documentation for IronPython or consult with an IT professional with an understanding of the IronPython programming language.");
			var bizObjIsImportWizardPreviewLine = bizObj is ImportWizardPreviewLine;

			var itemsToRemove = new List<ImportWizardMapping>();
			bool hasNewValueSet;
			do
			{
				hasNewValueSet = false;
				var importWizardMappings = mappedRecords.Where(n => !itemsToRemove.Contains(n));
				foreach (var mappedRecord in importWizardMappings)
				{
					bizObj = bizObjOld;
					Exception exception = null;
					IZType columnValue = null;
					var mappingName = mappedRecord.MappingName;

					if (mappedRecord.IsMapped() && !mappedRecord.UpdateExisting)
					{
						try
						{
							mappedRecord.TryParse(bizObj, values, out columnValue, out exception, mappedRecord.DefaultValue);
							if (customFieldProvider != null && mappingName.Contains(customiseKeyIdentifier) && !bizObj.ZPropertyInfoHash.ContainsKey(mappingName))
							{
								bizObj = customFieldProvider.GetCustomBusinessObject();
							}

							if (bizObj.ZPropertyInfoHash.ContainsKey(mappingName))
							{
								int indexOfSubPropertySeparator = mappingName.IndexOfAny(dotAndCommaSeparator);
								if (indexOfSubPropertySeparator >= 0 && !bizObjIsImportWizardPreviewLine)
								{
									string propertyName = mappingName.Substring(0, indexOfSubPropertySeparator);
									MethodInfo methodInfo = ImportInitializationMethodNameAttribute.GetInitializationMethod(bizObj, propertyName);
									if (methodInfo != null)
									{
										methodInfo.Invoke(bizObj, null);
									}
								}

								var propertyDesc = (KPropertyDescriptor)TypeDescriptor.GetProperties(bizObj)[mappingName];

								if (bizObjIsImportWizardPreviewLine || (propertyDesc != null && !propertyDesc.IsReadOnly)) // only looks at [ReadOnly] attribute
								{
									var propertyInfo = bizObj.ZPropertyInfoHash.GetPropertySafe(mappingName);

									if (bizObjIsImportWizardPreviewLine || (!propertyInfo.ReadOnly && propertyInfo.HasSetter)) // looks at both [ReadOnly], [ReadOnlyMember] attributes as well as XX_ReadOnly
									{
										if (mappedRecord.WesternCharactersOnly && columnValue is ZString)
										{
											columnValue = ((ZString)columnValue).StripNonWesternEuropeanCharacters();
										}
										if (propertyInfo.SupportsMaxLength && propertyInfo.MaxLength > 0 && columnValue is ZString)
										{
											ZString str = columnValue.ToString();
											if (str.Length > propertyInfo.MaxLength)
											{
												columnValue = str.Substring(0, propertyInfo.MaxLength);
											}
										}

										if (columnValue != null)
										{
											if (columnValue is ZDecimal decimalValue)
											{
												var places = MetaData.GetDecimalPlaces(bizObj, propertyDesc); //Note: This metadata will not exist during preview, only actual import.
												if (places > -1)
												{
													columnValue = (ZDecimal)Utilities.Round(decimalValue, places);
												}

												var tableSchema = EnterpriseSchema.GetTableSchema(bizObj.TableName);
												if (tableSchema != null)
												{
													var value = (ZDecimal)columnValue;
													var schemaColumn = tableSchema.GetSchemaColumn(mappingName);
													var column = (SchemaDecimalColumn)schemaColumn;

													if (column != null)
													{
														if (!value.IsWithinSqlPrecisionAndScale(column.Precision, column.Scale))
														{
															var maxValue = (decimal)Math.Pow(10, column.Precision - column.Scale) - 1;
															columnValue = (ZDecimal)maxValue;
															var message = Res.GetString("92E59147-D9CD-4513-904C-B8601C7D4E95", "Attempted to insert '{0}' into Field [{1}] which has a maximum numeric value of '{2}'. Field was truncated to the max value.", value.ToString(), column, maxValue);
															mappedRecord.AddRowWarning(message);
														}
													}
												}
											}

											setValue(bizObj, mappingName, columnValue);
										}
										else
										{
											setValue(bizObj, mappingName, ((IZType)bizObj[mappingName]).Default);
										}

										itemsToRemove.Add(mappedRecord);
										hasNewValueSet = true;
									}
								}
							}
						}
						catch (TargetInvocationException ex)
						{
							Exception currentException = ex;
							while (currentException is TargetInvocationException)
							{
								currentException = currentException.InnerException;
							}
							if (!(currentException is MaxLengthExceededException))
							{
								throw;
							}
						}
						catch (MaxLengthExceededException)
						{
						}
						catch (DlrException ex)
						{
							ZPropertyInfo info = bizObj.ZPropertyInfoHash.GetPropertySafe(mappingName);
							if (info != null)
							{
								var message = Res.GetString("3B273F84-C364-45DE-AD35-A477C9B256BA", "The expression you have defined for {0} has the following Error: {1}.", info.Name, ex.Message);
								mappedRecord.ExpressionInfo.AddError(message + System.Environment.NewLine + referToError);
							}
						}
					}
					else if (exception != null)
					{
						ZPropertyInfo info = bizObj.ZPropertyInfoHash.GetPropertySafe(mappingName);
						if (info != null)
						{
							info.AddError(exception.Message);
						}
					}
				}
			} while (hasNewValueSet);
		}

		#endregion

		#region Serialization

		public override Type SettingsType { get { return typeof(DataImportWizardSettings); } }

		protected override void GetSettingsCore(ImportExportWizardSettings serializableSettings)
		{
			var wizardSettings = (DataImportWizardSettings)serializableSettings;

			wizardSettings.StartingRow = StartingRow;

			var listMapping = new List<DataImportWizardMappingSetting>();
			for (var i = 0; i < Mapping.Count; i++)
			{
				listMapping.Add(new DataImportWizardMappingSetting()
				{
					MappingName = Mapping[i].MappingName,
					FileColumnIndexOrder = Mapping[i].FileColumnIndexOrder,
					Delimiter = Mapping[i].Delimiter,
					DefaultValue = Mapping[i].DefaultValue,
					Expression = Mapping[i].Expression,
					MapAs = Mapping[i].MapAs,
					ProperCase = Mapping[i].ProperCase,
					UpdateExisting = Mapping[i].UpdateExisting,
					CustomMapList = Mapping[i].CustomMapList,
					WesternCharactersOnly = Mapping[i].WesternCharactersOnly,
				});
			}
			wizardSettings.Mappings = listMapping;
		}

		protected override void SetSettingsCore(ImportExportWizardSettings serializableSettings)
		{
			DataImportWizardSettings wizardSettings = (DataImportWizardSettings)serializableSettings;

			StartingRow = wizardSettings.StartingRow;

			foreach (DataImportWizardMappingSetting wizardMapping in wizardSettings.Mappings)
			{
				for (int i = 0; i < Mapping.Count; i++)
				{
					if (Mapping[i].MappingName == wizardMapping.MappingName)
					{
						using (Mapping[i].GetValidationSuspender())
						{
							if (wizardMapping.FileColumnIndexOrder != null && wizardMapping.FileColumnIndexOrder.Count > 0)
							{
								Mapping[i].FileColumnIndexOrder.AddRange(wizardMapping.FileColumnIndexOrder);
							}
							else if (wizardMapping.FileColumnIndex > -1)
							{
								Mapping[i].FileColumnIndexOrder.Add(wizardMapping.FileColumnIndex);
							}
							Mapping[i].Delimiter = wizardMapping.Delimiter;
							Mapping[i].DefaultValue = wizardMapping.DefaultValue;
							Mapping[i].Expression = wizardMapping.Expression;
							Mapping[i].MapAs = wizardMapping.MapAs;
							Mapping[i].ProperCase = wizardMapping.ProperCase;
							Mapping[i].UpdateExisting = wizardMapping.UpdateExisting;
							Mapping[i].CustomMapList = wizardMapping.CustomMapList;
							Mapping[i].WesternCharactersOnly = wizardMapping.WesternCharactersOnly;
						}
						break;
					}
				}
			}
		}

		#endregion

		#region Progress

		public delegate void SavingEventHandler(int percentage, string status);
		public event SavingEventHandler Saving;
		protected void OnSaving(int percentage, string status)
		{
			if (Saving != null)
			{
				Saving(percentage, status);
			}
		}

		public delegate void SavingCompleteEventHandler(int percentage, string status);
		public event SavingCompleteEventHandler SavingComplete;
		protected void OnSavingComplete(int percentage, string status)
		{
			if (SavingComplete != null)
			{
				SavingComplete(percentage, status);
			}
		}

		#endregion

		public static bool IsType(Type type, Type parentType)
		{
			return parentType == type || type.IsSubclassOf(parentType);
		}

		string loadFileException;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool disposed;

		protected void Dispose(bool disposing)
		{
			if (this.disposed)
			{
				return;
			}

			try
			{
				if (disposing)
				{
					if (this.currentFileStream != null)
					{
						this.currentFileStream.Dispose();
						this.currentFileStream = null;
					}
				}
			}
			finally
			{
				disposed = true;
			}
		}

		~ImportWizard()
		{
			Dispose(false);
		}

		#region Read Only

		public void GenerateReadOnlyWarnings()
		{
			using (CollectionInfo.Collection.SuspendAdditionallyForImport())
			using (CollectionInfo.Collection.SuspendListChanged())
			{
				BusinessObject bizObj = null;
				var isNew = false;
				if (collectionInfo.Collection.Count > 0)
				{
					bizObj = CollectionInfo.Collection[0] as BusinessObject;
				}
				else if (collectionInfo.Collection.AllowNew)
				{
					bizObj = CollectionInfo.Collection.AddNew();
					isNew = true;
				}
				if (bizObj == null)
				{
					return;
				}
				var bizObjType = bizObj.GetType();
				var toRemove = new List<ImportWizardMapping>();
				foreach (ImportWizardMapping m in Mapping)
				{
					PropertyInfo readOnlyProperty;
					bool hasReadOnlyAttribute;
					string mappingName = m.Property.MappingName;
					var desc = (KPropertyDescriptor)TypeDescriptor.GetProperties(bizObj)[mappingName];
					if (desc != null)
					{
						if (desc.IsReadOnly)
						{
							toRemove.Add(m);
							continue;
						}
					}
					try
					{
						readOnlyProperty = bizObjType.GetProperty(mappingName + "_ReadOnly", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
						readOnlyProperty = (readOnlyProperty ?? bizObjType.GetProperty(mappingName + "ReadOnly", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
						hasReadOnlyAttribute = Attribute.IsDefined(bizObjType.GetProperty(mappingName, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static), typeof(ReadOnlyMemberAttribute));
						if (!hasReadOnlyAttribute)
						{
							var propertyInfo = (ZPropertyInfo)bizObjType.GetProperty(mappingName + "Info", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).GetValue(bizObj, null);
							if (propertyInfo != null)
							{
								hasReadOnlyAttribute = propertyInfo.ReadOnly;
							}
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						continue;
					}
					if (readOnlyProperty != null || hasReadOnlyAttribute)
					{
						m.AddRowWarning(Res.GetString("F76F3E73-C4C4-4B39-B9E0-7FE64680AE9D", "This field could be read only. Values for this field possibly may not be imported."));
					}
				}
				if (isNew)
				{
					CollectionInfo.Collection.Delete(bizObj);
				}
				RemoveReadOnly(toRemove);
			}
		}

		void RemoveReadOnly(List<ImportWizardMapping> toRemove)
		{
			foreach (var mapping in toRemove)
			{
				mapping.PropertyIsReadOnly = true;
				mapping.AddRowWarning(Res.GetString("aad6709e-ad14-4c25-978d-fcf584df4dd6", "This field is read only. It can only be used with 'Match for Update' function."));
			}
		}

		#endregion
	}
}
