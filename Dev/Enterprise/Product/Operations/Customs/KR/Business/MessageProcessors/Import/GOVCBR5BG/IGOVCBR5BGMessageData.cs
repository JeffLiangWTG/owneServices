using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5BGMessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZDateTime ApprovalDateTime { get; }
		ZString ResultType { get; }
		ZString FaultParty { get; }
		ZString ReasonCode { get; }
		ZString DeclarationOffice { get; }
		ZString CustomsPersonName { get; }
		ZString OtherReason { get; }
		ZString DismissalReason { get; }
	}

	class GOVCBR5BGMessageData : IGOVCBR5BGMessageData
	{
		public ZString ImportDeclarationNumber { get; set; }
		public ZDateTime ApprovalDateTime { get; set; }
		public ZString ResultType { get; set; }
		public ZString FaultParty { get; set; }
		public ZString ReasonCode { get; set; }
		public ZString DeclarationOffice { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString OtherReason { get; set; }
		public ZString DismissalReason { get; set; }
	}
}
