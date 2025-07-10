using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR023MessageData
	{
		ZDateTime IssueDateTime { get; }
		ZDateTime NoticeDateTime { get; }
		ZString ImportDeclarationNumber { get; }
		ZString NoticeNumber { get; }
		ZString CustomsManagerID { get; }
		ZString CustomsManagerName { get; }
		ZString ResultType { get; }
		ZString DocumentSubmitType { get; }
		ZDate PaymentDate { get; }
		ZString CustomsOfficeContent { get; }
		IReadOnlyDictionary<string, List<int>> CSCodes { get; }
	}

	class GOVCBR023MessageData : IGOVCBR023MessageData
	{
		public ZDateTime IssueDateTime { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZString ImportDeclarationNumber { get; set; }
		public ZString NoticeNumber { get; set; }
		public ZString CustomsManagerID { get; set; }
		public ZString CustomsManagerName { get; set; }
		public ZString ResultType { get; set; }
		public ZString DocumentSubmitType { get; set; }
		public ZDate PaymentDate { get; set; }
		public ZString CustomsOfficeContent { get; set; }
		public IReadOnlyDictionary<string, List<int>> CSCodes { get; set; }
	}
}
