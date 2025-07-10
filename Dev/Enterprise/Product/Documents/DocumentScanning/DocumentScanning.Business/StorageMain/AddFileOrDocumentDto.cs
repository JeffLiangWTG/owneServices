using CargoWise.Types;

namespace Enterprise.DocumentScanning.Business
{
	public class AddFileOrDocumentDto
	{
		public ZString FileName { get; set; }
		public ZString DocumentType { get; set; }
		public FileAction FileAction { get; set; } = FileAction.CreateNew;
		public ZGuid VisibleCompanyPK { get; set; }
		public ZGuid VisibleBranchPK { get; set; }
		public ZGuid VisibleDepartmentPK { get; set; }
		public ZString Description { get; set; }
		public ZString Source { get; set; }
		public bool IsArchiving { get; set; }
		public bool ShouldSupersedeOlderVersion { get; set; } = true;
	}
}
