using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR43MessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZInt AmendSequence { get; }
		ZDate DeclarationDate { get; }
		ZDateTime NoticeDateTime { get; }
		ZDate AfterReExportScheduledDate { get; }
		ZString ResultType { get; }
		ZString ResultReason { get; }
		ZString CustomsManagerName { get; }
		ZString CustomsOfficeAndDivision { get; }
		ZString CustomsOfficeContent { get; }
		IEnumerable<ZString> DutyFulfillment { get; }
		IEnumerable<GOVCBRR43LineMessageData> InvoiceLines { get; }
	}

	public class GOVCBRR43MessageData : IGOVCBRR43MessageData
	{
		public ZString ImportDeclarationNumber { get; set; }
		public ZInt AmendSequence { get; set; }
		public ZDate DeclarationDate { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZDate AfterReExportScheduledDate { get; set; }
		public ZString ResultType { get; set; }
		public ZString ResultReason { get; set; }
		public ZString CustomsManagerName { get; set; }
		public ZString CustomsOfficeAndDivision { get; set; }
		public ZString CustomsOfficeContent { get; set; }
		public IEnumerable<ZString> DutyFulfillment { get; set; }
		public IEnumerable<GOVCBRR43LineMessageData> InvoiceLines { get; set; }
	}
}
