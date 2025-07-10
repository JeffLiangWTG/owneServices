using System;
using System.IO;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using Enterprise.ClientSharedComponents.XML;

namespace Enterprise.Client.TEL.Definition
{
	internal class TELConsolShipXmlDocument : ExternalXmlDocument
	{
		public TELConsolShipXmlDocument(StreamReader document, INotifications notification)
			: base(document, notification)
		{
		}

		protected override XmlSchema DocumentSchema
		{
			get { return TELConsolShipSchemaDefinition.Instance.ConsolShipSchema; }
		}

		protected override string RootElementName
		{
			get
			{
				return "SeaMasterBills";
			}
		}

		protected override Type RootElementType
		{
			get
			{
				return typeof(SeaMasterBills);
			}
		}

		protected override DataTransfer.Xml.IValueObject GetNewIValueObject()
		{
			return new SeaMasterBills();
		}
	}
}
