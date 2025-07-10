using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Modules
{
	public class ZLimitedColumnsProvider : IZLimitedColumnsProvider
	{
		public ZLimitedColumnsProvider()
		{
		}

		public ZLimitedColumnsProvider(Type businessObjectType)
		{
			if (businessObjectType == null)
			{
				throw new ArgumentNullException(nameof(businessObjectType));
			}

			this.businessObjectType = businessObjectType;
			var customAttributes = businessObjectType.GetCustomAttributes(true);
			codePropertyName = customAttributes.OfType<CodePropertyAttribute>().FirstOrDefault()?.PropertyName;
			descriptionPropertyName = customAttributes.OfType<DescriptionPropertyAttribute>().FirstOrDefault()?.PropertyName;
		}

		public ZLimitedColumnsProvider(SchemaColumn codeSchemaColumn, SchemaColumn descriptionSchemaColumn)
		{
			if (codeSchemaColumn == null)
			{
				throw new ArgumentNullException(nameof(codeSchemaColumn));
			}

			if (descriptionSchemaColumn == null)
			{
				throw new ArgumentNullException(nameof(descriptionSchemaColumn));
			}

			this.codeSchemaColumn = codeSchemaColumn;
			this.descriptionSchemaColumn = descriptionSchemaColumn;
		}

		public bool LimitColumnExists => !string.IsNullOrEmpty(codePropertyName) || !string.IsNullOrEmpty(descriptionPropertyName);

		string TableName => BusinessObjectFactory.GetTableNameFromType(businessObjectType);

		public SchemaColumn CodeSchemaColumn => codeSchemaColumn ?? (codeSchemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(codePropertyName, TableName));
		public SchemaColumn DescriptionSchemaColumn => descriptionSchemaColumn ?? (descriptionSchemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(descriptionPropertyName, TableName));

		public string CodeColumnName => CodeSchemaColumn?.Name ?? codePropertyName;
		public string DescriptionColumnName => DescriptionSchemaColumn?.Name ?? descriptionPropertyName;

		readonly Type businessObjectType;
		readonly string codePropertyName;
		readonly string descriptionPropertyName;
		SchemaColumn codeSchemaColumn;
		SchemaColumn descriptionSchemaColumn;
	}
}
