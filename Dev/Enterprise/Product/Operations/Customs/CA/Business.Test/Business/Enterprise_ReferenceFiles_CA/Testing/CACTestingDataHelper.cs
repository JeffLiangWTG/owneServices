using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.DataImport;
using Enterprise.Customs.CA.Business.MessageProcessors.Cadex.RecordParsers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.Testing
{
	internal class CACTestingDataHelper
	{
		public class CadexMessageProcessor : DataImporter, ICadexProcessor
		{
			#region ProcessAA

			/// <summary>
			/// Process Classification File
			/// </summary>
			public void ProcessAA(StreamReader reader, bool shouldDelete = false, bool isFile = false)
			{
				var parser = new CadexRecordWrapper(this, false);
				parser.AddChildRecord(new OtherRecordParser());
				parser.AddChildRecord(new A50FreeStandingRecordParser());
				parser.AddChildRecord(new A60FreeStandingRecordParser());
				parser.AddChildRecord(new A20RecordParser(false));
				parser[3].AddChildRecord(new A30RecordParser());
				parser[3][0].AddChildRecord(new A40RecordParser());
				parser[3].AddChildRecord(new A50RecordParser());
				parser[3].AddChildRecord(new A60RecordParser());
				var deleteManager = new DeleteDataManager(shouldDelete);
				deleteManager.AddTableToDelete(CACClassHeader.Schema.TableName, new ZQuery());
				deleteManager.AddTableToDelete(CACRate.Schema.TableName, new ZQuery(CACRateSchema.ZC_ParentTableCode, CACRateHeaderSchema.Constants.Prefix));

				Process(reader, WrapParserWithFileHeaderAndTrailerIfRequired(parser, isFile), "Processing Classification Data...", deleteManager);
			}

			#endregion

			#region ProcessAC

			/// <summary>
			/// Process GST File
			/// </summary>
			public void ProcessAC(StreamReader reader, bool shouldDelete = false, bool isFile = false)
			{
				var parser = new CadexRecordWrapper(this, false);
				parser.AddChildRecord(new OtherRecordParser());
				parser.AddChildRecord(new C10RecordParser(isFile));

				var deleteManager = new DeleteDataManager(shouldDelete);
				deleteManager.AddTableToDelete(CACTaxRate.Schema.TableName, new ZQuery(CACTaxRateSchema.ZH_TaxType, CACTaxRate.TaxType.GST));

				Process(reader, WrapParserWithFileHeaderAndTrailerIfRequired(parser, isFile), "Processing GST Data...", deleteManager);
			}

			#endregion

			#region ProcessAD

			/// <summary>
			/// Process Excise Tax File
			/// </summary>
			public void ProcessAD(StreamReader reader, bool shouldDelete = false, bool isFile = false)
			{
				var parser = new CadexRecordWrapper(this, false);
				parser.AddChildRecord(new OtherRecordParser());
				parser.AddChildRecord(new D10RecordParser(isFile));

				var deleteManager = new DeleteDataManager(shouldDelete);
				deleteManager.AddTableToDelete(CACTaxRate.Schema.TableName, new ZQuery(CACTaxRateSchema.ZH_TaxType, CACTaxRate.TaxType.Excise));

				Process(reader, WrapParserWithFileHeaderAndTrailerIfRequired(parser, isFile), "Processing Excise Tax Data...", deleteManager);
			}

			#endregion

			#region ProcessRA

			/// <summary>
			/// Process Classification/Tariff Query Response
			/// </summary>
			public void ProcessRA(StreamReader reader)
			{
				a10RecordParser = new A10RecordParser(this);
				a10RecordParser.AddChildRecord(new CadexRecordWrapper(true));
				a10RecordParser[0].AddChildRecord(new A11RecordParser());
				a10RecordParser[0].AddChildRecord(new CadexRecordWrapper(false));
				a10RecordParser[0][1].AddChildRecord(new A20RecordWrapper());
				a10RecordParser[0][1][0].AddChildRecord(new A20RecordParser(false));
				a10RecordParser[0][1][0][0].AddChildRecord(new A30RecordParser());
				a10RecordParser[0][1][0][0][0].AddChildRecord(new A40RecordParser());
				a10RecordParser[0][1][0][0].AddChildRecord(new A52RecordParser(false));
				a10RecordParser[0][1][0][0].AddChildRecord(new A54RecordParser());
				a10RecordParser[0][1][0][0].AddChildRecord(new A60RecordParser());
				a10RecordParser[0][1][0].AddChildRecord(new A21RecordParser());
				a10RecordParser[0][1].AddChildRecord(new A70RecordWrapper());
				a10RecordParser[0][1][1].AddChildRecord(new A70RecordParser());
				a10RecordParser[0][1][1][0].AddChildRecord(new A80RecordParser());
				a10RecordParser[0][1][1].AddChildRecord(new A71RecordParser());

				Process(reader, a10RecordParser, "Processing Classification/Tariff Query Response Data...", new DeleteDataManager(false));
			}

			A10RecordParser a10RecordParser;

			public string R10ReferenceNumber
			{
				get
				{
					return a10RecordParser == null ? null : a10RecordParser.ReferenceNumber;
				}
			}

			#endregion

			#region Process

			CadexRecordParser WrapParserWithFileHeaderAndTrailerIfRequired(CadexRecordParser parser, bool isFile)
			{
				if (isFile)
				{
					var bRecordParser = new BRecordParser(this);
					foreach (var childParser in parser.ChildParsers)
					{
						bRecordParser.AddChildRecord(childParser);
					}
					bRecordParser.AddChildRecord(new YRecordParser());
					parser = bRecordParser;
				}
				return parser;
			}

			void Process(StreamReader reader, CadexRecordParser parser, string onStartMessage, DeleteDataManager deleteDataManager)
			{
				ProcessSafe(reader, onStartMessage, () => Process(parser, deleteDataManager));
			}

			void Process(CadexRecordParser parser, DeleteDataManager deleteDataManager)
			{
				DeleteDataIfRequired(deleteDataManager);

				var recordToParse = GetNextNotEmptyRecord();

				firstLineNumber = LineNumber;

				while (Success && !string.IsNullOrEmpty(recordToParse))
				{
					ParseRecord(parser, ref recordToParse);
					FireErrorsIfRequired(parser, ref recordToParse);
				}
				UpdateAdditionalDataIfRequired(parser);
			}

			void UpdateAdditionalDataIfRequired(CadexRecordParser parser)
			{
				var updateScripts = parser.AdditionalUpdateScripts;
				var scriptStingBuilder = new ZStringBuilder();
				short index = 0;
				foreach (var script in updateScripts)
				{
					scriptStingBuilder.Append(script);
					if (index >= RecordsPerFactory)
					{
						UpdateToDatabase();
						scriptStingBuilder.Clear();
						index = 0;
					}
					index++;
				}
				if (index > 0)
				{
					UpdateToDatabase();
				}

				void UpdateToDatabase()
				{
					Db.Connection.ExecuteNonQuery(scriptStingBuilder.ToStringWithDelimiterBetweenAppends($"\r\n"));
				}
			}

			protected override void FireOnImportStart(StreamReader reader, string message)
			{
				base.FireOnImportStart(reader, message);
				dataManager = new DeleteDataManager(true);
				skip = false;
				parserToStart = null;
				missingRecordErrors.Clear();
			}

			void DeleteDataIfRequired(DeleteDataManager deleteDataManager)
			{
				if (deleteDataManager.ShouldDelete)
				{
					deleteDataManager.Delete();
				}
			}

			string GetNextNotEmptyRecord()
			{
				string result = null;
				while (string.IsNullOrEmpty(result))
				{
					LineNumber++;
					if (StreamReader.Peek() == -1)
					{
						break;
					}

					result = StreamReader.ReadLine();
				}
				return result;
			}

			#region ParseRecord

			bool ParseRecord(CadexRecordParser parser, ref string recordToParse)
			{
				var currentLine = LineNumber;

				while ((skip && (skip = parserToStart != parser)) || parser.IsWrapper || TryParseRecord(parser, ref recordToParse))
				{
					ParseChildRecords(parser, ref recordToParse);
					if (skip || parser.IsWrapper)
					{
						break;
					}

					SaveFactoryIfRequired(parser, ref recordToParse);
				}

				return LineNumber > currentLine;
			}

			bool TryParseRecord(CadexRecordParser parser, ref string recordToParse)
			{
				var result = !string.IsNullOrEmpty(recordToParse) && parser.TryParse(recordToParse);
				if (result)
				{
					parserToStart = null;
					recordToParse = GetNextNotEmptyRecord();
					FireMissingRecordErrors();
					FireOnProgressPeriodically();
				}
				else if (parserToStart == null)
				{
					parserToStart = parser;
				}
				return result;
			}

			void ParseChildRecords(CadexRecordParser parser, ref string recordToParse)
			{
				foreach (var childParser in parser.ChildParsers)
				{
					if (ParseRecord(childParser, ref recordToParse))
					{
						if (parser.IsWrapper && ((CadexRecordWrapper)parser).ParseOneChildOnly)
						{
							break;
						}
					}
					else if (!skip && childParser.Mandatory)
					{
						AddMissingRecordError(recordToParse, childParser);
					}
				}
			}

			void SaveFactoryIfRequired(CadexRecordParser parser, ref string recordToParse)
			{
				if (parser.SaveRequired && SaveRequired)
				{
					dataManager.Delete();
					SaveFactory();
					if (Canceled)
					{
						recordToParse = null;
					}
				}
			}

			#endregion

			#region Errors

			void FireErrorsIfRequired(CadexRecordParser rootParser, ref string recordToParse)
			{
				if (string.IsNullOrEmpty(recordToParse))
				{
					FireMissingRecordErrors();
				}
				else if (LineNumber == firstLineNumber)
				{
					Success = false;
				}
				else if (rootParser.IsMatchAny(recordToParse))
				{
					Success = false;
				}
				else
				{
					InvalidLines++;
					recordToParse = GetNextNotEmptyRecord();
					missingRecordErrors.Clear();
					skip = true;
				}
			}

			void FireMissingRecordErrors()
			{
				if (missingRecordErrors.Count > 0 && !Canceled)
				{
					foreach (var missedRecord in missingRecordErrors)
					{
						InvalidLines++;
					}
					missingRecordErrors.Clear();
				}
			}

			void AddMissingRecordError(string record, CadexRecordParser parser)
			{
				if (parser is YRecordParser)
				{
					missingRecordErrors.Add("Y (file trailer) record has not been found. Inbound data may be not complete.\r\n\r\n");
				}
				else
				{
					if (parser.IsWrapper)
					{
						missingRecordErrors.Clear();
					}

					missingRecordErrors.Add($"Line {LineNumber}, Error Message: {parser.RecordName} record has not been found after {parser.Parent.RecordName}., Current Line Content: '{record}'\r\n\r\n");
				}
			}

			#endregion

			#region Implementation

			#region ICadexProcessor Members

			BusinessObjectFactory IFactoryProvider.Factory
			{
				get { return FactoryProvider.Current; }
			}

			void ICadexProcessor.ShowError(string message)
			{
				FireOnShowNotification(message);
			}

			void ICadexProcessor.ShowRecordError(string record, string message)
			{
				InvalidLines++;
			}

			void ICadexProcessor.AddDeleteFetchHint(string tableName, ZQuery filter)
			{
				dataManager.AddTableToDelete(tableName, filter);
			}

			#endregion

			#region DeleteDataManager

			class DeleteDataManager
			{
				internal DeleteDataManager(bool shouldDelete)
				{
					ShouldDelete = shouldDelete;
					tablesToDelete = new Dictionary<string, DeleteFilter>();
				}

				internal void AddTableToDelete(string tableName, ZQuery filter)
				{
					DeleteFilter deleteFilter;
					if (!tablesToDelete.TryGetValue(tableName, out deleteFilter))
					{
						deleteFilter = tablesToDelete[tableName] = new DeleteFilter();
					}
					deleteFilter.AddFilter(filter);
				}

				internal void Delete()
				{
					if (tablesToDelete.Count > 0)
					{
						var sqlTextBuilder = tablesToDelete.Aggregate(new StringBuilder(), (builder, table) => builder.AppendFormat("DELETE {0} {1}\r\n", table.Key, table.Value.ToString()));
						Db.Connection.ExecuteNonQuery(sqlTextBuilder.ToString());
						tablesToDelete.Clear();
					}
				}

				#region DeleteFilter

				class DeleteFilter
				{
					public override string ToString()
					{
						var query = new ZQuery();
						foreach (var filter in equalFilters.OrderBy(e => e.Value.Count))
						{
							query.AddToFilter(filter.Key, SQLComparisonOperator.Equal, filter.Value);
						}
						foreach (var filter in sqlInFilters)
						{
							query.AddToFilter(filter.Value);
						}

						var whereClause = query.LiteralTextSqlFormatted;
						return whereClause.Length > 0 ? "\r\n\tWHERE " + whereClause : string.Empty;
					}

					internal void AddFilter(ZQuery filter)
					{
						foreach (var part in filter.GetCompositeParts())
						{
							var filterParts = ((IFilterPartsProvider)part).FilterParts;
							if (filterParts.Length == 1)
							{
								if (filterParts[0] is ZSqlParameter)
								{
									AddEqualFilter(part, (ZSqlParameter)filterParts[0]);
								}
								else
								{
									AddSQLInFilter(part, filterParts[0]);
								}
							}
							else if (!string.IsNullOrEmpty(part.LiteralTextSqlFormatted))
							{
								ThrowNotSupportedQueryException(part);
							}
						}
					}

					void AddEqualFilter(ZQuery part, ZSqlParameter param)
					{
						if (param.ComparisonOperator != SQLComparisonOperator.Equal)
						{
							ThrowNotSupportedQueryException(part);
						}

						List<object> equalFilter;
						if (equalFilters.TryGetValue(param.SchemaColumn, out equalFilter))
						{
							if (!equalFilter.Contains(param.Value))
							{
								equalFilter.Add(param.Value);
							}
						}
						else
						{
							equalFilters[param.SchemaColumn] = new List<object> { param.Value };
						}
					}

					void AddSQLInFilter(ZQuery part, IFilterPart filterPart)
					{
						if (filterPart.GetType().Name != "ZSQLInFilter")
						{
							ThrowNotSupportedQueryException(part);
						}

						var key = part.LiteralTextSqlFormatted;
						if (!sqlInFilters.ContainsKey(key))
						{
							sqlInFilters[key] = part;
						}
					}

					static void ThrowNotSupportedQueryException(ZQuery filter)
					{
						throw new NotSupportedException("Only simple 'Equal' and 'In/Not In' filters are supported, but was:\r\n" + filter.LiteralTextSqlFormatted);
					}

					readonly Dictionary<SchemaColumn, List<object>> equalFilters = new Dictionary<SchemaColumn, List<object>>();
					readonly Dictionary<string, ZQuery> sqlInFilters = new Dictionary<string, ZQuery>();
				}

				#endregion

				internal bool ShouldDelete { get; private set; }
				readonly Dictionary<string, DeleteFilter> tablesToDelete;
			}

			#endregion

			CadexRecordParser parserToStart;
			int firstLineNumber;
			bool skip;
			readonly List<string> missingRecordErrors = new List<string>();
			DeleteDataManager dataManager;

			#endregion

			#endregion
		}

		#region CadexRecordParser

		abstract class CadexRecordParser
		{
			#region Constructors

			protected CadexRecordParser(ICadexProcessor processor, bool mandatory, bool saveRequired, bool isFile = false)
				: this(mandatory, saveRequired, isFile)
			{
				Processor = processor;
			}

			protected CadexRecordParser(bool mandatory, bool saveRequired, bool isFile = false)
			{
				Mandatory = mandatory;
				SaveRequired = saveRequired;
				IsFile = isFile;
				childParsers = new List<CadexRecordParser>();
			}

			#endregion

			#region RecordName

			public virtual string RecordName
			{
				get { return GetType().Name.Substring(0, 3); }
			}

			public virtual string RecordProcessedNotification
			{
				get { return string.Empty; }
			}

			#endregion

			#region Parent

			public CadexRecordParser Parent
			{
				get { return parent != null && parent.IsWrapper && parent.Parent != null ? parent.Parent : parent; }
				private set { parent = value; }
			}
			CadexRecordParser parent;

			public bool IsWrapper
			{
				get { return this is CadexRecordWrapper; }
			}

			#endregion

			#region ChildParsers

			public CadexRecordParser this[int index]
			{
				get { return childParsers[index]; }
			}

			public void AddChildRecord(CadexRecordParser recordParser)
			{
				recordParser.Parent = this;
				recordParser.Processor = Processor;
				childParsers.Add(recordParser);
			}

			public IEnumerable<CadexRecordParser> ChildParsers
			{
				get { return childParsers; }
			}
			readonly List<CadexRecordParser> childParsers;

			public IEnumerable<ZString> AdditionalUpdateScripts
			{
				get
				{
					return UpdateScripts.Union(ChildParsers.SelectMany(x => x.UpdateScripts));
				}
			}

			protected virtual IEnumerable<ZString> UpdateScripts
			{
				get { return Array.Empty<ZString>(); }
			}

			public bool IsFile { get; private set; }

			#endregion

			#region TryParse

			public abstract bool TryParse(string record);

			#region ParseRateLines

			internal static void ParseRateLines(CACRate rate, Match match, ICadexProcessor processor)
			{
				for (var i = 0; i < match.Groups["Rate"].Captures.Count; i++)
				{
					var rateType = match.Groups["RateType"].Captures[i].Value.Trim();
					if (!string.IsNullOrEmpty(rateType))
					{
						var rateLine = processor.Factory.New<CACRateLine>();
						using (rateLine.GetValidationSuspender())
						{
							rateLine.ZR_ZC_Rate = rate.PK;
							rateLine.ZR_DutyRateType = rateType;
							rateLine.ZR_DutyRateRegular = ParseDecimal(match.Groups["RateRegular"].Captures[i].Value);
							rateLine.ZR_DutyRateMax = ParseDecimal(match.Groups["RateMax"].Captures[i].Value);
							rateLine.ZR_DutyRateMin = ParseDecimal(match.Groups["RateMin"].Captures[i].Value);
						}
					}
				}
			}

			#endregion

			#region ParseDate

			protected ZDateTime ParseDate(string yyyyMMdd, string record)
			{
				ZDateTime result;
				if (string.IsNullOrEmpty(yyyyMMdd.Trim()))
				{
					result = ZDateTime.Empty;
				}
				else if (yyyyMMdd == "99999999")
				{
					result = maxSqlSmallDateValue;
				}
				else if (!ZDateTime.TryParseExact(yyyyMMdd, out result, "yyyyMMdd"))
				{
					Processor.ShowRecordError(record, string.Format("Invalid Date: '{0}'", yyyyMMdd));
				}
				return result;
			}

			readonly ZDateTime maxSqlSmallDateValue = new ZDateTime(2079, 6, 5);

			#endregion

			#region ParseDecimal

			internal static ZDecimal ParseDecimal(string value, int integralPart = 4)
			{
				return decimal.Parse(value.Insert(integralPart, "."), CultureInfo.InvariantCulture);
			}

			#endregion

			#region Regex

			public virtual bool IsMatchAny(string record)
			{
				return Regex.IsMatch(record) || ChildParsers.Any(parser => parser.IsMatchAny(record));
			}

			protected Regex Regex
			{
				get
				{
					return regex ?? (regex = new Regex(RegexPattern,
							RegexOptions.IgnoreCase
							| RegexOptions.IgnorePatternWhitespace
							| RegexOptions.Singleline
							| RegexOptions.Compiled
							| RegexOptions.ExplicitCapture));
				}
			}
			Regex regex;

			protected abstract string RegexPattern { get; }

			#endregion

			protected ICadexProcessor Processor { get; private set; }

			#endregion

			#region Errors

			protected void FireParentRecordNotFoundError(string record)
			{
				Processor.ShowRecordError(record, $"{Parent.RecordName} record has not been found before this one.");
			}

			#endregion

			public virtual bool Mandatory { get; private set; }
			public bool SaveRequired { get; private set; }
		}

		#endregion

		#region BRecordParser

		class BRecordParser : CadexRecordParser
		{
			public BRecordParser(ICadexProcessor processor) : base(processor, true, true) { }

			public override string RecordName
			{
				get { return "B"; }
			}

			public override bool TryParse(string record)
			{
				return Regex.IsMatch(record);
			}

			protected override string RegexPattern
			{
				get { return @"^B[\w\d\s]{3}\d{5}.{16}\d{8}[\w\d\s]{3}\w{2}$"; }
			}
		}

		#endregion

		#region A10RecordParser

		class A10RecordParser : CadexRecordParser
		{
			public A10RecordParser(ICadexProcessor processor)
				: base(processor, true, true)
			{
			}

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					LastMatch = match;
					IsValid = true;
					Errors = new List<string>();

					if (ReferenceNumber == null)
					{
						var group = LastMatch.Groups["RefNumber"];
						ReferenceNumber = group == null ? string.Empty : group.Value.Trim();
					}
				}
				return match.Success;
			}

			public override string RecordProcessedNotification
			{
				get
				{
					var builder = new ZStringBuilder();
					builder.Append("Classification/Tariff record has been processed.");
					Errors.Aggregate(builder, (b, e) => b.Append(e));
					builder.Append($"Classification: '{LastMatch.Groups["ClassNumber"].Value.Trim()}', Tariff: '{LastMatch.Groups["TariffCode"].Value.Trim()}', Effective Date: '{ParseDate(LastMatch.Groups["ClassEffDate"].Value.Trim(), LastMatch.Value).ToShortDateString()}'.");
					builder.Append($"Query Record Content: '{LastMatch.Value}'.");
					return builder.ToStringWithNewLineBetweenAppends();
				}
			}

			internal string ReferenceNumber { get; private set; }
			internal Match LastMatch { get; private set; }
			internal List<string> Errors { get; private set; }
			internal bool IsValid { get; set; }
			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^RA10
(?<ClassNumber>.{0,10})
(?<ClassEffDate>.{0,8})
(?<TariffCode>.{0,4})
(?<TariffEffDate>.{0,8})
(?<RefNumber>.{0,4})$";
				}
			}
			#endregion
		}

		#endregion

		#region A11RecordParser

		class A11RecordParser : CadexRecordParser
		{
			public A11RecordParser() : base(false, false) { }

			#region TryParse

			public override bool TryParse(string record)
			{
				var match = Regex.Match(record);
				if (match.Success)
				{
					AddQueryLineError((A10RecordParser)Parent, match);
				}

				return match.Success;
			}

			static void AddQueryLineError(A10RecordParser parent, Match match)
			{
				parent.IsValid = false;
				parent.Errors.Add(GetErrorInQueryLineMessage(match));
			}

			internal static string GetErrorInQueryLineMessage(Match match)
			{
				return $"An error in query line: '{match.Groups["MessageText"].Value.Trim()}'. Message Number : '{match.Groups["MessageNumber"].Value}'.";
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^RA11
(?<MessageNumber>\w{8})
(?<MessageText>.{0,92})$";
				}
			}
			#endregion
		}

		#endregion

		#region A20RecordParser

		class A20RecordParser : CadexRecordParser
		{
			public A20RecordParser(bool mandatory) : base(mandatory, true) { }

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					var classificationNumber = match.Groups["ClassNumber"].Value;
					var effectiveDate = ParseDate(match.Groups["ClassEffDate"].Value, match.Value);
					lastClassHeader = CadexParsersUtilities.LoadOrCreateClassHeader(Processor.Factory, effectiveDate, classificationNumber);
					using (lastClassHeader.GetValidationSuspender())
					{
						lastClassHeader.ZA_AreaCode = match.Groups["AreaCode"].Value;
						lastClassHeader.ZA_ExpiryDate = ParseDate(match.Groups["ClassExpDate"].Value, match.Value);
						lastClassHeader.ZA_ClassAuthorityNumber = match.Groups["ClassAuthNumber"].Value.TrimEnd();
						lastClassHeader.ZA_StatisticalUOMCode = match.Groups["UOM"].Value.Trim();
						lastClassHeader.ZA_ExchangeDateDeterminationFlagInfo.SetValueFromString(match.Groups["ExDateDet"].Value);
						lastClassHeader.ZA_InactiveIndInfo.SetValueFromString(match.Groups["Inactive"].Value);
						lastClassHeader.ZA_TariffAuthorityNumber = match.Groups["TariffAuthNumber"].Value.TrimEnd();
						lastClassHeader.ZA_TariffEffectiveDate = ParseDate(match.Groups["TariffEffDate"].Value, match.Value);
						lastClassHeader.ZA_TariffExpiryDate = ParseDate(match.Groups["TariffExpDate"].Value, match.Value);
						lastClassHeader.ZA_PermitIndInfo.SetValueFromString(match.Groups["Permit"].Value);
						lastClassHeader.ZA_QuotaIndInfo.SetValueFromString(match.Groups["Quota"].Value);
						lastTaxRefNumHeader = null;
					}
				}
				return match.Success;
			}

			#region LastClassHeader

			internal CACClassHeader GetClassHeader(ZDateTime effectiveDate, string сlassificationNumber, CadexRecordParser child)
			{
				return lastClassHeader != null && lastClassHeader.ZA_ClassificationNumber == сlassificationNumber
					   && ((lastClassHeader.ZA_EffectiveDate <= effectiveDate && lastClassHeader.ZA_ExpiryDate >= effectiveDate)
						   || (child is A30RecordParser && CACRateHeader.Load(lastClassHeader, lastClassHeader.ZA_EffectiveDate, CACRateHeader.RateType.ClassificationRate, true) == null)
						   || (child is A50RecordParser && CACTaxRefNumHeader.Load(lastClassHeader, lastClassHeader.ZA_EffectiveDate, true) == null)
						   || (child is A60RecordParser && CACRateHeader.Load(lastClassHeader, lastClassHeader.ZA_EffectiveDate, CACRateHeader.RateType.ExciseDutyRate, true) == null))
						? lastClassHeader : CACClassHeader.Load(Processor.Factory, effectiveDate, сlassificationNumber, true);
			}

			CACClassHeader lastClassHeader;

			#endregion

			#region LastTaxRefNumHeader

			internal CACTaxRefNumHeader LastTaxRefNumHeader
			{
				get
				{
					if (lastClassHeader != null && lastTaxRefNumHeader == null)
					{
						lastTaxRefNumHeader = CadexParsersUtilities.LoadOrCreateTaxRefNumHeader(Processor, lastClassHeader, lastClassHeader.ZA_EffectiveDate);
						LastTaxRefNumbersQueue = new Queue<CACTaxRefNumber>();
					}
					return lastTaxRefNumHeader;
				}
			}

			CACTaxRefNumHeader lastTaxRefNumHeader;

			internal Queue<CACTaxRefNumber> LastTaxRefNumbersQueue { get; set; }

			#endregion

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^[AGR]A20
(?<ClassNumber>\d{10})
(?<AreaCode>\d{3})
(?<ClassEffDate>\d{8})
(?<ClassExpDate>\d{8})
(?<ClassAuthNumber>.{13})
(?<UOM>[\w\s]{3})
(?<ExDateDet>[YN])
(?<Inactive>[YN])
(?<TariffAuthNumber>.{13})
(?<TariffEffDate>\d{8})
(?<TariffExpDate>\d{8})
(?<Permit>[YN])
(?<Quota>[YN])$";
				}
			}

			#endregion
		}

		#endregion

		#region A21RecordParser

		class A21RecordParser : CadexRecordParser
		{
			public A21RecordParser() : base(false, false) { }

			#region TryParse

			public override bool TryParse(string record)
			{
				var match = Regex.Match(record);
				var result = match.Success && match.Groups["Row"].Value == RecordName;
				if (result)
				{
					AddCodeNotFoundError((A10RecordParser)Parent, match);
				}

				return result;
			}

			void AddCodeNotFoundError(A10RecordParser parent, Match match)
			{
				parent.Errors.Add($"No {GetDescription()} duty rate is found on the database for the number and date queried.\r\nMessage Number: '{match.Groups["MessageNumber"].Value}', Importer/Broker Ref Number: '{match.Groups["RefNumber"].Value}'.");
			}

			protected virtual string GetDescription()
			{
				return "Classification Number";
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^R(?<Row>A21|A71)
(?<Number>\d{4,10})
(?<EffDate>\d{8})
(?<MessageNumber>\w{8})
(?<MessageText>.{0,92})
(?<RefNumber>\w{4})?$";
				}
			}

			#endregion
		}

		#endregion

		#region A30RecordParser

		class A30RecordParser : CadexRecordParser
		{
			public A30RecordParser() : base(false, false) { }

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					var effectiveDate = ParseDate(match.Groups["RateEffDate"].Value, match.Value);
					var classHeader = ((A20RecordParser)Parent).GetClassHeader(effectiveDate, match.Groups["ClassNumber"].Value, this);
					if (classHeader != null)
					{
						LastRateHeader = CadexParsersUtilities.LoadOrCreateRateHeader(Processor, classHeader, effectiveDate, CACRateHeader.RateType.ClassificationRate);
						var expiryDate = ParseDate(match.Groups["RateExpDate"].Value, match.Value);
						if (!LastRateHeader.HasChanges || !LastRateHeader.ZB_ExpiryDate.IsValid || expiryDate >= LastRateHeader.ZB_ExpiryDate)
						{
							using (LastRateHeader.GetValidationSuspender())
							{
								LastRateHeader.ZB_ExpiryDate = expiryDate;
								LastRateHeader.ZB_FreeIndInfo.SetValueFromString(match.Groups["Free"].Value);
								LastRateHeader.ZB_UnitOfMeasure = match.Groups["UOM"].Value.Trim();
								LastRateHeader.ZB_InactiveInfo.SetValueFromString(match.Groups["Inactive"].Value);
								LastRateHeader.ZB_DutyRateAuthorityNumber = match.Groups["RateAuthNumber"].Value.TrimEnd();
							}
						}
					}
					else
					{
						LastRateHeader = null;
						FireParentRecordNotFoundError(record);
					}
				}
				return match.Success;
			}

			internal CACRateHeader LastRateHeader { get; private set; }

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^[AGR]A30
(?<ClassNumber>\d{10})
(?<RateEffDate>\d{8})
(?<RateExpDate>\d{8})
(?<Free>[YN])
(?<UOM>[\w\s]{3})
(?<Inactive>[YN])
(?<RateAuthNumber>.{0,13})$";
				}
			}

			#endregion
		}

		#endregion

		#region A40RecordParser

		class A40RecordParser : CadexRecordParser
		{
			public A40RecordParser() : base(true, false) { }

			public override bool Mandatory
			{
				get { return Parent.LastRateHeader != null && !Parent.LastRateHeader.ZB_FreeInd; }
			}

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					if (Parent.LastRateHeader != null)
					{
						var rate = Processor.Factory.New<CACRate>();
						using (rate.GetValidationSuspender())
						{
							rate.ZC_ParentID = Parent.LastRateHeader.PK;
							rate.ZC_ParentTableCode = CACRateHeaderSchema.Constants.Prefix;
							rate.ZC_TreatmentCode = match.Groups["TreatmentCode"].Value;
							rate.ZC_FreeIndInfo.SetValueFromString(match.Groups["Free"].Value);

							ParseRateLines(rate, match, Processor);
						}
					}
					else
					{
						FireParentRecordNotFoundError(record);
					}
				}
				return match.Success;
			}

			new A30RecordParser Parent
			{
				get { return (A30RecordParser)base.Parent; }
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^[AGR]A40
(?<Rate>
(?<RateType>[\w\s])
(?<RateRegular>\d{9})
(?<RateMin>\d{9})
(?<RateMax>\d{9})-?){0,4}
(?<TreatmentCode>\d{2})
(?<Free>[YN])$";
				}
			}

			#endregion
		}

		#endregion

		#region A50RecordParser

		class A50RecordParser : CadexRecordParser
		{
			public A50RecordParser() : base(false, false) { }

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					var effectiveDate = ParseDate(match.Groups["RegulationEffDate"].Value, match.Value);
					var classHeader = ((A20RecordParser)Parent).GetClassHeader(effectiveDate, match.Groups["ClassNumber"].Value, this);
					if (classHeader != null)
					{
						ProcessA50Record(Processor, classHeader, effectiveDate, match, ParseDate(match.Groups["RegulationExpDate"].Value, match.Value));
					}
					else
					{
						FireParentRecordNotFoundError(record);
					}
				}
				return match.Success;
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return A50RegexPattern;
				}
			}

			internal static void ProcessA50Record(ICadexProcessor processor, CACClassHeader classHeader, ZDateTime effectiveDate, Match match, ZDateTime expiryDate)
			{
				bool shouldUpdateTaxRefNumHeader = true;
				var refNumHeader = CadexParsersUtilities.LoadTaxRefNumHeader(classHeader, effectiveDate);
				if (refNumHeader == null)
				{
					refNumHeader = CadexParsersUtilities.CreateTaxRefNumHeader(classHeader, effectiveDate);
				}
				else
				{
					shouldUpdateTaxRefNumHeader = !refNumHeader.HasChanges || !refNumHeader.ZD_ExpiryDate.IsValid || expiryDate >= refNumHeader.ZD_ExpiryDate;
				}

				if (shouldUpdateTaxRefNumHeader)
				{
					CadexParsersUtilities.DeleteRefNumbers(processor, refNumHeader);

					using (refNumHeader.GetValidationSuspender())
					{
						refNumHeader.ZD_ExpiryDate = expiryDate;
						refNumHeader.ZD_InactiveInfo.SetValueFromString(match.Groups["Inactive"].Value);

						var gsts = match.Groups["GST"].Captures;
						var exciseTaxes = match.Groups["ExciseTax"].Captures;

						for (var i = 0; i < gsts.Count; i++)
						{
							var refNumber = processor.Factory.New<CACTaxRefNumber>();
							using (refNumber.GetValidationSuspender())
							{
								refNumber.ZE_ZD_TaxRefNumHeader = refNumHeader.PK;
								refNumber.ZE_GSTRefNumber = gsts[i].Value.Trim();
								if (i < exciseTaxes.Count)
								{
									refNumber.ZE_ExciseTaxRefNumber = exciseTaxes[i].Value.Trim();
								}
							}
						}
					}
				}
			}

			internal const string A50RegexPattern = @"^[AG]A50
(?<ClassNumber>\d{10})
(?<RegulationEffDate>\d{8})
(?<RegulationExpDate>\d{8})
(?<Inactive>[YN])
(?<RefNumber>
(?<GST>[\w\d\s]{3})
(?<ExciseTax>[\w\d\s]{3})?-?){0,17}$";

			#endregion
		}

		class A50FreeStandingRecordParser : CadexRecordParser
		{
			public A50FreeStandingRecordParser() : base(false, true) { }

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					var effectiveDate = ParseDate(match.Groups["RegulationEffDate"].Value, match.Value);
					var classificationNumber = match.Groups["ClassNumber"].Value;
					var classHeader = CACClassHeader.Load(Processor.Factory, effectiveDate, classificationNumber);
					if (classHeader != null)
					{
						A50RecordParser.ProcessA50Record(Processor, classHeader, effectiveDate, match, ParseDate(match.Groups["RegulationExpDate"].Value, match.Value));
					}
					else
					{
						FireParentRecordNotFoundError("Class Header");
					}
				}
				return match.Success;
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return A50RecordParser.A50RegexPattern;
				}
			}

			#endregion
		}

		#endregion

		#region A52RecordParser

		class A52RecordParser : C10RecordParser
		{
			public A52RecordParser(bool isFile) : base(false, false, isFile) { }

			#region ParseTaxRate

			protected override void ParseTaxRate(CACTaxRate rate, Match match)
			{
				base.ParseTaxRate(rate, match);
				var parent = ((A20RecordParser)Parent);
				var refNumHeader = parent.LastTaxRefNumHeader;
				if (refNumHeader != null)
				{
					refNumHeader.ZD_ExpiryDate = refNumHeader.ClassHeader.ZA_ExpiryDate;
					var refNumber = Processor.Factory.New<CACTaxRefNumber>();
					using (refNumber.GetValidationSuspender())
					{
						refNumber.ZE_ZD_TaxRefNumHeader = refNumHeader.PK;
						refNumber.ZE_GSTRefNumber = rate.ZH_TaxRefNumber;
						parent.LastTaxRefNumbersQueue.Enqueue(refNumber);
					}
				}
				else
				{
					FireParentRecordNotFoundError(match.Value);
				}
			}

			#endregion
		}

		#endregion

		#region A54RecordParser

		class A54RecordParser : D10RecordParser
		{
			public A54RecordParser() : base(false, false, false) { }

			protected override void ParseTaxRate(CACTaxRate rate, Match match)
			{
				base.ParseTaxRate(rate, match);
				var parent = ((A20RecordParser)Parent);
				var refNumHeader = parent.LastTaxRefNumHeader;
				if (refNumHeader != null)
				{
					var taxRefNumber = parent.LastTaxRefNumbersQueue.Count > 0
										? parent.LastTaxRefNumbersQueue.Dequeue()
										: Processor.Factory.New<CACTaxRefNumber>();

					using (taxRefNumber.GetValidationSuspender())
					{
						taxRefNumber.ZE_ZD_TaxRefNumHeader = refNumHeader.PK;
						taxRefNumber.ZE_ExciseTaxRefNumber = rate.ZH_TaxRefNumber;
					}
				}
				else
				{
					FireParentRecordNotFoundError(match.Value);
				}
			}
		}

		#endregion

		#region A60RecordParser

		class A60RecordParser : CadexRecordParser
		{
			public A60RecordParser() : base(false, false) { }

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					var effectiveDate = ParseDate(match.Groups["ExciseEffDate"].Value, match.Value);
					var classHeader = ((A20RecordParser)Parent).GetClassHeader(effectiveDate, match.Groups["ClassNumber"].Value, this);
					if (classHeader != null)
					{
						A60RecordParser.ProcessA60Record(Processor, classHeader, effectiveDate, match, ParseDate(match.Groups["ExciseExpDate"].Value, match.Value));
					}
					else
					{
						FireParentRecordNotFoundError(record);
					}
				}
				return match.Success;
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return A60RegexPattern;
				}
			}

			internal static void ProcessA60Record(ICadexProcessor processor, CACClassHeader classHeader, ZDateTime effectiveDate, Match match, ZDateTime expiryDate)
			{
				var exciseRateHeader = CadexParsersUtilities.LoadOrCreateRateHeader(processor, classHeader, effectiveDate, CACRateHeader.RateType.ExciseDutyRate);
				using (exciseRateHeader.GetValidationSuspender())
				{
					exciseRateHeader.ZB_ExpiryDate = expiryDate;
					exciseRateHeader.ZB_FreeIndInfo.SetValueFromString(match.Groups["Free"].Value);
					exciseRateHeader.ZB_UnitOfMeasure = match.Groups["UOM"].Value.Trim();
					exciseRateHeader.ZB_InactiveInfo.SetValueFromString(match.Groups["Inactive"].Value);
					exciseRateHeader.ZB_DutyRateAuthorityNumber = match.Groups["RateAuthNumber"].Value.TrimEnd();

					var rate = processor.Factory.New<CACRate>();
					using (rate.GetValidationSuspender())
					{
						rate.ZC_ParentID = exciseRateHeader.PK;
						rate.ZC_ParentTableCode = CACRateHeaderSchema.Constants.Prefix;
						ParseRateLines(rate, match, processor);
					}
				}
			}

			internal const string A60RegexPattern = @"^[AGR]A60
(?<ClassNumber>\d{10})
(?<ExciseEffDate>\d{8})
(?<ExciseExpDate>\d{8})
(?<Free>[YN])
(?<UOM>[\w\s]{3})
(?<Rate>
(?<RateType>[\w\s])
(?<RateRegular>\d{9})
(?<RateMin>\d{9})
(?<RateMax>\d{9})-?){0,2}
(?<Inactive>[YN])$";

			#endregion
		}

		class A60FreeStandingRecordParser : CadexRecordParser
		{
			public A60FreeStandingRecordParser() : base(false, true) { }

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					var effectiveDate = ParseDate(match.Groups["ExciseEffDate"].Value, match.Value);
					var classificationNumber = match.Groups["ClassNumber"].Value;
					var classHeader = CACClassHeader.Load(Processor.Factory, effectiveDate, classificationNumber);
					if (classHeader != null)
					{
						A60RecordParser.ProcessA60Record(Processor, classHeader, effectiveDate, match, ParseDate(match.Groups["ExciseExpDate"].Value, match.Value));
					}
					else
					{
						FireParentRecordNotFoundError("Class Header");
					}
				}
				return match.Success;
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return A60RecordParser.A60RegexPattern;
				}
			}

			#endregion
		}

		#endregion

		#region A70RecordParser

		class A70RecordParser : B20RecordParser
		{
			public A70RecordParser()
				: base(false, true)
			{
			}
		}

		#endregion

		#region A71RecordParser

		class A71RecordParser : A21RecordParser
		{
			protected override string GetDescription()
			{
				return "Tariff Code";
			}
		}

		#endregion

		#region A80RecordParser

		class A80RecordParser : B30RecordParser
		{
		}

		#endregion

		#region B20RecordParser

		class B20RecordParser : CadexRecordParser
		{
			public B20RecordParser() : base(true, true) { }

			protected B20RecordParser(bool mandatory, bool saveRequired)
				: base(mandatory, saveRequired)
			{
			}

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					var tariffCode = match.Groups["TariffCode"].Value;
					var effectiveDate = ParseDate(match.Groups["AuthEffDate"].Value, match.Value);
					LastParsedObject = CadexParsersUtilities.LoadOrCreateTariffHeader(Processor, effectiveDate, tariffCode);
					using (LastParsedObject.GetValidationSuspender())
					{
						LastParsedObject.ZF_AuthExpiryDate = ParseDate(match.Groups["AuthExpDate"].Value, match.Value);
						LastParsedObject.ZF_RateEffectiveDate = ParseDate(match.Groups["RateEffDate"].Value, match.Value);
						LastParsedObject.ZF_RateExpiryDate = ParseDate(match.Groups["RateExpDate"].Value, match.Value);
						LastParsedObject.ZF_TariffCodeAuthorityNumber = match.Groups["AuthNumber"].Value;
						LastParsedObject.ZF_InactiveInfo.SetValueFromString(match.Groups["Inactive"].Value);
						LastParsedObject.ZF_FreeIndInfo.SetValueFromString(match.Groups["Free"].Value);
						LastParsedObject.ZF_GST0RateIndInfo.SetValueFromString(match.Groups["GST0Rate"].Value);
					}
				}
				else
				{
					LastParsedObject = null;
				}
				return match.Success;
			}

			internal CACTariffHeader LastParsedObject { get; private set; }

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^([AG]B20|RA70)
(?<TariffCode>\d{4})
(?<AuthEffDate>\d{8})
(?<AuthExpDate>\d{8})
(?<RateEffDate>\d{8})
(?<RateExpDate>\d{8})
(?<AuthNumber>.{13})
(?<Inactive>[YN])
(?<Free>[YN])
(?<GST0Rate>[YN])
(?<RefNumber>\w{4})?$";
				}
			}

			#endregion
		}

		#endregion

		#region B30RecordParser

		class B30RecordParser : CadexRecordParser
		{
			public B30RecordParser() : base(true, false) { }

			public override bool Mandatory
			{
				get { return Parent.LastParsedObject != null && !Parent.LastParsedObject.ZF_FreeInd; }
			}

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					var lastTariffHeader = Parent.LastParsedObject;
					if (lastTariffHeader != null)
					{
						var rate = Processor.Factory.New<CACRate>();
						using (rate.GetValidationSuspender())
						{
							rate.ZC_ParentID = lastTariffHeader.PK;
							rate.ZC_ParentTableCode = CACTariffHeaderSchema.Constants.Prefix;
							rate.ZC_TreatmentCode = match.Groups["TreatmentCode"].Value;
							rate.ZC_FreeIndInfo.SetValueFromString(match.Groups["Free"].Value);
							rate.ZC_InactiveInfo.SetValueFromString(match.Groups["Inactive"].Value);

							ParseRateLines(rate, match, Processor);
						}
					}
					else
					{
						FireParentRecordNotFoundError(record);
					}
				}
				return match.Success;
			}

			new B20RecordParser Parent
			{
				get { return (B20RecordParser)base.Parent; }
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^([AG]B30|RA80)
(?<Free>[YN])
(?<TreatmentCode>\d{2})
(?<Rate>
(?<RateType>[\w\s])
(?<RateRegular>\d{9})
(?<RateMin>\d{9})
(?<RateMax>\d{9})-?){0,4}
(?<Inactive>[YN])$";
				}
			}

			#endregion
		}

		#endregion

		#region C10RecordParser

		class C10RecordParser : D10RecordParser
		{
			public C10RecordParser(bool isFile) : base(isFile) { }

			protected C10RecordParser(bool mandatory, bool saveRequired, bool isFile)
				: base(mandatory, saveRequired, isFile)
			{
			}

			#region TryParse

			protected override void ParseTaxRate(CACTaxRate rate, Match match)
			{
				base.ParseTaxRate(rate, match);
				rate.ZH_CheckIndInfo.SetValueFromString(match.Groups["RateCheck"].Value);
				rate.ZH_CheckGroup = match.Groups["CheckGroup"].Value;
			}

			protected override ZString TaxType
			{
				get { return CACTaxRate.TaxType.GST; }
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^([AG]C10|RA52)
(?<ReferenceNumber>[\w\d]{3})
(?<RateEffDate>\d{8})
(?<RateExpDate>\d{8})
(?<RateCheck>[YN])
(?<CheckGroup>\d{1})
(?<RateType>[\w\s])
(?<Rate>\d{9})
(?<UOM>[\w\s]{3})
(?<Title>.{60})
(?<Inactive>[YN])$";
				}
			}

			#endregion
		}

		#endregion

		#region D10RecordParser

		class D10RecordParser : CadexRecordParser
		{
			public D10RecordParser(bool isFile)
				: this(true, true, isFile)
			{
			}

			protected D10RecordParser(bool mandatory, bool saveRequired, bool isFile)
				: base(mandatory, saveRequired, isFile)
			{
				processedRecordPKs = new List<ZGuid>();
			}

			#region TryParse

			public override bool TryParse(string record)
			{
				var match = Regex.Match(record);
				if (match.Success)
				{
					var refNumber = match.Groups["ReferenceNumber"].Value;

					var rate = CadexParsersUtilities.LoadOrCreateTaxRate(Processor.Factory, refNumber, TaxType, ParseDate(match.Groups["RateEffDate"].Value, match.Value));
					using (rate.GetValidationSuspender())
					{
						ParseTaxRate(rate, match);
					}
				}
				return match.Success;
			}

			protected virtual void ParseTaxRate(CACTaxRate rate, Match match)
			{
				rate.ZH_ExpiryDate = ParseDate(match.Groups["RateExpDate"].Value, match.Value);
				rate.ZH_RateType = match.Groups["RateType"].Value;
				rate.ZH_Rate = ParseDecimal(match.Groups["Rate"].Value);
				rate.ZH_UnitOfMeasure = match.Groups["UOM"].Value.Trim();
				rate.ZH_Title = match.Groups["Title"].Value.TrimEnd();
				rate.ZH_InactiveInfo.SetValueFromString(match.Groups["Inactive"].Value);
				if (IsFile)
				{
					ProcessedRecordPKs.Add(rate.PK);
				}
			}

			protected virtual ZString TaxType
			{
				get { return CACTaxRate.TaxType.Excise; }
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^([AG]D10|RA54)
(?<ReferenceNumber>[\w\d]{3})
(?<RateEffDate>\d{8})
(?<RateExpDate>\d{8})
(?<RateType>[\w\s])
(?<Rate>\d{9})
(?<UOM>[\w\s]{3})
(?<Title>.{60})
(?<Inactive>[YN])$";
				}
			}

			#endregion

			[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
			protected override IEnumerable<ZString> UpdateScripts
			{
				get
				{
					var result = new List<ZString>();
					if (IsFile)
					{
						var expiryDate = ZDateTime.Now.AddDays(-1).Date.ToISO8601ShortDateString();
						ZString whereClause = ProcessedRecordPKs.Any() ? $"{CACTaxRate.Schema.PK} NOT IN ({string.Join(", ", ProcessedRecordPKs.Select(pk => $"'{pk.ToString()}'"))}) AND {CACTaxRate.Schema.ZH_TaxType} = '{TaxType}'" : ZQuery.NoResultQuery.LiteralTextSqlFormatted;
						result.Add($"UPDATE {CACTaxRate.Schema.TableName} SET {CACTaxRate.Schema.ZH_ExpiryDate} = '{expiryDate}' \r\n\tWHERE 1 = 1 AND {(!whereClause.IsEmpty ? whereClause + "AND" : string.Empty)} {CACTaxRate.Schema.ZH_ExpiryDate} > '{expiryDate}'");
					}
					return result;
				}
			}

			public List<ZGuid> ProcessedRecordPKs
			{
				get { return processedRecordPKs; }
			}
			readonly List<ZGuid> processedRecordPKs;
		}

		#endregion

		#region E10RecordParser

		class E10RecordParser : CadexRecordParser
		{
			public E10RecordParser(ICadexProcessor processor) : base(processor, true, true) { }

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					LastMatch = match;
					IsValid = true;
					Errors = new List<string>();
				}
				return match.Success;
			}

			public override string RecordProcessedNotification
			{
				get
				{
					var builder = new ZStringBuilder();

					builder.Append($"Exchange rates for '{ParseDate(LastMatch.Groups["EffDate"].Value, LastMatch.Value).ToShortDateString()}' has been processed.");
					Errors.Aggregate(builder, (b, e) => b.Append(e));
					builder.Append($"Query Record Content: '{LastMatch.Value}'");
					return builder.ToStringWithNewLineBetweenAppends();
				}
			}

			Match LastMatch { get; set; }
			internal List<string> Errors { get; private set; }
			internal bool IsValid { get; set; }

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^RE10
(?<CountryCode>[\w\s]{2})
(?<CurrencyCode>[\w\s]{3})
(?<EffDate>\d{8})
(?<RefNumber>\w{4})?$";
				}
			}

			#endregion
		}

		#endregion

		#region E20RecordParser

		class E20RecordParser : CadexRecordParser
		{
			public E20RecordParser() : this(null, true, false) { }

			protected E20RecordParser(ICadexProcessor processor, bool mandatory, bool saveRequired)
				: base(processor, mandatory, saveRequired)
			{
				currencyCodesDic = new Dictionary<ZString, ZDecimal>();
			}

			public override bool Mandatory
			{
				get { return Parent == null ? base.Mandatory : Parent.IsValid; }
			}

			new E10RecordParser Parent
			{
				get { return base.Parent as E10RecordParser; }
			}

			#region TryParse

			public override bool TryParse(string record)
			{
				Match match;
				if ((match = Regex.Match(record)).Success)
				{
					var currencyCode = match.Groups["CurrencyCode"].Value;

					var currency = RefCurrency.LoadFromCurrencyCode(Processor.Factory, currencyCode);
					if (currency == null)
					{
						currency = RefCurrency.New(Processor.Factory);
						using (currency.GetValidationSuspender())
						{
							currency.RX_Code = currencyCode;
							currency.RX_Desc = match.Groups["CurrencyName"].Value;
						}
					}

					var key = currencyCode + "|" + match.Groups["EffDate"].Value;
					if (!currencyCodesDic.ContainsKey(key))
					{
						currencyCodesDic.Add(key, CadexRecordParser.ParseDecimal(match.Groups["ExchangeRate"].Value, 2));
					}
				}
				return match.Success;
			}

			#endregion

			#region RegexPattern

			protected override string RegexPattern
			{
				get
				{
					return @"^(AE10|RE20)
(?<CountryCode>[\w\s]{2})
(?<CountryName>.{30})
(?<CurrencyCode>\w{3})
(?<CurrencyName>.{30})
(?<EffDate>\d{8})
(?<ExchangeRate>\d{8})
(?<RefNumber>.{4})?$";
				}
			}

			#endregion

			protected override IEnumerable<ZString> UpdateScripts
			{
				get
				{
					var companyPK = GlbCompany.CurrentCompany.PK;
					foreach (var item in currencyCodesDic)
					{
						var keySpliter = item.Key.Split('|');
						var currencyCode = keySpliter[0];
						var effectiveDate = ParseDate(keySpliter[1], item.Key);

						if (effectiveDate.IsValid)
						{
							var expiryDate = new ZDateTime(effectiveDate.Year, effectiveDate.Month, effectiveDate.Day, 23, 59, 00);
							var sql =
								$"MERGE INTO dbo.ZZRefExchangeRate AS ExchangeRate USING (SELECT '{currencyCode}' AS RE_RX_NKExCurrency, '{companyPK}' as RE_GC, " +
								$"'{effectiveDate.SqlFormat}' AS RE_StartDate, '{Core.Constants.ExchangeRateTypes.Code.CustomsRate}' AS RE_ExRateType, {item.Value} AS RE_SellRate) AS SOURCE \r\n" +
								$"ON SOURCE.RE_RX_NKExCurrency = ExchangeRate.RE_RX_NKExCurrency AND SOURCE.RE_GC = ExchangeRate.RE_GC \r\n" +
								$"AND SOURCE.RE_StartDate = ExchangeRate.RE_StartDate AND SOURCE.RE_ExRateType = ExchangeRate.RE_ExRateType \r\n" +
								$"WHEN MATCHED THEN UPDATE SET ExchangeRate.RE_SellRate= SOURCE.RE_SellRate \r\n" +
								$"WHEN NOT MATCHED THEN INSERT(RE_PK, RE_ExRateType, RE_StartDate, RE_ExpiryDate, RE_SellRate, RE_RX_NKExCurrency, RE_GC, RE_AsPublished) \r\n" +
								$"VALUES (NEWID(), SOURCE.RE_ExRateType, SOURCE.RE_StartDate, '{expiryDate.SqlFormat}', SOURCE.RE_SellRate, SOURCE.RE_RX_NKExCurrency, SOURCE.RE_GC, '');\r\n";
							yield return sql;
						}
					}
				}
			}

			readonly Dictionary<ZString, ZDecimal> currencyCodesDic;
		}

		#endregion

		#region YRecordParser

		class YRecordParser : CadexRecordParser
		{
			public YRecordParser() : base(true, true) { }

			public override string RecordName
			{
				get { return "Y"; }
			}

			public override bool TryParse(string record)
			{
				return Regex.IsMatch(record);
			}

			protected override string RegexPattern
			{
				get { return @"^Y[\w\d\s]{3}\d{5}.{16}\d{8}[\w\d\s]{3}\w{2}[\w\d\s]{9}\d{20}$"; }
			}
		}

		#endregion

		#region OtherRecordParser

		class OtherRecordParser : CadexRecordParser
		{
			public OtherRecordParser() : base(false, false) { }

			public override bool TryParse(string record)
			{
				return Regex.IsMatch(record);
			}

			protected override string RegexPattern
			{
				get
				{
					return @"
^[AG]A10[YN][\s\d]{20}[\w\d\s]{1}\d{8}[YN]$
|^[AG][CD]20$
|^[AG]B10[YN][\s\d\w]{8}\d{8}[YN]$";
				}
			}
		}

		#endregion

		#region CadexRecordWrapper

		class CadexRecordWrapper : CadexRecordParser
		{
			public CadexRecordWrapper(bool parseOneChildOnly)
				: this(null, parseOneChildOnly)
			{
			}

			public CadexRecordWrapper(ICadexProcessor processor, bool parseOneChildOnly)
				: base(processor, true, false)
			{
				ParseOneChildOnly = parseOneChildOnly;
			}

			public override string RecordName
			{
				get
				{
					var result = new ZStringBuilder();
					ChildParsers.Aggregate(result, (builder, child) => builder.AppendIfNotEmpty(child.IsWrapper && !child.Mandatory ? string.Empty : child.RecordName));
					return result.ToStringWithDelimiterBetweenAppends(", ");
				}
			}

			public override bool IsMatchAny(string record)
			{
				return ChildParsers.Any(parser => parser.IsMatchAny(record));
			}

			public bool ParseOneChildOnly { get; private set; }

			#region TryParse

			public override bool TryParse(string record)
			{
				throw new NotSupportedException("TryParse() should not be used by wrapper");
			}

			protected override string RegexPattern
			{
				get { throw new NotSupportedException("RegexPattern should not be used by wrapper"); }
			}

			#endregion
		}

		#endregion

		#region A20RecordWrapper

		class A20RecordWrapper : CadexRecordWrapper
		{
			public A20RecordWrapper() : base(true) { }

			public override bool Mandatory
			{
				get { return Parent.IsValid && !string.IsNullOrEmpty(Parent.LastMatch.Groups["ClassNumber"].Value.Trim()); }
			}

			new A10RecordParser Parent
			{
				get { return (A10RecordParser)base.Parent; }
			}
		}

		#endregion

		#region A70RecordWrapper

		class A70RecordWrapper : CadexRecordWrapper
		{
			public A70RecordWrapper() : base(true) { }

			public override bool Mandatory
			{
				get { return Parent.IsValid && !string.IsNullOrEmpty(Parent.LastMatch.Groups["TariffCode"].Value.Trim()); }
			}

			new A10RecordParser Parent
			{
				get { return (A10RecordParser)base.Parent; }
			}
		}

		#endregion
	}
}
