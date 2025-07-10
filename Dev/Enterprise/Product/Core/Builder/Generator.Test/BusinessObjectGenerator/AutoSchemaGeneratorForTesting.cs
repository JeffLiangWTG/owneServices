using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Enterprise.Builder.Generator
{
	sealed class AutoSchemaGeneratorForTesting : AutoSchemaGenerator
	{
		public AutoSchemaGeneratorForTesting(GeneratorOutputDirectory outputDirectory)
			: base(outputDirectory)
		{
		}

		public Dictionary<string, DatabaseBizObjectTableCollection> DatabaseCommaDelimitedBusinessObjectTableTextList_Exposed
		{
			get { return DatabaseCommaDelimitedBusinessObjectTableTextList; }
		}

		public DataTable[] PersistantTablesColumns_Exposed
		{
			get { return PersistantTablesColumns; }
		}

		public string[] GeneratedNonBizObjTableSchemaList
		{
			get { return (string[])fGeneratedNonBizObjTableSchemaList.ToArray(typeof(string)); }
		}
		readonly ArrayList fGeneratedNonBizObjTableSchemaList = new ArrayList();

		protected override void GenerateSingleNonBizObjTableSchema(string dbName, string subFolder, string tableName)
		{
			if (!string.IsNullOrEmpty(subFolder))
			{
				subFolder += "/";
			}

			fGeneratedNonBizObjTableSchemaList.Add(subFolder + tableName);
		}
	}
}
