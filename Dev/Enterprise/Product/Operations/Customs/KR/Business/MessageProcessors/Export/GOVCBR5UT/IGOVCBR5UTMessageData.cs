using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5UTMessageData
	{
		ZString ExportDeclarationNumber { get; }
		ZString ShippingYN { get; }
		ZDate ShippingDate { get; }
	}

	class GOVCBR5UTMessageData : IGOVCBR5UTMessageData
	{
		public ZString ExportDeclarationNumber { get; set; }
		public ZString ShippingYN { get; set; }
		public ZDate ShippingDate { get; set; }
	}
}
