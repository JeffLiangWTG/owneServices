
namespace Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions
{
	public static class IEntitySetDefinitionExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "File name for the XML Schema")]
		public static string GetXSDFileName(this IEntitySetDefinition definition)
		{
			return "Native" + definition.Name + ".xsd";
		}

		public static bool IsLegacyOperationalNativeDataSetReplacedByUniversal(this IEntitySetDefinition definition)
		{
			string entitySetName = definition.Name;
			return entitySetName == "Shipment" | entitySetName == "Declaration" || entitySetName == "Order"; // Internal Entity Set Names
		}
	}
}

