using System;
using System.IO;
using System.Xml.Schema;

using CargoWise.ComponentModel;
using Enterprise.ClientSharedComponents.XML;
using Enterprise.DataTransfer.Xml;
using BtaXsd = Enterprise.Client.WFN.Definition;

namespace Enterprise.Client.WFN
{
	public class WFNXmlDocument : ExternalXmlDocument
	{
		public WFNXmlDocument(StreamReader document, INotifications notification)
			: base(document, notification)
		{
		}

		protected override XmlSchema DocumentSchema
		{
			get { return WFNSchemaDefinition.Instance.WFNSchema; }
		}

		protected override string RootElementName
		{
			get { return "MASTER"; }
		}

		protected override Type RootElementType
		{
			get { return typeof(BtaXsd.MASTER); }
		}

		protected override IValueObject GetNewIValueObject()
		{
			return new BtaXsd.MASTER();
		}
		#region internal testing variables
		internal XmlSchema internalDocumentSchemaTest => DocumentSchema;
		internal Type internalRootElementTypeTest => RootElementType;
		internal string internalRootElementNameTest => RootElementName;
		internal IValueObject internalGetNewIValueObjectTest() => GetNewIValueObject();
		#endregion
	}
}
