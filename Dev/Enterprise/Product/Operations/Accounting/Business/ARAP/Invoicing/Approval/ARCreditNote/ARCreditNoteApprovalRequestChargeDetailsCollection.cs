using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	[XmlRoot("ArrayOfChargeDetails")] //for compatibility with existing XMLs
	public class ARCreditNoteApprovalRequestChargeDetailsCollection :
		NonPersistentBusinessObjectCollection<ARCreditNoteApprovalRequestChargeDetails>,
		IXmlSerializable //custom serialization needed here to support XMLRoot attribute on collection elements for compatibility with existing XMLs. Default serialisation ignore XMLRoot on collection elements
	{
		[Obsolete("For serializer only")]
		public ARCreditNoteApprovalRequestChargeDetailsCollection()
			: base(new BusinessObjectFactory())
		{
		}

		public ARCreditNoteApprovalRequestChargeDetailsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var chargeDetails = new ARCreditNoteApprovalRequestChargeDetails(Factory);
			chargeDetails.ApprovalType = ApprovalType;
			return chargeDetails;
		}

		public ZString ApprovalType { get; set; }

		#region Serialization

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.IsEmptyElement)
			{
				reader.ReadStartElement();
			}
			else
			{
				reader.ReadStartElement();

				var serializer = ZXmlSerializer.New(typeof(ARCreditNoteApprovalRequestChargeDetails));
				while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
				{
					Add((ARCreditNoteApprovalRequestChargeDetails)serializer.Deserialize(reader));
				}

				reader.ReadEndElement();
			}
		}

		public void WriteXml(System.Xml.XmlWriter writer)
		{
			var serializer = ZXmlSerializer.New(typeof(ARCreditNoteApprovalRequestChargeDetails));
			foreach (var item in this)
			{
				serializer.Serialize(writer, item);
			}
		}

		#endregion
	}
}
