using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	[TestsSubclassesOf(typeof(IDataObject), ExcludePrivate = true)]
	public abstract class DataObjectTestCase<T> : TestCaseWithFactory where T : IDataObject
	{
		public void TestCollectionTypesUsedDefineTheCollectionAttributesAttributeCorrectlyOrNotAtAll()
		{
			var errors = new List<string>();

			foreach (var propertyInfo in typeof(T).GetProperties())
			{
				var propertyType = propertyInfo.PropertyType;
				if (propertyType.IsGenericAndATypeUsedForCollectionsForTesting())
				{
					var attribute = propertyType.GetAttribute<CollectionAttributesAttribute>();
					if (attribute != null)
					{
						foreach (var attributeName in attribute.GetAttributeNamesForTesting())
						{
							if (propertyType.GetProperty(attributeName) == null)
							{
								errors.Add(string.Format("Property Name: [{0}]  Type: [{1}]  Attribute Missing: [{2}]", propertyInfo.Name, propertyInfo.PropertyType.FullName, attributeName));
							}
						}
					}
				}
			}

			AssertMultilineASCIIEquals("Collection Types with CollectionAttributesAttribute applied must have properly typed fields for each attribute defined by when constructing the CollectionAttributesAttribute.", "", string.Join("\r\n", errors));
		}

		public void TestStringFieldsHaveCorrectMaxLength()
		{
			var expectedMaxLengths = ExpectedMaxLengthValues();
			if (expectedMaxLengths.Count == 0)
			{
				Assert(true);
				return;
			}
			var errors = new List<string>();

			foreach (var propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (propertyInfo.PropertyType.ShouldHaveMaxLengthDefinedForTesting())
				{
					var xmlPropertyName = propertyInfo.Name;
					var attribute = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();
					if (attribute == null)
					{
						errors.Add($"Property Name: [{xmlPropertyName}] is missing the {nameof(MaxLengthAttribute)}");
					}
					else
					{
						var xmlPropertyMaxLength = attribute.MaxLength;
						var expectedMaxLength = expectedMaxLengths.ContainsKey(xmlPropertyName) ? expectedMaxLengths[xmlPropertyName] : -1;
						if (expectedMaxLength != xmlPropertyMaxLength)
						{
							errors.Add($@"Property Name: [{xmlPropertyName}]  Expected Max Length: [{expectedMaxLength}] Actual Max Length of Xml Property: [{xmlPropertyMaxLength}]");
						}
					}
				}
			}

			AssertMultilineASCIIEquals($"ZString and ZCodeMappedZString properties should be the same length as their DB fields. Type: [{typeof(T)}]", "", string.Join("\r\n", errors));
		}

		public int GetDefaultFieldLength()
		{
			return 35;
		}

		public int GetEntityTypeLength()
		{
			return 40;
		}

		protected virtual Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>(0);
		}

		public void TestAllITopLevelDataObjectsHaveNamespaceDependentAttributesOnTheDataSource()
		{
			if (typeof(ITopLevelDataObject).IsAssignableFrom(typeof(T)))
			{
				Type type;
				Assert("Can't get NamespaceDependentAttributes for the 2011/11 namespace.", NamespaceDependentAttribute.TryGetType(typeof(T).GetProperty("DataContext"), UniversalXmlInfo.Namespace_2011_11, out type));
				AssertNotNull("Can get an appropriate Type back for the 2011/11 namespace.", type);

				Assert("Can't get NamespaceDependentAttributes for the 2012/11 namespace.", NamespaceDependentAttribute.TryGetType(typeof(T).GetProperty("DataContext"), UniversalXmlInfo.Namespace_2012_11, out type));
				AssertNotNull("Can get an appropriate NamespaceDependentAttributes for the 2012/11 namespace.", type);
			}
			else
			{
				Assert("Only interested in ITopLevelDataObjects", true);
			}
		}

		public void TestClassesFlattenedIntoAttributesOnlyExposeSimpleTypes()
		{
			var dataObjectType = typeof(T);
			var flattenedAttribute = dataObjectType.GetAttribute<FlattenedIntoAttributesAttribute>();
			if (flattenedAttribute != null)
			{
				var errors = new List<string>();
				foreach (var propertyInfo in dataObjectType.GetProperties())
				{
					if (propertyInfo.DeclaringType == dataObjectType)
					{
						var propertyType = propertyInfo.PropertyType;

						if (propertyType.IsGenericAndATypeUsedForCollectionsForTesting())
						{
							var typeOfContent = propertyType.GetGenericArguments()[0];

							if (typeof(IDataObject).IsAssignableFrom(typeOfContent))
							{
								errors.Add("List<" + typeOfContent.Name + "> " + propertyInfo.Name + " - Cannot have List<IDataObject> typed properties.");
							}
						}
						else
						{
							if (typeof(IDataObject).IsAssignableFrom(propertyType))
							{
								errors.Add(propertyType.Name + " " + propertyInfo.Name + " - Cannot have IDataObject typed properties.");
							}
						}
					}
				}

				AssertMultilineASCIIEquals("XML Attributes can only contain simple types, so IDataObjects with FlattenedIntoAttributes applied can only have simple types on them."
					, ""
					, string.Join("\r\n", errors.ToArray()));
			}
			else
			{
				Assert("Only interested in IDataObjects with the FlattenedIntoAttributesAttribute applied.", true);
			}
		}

		public void TestICodeDataObjectImplementationsHaveFlattenedIntoAttributesAttributeApplied()
		{
			var flattenedAttribute = typeof(T).GetAttribute<FlattenedIntoAttributesAttribute>();
			AssertEquals(@"ShouldBeFlattenedIntoAttributes:-
Data should be flattened into attributes for things like ICodeDataObjects where you have a small number of simple typed 
properties only and you are 100% sure you are never going to add any child complex types or collections. Representing
Types using Attributes gives a more compact view but doesn't allow expansion using child complex types and is harder 
to read when you have a large number of properties represented on the one element as attributes.

Right now we only look for subclasses of ICodeDataObject, but you may want to consider this for any new types you add.
", ShouldBeFlattenedIntoAttributes, flattenedAttribute != null);
		}

		protected virtual bool ShouldBeFlattenedIntoAttributes
		{
			get { return typeof(ICodeDataObject).IsAssignableFrom(typeof(T)); }
		}

		public void TestAllITopLevelDataObjectsHaveRootElementAttributeAndAreExternal()
		{
			if (typeof(ITopLevelDataObject).IsAssignableFrom(typeof(T)))
			{
				var rootElementAttribute = typeof(T).GetAttribute<RootElementAttribute>();
				AssertNotNull(typeof(T).Name + " must have RootElementAttribute applied as it is an ITopLevelDataObject.", rootElementAttribute);

				var schemaAttribute = typeof(T).GetAttribute<XsdSchemaAttribute>();
				CombineAssertions(delegate
				{
					AssertEquals("schemaAttribute.Placement", Placement.External, schemaAttribute.Placement);
					AssertEquals("schemaAttribute.SchemaName", rootElementAttribute.RootElementName + ".xsd", schemaAttribute.SchemaName);
				});
			}
			else
			{
				Assert("Only interested in ITopLevelDataObjects", true);
			}
		}

		public void TestXsdGenerationDoesntThrowAnyExceptions_2011_11() => TestXsdGenerationDoesntThrowAnyExceptions_2011_11Core();

		protected virtual void TestXsdGenerationDoesntThrowAnyExceptions_2011_11Core()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var generator = ObjectFactory.Get<IUniversalXsdGenerator>();
				string generatedXsd = null;
				AssertNoExceptionThrown(delegate { generatedXsd = generator.GetXsdOutput(typeof(T)); });
				XElement schema = null;
				AssertNoExceptionThrown("Should be able to use XElement.Parse() on the generated schema.", delegate { schema = XElement.Parse(generatedXsd); });

				if (typeof(TopLevelDataObject).IsAssignableFrom(typeof(T)))
				{
					var schemaAttribute = typeof(T).GetAttribute<XsdSchemaAttribute>();
					var expectedXML = GetManifestResourceStream(schemaAttribute.SchemaName);
					AssertMultilineASCIIEquals(schemaAttribute.SchemaName, expectedXML, generatedXsd);
				}
			}

			string GetManifestResourceStream(string fileName)
			{
				using (var stream = GetType().Assembly.GetManifestResourceStream($"Enterprise.UniversalDataBuss.Testing.DataObjects.TestSchemas.{fileName}"))
				{
					return new StreamReader(stream).ReadToEnd();
				}
			}
		}

		public void TestAllEnumsThatAreCandidateKeysAreAlsoMandatory()
		{
			var type = typeof(T);

			var failures = new List<string>();
			foreach (var propertyInfo in type.GetProperties())
			{
				var genericArguments = propertyInfo.PropertyType.GetGenericArguments();
				if (genericArguments.Length > 0
					&& genericArguments[0].IsEnum
					&& propertyInfo.GetCustomAttributes(typeof(CandidateKeyAttribute), false).Length > 0
					&& propertyInfo.GetCustomAttributes(typeof(MandatoryAttribute), false).Length == 0)
				{
					failures.Add(propertyInfo.Name);
				}
			}

			Assert("Enum that is a candidate key must also have the mandatory attribute. Please fix: " + string.Join(", ", failures.ToArray()), failures.Count == 0);
		}

		public void TestAllowLineControlWhiteSpaceAttributeIsApplied()
		{
			var allowLineControlWhiteSpaceAttributeProperties = new List<string>();
			foreach (var propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var attribute = propertyInfo.GetCustomAttribute<AllowLineControlWhiteSpaceAttribute>();
				if (attribute != null)
				{
					allowLineControlWhiteSpaceAttributeProperties.Add(propertyInfo.Name);
				}
			}

			AssertContainsExactElementsInAnyOrder("The properties that 'AllowLineControlWhiteSpaceAttribute' is applied.", ExpectedAllowLineControlWhiteSpaceAttributeProperties(), allowLineControlWhiteSpaceAttributeProperties);
		}

		public void TestTrimWhiteSpacePropertyIsApplied()
		{
			var trimWhiteSpaceProperties = new List<string>();
			foreach (var propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var attribute = propertyInfo.GetCustomAttribute<TrimWhiteSpaceAttribute>();
				if (attribute != null)
				{
					trimWhiteSpaceProperties.Add(propertyInfo.Name);
				}
			}

			AssertContainsExactElementsInAnyOrder("The properties that 'TrimWhiteSpaceAttribute' is applied.", ExpectedTrimWhiteSpaceProperties(), trimWhiteSpaceProperties);
		}

		public List<String> ExpectedAllowLineControlWhiteSpaceAttributeProperties() => ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore();

		protected virtual List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore()
		{
			return new List<string>();
		}

		public List<string> ExpectedTrimWhiteSpaceProperties() => ExpectedTrimWhiteSpacePropertiesCore();
		protected virtual List<string> ExpectedTrimWhiteSpacePropertiesCore()
		{
			return new List<string>();
		}
	}
}
