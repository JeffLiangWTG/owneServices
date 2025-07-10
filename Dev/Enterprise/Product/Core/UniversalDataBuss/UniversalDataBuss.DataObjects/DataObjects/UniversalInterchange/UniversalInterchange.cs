using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[Serializable]
	[XmlRoot("UniversalInterchange", Namespace = "http://www.cargowise.com/Schemas/Universal/2011/11")]
	public class UniversalInterchange
	{
		[XmlElement(ElementName = "Header")]
		public UniversalInterchangeHeader Header { get; set; }

		[XmlElement(ElementName = "Body")]
		public UniversalInterchangeBody Body { get; set; }

		public UniversalInterchange()
		{
			Header = new UniversalInterchangeHeader();
			Header.DeliveryMetadata = new UniversalInterchangeHeaderDeliveryMetadata();
			Body = new UniversalInterchangeBody();
		}

		public UniversalInterchange(UniversalInterchangeHeaderDeliveryMetadata metaData, string bodyText, string senderID, string recipientID)
		{
			Header = new UniversalInterchangeHeader();
			Header.SenderID = senderID;
			Header.RecipientID = recipientID;
			Header.DeliveryMetadata = metaData;
			Body = new UniversalInterchangeBody
			{
				BodyXml = bodyText
			};
		}
	}

	[Serializable]
	public partial class UniversalInterchangeHeader
	{
		string senderIDField;

		string recipientIDField;

		UniversalInterchangeHeaderAcknowledgement acknowledgementField;

		UniversalInterchangeHeaderDeliveryMetadata deliveryMetadataField;

		/// <remarks/>
		public string SenderID
		{
			get
			{
				return this.senderIDField;
			}
			set
			{
				this.senderIDField = value;
			}
		}

		/// <remarks/>
		public string RecipientID
		{
			get
			{
				return this.recipientIDField;
			}
			set
			{
				this.recipientIDField = value;
			}
		}

		/// <remarks/>
		public UniversalInterchangeHeaderAcknowledgement Acknowledgement
		{
			get
			{
				return this.acknowledgementField;
			}
			set
			{
				this.acknowledgementField = value;
			}
		}

		/// <remarks/>
		public UniversalInterchangeHeaderDeliveryMetadata DeliveryMetadata
		{
			get
			{
				return this.deliveryMetadataField;
			}
			set
			{
				this.deliveryMetadataField = value;
			}
		}
	}

	[Serializable]
	public partial class UniversalInterchangeHeaderAcknowledgement
	{
		UniversalInterchangeHeaderAcknowledgementRequired requiredField;

		UniversalInterchangeHeaderAcknowledgementChannel channelField;

		string recipientIDField;

		UniversalInterchangeHeaderAcknowledgementContext[] contextCollectionField;

		/// <remarks/>
		public UniversalInterchangeHeaderAcknowledgementRequired Required
		{
			get
			{
				return this.requiredField;
			}
			set
			{
				this.requiredField = value;
			}
		}

		/// <remarks/>
		public UniversalInterchangeHeaderAcknowledgementChannel Channel
		{
			get
			{
				return this.channelField;
			}
			set
			{
				this.channelField = value;
			}
		}

		/// <remarks/>
		public string RecipientID
		{
			get
			{
				return this.recipientIDField;
			}
			set
			{
				this.recipientIDField = value;
			}
		}

		/// <remarks/>
		[XmlArrayItem("Context", IsNullable = false)]
		public UniversalInterchangeHeaderAcknowledgementContext[] ContextCollection
		{
			get
			{
				return this.contextCollectionField;
			}
			set
			{
				this.contextCollectionField = value;
			}
		}
	}

	public enum UniversalInterchangeHeaderAcknowledgementRequired
	{
		/// <remarks/>
		OnAll,

		/// <remarks/>
		OnError,

		/// <remarks/>
		OnSuccess,
	}

	public enum UniversalInterchangeHeaderAcknowledgementChannel
	{
		/// <remarks/>
		eHub,

		/// <remarks/>
		eAdaptor,
	}

	public partial class UniversalInterchangeHeaderAcknowledgementContext
	{
		string typeField;

		string valueField;

		/// <remarks/>
		public string Type
		{
			get
			{
				return this.typeField;
			}
			set
			{
				this.typeField = value;
			}
		}

		/// <remarks/>
		public string Value
		{
			get
			{
				return this.valueField;
			}
			set
			{
				this.valueField = value;
			}
		}
	}

	[Serializable]
	public partial class UniversalInterchangeHeaderDeliveryMetadata
	{
		UniversalInterchangeHeaderDeliveryMetadataValue[] valueCollectionField;

		/// <remarks/>
		[XmlArrayItem("Value", IsNullable = false)]
		public UniversalInterchangeHeaderDeliveryMetadataValue[] ValueCollection
		{
			get
			{
				return this.valueCollectionField;
			}
			set
			{
				this.valueCollectionField = value;
			}
		}
	}

	public partial class UniversalInterchangeHeaderDeliveryMetadataValue
	{
		string nameField;

		UniversalInterchangeHeaderDeliveryMetadataValueType typeField;

		string dataField;

		/// <remarks/>
		public string Name
		{
			get
			{
				return this.nameField;
			}
			set
			{
				this.nameField = value;
			}
		}

		/// <remarks/>
		public UniversalInterchangeHeaderDeliveryMetadataValueType Type
		{
			get
			{
				return this.typeField;
			}
			set
			{
				this.typeField = value;
			}
		}

		/// <remarks/>
		public string Data
		{
			get
			{
				return this.dataField;
			}
			set
			{
				this.dataField = value;
			}
		}
	}

	public enum UniversalInterchangeHeaderDeliveryMetadataValueType
	{
		/// <remarks/>
		String,

		/// <remarks/>
		DateTime,

		/// <remarks/>
		Integer,

		/// <remarks/>
		Decimal,

		/// <remarks/>
		Byte,

		/// <remarks/>
		Boolean,

		/// <remarks/>
		Short,

		/// <remarks/>
		DateTimeOffset,

		/// <remarks/>
		Geography,

		/// <remarks/>
		Base64Binary,
	}

	[Serializable]
	public class UniversalInterchangeBody : IXmlSerializable
	{
		public string BodyXml { get; set; }

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			BodyXml = reader.ReadInnerXml();
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteRaw(BodyXml);
		}
	}
}
