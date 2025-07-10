using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR106MessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZString ResultType { get; }
		ZString CustomsDepartment { get; }
		ZString CustomsPersonName { get; }
		ZString CustomsPersonPhoneNumber { get; }
		ZDate ApprovalDate { get; }
		ZDate DeclarationDate { get; }
		ZString ContentDescription { get; }
	}

	class GOVCBR106MessageData : IGOVCBR106MessageData
	{
		public ZString ImportDeclarationNumber { get; set; }
		public ZString ResultType { get; set; }
		public ZString CustomsDepartment { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString CustomsPersonPhoneNumber { get; set; }
		public ZDate ApprovalDate { get; set; }
		public ZDate DeclarationDate { get; set; }
		public ZString ContentDescription { get; set; }
	}
}
