using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssembly("Enterprise.DataTransfer.XmlSerializers")]
	[XmlType(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRoot(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclass("Enterprise.DataTransfer.Xml.XsdVersion1.AutoXmlInterchange")]
	public class XmlInterchange : AutoXmlInterchange
	{
		public static XmlInterchange Empty
		{
			get
			{
				var interchnage = new XmlInterchange();
				interchnage.IsSpecified = false;
				return interchnage;
			}
		}

		[XmlIgnore]
		public bool ImportEDICode;

		public static XmlInterchange NewPopulatedInterchange(BusinessObjectFactory factory)
		{
			return NewPopulatedInterchange(factory, new ValueObjectExportContext(new NotificationBuffer()));
		}

		public static XmlInterchange NewPopulatedInterchange(BusinessObjectFactory factory, IValueObjectExportContext context)
		{
			var result = new XmlInterchange();
			result.Version = "1";

			var currentCompany = Env.CurrentCompany;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var currentUser = GlbStaff.CurrentUser;

			var organisation = (OrgHeader)factory.Load(typeof(OrgHeader), currentCompany.OrganisationPK);
			result.InterchangeInfo.EDIOrganisation = new OrganisationValueObjectDataAdapter().ExportToValueObject(organisation, context);
			result.InterchangeInfo.Source.CompanyCode = currentCompany.Code;
			result.InterchangeInfo.Source.EnterpriseCode = registrationKey.EnterpriseCode;
			result.InterchangeInfo.Source.OriginServer = registrationKey.ServerCode;
			result.InterchangeInfo.Source.LoginName = currentUser.GS_LoginName;
			result.InterchangeInfo.Source.LoginUserEmailAddress = currentUser.GS_EmailAddress;
			result.InterchangeInfo.Date = ZDateTime.Now;

			result.InterchangeInfo.XmlType = context.SimplifiedXML ? XmlType.LightWeight : XmlType.Verbose;

			return result;
		}

		#region DeserializeInterchangeAndPayload

		public static object DeserializeInterchangeAndPayload(XmlReader dataReader, XmlValueObjectSerializer payloadSerializer, out XmlInterchange xmlInterchange)
		{
			return DeserializeInterchangeAndPayload(dataReader, payloadSerializer, out xmlInterchange, null, false);
		}

		public static object DeserializeInterchangeAndPayload(XmlReader dataReader, XmlValueObjectSerializer payloadSerializer, out XmlInterchange xmlInterchange, INotifications notifications)
		{
			return DeserializeInterchangeAndPayload(dataReader, payloadSerializer, out xmlInterchange, notifications, false);
		}

		public static object DeserializeInterchangeAndPayload(XmlReader dataReader, XmlValueObjectSerializer payloadSerializer, out XmlInterchange xmlInterchange, bool elementType)
		{
			return DeserializeInterchangeAndPayload(dataReader, payloadSerializer, out xmlInterchange, null, elementType);
		}

		public static object DeserializeInterchangeAndPayload(XmlReader dataReader, XmlValueObjectSerializer payloadSerializer, out XmlInterchange xmlInterchange, INotifications notifications, bool elementType)
		{
			xmlInterchange = ReadInterchangeOnly(dataReader, notifications);

			dataReader.Read();
			dataReader.MoveToContent(); // Move to collection

			if (elementType)
			{
				dataReader.Read();
				dataReader.MoveToContent(); // Move to element
			}

			if (payloadSerializer.CanDeserialize(dataReader))
			{
				xmlInterchange.Payload.Data = payloadSerializer.Deserialize(dataReader);
			}
			else
			{
				dataReader.ReadStartElement();
				List<object> result = new List<object>();
				do
				{
					result.Add(payloadSerializer.Deserialize(dataReader));
				} while (payloadSerializer.CanDeserialize(dataReader));
				xmlInterchange.Payload.Data = result;
			}

			return xmlInterchange.Payload.Data;
		}

		#endregion

		#region ReadInterchangeOnly

		public static XmlInterchange ReadInterchangeOnly(TextReader dataReader, INotifications notifications)
		{
			return ReadInterchangeOnly(new PayloadSkippingXmlReader(XmlReader.Create(dataReader)), notifications);
		}

		public static XmlInterchange ReadInterchangeOnly(XmlReader dataReader, INotifications notifications)
		{
			return ReadInterchangeOnly(new PayloadSkippingXmlReader(dataReader), notifications);
		}

		static XmlInterchange ReadInterchangeOnly(PayloadSkippingXmlReader reader, INotifications notifications)
		{
			reader.MoveToContent();
			var interchangeXmlSerialiser = new XmlValueObjectSerializer(typeof(XmlInterchange));
			var canDeserialize = interchangeXmlSerialiser.CanDeserialize(reader);

			XmlInterchange result = null;
			if (canDeserialize)
			{
				try
				{
					result = (XmlInterchange)interchangeXmlSerialiser.Deserialize(reader);
				}
				catch (XmlException ex)
				{
					if (notifications != null)
					{
						notifications.Notify(new ErrorNotification(ErrorType.XmlSchemaValidation, ex.Message));
					}
					else
					{
						throw;
					}
				}
			}

			return result;
		}

		public class PayloadSkippingXmlReader : XmlReaderDelegator
		{
			public PayloadSkippingXmlReader(XmlReader reader) : base(reader) { }

			public override bool Read()
			{
				return !CheckFinished() && base.Read();
			}

			public override void ReadEndElement()
			{
				if (CheckFinished())
				{
					return;
				}
				base.ReadEndElement();
			}

			public override XmlNodeType NodeType
			{
				get { return CheckFinished() ? XmlNodeType.EndElement : base.NodeType; }
			}

			bool CheckFinished()
			{
				if (!finished && base.NodeType == XmlNodeType.Element && LocalName == PayloadElement)
				{
					finished = true;
				}
				return finished;
			}

			bool finished;

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "XML element name")]
			const string PayloadElement = "Payload";
		}

		#endregion
	}
}
