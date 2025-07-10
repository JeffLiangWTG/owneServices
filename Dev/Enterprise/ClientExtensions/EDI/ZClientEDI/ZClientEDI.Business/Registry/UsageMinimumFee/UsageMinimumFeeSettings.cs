using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class UsageMinimumFeeSettings : AutoUsageMinimumFeeSettings
	{
		public UsageMinimumFeeSettings()
		{
		}

		public UsageMinimumFeeSettings(BusinessObjectFactory factory)
			: base(factory) { }

		public UsageMinimumFeeSettings(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public UsageMinimumFeeSettings(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new UsageMinimumFeeSettings(fallbackLevel, factory);
			foreach (var minimumFee in (UsageMinimumFeeCollection)this.MinimumFeeList.Clone(fallbackLevel, factory))
			{
				clone.MinimumFeeList.Add(minimumFee);
			}
			return clone;
		}

		public UsageMinimumFeeCollection MinimumFeeList
		{
			get
			{
				if (minimumFees == null)
				{
					minimumFees = new UsageMinimumFeeCollection();
					RegisterEditableChildObject(minimumFees);
				}
				return minimumFees;
			}
		}
		UsageMinimumFeeCollection minimumFees;

		protected override void ReadMinimumFeeList(XmlReader reader)
		{
			MinimumFeeList.RemoveAll();
			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();
				while (reader.IsStartElement("MinimumFeeUsage"))
				{
					var minimumFee = MinimumFeeList.AddNew();
					((IXmlSerializable)minimumFee).ReadXml(reader);
				}
				reader.ReadEndElement();
			}
		}

		protected override void WriteMinimumFeeList(XmlWriter writer)
		{
			foreach (var usageMinimumFee in MinimumFeeList)
			{
				writer.WriteStartElement("MinimumFeeUsage");
				((IXmlSerializable)usageMinimumFee).WriteXml(writer);
				writer.WriteEndElement();
			}
		}
	}
}
