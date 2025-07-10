using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class GuaranteeResponseData
	{
		public ZString GuaranteeType { get; set; }
		public ZString GuaranteeReferenceNumber { get; set; }
		public ZString OtherGuaranteeReference { get; set; }
		public ZString AccessCode { get; set; }
		public ZBool ValidityLimitationEC { get; set; }
		public ZString ValidityLimitationOther { get; set; }
	}
}
