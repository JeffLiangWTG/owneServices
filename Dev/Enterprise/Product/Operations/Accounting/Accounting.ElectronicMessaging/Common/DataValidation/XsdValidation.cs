using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Common.DataValidation
{
	public class XsdValidation : IPayloadValidation
	{
		ZString[] XsdResourceNames { get; }

		public XsdValidation(params ZString[] xsdResourceNames)
		{
			if (xsdResourceNames == null || xsdResourceNames.Length == 0)
			{
				throw new ArgumentException("xsdResourceNames empty or null is not supported.");
			}

			XsdResourceNames = xsdResourceNames;
		}

		void IPayloadValidation.Validate(Stream stream, INotifications errorNotifications, INotifications warningNotifications)
		{
			ValidateXml(stream, errorNotifications, warningNotifications);
		}

		public IXPathNavigable ValidateXml(Stream stream, INotifications errorNotifications, INotifications warningNotifications)
		{
			stream.Position = 0;

			var document = new XmlDocument();
			var settings = new XmlReaderSettings();

			settings.ValidationType = System.Xml.ValidationType.Schema;
			settings.ValidationEventHandler += ValidationEventHandler;

			foreach (var xsdResourceName in XsdResourceNames)
			{
				using (var xsdStream = GetType().Assembly.GetManifestResourceStream(xsdResourceName))
				{
					settings.Schemas.Add(XmlSchema.Read(xsdStream, ValidationEventHandler));
				}
			}

			using (var reader = XmlReader.Create(stream, settings))
			{
				document.Load(reader);
			}

			return document;

			void ValidationEventHandler(object sender, ValidationEventArgs e)
			{
				if (e.Severity == XmlSeverityType.Warning)
				{
					warningNotifications.AddWarning(e.Message);
				}
				else if (e.Severity == XmlSeverityType.Error)
				{
					errorNotifications.AddError(e.Message);
				}
			}
		}
	}
}
