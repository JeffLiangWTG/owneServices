using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.IO;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class XmlValidatorTest_StreamContentWithNoNamespaceInSchema : XmlValidatorBaseTest
	{
		protected override void Validate(string xml, XmlSchema schema, INotifications notifications)
		{
			XmlValidator xmlValidator = new XmlValidator(schema);
			xmlValidator.Validate(StreamConverter.StringToStream(xml), notifications);
		}

		protected override bool IsSchemaInNoNamespace
		{
			get { return true; }
		}
	}
}
