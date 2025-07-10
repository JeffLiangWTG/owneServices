using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WebCustomThemeImageBusinessObject : RegistryBusinessObjectTemplate
	{
		public WebCustomThemeImageBusinessObject()
		{
		}

		public WebCustomThemeImageBusinessObject(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string ImageName = "ImageName";
			public const string ImageData = "ImageData";
		}

		#endregion

		[MaxLength(255)]
		public ZString ImageName
		{
			get { return imageName; }
			set
			{
				if (imageName != value)
				{
					CheckMaximumLength(ImageNameInfo, value);
					imageName = value;
					ImageNameInfo.RefreshBinding();
				}
			}
		}
		ZString imageName;

		public ZPropertyInfo ImageNameInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ImageName));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public byte[] Data
		{
			get
			{
				if (data == null)
				{
					return System.Array.Empty<byte>();
				}
				return data;
			}
			set { data = value; }
		}
		byte[] data;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WebCustomThemeImageBusinessObject();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var cloneObject = (WebCustomThemeImageBusinessObject)clone;
			cloneObject.ImageName = ImageName;
			cloneObject.Data = Data;
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ImageName = reader.ReadElementString(Schema.ImageName);
			Data = DeserialiseImage(reader.Reader, Schema.ImageData);
		}

		static byte[] DeserialiseImage(XmlReader reader, string elementName)
		{
			byte[] result = null;

			reader.ReadStartElement(elementName);

			var stream = new MemoryStream();
			int bufferSize = 4096;
			var data = new byte[bufferSize];
			int bytesRead;
			while ((bytesRead = reader.ReadContentAsBase64(data, 0, bufferSize)) > 0)
			{
				stream.Write(data, 0, bytesRead);
			}
			if (stream != null)
			{
				result = stream.ToArray();

				stream.Flush();
				stream.Position = 0;
			}

			reader.ReadEndElement();

			return result;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ImageName, ImageName);
			writer.WriteStartElement(Schema.ImageData);
			writer.WriteBase64(Data, 0, Data.Length);
			writer.WriteEndElement();
		}
	}
}
