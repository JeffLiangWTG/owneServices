
namespace Enterprise.Customs.CH.NCTS.Business;

public class CommonPreviousDocumentValidation : EU.NCTS.Business.CommonPreviousDocumentValidation
{
	public CommonPreviousDocumentValidation(CommonPreviousDocument parent) : base(parent)
	{
	}

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();
		if (Parent.Parent is NctsBill bill)
		{
			PassarValidation.CheckNP70254(bill);
		}
	}
}
