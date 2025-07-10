using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBRRENMessageData
	{
		ZString ExportDeclarationNumber { get; }
		ZDateTime NoticeDateTime { get; }
		ZInt SequenceNo { get; }
		ZString FunctionCode { get; }
		ZString SuspendedType { get; }
		ZString SuspendedCode { get; }
		ZString SuspendedReason { get; }
		ZDateTime StartDateTime { get; }
		ZDateTime EndDateTime { get; }
		ZString CustomsPersonName { get; }
		ZString CustomsPersonPhoneNumber { get; }
	}

	class GOVCBRRENMessageData : IGOVCBRRENMessageData
	{
		public ZString ExportDeclarationNumber { get; set; }
		public ZDateTime NoticeDateTime { get; set; }
		public ZInt SequenceNo { get; set; }
		public ZString FunctionCode { get; set; }
		public ZString SuspendedType { get; set; }
		public ZString SuspendedCode { get; set; }
		public ZString SuspendedReason { get; set; }
		public ZDateTime StartDateTime { get; set; }
		public ZDateTime EndDateTime { get; set; }
		public ZString CustomsPersonName { get; set; }
		public ZString CustomsPersonPhoneNumber { get; set; }
	}
}
