using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	interface IGOVCBR5AJMessageData
	{
		ZString NoticeNumber { get; }
		ZString DeclarantID { get; }
		ZString ImporterID { get; }
		ZString ImporterType { get; }
		ZString DeclarationNumberType { get; }
		ZString DutyTaxFreeType { get; }
		ZString ApplicationNumber { get; }
		ZString CarrierID { get; }
		ZString AgentID { get; }
		ZString PaymentNumber { get; }
		IEnumerable<IGoodsShipment5AJ> GoodsShipments { get; }
	}
	interface IGoodsShipment5AJ
	{
		ZString DeclarationNumber { get; }
		ZDecimal TemporaryOpeningFee { get; set; }
		ZDecimal InspectionFee { get; set; }
		ZDecimal OtherFee { get; set; }
	}

	class GOVCBR5AJMessageData : IGOVCBR5AJMessageData
	{
		public ZString NoticeNumber { get; set; }
		public ZString DeclarantID { get; set; }
		public ZString ImporterID { get; set; }
		public ZString ImporterType { get; set; }
		public ZString DeclarationNumberType { get; set; }
		public ZString DutyTaxFreeType { get; set; }
		public ZString ApplicationNumber { get; set; }
		public ZString CarrierID { get; set; }
		public ZString AgentID { get; set; }
		public ZString PaymentNumber { get; set; }
		public IEnumerable<IGoodsShipment5AJ> GoodsShipments { get; set; }
	}
	class GoodsShipment5AJ : IGoodsShipment5AJ
	{
		public ZString DeclarationNumber { get; set; }
		public ZDecimal TemporaryOpeningFee { get; set; }
		public ZDecimal InspectionFee { get; set; }
		public ZDecimal OtherFee { get; set; }
	}
}
