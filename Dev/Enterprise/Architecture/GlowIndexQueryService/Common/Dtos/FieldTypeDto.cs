namespace GlowIndexQueryService.Common
{
	class FieldTypeDto
	{
		public FieldTypeDto(string dataType, string dateTimeType, string unitType, int? scale, object lookupInfo)
		{
			DataType = dataType;
			DateTimeType = dateTimeType;
			UnitType = unitType;
			Scale = scale;
			LookupInfo = lookupInfo;
		}

		public string DataType { get; }
		public string DateTimeType { get; }
		public string UnitType { get; }
		public int? Scale { get; }
		public object LookupInfo { get; }
	}
}
