using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	sealed class CommonSchemaTest : TestCaseWithFactory
	{
		public void TestXsdGenerationDoesntThrowAnyExceptionsAndContainsAllComplexTypes()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var generator = ObjectFactory.Get<IUniversalXsdGenerator>();
				string generatedXsd = null;
				AssertNoExceptionThrown(() => generatedXsd = generator.GetCommonSchemaXsdOutput());
				XElement schema = null;
				AssertNoExceptionThrown("Should be able to use XElement.Parse() on the generated schema.", () => schema = XElement.Parse(generatedXsd));

				var regex = new Regex("complexType name=\"(?<complexType>[^\"]*)\"");
				var complexTypes = regex.Matches(generatedXsd);
				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					var expectedTypes = new StreamReader(resourceRetriever.GetStream("Enterprise.UniversalDataBuss.Testing.DataObjects.TestSchemas.UniversalCommon_ComplexTypes.txt"));
					foreach (Match complexType in complexTypes)
					{
						var typeName = complexType.Groups["complexType"].Value;
						var expectedType = expectedTypes.ReadLine();
						AssertEquals($"Universal Common xsd should contain the complexType {expectedType}", expectedType, typeName);
					}

					var missingTypes = expectedTypes.ReadToEnd();
					AssertEquals($"Universal Common xsd is missing the following complexTypes: {missingTypes}", true, string.IsNullOrEmpty(missingTypes));
				}
			}
		}

		public void TestNoExternalTypesAreUsedInMoreThanOneSchemaAsTheyShouldBeInCommonIfUsedInMoreThanOnce()
		{
			var externalTypeChecker = new SchemaExternalTypeChecker();
			foreach (var type in typeof(UniversalEvent).Assembly.GetExportedTypes())
			{
				externalTypeChecker.Check(type);
			}

			AssertMultilineASCIIEquals("The following Types need to have the [XsdSchema(UniversalXmlInfo.CommonSchemaName)] attribute applied.", "", string.Join("\r\n", externalTypeChecker.Errors.ToArray()));
		}

		class SchemaExternalTypeChecker
		{
			public readonly List<string> Errors = new List<string>();

			public void Check(Type type)
			{
				if (typeof(TopLevelDataObject).IsAssignableFrom(type) && type != typeof(TopLevelDataObject))
				{
					var schemaInfo = type.GetAttribute<XsdSchemaAttribute>();
					currentSchemaName = schemaInfo.SchemaName;

					CheckChildTypes(type);
				}
			}

			string currentSchemaName;

			void CheckChildTypes(Type type)
			{
				foreach (var thing in type.GetProperties())
				{
					var propertyType = thing.PropertyType;
					if (typeof(IDataObject).IsAssignableFrom(propertyType))
					{
						CheckIsNotDefinedInAnotherSchema(propertyType);
					}
					else if (propertyType.IsGenericAndATypeUsedForCollectionsForTesting())
					{
						var typeOfContent = propertyType.GetGenericArguments()[0];

						if (typeof(IDataObject).IsAssignableFrom(typeOfContent))
						{
							CheckIsNotDefinedInAnotherSchema(typeOfContent);
						}
					}
				}
			}

			void CheckIsNotDefinedInAnotherSchema(Type type)
			{
				var schemaInfo = type.GetAttribute<XsdSchemaAttribute>();

				if (schemaInfo != null && schemaInfo.Placement != Placement.External)
				{
					string schemaNameDefining;
					if (history.TryGetValue(type.FullName, out schemaNameDefining))
					{
						if (schemaNameDefining != null && schemaNameDefining != currentSchemaName)
						{
							Errors.Add(string.Format("[{0}] is defined in both [{1}] and [{2}].", type.FullName, schemaNameDefining, currentSchemaName));
							history[type.FullName] = null;
						}
					}
					else
					{
						history[type.FullName] = currentSchemaName;
						CheckChildTypes(type);
					}
				}
			}

			readonly Dictionary<string, string> history = new Dictionary<string, string>();
		}
	}
}
