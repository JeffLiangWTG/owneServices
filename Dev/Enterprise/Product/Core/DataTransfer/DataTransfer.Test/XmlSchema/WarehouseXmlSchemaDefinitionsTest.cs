using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(WarehouseXmlSchemaDefinitions))]
	sealed class WarehouseXmlSchemaDefinitionsTest : XmlSchemaDefinitionsBaseTest
	{
		#region Implementation

		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return WarehouseXmlSchemaDefinitions.Instance;
		}

		#endregion

	}
}
