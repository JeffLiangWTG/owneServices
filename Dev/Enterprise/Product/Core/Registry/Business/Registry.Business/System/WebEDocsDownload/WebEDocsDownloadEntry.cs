using System.Xml.Serialization;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WebEDocsDownloadEntry
	{
		public bool AllowAllDocTypes { get; set; }
		public RefDocTypeEntryCollection DocTypeCollection { get; set; }

		public WebEDocsDownloadEntry()
		{
			DocTypeCollection = new RefDocTypeEntryCollection();
		}

		public WebEDocsDownloadEntry(bool allowAllDocTypes, RefDocTypeEntryCollection docTypeCollection)
		{
			AllowAllDocTypes = allowAllDocTypes;

			DocTypeCollection = new RefDocTypeEntryCollection();
			DocTypeCollection.AddRange(docTypeCollection);
		}

		public void UpdateValues(bool allowAllDocTypes, RefDocTypeEntryCollection docTypeCollection)
		{
			AllowAllDocTypes = allowAllDocTypes;

			DocTypeCollection.RemoveAll();
			DocTypeCollection.AddRange(docTypeCollection.Clone(docTypeCollection.CurrentFallbackLevel, docTypeCollection.Factory));
		}

		public override bool Equals(object obj)
		{
			var webEDocs = obj as WebEDocsDownloadEntry;

			if (webEDocs == null)
			{
				return false;
			}

			if (webEDocs.AllowAllDocTypes != AllowAllDocTypes)
			{
				return false;
			}

			if (!webEDocs.DocTypeCollection.Equals(DocTypeCollection))
			{
				return false;
			}

			return true;
		}

		public override int GetHashCode()
		{
			return AllowAllDocTypes.GetHashCode() ^ DocTypeCollection.GetHashCode();
		}

		public WebEDocsDownloadEntry Clone()
		{
			return new WebEDocsDownloadEntry(AllowAllDocTypes, DocTypeCollection);
		}
	}
}
