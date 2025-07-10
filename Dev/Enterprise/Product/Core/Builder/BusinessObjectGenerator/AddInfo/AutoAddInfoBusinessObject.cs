using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.IO;

namespace Enterprise.BusinessObjectGenerator
{
	public class AutoAddInfoBusinessObject : AutoBusinessObject
	{
		public AutoAddInfoBusinessObject(BusinessObjectInfo info) : base(info)
		{
		}

		protected override string BodyOfClass
		{
			get
			{
				return LinesOfCode(
					SchemaOLD,
					CodeForConstructor,
					CodeForDataTable,
					CodeForProperties,
					CodeForValidation,
					CodeForAddInfo,
					CodeForConcurrency,
					CodeForAddInfoChildProperties,
					CodeForOldPrefix
					);
			}
		}

		bool IsCustoms
		{
			get
			{
				if (!isCustoms.HasValue)
				{
					isCustoms = Info.Namespace.Contains("Customs");
				}
				return isCustoms.Value;
			}
		}
		bool? isCustoms;

		bool HasParentView => hasParentView ??= !string.IsNullOrEmpty(Info.ParentView);
		bool? hasParentView;

		bool HasParentSchema => hasParentSchema ??= !string.IsNullOrEmpty(Info.ParentSchema);
		bool? hasParentSchema;

		bool HasOldPrefix => hasOldPrefix ??= !string.IsNullOrEmpty(Info.OldPrefix);
		bool? hasOldPrefix;

		protected override string CodeInConstructor
		{
			get
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append(base.CodeInConstructor ?? string.Empty);
				if (IsCustoms)
				{
					if (HasParentSchema || !HasParentView)
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.AppendLine();
						}
						if (!HasParentSchema)
						{
							stringBuilder.Append($@"			addInfo = new Enterprise.Customs.Business.AddInfoWrapper<{Info.ClassNames.BusinessObjectAuto}>(this");
						}
						else
						{
							stringBuilder.Append($@"			addInfo = new Enterprise.Customs.Business.AddInfoWrapperWithBaseAddInfoParent<{Info.ClassNames.BusinessObjectAuto}>(this, base.AddInfo");
						}
						if (HasAddInfoColumn)
						{
							stringBuilder.Append($@"
				, addInfoPropertyName: Schema.{Prefix}_AddInfo, getAddInfoNamesMapping: () => AddInfoNamesMapping");
						}
						if (HasNAddInfoColumn)
						{
							stringBuilder.Append($@"
				, nAddInfoPropertyName: Schema.{Prefix}_NAddInfo, getNAddInfoNamesMapping: () => NAddInfoNamesMapping");
						}
						stringBuilder.Append(");");
					}
				}
				else
				{
					if (HasAddInfoColumn)
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.AppendLine();
						}
						stringBuilder.Append($"\t\t\t{Prefix}_AddInfoInfo.ValueChanged += AddInfoInfo_ValueChanged;");
					}
					if (HasNAddInfoColumn)
					{
						if (stringBuilder.Length > 0)
						{
							stringBuilder.AppendLine();
						}
						stringBuilder.Append($"\t\t\t{Prefix}_NAddInfoInfo.ValueChanged += NAddInfoInfo_ValueChanged;");
					}
				}
				return stringBuilder.ToString();
			}
		}

		protected override string MiscAttributes
		{
			get
			{
				var result = "";
				if (IsCustoms && HasOldPrefix)
				{
					result = ", PropertyDescriptorCollection(typeof(Customs.Business.AddInfoPropertyDescriptorCollection))";
				}
				return result;
			}
		}

		protected override string ImplementsInterfaces
		{
			get
			{
				var result = base.ImplementsInterfaces;
				if (IsCustoms)
				{
					result += ", Customs.Business.IAddInfoManagerWithSchema";
				}
				result += ", IOriginalValueProvider, IConcurrencyExceptionDecorator, IAddInfoSchemaProvider";
				return result;
			}
		}

		#region Code for Using Clause

		protected override string UsingClause
		{
			get
			{
				return LinesOfCode(
					"using System.Collections.Generic;",
					"using System.Data;",
					"using System.Linq;",
					"using System.Text;",
					"using CargoWise.Common;",
					"using CargoWise.ComponentModel;",
					UsingCargoWiseEntityFramework,
					"using CargoWise.EntityFramework.Extensions;",
					"using CargoWise.Schema;",
					"using CargoWise.Types;",
					UsingMasterFiles,
					"using Enterprise.ZArchitecture.Schema;"
					);
			}
		}

		#endregion

		#region Code for Schema

		public override string SchemaOLD
		{
			get { return new AutoAddInfoSchema_OLD(Info).ToString(); }
		}

		#endregion

		HashSet<string> UnicodeProperties
		{
			get
			{
				if (unicodeProperties == null)
				{
					unicodeProperties = new HashSet<string>();
					var assembly = typeof(CargoWise.DbUpgrader.Scripts.Definitions.CoreScriptIndex).Assembly;
					var resourceRetriever = new EmbeddedResourceRetriever(assembly);
					var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(n => n.Contains($"{Info.TableName}.model.xml"));

					if (resourceName != null)
					{
						var str = resourceRetriever.GetString(resourceName, System.Text.Encoding.UTF8);
						unicodeProperties = GetAddInfos(str).Where(addInfo => addInfo.IsUnicode).Select(addInfo => addInfo.Name).ToHashSet();
					}
				}
				return unicodeProperties;
			}
		}
		HashSet<string> unicodeProperties;

		IEnumerable<(string Name, string DataType, string Precision, string Scale, int MaxLength, bool IsUnicode, bool Indexed)> GetAddInfos(string content)
		{
			using var stringReader = new StringReader(content);
			using var xmlTextReader = new XmlTextReader(stringReader)
			{
				Namespaces = false,
			};
			var xmlDocument = new XmlDocument();
			xmlDocument.Load(xmlTextReader);

			return xmlDocument.SelectNodes("View/AddInfos/AddInfo")
				.OfType<XmlNode>()
				.Select(node =>
				{
					int.TryParse(node.Attributes.GetNamedItem("MaxLength")?.Value, out var maxLength);
					return (
						node.Attributes.GetNamedItem("Name").Value,
						node.Attributes.GetNamedItem("DataType")?.Value,
						node.Attributes.GetNamedItem("Precision")?.Value,
						node.Attributes.GetNamedItem("Scale")?.Value,
						maxLength,
						"true".Equals(node.Attributes.GetNamedItem("IsUnicode")?.Value),
						"true".Equals(node.Attributes.GetNamedItem("Indexed")?.Value));
				});
		}

		#region Code for Properties

		protected override string CodeForProperties
		{
			get
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append(BaseOverrides);
				stringBuilder.Append(Mappings);
				stringBuilder.Append(Properties);
				return stringBuilder.ToString();
			}
		}

		#endregion

		#region Properties

		string Properties
		{
			get
			{
				var stringBuilder = new StringBuilder();
				stringBuilder.Append(@"		#region Properties
");
				foreach (var property in ApplicableProperties)
				{
					var fieldName = property.ColumnName.Substring(property.ColumnName.IndexOf("_") + 1);
					var fieldNameCamelCase = fieldName[0].ToString().ToLower() + fieldName.Substring(1);
					var zType = GetZType(property);

					var autoPropertyFK  = property as AutoPropertyFK;

					if (autoPropertyFK is not null)
					{
						stringBuilder.Append(
							LinesOfCode(
								"",
								autoPropertyFK.CodeForRelatedBusinessObjectAttribute,
								autoPropertyFK.CodeForListAttribute
						));
					}

					stringBuilder.Append($@"
		public virtual {zType} {property.ColumnName}
		{{
			get => {property.ColumnName}Data.Value;
			set
			{{
				SetNonPersistentPropertyValue({property.ColumnName}Info, ref {property.ColumnName}Data.Value, value);
				if (!IsValidationSuspended)
				{{
					Validation.Validate{property.ColumnName}();
				}}
			}}
		}}
		public ZPropertyInfo {property.ColumnName}Info => GetZPropertyInfo(Schema.{property.ColumnName});
		AddInfoPropertyData<{zType}> {property.ColumnName}Data => {fieldNameCamelCase}Data ?? ({fieldNameCamelCase}Data = new AddInfoPropertyData<{zType}>(Schema.{property.ColumnName}));
		AddInfoPropertyData<{zType}> {fieldNameCamelCase}Data;
");
					if (autoPropertyFK is not null)
					{
						stringBuilder.AppendLine(autoPropertyFK.CodeForRelatedBusinessObject);
					}
				}
				stringBuilder.Append(@"
		#endregion
");
				if (!HasParentView || HasParentSchema)
				{
					stringBuilder.Append(@"
		#region Original Value

		public IZType GetOriginalValue(ZPropertyInfo info)
		{
			return AddInfoPropertyNamesMapping.TryGetValue(info.Name.Substring(info.BizObj.TablePrefix.Length + 1), out var addInfoPropertyData)
				? addInfoPropertyData.OriginalValue
				: info.Value;
		}

		#endregion
");
			}

				return stringBuilder.ToString();
			}
		}

		string GetZType(AutoProperty property)
		{
			return property.DataType.Name switch
			{
				nameof(String) => "ZString",
				nameof(Decimal) => "ZDecimal",
				nameof(Boolean) => "ZBool",
				nameof(Guid) => "ZGuid",
				nameof(Int16) => "ZShort",
				nameof(Int32) => "ZInt",
				nameof(Int64) => "ZLong",
				nameof(DateTime) => property.SqlDbType == SqlDbType.Date ? "ZDate" : "ZDateTime",
				_ => throw new ArgumentException($"Name {property.DataType.Name} is not the name of a recognized data type", nameof(property)),
			};
		}

		#endregion

		#region Base Overrides

		string BaseOverrides
		{
			get
			{
				var stringBuilder = new StringBuilder();
				if (!HasParentView || HasParentSchema)
				{
					stringBuilder.Append(@"		#region Base Behaviour Overrides

		public override void OnLoaded()
		{
			base.OnLoaded();");
					if (HasAddInfoColumn)
					{
						stringBuilder.Append($@"
			AddInfoParser.Deserialise({Prefix}_AddInfo, AddInfoNamesMapping, true);");
					}

					if (HasNAddInfoColumn)
					{
						stringBuilder.Append($@"
			AddInfoParser.Deserialise({Prefix}_NAddInfo, NAddInfoNamesMapping, true);");
					}

					stringBuilder.Append($@"
		}}

		protected override void OnFactorySaving()
		{{
			if (HasChangesNotIncludingChildren{(HasParentSchema ? " || base.AddInfo.HasChanges" : "")})
			{{
				using (GetValidationSuspender())
				{{
");
					if (IsCustoms)
					{
						stringBuilder.Append(@"					addInfo.UpdateRelatedPropertyInfo();
");
					}
					else
					{
						var syncAddInfos = this is IAddInfoWithSyncPropertySupporter supporter ? supporter.GetSyncAddInfos() : null;
						if (HasAddInfoColumn)
						{
							stringBuilder.Append($@"					var syncAddInfos = this is IAddInfoWithSyncPropertySupporter supporter ? supporter.GetSyncAddInfos() : null;
					if (!settingAddInfoProperty)
					{{
						try
						{{
							settingAddInfoProperty = true;
							{Prefix}_AddInfo = AddInfoParser.Serialise(AddInfoNamesMapping,
								syncAddInfos?.Where(syncAddInfo => !syncAddInfo.info.IsNAddInfoField())
									.Select(syncAddInfo => new KeyValuePair<ZString, ZPropertyInfo>(syncAddInfo.addInfoName, syncAddInfo.info))
								?? Enumerable.Empty<KeyValuePair<ZString, ZPropertyInfo>>());
						}}
						finally
						{{
							settingAddInfoProperty = false;
						}}
					}}
");
						}

						if (HasNAddInfoColumn)
						{
							stringBuilder.Append($@"					if (!settingNAddInfoProperty)
					{{
						try
						{{
							{Prefix}_NAddInfo = AddInfoParser.Serialise(NAddInfoNamesMapping,
								syncAddInfos?.Where(syncAddInfo => syncAddInfo.info.IsNAddInfoField())
									.Select(syncAddInfo => new KeyValuePair<ZString, ZPropertyInfo>(syncAddInfo.addInfoName, syncAddInfo.info))
								?? Enumerable.Empty<KeyValuePair<ZString, ZPropertyInfo>>());
						}}
						finally
						{{
							settingNAddInfoProperty = false;
						}}
					}}
");
						}
					}

					stringBuilder.Append(@"				}
			}
			base.OnFactorySaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			if (saveSucceeded)
			{");

					if (HasAddInfoColumn)
					{
						stringBuilder.Append($@"
				AddInfoNamesMapping.Values.ForEach(x => x.OriginalValue = x.Value);");
					}

					if (HasNAddInfoColumn)
					{
						stringBuilder.Append($@"
				NAddInfoNamesMapping.Values.ForEach(x => x.OriginalValue = x.Value);");
					}

					stringBuilder.Append($@"
			}}
			base.OnSaved(saveSucceeded);
		}}

		protected override void CopyAdditionalPersistentValuesFrom(BusinessObject sourceObject, BusinessObjectCloneArgs args)
		{{
			base.CopyAdditionalPersistentValuesFrom(sourceObject, args);
			if (sourceObject is {Info.ClassNames.BusinessObjectAuto} source)
			{{
				AddInfoParser.CopyAddInfoPropertyData(this, args{(HasAddInfoColumn ? ", (AddInfoNamesMapping, source.AddInfoNamesMapping)" : "")}{(HasNAddInfoColumn ? ", (NAddInfoNamesMapping, source.NAddInfoNamesMapping)" : "")});
			}}
		}}

		#endregion

");
				}
				return stringBuilder.ToString();
			}
		}

		#endregion

		#region Mappings

		string Mappings
		{
			get
			{
				var stringBuilder = new StringBuilder();

				stringBuilder.Append(@"		#region Mappings");

				if (HasAddInfoColumn)
				{
					AppendMappings(stringBuilder, "AddInfoNamesMapping", "addInfoNamesMapping", ApplicableProperties.Where(p => !UnicodeProperties.Contains(p.ColumnName)));
				}

				if (HasNAddInfoColumn)
				{
					AppendMappings(stringBuilder, "NAddInfoNamesMapping", "nAddInfoNamesMapping", ApplicableProperties.Where(p => UnicodeProperties.Contains(p.ColumnName)));
				}

				if (!HasParentView || HasParentSchema)
				{
					stringBuilder.Append(@"
		IDictionary<string, IAddInfoPropertyData> AddInfoPropertyNamesMapping
		{
			get
			{
				if (addInfoPropertyNamesMapping == null)
				{
					addInfoPropertyNamesMapping = new Dictionary<string, IAddInfoPropertyData>();
					AddInfoColumnMappings.Values.ForEach(x => x.ForEach(y => addInfoPropertyNamesMapping.Add(y.Key, y.Value)));
				}
				return addInfoPropertyNamesMapping;
			}
		}
		IDictionary<string, IAddInfoPropertyData> addInfoPropertyNamesMapping;

		IDictionary<string, IDictionary<string, IAddInfoPropertyData>> AddInfoColumnMappings
		{
			get
			{
				if (addInfoColumnMappings == null)
				{
					addInfoColumnMappings = new Dictionary<string, IDictionary<string, IAddInfoPropertyData>>();");

					if (HasAddInfoColumn)
					{
						stringBuilder.Append($@"
					addInfoColumnMappings.Add(Schema.{Prefix}_AddInfo, AddInfoNamesMapping);");
					}

					if (HasNAddInfoColumn)
					{
						stringBuilder.Append($@"
					addInfoColumnMappings.Add(Schema.{Prefix}_NAddInfo, NAddInfoNamesMapping);");
					}

					stringBuilder.Append(@"
				}
				return addInfoColumnMappings;
			}
		}
		IDictionary<string, IDictionary<string, IAddInfoPropertyData>> addInfoColumnMappings;
");
				}
				if (HasAddInfoColumn)
				{
					if (!IsCustoms)
					{
						stringBuilder.Append($@"
		void AddInfoInfo_ValueChanged(object sender, System.EventArgs e)
		{{
			if (!settingAddInfoProperty)
			{{
				AddInfoParser.Deserialise({Prefix}_AddInfo, AddInfoNamesMapping, true);
			}}
		}}
		bool settingAddInfoProperty;
");
					}
					if (!HasParentView || HasParentSchema)
					{
						stringBuilder.Append($@"
		public string GetAddInfoString() => AddInfoParser.Serialise(AddInfoNamesMapping);
");
					}
				}
				if (HasNAddInfoColumn)
				{
					if (!IsCustoms)
					{
						stringBuilder.Append($@"
		void NAddInfoInfo_ValueChanged(object sender, System.EventArgs e)
		{{
			if (!settingNAddInfoProperty)
			{{
				AddInfoParser.Deserialise({Prefix}_NAddInfo, NAddInfoNamesMapping, true);
			}}
		}}
		bool settingNAddInfoProperty;
");
					}
					if (!HasParentView || HasParentSchema)
					{
						stringBuilder.Append($@"
		public string GetNAddInfoString() => AddInfoParser.Serialise(NAddInfoNamesMapping);
");
					}
				}
				stringBuilder.Append(@"
		#endregion

");

				return stringBuilder.ToString();
			}
		}

		void AppendMappings(StringBuilder stringBuilder, string methodName, string dictionaryName, IEnumerable<AutoProperty> properties)
		{
			var hasParent = HasParentView && !HasParentSchema;
			if (!hasParent)
			{
				stringBuilder.Append($@"
		IDictionary<string, IAddInfoPropertyData> {methodName}
		{{
			get
			{{
				return {dictionaryName} ?? ({dictionaryName} = Get{methodName}());
			}}
		}}
		IDictionary<string, IAddInfoPropertyData> {dictionaryName};
");
			}
			stringBuilder.Append($@"
		protected {(hasParent ? "override" : "virtual")} IDictionary<string, IAddInfoPropertyData> Get{methodName}() => new Dictionary<string, IAddInfoPropertyData>
		{{");
			foreach (var property in properties)
			{
				var fieldName = property.ColumnName.Substring(property.ColumnName.IndexOf("_") + 1);

				stringBuilder.Append($@"
			{{""{fieldName}"", {property.ColumnName}Data}},");
			}

			stringBuilder.Append($@"
		}}{(hasParent ? ".Union(base.GetAddInfoNamesMapping()).ToDictionary(x => x.Key, x => x.Value)" : "")};
");
		}

		#endregion

		#region Code for Lookups Property

		protected override string CodeForLookupsProperty => "";

		#endregion

		#region Code for Set Default Values

		protected override string CodeForSetDefaultValues => "";

		#endregion

		#region Code for Validation

		protected override string CodeForValidation
		{
			get
			{
				return $@"		#region Validation

		public new Auto{Info.ClassNames.Validation} Validation => (Auto{Info.ClassNames.Validation})base.Validation;

		#endregion
";
			}
		}

		#endregion

		#region Code for AddInfo

		string CodeForAddInfo
		{
			get
			{
				if (IsCustoms)
				{
					var stringBuilder = new StringBuilder();

					if (HasParentView)
					{
						var parentSchema = HasParentSchema ? Info.ParentSchema : $"{Info.ParentView}Schema";
						var underlyingTable = Info.UnderlyingTableName.Substring(Info.UnderlyingTableName.LastIndexOf(".") + 1);
						stringBuilder.AppendLine($@"		#region IAddInfoManagerWithSchema Members");
						if (HasParentSchema)
						{
							stringBuilder.AppendLine($@"
		Enterprise.Customs.Business.IAddInfo Enterprise.Customs.Business.IAddInfoManager.AddInfo => addInfo;
		readonly Enterprise.Customs.Business.IAddInfo addInfo;");
						}

						stringBuilder.AppendLine($@"
		ITableSchema Enterprise.Customs.Business.IAddInfoManagerWithSchema.AddInfoSchema => addInfoSchema ?? (addInfoSchema = new Customs.Business.CombinedAddInfoSchema({underlyingTable}Schema.Instance, new ITableSchema[] {{ {Info.TableName}Schema.Instance, {parentSchema}.Instance }}));
		ITableSchema addInfoSchema;

		#endregion");
					}
					else
					{
						stringBuilder.AppendLine($@"		#region IAddInfoManagerWithSchema Members

		Enterprise.Customs.Business.IAddInfo Enterprise.Customs.Business.IAddInfoManager.AddInfo => addInfo;
		readonly Enterprise.Customs.Business.IAddInfo addInfo;

		ITableSchema Enterprise.Customs.Business.IAddInfoManagerWithSchema.AddInfoSchema => {Info.TableName}Schema.Instance;

		#endregion");
					}

					return stringBuilder.ToString();
				}

				return null;
			}
		}

		#endregion

		#region Code for Concurrency

		string CodeForConcurrency
		{
			get
			{
				var stringBuilder = new StringBuilder();

				if (!HasParentView || HasParentSchema)
				{
					stringBuilder.Append(@"		#region Concurrency Handling

		protected override void OnConcurrencyExceptionCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			base.OnConcurrencyExceptionCore(propertyRecords);
			AddInfoParser.HandleConcurrencyException(propertyRecords, GetZPropertyInfo, AddInfoColumnMappings);
		}

		protected override void OnConcurrencyExceptionAfterMergeCore(IEnumerable<IPropertyRecord> propertyRecords)
		{
			AddInfoColumnMappings.ForEach(x => AddInfoParser.Deserialise((ZString)this[x.Key], x.Value, true));
			base.OnConcurrencyExceptionAfterMergeCore(propertyRecords);
		}

		public void AppendDecoratedDisplayName(StringBuilder stringBuilder, IPropertyRecord record)
		{
			AddInfoParser.AppendDecoratedDisplayName(stringBuilder, record, GetZPropertyInfo, AddInfoColumnMappings);
		}

		#endregion");
				}

				return stringBuilder.ToString();
			}
		}

		#endregion

		#region Code for AddInfoChild Properties

		string CodeForAddInfoChildProperties
		{
			get
			{
				if (Info.ChildTableProperties is null || Info.ChildTableProperties.Length == 0)
				{
					return null;
				}

				var stringBuilder = new StringBuilder();
				stringBuilder.Append(@"
		#region AddInfoChild Properties
");
				foreach (var property in Info.ChildTableProperties)
				{
					stringBuilder.Append($@"
		#region {property.ColumnName}

		public virtual {GetZType(property)} {property.ColumnName}
		{{
			get {{ return AddInfoChild.{property.ColumnName}; }}
			set {{ AddInfoChild.{property.ColumnName} = value; }}
		}}

		public virtual ZPropertyInfo {property.ColumnName}Info
		{{
			get {{ return GetWrappedZPropertyInfo(Schema.{property.ColumnName}, x => AddInfoChild.{property.ColumnName}Info); }}
		}}

		#endregion
");
				}

				stringBuilder.Append($@"
		#endregion

		#region AddInfoChild object/Validation and Lookups objects

		public {Info.ChildTableName}Lookups AddInfoChildLookups
		{{
			get {{ return AddInfoChild.Lookups; }}
		}}

		public {Info.ChildTableName}Validation AddInfoChildValidation
		{{
			get {{ return AddInfoChild.Validation; }}
		}}

		public {Info.ChildTableName} AddInfoChild => this.LoadOrCreateAddInfoChild(ref addInfoChild);
		{Info.ChildTableName} addInfoChild;

		public static System.Type AddInfoChildType => typeof({Info.ChildTableName});

		public static ITableSchema AddInfoChildSchema => {Info.ChildTableName}Schema.Instance;

		#endregion
");

				return stringBuilder.ToString();
			}
		}

		#endregion

		#region CodeForOldPrefix

		string CodeForOldPrefix
		{
			get
			{
				var stringBuilder = new StringBuilder();
				if (HasOldPrefix || (HasParentView && !HasParentSchema))
				{
					stringBuilder.Append(@"
		#region Wrapped Properties with Old Prefix
");
					if (HasOldPrefix)
					{
						foreach (var property in ApplicableProperties)
						{
							var fieldName = property.ColumnName.Substring(property.ColumnName.IndexOf("_") + 1);
							var fieldNameWithPrefix = $"{Info.OldPrefix}_{fieldName}";
							var zType = GetZType(property);
							stringBuilder.Append($@"
		public virtual {zType} {fieldNameWithPrefix}
		{{
			get => {property.ColumnName};
			set => {property.ColumnName} = value;
		}}
		public ZPropertyInfo {fieldNameWithPrefix}Info => GetWrappedZPropertyInfo(nameof({fieldNameWithPrefix}), x => {property.ColumnName}Info);
");
						}
					}

					var useNew = HasParentView ? " new" : "";

					stringBuilder.Append($@"
		public{useNew} Auto{Info.ClassNames.Validation} AddInfoValidation => Validation;
");

					if (Info.UnderlyingTableName.ToLower().IndexOf("dummy") == -1)
					{
							stringBuilder.Append($@"
		public{useNew} {Info.UnderlyingTableName}Lookups AddInfoLookups => ({Info.UnderlyingTableName}Lookups)Lookups;
");
					}

					if (IsCustoms)
					{
						if (HasParentView)
						{
							var parentSchema = HasParentSchema ? Info.ParentSchema : $"{Info.ParentView}Schema";
							var underlyingTable = Info.UnderlyingTableName.Substring(Info.UnderlyingTableName.LastIndexOf(".") + 1);
							stringBuilder.Append($@"
		public{useNew} static ITableSchema AddInfoSchema => new Customs.Business.CombinedAddInfoSchema({underlyingTable}Schema.Instance, new ITableSchema[] {{ {Info.TableName}Schema.Instance, {parentSchema}.Instance }});
");
						}
						else
						{
							stringBuilder.Append($@"
		public static ITableSchema AddInfoSchema => {Info.TableName}Schema.Instance;
");
						}

						if (HasOldPrefix)
						{
							stringBuilder.Append($@"
		public{(HasParentSchema ? "" : useNew)} static string OldTablePrefix => ""{Info.OldPrefix}"";
");
						}
					}

					stringBuilder.Append($@"
		#endregion");
				}
				return stringBuilder.Length == 0 ? null : stringBuilder.ToString();
			}
		}

		#endregion

		bool HasAddInfoColumn => UnicodeProperties.Count < ApplicableProperties.Length;
		bool HasNAddInfoColumn => UnicodeProperties.Any();

		AutoProperty[] ApplicableProperties => applicableProperties ?? (applicableProperties = AutoProperties.Properties.Where(property => !property.ColumnName.Contains(CargoWise.Schema.Schema.ClusterKeyColumnSuffix)).ToArray());
		AutoProperty[] applicableProperties;

		new AddInfoBusinessObjectInfo Info => (AddInfoBusinessObjectInfo)base.Info;
	}
}
