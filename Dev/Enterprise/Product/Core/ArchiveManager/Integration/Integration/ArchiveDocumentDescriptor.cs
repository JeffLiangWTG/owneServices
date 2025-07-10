using CargoWise.Common;
namespace Enterprise.ArchiveManager.Integration
{
	public class ArchiveDocumentDescriptor
	{
		public ArchiveDocumentDescriptor(string menuName)
		{
			_ = Argument.NotNullOrEmpty(menuName, "menuName");
			MenuName = menuName;
		}

		public string MenuName { get; private set; }

		/// <summary>
		/// Set this DocType and archive process will check for the presence of this doc type in the existing
		/// list of documents, and if found, then this document will not be run.
		/// </summary>
		public string DocTypeToCheck { get; set; }
	}
}
