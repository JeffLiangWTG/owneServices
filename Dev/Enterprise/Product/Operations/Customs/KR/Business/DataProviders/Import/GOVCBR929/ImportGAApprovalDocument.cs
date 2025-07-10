using System;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ImportGAApprovalDocument : IImportGAApprovalDocument
	{
		public int SequenceNo { get; set; }
		public string RequirementDocumentType { get; set; }
		public string RequirementApprovalNumber { get; set; }
		public string RegulationCategoryCode { get; set; }
		public string DocumentName { get; set; }
		public DateTime ApprovalDate { get; set; }
		public string UseCode { get; set; }
		public string UniqueItemID { get; set; }

		ZInt IImportGAApprovalDocument.SequenceNo => SequenceNo;
		ZString IImportGAApprovalDocument.RequirementDocumentType => RequirementDocumentType;
		ZString IImportGAApprovalDocument.RequirementApprovalNumber => RequirementApprovalNumber;
		ZString IImportGAApprovalDocument.RegulationCategoryCode => RegulationCategoryCode;
		ZString IImportGAApprovalDocument.DocumentName => DocumentName;
		ZDate IImportGAApprovalDocument.ApprovalDate => (ZDate)ApprovalDate;
		ZString IImportGAApprovalDocument.UseCode => UseCode;
		ZString IImportGAApprovalDocument.UniqueItemID => UniqueItemID;
	}
}
