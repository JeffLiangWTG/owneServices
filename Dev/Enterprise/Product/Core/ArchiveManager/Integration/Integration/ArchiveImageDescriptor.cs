using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration
{
	public class ArchiveImageDescriptor
	{
		public ArchiveImageDescriptor(ZBlob archiveImage, ZString filename, ZString documentType)
		{
			ArchiveImage = archiveImage;
			Filename = filename;
			DocumentType = documentType;
		}

		public ZBlob ArchiveImage { get; private set; }
		public ZString Filename { get; private set; }
		public ZString DocumentType { get; private set; }
	}
}
