using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5TVMessageData
	{
		ZString NoticeNumber { get; }
		ZString CustomsOfficeAndCustomsDivision { get; }
		ZString CustomsManagerName { get; }
		ZString CustomsPersonName { get; }
		ZString CustomsPersonPhoneNumber { get; }
		ZString ImportDeclarationNumber { get; }
		ZDate ImportDeclarationDate { get; }
		ZInt EntryLineCount { get; }
		ZDecimal TotalCustomsDisbursementDifferenceAmount { get; }
		ZString CorrectionReason { get; }
		ZDate CorrectionDate { get; }
		ZDate NoticeDate { get; }
		ZString NoticeCustomsOffice { get; }
		ZString PayerCompanyName { get; }
		ZString PayerRepresentativeName { get; }
		ZString AttachedDocumentName { get; }
		ZString DeclarantID { get; }
		ZString FormattedNoticeNumber { get; }
	}
	public class GOVCBR5TVMessageData : NonPersistentBusinessObject, IGOVCBR5TVMessageData
	{
		public GOVCBR5TVMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZString NoticeNumber { get; set; }
		public ZString CustomsOfficeAndCustomsDivision { get; set; }
		public ZString CustomsManagerName { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString CustomsPersonPhoneNumber { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZDate ImportDeclarationDate { get; set; }
		public ZInt EntryLineCount { get; set; }
		public ZDecimal TotalCustomsDisbursementDifferenceAmount { get; set; }
		public ZString CorrectionReason { get; set; }
		public ZDate CorrectionDate { get; set; }
		public ZDate NoticeDate { get; set; }
		public ZString NoticeCustomsOffice { get; set; }
		public ZString PayerCompanyName { get; set; }
		public ZString PayerRepresentativeName { get; set; }
		public ZString AttachedDocumentName { get; set; }
		public ZString DeclarantID { get; set; }

		public ZString CustomsOfficeName { get; set; }
		public ZString CustomsDepartmentName { get; set; }
		public ZString FormattedNoticeNumber { get; set; }
		public ZDecimal TotalDutyDifferenceAmount { get; set; }
		public ZDecimal TotalDifferenceVAT { get; set; }
		public ZDecimal TotalIndividualConsumptionDifferenceTax { get; set; }
		public ZDecimal TotalLiquorDifferenceTax { get; set; }
		public ZDecimal TotalTransportationDifferenceTax { get; set; }
		public ZDecimal TotalSpecialAgriculturalDifferenceTax { get; set; }
		public ZDecimal TotalEducationDifferenceTax { get; set; }

		public GOVCBR5TVLineMessageDataCollection Lines => lines ?? (lines = new GOVCBR5TVLineMessageDataCollection(this));
		GOVCBR5TVLineMessageDataCollection lines;
	}
}
