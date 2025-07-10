using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	public class CountryDiscount : AutoCountryDiscount
	{
		public static CountryDiscount NewFromXml(string xml)
		{
			CountryDiscount result;
			if (string.IsNullOrEmpty(xml))
			{
				result = new CountryDiscount();
				result.SetDefaults();
			}
			else
			{
				var serializer = ZXmlSerializer.New(typeof(CountryDiscount));
				using (var reader = new StringReader(xml))
				{
					result = (CountryDiscount)serializer.Deserialize(reader);
				}
			}
			return result;
		}

		void SetDefaults()
		{
			var line1 = Lines.AddNew();
			line1.Country = "CN";
			line1.Percent = 40;
			var line2 = Lines.AddNew();
			line2.Country = "IN";
			line2.Percent = 20;
			var line3 = Lines.AddNew();
			line3.Country = "MX";
			line3.Percent = 20;
		}

		public CountryDiscountLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new CountryDiscountLineCollection(this);
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		CountryDiscountLineCollection lines;

		#region Validation

		internal bool HasUniqueCountry(CountryDiscountLine countryDiscountLine)
		{
			return !Lines.Cast<CountryDiscountLine>().Any(x => x != countryDiscountLine && x.Country == countryDiscountLine.Country);
		}

		protected override void RunPreSaveValidationCore()
		{
			var codes = new HashSet<string>();

			foreach (CountryDiscountLine line in Lines)
			{
				if (!line.IsDeleted && !line.CountryInfo.HasErrors())
				{
					if (codes.Contains(line.Country))
					{
					}
					else
					{
						codes.Add(line.Country);
					}
				}
			}

			base.RunPreSaveValidationCore();
		}
		#endregion

		#region XML Serialization

		protected override void WriteLines(XmlWriter writer)
		{
			foreach (CountryDiscountLine line in Lines)
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

	public class CountryDiscountLineCollection : NonPersistentBusinessObjectCollection<CountryDiscountLine>
	{
		public CountryDiscountLineCollection()
		{
		}

		public CountryDiscountLineCollection(CountryDiscount parent)
		{
			Parent = parent;
		}

		readonly CountryDiscount Parent;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CountryDiscountLine(Parent);
		}
	}

	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class CountryDiscountLine : AutoCountryDiscountLine
	{
		public CountryDiscountLine()
		{
		}

		public CountryDiscountLine(CountryDiscount parent)
		{
			Parent = parent;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RequiresDomesticDiscount = true;
		}

		readonly CountryDiscount Parent;

		[List("Countries")]
		public override ZString Country
		{
			get
			{
				return base.Country;
			}

			set
			{
				base.Country = value;
			}
		}

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

			if (!CountryInfo.HasErrors())
			{
				if (Parent != null && !Parent.HasUniqueCountry(this))
				{
					CountryInfo.AddError("Country already used");
				}
			}
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

