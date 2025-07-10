using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRR21MessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZDate DeclarationDate { get; }
		ZString HouseBillNumber { get; }
		ZString RejectionCode { get; }
		ZDate RejectionDate { get; }
		ZString CustomsOffice { get; }
		ZString CustomsPersonName { get; }
	}

	class GOVCBRR21MessageData : IGOVCBRR21MessageData
	{
		public ZString ImportDeclarationNumber { get; set; }
		public ZDate DeclarationDate { get; set; }
		public ZString HouseBillNumber { get; set; }
		public ZString RejectionCode { get; set; }
		public ZDate RejectionDate { get; set; }
		public ZString CustomsOffice { get; set; }
		public ZString CustomsPersonName { get; set; }
	}
}
