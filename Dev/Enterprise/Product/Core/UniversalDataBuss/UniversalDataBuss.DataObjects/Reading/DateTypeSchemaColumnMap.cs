using CargoWise.Schema;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public class DateTypeSchemaColumnMap
	{
		public DateTypeSchemaColumnMap(SchemaDateTimeColumn schemaColumn, DateType dateType)
			: this(schemaColumn, new DateType[] { dateType })
		{
		}

		public DateTypeSchemaColumnMap(SchemaDateTimeColumn schemaColumn, DateType[] dateTypes)
		{
			this.SchemaColumn = schemaColumn;
			this.DateTypes = dateTypes;
		}

		public readonly SchemaDateTimeColumn SchemaColumn;
		public readonly DateType[] DateTypes;
	}
}
