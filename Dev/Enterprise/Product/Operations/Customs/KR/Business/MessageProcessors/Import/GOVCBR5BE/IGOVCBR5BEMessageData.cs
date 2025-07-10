using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5BEMessageData
	{
		ZString ImportDeclarationNumber { get; }
		ZDateTime NoticeDateTime { get; }
		ZDate PaymentDate { get; }
		ZString ResultType { get; }
		ZString ResultCode { get; }
		ZString ResultReason { get; }
		ZString ApprovalCode { get; }
		ZString PaymentType { get; }
		ZString DeclarationProcedureType { get; }
		ZString CustomsManagerName { get; }
		ZString DeclarationOffice { get; }
	}

	public class GOVCBR5BEMessageData : NonPersistentBusinessObject, IGOVCBR5BEMessageData
	{
		public GOVCBR5BEMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public ZString ImportDeclarationNumber { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZDate PaymentDate { get; set; }
		public ZString ResultType { get; set; }
		public ZString ResultCode { get; set; }
		public ZString ResultReason { get; set; }
		public ZString ApprovalCode { get; set; }
		public ZString PaymentType { get; set; }
		public ZString DeclarationProcedureType { get; set; }
		public ZString CustomsManagerName { get; set; }
		public ZString DeclarationOffice { get; set; }
	}
}
