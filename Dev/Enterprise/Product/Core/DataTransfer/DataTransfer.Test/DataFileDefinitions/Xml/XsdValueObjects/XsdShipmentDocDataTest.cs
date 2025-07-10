using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(Xsd.ShipmentDocData))]
	sealed class XsdShipmentDocDataTest : ValueObjectTestCase
	{
	}
}
