using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Billing.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class MasterOrgDevelopingCountryDiscount : AutoMasterOrgDevelopingCountryDiscount
	{
		public static MasterOrgDevelopingCountryDiscount NewFromXml(string xml)
		{
			MasterOrgDevelopingCountryDiscount result;
			if (string.IsNullOrEmpty(xml))
			{
				result = new MasterOrgDevelopingCountryDiscount();
			}
			else
			{
				var serializer = ZXmlSerializer.New(typeof(MasterOrgDevelopingCountryDiscount));
				using (var reader = new StringReader(xml))
				{
					result = (MasterOrgDevelopingCountryDiscount)serializer.Deserialize(reader);
				}
			}
			return result;
		}

		public MasterOrgDevelopingCountryDiscountLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new MasterOrgDevelopingCountryDiscountLineCollection();
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		MasterOrgDevelopingCountryDiscountLineCollection lines;

		#region XML Serialization

		protected override void WriteLines(XmlWriter writer)
		{
			foreach (MasterOrgDevelopingCountryDiscountLine line in Lines)
			{
				if (!line.IsDeleted)
				{
					writer.WriteStartElement("Line");
					((IXmlSerializable)line).WriteXml(writer);
					writer.WriteEndElement();
				}
			}
		}

		protected override void ReadLines(XmlReader reader)
		{
			Lines.RemoveAll();

			if (reader.IsEmptyElement)
			{
				reader.Skip();
			}
			else
			{
				reader.ReadStartElement();

				while (reader.IsStartElement("Line"))
				{
					var line = Lines.AddNew();
					((IXmlSerializable)line).ReadXml(reader);
				}

				reader.ReadEndElement();
			}
		}

		#endregion
	}

	public class MasterOrgDevelopingCountryDiscountLineCollection : NonPersistentBusinessObjectCollection<MasterOrgDevelopingCountryDiscountLine>
	{
		public MasterOrgDevelopingCountryDiscountLineCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new MasterOrgDevelopingCountryDiscountLine();
		}
	}

	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class MasterOrgDevelopingCountryDiscountLine : AutoMasterOrgDevelopingCountryDiscountLine
	{
		public MasterOrgDevelopingCountryDiscountLine()
		{
		}

		[List("Countries")]
		public override ZString Country { get => base.Country; set => base.Country = value; }

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(new BusinessObjectFactory() { RefreshEnabled = false }); }
		}

		#region Validation

		public override void ValidateCountry()
		{
			base.ValidateCountry();
			MandatoryValidation.CheckEntered(CountryInfo);
			ListValidation.ErrorIfInvalidCode(CountryInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CountryInfo);
		}

		public override void ValidatePercent()
		{
			base.ValidatePercent();
			MandatoryValidation.CheckNotZero(PercentInfo);
			if (!PercentInfo.HasErrors())
			{
				if (Percent > 100m)
				{
					PercentInfo.AddError("Percent must be less than or equal to 100");
				}
			}
		}

		#endregion
	}
}

