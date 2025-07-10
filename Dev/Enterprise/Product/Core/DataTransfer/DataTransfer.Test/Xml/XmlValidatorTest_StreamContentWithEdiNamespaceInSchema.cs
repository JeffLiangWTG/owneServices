using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.IO;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class XmlValidatorTest_StreamContentWithEdiNamespaceInSchema : XmlValidatorBaseTest
	{
		protected override void Validate(string xml, XmlSchema schema, INotifications notifications)
		{
			XmlValidator xmlValidator = new XmlValidator(schema);
			xmlValidator.Validate(StreamConverter.StringToStream(xml), notifications);
		}

		protected override bool IsSchemaInNoNamespace
		{
			get { return false; }
		}
	}
}
