using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	public class UniversalXsdGenerator : ElementProcessor, IUniversalXsdGenerator
	{
		public UniversalXsdGenerator()
		{
		}
		public UniversalXsdGenerator(string @namespace, string version)
			: base(@namespace, version)
		{
		}

		Type outermostType;
		ElementList<ObjectElementConverter> trailingOutermostDefinitions;
		Dictionary<Type, PropertyInfo> innerDefinedTypes;

		readonly HashSet<Type> typesForString = new HashSet<Type> { typeof(ZString), typeof(ZCodeMappedZString) };
		readonly Dictionary<Type, string> xmlTypeMapping = new Dictionary<Type, string>
		{
			[typeof(ZInt)] = "xs:int",
			[typeof(ZDecimal)] = "xs:decimal",
			[typeof(ZBool)] = "xs:boolean",
			[typeof(ZDateTime)] = "emptiable_dateTime",
			[typeof(ZDateTimeOffset)] = "emptiable_dateTime",
			[typeof(UXmlDateTime)] = "emptiable_dateTime",
			[typeof(ZDate)] = "emptiable_date",
			[typeof(ZLong)] = "xs:long",
			[typeof(ZShort)] = "xs:short",
			[typeof(ZByte)] = "xs:unsignedByte",
			[typeof(ZBlob)] = "xs:base64Binary",
			[typeof(TimeSpan)] = "xs:duration",
			[typeof(SubStreamableStream)] = "xs:base64Binary",
		};

		void Initialize(Type outermostType)
		{
			this.outermostType = outermostType;
			this.trailingOutermostDefinitions = new ElementList<ObjectElementConverter>();
			this.innerDefinedTypes = new Dictionary<Type, PropertyInfo>();
		}

		string IUniversalXsdGenerator.GetCommonSchemaXsdOutput()
		{
			return GetXsdOutput(typeof(Event).Assembly);
		}

		public string GetXsdOutput(Assembly assemblyContainingTypes)
		{
			var result = new XsdBuilder();
			trailingSimpleTypes = new Dictionary<string, List<string>>();

			WriteSchemaOpeningTag(result);

			Initialize(null);
			var commonTypes = new List<Type>();
			foreach (var type in assemblyContainingTypes.GetExportedTypes())
			{
				if (typeof(IDataObject).IsAssignableFrom(type) && !type.IsAbstract && type.IsValidInThisNamespace(Namespace))
				{
					if (type.GetAttribute<XsdSchemaAttribute>()?.Placement != Placement.Inner)
					{
						commonTypes.Add(type);
					}
					else if (type.GetAttribute<FlattenedIntoAttributesAttribute>() != null)
					{
						new FlattenedRelatedObjectElementConverter(type, this).ReflectOutChildConverters();
					}

					AddAttributeDefinitionsFromCollectionTypesUsed(type);
				}
			}

			foreach (var externalType in commonTypes.OrderBy(o => o.Name))
			{
				result.AddBlankLine();

				var elementConverter = ShouldFlattenToAttributes(externalType, out string _) ?
					(ObjectElementConverter)new FlattenedRelatedObjectElementConverter(externalType, this) :
					new OuterObjectElementConverter(externalType, this);

				elementConverter.ReflectOutChildConverters();
				elementConverter.WriteToList(result);
			}

			AddEmptiableDateTimeDefinition(trailingSimpleTypes);

			foreach (var trailingSimpleType in trailingSimpleTypes.OrderBy(o => o.Key))
			{
				result.AddBlankLine();
				foreach (var line in trailingSimpleType.Value)
				{
					result.Add(line);
				}
			}

			WriteSchemaClosingTag(result);
			return result.WriteOutXsd();
		}

		public string GetXsdOutput(Type outermostType)
		{
			Initialize(outermostType);

			outermostType.GetCustomAttribute<XsdSchemaAttribute>();

			var result = new XsdBuilder();

			WriteSchemaOpeningTag(result);

			var elementConverter = new OuterObjectElementConverter(outermostType, this);
			elementConverter.ReflectOutChildConverters();

			WriteIncludedSchemas(result);

			var rootElementInfo = outermostType.GetCustomAttribute<RootElementAttribute>(false);
			if (rootElementInfo != null)
			{
				WriteTopLevelElement(result, rootElementInfo);
			}

			WriteSchemaClosingTag(result);
			return result.WriteOutXsd();
		}

		#region Test Point added to stop having to add an assembly for each Bogus_ class in Unit Testing.
		// You are welcome to refactor if you have a cleverer way.
		// Was done when moving all to Common, is a copy of the way GetXsdOutput used to be.
		// Methods are still hit via one of the Assembly parametered overload which.... <sigh>.
#if DEBUG
		public void TestUnitTestBasedCheckingCodeForType(Type outermostType)
		{
			Initialize(outermostType);
			outermostType.GetCustomAttribute<XsdSchemaAttribute>();
			var result = new XsdBuilder();
			WriteSchemaOpeningTag(result);
			WriteIncludedSchemas(result);
			var elementConverter = new OuterObjectElementConverter(outermostType, this);
			elementConverter.ReflectOutChildConverters();
			WriteIncludedSchemas(result);

			var rootElementInfo = outermostType.GetCustomAttribute<RootElementAttribute>(false);
			if (rootElementInfo != null)
			{
				WriteTopLevelElement(result, rootElementInfo);
			}
			elementConverter.WriteToList(result);
			WriteTrailingDefinitionsToList(result);
			WriteSchemaClosingTag(result);
			result.WriteOutXsd();
		}
#endif
		#endregion

		void WriteSchemaOpeningTag(XsdBuilder result)
		{
			result.Add(@"<xs:schema targetNamespace=""{0}"" version=""{1}"" elementFormDefault=""qualified"" xmlns=""{0}"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">", Namespace, Version);
		}

		static void WriteSchemaClosingTag(XsdBuilder result)
		{
			result.AddBlankLine();
			result.Add(@"</xs:schema>");
		}

		void WriteTopLevelElement(XsdBuilder result, RootElementAttribute rootElementInfo)
		{
			result.Add(@"<xs:element name=""{0}"" type=""{0}Data"" />", rootElementInfo.RootElementName);

			result.AddBlankLine();

			result.Add(@"<xs:complexType name=""{0}Data"">", rootElementInfo.RootElementName);
			result.Add(@"<xs:all>");
			result.Add(@"<xs:element name=""{0}"" type=""{0}""/>", outermostType.Name);
			result.Add(@"</xs:all>");
			result.Add(@"<xs:attribute name=""version"" type=""xs:token"" />");
			result.Add(@"</xs:complexType>");

			result.AddBlankLine();
		}

		void WriteIncludedSchemas(XsdBuilder result)
		{
			result.Add($@"<xs:include schemaLocation=""{UniversalXmlInfo.CommonSchemaName}"" />");
			result.AddBlankLine();
		}

		internal void AddOutermostDefinition(Type typeDefinedExternally, bool shouldBeFlattened = false)
		{
			if (typeDefinedExternally != null
				&& typeDefinedExternally != outermostType
				&& trailingOutermostDefinitions != null
				&& !trailingOutermostDefinitions.Any((existingConverter) => { return typeDefinedExternally == existingConverter.TypeForFields; }))
			{
				var schemaInfo = typeDefinedExternally.GetCustomAttribute<XsdSchemaAttribute>();
				if (schemaInfo?.Placement != Placement.External)
				{
					var outermostDefinition = !shouldBeFlattened
						? ((ObjectElementConverter)new OuterObjectElementConverter(typeDefinedExternally, this))
						: new FlattenedRelatedObjectElementConverter(typeDefinedExternally, this);
					trailingOutermostDefinitions.Add(outermostDefinition);
					outermostDefinition.ReflectOutChildConverters();
				}
			}
		}

		void WriteTrailingDefinitionsToList(XsdBuilder builder)
		{
			trailingOutermostDefinitions.Sort();
			foreach (var outerElementConverter in trailingOutermostDefinitions)
			{
				builder.AddBlankLine();
				outerElementConverter.WriteToList(builder);
			}
		}

		internal XsdSchemaAttribute GetXsdSchemaInfoCheckingForDuplicateInnerTypes(Type propertyType, PropertyInfo propertyInfo)
		{
			var result = propertyType.GetCustomAttribute<XsdSchemaAttribute>();
			if (result.Placement == Placement.Inner)
			{
				if (innerDefinedTypes.TryGetValue(propertyType, out PropertyInfo previousPropertyInfo))
				{
					if (previousPropertyInfo != propertyInfo)
					{
						throw new XmlProcessingException(propertyType, "XsdSchemaAttribute should define this Type as 'Outer', not 'Inner' as it is used multiple times in this Schema.");
					}
				}
				else
				{
					innerDefinedTypes.Add(propertyType, propertyInfo);
				}
			}

			return result;
		}

		internal string GetSimpleXmlType(PropertyInfo propertyInfo)
		{
			var typeOfDataInField = propertyInfo.PropertyType.GetGenericArguments()[0];
			if (typesForString.Contains(typeOfDataInField))
			{
				var maxLength = propertyInfo.GetMaxLength();
				string stringTypeName = "string_maxLength" + maxLength.ToString();

				if (!trailingSimpleTypes.ContainsKey(stringTypeName))
				{
					var simpleTypeDefinition = new List<string>();
					simpleTypeDefinition.Add(string.Format(@"<xs:simpleType name=""{0}"">", stringTypeName));
					simpleTypeDefinition.Add(@"<xs:restriction base=""xs:string"">");
					simpleTypeDefinition.Add(string.Format(@"<xs:maxLength value=""{0}"" />", maxLength));
					simpleTypeDefinition.Add(@"</xs:restriction>");
					simpleTypeDefinition.Add(@"</xs:simpleType>");
					trailingSimpleTypes.Add(stringTypeName, simpleTypeDefinition);
				}

				return stringTypeName;
			}

			if (typeof(Enum).IsAssignableFrom(typeOfDataInField))
			{
				string enumTypeName = typeOfDataInField.Name;

				if (!trailingSimpleTypes.ContainsKey(enumTypeName))
				{
					var simpleTypeDefinition = new List<string>();
					simpleTypeDefinition.Add(string.Format(@"<xs:simpleType name=""{0}"">", enumTypeName));
					simpleTypeDefinition.Add(@"<xs:restriction base=""xs:string"">");

					foreach (var enumerationValueName in Enum.GetNames(typeOfDataInField))
					{
						simpleTypeDefinition.Add(string.Format(@"<xs:enumeration value=""{0}"" />", enumerationValueName));
					}

					simpleTypeDefinition.Add(@"</xs:restriction>");
					simpleTypeDefinition.Add(@"</xs:simpleType>");

					trailingSimpleTypes.Add(enumTypeName, simpleTypeDefinition);
				}

				return enumTypeName;
			}

			return GetXMLType(typeOfDataInField);
		}

		Dictionary<string, List<string>> trailingSimpleTypes = new Dictionary<string, List<string>>();

		internal string GetXMLType(Type typeOfContent)
		{
			if (xmlTypeMapping.TryGetValue(typeOfContent, out string xmlType))
			{
				return xmlType;
			}

			throw new InvalidOperationException("Type [" + typeOfContent.FullName + "] not handled by XSD Schema Generator.");
		}

		static void AddEmptiableDateTimeDefinition(Dictionary<string, List<string>> trailingSimpleTypes)
		{
			var emptiableDateTime = new List<string>();
			emptiableDateTime.Add(@"<xs:simpleType name=""emptiable_dateTime"">");
			emptiableDateTime.Add(@"<xs:union memberTypes=""xs:dateTime empty_string""/>");
			emptiableDateTime.Add(@"</xs:simpleType>");
			trailingSimpleTypes.Add("emptiable_dateTime", emptiableDateTime);

			var emptiableDate = new List<string>();
			emptiableDate.Add(@"<xs:simpleType name=""emptiable_date"">");
			emptiableDate.Add(@"<xs:union memberTypes=""xs:date empty_string""/>");
			emptiableDate.Add(@"</xs:simpleType>");
			trailingSimpleTypes.Add("emptiable_date", emptiableDate);

			var emptyString = new List<string>();
			emptyString.Add(@"<xs:simpleType name=""empty_string"">");
			emptyString.Add(@"<xs:restriction base=""xs:string"">");
			emptyString.Add(@"<xs:length value=""0""/>");
			emptyString.Add(@"</xs:restriction>");
			emptyString.Add(@"</xs:simpleType>");
			trailingSimpleTypes.Add("empty_string", emptyString);
		}

		void AddAttributeDefinitionsFromCollectionTypesUsed(Type type)
		{
			foreach (var propertyInfo in type.GetProperties())
			{
				var propertyType = propertyInfo.PropertyType;
				if (propertyType.IsGenericAndATypeUsedForCollections())
				{
					var attribute = propertyType.GetAttribute<CollectionAttributesAttribute>();
					attribute?.GetAttributeDefinitions(propertyType, GetSimpleXmlType);
				}
			}
		}

		void IUniversalXsdGenerator.SaveXsdFile(string content, string unmappedFileName)
		{
			UniversalXsdGenerator.SaveXsdFile(content, unmappedFileName);
		}
		public static void SaveXsdFile(string content, string unmappedFileName)
		{
			if (string.IsNullOrEmpty(content))
			{
				ErrorReporter.ReportOnce("XSD file should not be empty", $"XSD file {Path.GetFileName(unmappedFileName)} should not be empty");
				throw new XsdCreationException("XSD file should not be empty");
			}

			var versionHeader = string.Format(@"<!-- CW1 Version : {0} Release : {1}-->", new EnterpriseInformationRetriever().VersionNumber, new EnterpriseInformationRetriever().Release);
			content = string.Join("\r\n", versionHeader, content);

			try
			{
				SaveXsdFileCore(content, unmappedFileName);
			}
			catch (ArgumentException exception)
			{
				throw new XsdCreationException(string.Format("Cannot create [{0}] - {1}", unmappedFileName, exception.Message));
			}
			catch (IOException ioException)
			{
				throw new XsdCreationException(string.Format("Cannot create [{0}] - {1}", unmappedFileName, ioException.Message));
			}
			catch (UnauthorizedAccessException)
			{
				throw new XsdCreationException("Cannot write the file to disk. Please check with your system administrator.");
			}
		}

		static void SaveXsdFileCore(string content, string unmappedFileName)
		{
			using (var writer = new StreamWriter(File.Open(unmappedFileName, FileMode.Create, FileAccess.ReadWrite)))
			{
				writer.Write(content);
			}
		}
	}
}

