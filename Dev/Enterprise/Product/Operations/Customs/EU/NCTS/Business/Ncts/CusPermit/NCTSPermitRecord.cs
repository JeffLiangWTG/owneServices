using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business;

public class NCTSPermitRecord
{
	public EU.Business.Declaration.MultiLineAddInfos.SupportingDocument SupportingDocument { get; set; }
	public ZDecimal CustomsValue { get; set; }
	public ZDecimal Quantity { get; set; }
	public ZString QuantityUnit { get; set; }
	public ZString PermitType { get; set; }
}
