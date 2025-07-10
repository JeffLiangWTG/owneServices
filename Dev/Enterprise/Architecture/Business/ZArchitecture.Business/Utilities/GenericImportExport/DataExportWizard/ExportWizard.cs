using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.Business.Res;
using ResString = Enterprise.ZArchitecture.Business.ResString;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ExportWizard : ImportExportWizard
	{
		public ExportWizard(IExportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
			: base(settingsStorage, fileMapper)
		{
			this.collectionInfo = collectionInfo;
			using (SuspendSettingHasChanges())
			{
				RowTypeNameFilter = collectionInfo.RowTypes.First().Name;
			}
		}

		public bool ShowDefaultValue { get; set; }

		public Encoding FileExportEncoding
		{
			get
			{
				if (encodingType == Constants.EncodingTypes.UTF8WithoutBOM)
				{
					return new UTF8Encoding(false);
				}
				return Encoding.GetEncoding(EncodingType);
			}
		}

		[BusinessObjectTestExclude]
		[List("EncodingTypes")]
		public ZString EncodingType
		{
			get => encodingType;
			set
			{
				if (encodingType != value)
				{
					if (!EncodingTypes.ContainsCode(value))
					{
						throw new NotSupportedException(FormattableString.Invariant($"Currently only {ListFormatter.GetCommaSeparatedText(EncodingTypes.ToArray())} are supported for the export, any other encoding types are not allowed."));
					}

					SetNonPersistentPropertyValue(EncodingTypeInfo, ref encodingType, value);
				}

				ValidateEncodingTypes();
				EncodingTypeInfo.RefreshBinding();
			}
		}
		ZString encodingType = Constants.EncodingTypes.ASCII;

		public ZPropertyInfo EncodingTypeInfo => GetZPropertyInfo(nameof(EncodingType));

		public CodeDescriptionPairList EncodingTypes
		{
			get
			{
				if (encodingTypes == null)
				{
					encodingTypes = new CodeDescriptionPairList();
					AddSupportedEncodingTypes(encodingTypes);
				}

				return encodingTypes;
			}
		}

		void AddSupportedEncodingTypes(CodeDescriptionPairList types)
		{
			types.AddPair(Constants.EncodingTypes.ASCII, ResString.GetMultilingualString("B9F2F905-7F13-4DFF-8569-94EE7758C783", "American Standard Code for Information Interchange"));
			types.AddPair(Constants.EncodingTypes.UTF8, ResString.GetMultilingualString("D9AAEBF4-43B1-471B-B2EB-B9F8BB90B6FE", "8-bit Unicode Transformation Format"));
			types.AddPair(Constants.EncodingTypes.UTF8WithoutBOM, ResString.GetMultilingualString("F3629F15-DD7F-436B-9158-BF7D1E9C7739", "8-bit Unicode Transformation Format Without BOM"));
		}

		CodeDescriptionPairList encodingTypes;

		void ValidateEncodingTypes()
		{
			EncodingTypeInfo.ClearAllNotifications();
			if (EncodingType.IsEmpty)
			{
				MandatoryValidation.CheckEntered(EncodingTypeInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(EncodingTypeInfo);
			}
		}

		#region Properties

		#region CollectionInfo

		public override IEnumerable<IImportPropertyInfo> CollectionInfoProperties
		{
			get
			{
				return from rowType in CollectionInfo.RowTypes from property in rowType.Properties select property;
			}
		}

		public IExportCollectionInfo CollectionInfo
		{
			get { return collectionInfo; }
		}
		readonly IExportCollectionInfo collectionInfo;

		#endregion

		#region IncludeHeaders

		public ZBool IncludeHeaders
		{
			get { return includeHeaders; }
			set
			{
				if (includeHeaders != value)
				{
					SetNonPersistentPropertyValue(IncludeHeadersInfo, ref includeHeaders, value);
				}
			}
		}
		ZBool includeHeaders = true;

		public ZPropertyInfo IncludeHeadersInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeHeaders)); }
		}

		#endregion

		#region FixedWidth

		public ZBool FixedWidth
		{
			get { return fixedWidth; }
			set
			{
				if (fixedWidth != value)
				{
					SetNonPersistentPropertyValue(FixedWidthInfo, ref fixedWidth, value);
				}
			}
		}
		ZBool fixedWidth;

		public ZPropertyInfo FixedWidthInfo
		{
			get { return GetZPropertyInfo(nameof(FixedWidth)); }
		}

		protected override bool Delimiter_ReadOnly
		{
			get { return FixedWidth; }
		}

		#endregion

		#region ShouldValidateFileName

		protected override bool ShouldValidateFileName
		{
			get
			{
				return string.IsNullOrEmpty(FileNameExpression);
			}
		}

		#endregion

		#region FileNameExpression

		[MaxLength(2000)]
		public ZString FileNameExpression
		{
			get { return fileNameExpression; }
			set
			{
				if (fileNameExpression != value)
				{
					SetNonPersistentPropertyValue(FileNameExpressionInfo, ref fileNameExpression, value);
					ValidateFileNameExpression();
				}
			}
		}
		ZString fileNameExpression;

		public ZPropertyInfo FileNameExpressionInfo
		{
			get { return GetZPropertyInfo(nameof(FileNameExpression)); }
		}

		/// <summary>
		/// Evaluates FileNameExpression to determine the filename that will be saved in ExportCollection().
		/// </summary>
		public (ZString result, Exception error) EvaluateFileNameExpression()
		{
			var (result, error) = TryEvaluateFileNameExpression();
			if (error != null)
			{
				return (ZString.Empty, error);
			}
			if (result.IsEmpty)
			{
				return (ZString.Empty, null);
			}

			if (string.IsNullOrEmpty(Path.GetDirectoryName(result)))
			{
				try
				{
					var myDocsPath = fileMapper.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
					result = Path.Combine(myDocsPath, result);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return (ZString.Empty, ex);
				}
			}

			return (result, null);
		}

		(ZString result, Exception error) TryEvaluateFileNameExpression()
		{
			if (string.IsNullOrEmpty(FileNameExpression) || FileNameExpressionObject == null)
			{
				return (ZString.Empty, null);
			}

			try
			{
				var result = EvaluateStringExpression<string>(FileNameExpression).Invoke(FileNameExpressionObject);
				return (result, null);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return (ZString.Empty, ex);
			}
		}

		protected virtual void ValidateFileNameExpression()
		{
			FileNameExpressionInfo.ClearAllNotifications();

			var (_, evaluateException) = TryEvaluateFileNameExpression();
			if (evaluateException != null)
			{
				FileNameExpressionInfo.AddError(evaluateException.Message);
			}
		}

		public BusinessObject FileNameExpressionObject
		{
			get
			{
				return fileNameExpressionObject;
			}
			set
			{
				if (fileNameExpressionObject != value)
				{
					fileNameExpressionObject = value;
					ValidateFileNameExpression();
					OnFileNameExpressionObjectValueChanged(new EventArgs());
				}
			}
		}
		BusinessObject fileNameExpressionObject;

		#region FileNameExpressionObjectValueChanged

		public delegate void FileNameExpressionObjectValueChangedEventHandler(object sender, EventArgs e);

		public event FileNameExpressionObjectValueChangedEventHandler FileNameExpressionObjectValueChanged;

		protected virtual void OnFileNameExpressionObjectValueChanged(EventArgs e)
		{
			if (FileNameExpressionObjectValueChanged != null)
			{
				FileNameExpressionObjectValueChanged(this, e);
			}
		}

		#endregion

		#endregion

		#region Mapping

		public ExportWizardMappingCollection Mapping
		{
			get
			{
				if (mapping == null)
				{
					mapping = new ExportWizardMappingCollection(this);
					mapping.Load();
					RegisterEditableChildObject(mapping);
				}

				return mapping;
			}
		}
		ExportWizardMappingCollection mapping;

		public ExportWizardMappingCollectionView MappingView
		{
			get
			{
				if (mappingView == null)
				{
					mappingView = new ExportWizardMappingCollectionView(Mapping);
				}

				return mappingView;
			}
		}
		ExportWizardMappingCollectionView mappingView;

		public bool IsThisPartOfView(ExportWizardMapping mapping)
		{
			return mapping.RowType.Name == RowTypeNameFilter;
		}

		IEnumerable<ExportWizardMapping> GetMappings(RowType rowType)
		{
			return from m in Mapping.Cast<ExportWizardMapping>()
				   where m.RowType.Equals(rowType)
				   orderby m.Order
				   select m;
		}

		#endregion

		#region Preview

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "The Preview property is used to return a 2D array for compatibility with existing code and to facilitate data manipulation.")]
		[BusinessObjectTestExclude]
		public string[,] Preview
		{
			get { return preview; }
		}

		string[,] preview;

		public void LoadPreview()
		{
			var previewCollection = ExportCollection(CollectionInfo.BusinessObjects, MaxPreviewLines);
			int maxRowLength = (from row in previewCollection select row.Length).DefaultIfEmpty(0).Max();
			int rowCount = previewCollection.Count();
			preview = new string[rowCount, maxRowLength];
			int i = 0;
			foreach (var row in previewCollection)
			{
				for (int j = 0; j < row.Length; j++)
				{
					preview[i, j] = row[j];
				}
				for (int j = row.Length; j < maxRowLength; j++)
				{
					preview[i, j] = String.Empty;
				}
				++i;
			}
		}

		#endregion

		#region RowTypeNameFilter

		[List("RowTypeNames")]
		[MaxLength(40)]
		public ZString RowTypeNameFilter
		{
			get
			{
				return rowTypeNameFilter;
			}
			set
			{
				if (rowTypeNameFilter != value)
				{
					filteredProperties = null;
					SetNonPersistentPropertyValue(RowTypeNameFilterInfo, ref rowTypeNameFilter, value);
					if (!IsValidationSuspended)
					{
						ValidateRowTypeNameFilter();
					}
					MappingView.Rebuild();
				}
			}
		}
		ZString rowTypeNameFilter;

		public ZPropertyInfo RowTypeNameFilterInfo
		{
			get { return GetZPropertyInfo(nameof(RowTypeNameFilter)); }
		}

		void ValidateRowTypeNameFilter()
		{
			RowTypeNameFilterInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(RowTypeNameFilterInfo);
			ListValidation.ErrorIfInvalidCode(RowTypeNameFilterInfo);
		}

		public IImmutableList<IImportPropertyInfo> FilteredProperties
		{
			get
			{
				if (filteredProperties == null)
				{
					filteredProperties = (
						from rowType in CollectionInfo.RowTypes
						where rowType.Name == RowTypeNameFilter
						from property in rowType.Properties
						select property).ToArray();
				}

				return filteredProperties.ToImmutableList();
			}
		}
		IImportPropertyInfo[] filteredProperties;

		#endregion

		#region Use Text Qualifier

		[List("UseTextQualifierPairList")]
		[MaxLength(3)]
		public ZString UseTextQualifier
		{
			get { return useTextQualifier; }
			set
			{
				if (useTextQualifier != value)
				{
					SetNonPersistentPropertyValue(UseTextQualifierInfo, ref useTextQualifier, value);
				}
			}
		}
		ZString useTextQualifier = Constants.QuoteWhenSpacesOrDelimiter;

		public ZPropertyInfo UseTextQualifierInfo
		{
			get { return GetZPropertyInfo(nameof(UseTextQualifier)); }
		}

		protected bool UseTextQualifier_ReadOnly
		{
			get { return FixedWidth; }
		}

		#endregion

		#region Append Extra New Line To End Of File

		public ZBool AppendExtraNewLineToEndOfFile
		{
			get { return appendExtraNewLineToEndOfFile; }
			set
			{
				if (appendExtraNewLineToEndOfFile != value)
				{
					SetNonPersistentPropertyValue(AppendExtraNewLineToEndOfFileInfo, ref appendExtraNewLineToEndOfFile, value);
				}
			}
		}
		ZBool appendExtraNewLineToEndOfFile = true;

		public ZPropertyInfo AppendExtraNewLineToEndOfFileInfo
		{
			get { return GetZPropertyInfo(nameof(AppendExtraNewLineToEndOfFile)); }
		}

		#endregion

		#endregion

		#region Lookups

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Code should not be translated")]
		protected override void AddSupportedDelimiters(CodeDescriptionPairList delimiters)
		{
			base.AddSupportedDelimiters(delimiters);
			delimiters.AddPair("none", Res.GetString("481456bc-ce5d-4b99-ab58-4a4c27abc166", "No Delimiter"));
		}

		public CodeDescriptionPairList RowTypeNames
		{
			get
			{
				if (rowTypeNames == null)
				{
					rowTypeNames = new CodeDescriptionPairList();
					foreach (var rowType in CollectionInfo.RowTypes)
					{
						rowTypeNames.AddPair(rowType.Name, "");
					}
				}

				return rowTypeNames;
			}
		}
		CodeDescriptionPairList rowTypeNames;

		#region Use Text Qualifier Pair List

		public static class Constants
		{
			public const string AlwaysQuote = "QUO";
			public const string NeverQuote = "NOQ";
			public const string QuoteWhenSpaces = "SPA";
			public const string QuoteWhenDelimiter = "DEL";
			public const string QuoteWhenSpacesOrDelimiter = "SOD";

			public static class EncodingTypes
			{
				public const string ASCII = "ASCII";
				public const string UTF8 = "UTF-8";
				[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "EncodingTypes")]
				public const string UTF8WithoutBOM = "UTF-8 WITHOUT BOM";
			}
		}

		public CodeDescriptionPairList UseTextQualifierPairList
		{
			get
			{
				if (useTextQualifierPairList == null)
				{
					useTextQualifierPairList = new CodeDescriptionPairList();
					useTextQualifierPairList.AddPair(Constants.AlwaysQuote, Res.GetString("bbd25fa7-5265-4cbd-9f71-bb37b4654850", "Always enclose data fields in quotes"));
					useTextQualifierPairList.AddPair(Constants.QuoteWhenSpaces, Res.GetString("d888f22e-7c8b-41d5-9d9a-0c1701cebbe6", "Enclose data fields in quotes only when they contain spaces"));
					useTextQualifierPairList.AddPair(Constants.QuoteWhenDelimiter, Res.GetString("2f48c2f1-68c4-4a54-9b3b-11287ec2335b", "Enclose data fields in quotes only when they contain delimiters"));
					useTextQualifierPairList.AddPair(Constants.QuoteWhenSpacesOrDelimiter, Res.GetString("32dcf99b-7c23-4515-8e84-c780fae7eaf4", "Enclose data fields in quotes when they contain spaces or delimiters"));
					useTextQualifierPairList.AddPair(Constants.NeverQuote, Res.GetString("ccc336af-7d4c-4b3d-9e7b-b4133507b7a1", "Never enclose data fields in quotes"));
				}

				return useTextQualifierPairList;
			}
		}
		CodeDescriptionPairList useTextQualifierPairList;

		#endregion

		#endregion

		#region ExportCollection

		public bool ExportCollection(IEnumerable<BusinessObject> collection, out string errorMessage)
		{
			return ExportCollectionCore(collection, out errorMessage);
		}

		protected virtual bool ExportCollectionCore(IEnumerable<BusinessObject> collection, out string errorMessage)
		{
			errorMessage = null;
			var result = ExportCollection(collection, Int32.MaxValue);
			if (result != null)
			{
				try
				{
					if (String.Compare(Path.GetExtension(FileName), ".xls", true) == 0)
					{
						SaveXls(result);
					}
					else if (FixedWidth)
					{
						SaveFixedWidth(result);
					}
					else
					{
						SaveCsv(result);
					}
				}
				catch (Exception ex) when (ex is IOException || ex is SystemException || ex is ApplicationException)
				{
					errorMessage = ex.Message;
					if (ex.InnerException != null)
					{
						errorMessage += System.Environment.NewLine + ex.InnerException.Message;
					}
				}

				return errorMessage == null;
			}

			return true;
		}

#if DEBUG
		public virtual
#endif
		IEnumerable<string[]> ExportCollection(IEnumerable<BusinessObject> collection, int maxRecordsToProcess)
		{
			var result = new List<string[]>();
			if (IncludeHeaders)
			{
				result.Add((from m in GetMappings(collectionInfo.RowTypes.First()) select m.GetHeaderString()).ToArray());
			}

			bool cancel = false;
			int recordsToProcess = Math.Min(maxRecordsToProcess, collection.Count());
			int rowIndex = 0;
			foreach (BusinessObject bizObj in collection)
			{
				if (rowIndex >= recordsToProcess)
				{
					break;
				}

				if (!OnProgressChanged(rowIndex * 100 / recordsToProcess, Res.GetString("be7c58ac-0d1e-402f-b685-9a5e519dbfc3", "Exporting ({0} of {1}) ...", rowIndex, recordsToProcess)))
				{
					cancel = true;
					break;
				}
				var rowType = collectionInfo.RowTypes.FirstOrDefault(rt => rt.Type.IsAssignableFrom(bizObj.GetType()));
				if (rowType != null)
				{
					List<string> row = new List<string>();
					foreach (var m in GetMappings(rowType))
					{
						string errorMessage;
						if (m.SatisfiesCondition(bizObj, out errorMessage))
						{
							string value;
							if (errorMessage != null)
							{
								value = errorMessage;
							}
							else
							{
								value = m.ToString(bizObj, ShowDefaultValue);
								if (!String.IsNullOrEmpty(m.CustomMapList))
								{
									m.TryFromCustomMapList(ref value);
								}
							}
							row.Add(value);
						}
					}
					if (row.Count > 0)
					{
						result.Add(row.ToArray());
						++rowIndex;
					}
				}
			}

			if (cancel)
			{
				return Enumerable.Empty<string[]>();
			}
			else
			{
				return result;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Name of a tab character, Name of a space character, Name of 'none' character")]
		void SaveCsv(IEnumerable<string[]> values)
		{
			string delim;
			switch (Delimiter.ToLower())
			{
				case "tab":
					delim = "\t";
					break;

				case "space":
					delim = " ";
					break;

				case "none":
					delim = "";
					break;

				default:
					delim = Delimiter;
					break;
			}

			Stream writerStream;
#if DEBUG
			if (Globals.IsTest && CsvStreamForTest != null)
			{
				writerStream = CsvStreamForTest;
			}
			else
#endif
			{
				writerStream = GetFileStream();
			}

			if (writerStream == Stream.Null)
			{
				var exception = new IOException(Res.GetString("9550B92D-4318-4D6B-A0E2-44F495342FE9", "Path Specified in Filename Expression is not valid. You can change this setting in Data Export Wizard"));
				throw exception;
			}
			using (StreamWriter writer = new StreamWriter(writerStream, FileExportEncoding))
			{
				bool notFirstRow = false;

				foreach (var row in values)
				{
					StringBuilder sb = new StringBuilder();
					ProcessRow(delim, row, sb);

					if (notFirstRow)
					{
						writer.WriteLine();
					}
					writer.Write(sb.ToString());
					notFirstRow = true;
				}

				if (notFirstRow && AppendExtraNewLineToEndOfFile)
				{
					writer.WriteLine();
				}
#if DEBUG
				if (Globals.IsTest && CsvStreamForTest != null)
				{
					writer.Flush();
					CsvExportByteArrayForTest = ((MemoryStream)writerStream).ToArray();
				}
#endif
			}
		}

#if DEBUG
		public byte[] CsvExportByteArrayForTest;
		public Stream CsvStreamForTest;
#endif

		void ProcessRow(string delim, string[] row, StringBuilder sb)
		{
			var delimiterIsEmpty = delim.Length == 0;

			for (int i = 0; i < row.Length; i++)
			{
				bool containsTextQualifier = false;
				bool containsWhiteSpace = false;
				bool containsDelimiter = false;
				bool containsWhiteSpaceOrDelimiter = false;

				foreach (char c in row[i])
				{
					if (c == TextQualifier[0])
					{
						containsTextQualifier = true;
					}
					else if (Char.IsWhiteSpace(c))
					{
						containsWhiteSpace = true;
						containsWhiteSpaceOrDelimiter = true;
					}
					else if (!delimiterIsEmpty && c == delim[0])
					{
						containsDelimiter = true;
						containsWhiteSpaceOrDelimiter = true;
					}
				}

				bool stripTextQualifier = false;
				bool escapeTextQualifier = false;
				bool surroundWithQuotes = false;
				bool stripDelimiter = false;

				if (UseTextQualifier == Constants.AlwaysQuote)
				{
					surroundWithQuotes = true;
					stripTextQualifier = false;
					escapeTextQualifier = containsTextQualifier;
					stripDelimiter = false;
				}
				else if (UseTextQualifier == Constants.NeverQuote)
				{
					surroundWithQuotes = false;
					stripTextQualifier = false;
					escapeTextQualifier = false;
					stripDelimiter = containsDelimiter;
				}
				else if (UseTextQualifier == Constants.QuoteWhenSpaces)
				{
					surroundWithQuotes = containsWhiteSpace;
					stripTextQualifier = false;
					escapeTextQualifier = containsTextQualifier;
					stripDelimiter = containsDelimiter && !surroundWithQuotes;
				}
				else if (UseTextQualifier == Constants.QuoteWhenDelimiter)
				{
					surroundWithQuotes = containsDelimiter;
					stripTextQualifier = false;
					escapeTextQualifier = containsTextQualifier;
					stripDelimiter = false;
				}
				else if (UseTextQualifier == Constants.QuoteWhenSpacesOrDelimiter)
				{
					surroundWithQuotes = containsWhiteSpaceOrDelimiter;
					stripTextQualifier = false;
					escapeTextQualifier = containsTextQualifier;
					stripDelimiter = false;
				}

				string rowData = row[i];
				if (stripTextQualifier)
				{
					rowData = rowData.Replace(TextQualifier, string.Empty);
				}
				else if (escapeTextQualifier)
				{
					rowData = rowData.Replace(TextQualifier, TextQualifier + TextQualifier);
				}

				if (stripDelimiter)
				{
					rowData = rowData.Replace(delim[0].ToString(), string.Empty);
				}

				if (surroundWithQuotes)
				{
					sb.Append(TextQualifier);
					sb.Append(rowData);
					sb.Append(TextQualifier);
				}
				else
				{
					sb.Append(rowData);
				}
				if (i < row.Length - 1)
				{
					sb.Append(delim);
				}
			}
		}

		void SaveFixedWidth(IEnumerable<string[]> values)
		{
			using (StreamWriter writer = new StreamWriter(GetFileStream(), FileExportEncoding))
			{
				bool notFirstRow = false;

				foreach (var row in values)
				{
					StringBuilder sb = new StringBuilder();
					for (int i = 0; i < row.Length; i++)
					{
						sb.Append(row[i]);
					}

					if (notFirstRow)
					{
						writer.WriteLine();
					}
					writer.Write(sb.ToString());
					notFirstRow = true;
				}

				if (notFirstRow && AppendExtraNewLineToEndOfFile)
				{
					writer.WriteLine();
				}
			}
		}

		void SaveXls(IEnumerable<string[]> values)
		{
			using (IExcelInterface excelDoc = ExcelInterfaceFactory.New())
			{
				excelDoc.NewExcelFile(1);
				int rowIndex = 0;
				foreach (var row in values)
				{
					for (int i = 0; i < row.Length; i++)
					{
						excelDoc.WorkSheets[0][rowIndex, i] = row[i];
					}
					++rowIndex;
				}

				using (var fileStream = GetFileStream())
				{
					excelDoc.SaveToStream(fileStream);
				}
			}
		}

		public ZString LastExportedFullFileName { get; set; }

		Stream GetFileStream()
		{
			string fullPath;
			if (string.IsNullOrEmpty(FileNameExpression) || FileNameExpressionObject == null)
			{
				if (String.IsNullOrEmpty(Path.GetDirectoryName(FileName)))
				{
					fullPath = Path.Combine(fileMapper.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), FileName);
				}
				else
				{
					fullPath = FileName;
				}
			}
			else
			{
				fullPath = EvaluateStringExpression<string>(FileNameExpression).Invoke(FileNameExpressionObject);
				if (string.IsNullOrEmpty(Path.GetDirectoryName(fullPath)))
				{
					fullPath = Path.Combine(fileMapper.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), fullPath);
				}
			}
			LastExportedFullFileName = fullPath;
			return fileMapper.OpenWrite(fullPath);
		}

		#endregion

		#region EvaluateStringExpression

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "iron python script")]
		Func<BusinessObject, T> EvaluateStringExpression<T>(string expr)
		{
			Func<BusinessObject, T> result;
			string funName = "f" + Guid.NewGuid().ToString("N");
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("def {0}(obj):", funName);
			sb.AppendLine();

			sb.AppendFormat("  retVal = {0}", expr);
			sb.AppendLine();
			if (typeof(T).Equals(typeof(string)))
			{
				sb.AppendLine("  return retVal.ToString()");
			}
			else
			{
				sb.AppendLine("  return retVal");
			}

			DlrProxy.RunScript(sb.ToString());
			var fun = DlrProxy.CreateLambda<Func<BusinessObject, T>>
				(funName + "(obj)", "obj");
			result = bizObj => fun(bizObj);

			return result;
		}

		#endregion

		#region Serialization

		public override Type SettingsType { get { return typeof(DataExportWizardSettings); } }

		protected override void GetSettingsCore(ImportExportWizardSettings serializableSettings)
		{
			DataExportWizardSettings wizardSettings = (DataExportWizardSettings)serializableSettings;
			wizardSettings.IncludeHeaders = IncludeHeaders;
			wizardSettings.AppendExtraNewLineToEndOfFile = AppendExtraNewLineToEndOfFile;
			wizardSettings.FixedWidth = FixedWidth;
			wizardSettings.UseTextQualifier = UseTextQualifier;
			wizardSettings.FileNameExpression = FileNameExpression;
			wizardSettings.EncodingType = EncodingType;
			wizardSettings.Mappings = (
					from rowType in CollectionInfo.RowTypes
					from mapping in GetMappings(rowType)
					select NewExportWizardFromMapping(mapping, rowType)).ToList();
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Values assigned on current DPI, no need for scaling")]
		DataExportWizardMappingSetting NewExportWizardFromMapping(ExportWizardMapping mapping, RowType rowType)
		{
			return new DataExportWizardMappingSetting()
			{
				MappingName = mapping.MappingName,
				MapAs = mapping.MapAsLookup.GetDescriptionFromCode(mapping.MapAs),
				CustomMapList = mapping.CustomMapList,
				Header = mapping.Header,
				Width = mapping.Width,
				Alignment = mapping.Alignment,
				RowTypeName = rowType.Name,
				Expression = mapping.Expression,
				ConditionExpr = mapping.ConditionExpr,
				RowTypeIdentifier = rowType.Identifier
			};
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Values assigned on current DPI, no need for scaling")]
		protected override void SetSettingsCore(ImportExportWizardSettings serializableSettings)
		{
			DataExportWizardSettings wizardSettings = (DataExportWizardSettings)serializableSettings;

			IncludeHeaders = wizardSettings.IncludeHeaders;
			AppendExtraNewLineToEndOfFile = wizardSettings.AppendExtraNewLineToEndOfFile;
			FixedWidth = wizardSettings.FixedWidth;
			UseTextQualifier = wizardSettings.UseTextQualifier;
			FileNameExpression = wizardSettings.FileNameExpression;
			EncodingType = wizardSettings.EncodingType;

			Mapping.RemoveAndDeleteAll();
			foreach (var wizardMapping in wizardSettings.Mappings)
			{
				var rowType =
					(
						from rt in CollectionInfo.RowTypes
						where rt.Name.Equals(wizardMapping.RowTypeName) || rt.NameInEnglish.Contains(wizardMapping.RowTypeName) || rt.Identifier.Equals(wizardMapping.RowTypeIdentifier)
						select rt
					)
					.FirstOrDefault();

				if (rowType != null)
				{
					ExportWizardMapping newMapping = null;

					if (!String.IsNullOrEmpty(wizardMapping.MappingName))
					{
						var prop = (from p in rowType.Properties
									where p.MappingName.Equals(wizardMapping.MappingName)
									select p).FirstOrDefault();

						if (prop != null)
						{
							newMapping = new ExportWizardMapping(prop, Mapping);
						}
					}
					else
					{
						newMapping = new ExportWizardMapping(rowType, Mapping)
						{
							Expression = wizardMapping.Expression,
						};
					}

					if (newMapping != null)
					{
						newMapping.MapAs = wizardMapping.MapAs;
						newMapping.CustomMapList = wizardMapping.CustomMapList;
						newMapping.Header = wizardMapping.Header;
						newMapping.Alignment = wizardMapping.Alignment;
						newMapping.ConditionExpr = wizardMapping.ConditionExpr;
						newMapping.Width = wizardMapping.Width;

						if (!wizardMapping.MapAs.IsNullOrEmpty() && newMapping.MapAsLookup.GetDescriptionFromCode(wizardMapping.MapAs).IsNullOrEmpty())
						{
							newMapping.MapAs = newMapping.MapAsLookup.GetCodeFromDescription(wizardMapping.MapAs);
						}

						Mapping.Add(newMapping);
					}
				}
			}

			if (wizardSettings.Mappings.Count > 0 && Mapping.Count == 0)
			{
				Globals.Message.ShowWarning(Res.GetString("E7F7AA7A-5069-4494-936B-39CEAEF32796", "Imported settings are in a different language.To use it, please re-export."));
			}
		}

		#endregion
	}
}
