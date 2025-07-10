#if DEBUG

using System.Xml.Serialization;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.XmlSerializers")]
	public class TestSomeRandomValueObject : ValueObject
	{
	}
}

#endif
