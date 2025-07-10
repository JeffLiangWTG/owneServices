namespace Enterprise.Builder.Generator
{
	using System;
	using System.Text;
	using CargoWise.BuildTools;
	using CargoWise.Data;

	public class BuildXmlBizOEntryWrapper
	{
		public BuildXmlBizOEntryWrapper(BuildXmlBizOEntry bizObjEntry)
		{
			this.bizObjEntry = bizObjEntry;
		}

		readonly BuildXmlBizOEntry bizObjEntry;

		public bool PreventDelete { get { return bizObjEntry?.PreventDelete ?? true; } }
		public string RefDbCountry { get { return bizObjEntry?.RefDbCountry; } }

		public RefDbTypeEnum? ReferenceDbType
		{
			get
			{
				if (!referenceDbTypeSet)
				{
					if (bizObjEntry != null)
					{
						if (String.Equals(bizObjEntry.RefDbType, nameof(RefDbTypeEnum.Customs), StringComparison.OrdinalIgnoreCase))
						{
							referenceDbType = RefDbTypeEnum.Customs;
						}
						else if (String.Equals(bizObjEntry.RefDbType, nameof(RefDbTypeEnum.Enterprise), StringComparison.OrdinalIgnoreCase))
						{
							referenceDbType = RefDbTypeEnum.Enterprise;
						}
						else if (String.Equals(bizObjEntry.RefDbType, nameof(RefDbTypeEnum.Tariff), StringComparison.OrdinalIgnoreCase))
						{
							referenceDbType = RefDbTypeEnum.Tariff;
						}
						else if (String.Equals(bizObjEntry.RefDbType, nameof(RefDbTypeEnum.Single), StringComparison.OrdinalIgnoreCase))
						{
							referenceDbType = RefDbTypeEnum.Single;
						}
					}

					referenceDbTypeSet = true;
				}

				return referenceDbType;
			}
		}
		RefDbTypeEnum? referenceDbType;
		bool referenceDbTypeSet;

		public string SchemaClassFolder
		{
			get
			{
				if (schemaClassFolder == null)
				{
					schemaClassFolder = (ReferenceDbType == null) ?
						"" :
						RefDbTableNameResolver.GetRefDbSchemaClassFolder(ReferenceDbType.Value, RefDbCountry);
				}

				return schemaClassFolder;
			}
		}

		string schemaClassFolder;

		public string GetRegenTimeDatabaseName(string baseDbName, bool addBrackets = true)
		{
			return (ReferenceDbType == null) ?
				baseDbName :
				(addBrackets ? "[" : "") + ((IPhysicalRefDbLocation)Db.Connection).GetReferenceDatabaseName(ReferenceDbType.Value, RefDbCountry) + (addBrackets ? "]" : "");
		}
	}

	public class DatabaseBizObjectTableCollection
	{
		public DatabaseBizObjectTableCollection(BuildXmlBizOEntryWrapper dbSampleBizObj)
		{
			this.dbSampleBizObj = dbSampleBizObj;
			this.csvBizObjTableList = new StringBuilder();
		}

		readonly StringBuilder csvBizObjTableList;

		public string CsvBizObjTableList
		{
			get { return csvBizObjTableList.ToString(); }
		}

		public void AppendBizObjTable(string bizObjTable)
		{
			if (csvBizObjTableList.Length != 0)
			{
				csvBizObjTableList.Append(",");
			}

			csvBizObjTableList.Append("'" + bizObjTable + "'");
		}

		public string GetRegenTimeDatabaseName(string baseDbName, bool addBrackets = true)
		{
			return dbSampleBizObj.GetRegenTimeDatabaseName(baseDbName, addBrackets);
		}

		readonly BuildXmlBizOEntryWrapper dbSampleBizObj;
	}
}
