using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors;

namespace Enterprise.DocumentEngine.ValueProviders
{
	public class ValueProviderMap : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ValueProviderMap()
		{
		}

		public MacroValueProviderMapCollection Macros
		{
			get
			{
				if (macros == null)
				{
					macros = new MacroValueProviderMapCollection();
					PopulateMacroMapElements(macros);
				}

				return macros;
			}
		}

		MacroValueProviderMapCollection macros;

		protected virtual void PopulateMacroMapElements(MacroValueProviderMapCollection collection)
		{
			ValueProviderCollector collector = new ValueProviderCollector();
			foreach (ValueProvider provider in collector.ValueProviders.Providers)
			{
				try
				{
					if (!provider.IsDocumentationVisible)
					{
						continue;
					}

					var documentation = provider.Documentation;
					var macroValueMap = new MacroValueProviderMap(provider.GetType().Name, documentation.Useage, documentation.Explanation);
					collection.Add(macroValueMap);
				}
				catch (NullReferenceException ex)
				{
					ex.Data["Provider"] = provider?.GetType().FullName;
					throw;
				}
			}

			// Map Function Extractor providers
			collection.Add(new MacroValueProviderMap(nameof(GetCustomFieldFunctionExtractor), "<GetCustomField({CustomFieldName})>", Res.GetString("5882d24d-63d7-4487-be4d-b3c35bd7e9cd", "Returns the value stored in a customized field on an element.")));
			collection.Add(new MacroValueProviderMap(nameof(GetCustomFieldWithTypeFunctionExtractor), "<GetCustomFieldWithType({CustomFieldName}, {CustomFieldType})>", Res.GetString("E3107FE8-1D56-413C-AB2F-66C2BB10A6CD", "Returns the value stored in a customized field of a specific type on an element.")));
			collection.Add(new MacroValueProviderMap(nameof(GetCustomFieldCodeDescriptionFunctionExtractor), "<GetCustomFieldCodeDescription({CustomFieldName})>", Res.GetString("350e8838-40c7-4b5e-9d86-ca17aaad463f", "Returns the description of a value stored in customized field on an element.")));
			collection.Add(new MacroValueProviderMap(nameof(GetCustomFieldCodeDescriptionWithTypeFunctionExtractor), "<GetCustomFieldCodeDescriptionWithType({CustomFieldName}, {CustomFieldType})>", Res.GetString("81997D35-A702-481F-BA0D-9599B5CD5C96", "Returns the description of a value stored in customized field of a specific type on an element.")));
			collection.Add(new MacroValueProviderMap(nameof(GetEventLastDateTimeFunctionExtractor), "<GetEventLastDateTime({EventCode})>",
				Res.GetString("57868f79-9b8b-45e9-98c8-53ae71fcada3", @"Returns last occurrence date of event with specified event code (ignores estimated and canceled events). If there is no such event, empty string is returned.
				E.g: {0}", "<GetEventLastDateTime(Z00)>")));
		}
	}
}
