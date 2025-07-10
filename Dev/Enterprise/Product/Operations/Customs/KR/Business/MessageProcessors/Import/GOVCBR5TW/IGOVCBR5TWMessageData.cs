using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5TWMessageData
	{
		ZString NoticeNumber { get; }
		ZString FormattedNoticeNumber { get; }
		ZString RequestDocumentNumber { get; }
		ZString PayerCompanyName { get; }
		ZString PayerRepresentativeName { get; }
		ZString ImportDeclarationNumber { get; }
		ZString FormattedImportDeclarationNumber { get; }
		ZInt ImportEntryLineNo { get; }
		ZDate ImportDeclarationDate { get; }
		ZDate ExamineStartDate { get; }
		ZDate ExamineEndDate { get; }
		ZString ContentDescription { get; }
		ZString CorrectionResult { get; }
		ZDate NoticeDate { get; }
		ZString NoticeCustomsOffice { get; }
		ZString NoticeCustomsOfficeName { get; }
		ZInt DeclarationsCount { get; }
		ZString DeclarantID { get; }
	}

	public class GOVCBR5TWMessageData : NonPersistentBusinessObject, IGOVCBR5TWMessageData
	{
		public GOVCBR5TWMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZString NoticeNumber { get; set; }
		public ZString FormattedNoticeNumber { get; set; }
		public ZString RequestDocumentNumber { get; set; }
		public ZString PayerCompanyName { get; set; }
		public ZString PayerRepresentativeName { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZString FormattedImportDeclarationNumber { get; set; }
		public ZInt ImportEntryLineNo { get; set; }
		public ZDate ImportDeclarationDate { get; set; }
		public ZDate ExamineStartDate { get; set; }
		public ZDate ExamineEndDate { get; set; }
		public ZString ContentDescription { get; set; }
		public ZString CorrectionResult { get; set; }
		public ZDate NoticeDate { get; set; }
		public ZString NoticeCustomsOffice { get; set; }
		public ZString NoticeCustomsOfficeName { get; set; }
		public ZInt DeclarationsCount { get; set; }
		public ZString DeclarantID { get; set; }

		public ZString ContentDescriptionShort { get => ContentDescription.SubstringSafe(0, 100); }
		public ZString CorrectionResultShort { get => CorrectionResult.SubstringSafe(0, 100); }

		public GOVCBR5TWLineMessageDataCollection Lines => lines ?? (lines = new GOVCBR5TWLineMessageDataCollection(Factory));
		GOVCBR5TWLineMessageDataCollection lines;
	}
}
