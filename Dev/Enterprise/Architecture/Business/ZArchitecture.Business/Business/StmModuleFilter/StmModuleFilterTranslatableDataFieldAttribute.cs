using System;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	sealed class StmModuleFilterTranslatableDataFieldAttribute : TranslatableDataFieldAttribute, ICustomizableDataCaptionSource
	{
		public StmModuleFilterTranslatableDataFieldAttribute(string tableName, string columnName, string dataXmlFilePaths, string contextColumnName)
			: base(tableName, columnName, dataXmlFilePaths, contextColumnName)
		{
		}

		public override ZQuery Filter
		{
			get
			{
				return new ZQuery(StmModuleFilterSchema.S9_IsPublished, true);
			}
		}
	}
}

