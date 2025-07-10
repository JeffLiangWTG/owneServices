using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5SNMessageData
	{
		ZString ValueDeclarationTemplateNumber { get; }
		ZDate ApprovalDate { get; }
		ZDate EffectiveToDate { get; }
		ZString ResultType { get; }
		ZString DeclarationOffice { get; }
		ZString CustomsPersonName { get; }
		ZString ResultReason { get; }
		ZString IdentificationNumber { get; }
	}

	class GOVCBR5SNMessageData : IGOVCBR5SNMessageData
	{
		public ZString ValueDeclarationTemplateNumber { get; set; }
		public ZDate ApprovalDate { get; set; }
		public ZDate EffectiveToDate { get; set; }
		public ZString ResultType { get; set; }
		public ZString DeclarationOffice { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString ResultReason { get; set; }
		public ZString IdentificationNumber { get; set; }
	}
}
