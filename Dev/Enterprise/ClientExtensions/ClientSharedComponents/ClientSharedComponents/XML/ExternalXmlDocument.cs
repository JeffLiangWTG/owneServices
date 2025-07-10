using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.ClientSharedComponents.XML
{
	public abstract class ExternalXmlDocument
	{
		public ExternalXmlDocument(StreamReader externalXmlFile, INotifications notifications)
		{
			Notification = notifications;
			XmlFile = externalXmlFile;
		}

		#region ConvertToValueObjects

		public IValueObject[] ConvertToValueObjects()
		{
			IValueObject[] result = null;
			XmlValidator validator = new XmlValidator(DocumentSchema);

			XmlDocument externalXmlDoc = new XmlDocument();
			externalXmlDoc.Load(XmlFile);
			XmlFile.BaseStream.Position = 0;

			validator.Validate(XmlFile.BaseStream, Notification);
			XmlFile.BaseStream.Position = 0;

			XmlNodeList rootElements = externalXmlDoc.GetElementsByTagName(RootElementName);

			result = new IValueObject[rootElements.Count];

			for (int i = 0; i < rootElements.Count; i++)
			{
				XmlNode rootElementNode = rootElements[i];
				IValueObject valueObject = GetNewIValueObject();

				StringReader strReader = new StringReader("<" + RootElementName + ">" + rootElementNode.InnerXml + "</" + RootElementName + ">");
				XmlTextReader readXmlFragment = new XmlTextReader(strReader);

				XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(RootElementType);
				valueObject = (IValueObject)serializer.Deserialize(readXmlFragment);

				result[i] = valueObject;
			}

			return result;
		}

		#endregion

		#region Abstracts

		protected abstract XmlSchema DocumentSchema
		{
			get;
		}

		protected abstract string RootElementName
		{
			get;
		}

		protected abstract Type RootElementType
		{
			get;
		}

		protected abstract IValueObject GetNewIValueObject();

		#endregion

		protected readonly StreamReader XmlFile;
		protected readonly INotifications Notification;
	}
}
