using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(RatingXmlSchemaDefinitions))]
	sealed class RatingXmlSchemaDefinitionsTest : XmlSchemaDefinitionsBaseTest
	{
		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return RatingXmlSchemaDefinitions.Instance;
		}
	}
}
