using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

#if DEBUG
using NUnit.Framework;
#endif

namespace CargoWise.EntityFramework
{
	[SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Inherited in Enteprise.Registry.Business")]
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class TranslatableDataFieldAttribute : SingleMetaDataAttribute, ICustomizableDataCaptionSource
	{
		public const string CDRSMetaDataTypeId = "CDRS";

		static TranslatableDataFieldAttribute()
		{
			MetaDataType.RegisterMetaDataType(new MetaDataType(CDRSMetaDataTypeId, typeof(CustomizableDataResourceStrings), null));
		}

		public TranslatableDataFieldAttribute(string tableName, string columnName)
			: this(tableName, columnName, null)
		{ }

		public TranslatableDataFieldAttribute(string tableName, string columnName, string dataXmlFilePaths)
			: this(tableName, columnName, dataXmlFilePaths, null)
		{ }

		public TranslatableDataFieldAttribute(string tableName, string columnName, int maxLength, string contextColumnName)
			: this(tableName, columnName, null, contextColumnName)
		{
			MaxLength = maxLength;
		}

		public TranslatableDataFieldAttribute(string tableName, string columnName, string dataXmlFilePaths, string contextColumnName)
			: base(CDRSMetaDataTypeId)
		{
			this.tableName = tableName;
			this.columnName = columnName;
			this.dataXmlFilePaths = dataXmlFilePaths;
			this.contextColumnName = contextColumnName;
		}

		public Type Type
		{
			get;
			set;
		}

		public string SecurityCheckpoint
		{
			get;
			set;
		}

		readonly string tableName;
		readonly string columnName;
		readonly string dataXmlFilePaths;
		readonly string contextColumnName;

		public string ContextColumnName
		{
			get { return contextColumnName; }
		}

		public string Description
		{
			get { return TableDescriptionPrefix + DataBoundResourceStrings.GetDataForProperty(Type, columnName).Caption; }
		}

		protected virtual string TableDescriptionPrefix
		{
			get { return DataBoundResourceStrings.GetStringForTable(Type) + " "; }
		}

		public string RootKeyPrefixWithSeperator
		{
			get { return string.IsNullOrEmpty(contextColumnName) ? KeyPrefixContextColumnName + "$" : KeyPrefixContextColumnName + "@"; }
		}

		internal protected string GetKeyPrefix(object context)
		{
			if (!string.IsNullOrEmpty(contextColumnName))
			{
				var contextValue = GetContextValue(context, contextColumnName);
				if (contextValue.Length > 50)
				{
					using (var md5 = MD5.Create())
					{
						contextValue = contextValue.Substring(0, 25) + Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(contextValue)));
					}
				}
				return KeyPrefixContextColumnName + "@" + contextValue;
			}
			else
			{
				return KeyPrefixContextColumnName;
			}
		}

		protected virtual string KeyPrefixContextColumnName
		{
			get { return columnName; }
		}

		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "BusinessObject.IsNull is evil")]
		static string GetContextValue(object context, string contextColumnName)
		{
			string contextValue;
			if (context is BusinessObject) // to avoid exception for bizo.IsNull
			{
				contextValue = ((BusinessObject)context)[contextColumnName].ToString();
			}
			else if (context is string || context is ZString)
			{
				contextValue = context.ToString();
			}
			else
			{
				throw new ArgumentException("Valid context object must be provided", nameof(context));
			}
			return contextValue;
		}

		public virtual string GetKey(object context, string caption)
		{
			return CustomizableDataResourceStrings.GetCustomizableDataKey(GetKeyPrefix(context), caption.TrimEnd());
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public virtual IEnumerable<IResString> GetCompileTimeSystemCaptions()
		{
			var captions = new HashSet<ResourceString>(new ResourceString.ResourceKeyEqualityComparer());

			if (dataXmlFilePaths == null)
			{
				return captions;
			}

			foreach (var path in dataXmlFilePaths.Split(';'))
			{
				var dataXmlFilePath = path.Trim();

				if (string.IsNullOrEmpty(dataXmlFilePath))
				{
					continue;
				}

				var filterParts = Filter != null ? ((IFilterPartsProvider)Filter).FilterParts : null;

				string rootPath =
#if DEBUG
					TestingState.IsRunningTests ? TestCase.BaseSourcePath :
#endif
						Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

				var dataFilePath = Path.Combine(rootPath, dataXmlFilePath);

				using (var reader = dataFilePath.EndsWith(".gz") ? XmlReader.Create(new GZipStream(File.OpenRead(dataFilePath), CompressionMode.Decompress)) : XmlReader.Create(dataFilePath))
				{
					var filterPartResults = filterParts != null ? new bool?[filterParts.Length] : null;
					bool readingTableItem = false;
					string itemValue = null;
					string itemContext = null;

					while (reader.Read())
					{
						if (reader.NodeType == XmlNodeType.Element && reader.Name == tableName)
						{
							readingTableItem = true;
						}
						else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tableName)
						{
							if (!string.IsNullOrEmpty(itemValue) && ItemMatchesFilter(filterParts, filterPartResults))
							{
								captions.Add(CustomizableDataResourceStrings.GetMultilingualString(this, itemContext, itemValue));
							}

							itemValue = null;
							if (filterPartResults != null)
							{
								Array.Clear(filterPartResults, 0, filterPartResults.Length);
							}
							readingTableItem = false;
						}
						if (readingTableItem)
						{
							if (reader.NodeType == XmlNodeType.Element)
							{
								string elementName = reader.Name;
								string elementValue = null;
								if (elementName == columnName)
								{
									itemValue = elementValue = ResourceString.NormalizeNewLines(reader.ReadElementContentAsString().TrimEnd());
								}
								else if (elementName == contextColumnName)
								{
									itemContext = elementValue = reader.ReadElementContentAsString().TrimEnd();
								}
								if (filterParts != null)
								{
									for (int i = 0; i < filterParts.Length; i++)
									{
										ZSqlParameter sqlParameter;
										if ((sqlParameter = filterParts[i] as ZSqlParameter) != null && elementName == sqlParameter.SchemaColumn.Name)
										{
											if (filterPartResults[i].HasValue)
											{
												throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Multiple matches for {0}", filterParts[i]));
											}
											filterPartResults[i] = sqlParameter.GetPredicate().Invoke(elementValue ?? (elementValue = reader.ReadElementContentAsString().TrimEnd()));
										}
									}
								}
							}
						}
					}
				}
			}
			return captions;
		}

		bool ItemMatchesFilter(IFilterPart[] filterParts, bool?[] filterPartResults)
		{
			bool isMatch = true;
			if (filterParts != null)
			{
				var lastJoinCondition = JoinCondition.And;
				for (int i = 0; i < filterParts.Length; i++)
				{
					if (filterParts[i] is JoinCondition)
					{
						lastJoinCondition = (JoinCondition)filterParts[i];
					}
					else
					{
						if (!filterPartResults[i].HasValue)
						{
							throw new InvalidOperationException(string.Format("No matching field for {0}", filterParts[i]));
						}
						if (lastJoinCondition == JoinCondition.Or)
						{
							isMatch = isMatch || filterPartResults[i].Value;
						}
						else
						{
							isMatch = isMatch && filterPartResults[i].Value;
						}
					}
				}
			}
			return isMatch;
		}

		public virtual IEnumerable<IResString> GetRuntimeCaptions(IResString userCaption = null, object context = null)
		{
			var captions = new HashSet<IResString>(GetRuntimeCaptionsFromDatabase(userCaption, context), new ResourceString.ResourceKeyEqualityComparer());
			if (userCaption != null && !string.IsNullOrEmpty(userCaption.EnglishText))
			{
				captions.Add(userCaption);
			}
			return captions;
		}

		protected virtual IEnumerable<ResourceString> GetRuntimeCaptionsFromDatabase(IResString userCaption = null, object context = null)
		{
			var sql = GetRuntimeCaptionsSQL(context);
			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					string caption = reader[0] as string;
					if (!string.IsNullOrEmpty(caption))
					{
						caption = caption.TrimEnd();
						yield return CustomizableDataResourceStrings.GetMultilingualString(this, string.IsNullOrEmpty(contextColumnName) ? null : reader[1], caption);
					}
				}
			}
		}

		protected virtual string GetRuntimeCaptionsSQL(object context)
		{
			return GetRuntimeCaptionsSQL(context, contextColumnName);
		}

		protected string GetRuntimeCaptionsSQL(object context, string filterContextColumnName)
		{
			string sql;
			if (!string.IsNullOrEmpty(filterContextColumnName))
			{
				var filter = Filter ?? new ZQuery();				
				if (context != null)
				{
					var contextValue = GetContextValue(context, filterContextColumnName);
					filter.AddToFilter(
						ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(filterContextColumnName, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(CargoWise.Schema.Schema.GetPrefixFromColumnName(filterContextColumnName)).TableName),
						contextValue);
				}
				sql = string.Format(CultureInfo.InvariantCulture, "select {0}, {1} from {2} {3}", columnName, filterContextColumnName, tableName, filter.GetAsWhereClause(true));
			}
			else
			{
				if (Filter == null)
				{
					sql = string.Format(CultureInfo.InvariantCulture, "select distinct {0} from {1}", columnName, tableName);
				}
				else
				{
					sql = string.Format(CultureInfo.InvariantCulture, "select distinct {0} from {1} where {2}", columnName, tableName, Filter.LiteralTextSql);
				}
			}
			return sql;
		}

		public int MaxLength
		{
			get;
			set;
		} = -1;

		public virtual ZQuery Filter
		{
			get { return null; }
		}

		/// <summary>
		/// Value is set by ResourceStringAnalyzer code injection
		/// </summary>
		public ushort Asmid { get; set; }

		public override bool ProvidesMetaDataValue(string metaDataTypeId)
		{
			return metaDataTypeId == MetaDataTypeId;
		}

		public override object GetMetaDataValue(string metaDataTypeId)
		{
			return new CustomizableDataResourceStrings(this);
		}

		public override bool ProvidesMetaDataMember(string metaDataTypeId)
		{
			return false;
		}

		public override string GetMetaDataMember(string metaDataTypeId)
		{
			throw new NotImplementedException();
		}

#if DEBUG
		public virtual BusinessObject[] GetSystemDefinedParentObjectsForTest(BusinessObjectFactory factory, Type type)
		{
			return factory.Load(type, new ZQuery(Filter));
		}
#endif

		public static TranslatableDataFieldAttribute GetAttributeForColumn(string columnName, string boName = "")
		{
			TranslatableDataFieldAttribute attribute = null;

			var key = "$" + columnName;
			if (!string.IsNullOrEmpty(boName))
			{
				key += "+" + boName;
			}

			var type = DataBoundResourceStrings.GetTypeForInversionTableEntry(key);
			if (type != null)
			{
				attribute = DataBoundResourceStrings.GetAttribute<TranslatableDataFieldAttribute>(type, columnName, out _);
			}
			return attribute;
		}

		public static void Validate(ZPropertyInfo property)
		{
			var formattingErrorMessage = Res.GetString("088bb1b6-b9b9-41e1-9f6e-3b36a7b6009e", "Formatting error");
			try
			{
				string value = (ZString)property.Value;
				if (string.Format(value) != value)
				{
					property.AddError(formattingErrorMessage);
				}
			}
			catch (FormatException)
			{
				property.AddError(formattingErrorMessage);
			}
		}
	}
}
