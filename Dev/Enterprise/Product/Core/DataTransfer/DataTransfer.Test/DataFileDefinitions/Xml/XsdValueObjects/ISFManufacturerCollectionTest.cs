using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.ISFManufacturerCollection))]
	sealed class ISFManufacturerCollectionTest : ValueObjectCollectionTestCase
	{
	}
}
