using System.Xml.Schema;
using CargoWise.ComponentModel;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class XmlValidatorTest_StringContentWithNoNamespaceInSchema : XmlValidatorBaseTest
	{
		protected override void Validate(string xml, XmlSchema schema, INotifications notifications)
		{
			XmlValidator xmlValidator = new XmlValidator(schema);
			xmlValidator.Validate(xml, notifications);
		}

		protected override bool IsSchemaInNoNamespace
		{
			get { return true; }
		}
	}
}
