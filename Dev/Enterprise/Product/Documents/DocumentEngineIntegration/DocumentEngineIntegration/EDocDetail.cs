using System;

namespace Enterprise.DocumentEngineIntegration
{
	public class EDocDetail
	{
		public Guid Id { get; set; }
		public DateTime DateAdded { get; set; }
		public DateTime LastEditDate { get; set; }
		public Guid DocumentTypePK { get; set; }
		public string DocumentTypeCode { get; set; }
		public string DocumentTypeDescription { get; set; }
		public string DataType { get; set; }
		public string Description { get; set; }
		public string FileName { get; set; }
		public bool IsSystemGenerated { get; set; }
		public bool IsPublished { get; set; }
		public bool IsDeleted { get; set; }
		public int DatabaseNumber { get; set; }
		public string CreatingUser { get; set; }
		public string LastEditUser { get; set; }
		public string OwnerReadableName { get; set; }
	}
}
