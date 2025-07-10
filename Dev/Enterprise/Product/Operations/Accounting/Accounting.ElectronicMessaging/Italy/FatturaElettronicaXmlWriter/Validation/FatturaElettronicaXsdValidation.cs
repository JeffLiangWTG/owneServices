using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using CargoWise.ComponentModel;

namespace Enterprise.Accounting.ElectronicMessaging.Italy
{
	public class FatturaElettronicaXsdValidation
	{
		public FatturaElettronicaXsdValidation(INotifications notifications)
		{
			Notifications = notifications;
		}

		// Return IXPathNavigable to avoid CA1059 warning.
		public IXPathNavigable ValidateXml(Stream stream)
		{
			stream.Position = 0;

			var document = new XmlDocument();
			var settings = new XmlReaderSettings();

			settings.ValidationType = System.Xml.ValidationType.Schema;
			settings.ValidationEventHandler += new ValidationEventHandler(ValidationEventHandler);

			using (var fatturaElettronicaXsdStream = GetType().Assembly.GetManifestResourceStream(XsdSchemaResource))   // Developer only xsd file name.
			using (var signatureXsdStream = GetType().Assembly.GetManifestResourceStream("Enterprise.Accounting.ElectronicMessaging.Italy.FatturaElettronicaXmlWriter.xmldsig-core-schema.xsd"))   // Developer only xsd file name.
			{
				settings.Schemas.Add(XmlSchema.Read(signatureXsdStream, ValidationEventHandler));
				settings.Schemas.Add(XmlSchema.Read(fatturaElettronicaXsdStream, ValidationEventHandler));
			}

			using (var reader = XmlReader.Create(stream, settings))
			{
				document.Load(reader);
			}

			return document;
		}

		void ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			if (e.Severity == XmlSeverityType.Warning)
			{
				Notifications.AddWarning(e.Message);
			}
			else if (e.Severity == XmlSeverityType.Error)
			{
				Notifications.AddError(e.Message);
			}
		}

		readonly INotifications Notifications;
		public string XsdSchemaResource => FatturaElettronicaDataHelper.ShouldUseNewSchema
				? "Enterprise.Accounting.ElectronicMessaging.Italy.FatturaElettronicaXmlWriter.FatturaPA_versione_1.2.1.xsd"
				: "Enterprise.Accounting.ElectronicMessaging.Italy.FatturaElettronicaXmlWriter.FatturaPA_versione_1.2.xsd";
	}
}
