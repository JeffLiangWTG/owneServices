using System;
using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.CodeMappings;

namespace Enterprise.DataTransfer.Native.Common.Definitions
{
	public class GlobalDefinition
	{
		#region Construction

		[ThreadStatic] static GlobalDefinition instance;

		public static GlobalDefinition Instance
		{
			get { return instance ?? (instance = new GlobalDefinition()); }
		}

#if DEBUG

		public static void ResetStaticCacheForTesting()
		{
			instance = null;
		}

		public void AddToTableMappingsForTesting(string key, string value)
		{
			this.tableMapping.Add(key, value);
		}

		public void AddDummyBizoToTableMappings()
		{
			this.tableMapping.Add("DummyBizo", "Dummy");
		}

#endif

		GlobalDefinition()
		{
			var builder = new GlobalDefinitionBuilder();
			var definitionElement = builder.LoadDefinition();
			this.tableMapping = builder.ParseTableMappings(definitionElement);
			this.excludedProperties = builder.ParseExcludedProperties(definitionElement);
			this.mappings = builder.ParseDefinitionElement(definitionElement);
		}

		#endregion

		public IDictionary<string, string> TableMapping
		{
			get
			{
				return tableMapping;
			}
		}
		readonly Dictionary<string, string> tableMapping;

		public IEnumerable<string> ExcludedProperties
		{
			get
			{
				return excludedProperties;
			}
		}
		readonly IEnumerable<string> excludedProperties;

		public CodeMappingCollection CodeMappings
		{
			get { return new CodeMappingCollection(mappings); }
		}
		readonly IEnumerable<CodeMapping> mappings;
	}
}
