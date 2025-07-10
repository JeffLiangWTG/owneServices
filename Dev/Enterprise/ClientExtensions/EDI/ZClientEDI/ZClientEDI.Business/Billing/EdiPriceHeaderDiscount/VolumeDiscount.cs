using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.Billing.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class VolumeDiscount : NonPersistentBusinessObject, IObsoleteValidation, IXmlSerializable
	{
		public static VolumeDiscount NewFromXml(string xml)
		{
			VolumeDiscount result;
			if (string.IsNullOrEmpty(xml))
			{
				result = new VolumeDiscount();
				result.SetDefaults();
			}
			else
			{
				var serializer = ZXmlSerializer.New(typeof(VolumeDiscount));
				using (var reader = new StringReader(xml))
				{
					result = (VolumeDiscount)serializer.Deserialize(reader);
				}
			}

			result.lines.Sort(AutoVolumeDiscountLine.Schema.UnitCount);
			return result;
		}

		void SetDefaults()
		{
			CreateLine(10000, 5m);
			CreateLine(20000, 10m);
			CreateLine(40000, 15m);
			CreateLine(80000, 20m);
			CreateLine(160000, 24m);
			CreateLine(320000, 28m);
			CreateLine(640000, 32m);
			CreateLine(1280000, 36m);
			CreateLine(2560000, 40m);
			CreateLine(5120000, 44m);
			CreateLine(10240000, 47m);
			CreateLine(20480000, 50m);
		}

		void CreateLine(ZDecimal unitCount, ZDecimal percent)
		{
			var line = Lines.AddNew();
			line.UnitCount = unitCount;
			line.Percent = percent;
		}

		public VolumeDiscountLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new VolumeDiscountLineCollection(this);
					RegisterEditableChildObject(lines);
				}
				return lines;
			}
		}
		VolumeDiscountLineCollection lines;

		#region XML Serialization

		protected void WriteLines(XmlWriter writer)
		{
			foreach (VolumeDiscountLine line in Lines)
			{
				if (!line.IsDeleted)
				{
					writer.WriteStartElement("Line");
					((IXmlSerializable)line).WriteXml(writer);
					writer.WriteEndElement();
				}
			}
		}

		protected void ReadLines(XmlReader reader)
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

		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("Lines");
			WriteLines(writer);
			writer.WriteEndElement();
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			try
			{
				isDeserialising = true;
				reader.ReadStartElement();
				ReadLines(reader);
				reader.ReadEndElement();
			}
			finally
			{
				isDeserialising = false;
			}
		}

		protected bool IsDeserialising
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return isDeserialising;
			}
		}

		bool isDeserialising;

		#endregion
	}

	public class VolumeDiscountLineCollection : NonPersistentBusinessObjectCollection<VolumeDiscountLine>
	{
		public VolumeDiscountLineCollection()
		{
		}

		public VolumeDiscountLineCollection(VolumeDiscount parent)
		{
			Parent = parent;
		}

		readonly VolumeDiscount Parent;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new VolumeDiscountLine(Parent);
		}
	}

	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class VolumeDiscountLine : AutoVolumeDiscountLine
	{
		public VolumeDiscountLine()
		{
		}

		public VolumeDiscountLine(VolumeDiscount parent)
		{
			Parent = parent;
		}

		readonly VolumeDiscount Parent;

		public decimal ScaleFactor => Percent / 100m;

		#region Validation

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

		public override void ValidateUnitCount()
		{
			base.ValidateUnitCount();
			MandatoryValidation.CheckNotZero(UnitCountInfo);
			MandatoryValidation.CheckNotNegative(UnitCountInfo);

			if (Parent != null && Parent.Lines.OfType<VolumeDiscountLine>().Any(x => x.PK != this.PK && x.UnitCount == this.UnitCount))
			{
				UnitCountInfo.AddError("The Unit Count has been duplicated and must be unique.");
			}
		}

		#endregion
	}
}

