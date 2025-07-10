using System;
using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public class ExportGAApprovalDocument : IExportGAApprovalDocument
	{
		public string SequenceNo { get; set; }
		public string RequirementApprovalNumber { get; set; }
		public string RequirementDocumentType { get; set; }
		public string DocumentName { get; set; }
		public DateTime ApprovalDate { get; set; }
		public string RegulationCategoryCode { get; set; }
		public string RequirementType { get; set; }
		public string ReasonForMissingApprovalNumber { get; set; }
		public string UniqueItemID { get; set; }
		public string NonGAReasonType { get; set; }

		ZString IExportGAApprovalDocument.SequenceNo => SequenceNo;
		ZString IExportGAApprovalDocument.RequirementApprovalNumber => RequirementApprovalNumber;
		ZString IExportGAApprovalDocument.RequirementDocumentType => RequirementDocumentType;
		ZString IExportGAApprovalDocument.DocumentName => DocumentName;
		ZDate IExportGAApprovalDocument.ApprovalDate => (ZDate)ApprovalDate;
		ZString IExportGAApprovalDocument.RegulationCategoryCode => RegulationCategoryCode;
		ZString IExportGAApprovalDocument.RequirementType => RequirementType;
		ZString IExportGAApprovalDocument.ReasonForMissingApprovalNumber => ReasonForMissingApprovalNumber;
		ZString IExportGAApprovalDocument.UniqueItemID => UniqueItemID;
		ZString IExportGAApprovalDocument.NonGAReasonType => NonGAReasonType;
	}
}
