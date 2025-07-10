using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Billing.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class DomesticDiscount : AutoDomesticDiscount
	{
		public static DomesticDiscount NewFromXml(string xml)
		{
			DomesticDiscount result;
			if (string.IsNullOrEmpty(xml))
			{
				result = new DomesticDiscount();
				result.SetDefaults();
			}
			else
			{
				var serializer = ZXmlSerializer.New(typeof(DomesticDiscount));
				using (var reader = new StringReader(xml))
				{
					result = (DomesticDiscount)serializer.Deserialize(reader);
				}
			}
			return result;
		}

		void SetDefaults()
		{
			RequiresDevelopingCountry = true;
			MaxForeignCompanyCount = 5;
			MaxForeignUserCount = 50;
			MaxForeignUserPercent = 20;
			MultiEntityPercent = 10;
			ExpiryMonthCount = 3;
			var line1 = Lines.AddNew();
			line1.Percent = 30;
			line1.UserCount = 0;
			var line2 = Lines.AddNew();
			line2.Percent = 20;
			line2.UserCount = 20;
		}

		public DomesticDiscountLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new DomesticDiscountLineCollection(this);
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		DomesticDiscountLineCollection lines;

		public DomesticDiscountLine FindMatchingBreak(int userCount)
		{
			DomesticDiscountLine match = null;
			foreach (DomesticDiscountLine line in Lines)
			{
				if (userCount >= line.UserCount
					&& (match == null || match.UserCount < line.UserCount))
				{
					match = line;
				}
			}
			return match;
		}

		public decimal CalculatePercent(DatabaseCountryUsers countryUsers)
		{
			decimal percent = 0;
			var userGroup = countryUsers.DevelopingRegionUserGroup;
			if (userGroup.DomesticCompanyCount == 1 && userGroup.ForeignCompanyCount == 0)
			{
				var line = FindMatchingBreak(userGroup.TotalUserCount);
				if (line != null)
				{
					percent = line.Percent;
				}
			}
			else if (userGroup.ForeignCompanyCount <= MaxForeignCompanyCount
					&& userGroup.ForeignUserCount <= MaxForeignUserCount
					&& userGroup.ForeignUserPercent <= MaxForeignUserPercent)
			{
				percent = MultiEntityPercent;
			}

			return percent;
		}

		#region XML Serialization

		protected override void WriteLines(XmlWriter writer)
		{
			foreach (DomesticDiscountLine line in Lines)
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

		#region Validation

		internal bool HasUniquePercent(DomesticDiscountLine domesticDiscountLine)
		{
			return !Lines.Cast<DomesticDiscountLine>().Any(x => x != domesticDiscountLine && x.Percent == domesticDiscountLine.Percent);
		}

		internal bool HasUniqueUserCount(DomesticDiscountLine domesticDiscountLine)
		{
			return !Lines.Cast<DomesticDiscountLine>().Any(x => x != domesticDiscountLine && x.UserCount == domesticDiscountLine.UserCount);
		}

		#endregion
	}

	public class DomesticDiscountLineCollection : NonPersistentBusinessObjectCollection<DomesticDiscountLine>
	{
		public DomesticDiscountLineCollection()
		{
		}

		public DomesticDiscountLineCollection(DomesticDiscount parent)
		{
			Parent = parent;
		}

		readonly DomesticDiscount Parent;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DomesticDiscountLine(Parent);
		}
	}

	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class DomesticDiscountLine : AutoDomesticDiscountLine
	{
		public DomesticDiscountLine(DomesticDiscount parent = null)
		{
			Parent = parent;
		}

		readonly DomesticDiscount Parent;

		#region Validation

		public override void ValidateUserCount()
		{
			base.ValidateUserCount();
			if (!UserCountInfo.HasErrors())
			{
				if (Parent != null && !Parent.HasUniqueUserCount(this))
				{
					UserCountInfo.AddError("User Count is already used");
				}
			}
		}

		public override void ValidatePercent()
		{
			base.ValidatePercent();
			if (!PercentInfo.HasErrors())
			{
				if (Percent > 100m)
				{
					PercentInfo.AddError("Percent must be less than or equal to 100");
				}
				else if (Parent != null && !Parent.HasUniquePercent(this))
				{
					PercentInfo.AddError("Percentage is already used");
				}
			}
		}

		#endregion
	}
}

