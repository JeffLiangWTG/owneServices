using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Support
{
	public abstract class OperationalActionMethodSettings : NonPersistentBusinessObject, IXmlSerializable
	{
		protected OperationalActionMethodSettings() { }
		protected OperationalActionMethodSettings(BusinessObjectFactory factory)
			: base(factory) { }

		protected abstract void ReadXml(XmlReader reader);
		protected abstract void WriteXml(XmlWriter writer);

		#region IXmlSerializable Members

		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			ReadXml(reader);
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			WriteXml(writer);
		}

		#endregion
	}
}
