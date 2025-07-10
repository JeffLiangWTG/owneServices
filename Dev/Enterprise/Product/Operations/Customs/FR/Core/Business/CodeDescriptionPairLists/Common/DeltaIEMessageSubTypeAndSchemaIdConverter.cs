using CargoWise.Types;

namespace Enterprise.Customs.FR.Business
{
	public static class DeltaIEMessageSubTypeAndSchemaIdConverter
	{
		public static ZString ConvertMessageSubTypeToSchemaId(ZString messageSubType)
		{
			if (messageSubType.StartsWith("1"))
			{
				return "FRA" + messageSubType;
			}
			else
			{
				return "IE" + messageSubType;
			}
		}

		public static ZString ConvertSchemaIdToMessageSubType(ZString schemaId)
		{
			return schemaId.KeepNumericCharacters();
		}
	}
}
