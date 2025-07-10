using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.InversionOfControl;
using CargoWise.Common;
using CargoWise.Database.Abstractions;
using CargoWise.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class EnterpriseApplicationConfigurationTests : TestCase
	{
		public void TestExternalRequestSupportedAddressTypesProviders()
		{
			var externalRequestTypeProviders = ObjectFactory.Get("ExternalRequestSupportedAddressTypesProviders") as Hashtable;
			AssertNotNull("ExternalRequestSupportedAddressTypesProviders is defined in config", externalRequestTypeProviders);
		}

		public void TestIApplicationSchemaResolver()
		{
			var resolver1 = ObjectFactory.Get<IApplicationSchemaResolver>();
			AssertNotNull("(pre-condition) IApplicationSchemaResolver is defined in config", resolver1);

			var resolver2 = ObjectFactory.Get<IApplicationSchemaResolver>();
			AssertSame("IApplicationSchemaResolver is a singleton", resolver1, resolver2);
		}

		public void TestConnectsGlobalServiceProviderToObjectFactory()
		{
			// By the time this test gets run, ConfigureObjectFactory will have been called.

			var resolver1 = ObjectFactory.Get<IApplicationSchemaResolver>();
			AssertNotNull("(pre-condition) IApplicationSchemaResolver is defined in config", resolver1);

			var resolver2 = GlobalServiceProvider.Instance.GetService(typeof(IApplicationSchemaResolver));
			AssertSame("IApplicationSchemaResolver is a singleton, regardless of how you get to it.", resolver1, resolver2);
		}

		public void TestNoDuplicatesInEnterpriseApplicationConfigurationXML()
		{
			var binPath = AssemblyLoader.GetBinPath();
			var dictionary = new Dictionary<string, List<(string configurationLocation, ObjectDefinition definition)>>();
			var duplicates = new Dictionary<string, List<(string configurationLocation, ObjectDefinition definition)>>();
			var dictionaryValuesSubSetDuplicates = new Dictionary<string, List<(string configurationLocation, ObjectDefinition definition)>>();
			var listValuesSubSetDuplicates = new Dictionary<string, List<(string configurationLocation, ObjectDefinition definition)>>();
			foreach ((string configurationResourceFileUriOrconfigurationFile, ObjectDefinitions objectDefinitions) in EnterpriseApplicationConfiguration.ConfigurationLocations.Select(x =>
				{
					return x.IsResourceUri ?
					(x.ResourceUriOrResourceName, ObjectDefinitions.Create(x.ResourceUriOrResourceName)) :
					(x.ResourceUriOrResourceName, ObjectDefinitions.CreateUsingFile(Path.Combine(binPath, x.AssemblyFileName), x.ResourceUriOrResourceName));
				}))
			{
				foreach (var definition in objectDefinitions.Definitions)
				{
					var key = definition.Name;
					var list = dictionary.GetOrAdd(key, () => new List<(string configurationLocation, ObjectDefinition definition)>());
					list.Add((configurationResourceFileUriOrconfigurationFile, definition));
					if (definition.PropertyDefinitions?.FirstOrDefault() is ObjectPropertyDefinition objectPropertyDefinition && objectPropertyDefinition.IsSubSet
						&& (objectPropertyDefinition.DictionaryValues != null || objectPropertyDefinition.ListValues != null))
					{
						if (objectPropertyDefinition.DictionaryValues != null)
						{
							if (!dictionaryValuesSubSetDuplicates.ContainsKey(key))
							{
								dictionaryValuesSubSetDuplicates.Add(key, list);
							}
						}
						else if (!listValuesSubSetDuplicates.ContainsKey(key))
						{
							listValuesSubSetDuplicates.Add(key, list);
						}
					}
					else if (list.Count > 1 && !duplicates.ContainsKey(key))
					{
						duplicates.Add(key, list);
					}
				}
			}

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Duplicate keys exists across multiple configurations", Enumerable.Empty<string>(), duplicates.Select(x => $"{x.Key} in the following locations:\r\n{string.Join("\r\n", x.Value.Select(v => v.configurationLocation))}"));
				if (dictionaryValuesSubSetDuplicates.Count > 0 || listValuesSubSetDuplicates.Count > 0)
				{
					var subSetElementDuplicates = new Dictionary<string, List<string>>();
					void GatherData<T>(Dictionary<string, List<(string configurationLocation, ObjectDefinition definition)>> dictionary, Func<ObjectPropertyDefinition, T[]> getValues, Func<T, string> getKey)
					{
						dictionary.ForEach(pair =>
						{
							pair.Value.ForEach(definitionDetail =>
							{
								var dictionaryValues = getValues(definitionDetail.definition.PropertyDefinitions[0]);
								foreach (var dictionaryValue in dictionaryValues)
								{
									var key = $"{pair.Key}|{getKey(dictionaryValue)}";
									subSetElementDuplicates.GetOrAdd(key, () => new List<string>()).Add(definitionDetail.configurationLocation);
								}
							});
						});
					}
					GatherData(dictionaryValuesSubSetDuplicates, propertyDefinition => propertyDefinition.DictionaryValues, dictionaryValue => dictionaryValue.Key.Value);
					GatherData(listValuesSubSetDuplicates, propertyDefinition => propertyDefinition.ListValues, listValue => listValue.ObjectName);
					AssertContainsExactElementsInAnyOrder("Duplicate SubSet elements exists across multiple configurations", Enumerable.Empty<string>(), subSetElementDuplicates.Where(x => x.Value.Count > 1).Select(x => $"{x.Key} in the following locations:\r\n{string.Join("\r\n", x.Value)}"));
				}
			});
		}
	}
}
