using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration.OldBillingTransactions;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Billing.Integration
{
	public sealed class BillingManager
	{
		public static string CurrentSchema
		{
			get { return "http://www.edi.com.au/EnterpriseService/#Billing_" + CurrentSchemaVersion; } // XML schema cannot be localized
		}

		public void AddTransactions(IEnumerable<UsageTransaction> transactions, DbConnection dbConnection)
		{
			var factory = new BusinessObjectFactory(dbConnection);
			AddTransactions(transactions, factory);
			factory.Save();
		}

		public void AddTransactions(IEnumerable<UsageTransaction> transactions, BusinessObjectFactory factory)
		{
			foreach (var transaction in transactions)
			{
				var xml = GenerateTransactionXml(transaction);
				var stmUsageData = factory.New(StmUsageDataType);
				stmUsageData[StmUsageDataSchema.Constants.SUD_Data] = BillingDataEncryptor.Encrypt(Encoding.GetBytes(xml));
				stmUsageData[StmUsageDataSchema.Constants.SUD_Schema] = CurrentUsageSchemaVersion;
				stmUsageData[StmUsageDataSchema.Constants.SUD_Category] = UsageCategory;
				stmUsageData[StmUsageDataSchema.Constants.SUD_Code] = UsagePriceItemCode;
				stmUsageData[StmUsageDataSchema.Constants.SUD_Fail] = false;
				stmUsageData[StmUsageDataSchema.Constants.SUD_SubmissionPriority] = (byte)SubmissionPriority.NotMandatoryForMilestone;
			}
		}

		public void AddTransactions(IEnumerable<BillingTransaction> transactions, DbConnection dbConnection)
		{
			AddTransactions(transactions.Select(t => new BillingTransactionWrapper(t, SubmissionPriority.Default)), dbConnection);
		}

		public void AddTransactions(IEnumerable<BillingTransactionWrapper> transactionWrappers, DbConnection dbConnection)
		{
			var factory = new BusinessObjectFactory(dbConnection);
			AddTransactions(transactionWrappers, factory);
			factory.Save();
		}

		public void AddTransactions(IEnumerable<BillingTransactionWrapper> transactionWrappers, BusinessObjectFactory factory)
		{
			if (transactionWrappers == null || !transactionWrappers.Any())
			{
				return;
			}

			var rules = GetFieldLengthRestrictions();
			foreach (var transactionWrapper in transactionWrappers)
			{
				var transaction = transactionWrapper.Transaction;
				ModifyReferenceFieldsData(transaction, rules);
				var xml = GenerateTransactionXml(transaction);
				var stmUsageData = factory.New(StmUsageDataType);
				var category = GetCategory(transaction);
				var priceItemCode = GetPriceItemCode(transaction);
				stmUsageData[StmUsageDataSchema.Constants.SUD_Data] = BillingDataEncryptor.Encrypt(Encoding.GetBytes(xml));
				stmUsageData[StmUsageDataSchema.Constants.SUD_Schema] = CurrentSchemaVersion;
				stmUsageData[StmUsageDataSchema.Constants.SUD_Category] = category;
				stmUsageData[StmUsageDataSchema.Constants.SUD_Code] = priceItemCode;
				stmUsageData[StmUsageDataSchema.Constants.SUD_Fail] = false;
				stmUsageData[StmUsageDataSchema.Constants.SUD_SubmissionPriority] = (byte)transactionWrapper.SubmissionPriority;
			}
		}

#if DEBUG
		public IEnumerable<BusinessObject> GetTransactions(BusinessObjectFactory factory, int maxCount)
		{
			var pendingItems = GetPendingTransactions(factory, maxCount);
			return ValidatePendingTransactions(factory, pendingItems);
		}
#endif

		public IEnumerable<BusinessObject> GetPendingTransactions(BusinessObjectFactory factory, int maxCount) => LoadStmUsageDataItems(factory, maxCount);

		public IEnumerable<BusinessObject> ValidatePendingTransactions(BusinessObjectFactory factory, IEnumerable<BusinessObject> items)
		{
			var validItems = new List<BusinessObject>();
			var failedTransactionPKs = new List<ZGuid>();
			foreach (var item in items)
			{
				var category = GetCategory(item);
				var priceItemCode = GetPriceItemCode(item);
				if (category == NoCategory ||
					category == InvalidCategory)
				{
					ErrorReporter.ReportOnce("NotFailedCategory [" + category + "]", "XML: [" + GetTransactionXml(item) + "]");
					failedTransactionPKs.Add(item.PK);
				}
				else if (priceItemCode == NoPriceItemCode ||
							priceItemCode == InvalidPriceItemCode)
				{
					ErrorReporter.ReportOnce("NotFailedPriceItemCode [" + priceItemCode + "]", "XML: [" + GetTransactionXml(item) + "]");
					failedTransactionPKs.Add(item.PK);
				}
				else
				{
					validItems.Add(item);
				}
			}
			MarkAsFailed(failedTransactionPKs.ToArray());

			return validItems;
		}

		public static string EncryptTransaction(BillingTransaction transaction)
		{
			var xml = GenerateTransactionXml(transaction);
			return BillingDataEncryptor.Encrypt(Encoding.GetBytes(xml));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML element is not localizable")]
		public static BillingTransaction DecryptTransaction(string data, string schemaVersion)
		{
			IOldBillingTransaction oldBillingTransaction;
			if (string.IsNullOrEmpty(schemaVersion))
			{
				try
				{
					oldBillingTransaction = DecryptTransaction<OldBillingTransactions.NoVersion.BillingTransaction>(data);
				}
				catch (InvalidOperationException e)
				{
					const string billingTransactionHeaderStart = "<BillingTransaction xmlns='http://www.edi.com.au/EnterpriseService/#Billing_";
					if (e.InnerException is InvalidOperationException innerException && innerException.Message.StartsWith(billingTransactionHeaderStart, StringComparison.Ordinal))
					{
						return DecryptTransaction(data, innerException.Message.Substring(billingTransactionHeaderStart.Length, 3));
					}
					throw;
				}
			}
			else
			{
				switch (schemaVersion)
				{
					case "1.2":
						oldBillingTransaction = DecryptTransaction<OldBillingTransactions.OneTwo.BillingTransaction>(data);
						break;
					case CurrentSchemaVersion:
						return DecryptTransaction<BillingTransaction>(data);
					default:
						throw new ArgumentOutOfRangeException(nameof(schemaVersion), schemaVersion, "Unknown billing transaction schema version.");
				}
			}
			return oldBillingTransaction.ToLatest();
		}

		public static UsageTransaction DecryptUsageTransaction(string data) => DecryptTransaction<UsageTransaction>(data);

		public static byte[] GetTransactionData(BusinessObject stmUsageData)
		{
			return BillingDataEncryptor.Decrypt(stmUsageData[StmUsageDataSchema.Constants.SUD_Data].ToString());
		}

		public static string GetTransactionXml(BusinessObject stmUsageData)
		{
			return Encoding.GetString(GetTransactionData(stmUsageData));
		}

		public static void MarkAsFailed(params ZGuid[] pks)
		{
			var factory = new BusinessObjectFactory();
			var stmUsageDataList = factory.Load(StmUsageDataType, new ZQuery(StmUsageDataSchema.PK, pks));
			foreach (var stmUsageData in stmUsageDataList)
			{
				stmUsageData[StmUsageDataSchema.Constants.SUD_Fail] = true;
			}
			factory.Save();
		}

		public static string GetCategory(BillingTransaction transaction)
		{
			if (string.IsNullOrWhiteSpace(transaction.Category))
			{
				return NoCategory;
			}

			if (transaction.Category.Length != 3)
			{
				return InvalidCategory;
			}

			return transaction.Category;
		}

		public static string GetPriceItemCode(BillingTransaction transaction)
		{
			if (string.IsNullOrWhiteSpace(transaction.PriceItemCode))
			{
				return NoPriceItemCode;
			}

			if (transaction.PriceItemCode.Length != 3)
			{
				return InvalidPriceItemCode;
			}

			return transaction.PriceItemCode;
		}

		public static Encoding Encoding
		{
			get { return new UTF8Encoding(false); }
		}

		public static XmlSchemaSet Schemas
		{
			get { return schemas ?? (schemas = CreateSchemas()); }
		}

		static XmlSchemaSet CreateSchemas()
		{
			var result = new XmlSchemaSet();
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Billing.Integration.BillingTransaction.xsd");
			using (var reader = new StreamReader(stream))
			{
				result.Add(CurrentSchema, XmlReader.Create(reader));
			}
			return result;
		}

		internal static string GetCategory(BusinessObject stmUsageData)
		{
			return stmUsageData[StmUsageDataSchema.Constants.SUD_Category].ToString();
		}

		internal static string GetPriceItemCode(BusinessObject stmUsageData)
		{
			return stmUsageData[StmUsageDataSchema.Constants.SUD_Code].ToString();
		}

		public static string GenerateTransactionXml<T>(T transaction)
		{
			using (var stream = new MemoryStream())
			using (var writer = new XmlTextWriter(stream, Encoding))
			{
				writer.Formatting = Formatting.Indented;
				var serializer = ZXmlSerializer.New(typeof(T));
				serializer.Serialize(writer, transaction);
				return Encoding.GetString(stream.ToArray());
			}
		}

		class ReferenceFieldLengthRestriction
		{
			public string FieldName { get; set; }
			public int? MinLength { get; set; }
			public int? MaxLength { get; set; }
		}

		List<ReferenceFieldLengthRestriction> ruleCache;
		readonly object ruleCacheLock = new object();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		List<ReferenceFieldLengthRestriction> GetFieldLengthRestrictions()
		{
			const string referenceFieldNamePrefix = "Reference"; // XML element name
			const string minLengthFieldName = "minLength"; // XML element name
			const string maxLengthFieldName = "maxLength"; // XML element name

			lock (ruleCacheLock)
			{
				if (ruleCache != null)
				{
					return ruleCache;
				}

				ruleCache = new List<ReferenceFieldLengthRestriction>();

				var assembly = Assembly.GetExecutingAssembly();
				var billingTransactionXsd = new XmlDocument();
				using (var stream = assembly.GetManifestResourceStream(assembly.GetName().Name + "." + "BillingTransaction.xsd")) // resource name
				{
					billingTransactionXsd.Load(stream);
				}
				var nsmgr = new XmlNamespaceManager(billingTransactionXsd.NameTable);
				nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");  // XML namespaces

				for (int i = 1; i <= 5; i++)
				{
					string fullReferenceFieldName = referenceFieldNamePrefix + i.ToString(CultureInfo.InvariantCulture);

					int? referenceMinLength = FindLengthRestrictionValue(billingTransactionXsd, nsmgr, fullReferenceFieldName, minLengthFieldName);
					int? referenceMaxLength = FindLengthRestrictionValue(billingTransactionXsd, nsmgr, fullReferenceFieldName, maxLengthFieldName);
					ruleCache.Add(new ReferenceFieldLengthRestriction
					{
						FieldName = fullReferenceFieldName,
						MinLength = referenceMinLength,
						MaxLength = referenceMaxLength
					});
				}
			}
			return ruleCache;
		}

		static int? FindLengthRestrictionValue(XmlDocument doc, XmlNamespaceManager nsmgr, string fieldName, string lengthRestrictionName)
		{
			var fieldAttribute = doc.SelectSingleNode($"//xs:element[@name='{fieldName}']/xs:simpleType/xs:restriction/xs:{lengthRestrictionName}/@value", nsmgr); // xpath query string
			if (fieldAttribute != null && int.TryParse(fieldAttribute.Value, out int ret))
			{
				return ret;
			}
			return null;
		}

		static void ModifyReferenceFieldsData(BillingTransaction transaction, List<ReferenceFieldLengthRestriction> fieldRules)
		{
			if (transaction != null)
			{
				foreach (var fieldRule in fieldRules)
				{
					var referencePropertyInfo = transaction.GetType().GetProperty(fieldRule.FieldName);
					var currentValue = (string)referencePropertyInfo.GetValue(transaction);
					var newValue = FixReferenceLengthIfNeeded(currentValue, fieldRule.MinLength, fieldRule.MaxLength);
					referencePropertyInfo.SetValue(transaction, newValue);
				}
			}
		}

		static string FixReferenceLengthIfNeeded(string transactionField, int? minReferenceLength, int? maxReferenceLength)
		{
			var referenceLength = transactionField?.Length ?? 0;

			if (minReferenceLength.HasValue && referenceLength < minReferenceLength)
			{
				var numCharsToPad = minReferenceLength.Value - referenceLength;
				transactionField = (transactionField ?? string.Empty) + new string('.', numCharsToPad);
			}
			else if (maxReferenceLength.HasValue && referenceLength > maxReferenceLength)
			{
				transactionField = transactionField.Substring(0, maxReferenceLength.Value);
			}
			return transactionField;
		}

		public static BusinessObject[] LoadStmUsageDataItems(IFactory factory, int maxCount)
		{
			var query = new ZQuery(StmUsageDataSchema.SUD_Code, SQLComparisonOperator.NotEqual, "") { MaximumRows = maxCount };
			query.AddToFilter(StmUsageDataSchema.SUD_Category, SQLComparisonOperator.NotEqual, "");
			query.AddToFilter(StmUsageDataSchema.SUD_Fail, false);
			query.AddToFilter(StmUsageDataSchema.SUD_Fixing, false);
			query.OrderBy = StmUsageDataSchema.SUD_SubmissionPriority.Name + "," + StmUsageDataSchema.SUD_PostedTimeUtc.Name;
			query.TableIndexHints.Add(new TableIndexHint(StmUsageDataSchema.Constants.Indexes.NR_RX__SUD_SubmissionPriority_SUD_PostedTimeUtc));
			query.QueryHints = QueryHints.RECOMPILE;
			return factory.Load(StmUsageDataType, query);
		}

		static TBillingTransaction DecryptTransaction<TBillingTransaction>(string data)
		{
			using (var stream = new MemoryStream(BillingDataEncryptor.Decrypt(data)))
			using (var reader = new StreamReader(stream, Encoding))
			{
				var serializer = ZXmlSerializer.New(typeof(TBillingTransaction));
				return (TBillingTransaction)serializer.Deserialize(reader);
			}
		}

		// If you change billing schema, make sure that eHub is updated first. Otherwise transactions will be lost.
		public const string CurrentSchemaVersion = "1.4";
		public const string ReportingSource = "ENT";
		public const string CurrentUsageSchemaVersion = "2.1";
		public const string UsageCategory = "USG";
		public const string UsagePriceItemCode = "USG";

		public const string NoCategory = "???";
		public const string InvalidCategory = "###";
		public const string NoPriceItemCode = "???";
		public const string InvalidPriceItemCode = "###";

#if DEBUG
		public
#endif
		static readonly Type StmUsageDataType = ObjectFactory.GetType("IStmUsageData");

		[ThreadStatic] static XmlSchemaSet schemas;

		class XmlTextWriter : System.Xml.XmlTextWriter
		{
			public XmlTextWriter(Stream w, Encoding encoding) : base(w, encoding)
			{
			}

			public XmlTextWriter(string filename, Encoding encoding) : base(filename, encoding)
			{
			}

			public XmlTextWriter(TextWriter w) : base(w)
			{
			}

			public override void WriteString(string text)
			{
				base.WriteString(ZXmlValidation.EscapeInvalidXmlCharacters(text));
			}
		}
	}
}
