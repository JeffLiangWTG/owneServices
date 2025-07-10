using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public class InlandTransportValidation : CusCodeDataValidation
{
	public InlandTransportValidation(InlandTransport parent) : base(parent)
	{
	}

	protected override void CheckCY_Order()
	{
		base.CheckCY_Order();

		MandatoryValidation.CheckNotNegative(Parent.CY_OrderInfo);
	}
}
