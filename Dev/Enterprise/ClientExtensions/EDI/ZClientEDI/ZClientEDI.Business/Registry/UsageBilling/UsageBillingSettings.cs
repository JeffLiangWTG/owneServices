using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class UsageBillingSettings : AutoUsageBillingSettings
	{
		public UsageBillingSettings()
		{
		}

		public UsageBillingSettings(BusinessObjectFactory factory)
			: base(factory) { }

		public UsageBillingSettings(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public UsageBillingSettings(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new UsageBillingSettings(fallbackLevel, factory);
			foreach (var priceList in (UsageBillingPriceListCollection)this.PriceLists.Clone(fallbackLevel, factory))
			{
				clone.PriceLists.Add(priceList);
			}
			foreach (var branchRestriction in (UsageBillingBranchRestrictionCollection)this.BranchRestrictions.Clone(fallbackLevel, factory))
			{
				clone.BranchRestrictions.Add(branchRestriction);
			}

			return clone;
		}

		public static UsageBillingSettings GetDefaultValue()
		{
			return new UsageBillingSettings();
		}

		public UsageBillingPriceListCollection PriceLists
		{
			get
			{
				if (products == null)
				{
					products = new UsageBillingPriceListCollection();
					RegisterEditableChildObject(products);
				}
				return products;
			}
		}
		UsageBillingPriceListCollection products;

		protected override void ReadPriceLists(XmlReader reader)
		{
			PriceLists.RemoveAll();

			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();

				while (reader.IsStartElement("PriceLists"))
				{
					var product = PriceLists.AddNew();
					((IXmlSerializable)product).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		protected override void WritePriceLists(XmlWriter writer)
		{
			foreach (UsageBillingPriceList product in PriceLists)
			{
				writer.WriteStartElement("PriceLists");
				((IXmlSerializable)product).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		public UsageBillingBranchRestrictionCollection BranchRestrictions
		{
			get
			{
				if (banchRestrictions == null)
				{
					banchRestrictions = new UsageBillingBranchRestrictionCollection();
					RegisterEditableChildObject(banchRestrictions);
				}
				return banchRestrictions;
			}
		}
		UsageBillingBranchRestrictionCollection banchRestrictions;

		protected override void ReadBranchRestrictions(XmlReader reader)
		{
			BranchRestrictions.RemoveAll();

			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();

				while (reader.IsStartElement("BranchRestrictions"))
				{
					var branchRestriction = BranchRestrictions.AddNew();
					((IXmlSerializable)branchRestriction).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		protected override void WriteBranchRestrictions(XmlWriter writer)
		{
			foreach (UsageBillingBranchRestriction branchRestriction in BranchRestrictions)
			{
				writer.WriteStartElement("BranchRestrictions");
				((IXmlSerializable)branchRestriction).WriteXml(writer);
				writer.WriteEndElement();
			}
		}
	}
}
