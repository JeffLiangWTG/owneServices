using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DocumentPreviewFormLayout : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string IsThumbnailPanelVisible = "IsThumbnailPanelVisible";
			public const string ThumbnailPanelPixelWidth = "ThumbnailPanelPixelWidth";
			public const string ZoomValue = "ZoomValue";
		}

		#endregion

		public DocumentPreviewFormLayout()
		{
		}

		public DocumentPreviewFormLayout(bool isThumbnailPanelVisible, int thumbnailPanelPixelWidth, int zoomValue)
		{
			this.IsThumbnailPanelVisible = isThumbnailPanelVisible;
			this.ThumbnailPanelPixelWidth = thumbnailPanelPixelWidth;
			this.ZoomValue = zoomValue;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentPreviewFormLayout(IsThumbnailPanelVisible, ThumbnailPanelPixelWidth, ZoomValue);
		}

		public bool IsThumbnailPanelVisible;
		public int ThumbnailPanelPixelWidth;
		public int ZoomValue;

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.IsThumbnailPanelVisible, IsThumbnailPanelVisible.ToString());
			writer.WriteElementString(Schema.ThumbnailPanelPixelWidth, ThumbnailPanelPixelWidth.ToString());
			writer.WriteElementString(Schema.ZoomValue, ZoomValue.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsThumbnailPanelVisible = bool.Parse(reader.ReadElementString(Schema.IsThumbnailPanelVisible));
			ThumbnailPanelPixelWidth = int.Parse(reader.ReadElementString(Schema.ThumbnailPanelPixelWidth));
			ZoomValue = int.Parse(reader.ReadElementString(Schema.ZoomValue));
		}

		#endregion
	}
}
