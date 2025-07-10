using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	public class BillOfLadingNumberCustomisationCollection : NonPersistentBusinessObjectCollection<BillOfLadingNumberCustomisation>, IXmlSerializable
	{
		public BillOfLadingNumberCustomisationCollection(BillOfLadingNumberCustomisationsByServiceLevel customisations)
			: base(customisations.Factory)
		{
			this.customisations = customisations;
		}

		public BillOfLadingNumberCustomisation this[string serviceLevel]
		{
			get
			{
				foreach (BillOfLadingNumberCustomisation element in this)
				{
					if (element.ServiceLevel == serviceLevel)
					{
						return element;
					}
				}
				return null;
			}
		}

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			BillOfLadingNumberCustomisation customisation = new BillOfLadingNumberCustomisation();
			customisation.Categories = customisations.Categories;
			customisation.AllowNonAlphanumericCharacters = customisations.AllowNonAlphanumericCharacters;
			customisation.EnableMacroInsertion = customisations.EnableMacroInsertion;
			customisation.SetParent(customisations);
			return customisation;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			BillOfLadingNumberCustomisation customisation = (BillOfLadingNumberCustomisation)bizOAdded;
			customisation.SetParent(customisations);
		}

		readonly BillOfLadingNumberCustomisationsByServiceLevel customisations;

		#endregion

		#region IXmlSerializable Members

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}

		public void ReadXml(XmlReader reader)
		{
			//Reader is pointing at <ServiceLevelCustomisations>
			this.RemoveAndDeleteAll();
			if (reader.IsEmptyElement)
			{
				reader.ReadStartElement();
			}
			else
			{
				reader.ReadStartElement();
				while (reader.Name == "CustomisationByServiceLevel")
				{
					BillOfLadingNumberCustomisation customisation = AddNew();
					customisation.Categories = customisations.Categories;
					customisation.AllowNonAlphanumericCharacters = customisations.AllowNonAlphanumericCharacters;
					customisation.EnableMacroInsertion = customisations.EnableMacroInsertion;
					if (customisation != null)
					{
						((IXmlSerializable)customisation).ReadXml(reader);
					}
				}
				reader.ReadEndElement();
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach (BillOfLadingNumberCustomisation customisation in this)
			{
				writer.WriteStartElement("CustomisationByServiceLevel");
				((IXmlSerializable)customisation).WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion
	}
}
