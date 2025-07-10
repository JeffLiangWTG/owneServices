using System.Data;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Accounting.Business
{
	public class eNettEDIMessage : EDIMessage
	{
		public eNettEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.ENettMessageNo(GlbCompany.CurrentCompany.PK.ToGuid()).GetNextFormatted(Factory);
		}

		protected XmlInterchange GetXmlInterchange(XmlElement updates)
		{
			XmlValueObjectSerializer interchangeXmlSerialiser = new XmlValueObjectSerializer(typeof(XmlInterchange));
			XmlInterchange result = (XmlInterchange)interchangeXmlSerialiser.DeserialiseFromXmlElement(updates);

			return result;
		}
	}
}