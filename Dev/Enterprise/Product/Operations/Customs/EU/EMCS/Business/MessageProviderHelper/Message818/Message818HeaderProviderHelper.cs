using System;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class Message818HeaderProviderHelper : HeaderProviderHelper
	{
		public Message818HeaderProviderHelper(EMCSJobDeclaration emcsJobDeclaration, IReportOfReceipt reportOfReceipt) : base(emcsJobDeclaration)
		{
			this.reportOfReceipt = reportOfReceipt;
		}
		readonly IReportOfReceipt reportOfReceipt;

		public string DestinationOfficeReferenceNumber => emcsJobDeclaration.GetOfficeReferenceNumber(OfficeCodes_EMCS.Codes.CompetentAuthorityOfArrival);

		public DateTime? DateOfArrivalOfExciseProducts => reportOfReceipt.ArrivalDate.ToNullableDateTime();

		public string GlobalConclusionOfReceipt => reportOfReceipt.ReceiptResult;

		public bool IsReceiptPartiallyRefused => reportOfReceipt.ReceiptResult == EMCSReceiptResultList.Codes.ReceiptPartiallyRefused;
	}
}
