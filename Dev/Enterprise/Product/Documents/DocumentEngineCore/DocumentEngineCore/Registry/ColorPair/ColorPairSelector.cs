using System.Drawing;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class ColorPairSelector : RegistryBusinessObjectTemplate
	{
		public ColorPairSelector()
		{
			PrimaryColor = Color.White;
			SecondaryColor = Color.Black;
		}

		public Color PrimaryColor
		{
			get;
			set;
		}

		public Color SecondaryColor
		{
			get;
			set;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ColorPairSelector { PrimaryColor = PrimaryColor, SecondaryColor = SecondaryColor };
		}

		public override bool Equals(object obj)
		{
			var other = obj as ColorPairSelector;
			return other != null && PrimaryColor == other.PrimaryColor && SecondaryColor == other.SecondaryColor;
		}

		public override int GetHashCode()
		{
			return PrimaryColor.GetHashCode() ^ SecondaryColor.GetHashCode();
		}

		#region Xml Serialisation
		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			writer.WriteStartElement("PrimaryColor");
			writer.WriteValue(PrimaryColor.ToArgb());
			writer.WriteEndElement();

			writer.WriteStartElement("SecondaryColor");
			writer.WriteValue(SecondaryColor.ToArgb());
			writer.WriteEndElement();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			int colorInt;
			if (int.TryParse(reader.ReadElementString("PrimaryColor"), out colorInt))
			{
				PrimaryColor = Color.FromArgb(colorInt);
			}
			if (int.TryParse(reader.ReadElementString("SecondaryColor"), out colorInt))
			{
				SecondaryColor = Color.FromArgb(colorInt);
			}
		}
		#endregion
	}
}
